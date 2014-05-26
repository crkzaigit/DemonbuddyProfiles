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
    internal class Crusader
    {
        private static readonly WaitTimer PrimarySpamTimer = WaitTimer.OneSecond;
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static readonly ContextChangeHandler CtxChanger =
            ctx =>
                new CombatContext(
                    context => CombatTargeting.Instance.FirstNpc);

        [Class(ActorClass.Crusader)]
        [Behavior(BehaviorType.Combat)]
        public static Composite CrusaderCombat()
        {
            return new PrioritySelector(CtxChanger,
                Common.CreateUsePotion(),
                Common.CreateWaitWhileFearedStunnedFrozenOrBlind(),
                Common.CreateGetHealthGlobe(),
                Common.CreateUseHealthWell(),
                Common.CreateWaitForAttack(),

                #region Defensive

                new CastAtLocation(SNOPower.X1_Crusader_Judgment, ctx => ((CombatContext) ctx).TargetPosition,
                    ctx =>
                        (((CombatContext) ctx).CurrentTargetIsElite && ((CombatContext) ctx).TargetDistance <= 30) ||
                        Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget, ((CombatContext) ctx),
                            15f) >=
                        BelphegorSettings.Instance.Crusader.JudgmentAoECount),
                new CastOnUnit(SNOPower.X1_Crusader_ShieldGlare, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx =>
                        Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                        Clusters.GetFacingCount(16f, 70f, ((CombatContext) ctx)) >=
                        BelphegorSettings.Instance.Crusader.ShieldGlareAoECount),
                new SelfCast(SNOPower.X1_Crusader_IronSkin,
                    ret => ZetaDia.Me.HitpointsCurrentPct <= BelphegorSettings.Instance.Crusader.IronSkinHpPct),
                new SelfCast(SNOPower.X1_Crusader_Consecration,
                    ret => ZetaDia.Me.HitpointsCurrentPct <= BelphegorSettings.Instance.Crusader.ConsecrationHpPct),

                #endregion

                #region Laws
                new SelfCast(SNOPower.X1_Crusader_LawsOfHope2,
                    ret => ZetaDia.Me.HitpointsCurrentPct <= BelphegorSettings.Instance.Crusader.LawsOfHopeHpPct),
                new SelfCast(SNOPower.X1_Crusader_LawsOfJustice2,
                    ctx =>
                        Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                        ZetaDia.Me.HitpointsCurrentPct <= BelphegorSettings.Instance.Crusader.LawsOfJusticeHpPct),
                new SelfCast(SNOPower.X1_Crusader_LawsOfValor2,
                    ctx =>
                        Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                        (Hotbar.HasRune(SNOPower.X1_Crusader_LawsOfValor2, 4) &&
                         ((CombatContext) ctx).TargetDistance <= 15f)),

                #endregion

                #region Conviction

                new SelfCast(SNOPower.X1_Crusader_AkaratsChampion,
                    ctx => Unit.IsEliteInRange(16f, ((CombatContext) ctx)) || ZetaDia.Me.HitpointsCurrentPct < 0.30),
                new CastOnUnit(SNOPower.X1_Crusader_Bombardment, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx => ((CombatContext) ctx).CurrentTargetIsElite ||
                           Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx), 15f) >=
                           BelphegorSettings.Instance.Crusader.BombardmentAoECount),
                new CastOnUnit(SNOPower.X1_Crusader_FallingSword, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx =>
                        ((CombatContext) ctx).CurrentTargetIsElite ||
                        Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx), 15f) >=
                        BelphegorSettings.Instance.Crusader.FallingSwordAoECount),
                new CastOnUnit(SNOPower.X1_Crusader_HeavensFury3, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx =>
                        ((CombatContext) ctx).CurrentTargetIsElite ||
                        Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx), 15f) >=
                        BelphegorSettings.Instance.Crusader.HeavensFuryAoECount),

                #endregion

                #region Utility
                new SelfCast(SNOPower.X1_Crusader_Provoke,
                    ctx =>
                        ZetaDia.Me.CurrentPrimaryResource < 70 &&
                        (Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget, ((CombatContext) ctx),
                            15f) >= 3 || Unit.IsEliteInRange(14f, ((CombatContext) ctx)))),
                new SelfCast(SNOPower.X1_Crusader_Condemn,
                    ctx =>
                        (((CombatContext) ctx).CurrentTargetIsElite && ((CombatContext) ctx).TargetDistance <= 15f) ||
                        Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx), 15f) >=
                        BelphegorSettings.Instance.Crusader.CondemnAoECount),
                new SelfCast(SNOPower.X1_Crusader_SteedCharge,
                    ctx =>
                        ((CombatContext) ctx).TargetDistance >= BelphegorSettings.Instance.Crusader.SteedChargeMinRange),
                new CastOnUnit(SNOPower.x1_Crusader_Phalanx3, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx =>
                        Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget, ((CombatContext) ctx),
                            15f) >= 3),

                #endregion

                #region Secondary 
                //Secondary skills
                new SelfCast(SNOPower.X1_Crusader_BlessedHammer,
                    ctx =>
                        Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx), 18f) >=
                        BelphegorSettings.Instance.Crusader.BlessedHammerkAoECount),

                //If we have the 5th rune it no longer bounces
                new CastOnUnit(SNOPower.X1_Crusader_BlessedShield, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx => Hotbar.HasRune(SNOPower.X1_Crusader_BlessedShield, 5) ||
                           Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget, ((CombatContext) ctx),
                               14f) >= 3),
                new CastOnUnit(SNOPower.X1_Crusader_FistOfTheHeavens, ctx => ((CombatContext) ctx).TargetGuid),
                new CastOnUnit(SNOPower.X1_Crusader_ShieldBash2, ctx => ((CombatContext) ctx).TargetGuid),
                new CastOnUnit(SNOPower.X1_Crusader_SweepAttack, ctx => ((CombatContext) ctx).TargetGuid,
                    ctx => Unit.IsEliteInRange(18f, ((CombatContext) ctx)) ||
                           Clusters.GetFacingCount(18f, 100f, ((CombatContext) ctx)) >=
                           BelphegorSettings.Instance.Crusader.SweepAttackAoECount),

                #endregion

                #region Primary
                new CastOnUnit(SNOPower.X1_Crusader_Slash,
                    ctx => ((CombatContext) ctx).TargetGuid, null,
                    ctx => PrimarySpamTimer.Reset(),
                    keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger),
                new CastOnUnit(SNOPower.X1_Crusader_Smite,
                    ctx => ((CombatContext) ctx).TargetGuid, null,
                    ctx => PrimarySpamTimer.Reset(),
                    keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger),
                new CastOnUnit(SNOPower.X1_Crusader_Punish,
                    ctx => ((CombatContext) ctx).TargetGuid, null,
                    ctx => PrimarySpamTimer.Reset(),
                    keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger),
                new CastOnUnit(SNOPower.X1_Crusader_Justice,
                    ctx => ((CombatContext) ctx).TargetGuid, null,
                    ctx => PrimarySpamTimer.Reset(),
                    keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger)
                );

            #endregion
        }

        [Class(ActorClass.Crusader)]
        [Behavior(BehaviorType.Buff)]
        public static Composite CrusaderBuff()
        {
            return
                new Decorator(ctx => !ZetaDia.IsInTown,
                    new PrioritySelector(CtxChanger,
                        Common.CreateWaitForAttack(),
                        Common.CreateWaitForCast(),
                        Common.CreateGetPowerGlobe(),
                        Common.CreateUsePoolOfReflection(),
                        Avoidance.CreateMoveForAvoidance(BelphegorSettings.Instance.Crusader.MaximumRange)
                        )
                    );
        }

        [Class(ActorClass.Crusader)]
        [Behavior(BehaviorType.Movement)]
        public static Composite CrusaderMovement()
        {
            return new PrioritySelector(
                Common.CreateWaitForAttack(),
                Common.CreateWaitForCast(),
                new CastAtLocation(SNOPower.X1_Crusader_SteedCharge, ctx => (Vector3) ctx,
                    ctx => BelphegorSettings.Instance.Crusader.SteedChargeOOC &&
                           ZetaDia.Me.Position.Distance((Vector3) ctx) >
                           BelphegorSettings.Instance.Crusader.SteedChargeMinRange),
                new Action(ret =>
                {
                    ZetaDia.Me.Movement.MoveActor((Vector3) ret);
                    return RunStatus.Success;
                })
                );
        }

        public static void CrusaderOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Crusader)
                return;

            if (!BelphegorSettings.Instance.EnableLevelUpSkilling) return;

            int myLevel = ZetaDia.Me.Level;

            Log.InfoFormat("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );


            if (myLevel == 2)
            {
                Log.Info("Setting Shield Bash");
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_ShieldBash2, -1, 1);
            }

            if (myLevel == 3)
            {
                Log.Info("Setting Slash");
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Slash, -1, 0);
            }

            if (myLevel == 4)
            {
                Log.Info("Setting Shield Glare");
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_ShieldGlare, -1, 2);
            }

            if (myLevel == 5)
            {
                Log.Info("Setting Sweep Attack");
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_SweepAttack, -1, 1);
            }

            if (myLevel == 6)
            {
                Log.Info("Setting Punish (Roar)");
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Punish, 3, 1);
            }

            if (myLevel == 9)
            {
                Log.Info("Setting Provoke");
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Provoke, -1, 3);
            }

            if (myLevel == 10)
            {
                Log.Info("Setting Passive Fervor");
                ZetaDia.Me.SetTraits(SNOPower.X1_Crusader_Passive_Fervor);
            }

            if (myLevel == 12)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Punish, 3, 0);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_SweepAttack, 1, 1);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_ShieldGlare, 0, 2);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Provoke, -1, 3);
                ZetaDia.Me.SetTraits(SNOPower.X1_Crusader_Passive_Fervor);
            }

            if (myLevel == 14)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Punish, 3, 0);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_SweepAttack, 1, 1);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_ShieldGlare, 0, 2);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_Provoke, -1, 3);
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_Crusader_LawsOfValor, -1, 4);
                ZetaDia.Me.SetTraits(SNOPower.X1_Crusader_Passive_Fervor);
            }

            Hotbar.Update();
        }
    }
}