﻿using System;
using System.Linq;
using System.Threading;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.DbProvider
{
    public static class DeathHandler
    {
        [Obsolete("Demonbuddy handles this kind of behavior now, this class is no longer needed")]
        public static Composite CreateDeathHandler(Composite original)
        {
            return
                new PrioritySelector(
                    new Action(ret => TrinityDeathSafetyCheck()),
                    original
                );
        }

        public static RunStatus TrinityDeathSafetyCheck()
        {
            if (!ZetaDia.Me.IsDead)
                return RunStatus.Failure;

            if (ZetaDia.IsInTown)
                return RunStatus.Failure;

            // Items with "Ignore Durability Loss" should not be considered, or if all items are at 100%
            var equippedItems = ZetaDia.Me.Inventory.Equipped.Where(i => i.DurabilityCurrent != i.DurabilityMax);
            if (!equippedItems.Any())
                return RunStatus.Failure;

            double min = equippedItems.Min(i => i.DurabilityPercent);
            double avg = equippedItems.Average(i => i.DurabilityPercent);
            double max = equippedItems.Max(i => i.DurabilityPercent);
            Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "We died, durability is min/avg/max: {0:0.00}/{1:0.00}/{2:0.00}", min, avg, max);

            // We keep dying because we're spawning in AoE and next to 50 elites and we need to just leave the game
            if (max <= 1)
            {
                Logger.Log("Durability is zero, emergency leave game");
                ZetaDia.Service.Party.LeaveGame(true);
                Thread.Sleep(11000);
                return RunStatus.Success;
            }
            else
            {
                return RunStatus.Failure;
            }
        }

        public static bool EquipmentNeedsEmergencyRepair()
        {
            var equippedItems = ZetaDia.Me.Inventory.Equipped.Where(i => i.DurabilityCurrent < i.DurabilityMax);

            if (!equippedItems.Any())
                return false;

            double max = equippedItems.Max(i => i.DurabilityPercent);

            //Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "EquipmentNeedsEmergencyRepair={0}", max <= 1);
            return max <= 1;
        }

    }
}
