﻿using System;
using System.Linq;
using Trinity.Cache;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;
namespace Trinity
{
    public partial class Trinity
    {
        /// <summary>
        /// This will eventually be come our single source of truth and we can get rid of most/all of the below "c_" variables
        /// </summary>
        private static TrinityCacheObject CurrentCacheObject = new TrinityCacheObject();

        private static double c_HitPointsPct = 0d;
        private static double c_HitPoints = 0d;
        private static float c_ZDiff = 0f;
        private static string c_ItemDisplayName = "";
        private static string c_IgnoreReason = "";
        private static string c_IgnoreSubStep = "";
        private static int c_ItemLevel = 0;
        private static string c_ItemLink = String.Empty;
        private static int c_GoldStackSize = 0;
        private static bool c_IsOneHandedItem = false;
        private static bool c_IsTwoHandedItem = false;
        private static ItemQuality c_ItemQuality = ItemQuality.Invalid;
        private static ItemType c_DBItemType = ItemType.Unknown;
        private static ItemBaseType c_DBItemBaseType = ItemBaseType.None;
        private static FollowerType c_item_tFollowerType = FollowerType.None;
        private static GItemType c_item_GItemType = GItemType.Unknown;
        private static MonsterSize c_unit_MonsterSize = MonsterSize.Unknown;
        private static DiaObject c_diaObject = null;
        private static SNOAnim c_CurrentAnimation = SNOAnim.Invalid;
        private static bool c_unit_IsElite = false;
        private static bool c_unit_IsRare = false;
        private static bool c_unit_IsUnique = false;
        private static bool c_unit_IsMinion = false;
        private static bool c_unit_IsTreasureGoblin = false;
        private static bool c_IsEliteRareUnique = false;
        private static bool c_unit_IsAttackable = false;
        private static bool c_unit_HasShieldAffix = false;
        private static bool c_IsObstacle = false;
        private static bool c_HasBeenNavigable = false;
        private static bool c_HasBeenRaycastable = false;
        private static bool c_HasBeenInLoS = false;
        private static string c_ItemMd5Hash = string.Empty;
        private static bool c_HasDotDPS = false;
        private static MonsterAffixes c_MonsterAffixes = MonsterAffixes.None;
        private static bool c_IsFacingPlayer;
        private static float c_Rotation;
        private static Vector2 c_DirectionVector = Vector2.Zero;

        private static bool CacheDiaObject(DiaObject freshObject)
        {
            if (!freshObject.IsValid)
                return false;

            /*
             *  Initialize Variables
             */
            bool AddToCache;

            RefreshStepInit(out AddToCache);
            /*
             *  Get primary reference objects and keys
             */
            c_diaObject = freshObject;

            // Ractor GUID
            CurrentCacheObject.RActorGuid = freshObject.RActorGuid;
            // Check to see if we've already looked at this GUID
            CurrentCacheObject.RActorGuid = freshObject.RActorGuid;
            CurrentCacheObject.ACDGuid = freshObject.ACDGuid;

            // Get Name
            CurrentCacheObject.InternalName = nameNumberTrimRegex.Replace(freshObject.Name, "");
            CurrentCacheObject.InternalName = nameNumberTrimRegex.Replace(freshObject.Name, "");

            CurrentCacheObject.ActorSNO = freshObject.ActorSNO;
            CurrentCacheObject.ActorType = freshObject.ActorType;
            CurrentCacheObject.ACDGuid = freshObject.ACDGuid;
            if (CurrentCacheObject.CommonData == null)
            {
                c_IgnoreReason = "ACDNull";
                return false;
            }
            if (!CurrentCacheObject.CommonData.IsValid)
            {
                c_IgnoreReason = "ACDInvalid";
            }

            // Position
            CurrentCacheObject.Position = CurrentCacheObject.Object.Position;

            // Distance
            CurrentCacheObject.Distance = Player.Position.Distance2D(CurrentCacheObject.Position);

            float radius;
            if (!DataDictionary.CustomObjectRadius.TryGetValue(CurrentCacheObject.ActorSNO, out radius))
            {
                try
                {
                    radius = CurrentCacheObject.Object.CollisionSphere.Radius;
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogCategory.CacheManagement, "Error refreshing Radius: {0}", ex.Message);
                }
            }

            // Radius Distance
            CurrentCacheObject.Radius = radius;

            // Have ActorSNO Check for SNO based navigation obstacle hashlist
            c_IsObstacle = DataDictionary.NavigationObstacleIds.Contains(CurrentCacheObject.ActorSNO);

            // Add Cell Weight for Obstacle
            if (c_IsObstacle)
            {
                Vector3 pos;
                if (!CacheData.Position.TryGetValue(CurrentCacheObject.RActorGuid, out pos))
                {
                    CurrentCacheObject.Position = c_diaObject.Position;
                    //CacheData.Position.Add(CurrentCacheObject.RActorGuid, CurrentCacheObject.Position);
                }
                if (pos != Vector3.Zero)
                    CurrentCacheObject.Position = pos;

                CacheData.NavigationObstacles.Add(new CacheObstacleObject()
                {
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    Name = CurrentCacheObject.InternalName,
                    Position = CurrentCacheObject.Position,
                    Radius = CurrentCacheObject.Radius,
                    ObjectType = CurrentCacheObject.Type,
                });

                ((MainGridProvider)MainGridProvider).AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);

                c_IgnoreReason = "NavigationObstacle";
                AddToCache = false;
                return AddToCache;
            }

            // Summons by the player 
            AddToCache = RefreshStepCachedSummons(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CachedPlayerSummons"; return AddToCache; }

            using (new PerformanceLogger("RefreshDiaObject.CachedType"))
            {
                /*
                 * Set Object Type
                 */
                AddToCache = RefreshStepCachedObjectType(AddToCache);
                if (!AddToCache) { c_IgnoreReason = "CachedObjectType"; return AddToCache; }
            }

            CurrentCacheObject.Type = CurrentCacheObject.Type;
            if (CurrentCacheObject.Type != GObjectType.Item)
            {
                CurrentCacheObject.ObjectHash = HashGenerator.GenerateWorldObjectHash(CurrentCacheObject.ActorSNO, CurrentCacheObject.Position, CurrentCacheObject.Type.ToString(), Trinity.CurrentWorldDynamicId);
            }

            // Check Blacklists
            AddToCache = RefreshStepCheckBlacklists(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "CheckBlacklists"; return AddToCache; }

            if (CurrentCacheObject.Type == GObjectType.Item)
            {
                if (GenericBlacklist.ContainsKey(CurrentCacheObject.ObjectHash))
                {
                    AddToCache = false;
                    c_IgnoreReason = "GenericBlacklist";
                    return AddToCache;
                }
            }

            // Always Refresh ZDiff for every object
            AddToCache = RefreshStepObjectTypeZDiff(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "ZDiff"; return AddToCache; }

            using (new PerformanceLogger("RefreshDiaObject.MainObjectType"))
            {
                /* 
                 * Main Switch on Object Type - Refresh individual object types (Units, Items, Gizmos)
                 */
                RefreshStepMainObjectType(ref AddToCache);
                if (!AddToCache) { c_IgnoreReason = "MainObjectType"; return AddToCache; }
            }

            if (CurrentCacheObject.ObjectHash != String.Empty && GenericBlacklist.ContainsKey(CurrentCacheObject.ObjectHash))
            {
                AddToCache = false;
                c_IgnoreSubStep = "GenericBlacklist";
                return AddToCache;
            }

            // Ignore anything unknown
            AddToCache = RefreshStepIgnoreUnknown(AddToCache);
            if (!AddToCache) { c_IgnoreReason = "IgnoreUnknown"; return AddToCache; }

            using (new PerformanceLogger("RefreshDiaObject.LoS"))
            {
                // Ignore all LoS
                AddToCache = RefreshStepIgnoreLoS(AddToCache);
                if (!AddToCache) { c_IgnoreReason = "IgnoreLoS"; return AddToCache; }
            }

            // If it's a unit, add it to the monster cache
            AddUnitToMonsterObstacleCache();

            c_IgnoreReason = "None";

            CurrentCacheObject.ACDGuid = CurrentCacheObject.ACDGuid;
            CurrentCacheObject.ActorSNO = CurrentCacheObject.ActorSNO;
            CurrentCacheObject.Animation = c_CurrentAnimation;
            CurrentCacheObject.DBItemBaseType = c_DBItemBaseType;
            CurrentCacheObject.DBItemType = c_DBItemType;
            CurrentCacheObject.DirectionVector = c_DirectionVector;
            CurrentCacheObject.Distance = CurrentCacheObject.Distance;
            CurrentCacheObject.DynamicID = CurrentCacheObject.DynamicID;
            CurrentCacheObject.FollowerType = c_item_tFollowerType;
            CurrentCacheObject.GameBalanceID = CurrentCacheObject.GameBalanceID;
            CurrentCacheObject.GoldAmount = c_GoldStackSize;
            CurrentCacheObject.HasAffixShielded = c_unit_HasShieldAffix;
            CurrentCacheObject.HasBeenInLoS = c_HasBeenInLoS;
            CurrentCacheObject.HasBeenNavigable = c_HasBeenNavigable;
            CurrentCacheObject.HasBeenRaycastable = c_HasBeenRaycastable;
            CurrentCacheObject.HasDotDPS = c_HasDotDPS;
            CurrentCacheObject.HitPoints = c_HitPoints;
            CurrentCacheObject.HitPointsPct = c_HitPointsPct;
            CurrentCacheObject.InternalName = CurrentCacheObject.InternalName;
            CurrentCacheObject.IsAttackable = c_unit_IsAttackable;
            CurrentCacheObject.IsElite = c_unit_IsElite;
            CurrentCacheObject.IsEliteRareUnique = c_IsEliteRareUnique;
            CurrentCacheObject.IsFacingPlayer = c_IsFacingPlayer;
            CurrentCacheObject.IsMinion = c_unit_IsMinion;
            CurrentCacheObject.IsRare = c_unit_IsRare;
            CurrentCacheObject.IsTreasureGoblin = c_unit_IsTreasureGoblin;
            CurrentCacheObject.IsUnique = c_unit_IsUnique;
            CurrentCacheObject.ItemLevel = c_ItemLevel;
            CurrentCacheObject.ItemLink = c_ItemLink;
            CurrentCacheObject.ItemQuality = c_ItemQuality;
            CurrentCacheObject.MonsterAffixes = c_MonsterAffixes;
            CurrentCacheObject.MonsterSize = c_unit_MonsterSize;
            CurrentCacheObject.OneHanded = c_IsOneHandedItem;
            CurrentCacheObject.RActorGuid = CurrentCacheObject.RActorGuid;
            CurrentCacheObject.Radius = CurrentCacheObject.Radius;
            CurrentCacheObject.Rotation = c_Rotation;
            CurrentCacheObject.TrinityItemType = c_item_GItemType;
            CurrentCacheObject.TwoHanded = c_IsTwoHandedItem;
            CurrentCacheObject.Type = CurrentCacheObject.Type;
            ObjectCache.Add(CurrentCacheObject);
            return true;
        }

        private static void AddGizmoToNavigationObstacleCache()
        {
            switch (CurrentCacheObject.Type)
            {
                case GObjectType.Barricade:
                case GObjectType.Container:
                case GObjectType.Destructible:
                case GObjectType.Door:
                case GObjectType.HealthWell:
                case GObjectType.Interactable:
                case GObjectType.Shrine:
                    CacheData.NavigationObstacles.Add(new CacheObstacleObject()
                    {
                        ActorSNO = CurrentCacheObject.ActorSNO,
                        Radius = CurrentCacheObject.Radius,
                        Position = CurrentCacheObject.Position,
                        Name = CurrentCacheObject.InternalName,
                        ObjectType = CurrentCacheObject.Type,
                    });
                    break;
            }
        }
        /// <summary>
        /// Adds a unit to cache hashMonsterObstacleCache
        /// </summary>
        private static void AddUnitToMonsterObstacleCache()
        {
            if (CurrentCacheObject.Type == GObjectType.Unit)
            {
                // Add to the collision-list
                CacheData.MonsterObstacles.Add(new CacheObstacleObject()
                {
                    ActorSNO = CurrentCacheObject.ActorSNO,
                    Name = CurrentCacheObject.InternalName,
                    Position = CurrentCacheObject.Position,
                    Radius = CurrentCacheObject.Radius,
                    ObjectType = CurrentCacheObject.Type,
                });
            }
        }
        /// <summary>
        /// Initializes variable set for single object refresh
        /// </summary>
        private static void RefreshStepInit(out bool AddTocache)
        {
            CurrentCacheObject = new TrinityCacheObject();
            AddTocache = true;
            // Start this object as off as unknown type
            CurrentCacheObject.Type = GObjectType.Unknown;

            CurrentCacheObject.Distance = 0f;
            CurrentCacheObject.Radius = 0f;
            c_ZDiff = 0f;
            c_ItemDisplayName = "";
            c_ItemLink = "";
            CurrentCacheObject.InternalName = "";
            c_IgnoreReason = "";
            c_IgnoreSubStep = "";
            CurrentCacheObject.ACDGuid = -1;
            CurrentCacheObject.RActorGuid = -1;
            CurrentCacheObject.DynamicID = -1;
            CurrentCacheObject.GameBalanceID = -1;
            CurrentCacheObject.ActorSNO = -1;
            c_ItemLevel = -1;
            c_GoldStackSize = -1;
            c_HitPointsPct = -1;
            c_HitPoints = -1;
            c_IsOneHandedItem = false;
            c_IsTwoHandedItem = false;
            c_unit_IsElite = false;
            c_unit_IsRare = false;
            c_unit_IsUnique = false;
            c_unit_IsMinion = false;
            c_unit_IsTreasureGoblin = false;
            c_unit_IsAttackable = false;
            c_unit_HasShieldAffix = false;
            c_IsEliteRareUnique = false;
            c_IsObstacle = false;
            c_HasBeenNavigable = false;
            c_HasBeenRaycastable = false;
            c_HasBeenInLoS = false;
            c_ItemMd5Hash = string.Empty;
            c_ItemQuality = ItemQuality.Invalid;
            c_DBItemBaseType = ItemBaseType.None;
            c_DBItemType = ItemType.Unknown;
            c_item_tFollowerType = FollowerType.None;
            c_item_GItemType = GItemType.Unknown;
            c_unit_MonsterSize = MonsterSize.Unknown;
            c_diaObject = null;
            c_CurrentAnimation = SNOAnim.Invalid;
            c_HasDotDPS = false;
            c_MonsterAffixes = MonsterAffixes.None;
            c_IsFacingPlayer = false;
            c_Rotation = 0f;
            c_DirectionVector = Vector2.Zero;
        }

        private static bool RefreshStepIgnoreNullCommonData(bool AddToCache)
        {
            // Null Common Data makes a DiaUseless!
            if (CurrentCacheObject.Type == GObjectType.Unit || CurrentCacheObject.Type == GObjectType.Item || CurrentCacheObject.Type == GObjectType.Gold)
            {
                if (CurrentCacheObject.CommonData == null)
                {
                    AddToCache = false;
                }
                if (CurrentCacheObject.CommonData != null && !CurrentCacheObject.CommonData.IsValid)
                {
                    AddToCache = false;
                }
            }
            return AddToCache;
        }

        private static bool RefreshStepCachedObjectType(bool AddToCache)
        {
            // Set the object type
            // begin with default... 
            CurrentCacheObject.Type = GObjectType.Unknown;

            if (ignoreNames.Any(n => CurrentCacheObject.InternalName.ToLower().Contains(n.ToLower())))
            {
                AddToCache = false;
                c_IgnoreSubStep = "IgnoreNames";
                return AddToCache;
            }

            try
            {
                // Check if it's a unit with an animation we should avoid. We need to recheck this every time.
                if (CurrentCacheObject.Unit != null && CurrentCacheObject.CommonData != null &&
                    Settings.Combat.Misc.AvoidAOE && DataDictionary.AvoidanceAnimations.Contains(new DoubleInt(CurrentCacheObject.ActorSNO, (int)CurrentCacheObject.CommonData.CurrentAnimation)))
                {
                    // The ActorSNO and Animation match a known pair, avoid this!
                    // Example: "Grotesque" death animation
                    AddToCache = true;
                    CurrentCacheObject.Type = GObjectType.Avoidance;
                }
            }
            catch { }

            // Either get the cached object type, or calculate it fresh
            if (!c_IsObstacle)
            {
                // See if it's an avoidance first from the SNO
                bool isAvoidanceSNO = (DataDictionary.Avoidances.Contains(CurrentCacheObject.ActorSNO) ||
                    DataDictionary.AvoidanceBuffs.Contains(CurrentCacheObject.ActorSNO) ||
                    DataDictionary.AvoidanceProjectiles.Contains(CurrentCacheObject.ActorSNO));

                // We're avoiding AoE and this is an AoE
                if (Settings.Combat.Misc.AvoidAOE && isAvoidanceSNO)
                {
                    using (new PerformanceLogger("RefreshCachedType.0"))
                    {
                        // Checking for BuffVisualEffect - for Butcher, maybe useful other places?
                        if (DataDictionary.AvoidanceBuffs.Contains(CurrentCacheObject.ActorSNO))
                        {
                            bool hasBuff = false;
                            try
                            {
                                hasBuff = CurrentCacheObject.CommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) > 0;
                            }
                            catch
                            {

                            }
                            if (hasBuff)
                            {
                                AddToCache = true;
                                CurrentCacheObject.Type = GObjectType.Avoidance;
                            }
                            else
                            {
                                AddToCache = false;
                                c_IgnoreSubStep = "NoBuffVisualEffect";
                            }
                        }
                        else
                        {
                            // Avoidance isn't disabled, so set this object type to avoidance
                            CurrentCacheObject.Type = GObjectType.Avoidance;
                        }
                    }
                }
                else if (!Settings.Combat.Misc.AvoidAOE && isAvoidanceSNO)
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "IgnoreAvoidance";
                }
                // It's not an avoidance, so let's calculate it's object type "properly"
                else
                {
                    // Calculate the object type of this object
                    if (c_diaObject.ActorType == ActorType.Monster)
                    //if (c_diaObject is DiaUnit)
                    {
                        using (new PerformanceLogger("RefreshCachedType.1"))
                        {
                            if (CurrentCacheObject.CommonData == null)
                            {
                                c_IgnoreSubStep = "InvalidUnitCommonData";
                                AddToCache = false;
                            }
                            else if (c_diaObject.ACDGuid != CurrentCacheObject.CommonData.ACDGuid)
                            {
                                c_IgnoreSubStep = "InvalidUnitACDGuid";
                                AddToCache = false;
                            }
                            else
                            {
                                CurrentCacheObject.Type = GObjectType.Unit;
                            }
                        }
                    }
                    else if (c_diaObject.ActorType == ActorType.Player)
                    {
                        CurrentCacheObject.Type = GObjectType.Player;
                    }
                    else if (DataDictionary.ForceToItemOverrideIds.Contains(CurrentCacheObject.ActorSNO) || (c_diaObject.ActorType == ActorType.Item))
                    {
                        using (new PerformanceLogger("RefreshCachedType.2"))
                        {
                            CurrentCacheObject.Type = GObjectType.Item;

                            if (CurrentCacheObject.CommonData == null)
                            {
                                AddToCache = false;
                            }
                            if (CurrentCacheObject.CommonData != null && c_diaObject.ACDGuid != CurrentCacheObject.CommonData.ACDGuid)
                            {
                                AddToCache = false;
                            }

                            if (CurrentCacheObject.InternalName.ToLower().StartsWith("gold"))
                            {
                                CurrentCacheObject.Type = GObjectType.Gold;
                            }
                        }
                    }
                    else if (DataDictionary.InteractWhiteListIds.Contains(CurrentCacheObject.ActorSNO))
                        CurrentCacheObject.Type = GObjectType.Interactable;

                    else if (c_diaObject is DiaGizmo && c_diaObject.ActorType == ActorType.Gizmo && CurrentCacheObject.Distance <= 90)
                    {

                        DiaGizmo c_diaGizmo;
                        c_diaGizmo = (DiaGizmo)c_diaObject;

                        if (CurrentCacheObject.InternalName.Contains("CursedChest"))
                        {
                            CurrentCacheObject.Type = GObjectType.CursedChest;
                            return true;
                        }

                        if (CurrentCacheObject.InternalName.Contains("CursedShrine"))
                        {
                            CurrentCacheObject.Type = GObjectType.CursedShrine;
                            return true;
                        }

                        if (c_diaGizmo.IsBarricade)
                        {
                            CurrentCacheObject.Type = GObjectType.Barricade;
                        }
                        else
                        {
                            switch (c_diaGizmo.ActorInfo.GizmoType)
                            {
                                case GizmoType.HealingWell:
                                    CurrentCacheObject.Type = GObjectType.HealthWell;
                                    break;
                                case GizmoType.Door:
                                    CurrentCacheObject.Type = GObjectType.Door;
                                    break;
                                case GizmoType.PoolOfReflection:
                                case GizmoType.PowerUp:
                                    CurrentCacheObject.Type = GObjectType.Shrine;
                                    break;
                                case GizmoType.Chest:
                                    CurrentCacheObject.Type = GObjectType.Container;
                                    break;
                                case GizmoType.BreakableDoor:
                                    CurrentCacheObject.Type = GObjectType.Barricade;
                                    break;
                                case GizmoType.BreakableChest:
                                    CurrentCacheObject.Type = GObjectType.Destructible;
                                    break;
                                case GizmoType.DestroyableObject:
                                    CurrentCacheObject.Type = GObjectType.Destructible;
                                    break;
                                case GizmoType.PlacedLoot:
                                case GizmoType.Switch:
                                case GizmoType.Headstone:
                                    CurrentCacheObject.Type = GObjectType.Interactable;
                                    break;
                                default:
                                    CurrentCacheObject.Type = GObjectType.Unknown;
                                    break;
                            }
                        }
                    }
                    else
                        CurrentCacheObject.Type = GObjectType.Unknown;
                }
                if (CurrentCacheObject.Type != GObjectType.Unknown)
                {  // Now cache the object type if it's on the screen and we know what it is
                    //CacheData.ObjectType.Add(CurrentCacheObject.RActorGuid, CurrentCacheObject.Type);
                }
            }
            return AddToCache;
        }

        private static void RefreshStepMainObjectType(ref bool AddToCache)
        {
            // Now do stuff specific to object types
            switch (CurrentCacheObject.Type)
            {
                case GObjectType.Player:
                    {
                        AddToCache = RefreshUnit();
                        break;
                    }
                // Handle Unit-type Objects
                case GObjectType.Unit:
                    {
                        // Not allowed to kill monsters due to profile settings
                        if (!Combat.Abilities.CombatBase.IsCombatAllowed)
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "CombatDisabled";
                            break;
                        }
                        
                        AddToCache = RefreshUnit();
                        break;
                    }
                // Handle Item-type Objects
                case GObjectType.Item:
                    {
                        // Not allowed to loot due to profile settings
                        // rrrix disabled this since noobs can't figure out their profile is broken... looting is always enabled now
                        //if (!LootTargeting.Instance.AllowedToLoot || LootTargeting.Instance.DisableLooting)
                        //{
                        //    AddToCache = false;
                        //    c_IgnoreSubStep = "LootingDisabled";
                        //    break;
                        //}

                        if (TrinityItemManager.FindValidBackpackLocation(true) == new Vector2(-1, -1))
                        {
                            AddToCache = false;
                            c_IgnoreSubStep = "NoFreeSlots";
                            break;
                        }
                        AddToCache = RefreshItem();
                        break;

                    }
                // Handle Gold
                case GObjectType.Gold:
                    {
                        // Not allowed to loot due to profile settings
                        //if (!LootTargeting.Instance.AllowedToLoot || LootTargeting.Instance.DisableLooting)
                        //{
                        //    AddToCache = false;
                        //    c_IgnoreSubStep = "LootingDisabled";
                        //    break;
                        //}
                        AddToCache = RefreshGold(AddToCache);
                        break;
                    }
                case GObjectType.PowerGlobe:
                case GObjectType.HealthGlobe:
                    {
                        // Ignore it if it's not in range yet
                        if (CurrentCacheObject.Distance > CurrentBotLootRange || CurrentCacheObject.Distance > 60f)
                        {
                            c_IgnoreSubStep = "GlobeOutOfRange";
                            AddToCache = false;
                        }
                        AddToCache = true;
                        break;
                    }
                // Handle Avoidance Objects
                case GObjectType.Avoidance:
                    {
                        AddToCache = RefreshAvoidance(AddToCache);
                        if (!AddToCache) { c_IgnoreSubStep = "RefreshAvoidance"; }

                        break;
                    }
                // Handle Other-type Objects
                case GObjectType.Destructible:
                case GObjectType.Door:
                case GObjectType.Barricade:
                case GObjectType.Container:
                case GObjectType.Shrine:
                case GObjectType.Interactable:
                case GObjectType.HealthWell:
                case GObjectType.CursedChest:
                case GObjectType.CursedShrine:
                    {
                        AddToCache = RefreshGizmo(AddToCache);
                        break;
                    }
                // Object switch on type (to seperate shrines, destructibles, barricades etc.)
                case GObjectType.Unknown:
                default:
                    {
                        c_IgnoreSubStep = "Unknown." + c_diaObject.ActorType.ToString();
                        AddToCache = false;
                        break;
                    }
            }
        }


        /// <summary>
        /// Special handling for whether or not we want to cache an object that's not in LoS
        /// </summary>
        /// <param name="c_diaObject"></param>
        /// <param name="AddToCache"></param>
        /// <returns></returns>
        private static bool RefreshStepIgnoreLoS(bool AddToCache = false)
        {
            try
            {
                if (CurrentCacheObject.Type == GObjectType.Item || CurrentCacheObject.Type == GObjectType.Gold)
                    return true;

                if (!DataDictionary.AlwaysRaycastWorlds.Contains(Trinity.Player.WorldID))
                {
                    // Bounty Objectives should always be on the weight list
                    if (CurrentCacheObject.IsBountyObjective)
                        return true;

                    // Quest Monsters should get LoS white-listed
                    if (CurrentCacheObject.IsQuestMonster)
                        return true;

                    // Always LoS Units during events
                    if (CurrentCacheObject.Type == GObjectType.Unit && Player.InActiveEvent)
                        return true;
                }
                // Everything except items and the current target
                if (CurrentCacheObject.RActorGuid != LastTargetRactorGUID && CurrentCacheObject.Type != GObjectType.Unknown)
                {
                    if (CurrentCacheObject.Distance < 95)
                    {
                        using (new PerformanceLogger("RefreshLoS.2"))
                        {
                            // Get whether or not this RActor has ever been in a path line with AllowWalk. If it hasn't, don't add to cache and keep rechecking
                            if (!CacheData.HasBeenRayCasted.TryGetValue(CurrentCacheObject.RActorGuid, out c_HasBeenRaycastable) || DataDictionary.AlwaysRaycastWorlds.Contains(Trinity.Player.WorldID))
                            {
                                if (CurrentCacheObject.Distance >= 1f && CurrentCacheObject.Distance <= 5f)
                                {
                                    c_HasBeenRaycastable = true;
                                    if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                                        CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                                }
                                else if (Settings.Combat.Misc.UseNavMeshTargeting)
                                {
                                    Vector3 myPos = new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z + 8f);
                                    Vector3 cPos = new Vector3(CurrentCacheObject.Position.X, CurrentCacheObject.Position.Y, CurrentCacheObject.Position.Z + 8f);
                                    cPos = MathEx.CalculatePointFrom(cPos, myPos, CurrentCacheObject.Radius + 1f);

                                    if (Single.IsNaN(cPos.X) || Single.IsNaN(cPos.Y) || Single.IsNaN(cPos.Z))
                                        cPos = CurrentCacheObject.Position;

                                    if (!NavHelper.CanRayCast(myPos, cPos))
                                    {
                                        AddToCache = false;
                                        c_IgnoreSubStep = "UnableToRayCast";
                                    }
                                    else
                                    {
                                        c_HasBeenRaycastable = true;
                                        if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                                            CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                                    }
                                }
                                else
                                {
                                    if (c_ZDiff > 14f)
                                    {
                                        AddToCache = false;
                                        c_IgnoreSubStep = "LoS.ZDiff";
                                    }
                                    else
                                    {
                                        c_HasBeenRaycastable = true;
                                        if (!CacheData.HasBeenRayCasted.ContainsKey(CurrentCacheObject.RActorGuid))
                                            CacheData.HasBeenRayCasted.Add(CurrentCacheObject.RActorGuid, c_HasBeenRaycastable);
                                    }

                                }
                            }
                        }
                        using (new PerformanceLogger("RefreshLoS.3"))
                        {
                            // Get whether or not this RActor has ever been in "Line of Sight" (as determined by Demonbuddy). If it hasn't, don't add to cache and keep rechecking
                            if (!CacheData.HasBeenInLoS.TryGetValue(CurrentCacheObject.RActorGuid, out c_HasBeenInLoS) || DataDictionary.AlwaysRaycastWorlds.Contains(Trinity.Player.WorldID))
                            {
                                // Ignore units not in LoS except bosses
                                if (!CurrentCacheObject.IsBoss && !c_diaObject.InLineOfSight)
                                {
                                    AddToCache = false;
                                    c_IgnoreSubStep = "NotInLoS";
                                }
                                else
                                {
                                    c_HasBeenInLoS = true;
                                    if (!CacheData.HasBeenInLoS.ContainsKey(CurrentCacheObject.RActorGuid))
                                        CacheData.HasBeenInLoS.Add(CurrentCacheObject.RActorGuid, c_HasBeenInLoS);
                                }

                            }
                        }
                    }
                    else
                    {
                        AddToCache = false;
                        c_IgnoreSubStep = "LoS-OutOfRange";
                    }


                    // always set true for bosses nearby
                    if (CurrentCacheObject.IsBoss || CurrentCacheObject.IsQuestMonster || CurrentCacheObject.IsBountyObjective)
                    {
                        AddToCache = true;
                        c_IgnoreSubStep = "";
                    }
                    // always take the current target even if not in LoS
                    if (CurrentCacheObject.RActorGuid == LastTargetRactorGUID)
                    {
                        AddToCache = true;
                        c_IgnoreSubStep = "";
                    }
                }

                // Simple whitelist for LoS 
                if (DataDictionary.LineOfSightWhitelist.Contains(CurrentCacheObject.ActorSNO))
                {
                    AddToCache = true;
                    c_IgnoreSubStep = "";
                }
                // Always pickup Infernal Keys whether or not in LoS
                if (DataDictionary.ForceToItemOverrideIds.Contains(CurrentCacheObject.ActorSNO))
                {
                    AddToCache = true;
                    c_IgnoreSubStep = "";
                }

            }
            catch (Exception ex)
            {
                AddToCache = true;
                c_IgnoreSubStep = "IgnoreLoSException";
                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
            }
            return AddToCache;
        }

        private static bool RefreshStepIgnoreUnknown(bool AddToCache)
        {
            // We couldn't get a valid object type, so ignore it
            if (!c_IsObstacle && CurrentCacheObject.Type == GObjectType.Unknown)
            {
                AddToCache = false;
            }
            return AddToCache;
        }

        private static bool RefreshStepObjectTypeZDiff(bool AddToCache)
        {
            c_ZDiff = c_diaObject.ZDiff;
            // always take current target regardless if ZDiff changed
            if (CurrentCacheObject.RActorGuid == LastTargetRactorGUID)
            {
                AddToCache = true;
                return AddToCache;
            }

            // Special whitelist for always getting stuff regardless of ZDiff or LoS
            if (DataDictionary.LineOfSightWhitelist.Contains(CurrentCacheObject.ActorSNO))
            {
                AddToCache = true;
                return AddToCache;
            }
            // Ignore stuff which has a Z-height-difference too great, it's probably on a different level etc. - though not avoidance!
            if (CurrentCacheObject.Type != GObjectType.Avoidance)
            {
                // Calculate the z-height difference between our current position, and this object's position
                switch (CurrentCacheObject.Type)
                {
                    case GObjectType.Door:
                    case GObjectType.Unit:
                    case GObjectType.Barricade:
                        // Ignore monsters (units) who's Z-height is 14 foot or more than our own z-height except bosses
                        if (c_ZDiff >= 14f && !CurrentCacheObject.IsBoss)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Item:
                    case GObjectType.HealthWell:
                        // Items at 26+ z-height difference (we don't want to risk missing items so much)
                        if (c_ZDiff >= 26f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Gold:
                    case GObjectType.HealthGlobe:
                    case GObjectType.PowerGlobe:
                        // Gold/Globes at 11+ z-height difference
                        if (c_ZDiff >= 11f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Destructible:
                    case GObjectType.Shrine:
                    case GObjectType.Container:
                        // Destructibles, shrines and containers are the least important, so a z-height change of only 7 is enough to ignore (help avoid stucks at stairs etc.)
                        if (c_ZDiff >= 7f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Interactable:
                        // Special interactable objects
                        if (c_ZDiff >= 9f)
                        {
                            AddToCache = false;
                        }
                        break;
                    case GObjectType.Unknown:
                    default:
                        {
                            // Don't touch it!
                        }
                        break;
                }
            }
            else
            {
                AddToCache = true;
            }
            return AddToCache;
        }

        private static bool RefreshStepCheckBlacklists(bool AddToCache)
        {
            if (!DataDictionary.Avoidances.Contains(CurrentCacheObject.ActorSNO) && !DataDictionary.AvoidanceBuffs.Contains(CurrentCacheObject.ActorSNO) && !CurrentCacheObject.IsBountyObjective && !CurrentCacheObject.IsQuestMonster)
            {
                // See if it's something we should always ignore like ravens etc.
                if (!c_IsObstacle && DataDictionary.BlackListIds.Contains(CurrentCacheObject.ActorSNO))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "Blacklist";
                    return AddToCache;
                }
                // Temporary ractor GUID ignoring, to prevent 2 interactions in a very short time which can cause stucks
                if (_ignoreTargetForLoops > 0 && _ignoreRactorGuid == CurrentCacheObject.RActorGuid)
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "IgnoreRactorGUID";
                    return AddToCache;
                }
                // Check our extremely short-term destructible-blacklist
                if (_destructible3SecBlacklist.Contains(CurrentCacheObject.RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "Destructible3SecBlacklist";
                    return AddToCache;
                }
                // Check our extremely short-term destructible-blacklist
                if (Blacklist3Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "hashRGUIDBlacklist3";
                    return AddToCache;
                }
                // See if it's on our 90 second blacklist (from being stuck targeting it), as long as it's distance is not extremely close
                if (Blacklist90Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "Blacklist90Seconds";
                    return AddToCache;
                }
                // 60 second blacklist
                if (Blacklist60Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "Blacklist60Seconds";
                    return AddToCache;
                }
                // 15 second blacklist
                if (Blacklist15Seconds.Contains(CurrentCacheObject.RActorGuid))
                {
                    AddToCache = false;
                    c_IgnoreSubStep = "Blacklist15Seconds";
                    return AddToCache;
                }
            }
            else
            {
                AddToCache = true;
            }
            return AddToCache;
        }



        private static string UtilSpacedConcat(params object[] args)
        {
            string output = "";
            foreach (object o in args)
            {
                output += o.ToString() + ", ";
            }
            return output;
        }


        private static void RefreshCachedHealth(int iLastCheckedHealth, double dThisCurrentHealth, bool bHasCachedHealth)
        {
            if (!bHasCachedHealth)
            {
                CacheData.CurrentUnitHealth.Add(CurrentCacheObject.RActorGuid, dThisCurrentHealth);
                CacheData.LastCheckedUnitHealth.Add(CurrentCacheObject.RActorGuid, iLastCheckedHealth);
            }
            else
            {
                CacheData.CurrentUnitHealth[CurrentCacheObject.RActorGuid] = dThisCurrentHealth;
                CacheData.LastCheckedUnitHealth[CurrentCacheObject.RActorGuid] = iLastCheckedHealth;
            }
        }

    }
}
