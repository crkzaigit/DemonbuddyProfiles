﻿using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Technicals;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {


        /// <summary>
        /// Check if a particular buff is present
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool GetHasBuff(SNOPower power)
        {
            int id = (int)power;
            return listCachedBuffs.Any(u => u.SNOId == id);
        }

        /// <summary>
        /// Returns how many stacks of a particular buff there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetBuffStacks(SNOPower power)
        {
            int stacks;
            if (PlayerBuffs.TryGetValue((int)power, out stacks))
            {
                return stacks;
            }
            return 0;
        }

        /// <summary>
        /// Check re-use timers on skills, returns true if we can use the power
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="recheck">if set to <c>true</c> check again.</param>
        /// <returns>
        /// Returns whether or not we can use a skill, or if it's on our own internal Trinity cooldown timer
        /// </returns>
        public static bool SNOPowerUseTimer(SNOPower power, bool recheck = false)
        {
            if (TimeSinceUse(power) >= CombatBase.GetSNOPowerUseDelay(power))
                return true;
            if (recheck && TimeSinceUse(power) >= 150 && TimeSinceUse(power) <= 600)
                return true;
            return false;
        }
        
        /// <summary>
        /// A default power in case we can't use anything else
        /// </summary>
        private static TrinityPower defaultPower = new TrinityPower();

        /// <summary>
        /// Returns an appropriately selected TrinityPower and related information
        /// </summary>
        /// <param name="IsCurrentlyAvoiding">Are we currently avoiding?</param>
        /// <param name="UseOOCBuff">Buff Out Of Combat</param>
        /// <param name="UseDestructiblePower">Is this for breaking destructables?</param>
        /// <returns></returns>
        internal static TrinityPower AbilitySelector(bool IsCurrentlyAvoiding = false, bool UseOOCBuff = false, bool UseDestructiblePower = false)
        {
            using (new PerformanceLogger("AbilitySelector"))
            {
                if (!UseOOCBuff && CurrentTarget == null)
                    return new TrinityPower();

                // See if archon just appeared/disappeared, so update the hotbar
                if (ShouldRefreshHotbarAbilities || HotbarRefreshTimer.ElapsedMilliseconds > 5000)
                    PlayerInfoCache.RefreshHotbar();

                // Switch based on the cached character class

                TrinityPower power = CombatBase.CurrentPower;


                using (new PerformanceLogger("AbilitySelector.ClassAbility"))
                {
                    switch (Player.ActorClass)
                    {
                        // Barbs
                        case ActorClass.Barbarian:
                            //power = GetBarbarianPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            power = BarbarianCombat.GetPower();
                            break;
                        // Crusader
                        case ActorClass.Crusader:
                            power = CrusaderCombat.GetPower();
                            break;
                        // Monks
                        case ActorClass.Monk:
                            power = GetMonkPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                        // Wizards
                        case ActorClass.Wizard:
                            power = GetWizardPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                        // Witch Doctors
                        case ActorClass.Witchdoctor:
                            power = WitchDoctorCombat.GetPower();
                            break;
                        // Demon Hunters
                        case ActorClass.DemonHunter:
                            power = GetDemonHunterPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                    }
                }
                // use IEquatable to check if they're equal
                if (CombatBase.CurrentPower == power)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Keeping {0}", CombatBase.CurrentPower.ToString());
                    return CombatBase.CurrentPower;
                }
                else if (power != null && power.SNOPower != SNOPower.None)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Selected new {0}", power.ToString());
                    return power;
                }
                else
                    return defaultPower;
            }
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or true if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!Hotbar.Contains(snoPower) || (Hotbar.Contains(snoPower) && GetHasBuff(snoPower)));

        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static double TimeSinceUse(SNOPower power)
        {
            if (CacheData.AbilityLastUsed.ContainsKey(power))
                return DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[power]).TotalMilliseconds;
            else
                return -1;
        }


    }

}
