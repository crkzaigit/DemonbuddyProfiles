using System;
using System.Linq;
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
    public class DemonHunter
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static readonly ContextChangeHandler CtxChanger =
            ctx => new CombatContext(context => CombatTargeting.Instance.FirstNpc);

        private static readonly WaitTimer VaultMovementTimer = new WaitTimer(TimeSpan.FromSeconds(2));
        private static readonly WaitTimer MarkTimer = new WaitTimer(TimeSpan.FromSeconds(10));
        private static readonly WaitTimer SmokeScreenTimer = new WaitTimer(TimeSpan.FromSeconds(10));
        private static readonly WaitTimer CompanionTimer = new WaitTimer(TimeSpan.FromSeconds(10));
        private static readonly WaitTimer ShadowPowerTimer = new WaitTimer(TimeSpan.FromSeconds(3.5));
        private static readonly WaitTimer TrapTimer = new WaitTimer(TimeSpan.FromSeconds(2));
        private static readonly WaitTimer ShurikenCloudTimer = new WaitTimer(TimeSpan.FromSeconds(100));

        static DemonHunter()
        {
            GameEvents.OnGameLeft += OnGameLeft;
            GameEvents.OnPlayerDied += OnGameLeft;
        }

        [Class(ActorClass.DemonHunter)]
        [Behavior(BehaviorType.Buff)]
        public static Composite DemonHunterBuffs()
        {
            return
                new PrioritySelector(CtxChanger,
                    Common.CreateWaitForAttack(),
                    Common.CreateWaitForCast(),
                    Common.CreateGetPowerGlobe(),
                    Common.CreateUsePoolOfReflection(),
                    new SelfCast(SNOPower.X1_DemonHunter_Companion,
                        ctx =>
                            CompanionTimer.IsFinished && !Minion.HasPet(((CombatContext) ctx), Pet.DH_Companion),
                        s => CompanionTimer.Reset()),
                    new SelfCast(SNOPower.DemonHunter_Chakram,
                        extra =>
                            BelphegorSettings.Instance.DemonHunter.ShurikenCloud &&
                            ShurikenCloudTimer.IsFinished, s => ShurikenCloudTimer.Reset()),
                    new SelfCast(SNOPower.DemonHunter_SmokeScreen,
                        extra =>
                            SmokeScreenTimer.IsFinished && ZetaDia.Me.HitpointsCurrentPct <=
                            BelphegorSettings.Instance.DemonHunter
                                .SmokeScreenHP ||
                            BelphegorSettings.Instance.DemonHunter.SpamSmokeScreen,
                        s => SmokeScreenTimer.Reset()),
                    Avoidance.CreateMoveForAvoidance(
                        BelphegorSettings.Instance.DemonHunter.MaximumRange)
                    );
        }

        [Class(ActorClass.DemonHunter)]
        [Behavior(BehaviorType.Combat)]
        public static Composite DemonHunterCombat()
        {
            return
                new PrioritySelector(CtxChanger,
                    new Decorator(
                        ctx =>
                            ctx is CombatContext &&
                            ((CombatContext) ctx).CurrentTarget != null,
                        new PrioritySelector(
                            new SelfCast(SNOPower.DemonHunter_SmokeScreen,
                                extra => ((CombatContext) extra).IsPlayerIncapacited),
                            Common.CreateUsePotion(),
                            Common.CreateWaitWhileFearedStunnedFrozenOrBlind(),
                            Common.CreateGetHealthGlobe(),
                            Common.CreateUseHealthWell(),
                            Kiting.CreateKitingBehavior(),
                            Common.CreateWaitForAttack(),
                            new SelfCast(SNOPower.DemonHunter_ShadowPower,
                                extra =>
                                    ShadowPowerTimer.IsFinished &&
                                    ((CombatContext) extra).CurrentHealthPercentage <
                                    BelphegorSettings.Instance.DemonHunter.ShadowPowerHp,
                                s => ShadowPowerTimer.Reset()),
                            new CastOnUnit(SNOPower.DemonHunter_MarkedForDeath,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                ctx =>
                                    ((CombatContext) ctx).CurrentTargetIsElite &&
                                    MarkTimer.IsFinished,
                                s => MarkTimer.Reset()),
                            new SelfCast(SNOPower.X1_DemonHunter_Vengeance,
                                ctx => ((CombatContext) ctx).CurrentTargetIsElite),
                            new SelfCast(SNOPower.DemonHunter_Caltrops,
                                ctx =>
                                    TrapTimer.IsFinished &&
                                    (Minion.PetCount(((CombatContext) ctx), Pet.DH_Caltrops) < 3 &&
                                     (Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                         16f) >=
                                      BelphegorSettings.Instance.DemonHunter.CaltropsAoECount ||
                                      Unit.IsEliteInRange(16f, ((CombatContext) ctx))) ||
                                     (BelphegorSettings.Instance.DemonHunter.StandInCaltrops &&
                                      Minion.GetMinions(((CombatContext) ctx), Pet.DH_Caltrops).Count(
                                          c => c.Distance < 8f) < 1)),
                                s => TrapTimer.Reset()),
                            new SelfCast(SNOPower.DemonHunter_SpikeTrap,
                                ctx =>
                                    TrapTimer.IsFinished && Minion.PetCount(((CombatContext) ctx), Pet.DH_SpikeTrap) < 3 &&
                                    Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                        16f) >=
                                    BelphegorSettings.Instance.DemonHunter.SpikeTrapAoECount ||
                                    Unit.IsEliteInRange(16f, ((CombatContext) ctx)),
                                s => TrapTimer.Reset()),
                            // AOE
                            new CastAtLocation(SNOPower.DemonHunter_RainOfVengeance,
                                ctx => ((CombatContext) ctx).TargetPosition,
                                ctx =>
                                    Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget, ((CombatContext) ctx),
                                        20f) >= 3 || ((CombatContext) ctx).CurrentTargetIsElite),
                            new CastOnUnit(SNOPower.DemonHunter_Strafe,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                req =>
                                    Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) req),
                                        45f) >= 2
                                ),
                            new CastAtLocation(SNOPower.DemonHunter_Multishot,
                                ctx => ((CombatContext) ctx).TargetPosition,
                                req =>
                                    Clusters.GetClusterCount(
                                        ((CombatContext) req).CurrentTarget,
                                        ((CombatContext) req), 45f) >= 2
                                ),
                            new CastOnUnit(SNOPower.DemonHunter_FanOfKnives,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                ctx =>
                                    ((CombatContext) ctx).CurrentTargetIsElite &&
                                    ((CombatContext) ctx).TargetDistance < 20f ||
                                    Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx),
                                        20f) >=
                                    BelphegorSettings.Instance.DemonHunter.FanOfKnivesAoECount
                                ),
                            new CastOnUnit(SNOPower.DemonHunter_ClusterArrow,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                req =>
                                    Clusters.GetClusterCount(
                                        ((CombatContext) req).CurrentTarget,
                                        ((CombatContext) req), 15f) >= 2 ||
                                    ((CombatContext) req).CurrentTargetIsElite
                                ),
                            new CastAtLocation(SNOPower.DemonHunter_Sentry,
                                ctx => ((CombatContext) ctx).TargetPosition,
                                ctx => !Minion.HasPet(((CombatContext) ctx), Pet.DH_Sentry) &&
                                       Clusters.GetClusterCount(ZetaDia.Me,
                                           ((CombatContext) ctx),
                                           35f) >=
                                       2 ||
                                       ((CombatContext) ctx).CurrentTargetIsElite
                                ),
                            new SelfCast(SNOPower.DemonHunter_Preparation,
                                extra => ZetaDia.Me.CurrentSecondaryResource <=
                                         BelphegorSettings.Instance.DemonHunter.
                                             PrperationDiscipline),
                            // Singles
                            new CastOnUnit(SNOPower.DemonHunter_Chakram,
                                ctx => ((CombatContext) ctx).TargetGuid),
                            new CastOnUnit(SNOPower.X1_DemonHunter_EvasiveFire,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                ctx =>
                                    !BelphegorSettings.Instance.DemonHunter.
                                        OnlyEvasiveFireWhenClose ||
                                    ((CombatContext) ctx).TargetDistance <= 16f),
                            new CastOnUnit(SNOPower.DemonHunter_Impale,
                                ctx => ((CombatContext) ctx).TargetGuid),
                            new CastOnUnit(SNOPower.DemonHunter_RapidFire,
                                ctx => ((CombatContext) ctx).TargetGuid,
                                extra => ZetaDia.Me.CurrentPrimaryResource > 20),
                            new CastOnUnit(SNOPower.DemonHunter_ElementalArrow,
                                ctx => ((CombatContext) ctx).TargetGuid),
                            // Hatred Generators
                            new CastOnUnit(SNOPower.DemonHunter_Bolas,
                                ctx => ((CombatContext) ctx).TargetGuid),
                            new CastOnUnit(SNOPower.DemonHunter_Grenades,
                                ctx => ((CombatContext) ctx).TargetGuid),
                            new CastOnUnit(SNOPower.DemonHunter_HungeringArrow,
                                ctx => ((CombatContext) ctx).TargetGuid),
                            new CastOnUnit(SNOPower.X1_DemonHunter_EntanglingShot,
                                ctx => ((CombatContext) ctx).TargetGuid)
                            )
                        ),
                    new Action(ret => RunStatus.Success)
                    );
        }

        [Class(ActorClass.DemonHunter)]
        [Behavior(BehaviorType.Movement)]
        public static Composite DemonHunterMovement()
        {
            return new PrioritySelector(
                Common.CreateWaitForAttack(),
                Common.CreateWaitForCast(),
                new SelfCast(SNOPower.DemonHunter_Preparation,
                    ctx => BelphegorSettings.Instance.DemonHunter.UsePreparationForMovement),
                new SelfCast(SNOPower.DemonHunter_ShadowPower,
                    ctx =>
                        BelphegorSettings.Instance.DemonHunter.UseShadowPowerForMovement &&
                        !ZetaDia.Me.HasBuff(SNOPower.DemonHunter_ShadowPower)),
                new CastAtLocation(SNOPower.DemonHunter_Vault,
                    ctx => (Vector3) ctx,
                    ctx =>
                        BelphegorSettings.Instance.DemonHunter.UseVaultForMovement &&
                        ZetaDia.Me.CurrentSecondaryResource > BelphegorSettings.Instance.DemonHunter.VaultDiscipline &&
                        VaultMovementTimer.IsFinished &&
                        ZetaDia.Me.Position.Distance((Vector3) ctx) >
                        BelphegorSettings.Instance.DemonHunter.VaultDistance,
                    s => VaultMovementTimer.Reset()),
                new Action(ret =>
                {
                    ZetaDia.Me.Movement.MoveActor((Vector3) ret);
                    return RunStatus.Success;
                })
                );
        }

        private static void OnGameLeft(object sender, EventArgs e)
        {
            VaultMovementTimer.Stop();
            MarkTimer.Stop();
            SmokeScreenTimer.Stop();
            CompanionTimer.Stop();
            ShadowPowerTimer.Stop();
            TrapTimer.Stop();
            ShurikenCloudTimer.Stop();
        }

        public static void DemonHunterOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.DemonHunter)
                return;

            if (!BelphegorSettings.Instance.EnableLevelUpSkilling) return;

            int myLevel = ZetaDia.Me.Level;

            Log.InfoFormat("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            #region Primary Slot

            if (myLevel == 6)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_HungeringArrow, 1, 0);
                Log.Info("Add [R] Puncturing Arrow to Hungering Arrow");
            }
            if (myLevel == 14)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_DemonHunter_EvasiveFire, -1, 0);
                Log.Info("Setting Evasive Fire as Primary skill");
            }
            if (myLevel == 21)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_DemonHunter_EvasiveFire, 1, 0);
                Log.Info("Setting [R] Shrapnel to Evasive Fire");
            }
            if (myLevel == 34)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_DemonHunter_EvasiveFire, 3, 0);
                Log.Info("Setting [R] Covering Fire to Evasive Fire");
            }

            #endregion

            #region Secondary Slot

            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Impale, -1, 1);
                Log.Info("Setting Impale as Secondary skill");
            }

            if (myLevel == 22)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Multishot, -1, 1);
                Log.Info("Setting Multishot as Secondary skill");
            }

            if (myLevel == 26)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Multishot, 1, 1);
                Log.Info("Setting [R] Fire at Will to Multishot");
            }

            #endregion

            #region Defensive Slot

            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Caltrops, -1, 2);
                Log.Info("Setting Caltrops as Defensive skill");
            }

            if (myLevel == 8)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_SmokeScreen, -1, 2);
                Log.Info("Setting Smoke Screen as Defensive skill");
            }

            if (myLevel == 14)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_SmokeScreen, 1, 2);
                Log.Info("Setting [R] Displacement to Smoke Screen");
            }

            if (myLevel == 33)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_SmokeScreen, 3, 2);
                Log.Info("Setting [R] Breathe Deep to Smoke Screen");
            }

            #endregion

            #region Hunting Slot

            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_Vault, -1, 3);
                Log.Info("Setting Vault as Hunting skill");
            }
            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.X1_DemonHunter_Companion, -1, 3);
                Log.Info("Setting Companion as Hunting skill");
            }

            #endregion

            #region Device Slot

            if (myLevel == 21)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_MarkedForDeath, -1, 4);
                Log.Info("Setting Marked for Death as Device skill");
            }
            if (myLevel == 27)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_MarkedForDeath, 1, 4);
                Log.Info("Setting [R] Contagion to Marked for Death");
            }

            #endregion

            #region Archery Slot

            if (myLevel == 16)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.DemonHunter_ShadowPower, -1, 5);
                Log.Info("Setting Shadow Power as Archery skill");
            }

            #endregion

            Hotbar.Update();
        }
    }
}