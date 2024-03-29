﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Config;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class CombatBase
    {
        private static TrinityPower _currentPower = new TrinityPower();
        private static Vector3 _lastZigZagLocation = Vector3.Zero;
        private static Vector3 _zigZagPosition = Vector3.Zero;
        private static bool _isCombatAllowed = true;
        private static KiteMode _playerKiteMode = KiteMode.Never;


        public enum CanCastFlags
        {
            All = 2,
            NoTimer = 4,
            NoPowerManager = 8
        }

        /// <summary>
        /// Distance to kite, read settings (class independant)
        /// </summary>
        public static int PlayerKiteDistance { get; set; }

        // When to Kite
        public static KiteMode PlayerKiteMode
        {
            get { return _playerKiteMode; }
            set { _playerKiteMode = value; }
        }


        /// <summary>
        /// Allows for completely disabling combat. Settable through API only. 
        /// </summary>
        public static bool IsCombatAllowed
        {
            get
            {
                // if disabled in the profile, or disabled through api
                if (!CombatTargeting.Instance.AllowedToKillMonsters)
                    return false;

                if (!_isCombatAllowed)
                    return false;
                return true;
            }
            set { _isCombatAllowed = value; }
        }

        public static bool IsQuestingMode { get; set; }

        /// <summary>
        /// The last "ZigZag" position, used with Barb Whirlwind, Monk Tempest Rush, etc.
        /// </summary>
        public static Vector3 ZigZagPosition
        {
            get { return _zigZagPosition; }
            internal set { _zigZagPosition = value; }
        }

        /// <summary>
        /// Returns an appropriately selected TrinityPower and related information
        /// </summary>
        /// <returns></returns>
        internal static TrinityPower AbilitySelector()
        {
            using (new PerformanceLogger("AbilitySelector"))
            {
                // See if archon just appeared/disappeared, so update the hotbar
                if (Trinity.ShouldRefreshHotbarAbilities || Trinity.HotbarRefreshTimer.ElapsedMilliseconds > 5000)
                    PlayerInfoCache.RefreshHotbar();

                // Switch based on the cached character class

                TrinityPower power = CurrentPower;

                using (new PerformanceLogger("AbilitySelector.ClassAbility"))
                {
                    switch (Player.ActorClass)
                    {
                        // Barbs
                        case ActorClass.Barbarian:
                            power = BarbarianCombat.GetPower();
                            break;
                        case ActorClass.Crusader:
                            power = CrusaderCombat.GetPower();
                            break;
                        // Monks
                        //case ActorClass.Monk:
                        //    power = GetMonkPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                        //// Wizards
                        //case ActorClass.Wizard:
                        //    power = GetWizardPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                        //// Witch Doctors
                        //case ActorClass.Witchdoctor:
                        //    power = GetWitchDoctorPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                        //// Demon Hunters
                        //case ActorClass.DemonHunter:
                        //    power = GetDemonHunterPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                    }
                }
                // use IEquatable to check if they're equal
                if (CurrentPower == power)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Keeping {0}", CurrentPower.ToString());
                    return CurrentPower;
                }
                if (power != null)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Selected new {0}", power.ToString());
                    return power;
                }
                return DefaultPower;
            }
        }

        /// <summary>
        /// A dictionary containing the date time we last used a specific spell
        /// </summary>
        public static Dictionary<SNOPower, DateTime> AbilityLastUsedCache
        {
            get
            {
                return CacheData.AbilityLastUsed;
            }
            set
            {
                CacheData.AbilityLastUsed = value;
            }
        }

        /// <summary>
        /// Always contains the last power used
        /// </summary>
        public static SNOPower LastPowerUsed
        {
            get
            {
                return Trinity.LastPowerUsed;
            }
        }

        /// <summary>
        /// Gets/sets whether we are building up energy for a big spell
        /// </summary>
        public static bool IsWaitingForSpecial { get; set; }

        /// <summary>
        /// Minimum energy reserve for using "Big" spells/powers
        /// </summary>
        public static int MinEnergyReserve
        {
            get
            {
                switch (Player.ActorClass)
                {
                    case ActorClass.Barbarian:
                        return V.I("Barbarian.MinEnergyReserve");
                    case ActorClass.DemonHunter:
                        return V.I("DemonHunter.MinEnergyReserve");
                    case ActorClass.Monk:
                        return V.I("Monk.MinEnergyReserve");
                    case ActorClass.Witchdoctor:
                        return V.I("WitchDoctor.MinEnergyReserve");
                    case ActorClass.Wizard:
                        return V.I("Wizard.MinEnergyReserve");
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Arcane, Frozen, Jailer, Molten, Electrified+Reflect Damage elites
        /// </summary>
        public static bool HardElitesPresent
        {
            get
            {
                return
                   Trinity.ObjectCache.Any(o => o.IsEliteRareUnique &&
                          o.MonsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted | MonsterAffixes.Frozen | MonsterAffixes.Jailer | MonsterAffixes.Molten) ||
                          (o.MonsterAffixes.HasFlag(MonsterAffixes.Electrified) && o.MonsterAffixes.HasFlag(MonsterAffixes.ReflectsDamage))) ||
                        Trinity.ObjectCache.Any(o => o.IsBoss);
            }
        }

        public static bool IgnoringElites
        {
            get
            {
                return !IsQuestingMode && Settings.Combat.Misc.IgnoreElites;
            }
        }

        public static TrinitySetting Settings
        {
            get { return Trinity.Settings; }
        }

        public static bool UseOOCBuff
        {
            get
            {
                if (CurrentTarget == null)
                    return true;
                return false;
            }
        }

        public static bool IsCurrentlyAvoiding
        {
            get
            {
                if (CurrentTarget == null)
                    return false;

                if (CurrentTarget.Type == GObjectType.Avoidance)
                    return true;
                return false;
            }
        }

        public static bool UseDestructiblePower
        {
            get
            {
                if (CurrentTarget == null)
                    return false;

                switch (CurrentTarget.Type)
                {
                    case GObjectType.Destructible:
                    case GObjectType.Barricade:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static Vector3 LastZigZagLocation
        {
            get { return _lastZigZagLocation; }
            set { _lastZigZagLocation = value; }
        }

        public static TrinityPower CurrentPower
        {
            get { return _currentPower; }
            set { _currentPower = value; }
        }

        public static List<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }
        public static PlayerInfoCache Player
        {
            get
            {
                return Trinity.Player;
            }
        }

        public static TrinityCacheObject CurrentTarget
        {
            get
            {
                return Trinity.CurrentTarget;
            }
        }
        public static TrinityPower DefaultPower
        {
            get
            {
                Trinity.ShouldRefreshHotbarAbilities = true;

                // Default attacks
                if (!UseOOCBuff && !IsCurrentlyAvoiding)
                {
                    if (Trinity.Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_SweepingWind))
                    {
                        Trinity.Monk_TickSweepingWindSpam();
                    }

                    return new TrinityPower
                    {
                        SNOPower = DefaultWeaponPower,
                        MinimumRange = DefaultWeaponDistance,
                        TargetACDGUID = CurrentTarget.ACDGuid,
                    };
                }
                return new TrinityPower();
            }
        }

        /// <summary>
        /// Gets the default weapon power based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static SNOPower DefaultWeaponPower
        {
            get
            {
                ACDItem lhItem = ZetaDia.Me.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == InventorySlot.LeftHand);
                if (lhItem == null)
                    return SNOPower.None;

                switch (lhItem.ItemType)
                {
                    default:
                        return SNOPower.Weapon_Melee_Instant;
                    case ItemType.Axe:
                    case ItemType.CeremonialDagger:
                    case ItemType.Dagger:
                    case ItemType.Daibo:
                    case ItemType.FistWeapon:
                    case ItemType.Mace:
                    case ItemType.Polearm:
                    case ItemType.Spear:
                    case ItemType.Staff:
                    case ItemType.Sword:
                    case ItemType.MightyWeapon:
                        return SNOPower.Weapon_Melee_Instant;
                    case ItemType.Wand:
                        return SNOPower.Weapon_Ranged_Wand;
                    case ItemType.Bow:
                    case ItemType.Crossbow:
                    case ItemType.HandCrossbow:
                        return SNOPower.Weapon_Ranged_Projectile;
                }
            }
        }
        /// <summary>
        /// Gets the default weapon distance based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static float DefaultWeaponDistance
        {
            get
            {
                switch (DefaultWeaponPower)
                {
                    case SNOPower.Weapon_Ranged_Instant:
                    case SNOPower.Weapon_Ranged_Projectile:
                        return 65f;
                    case SNOPower.Weapon_Ranged_Wand:
                        return 35f;
                    default:
                        return 10f;
                }
            }
        }

        /// <summary>
        /// Performs basic checks to see if we have and can cast a power (hotbar, power manager). Checks use timer for Wiz, DH, Monk
        /// </summary>
        /// <param name="power"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool CanCast(SNOPower power, CanCastFlags flags = CanCastFlags.All)
        {
            bool hasPower = Hotbar.Contains(power);
            if (!hasPower)
                return false;

            // Skip this or Barb, Crusader, WD
            if (Player.ActorClass == ActorClass.Wizard || Player.ActorClass == ActorClass.DemonHunter || Player.ActorClass == ActorClass.Monk)
            {
                bool timer = flags.HasFlag(CanCastFlags.NoTimer) || SNOPowerUseTimer(power);

                if (!timer)
                    return false;
            }

            bool powerManager = flags.HasFlag(CanCastFlags.NoPowerManager) || PowerManager.CanCast(power);

            if (!powerManager)
                return false;

            return true;
        }

        /// <summary>
        /// Check if a particular buff is present
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool GetHasBuff(SNOPower power)
        {
            int id = (int)power;
            return Trinity.listCachedBuffs.Any(u => u.SNOId == id);
        }

        /// <summary>
        /// Returns how many stacks of a particular buff there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetBuffStacks(SNOPower power)
        {
            int stacks;
            if (Trinity.PlayerBuffs.TryGetValue((int)power, out stacks))
            {
                return stacks;
            }
            return 0;
        }

        /// <summary>
        /// Check re-use timers on skills
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="recheck">if set to <c>true</c> check again.</param>
        /// <returns>
        /// Returns whether or not we can use a skill, or if it's on our own internal Trinity cooldown timer
        /// </returns>
        public static bool SNOPowerUseTimer(SNOPower power, bool recheck = false)
        {
            if (TimeSincePowerUse(power) >= GetSNOPowerUseDelay(power))
                return true;
            if (recheck && TimeSincePowerUse(power) >= 150 && TimeSincePowerUse(power) <= 600)
                return true;
            return false;
        }

        public static void SetSNOPowerUseDelay(SNOPower power, double delay)
        {
            string key = "SpellDelay." + power.ToString();
            TVar v = V.Data[key];

            bool hasDefaultValue = v.Value == v.DefaultValue;

            if (hasDefaultValue)
            {
                // Create a new TVar (changes the default value)
                V.Set(new TVar(v.Name, delay, v.Description));
            }
        }

        public static double GetSNOPowerUseDelay(SNOPower power)
        {
            return V.D("SpellDelay." + power.ToString());
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or true if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!Trinity.Hotbar.Contains(snoPower) || (Trinity.Hotbar.Contains(snoPower) && GetHasBuff(snoPower)));

        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static double TimeSincePowerUse(SNOPower power)
        {
            if (CacheData.AbilityLastUsed.ContainsKey(power))
                return DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[power]).TotalMilliseconds;
            return -1;
        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static TimeSpan TimeSpanSincePowerUse(SNOPower power)
        {
            if (CacheData.AbilityLastUsed.ContainsKey(power))
                return DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[power]);
            return TimeSpan.MinValue;
        }

        /// <summary>
        /// Check if a power is null
        /// </summary>
        /// <param name="power"></param>
        protected static bool IsNull(TrinityPower power)
        {
            return power == null;
        }
    }
}
