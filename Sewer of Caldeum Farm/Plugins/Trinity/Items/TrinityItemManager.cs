﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Demonbuddy;
using Trinity.Config.Loot;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Items;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public class TrinityItemManager : ItemManager
    {
        private RuleTypePriority _priority;

        public override RuleTypePriority Priority
        {
            get
            {
                if (_priority == null)
                {
                    _priority = new RuleTypePriority
                    {
                        Priority1 = ItemEvaluationType.Keep,
                        Priority2 = ItemEvaluationType.Salvage,
                        Priority3 = ItemEvaluationType.Sell
                    };
                }
                return _priority;
            }
        }

        public override bool EvaluateItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            if (Trinity.Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy)
            {
                Current.EvaluateItem(item, evaluationType);
            }
            else
            {
                switch (evaluationType)
                {
                    case ItemEvaluationType.Keep:
                        return ShouldStashItem(item);
                    case ItemEvaluationType.Salvage:
                        return ShouldSalvageItem(item);
                    case ItemEvaluationType.Sell:
                        return ShouldSellItem(item);
                }
            }
            return false;
        }

        public override bool ShouldSalvageItem(ACDItem item)
        {
            return ShouldSalvageItem(item, ItemEvaluationType.Salvage);
        }

        public bool ShouldSalvageItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            if (Current.ItemIsProtected(item))
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
            {
                return Current.ShouldSalvageItem(item);
            }
            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                return ItemRulesSalvageSell(item, evaluationType);
            }
            return TrinitySalvage(item);
        }

        public override bool ShouldSellItem(ACDItem item)
        {
            return ShouldSellItem(item, ItemEvaluationType.Sell);
        }

        public bool ShouldSellItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            if (Current.ItemIsProtected(cItem.AcdItem))
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
            {
                return Current.ShouldSellItem(item);
            }
            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                return ItemRulesSalvageSell(item, evaluationType);
            }
            return TrinitySell(item);
        }

        public override bool ShouldStashItem(ACDItem item)
        {
            return ShouldStashItem(item, ItemEvaluationType.Keep);
        }

        public bool ShouldStashItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            if (Current.ItemIsProtected(item))
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
            {
                return ItemManager.Current.ShouldStashItem(item);
            }

            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Now look for Misc items we might want to keep
            GItemType tItemType = cItem.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType tBaseType = cItem.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

            bool isEquipment = (tBaseType == GItemBaseType.Armor ||
                tBaseType == GItemBaseType.Jewelry ||
                tBaseType == GItemBaseType.Offhand ||
                tBaseType == GItemBaseType.WeaponOneHand ||
                tBaseType == GItemBaseType.WeaponRange ||
                tBaseType == GItemBaseType.WeaponTwoHand);

            if (cItem.TrinityItemType == GItemType.HoradricCache && Trinity.Settings.Loot.TownRun.OpenHoradricCaches)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (ignoring Horadric Cache)", cItem.RealName, cItem.InternalName);
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", cItem.RealName, cItem.InternalName);
                return true;
            }
            if (tItemType == GItemType.StaffOfHerding)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.CraftingMaterial)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == GItemType.Emerald || tItemType == GItemType.Amethyst || tItemType == GItemType.Topaz || tItemType == GItemType.Ruby || tItemType == GItemType.Diamond)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.CraftTome)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.InfernalKey)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.HealthPotion)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            if (tItemType == GItemType.CraftingPlan && cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendary plans)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, evaluationType);

                if (evaluationType == ItemEvaluationType.Keep)

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (" + action + ")", cItem.AcdItem.Name, cItem.AcdItem.InternalName, cItem.AcdItem.ItemType);
                switch (action)
                {
                    case Interpreter.InterpreterAction.KEEP:
                        return true;
                    case Interpreter.InterpreterAction.TRASH:
                        return false;
                    case Interpreter.InterpreterAction.SCORE:
                        break;
                }
            }

            if (tItemType == GItemType.CraftingPlan)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            // Stashing Whites, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashWhites && isEquipment && cItem.Quality <= ItemQuality.Superior)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing whites)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            // Else auto-trash
            if (cItem.Quality <= ItemQuality.Superior && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trash whites)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            // Stashing blues, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashBlues && isEquipment && cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing blues)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            // Else auto trash
            if (cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3 && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trashing blues)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            // Force salvage Rares
            if (Trinity.Settings.Loot.TownRun.ForceSalvageRares && cItem.Quality >= ItemQuality.Rare4 && cItem.Quality <= ItemQuality.Rare6 && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (force salvage rare)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            if (cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = Trinity.ScoreNeeded(item.ItemBaseType);
            double iMyScore = ItemValuation.ValueThisItem(cItem, tItemType);

            if (evaluationType == ItemEvaluationType.Keep)
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", cItem.RealName, cItem.InternalName, tItemType, iMyScore);
            if (iMyScore >= iNeedScore)
                return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        private bool ItemRulesSalvageSell(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();
            if (!item.IsPotion)
                Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation,
                    "Incoming {0} Request: {1}, {2}, {3}, {4}, {5}",
                    evaluationType, item.ItemQualityLevel, item.Level, item.ItemBaseType,
                    item.ItemType, item.IsOneHand ? "1H" : item.IsTwoHand ? "2H" : "NH");

            Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, ItemEvaluationType.Salvage);
            switch (action)
            {
                case Interpreter.InterpreterAction.SALVAGE:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0}: {1}", evaluationType, (evaluationType == ItemEvaluationType.Salvage));
                    return (evaluationType == ItemEvaluationType.Salvage);
                case Interpreter.InterpreterAction.SELL:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0}: {1}", evaluationType, (evaluationType == ItemEvaluationType.Sell));
                    return (evaluationType == ItemEvaluationType.Sell);
                default:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ScriptRule, "Trinity, item is unhandled by ItemRules (SalvageSell)!");
                    switch (evaluationType)
                    {
                        case ItemEvaluationType.Salvage:
                            return TrinitySalvage(item);
                        default:
                            return TrinitySell(item);
                    }
            }
        }

        private bool TrinitySalvage(ACDItem item)
        {
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            if (cItem.AcdItem.IsVendorBought)
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            GItemType trinityItemType = Trinity.DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityItemBaseType = Trinity.DetermineBaseType(trinityItemType);

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(cItem.Quality);

            // Stashing Whites
            if (Trinity.Settings.Loot.TownRun.StashWhites && cItem.Quality < ItemQuality.Magic1)
                return false;

            // Stashing Blues
            if (Trinity.Settings.Loot.TownRun.StashBlues && cItem.Quality > ItemQuality.Superior && cItem.Quality < ItemQuality.Rare4)
                return false;

            if (cItem.Quality >= ItemQuality.Legendary && salvageOption == SalvageOption.InfernoOnly && cItem.Level >= 60)
                return true;

            switch (trinityItemBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return ((cItem.Level >= 61 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.Jewelry:
                    return ((cItem.Level >= 59 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.FollowerItem:
                    return ((cItem.Level >= 60 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                case GItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }
        }

        private bool TrinitySell(ACDItem item)
        {
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            switch (cItem.TrinityItemBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                case GItemBaseType.Jewelry:
                case GItemBaseType.FollowerItem:
                    return true;
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                    if (cItem.TrinityItemType == GItemType.CraftingPlan)
                        return true;
                    return false;
                case GItemBaseType.Unknown:
                    return false;
            }

            return false;
        }

        private SalvageOption GetSalvageOption(ItemQuality quality)
        {
            if (quality < ItemQuality.Magic1)
            {
                return SalvageOption.All;
            }

            if (quality >= ItemQuality.Magic1 && quality <= ItemQuality.Magic3)
            {
                return Trinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }
            if (quality >= ItemQuality.Rare4 && quality <= ItemQuality.Rare6)
            {
                return Trinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }
            if (quality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.None;
        }

        public enum DumpItemLocation
        {
            All,
            Equipped,
            Backpack,
            Ground,
            Stash,
            Merchant,
        }

        public static void DumpQuickItems()
        {
            List<ACDItem> itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
            StringBuilder sbTopList = new StringBuilder();
            foreach (var item in itemList)
            {
                sbTopList.AppendFormat("\nName={0} InternalName={1} ActorSNO={2} DynamicID={3} InventorySlot={4}",
                    item.Name, item.InternalName, item.ActorSNO, item.DynamicId, item.InventorySlot);
            }
            Logger.Log(sbTopList.ToString());
        }

#pragma warning disable 1718
        public static void DumpItems(DumpItemLocation location)
        {
            ZetaDia.Actors.Update();
            using (ZetaDia.Memory.SaveCacheState())
            {
                ZetaDia.Memory.TemporaryCacheState(false);

                List<ACDItem> itemList = new List<ACDItem>();

                switch (location)
                {
                    case DumpItemLocation.All:
                        itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
                        break;
                    case DumpItemLocation.Backpack:
                        itemList = ZetaDia.Me.Inventory.Backpack.ToList();
                        break;
                    case DumpItemLocation.Merchant:
                        itemList = ZetaDia.Me.Inventory.MerchantItems.ToList();
                        break;
                    case DumpItemLocation.Ground:
                        itemList = ZetaDia.Actors.GetActorsOfType<DiaItem>(true, false).Select(i => i.CommonData).ToList();
                        break;
                    case DumpItemLocation.Equipped:
                        itemList = ZetaDia.Me.Inventory.Equipped.ToList();
                        break;
                    case DumpItemLocation.Stash:
                        if (UIElements.StashWindow.IsVisible)
                        {
                            itemList = ZetaDia.Me.Inventory.StashItems.ToList();
                        }
                        else
                        {
                            Logger.Log("Stash window not open!");
                        }
                        break;
                }


                foreach (var item in itemList.OrderBy(i => i.InventorySlot).ThenBy(i => i.Name))
                {
                    try
                    {
                        string itemName = string.Format("\nName={0} InternalName={1} ActorSNO={2} DynamicID={3} InventorySlot={4}",
                        item.Name, item.InternalName, item.ActorSNO, item.DynamicId, item.InventorySlot);

                        Logger.Log(itemName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Basic Item Info\n{0}", ex.ToString());
                    }
                    try
                    {
                        foreach (object val in Enum.GetValues(typeof(ActorAttributeType)))
                        {
                            int iVal = item.GetAttribute<int>((ActorAttributeType)val);
                            float fVal = item.GetAttribute<float>((ActorAttributeType)val);

                            if (iVal > 0 || fVal > 0)
                                Logger.Log("Attribute: {0}, iVal: {1}, fVal: {2}", val, iVal, (fVal != fVal) ? "" : fVal.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading attributes for {0}\n{1}", item.Name, ex.ToString());
                    }

                    try
                    {
                        foreach (var stat in Enum.GetValues(typeof(ItemStats.Stat)).Cast<ItemStats.Stat>())
                        {
                            float fStatVal = item.Stats.GetStat<float>(stat);
                            int iStatVal = item.Stats.GetStat<int>(stat);
                            if (fStatVal > 0 || iStatVal > 0)
                                Logger.Log("Stat {0}={1}f ({2})", stat, fStatVal, iStatVal);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Stats\n{0}", ex.ToString());
                    }

                    try
                    {
                        Logger.Log("Link Color ItemQuality=" + item.ItemLinkColorQuality());
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Link\n{0}", ex.ToString());
                    }

                    try
                    {
                        PrintObjectProperties<ACDItem>(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Properties\n{0}", ex.ToString());
                    }

                }
            }

        }

        private static void PrintObjectProperties<T>(T item)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties.ToList().OrderBy(p => p.Name))
            {
                try
                {
                    object val = property.GetValue(item, null);
                    if (val != null)
                    {
                        Logger.Log(typeof(T).Name + "." + property.Name + "=" + val.ToString());

                        // Special cases!
                        if (property.Name == "ValidInventorySlots")
                        {
                            foreach (var slot in ((InventorySlot[])val))
                            {
                                Logger.Log(slot.ToString());
                            }
                        }
                    }
                }
                catch
                {
                    Logger.Log("Exception reading {0} from object", property.Name);
                }
            }
        }


        private static int _lastBackPackCount;
        private static int _lastProtectedSlotsCount;
        private static Vector2 _lastBackPackLocation = new Vector2(-2, -2);

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        /// <param name="isOriginalTwoSlot"></param>
        /// <returns></returns>
        internal static Vector2 FindValidBackpackLocation(bool isOriginalTwoSlot)
        {
            using (new PerformanceLogger("FindValidBackpackLocation"))
            {
                try
                {
                    if (_lastBackPackLocation != new Vector2(-2, -2) &&
                        _lastBackPackCount == ZetaDia.Me.Inventory.Backpack.Count(i => i.IsValid) &&
                        _lastProtectedSlotsCount == CharacterSettings.Instance.ProtectedBagSlots.Count)
                    {
                        return _lastBackPackLocation;
                    }

                    bool[,] BackpackSlotBlocked = new bool[10, 6];

                    int freeBagSlots = 60;

                    _lastProtectedSlotsCount = CharacterSettings.Instance.ProtectedBagSlots.Count;
                    _lastBackPackCount = ZetaDia.Me.Inventory.Backpack.Count(i => i.IsValid);

                    // Block off the entire of any "protected bag slots"
                    foreach (InventorySquare square in CharacterSettings.Instance.ProtectedBagSlots)
                    {
                        BackpackSlotBlocked[square.Column, square.Row] = true;
                        freeBagSlots--;
                    }

                    // Map out all the items already in the backpack
                    foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                    {
                        if (!item.IsValid)
                            continue;

                        int row = item.InventoryRow;
                        int col = item.InventoryColumn;

                        // Slot is already protected, don't double count
                        if (!BackpackSlotBlocked[col, row])
                        {
                            BackpackSlotBlocked[col, row] = true;
                            freeBagSlots--;
                        }

                        if (!item.IsTwoSquareItem)
                            continue;

                        // Slot is already protected, don't double count
                        if (BackpackSlotBlocked[col, row + 1])
                            continue;

                        freeBagSlots--;
                        BackpackSlotBlocked[col, row + 1] = true;
                    }

                    bool noFreeSlots = freeBagSlots < 1;
                    int unprotectedSlots = 60 - _lastProtectedSlotsCount;

                    // Use count of Unprotected slots if FreeBagSlots is higher than unprotected slots
                    int minFreeSlots = Trinity.Player.IsInTown ?
                        Math.Min(Trinity.Settings.Loot.TownRun.FreeBagSlotsInTown, unprotectedSlots) :
                        Math.Min(Trinity.Settings.Loot.TownRun.FreeBagSlots, unprotectedSlots);

                    // free bag slots is less than required
                    if (noFreeSlots || freeBagSlots < minFreeSlots)
                    {
                        Logger.LogDebug("Free Bag Slots is less than required. FreeSlots={0}, FreeBagSlots={1} FreeBagSlotsInTown={2} IsInTown={3} Protected={4} BackpackCount={5}",
                            freeBagSlots, Trinity.Settings.Loot.TownRun.FreeBagSlots, Trinity.Settings.Loot.TownRun.FreeBagSlotsInTown, Trinity.Player.IsInTown,
                            _lastProtectedSlotsCount, _lastBackPackCount);
                        _lastBackPackLocation = new Vector2(-1, -1);
                        return _lastBackPackLocation;
                    }
                    // 10 columns
                    for (int col = 0; col <= 9; col++)
                    {
                        // 6 rows
                        for (int row = 0; row <= 5; row++)
                        {
                            // Slot is blocked, skip
                            if (BackpackSlotBlocked[col, row])
                                continue;

                            // Not a two slotitem, slot not blocked, use it!
                            if (!isOriginalTwoSlot)
                            {
                                _lastBackPackLocation = new Vector2(col, row);
                                return _lastBackPackLocation;
                            }

                            // Is a Two Slot, Can't check for 2 slot items on last row
                            if (row == 5)
                                continue;

                            // Is a Two Slot, check row below
                            if (BackpackSlotBlocked[col, row + 1])
                                continue;

                            _lastBackPackLocation = new Vector2(col, row);
                            return _lastBackPackLocation;
                        }
                    }

                    // no free slot
                    Logger.LogDebug("No Free slots!");
                    _lastBackPackLocation = new Vector2(-1, -1);
                    return _lastBackPackLocation;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogCategory.UserInformation, "Error in finding backpack slot");
                    Logger.Log(LogCategory.UserInformation, "{0}", ex.ToString());
                    return new Vector2(1, 1);
                }
            }
        }

    }
}