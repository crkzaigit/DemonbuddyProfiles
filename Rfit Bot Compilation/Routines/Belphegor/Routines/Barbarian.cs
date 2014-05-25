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
    public class Barbarian
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static readonly ContextChangeHandler CtxChanger =
            ctx =>
                new CombatContext(
                    context => CombatTargeting.Instance.FirstNpc);

        [Class(ActorClass.Barbarian)]
        [Behavior(BehaviorType.Buff)]
        public static Composite BarbarianBuffs()
        {
            return
                new PrioritySelector(CtxChanger,
                    Common.CreateWaitForAttack(),
                    Common.CreateWaitForCast(),
                    Common.CreateGetPowerGlobe(),
                    Common.CreateUsePoolOfReflection(),
                    new SelfCast(SNOPower.X1_Barbarian_WarCry_v2,
                        extra => (!ZetaDia.Me.HasBuff(SNOPower.X1_Barbarian_WarCry_v2)
                                  || BelphegorSettings.Instance.Barbarian.SpamWarCry)
                                 && !ZetaDia.IsInTown),
                    new SelfCast(SNOPower.Barbarian_BattleRage,
                        extra => !ZetaDia.Me.HasBuff(SNOPower.Barbarian_BattleRage)),
                    Avoidance.CreateMoveForAvoidance(BelphegorSettings.Instance.Barbarian.MaximumRange)
                    );
        }

        [Class(ActorClass.Barbarian)]
        [Behavior(BehaviorType.Combat)]
        public static Composite BarbarianCombat()
        {
            return
                new PrioritySelector(CtxChanger,
                    new Decorator(ctx => ctx != null && ((CombatContext) ctx).CurrentTarget != null,
                        new PrioritySelector(
                            // Buff attack rate or get free!
                            new SelfCast(SNOPower.Barbarian_WrathOfTheBerserker,
                                extra => ((CombatContext) extra).IsPlayerIncapacited
                                ),
                            Common.CreateUsePotion(),
                            Common.CreateWaitWhileFearedStunnedFrozenOrBlind(),
                            Common.CreateGetHealthGlobe(),
                            Common.CreateUseHealthWell(),
                            Common.CreateWaitForAttack(),

                            // Defence low hp or many attackers.
                            new SelfCast(SNOPower.Barbarian_IgnorePain, require =>
                                ((CombatContext) require).CurrentHealthPercentage <=
                                BelphegorSettings.Instance.Barbarian.IgnorePainPct
                                ||
                                Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) require, 12f) >=
                                6
                                ),
                            ThrowBarbBehavior(),
                            // Pull phase.
                            new Decorator(ctx =>
                                ((CombatContext) ctx).TargetDistance > BelphegorSettings.Instance.Barbarian.MaximumRange &&
                                !((CombatContext) ctx).IsPlayerIncapacited,
                                new PrioritySelector(
                                    new CastAtLocation(SNOPower.Barbarian_Leap,
                                        ctx => ((CombatContext) ctx).TargetPosition),
                                    new CastOnUnit(SNOPower.Barbarian_FuriousCharge,
                                        ctx => ((CombatContext) ctx).TargetGuid),
                                    new CastOnUnit(SNOPower.X1_Barbarian_AncientSpear,
                                        ctx => ((CombatContext) ctx).TargetGuid)
                                    )
                                ),
                            new CastAtLocation(SNOPower.X1_Barbarian_Avalanche_v2,
                                ctx => ((CombatContext) ctx).TargetPosition,
                                ctx => ((CombatContext) ctx).CurrentTargetIsElite ||
                                       Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget,
                                           ((CombatContext) ctx), 15f) >=
                                       BelphegorSettings.Instance.Barbarian.AvalancheAoeCount),
                            //Leap on cooldown, usefull for the increased armour buff
                            new CastAtLocation(SNOPower.Barbarian_Leap, ctx => ((CombatContext) ctx).TargetPosition,
                                extra => BelphegorSettings.Instance.Barbarian.LeapOnCooldown
                                         && !((CombatContext) extra).IsPlayerIncapacited
                                ),
                            new SelfCast(SNOPower.Barbarian_Sprint,
                                extra => SprintTimer.IsFinished
                                         && !ZetaDia.Me.HasBuff(SNOPower.Barbarian_Sprint),
                                s => SprintTimer.Reset()
                                ),
                            new SelfCast(SNOPower.Barbarian_Rend,
                                ctx => RendTimer.IsFinished &&
                                       ((CombatContext) ctx).TargetDistance <=
                                       BelphegorSettings.Instance.Barbarian.RendRange,
                                s => RendTimer.Reset()),
                            new CastAtLocation(SNOPower.Barbarian_Revenge,
                                ctx => ((CombatContext) ctx).TargetPosition,
                                ctx => ((CombatContext) ctx).TargetDistance < 16f
                                ),

                            //Uses Dreadnought rune to heal
                            new CastOnUnit(SNOPower.Barbarian_FuriousCharge, ctx => ((CombatContext) ctx).TargetGuid,
                                extra => BelphegorSettings.Instance.Barbarian.FuriousChargeDreadnought
                                         &&
                                         ((CombatContext) extra).CurrentHealthPercentage <=
                                         BelphegorSettings.Instance.Barbarian.FuriousChargeDreadnoughtHP
                                ),

                            //Rage
                            new SelfCast(SNOPower.Barbarian_WrathOfTheBerserker,
                                ctx => Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                                       Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx, 16f) >=
                                       BelphegorSettings.Instance.Barbarian.WotBAoeCount
                                ),
                            new SelfCast(SNOPower.Barbarian_CallOfTheAncients,
                                ctx => Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                                       Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx, 16f) >=
                                       BelphegorSettings.Instance.Barbarian.CotAAoeCount
                                ),
                            new SelfCast(SNOPower.Barbarian_Earthquake,
                                ctx => Unit.IsEliteInRange(16f, ((CombatContext) ctx)) ||
                                       Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx, 16f) >=
                                       BelphegorSettings.Instance.Barbarian.EarthquakeAoeCount
                                ),
                            new SelfCast(SNOPower.Barbarian_GroundStomp,
                                ctx =>
                                    Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx, 12f) >=
                                    2
                                    || Unit.IsEliteInRange(18f, ((CombatContext) ctx))
                                ),
                            new SelfCast(SNOPower.Barbarian_Overpower,
                                ctx =>
                                    Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx, 16f) >=
                                    BelphegorSettings.Instance.Barbarian.OverpowerAoeCount
                                    || Unit.IsEliteInRange(16f, ((CombatContext) ctx))
                                ),

                            // Threatning shout.
                            new SelfCast(SNOPower.Barbarian_ThreateningShout,
                                ctx =>
                                    Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx, 16f) >=
                                    2
                                    || Unit.IsEliteInRange(16f, ((CombatContext) ctx))
                                ),

                            // Fury spenders.
                            new CastOnUnit(SNOPower.Barbarian_HammerOfTheAncients,
                                ctx => ((CombatContext) ctx).TargetGuid
                                ),
                            new CastAtLocation(SNOPower.Barbarian_SeismicSlam,
                                ctx => ((CombatContext) ctx).TargetPosition
                                ),
                            new CastOnUnit(SNOPower.X1_Barbarian_WeaponThrow,
                                ctx => ((CombatContext) ctx).TargetGuid
                                ),
                            new CastAtLocation(SNOPower.Barbarian_Whirlwind,
                                ctx => ((CombatContext) ctx).WhirlWindTargetPosition,
                                ctx =>
                                    Clusters.GetClusterCount(ZetaDia.Me, (CombatContext) ctx,
                                        BelphegorSettings.Instance.Barbarian.WhirlwindClusterRange) >=
                                    BelphegorSettings.Instance.Barbarian.WhirlwindAoeCount
                                    ||
                                    ((CombatContext) ctx).CurrentTargetIsElite &&
                                    ((CombatContext) ctx).TargetDistance <= 10f
                                ),

                            // Fury Generators
                            new CastOnUnit(SNOPower.Barbarian_Cleave,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger
                                ),
                            new CastOnUnit(SNOPower.Barbarian_Bash,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger
                                ),
                            new CastOnUnit(SNOPower.Barbarian_Frenzy,
                                ctx => ((CombatContext) ctx).TargetGuid, null,
                                ctx => PrimarySpamTimer.Reset(),
                                keepSpamming => !PrimarySpamTimer.IsFinished, CtxChanger
                                )

                            //new Action(ctx => ZetaDia.Me.UsePower(SNOPower.Weapon_Melee_Instant, ((DiaUnit)ctx).Position, ZetaDia.CurrentWorldDynamicId))
                            )
                        ),
                    new Action(ret => RunStatus.Success)
                    );
        }

        [Class(ActorClass.Barbarian)]
        [Behavior(BehaviorType.Movement)]
        public static Composite BarbarianMovement()
        {
            return
                new PrioritySelector(
                    Common.CreateWaitForAttack(),
                    Common.CreateWaitForCast(),
                    new SelfCast(SNOPower.Barbarian_Sprint,
                        extra => SprintTimer.IsFinished && !ZetaDia.Me.HasBuff(SNOPower.Barbarian_Sprint),
                        s => SprintTimer.Reset()),
                    new CastAtLocation(SNOPower.Barbarian_Leap, ctx => (Vector3) ctx,
                        ctx =>
                            BelphegorSettings.Instance.Barbarian.UseLeapForMovement &&
                            ZetaDia.Me.Position.Distance((Vector3) ctx) >
                            BelphegorSettings.Instance.Barbarian.LeapDistance),
                    new CastAtLocation(SNOPower.Barbarian_FuriousCharge, ctx => (Vector3) ctx,
                        ctx =>
                            BelphegorSettings.Instance.Barbarian.UseFuriousChargeForMovement &&
                            ZetaDia.Me.Position.Distance((Vector3) ctx) >
                            BelphegorSettings.Instance.Barbarian.FuriousChargeDistance),
                    new Action(ret =>
                    {
                        ZetaDia.Me.Movement.MoveActor((Vector3) ret);
                        return RunStatus.Success;
                    })
                    );
        }

        public static Composite ThrowBarbBehavior()
        {
            return new Decorator(
                ctx => BelphegorSettings.Instance.Barbarian.IsThrowBarbEnabled,
                new PrioritySelector(
                    new SelfCast(SNOPower.Barbarian_BattleRage,
                        extra => !ZetaDia.Me.HasBuff(SNOPower.Barbarian_BattleRage)),
                    new SelfCast(SNOPower.X1_Barbarian_WarCry_v2,
                        extra => !ZetaDia.Me.HasBuff(SNOPower.X1_Barbarian_WarCry_v2)),
                    new SelfCast(SNOPower.Barbarian_Overpower),
                    new CastOnUnit(SNOPower.X1_Barbarian_AncientSpear, ctx => ((CombatContext) ctx).TargetGuid, null,
                        ctx => PrimarySpamTimer.Reset(), keepSpamming => !PrimarySpamTimer.IsFinished,
                        CtxChanger),
                    new CastOnUnit(SNOPower.X1_Barbarian_WeaponThrow, ctx => ((CombatContext) ctx).TargetGuid),
                    new SelfCast(SNOPower.X1_Barbarian_WarCry_v2)
                    )
                );
        }

        public static void BarbarianOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Barbarian)
                return;

            if (!BelphegorSettings.Instance.EnableLevelUpSkilling) return;

            int myLevel = ZetaDia.Me.Level;

            Log.InfoFormat("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            #region Primarey Slot

            if (myLevel == 18)
            {
                Log.Info("Add [R] Ravage to Cleave");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Cleave, 2, 0);
            }
            else if (myLevel == 9)
            {
                Log.Info("Add [R] Rupture to Cleave");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Cleave, 1, 0);
            }
            else if (myLevel == 3)
            {
                Log.Info("Equip Cleave");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Cleave, -1, 0);
            }

            #endregion

            #region Secondary Slot

            if (myLevel == 19)
            {
                Log.Info("Add [R] Blood Lust to Rend. ");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Rend, 2, 1);
            }
            else if (myLevel == 11)
            {
                Log.Info("Equip Rend with [R] Ravage in the place of Hammer of the Ancients.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Rend, 1, 1);
            }
            else if (myLevel == 7)
            {
                Log.Info("Add [R] Rolling Thunder to Hammer of the Ancients.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_HammerOfTheAncients, 1, 1);
            }
            else if (myLevel == 2)
            {
                Log.Info("Setting Hammer of the Ancients");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_HammerOfTheAncients, -1, 1);
            }

            #endregion

            #region Defencive Slot

            if (myLevel == 27)
            {
                Log.Info("Add [R] Battering Ram to Furious Charge. ");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_FuriousCharge, 1, 2);
            }
            else if (myLevel == 21)
            {
                Log.Info("Equip Furious Charge in the place of Leap.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_FuriousCharge, -1, 2);
            }
            else if (myLevel == 14)
            {
                Log.Info("Add [R] Iron Impact to Leap.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Leap, -1, 2);
            }
            else if (myLevel == 8)
            {
                Log.Info("Equip Leap in the place of Ground Stomp.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Leap, -1, 2);
            }
            else if (myLevel == 4)
            {
                Log.Info("Setting Ground Stomp");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_GroundStomp, -1, 2);
            }

            #endregion

            #region Might Slot

            if (myLevel == 26)
            {
                Log.Info("Add [R] Marauder's Rage to Battle Rage. ");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_BattleRage, 1, 3);
            }
            else if (myLevel == 22)
            {
                Log.Info("Equip Battle Rage in the place of Threatening Shout.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_BattleRage, -1, 3);
            }
            else if (myLevel == 17)
            {
                Log.Info("Equip Threatening Shout in the place of Ground Stomp.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_ThreateningShout, -1, 3);
            }
            else if (myLevel == 12)
            {
                Log.Info("Add [R] Deafening Crash to Ground Stomp.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_GroundStomp, 1, 3);
            }
            else if (myLevel == 9)
            {
                Log.Info("Equip Ground Stomp.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_GroundStomp, -1, 3);
            }

            #endregion

            #region Tactics Slot

            if (myLevel == 19)
            {
                Log.Info("Add [R] Vengeance Is Mine to Revenge. ");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Revenge, 1, 4);
            }
            if (myLevel == 14)
            {
                Log.Info("Equip Revenge. ");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Revenge, -1, 4);
            }

            #endregion

            #region Rage Slot

            if (myLevel == 29)
            {
                Log.Info("Add [R] Storm of Steel to Overpower.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Overpower, 1, 5);
            }
            else if (myLevel == 26)
            {
                Log.Info("Equip Overpower in the place of Earthquake.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Overpower, -1, 5);
            }
            else if (myLevel == 24)
            {
                Log.Info("Add [R] Giant's Stride to Earthquake.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Earthquake, 1, 5);
            }
            else if (myLevel == 19)
            {
                Log.Info("Equip Earthquake.");
                ZetaDia.Me.SetActiveSkill(SNOPower.Barbarian_Earthquake, -1, 5);
            }

            #endregion

            #region Passive Skills

            if (myLevel == 30)
            {
                Log.Info("Equip [P] Bloodthirst.");
                ZetaDia.Me.SetTraits(SNOPower.Barbarian_Passive_WeaponsMaster,
                    SNOPower.Barbarian_Passive_InspiringPresence,
                    SNOPower.Barbarian_Passive_Bloodthirst);
            }
            if (myLevel == 20)
            {
                Log.Info("Equip [P] Inspiring Presence.");
                ZetaDia.Me.SetTraits(SNOPower.Barbarian_Passive_WeaponsMaster,
                    SNOPower.Barbarian_Passive_InspiringPresence);
            }
            else if (myLevel == 16)
            {
                Log.Info("Equip [P] Weapons Master in the place of [P] Ruthless.");
                ZetaDia.Me.SetTraits(SNOPower.Barbarian_Passive_WeaponsMaster);
            }
            else if (myLevel == 10)
            {
                Log.Info("Equip [P] Ruthless.");
                ZetaDia.Me.SetTraits(SNOPower.Barbarian_Passive_Ruthless);
            }

            #endregion

            Hotbar.Update();
        }

        #region timmers

        private static readonly WaitTimer SprintTimer = new WaitTimer(TimeSpan.FromSeconds(3));
        private static readonly WaitTimer RendTimer = new WaitTimer(TimeSpan.FromSeconds(4.5));
        private static readonly WaitTimer PrimarySpamTimer = WaitTimer.OneSecond;

        static Barbarian()
        {
            GameEvents.OnGameLeft += OnGameLeft;
            GameEvents.OnPlayerDied += OnGameLeft;
        }


        private static void OnGameLeft(object sender, EventArgs e)
        {
            SprintTimer.Stop();
            RendTimer.Stop();
            PrimarySpamTimer.Stop();
        }

        #endregion
    }
}