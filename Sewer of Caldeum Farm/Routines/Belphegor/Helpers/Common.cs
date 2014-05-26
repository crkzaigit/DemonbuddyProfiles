using System;
using System.Linq;
using Belphegor.Settings;
using log4net;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Belphegor.Helpers
{
    public static class Common
    {
        private static readonly WaitTimer PotionCooldownTimer = WaitTimer.ThirtySeconds;
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static AnimationState CurrentAnimationState
        {
            get
            {
                if (ZetaDia.Me.CommonData.AnimationInfo != null)
                    return ZetaDia.Me.CommonData.AnimationInfo.State;

                return AnimationState.Invalid;
            }
        }

        /// <summary>
        ///     Gets the best potion.
        /// </summary>
        /// <remarks>Created 2012-07-11</remarks>
        private static ACDItem HealthPotion
        {
            get
            {
                return
                    ZetaDia.Me.Inventory.Backpack.OrderByDescending(i => i.ItemQualityLevel)
                        .FirstOrDefault(i => i.ItemType == ItemType.Potion);
            }
        }

        private static DiaItem HealthGlobes
        {
            get
            {
                return
                    ZetaDia.Actors.GetActorsOfType<DiaItem>().Where(
                        i => i.IsValid &&
                             (i.ActorSNO == 4267 || i.ActorSNO == 85798) &&
                             i.Distance <= BelphegorSettings.Instance.HealthGlobeDistance).OrderBy
                        (i => i.Distance).FirstOrDefault();
            }
        }


        private static DiaItem PowerGlobe
        {
            get
            {
                //Type: Item Name: Console_PowerGlobe-1217 ActorSNO: 301283
                return
                    ZetaDia.Actors.GetActorsOfType<DiaItem>().FirstOrDefault(i => i.IsValid && i.ActorSNO == 301283);
            }
        }

        private static DiaObject PoolOfReflection
        {
            get
            {
                return
                    ZetaDia.Actors.GetActorsOfType<DiaItem>().Where(
                        i => i.IsValid &&
                             !Blacklist.Contains(i.ACDGuid) && i.ActorType == ActorType.Gizmo &&
                             i.ActorInfo.GizmoType == GizmoType.PoolOfReflection &&
                             i.Distance <= BelphegorSettings.Instance.PoolOfReflectionDistance)
                        .OrderBy(i => i.Distance)
                        .FirstOrDefault();
            }
        }

        private static DiaObject HealingWell
        {
            get
            {
                return
                    ZetaDia.Actors.GetActorsOfType<DiaItem>().Where(
                        i => i.IsValid &&
                             !Blacklist.Contains(i.ACDGuid) && i.ActorType == ActorType.Gizmo &&
                             i.ActorInfo.GizmoType == GizmoType.HealingWell &&
                             i.Distance <= BelphegorSettings.Instance.HealthWellDistance).OrderBy(i => i.Distance).
                        FirstOrDefault();
            }
        }

        public static Composite CreateWaitWhileRunningAttackingChannelingOrCasting()
        {
            return
                new Decorator(
                    ret =>
                        CurrentAnimationState.HasFlag((AnimationState) 15),
                    new Action(ret => RunStatus.Success)
                    );
        }


        /// <summary>
        ///     Creates  a behavior that makes sure the bot doesn't perform any actions in combat if the player is stunned
        /// </summary>
        /// <returns></returns>
        /// <remarks>Created 2012-07-29</remarks>
        public static Composite CreateWaitWhileFearedStunnedFrozenOrBlind()
        {
            return
                new Decorator(ret => ((CombatContext) ret).IsPlayerFearedStunnedFrozenOrBlind,
                    new Action(ret => RunStatus.Success)
                    );
        }

        /// <summary>
        ///     Creates  a behavior that makes sure the bot doesn't perform any actions in combat if the player is incapacitated
        /// </summary>
        /// <returns></returns>
        /// <remarks>Created 2012-07-11</remarks>
        public static Composite CreateWaitWhileIncapacitated()
        {
            return
                new Decorator(ret => ((CombatContext) ret).IsPlayerIncapacited,
                    new Action(ret => RunStatus.Success)
                    );
        }

        /// <summary>
        ///     Creates a behavior that makes sure the bot waits for the previous spell to finish casting.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Created 2012-07-11</remarks>
        public static Composite CreateWaitForAttack()
        {
            return
                new Decorator(ret => CurrentAnimationState == AnimationState.Attacking,
                    new Action(ret => RunStatus.Success)
                    );
        }

        /// <summary>
        ///     Creates a behavior that makes sure the bot waits for the previous spell to finish casting.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Created 2012-07-11</remarks>
        public static Composite CreateWaitForCast()
        {
            return
                new Decorator(ret => CurrentAnimationState == AnimationState.Casting,
                    new Action(ret => RunStatus.Success)
                    );
        }

        /// <summary>
        ///     Creates a behavior for using potions.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Created 2012-07-11</remarks>
        public static Composite CreateUsePotion()
        {
            return
                new Decorator(
                    ret =>
                        PotionCooldownTimer.IsFinished &&
                        ((CombatContext) ret).CurrentHealthPercentage <= BelphegorSettings.Instance.HealthPotionPct,
                    new PrioritySelector(ctx => HealthPotion,
                        new Decorator(ctx => ctx != null,
                            new Action(
                                ctx =>
                                {
                                    ZetaDia.Me.Inventory.UseItem(((ACDItem) ctx).DynamicId);
                                    PotionCooldownTimer.Reset();
                                    Log.InfoFormat(
                                        "Using {0}, Health is {1}/{2}", ((ACDItem) ctx).Name,
                                        100*BelphegorSettings.Instance.HealthPotionPct,
                                        Math.Round(100*ZetaDia.Me.HitpointsCurrentPct),
                                        ((ACDItem) ctx).Name);
                                    return RunStatus.Success;
                                })
                            )
                        )
                    );
        }

        public static Composite CreateGetHealthGlobe()
        {
            return
                new Decorator(
                    ret =>
                        BelphegorSettings.Instance.GetHealthGlobe &&
                        ZetaDia.Me.HitpointsCurrentPct <= BelphegorSettings.Instance.HealthGlobeHP,
                    new PrioritySelector(ctx => HealthGlobes,
                        new Decorator(ctx => (((DiaItem) ctx)) != null,
                            CommonBehaviors.MoveTo(ctx => ((DiaItem) ctx).Position, "Health Globe")
                            )));
        }

        public static Composite CreateGetPowerGlobe()
        {
            return
                new Decorator(
                    ret => BelphegorSettings.Instance.GetPowerGlobe,
                    new PrioritySelector(ctx => PowerGlobe,
                        new Decorator(ctx => (((DiaItem) ctx)) != null,
                            CommonBehaviors.MoveTo(ctx => ((DiaItem) ctx).Position, "Power Globe")
                            )));
        }

        public static Composite CreateUseHealthWell()
        {
            return
                new Decorator(
                    ret =>
                        BelphegorSettings.Instance.UseHealthWell &&
                        ZetaDia.Me.HitpointsCurrentPct <= BelphegorSettings.Instance.HealthWellHP,
                    new PrioritySelector(ctx => HealingWell,
                        new Decorator(ctx => ctx != null,
                            new PrioritySelector(
                                new Decorator(ctx => ((DiaObject) ctx).Distance > 14f,
                                    CommonBehaviors.MoveAndStop(
                                        ctx => ((DiaObject) ctx).Position, 14f)),
                                new Decorator(ctx => ((DiaObject) ctx).Distance <= 14f,
                                    new Action(ctx =>
                                    {
                                        Log.InfoFormat("Using {0}", ((DiaObject) ctx).Name);
                                        ((DiaObject) ctx).Interact();
                                        Blacklist.Add(
                                            ((DiaObject) ctx).
                                                ACDGuid,
                                            BlacklistFlags.All,
                                            TimeSpan.FromMinutes(10));
                                    }
                                        )
                                    )
                                )
                            )
                        )
                    );
        }

        public static Composite CreateUsePoolOfReflection()
        {
            return
                new Decorator(
                    ret =>
                        BelphegorSettings.Instance.UsePoolOfReflection,
                    new PrioritySelector(ctx => PoolOfReflection,
                        new Decorator(ctx => ctx != null,
                            new PrioritySelector(
                                new Decorator(ctx => ((DiaObject) ctx).Distance > 14f,
                                    CommonBehaviors.MoveAndStop(ctx => ((DiaObject) ctx).Position, 14f)),
                                new Decorator(ctx => ((DiaObject) ctx).Distance <= 14f,
                                    new Action(ctx =>
                                    {
                                        Log.InfoFormat("Using {0}", ((DiaObject) ctx).Name);
                                        ((DiaObject) ctx).Interact();
                                        Blacklist.Add(((DiaObject) ctx).ACDGuid, BlacklistFlags.All,
                                            TimeSpan.FromMinutes(10));
                                    }
                                        )
                                    )
                                )
                            )
                        )
                    );
        }
    }
}