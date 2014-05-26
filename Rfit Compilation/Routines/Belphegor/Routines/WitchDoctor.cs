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
    public class WitchDoctor
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();


        private static readonly ContextChangeHandler CtxChanger =
            ctx => new CombatContext(context => CombatTargeting.Instance.FirstNpc);

        [Class(ActorClass.Witchdoctor)]
        [Behavior(BehaviorType.Buff)]
        public static Composite WitchDoctorBuff()
        {
            return
                new PrioritySelector(CtxChanger,
                    Common.CreateWaitForAttack(),
                    Common.CreateWaitForCast(),
                    Common.CreateGetPowerGlobe(),
                    Common.CreateUsePoolOfReflection(),
                    new SelfCast(SNOPower.Witchdoctor_Gargantuan,
                        ctx => !Minion.HasPet(((CombatContext) ctx), Pet.Gargantuan)),
                    new SelfCast(SNOPower.Witchdoctor_SummonZombieDog,
                        ctx => Minion.PetCount(((CombatContext) ctx), Pet.ZombieDogs) < 3),
                    Avoidance.CreateMoveForAvoidance(
                        BelphegorSettings.Instance.WitchDoctor.MaximumRange)
                    );
        }

        [Class(ActorClass.Witchdoctor)]
        [Behavior(BehaviorType.Combat)]
        public static Composite WitchDoctorCombat()
        {
            return new PrioritySelector(CtxChanger,
                new Decorator(
                    ctx =>
                        ctx is CombatContext &&
                        ((CombatContext) ctx).CurrentTarget != null,
                    new PrioritySelector(
                        Common.CreateUsePotion(),
                        Common.CreateWaitWhileFearedStunnedFrozenOrBlind(),
                        Common.CreateGetHealthGlobe(),
                        Common.CreateUseHealthWell(),
                        Common.CreateWaitForAttack(),
                        Kiting.CreateKitingBehavior(),
                        // Make sure we are within range/line of sight of the unit.
                        Movement.MoveTo(ctx => ((CombatContext) ctx).TargetPosition,
                            BelphegorSettings.Instance.WitchDoctor.MaximumRange),
                        new SelfCast(SNOPower.Witchdoctor_SpiritWalk,
                            extra =>
                                ((CombatContext) extra).CurrentHealthPercentage <=
                                BelphegorSettings.Instance.WitchDoctor.SpiritWalkHp),
                        new SelfCast(SNOPower.Witchdoctor_Sacrifice,
                            ctx => Minion.PetCount(((CombatContext) ctx), Pet.ZombieDogs) > 1 &&
                                   ((CombatContext) ctx).CurrentHealthPercentage <=
                                   BelphegorSettings.Instance.WitchDoctor.SacrificeHp),
                        new SelfCast(SNOPower.Witchdoctor_Hex),
                        new SelfCast(SNOPower.Witchdoctor_SoulHarvest,
                            ctx =>
                                Clusters.GetClusterCount(ZetaDia.Me, ((CombatContext) ctx), 16f) >=
                                BelphegorSettings.Instance.WitchDoctor.SoulHarvestAoECount ||
                                (((CombatContext) ctx).CurrentTargetIsElite &&
                                 ((CombatContext) ctx).TargetDistance < 16f)),
                        new CastAtLocation(SNOPower.Witchdoctor_Piranhas, ctx => ((CombatContext) ctx).TargetPosition,
                            ctx =>
                                ((CombatContext) ctx).CurrentTargetIsElite ||
                                Clusters.GetClusterCount(((CombatContext) ctx).CurrentTarget, ((CombatContext) ctx), 10f) >=
                                BelphegorSettings.Instance.WitchDoctor.PiranhasAoECount),
                        new SelfCast(SNOPower.Witchdoctor_BigBadVoodoo,
                            ctx =>
                                Clusters.GetClusterCount(
                                    ((CombatContext) ctx).CurrentTarget,
                                    ((CombatContext) ctx), 40f) >=
                                BelphegorSettings.Instance.WitchDoctor.BigBadVoodooAoECount ||
                                ((CombatContext) ctx).CurrentTargetIsElite),
                        new SelfCast(SNOPower.Witchdoctor_FetishArmy,
                            ctx =>
                                Clusters.GetClusterCount(
                                    ((CombatContext) ctx).CurrentTarget,
                                    ((CombatContext) ctx), 40f) >=
                                BelphegorSettings.Instance.WitchDoctor.FetishArmyAoECount ||
                                ((CombatContext) ctx).CurrentTargetIsElite),
                        new SelfCast(SNOPower.Witchdoctor_Horrify,
                            extra =>
                                Clusters.GetClusterCount(
                                    ((CombatContext) extra).CurrentTarget,
                                    ((CombatContext) extra), 40f) >=
                                BelphegorSettings.Instance.WitchDoctor.HorrifyAoECount),
                        new CastOnUnit(SNOPower.Witchdoctor_MassConfusion,
                            ctx => ((CombatContext) ctx).TargetGuid,
                            extra =>
                                Clusters.GetClusterCount(
                                    ((CombatContext) extra).CurrentTarget,
                                    ((CombatContext) extra), 40f) >=
                                BelphegorSettings.Instance.WitchDoctor.
                                    MassConfusionAoECount),
                        new CastOnUnit(SNOPower.Witchdoctor_GraspOfTheDead,
                            ctx => ((CombatContext) ctx).TargetGuid),
                        new CastAtLocation(SNOPower.Witchdoctor_AcidCloud,
                            ctx => ((CombatContext) ctx).TargetPosition,
                            ctx =>
                                AcidTimer.IsFinished &&
                                Clusters.GetClusterCount(
                                    ((CombatContext) ctx).CurrentTarget,
                                    ((CombatContext) ctx), 18f) >=
                                BelphegorSettings.Instance.WitchDoctor.
                                    AcidCloudAoECount,
                            s => AcidTimer.Reset()),
                        new CastAtLocation(SNOPower.Witchdoctor_Firebats,
                            ctx => ((CombatContext) ctx).TargetPosition,
                            extra =>
                                Clusters.GetClusterCount(
                                    ((CombatContext) extra).CurrentTarget,
                                    ((CombatContext) extra), 40f) >=
                                BelphegorSettings.Instance.WitchDoctor.
                                    FirebatsAoECount),
                        new CastAtLocation(SNOPower.Witchdoctor_WallOfZombies,
                            ctx => ((CombatContext) ctx).TargetPosition,
                            extra =>
                                Clusters.GetClusterCount(
                                    ((CombatContext) extra).CurrentTarget,
                                    ((CombatContext) extra), 40f) >=
                                BelphegorSettings.Instance.WitchDoctor.
                                    WallOfZombiesAoECount),
                        new CastOnUnit(SNOPower.Witchdoctor_Locust_Swarm,
                            ctx => ((CombatContext) ctx).TargetGuid,
                            ctx =>
                                LocustSwarmTimer.IsFinished &&
                                ((CombatContext) ctx).TargetDistance < 16 &&
                                ((CombatContext) ctx).CurrentTargetIsElite ||
                                Clusters.GetClusterCount(
                                    ((CombatContext) ctx).CurrentTarget,
                                    ((CombatContext) ctx), 20) >=
                                BelphegorSettings.Instance.WitchDoctor.
                                    LocustSwarmAoECount,
                            s => LocustSwarmTimer.Reset()),
                        new CastOnUnit(SNOPower.Witchdoctor_Haunt,
                            ctx => ((CombatContext) ctx).TargetGuid,
                            extra => HauntTimer.IsFinished, s => HauntTimer.Reset()),
                        //Other spells
                        new CastOnUnit(SNOPower.Witchdoctor_SpiritBarrage,
                            ctx => ((CombatContext) ctx).TargetGuid),
                        new CastOnUnit(SNOPower.Witchdoctor_ZombieCharger,
                            ctx => ((CombatContext) ctx).TargetGuid),
                        //Primary
                        new CastAtLocation(SNOPower.Witchdoctor_PlagueOfToads,
                            ctx => ((CombatContext) ctx).TargetPosition),
                        new CastAtLocation(SNOPower.Witchdoctor_CorpseSpider,
                            ctx => ((CombatContext) ctx).TargetPosition),
                        new CastOnUnit(SNOPower.Witchdoctor_Firebomb,
                            ctx => ((CombatContext) ctx).TargetGuid),
                        new CastOnUnit(SNOPower.Witchdoctor_PoisonDart,
                            ctx => ((CombatContext) ctx).TargetGuid)
                        )
                    ),
                new Action(ret => RunStatus.Success)
                );
        }

        [Class(ActorClass.Witchdoctor)]
        [Behavior(BehaviorType.Movement)]
        public static Composite WitchDoctorMovement()
        {
            return new PrioritySelector(
                Common.CreateWaitForAttack(),
                Common.CreateWaitForCast(),
                new SelfCast(SNOPower.Witchdoctor_SpiritWalk,
                    ctx => BelphegorSettings.Instance.WitchDoctor.UseSpiritWalkForMovement),
                new Action(ret =>
                {
                    ZetaDia.Me.Movement.MoveActor((Vector3) ret);
                    return RunStatus.Success;
                })
                );
        }

        public static void WitchDoctorOnLevelUp(object sender, EventArgs e)
        {
            if (ZetaDia.Me.ActorClass != ActorClass.Witchdoctor)
                return;

            if (!BelphegorSettings.Instance.EnableLevelUpSkilling) return;

            int myLevel = ZetaDia.Me.Level;

            Log.InfoFormat("Player leveled up, congrats! Your level is now: {0}",
                myLevel
                );

            // Set Lashing tail kick once we reach level 2
            if (myLevel == 2)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_GraspOfTheDead, -1, 1);
                Log.Info("Setting Grasp of the Dead as Secondary");
            }

            // Set Dead reach it's better then Fists of thunder imo.
            if (myLevel == 3)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_CorpseSpider, -1, 0);
                Log.Info("Setting Grasp of the Dead as Secondary");
            }

            // Make sure we set binding flash, useful spell in crowded situations!
            if (myLevel == 4)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_SummonZombieDog, -1, 2);
                Log.Info("Setting Summon Zombie Dogs as Defensive");
            }

            // Make sure we set Dashing strike, very cool and useful spell great opener.
            if (myLevel == 9)
            {
                ZetaDia.Me.SetActiveSkill(SNOPower.Witchdoctor_SoulHarvest, -1, 3);
                Log.Info("Setting Sould Harvest as Terror");
            }

            if (myLevel == 10)
            {
                ZetaDia.Me.SetTraits(SNOPower.Witchdoctor_Passive_JungleFortitude);
            }
            if (myLevel == 13)
            {
                ZetaDia.Me.SetTraits(SNOPower.Witchdoctor_Passive_SpiritualAttunement);
            }

            Hotbar.Update();
        }

        #region timmers

        private static readonly WaitTimer AcidTimer = new WaitTimer(TimeSpan.FromSeconds(5));
        private static readonly WaitTimer LocustSwarmTimer = new WaitTimer(TimeSpan.FromSeconds(6));
        private static readonly WaitTimer HauntTimer = new WaitTimer(TimeSpan.FromSeconds(10));

        static WitchDoctor()
        {
            GameEvents.OnGameLeft += OnGameLeft;
            GameEvents.OnPlayerDied += OnGameLeft;
        }


        private static void OnGameLeft(object sender, EventArgs e)
        {
            AcidTimer.Stop();
            LocustSwarmTimer.Stop();
            HauntTimer.Stop();
        }

        #endregion
    }
}