using System;
using Belphegor.Composites;
using Belphegor.Dynamics;
using Belphegor.Helpers;
using Belphegor.Settings;
using Belphegor.Utilities;
using log4net;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Belphegor.Routines
{
    public class Monk
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static readonly ContextChangeHandler CtxChanger =
            ctx =>
                new CombatContext(
                    context => CombatTargeting.Instance.FirstNpc);

        [Class(ActorClass.Monk)]
        [Behavior(BehaviorType.Buff)]
        public static Composite MonkBuff()
        {
            return
                new Decorator(ctx => !ZetaDia.IsInTown,
                    new PrioritySelector(CtxChanger,
                        Common.CreateWaitForAttack(),
                        Common.CreateWaitForCast(),
                        Common.CreateGetPowerGlobe(),
                        Common.CreateUsePoolOfReflection(),
                        new SelfCast(SNOPower.Monk_BreathOfHeaven,
                            ret =>
                                ZetaDia.Me.CurrentPrimaryResource >
                                BelphegorSettings.Instance.Monk.
                                    BoHBlazingWrathOutOfCombatSpiritTreshold &&
                                !ZetaDia.Me.HasBuff(SNOPower.Monk_BreathOfHeaven) &&
                                BelphegorSettings.Instance.Monk.BoHBlazingWrath),
                        Avoidance.CreateMoveForAvoidance(BelphegorSettings.Instance.Monk.MaximumRange)
                        )
                    );
        }

        [Class(ActorClass.Monk)]
        [Behavior(BehaviorType.Combat)]
        public static Composite MonkCombat()
        {
            return
                new PrioritySelector(CtxChanger,
                    new SelfCast(SNOPower.Monk_Serenity,
                        extra =>
                            ZetaDia.Me.HitpointsCurrentPct <=
                            BelphegorSettings.Instance.Monk.SerenityHp ||
                            ((CombatContext) extra).IsPlayerIncapacited),
                    Common.CreateUsePotion(),
                    Common.CreateWaitWhileFearedStunnedFrozenOrBlind(),
                    Common.CreateGetHealthGlobe(),
                    Common.CreateUseHealthWell(),
                    Common.CreateWaitForAttack(),
                    //Heals
                    new SelfCast(SNOPower.Monk_BreathOfHeaven,
                        extra =>
                            ((CombatContext) extra).CurrentHealthPercentage <=
                            BelphegorSettings.Instance.Monk.BreathOfHeavenHp
                            ||
                            (!ZetaDia.Me.HasBuff(SNOPower.Monk_BreathOfHeaven) &&
                             BelphegorSettings.Instance.Monk.BoHBlazingWrath)),
                    new SelfCast(SNOPower.X1_Monk_InnerSanctuary,
                        extra => ZetaDia.Me.HitpointsCurrentPct <= 0.4),
                    new Decorator(
                        ctx =>
                            ctx is CombatContext &&
                            ((CombatContext) ctx).CurrentTarget != null,
                        new PrioritySelector(
                            //Mantra
                            new Decorator(
                                ret =>
                                    ZetaDia.Me.CurrentPrimaryResource >=
                                    BelphegorSettings.Instance.Monk.MantraSpirit &&
                                    (!BelphegorSettings.Instance.Monk.WaitForSweepingWind ||
                                     ZetaDia.Me.HasBuff(SNOPower.Monk_SweepingWind)),
                                new PrioritySelector(
                                    new SelfCast(SNOPower.X1_Monk_MantraOfEvasion_v2,
                                        extra =>
                                            !ZetaDia.Me.HasBuff(SNOPower.X1_Monk_MantraOfEvasion_v2)),
                                    new SelfCast(SNOPower.X1_Monk_MantraOfConviction_v2,
                                        extra =>
                                            !ZetaDia.Me.HasBuff(SNOPower.X1_Monk_MantraOfConviction_v2)),
                                    new SelfCast(SNOPower.X1_Monk_MantraOfHealing_v2,
                                        extra =>
                                            !ZetaDia.Me.HasBuff(SNOPower.X1_Monk_MantraOfHealing_v2)),
                                    new SelfCast(SNOPower.X1_Monk_MantraOfRetribution_v2,
                                        extra =>
                                            !ZetaDia.Me.HasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2))
                                    )),
                            // Pull phase.
                            new Decorator(
                                ctx =>
                                    ((CombatContext) ctx).TargetDistance >
                                    BelphegorSettings.Instance.Monk.MaximumRange,
                                new PrioritySelector(
                                    new CastAtLocation(SNOPower.X1_Monk_DashingStrike,
                                        ctx => ((CombatContext) ctx).TargetPosition),
                                    new CastOnUnit(SNOPower.Monk_FistsofThunder,
                                        ctx => ((CombatContext) ctx).TargetGuid)
                                    //CommonBehaviors.MoveTo(ctx => ((DiaUnit)ctx).Position, "Moving towards unit")
                                    )),
                            //Buffs
                            new SelfCast(SNOPower.Monk_SweepingWind,
                                extra => !ZetaDia.Me.HasBuff(SNOPower.Monk_SweepingWind)),
                            //Mystic Ally 

                            //Focus Skills
                            new SelfCast(SNOPower.Monk_CycloneStrike,
                                ctx =>
                                    Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                                    Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                        20f) >=
                                    BelphegorSettings.Instance.Monk.CycloneStrikeAoECount),
                            new SelfCast(SNOPower.Monk_SevenSidedStrike,
                                ctx =>
                                    Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                                    Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                        20f) >=
                                    BelphegorSettings.Instance.Monk.SevenSidedStrikeAoECount),
                            //Secondary
                            new CastOnUnit(SNOPower.Monk_ExplodingPalm,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                extra => ExplodingPalm.IsFinished,
                                s => ExplodingPalm.Reset()),
                            new CastOnUnit(SNOPower.Monk_LashingTailKick,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                ctx =>
                                    (Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                        16f) >=
                                     BelphegorSettings.Instance.Monk.LashingTailKickAoECount ||
                                     Unit.IsEliteInRange(16f, ((CombatContext) ctx))) &&
                                    ZetaDia.Me.CurrentPrimaryResource >
                                    BelphegorSettings.Instance.Monk.
                                        LashingTailKickSpiritTreshold),
                            new SelfCast(SNOPower.Monk_BlindingFlash,
                                ctx =>
                                    Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                        18f) >= 5 ||
                                    Unit.IsEliteInRange(18f, ((CombatContext) ctx))),
                            new CastOnUnit(SNOPower.Monk_WaveOfLight,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                ctx => ((CombatContext) ctx).TargetDistance <= 16f),
                            new CastOnUnit(SNOPower.Monk_TempestRush,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                ctx => ZetaDia.Me.CurrentPrimaryResource > 15),
                            // Primary Skills. 
                            new CastOnUnit(SNOPower.Monk_DeadlyReach,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger),
                            new CastOnUnit(SNOPower.Monk_CripplingWave,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger),
                            new CastOnUnit(SNOPower.Monk_WayOfTheHundredFists,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger),
                            new CastOnUnit(SNOPower.Monk_FistsofThunder,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger)
                            )
                        ),
                    new Action(ret => RunStatus.Success)
                    );
        }

        [Class(ActorClass.Monk)]
        [Behavior(BehaviorType.Movement)]
        public static Composite MonkMovement()
        {
            return new PrioritySelector(
                Common.CreateWaitForAttack(),
                Common.CreateWaitForCast(),
                new CastAtLocation(SNOPower.Monk_TempestRush, ctx => (Vector3) ctx,
                    ctx => ZetaDia.Me.CurrentPrimaryResource > 15 &&
                           BelphegorSettings.Instance.Monk.UseTempestRushForMovement),
                new CastAtLocation(SNOPower.X1_Monk_DashingStrike, ctx => (Vector3) ctx,
                    ctx =>
                        ZetaDia.Me.Position.Distance((Vector3) ctx) >
                        15f),
                new Action(ret =>
                {
                    ZetaDia.Me.Movement.MoveActor((Vector3) ret);
                    return RunStatus.Success;
                })
                );
        }

        public static void MonkOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Monk)
                return;

            if (!BelphegorSettings.Instance.EnableLevelUpSkilling) return;

            int myLevel = ZetaDia.Me.Level;

            Log.InfoFormat("Player leveled up, congrats! Your level is now: {0}", myLevel);

            // Set Lashing tail kick once we reach level 2
            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_LashingTailKick, -1, 1);
                Log.Info("Setting Lash Tail Kick as Secondary");
            }

            // Set Dead reach it's better then Fists of thunder imo.
            if (myLevel == 3)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_DeadlyReach, -1, 0);
                Log.Info("Setting Deadly Reach as Primary");
            }

            // Make sure we set binding flash, useful spell in crowded situations!
            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_BlindingFlash, -1, 2);
                Log.Info("Setting Binding Flash as Defensive");
            }

            // Binding flash is nice but being alive is even better!
            if (myLevel == 8)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_BreathOfHeaven, -1, 2);
                Log.Info("Setting Breath of Heaven as Defensive");
            }

            // Make sure we set Dashing strike, very cool and useful spell great opener.
            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Monk_DashingStrike, -1, 3);
                Log.Info("Setting Dashing Strike as Techniques");
            }

            Hotbar.Update();
        }

        #region Timers

        private static readonly WaitTimer ExplodingPalm =
            new WaitTimer(TimeSpan.FromSeconds(BelphegorSettings.Instance.Monk.ExplodingPalmDelay));

        private static readonly WaitTimer AllyTimer = new WaitTimer(TimeSpan.FromSeconds(10));
        private static readonly WaitTimer PrimarySpamTimer = WaitTimer.OneSecond;

        static Monk()
        {
            GameEvents.OnGameLeft += OnGameLeft;
            GameEvents.OnPlayerDied += OnGameLeft;
        }

        private static void OnGameLeft(object sender, EventArgs e)
        {
            ExplodingPalm.Stop();
            AllyTimer.Stop();
            PrimarySpamTimer.Stop();
        }

        #endregion
    }
}