using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Armory.Hero
{
    /// <summary>
    ///     The base class for all Hero types
    /// </summary>
    public abstract class HeroClass
    {
        protected HeroClass()
        {
            // nothing
        }

        /// <summary>
        ///     All equipped "Active" skills
        /// </summary>
        public static List<SNOPower> PassiveSkills
        {
            get { return ZetaDia.CPlayer.PassiveSkills.ToList(); }
        }

        /// <summary>
        ///     All equipped "Passive" skills
        /// </summary>
        public static List<SNOPower> ActiveSkills
        {
            get { return ZetaDia.CPlayer.ActiveSkills.ToList(); }
        }

        /// <summary>
        ///     Returns the rune index (integer) from a given Hotbar slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        internal static int GetRuneIndexFromSlot(HotbarSlot slot)
        {
            return ZetaDia.CPlayer.GetRuneIndexForSlot(slot);
        }

        /// <summary>
        ///     Returns the current Power from a given Hotbar slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        internal static SNOPower GetPowerFromSlot(HotbarSlot slot)
        {
            return ZetaDia.CPlayer.GetPowerForSlot(slot);
        }

        /// <summary>
        ///     Gets the HotbarSlot from a given Power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static HotbarSlot GetSlotFromPower(SNOPower power)
        {
            foreach (HotbarSlot slot in Enum.GetValues(typeof(HotbarSlot)).Cast<HotbarSlot>().Where(slot => GetPowerFromSlot(slot) == power))
            {
                return slot;
            }
            return HotbarSlot.Invalid;
        }

        /// <summary>
        /// Checks to see if a Follower can Equip an Item (Exluding bows)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool CanFollowerEquipItem(ACDItem item)
        {
            ItemType itemType = item.ItemType;

            bool result = itemType == ItemType.Amulet || itemType == ItemType.Ring || itemType == ItemType.Axe ||
                          itemType == ItemType.Dagger || itemType == ItemType.Mace || itemType == ItemType.Spear || itemType == ItemType.Sword;

            return item.RequiredLevel < ZetaDia.Me.Level - 1 && result;
        }

        /// <summary>
        /// The Hero's Level
        /// </summary>
        internal int Level { get { return ZetaDia.Me.Level; } }

        /// <summary>
        /// The Hero's Paragon Level
        /// </summary>
        internal int ParagonLevel { get { return ZetaDia.Me.ParagonLevel; } }

        /// <summary>
        /// The Hero's base Vitality
        /// </summary>
        internal double Vitality
        {
            get { return 7 + (ZetaDia.Me.Level + ZetaDia.Me.ParagonLevel) * 2; }
        }

        /*
         * Virtual methods available for class-specific implimentation
         */
        internal abstract List<ItemType> EquippableItemTypes { get; }

        internal abstract List<ItemType> UnEquippableItemTypes { get; }

        /// <summary>
        /// The Hero's base Dexterity
        /// </summary>
        internal virtual double Dexterity
        {
            get { return 7 * (Level + ParagonLevel); }
        }

        /// <summary>
        /// The Hero's base Intelligence
        /// </summary>
        internal virtual double Intelligence
        {
            get { return 7 * (Level + ParagonLevel); }
        }

        /// <summary>
        /// The Hero's base Strength
        /// </summary>
        internal virtual double Strength
        {
            get { return 7 * (Level + ParagonLevel); }
        }

        internal virtual bool CanEquipItem(ItemWrapper item, InventorySlot slot)
        {
            return ItemEvaluator.GenericEquippableItemTypes.Contains(item.ItemType);
        }

        /*
         * Primary Stats
         */
        internal virtual double GetAddedStrength(double baseValue = 0)
        {
            return baseValue;
        }

        internal virtual double GetAddedIntelligence(double baseValue = 0)
        {
            return baseValue;
        }


        internal virtual double GetAddedDexterity(double baseValue = 0)
        {
            return baseValue;
        }


        /*
         * EHP stats
         */
        internal virtual double GetBaseReduction(double baseValue = 100)
        {
            return baseValue;
        }


        internal virtual double GetAddedArmor(double baseValue = 0)
        {
            return baseValue;
        }

        internal virtual double GetAddedAllResist(double baseValue = 0)
        {
            return baseValue;
        }

        internal virtual double GetAddedDodge(double baseValue = 0)
        {
            return baseValue;
        }

        internal virtual double GetAddedHitPoints(double baseValue = 0)
        {
            return baseValue;
        }
        internal virtual double GetAddedDamageReduction(double baseValue = 0)
        {
            return baseValue;
        }
        /*
         *  DPS stats
         */
        internal virtual double GetAddedDamagePercent(double baseValue = 0)
        {
            return baseValue;
        }
        internal virtual double GetAddedCritPercent(double baseValue = 0)
        {
            return baseValue;
        }
        internal virtual double GetAddedCritDamagePercent(double baseValue = 0)
        {
            return baseValue;
        }
        internal virtual double GetAddedAttackSpeed(double baseValue = 0)
        {
            return baseValue;
        }
        internal virtual double GetAddedPrimaryStat(double baseValue = 0)
        {
            return baseValue;
        }
    }
}