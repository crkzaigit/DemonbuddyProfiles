using System;
using System.Collections.Generic;
using System.Linq;
using Armory.Settings;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Armory.Behaviors
{
    public class IdentifyBehavior
    {
        private const int identifyDelay = 7000;

        private static float startHealth = 0;
        private static DateTime lastIdentifyRequest = DateTime.MinValue;

        private static ACDItem UnIdentifiedItem
        {
            get
            {
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld || ZetaDia.Me == null)
                    return default(ACDItem);
                if (!ZetaDia.Me.IsValid)
                    return default(ACDItem);
                return ZetaDia.Me.Inventory.Backpack.FirstOrDefault(i => i.IsUnidentified);
            }
        }

        /// <summary>
        /// This will override a Treehook composite to identify items
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public static Composite CreateBehavior()
        {
            return
            new PrioritySelector(
                new Decorator(ret => ArmorySettings.Instance.IdentifyItems && ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Me != null && ZetaDia.Me.IsValid,
                    new PrioritySelector(
                        new Action(ret =>
                        {
                            if (ZetaDia.Me.LoopingAnimationEndTime > 0 && ZetaDia.Me.HitpointsCurrent >= startHealth)
                                return RunStatus.Running;
                            return RunStatus.Failure;
                        }),
                        new Decorator(ret => ZetaDia.Me.HitpointsCurrent < startHealth,
                            new Sequence(
                                new Action(ret => startHealth = ZetaDia.Me.HitpointsCurrent),
                                new Action(ret => RunStatus.Failure)
                            )
                        ),
                        new Decorator(ret => IdentifyDelayReady() && ZetaDia.Me.Inventory.Backpack.Any(i => i.IsUnidentified),
                            new Sequence(
                                new Action(ret => startHealth = ZetaDia.Me.HitpointsCurrent),
                                new Action(ret => Logger.Log("Identifying item {0}", UnIdentifiedItem.Name)),
                                new Action(ret => ZetaDia.Me.Inventory.IdentifyItem(UnIdentifiedItem.DynamicId)),
                                new Action(ret => lastIdentifyRequest = DateTime.UtcNow),
                                new Sleep(500),
                                new Action(ret =>
                                {
                                    if ((DateTime.UtcNow.Subtract(lastIdentifyRequest).TotalMilliseconds < 500 || ZetaDia.Me.LoopingAnimationEndTime > 0) && ZetaDia.Me.HitpointsCurrent >= startHealth)
                                    {
                                        //Logger.Log("Identify running...");
                                        return RunStatus.Running;
                                    }
                                    else
                                    {
                                        //Logger.Log("Identify wait conditions not met; lastIdentify:{0} looping:{1} ");
                                        return RunStatus.Success;
                                    }
                                })
                            )
                        )
                    )
                )
            );
        }

        private static bool IdentifyDelayReady()
        {
            return DateTime.UtcNow.Subtract(lastIdentifyRequest).TotalMilliseconds > identifyDelay;
        }
    }
}
