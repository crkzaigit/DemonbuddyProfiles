﻿using System;
using System.Linq;
using Trinity.Technicals;
using Zeta.Bot.Logic;
using Zeta.Game;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.Helpers
{
    public class Composites
    {
        private const int WaitForCacheDropDelay = 1500;
        private const int MultiOpenPauseDelay = 250;

        public static Composite CreateLootBehavior(Composite child)
        {
            return
            new PrioritySelector(
                CreateUseHoradricCache(),
                child
            );
        }

        private static DateTime _lastCheckedForHoradricCache = DateTime.MinValue;
        private static DateTime _lastFoundHoradricCache = DateTime.MinValue;

        public static DateTime LastCheckedForHoradricCache
        {
            get { return _lastCheckedForHoradricCache; }
            set { _lastCheckedForHoradricCache = value; }
        }

        public static DateTime LastFoundHoradricCache
        {
            get { return _lastFoundHoradricCache; }
            set { _lastFoundHoradricCache = value; }
        }

        public static Composite CreateUseHoradricCache()
        {
            return
            new Decorator(ret => Trinity.Settings.Loot.TownRun.OpenHoradricCaches && !BrainBehavior.IsVendoring && !Trinity.ForceVendorRunASAP && !TownRun.IsTryingToTownPortal() &&
                    DateTime.UtcNow.Subtract(LastCheckedForHoradricCache).TotalSeconds > 1,
                new Sequence(
                    new Action(ret => LastCheckedForHoradricCache = DateTime.UtcNow),
                    new Decorator(ret => HasHoradricCaches(),
                        new Action(ret => OpenHoradricCache())
                    )
                )
            );

        }

        internal static RunStatus OpenHoradricCache()
        {
            if (DateTime.UtcNow.Subtract(LastFoundHoradricCache).TotalMilliseconds < MultiOpenPauseDelay)
            {
                // Pause between opening caches
                return RunStatus.Running;
            }

            if (HasHoradricCaches())
            {
                var item = ZetaDia.Me.Inventory.Backpack.FirstOrDefault(i => i.InternalName.StartsWith(Items.ItemIds.HoradricCache));
                ZetaDia.Me.Inventory.UseItem(item.DynamicId);
                LastFoundHoradricCache = DateTime.UtcNow;
                Trinity.TotalBountyCachesOpened++;
                return RunStatus.Running;
            }

            if (DateTime.UtcNow.Subtract(LastFoundHoradricCache).TotalMilliseconds < WaitForCacheDropDelay)
            {
                Logger.Log("Waiting for Horadric Cache drops");
                return RunStatus.Running;
            }

            return RunStatus.Success;

        }

        internal static bool HasHoradricCaches()
        {
            return ZetaDia.Me.Inventory.Backpack.Any(i => i.InternalName.StartsWith(Items.ItemIds.HoradricCache));
        }

    }
}
