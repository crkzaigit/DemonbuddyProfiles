﻿using System;
using System.IO;
using Trinity.Cache;
using Trinity.Config.Loot;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class Trinity
    {
        private static bool RefreshItem()
        {
            using (new PerformanceLogger("RefreshItem"))
            {
                bool logNewItem;
                bool AddToCache;

                var diaItem = c_diaObject as DiaItem;

                if (diaItem == null)
                    return false;

                if (diaItem.CommonData == null)
                    return false;

                if (!diaItem.IsValid)
                    return false;

                if (!diaItem.CommonData.IsValid)
                    return false;

                GItemBaseType itemBaseType;
                c_ItemQuality = diaItem.CommonData.ItemQualityLevel;
                ((DiaItem)c_diaObject).CommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevelIdentified);
                c_ItemDisplayName = diaItem.CommonData.Name;

                CurrentCacheObject.DynamicID = CurrentCacheObject.CommonData.DynamicId;
                CurrentCacheObject.GameBalanceID = CurrentCacheObject.CommonData.GameBalanceId;

                c_ItemLevel = diaItem.CommonData.Level;
                c_DBItemBaseType = diaItem.CommonData.ItemBaseType;
                c_DBItemType = diaItem.CommonData.ItemType;
                c_IsOneHandedItem = diaItem.CommonData.IsOneHand;
                c_IsTwoHandedItem = diaItem.CommonData.IsTwoHand;
                c_item_tFollowerType = diaItem.CommonData.FollowerSpecialType;
                // Calculate item type
                c_item_GItemType = DetermineItemType(CurrentCacheObject.InternalName, c_DBItemType, c_item_tFollowerType);

                // And temporarily store the base type
                itemBaseType = DetermineBaseType(c_item_GItemType);

                // Compute item quality from item link for Crafting Plans (Blacksmith or Jewler)
                if (itemBaseType == GItemBaseType.Misc || itemBaseType == GItemBaseType.Unknown)
                {
                    if (!CacheData.ItemLinkQuality.TryGetValue(CurrentCacheObject.ACDGuid, out c_ItemQuality))
                    {
                        c_ItemQuality = diaItem.CommonData.ItemLinkColorQuality();
                        CacheData.ItemLinkQuality.Add(CurrentCacheObject.ACDGuid, c_ItemQuality);
                    }
                }

                if (itemBaseType == GItemBaseType.Gem)
                    c_ItemLevel = diaItem.CommonData.GemQuality;

                CurrentCacheObject.ObjectHash = HashGenerator.GenerateItemHash(
                    CurrentCacheObject.Position,
                    CurrentCacheObject.ActorSNO,
                    CurrentCacheObject.InternalName,
                    Player.WorldID,
                    c_ItemQuality,
                    c_ItemLevel);

                float range = 0f;

                // no range check on Legendaries
                if (c_ItemQuality < ItemQuality.Legendary)
                {
                    if (c_ItemQuality >= ItemQuality.Rare4)
                        range = CurrentBotLootRange;

                    if (_keepLootRadiusExtendedForSeconds > 0)
                        range += 90f;

                    if (CurrentCacheObject.Distance > (CurrentBotLootRange + range))
                    {
                        c_IgnoreSubStep = "OutOfRange";
                        AddToCache = false;
                        // return here to save CPU on reading unncessary attributes for out of range items;
                        if (!AddToCache)
                            return AddToCache;
                    }
                }

                float damage, toughness, healing = 0;
                bool isUpgrade = false;

                diaItem.CommonData.GetStatChanges(out damage, out healing, out toughness);

                if (damage > 0 && toughness > 0)
                    isUpgrade = true;

                var pickupItem = new PickupItem
                {
                    Name = c_ItemDisplayName,
                    InternalName = CurrentCacheObject.InternalName,
                    Level = c_ItemLevel,
                    Quality = c_ItemQuality,
                    BalanceID = CurrentCacheObject.GameBalanceID,
                    DBBaseType = c_DBItemBaseType,
                    DBItemType = c_DBItemType,
                    TBaseType = itemBaseType,
                    TType = c_item_GItemType,
                    IsOneHand = c_IsOneHandedItem,
                    IsTwoHand = c_IsTwoHandedItem,
                    ItemFollowerType = c_item_tFollowerType,
                    DynamicID = CurrentCacheObject.DynamicID,
                    Position = CurrentCacheObject.Position,
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    ACDGuid = CurrentCacheObject.ACDGuid,
                    RActorGUID = CurrentCacheObject.RActorGuid,
                    IsUpgrade = isUpgrade,
                    UpgradeDamage = damage,
                    UpgradeToughness = toughness,
                    UpgradeHealing = healing
                };

                // Treat all globes as a yes
                if (c_item_GItemType == GItemType.HealthGlobe)
                {
                    CurrentCacheObject.Type = GObjectType.HealthGlobe;
                    AddToCache = true;
                    return AddToCache;
                }

                // Treat all globes as a yes
                if (c_item_GItemType == GItemType.PowerGlobe)
                {
                    CurrentCacheObject.Type = GObjectType.PowerGlobe;
                    AddToCache = true;
                    return AddToCache;
                }

                // Item stats
                logNewItem = RefreshItemStats(itemBaseType);

                // Get whether or not we want this item, cached if possible
                if (!CacheData.PickupItem.TryGetValue(CurrentCacheObject.RActorGuid, out AddToCache))
                {
                    if (pickupItem.IsTwoHand && Settings.Loot.Pickup.IgnoreTwoHandedWeapons && c_ItemQuality < ItemQuality.Legendary)
                    {
                        AddToCache = false;
                    }
                    else if (Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
                    {
                        AddToCache = ItemManager.Current.ShouldPickUpItem((ACDItem)CurrentCacheObject.CommonData);
                    }
                    else if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
                    {
                        AddToCache = ItemRulesPickupValidation(pickupItem);
                    }
                    else // Trinity Scoring Only
                    {
                        AddToCache = PickupItemValidation(pickupItem);
                    }

                    // Pickup low level enabled, and we're a low level
                    if (!AddToCache && Settings.Loot.Pickup.PickupLowLevel && Player.Level <= 10)
                    {
                        AddToCache = PickupItemValidation(pickupItem);
                    }

                    CacheData.PickupItem.Add(CurrentCacheObject.RActorGuid, AddToCache);
                }

                // Using DB built-in item rules
                if (AddToCache && ForceVendorRunASAP)
                    c_IgnoreSubStep = "ForcedVendoring";

                // Didn't pass pickup rules, so ignore it
                if (!AddToCache && c_IgnoreSubStep == String.Empty)
                    c_IgnoreSubStep = "NoMatchingRule";

                if (Settings.Advanced.LogDroppedItems && logNewItem && c_item_GItemType != GItemType.HealthGlobe && c_item_GItemType != GItemType.HealthPotion && c_item_GItemType != GItemType.PowerGlobe)
                    //LogDroppedItem();
                    ItemDroppedAppender.Instance.AppendDroppedItem(pickupItem);

                return AddToCache;
            }
        }

        private static bool RefreshGold(bool AddToCache)
        {
            AddToCache = true;

            if (!Settings.Loot.Pickup.PickupGold)
            {
                c_IgnoreSubStep = "PickupDisabled";
                AddToCache = false;
                return AddToCache;
            }

            if (Player.ActorClass == ActorClass.Barbarian && Settings.Combat.Barbarian.IgnoreGoldInWOTB && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                AddToCache = false;
                c_IgnoreSubStep = "IgnoreGoldInWOTB";
                return AddToCache;
            }

            // Get the gold amount of this pile, cached if possible
            if (!CacheData.GoldStack.TryGetValue(CurrentCacheObject.RActorGuid, out c_GoldStackSize))
            {
                try
                {
                    c_GoldStackSize = ((ACDItem)CurrentCacheObject.CommonData).Gold;
                }
                catch
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting gold pile amount for item {0} [{1}]", CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO);
                    AddToCache = false;
                    c_IgnoreSubStep = "GetAttributeException";
                }
                CacheData.GoldStack.Add(CurrentCacheObject.RActorGuid, c_GoldStackSize);
            }

            if (c_GoldStackSize < Settings.Loot.Pickup.MinimumGoldStack)
            {
                AddToCache = false;
                c_IgnoreSubStep = "NotEnoughGold";
                return AddToCache;
            }

            if (CurrentCacheObject.Distance <= Player.GoldPickupRadius)
            {
                AddToCache = false;
                c_IgnoreSubStep = "WithinPickupRadius";
                return AddToCache;
            }
            return AddToCache;
        }
        private static bool RefreshItemStats(GItemBaseType tempbasetype)
        {
            bool isNewLogItem = false;

            c_ItemMd5Hash = HashGenerator.GenerateItemHash(CurrentCacheObject.Position, CurrentCacheObject.ActorSNO, CurrentCacheObject.InternalName, CurrentWorldDynamicId, c_ItemQuality, c_ItemLevel);

            if (!GenericCache.ContainsKey(c_ItemMd5Hash))
            {
                GenericCache.AddToCache(new GenericCacheObject(c_ItemMd5Hash, null, new TimeSpan(1, 0, 0)));

                try
                {
                    isNewLogItem = true;
                    if (tempbasetype == GItemBaseType.Armor || tempbasetype == GItemBaseType.WeaponOneHand || tempbasetype == GItemBaseType.WeaponTwoHand ||
                        tempbasetype == GItemBaseType.WeaponRange || tempbasetype == GItemBaseType.Jewelry || tempbasetype == GItemBaseType.FollowerItem ||
                        tempbasetype == GItemBaseType.Offhand)
                    {
                        try
                        {
                            int iThisQuality;
                            ItemsDroppedStats.Total++;
                            if (c_ItemQuality >= ItemQuality.Legendary)
                                iThisQuality = QUALITYORANGE;
                            else if (c_ItemQuality >= ItemQuality.Rare4)
                                iThisQuality = QUALITYYELLOW;
                            else if (c_ItemQuality >= ItemQuality.Magic1)
                                iThisQuality = QUALITYBLUE;
                            else
                                iThisQuality = QUALITYWHITE;
                            ItemsDroppedStats.TotalPerQuality[iThisQuality]++;
                            ItemsDroppedStats.TotalPerLevel[c_ItemLevel]++;
                            ItemsDroppedStats.TotalPerQPerL[iThisQuality, c_ItemLevel]++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error Refreshing Item Stats for Equippable Item: " + ex.ToString());
                        }
                    }
                    else if (tempbasetype == GItemBaseType.Gem)
                    {
                        try
                        {
                            int iThisGemType = 0;
                            ItemsDroppedStats.TotalGems++;
                            if (c_item_GItemType == GItemType.Topaz)
                                iThisGemType = GEMTOPAZ;
                            if (c_item_GItemType == GItemType.Ruby)
                                iThisGemType = GEMRUBY;
                            if (c_item_GItemType == GItemType.Emerald)
                                iThisGemType = GEMEMERALD;
                            if (c_item_GItemType == GItemType.Amethyst)
                                iThisGemType = GEMAMETHYST;
                            if (c_item_GItemType == GItemType.Diamond)
                                iThisGemType = GEMDIAMOND;
                            ItemsDroppedStats.GemsPerType[iThisGemType]++;
                            ItemsDroppedStats.GemsPerLevel[c_ItemLevel]++;
                            ItemsDroppedStats.GemsPerTPerL[iThisGemType, c_ItemLevel]++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error refreshing item stats for Gem: " + ex.ToString());
                        }
                    }
                    else if (c_item_GItemType == GItemType.InfernalKey)
                    {
                        try
                        {
                            ItemsDroppedStats.TotalInfernalKeys++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error refreshing item stats for InfernalKey: " + ex.ToString());
                        }
                    }
                    // See if we should update the stats file
                    if (DateTime.UtcNow.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                    {
                        try
                        {
                            ItemStatsLastPostedReport = DateTime.UtcNow;
                            OutputReport();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error Calling OutputReport from RefreshItemStats " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Couldn't do Item Stats" + ex.ToString());
                }
            }
            return isNewLogItem;
        }

        private static void LogSkippedGold()
        {
            string skippedItemsPath = Path.Combine(FileManager.LoggingPath, String.Format("SkippedGoldStacks_{0}_{1}.csv", Player.ActorClass, DateTime.UtcNow.ToString("yyyy-MM-dd")));

            bool writeHeader = !File.Exists(skippedItemsPath);
            using (var LogWriter = new StreamWriter(skippedItemsPath, true))
            {
                if (writeHeader)
                {
                    LogWriter.WriteLine("ActorSNO,RActorGUID,DyanmicID,ACDGuid,Name,GoldStackSize,IgnoreItemSubStep,Distance");
                }
                LogWriter.Write(FormatCSVField(CurrentCacheObject.ActorSNO));
                LogWriter.Write(FormatCSVField(CurrentCacheObject.RActorGuid));
                LogWriter.Write(FormatCSVField(CurrentCacheObject.DynamicID));
                LogWriter.Write(FormatCSVField(CurrentCacheObject.ACDGuid));
                LogWriter.Write(FormatCSVField(CurrentCacheObject.InternalName));
                LogWriter.Write(FormatCSVField(c_GoldStackSize));
                LogWriter.Write(FormatCSVField(c_IgnoreSubStep));
                LogWriter.Write(FormatCSVField(CurrentCacheObject.Distance));
                LogWriter.Write("\n");
            }
        }

        private static string FormatCSVField(DateTime time)
        {
            return String.Format("\"{0:yyyy-MM-ddTHH:mm:ss.ffff}\",", time.ToLocalTime());
        }

        private static string FormatCSVField(string text)
        {
            return String.Format("\"{0}\",", text);
        }

        private static string FormatCSVField(int number)
        {
            return String.Format("\"{0}\",", number);
        }

        private static string FormatCSVField(double number)
        {
            return String.Format("\"{0:0}\",", number);
        }

        private static string FormatCSVField(bool value)
        {
            return String.Format("\"{0}\",", value);
        }
    }
}