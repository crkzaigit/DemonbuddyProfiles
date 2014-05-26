using System;
using System.Collections.Generic;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

namespace Armory
{
    public class ItemWrapper
    {

        public int ActorSNO { get; set; }
        public int GameBalanceId { get; set; }
        public int DynamicID { get; set; }
        public int ACDGuid { get; set; }
        public int RequiredLevel { get; set; }
        public InventorySlot InventorySlot { get; set; }
        public InventorySlot[] ValidInventorySlots { get; set; }
        public bool IsUnidentified { get; set; }
        public bool IsTwoHand { get; set; }
        public bool IsOneHand { get; set; }
        public string Name { get; set; }
        public string InternalName { get; set; }
        public ItemType ItemType { get; set; }
        public ItemBaseType ItemBaseType { get; set; }
        public bool HasSingleUseSlot { get; set; }
        public bool IsShield { get; set; }
        public bool IsOffHand { get; set; }
        public bool IsWeapon { get; set; }
        public bool IsJewlery { get; set; }
        public bool IsArmor { get; set; }
        public bool IsMisc { get; set; }

        public ACDItem Item { get; private set; }
        public ItemStats Stats { get; private set; }

        public ItemWrapper(ACDItem item)
        {
            ActorSNO = item.ActorSNO;
            GameBalanceId = item.GameBalanceId;
            DynamicID = item.DynamicId;
            ACDGuid = item.ACDGuid;
            InventorySlot = item.InventorySlot;
            ValidInventorySlots = item.ValidInventorySlots;
            RequiredLevel = item.RequiredLevel;
            IsUnidentified = item.IsUnidentified;
            IsTwoHand = item.IsTwoHand;
            IsOneHand = item.IsOneHand;
            Name = item.Name;
            InternalName = item.InternalName;
            this.ItemType = item.ItemType;
            this.ItemBaseType = item.ItemBaseType;
            IsShield = ShieldTypes.Contains(ItemType);
            IsOffHand = OffHandTypes.Contains(ItemType);
            IsArmor = ArmorTypes.Contains(ItemType);
            IsJewlery = JewleryTypes.Contains(ItemType);
            IsWeapon = WeaponTypes.Contains(ItemType);
            IsMisc = MiscTypes.Contains(ItemType);

            HasSingleUseSlot = IsSingleSlotItem();

            Item = item;
            Stats = item.Stats;
        }

        /// <summary>
        /// Single slot items are things like helms, neck, shoulders, hands, feet, etc. Dual slot items are rings, 1h weapons (dual wield)
        /// </summary>
        /// <returns></returns>
        public bool IsSingleSlotItem()
        {
            if (IsOneHand)
                return false;

            if (ItemType == Zeta.Game.Internals.Actors.ItemType.Ring)
                return false;

            return true;
        }

        public double GetToughness()
        {
            return 0d;
        }

        public double GetDamage()
        {
            return 0d;
        }

		public double GetHealing()
		{
			return 0d;
		}

		/// <summary>
		/// Calculate itemID from equipped weapons, for legendary handling
		/// </summary>
		/// <returns></returns>
		internal double ItemID(ACDItem item)
		{
			return item.GameBalanceId;
		}

	    /// <summary>
        /// Calculate base DPS from equipped weapons
        /// </summary>
        /// <returns></returns>
        internal double GetBaseDPS(ACDItem item) // Requires a full inventory read to properly detect attacks per second.
	    {
		    double dualWieldModifier = ((item.Stats.WeaponAttacksPerSecond) > 0 ? 1.15 : 1);
		    return ((item.Stats.MinDamage + item.Stats.MaxDamage + item.Stats.WeaponMinDamage + item.Stats.WeaponMaxDamage)/2)*
		           (item.Stats.WeaponAttacksPerSecond*dualWieldModifier);
        }

        /// <summary>
        /// Calculate Elemental Damage
        /// </summary>
        /// <returns></returns>
        //internal double ElementalDamageModifier(ACDItem item)
        //{
        //    return item.Stats.PoisonDamagePercent * 100 + item.Stats.ColdDamagePercent + item.Stats.FireDamagePercent 
        //        + item.Stats.LightningDamagePercent + item.Stats.ArcaneDamagePercent + item.Stats.HolyDamagePercent / 100;
        //}

        /// <summary>
        /// Calculate Additional DPS from Primary Stat
        /// </summary>
        /// <returns></returns>
        internal double PrimaryStat(ACDItem item)
        {
	        double primaryStat = 0;
	        switch (ZetaDia.Me.ActorClass)
			{
				case ActorClass.Barbarian:
                case ActorClass.Crusader:
					primaryStat = item.Stats.Strength;
					break;
				case ActorClass.Monk:
				case ActorClass.DemonHunter:
					primaryStat = item.Stats.Dexterity;
					break;
				case ActorClass.Wizard:
				case ActorClass.Witchdoctor:
					primaryStat = item.Stats.Intelligence;
					break;
			}
	        return primaryStat;
        }

		/// <summary>
		/// Calculate defencive effects of Strenght
		/// </summary>
		/// <returns></returns>
		internal double Strength(ACDItem item)
		{
			return item.Stats.Strength;
		}

		/// <summary>
		/// Calculate defencive effects of Strenght
		/// </summary>
		/// <returns></returns>
		internal double Dexterity(ACDItem item)
		{
			return item.Stats.Dexterity;
		}

		/// <summary>
		/// Calculate defencive effects of Strenght
		/// </summary>
		/// <returns></returns>
		internal double Intelligence(ACDItem item)
		{
			return item.Stats.Intelligence;
		}

		/// <summary>
		/// Calculate defencive effects of Strenght
		/// </summary>
		/// <returns></returns>
		internal double Vitality(ACDItem item)
		{
			return item.Stats.Vitality;
		}

        /// <summary>
        /// Calculate Additional DPS from generic Abilities
        /// </summary>
        /// <returns></returns>
        internal double AbilityMultiplier(ACDItem item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculate Base Damage
        /// </summary>
        /// <returns></returns>
        internal double BaseDamage(ACDItem item)
        {
			// Code seems redundant, math is already done in GetBaseDPS()
			return (item.Stats.MinDamage + item.Stats.MaxDamage + item.Stats.WeaponMinDamage + item.Stats.WeaponMaxDamage) / 2;
        }

		/// <summary>
		/// Check if the item is a weapon (to detect dual wield later on)
		/// </summary>
		/// <returns></returns>
		internal Boolean IsItemWeapon(ACDItem item)
		{
			// Code seems redundant, math is already done in GetBaseDPS()
			return item.Stats.WeaponAttacksPerSecond > 0 ? true : false;
		}

        /// <summary>
        /// Calculate Base Damage Percent
        /// </summary>
        /// <returns></returns>
        internal double BaseDamagePercent(ACDItem item)
        {
            return 0;
        }

        /// <summary>
        /// Calculate Crit Chance Percent
        /// </summary>
        /// <returns></returns>
        internal double CritChancePercent(ACDItem item)
        {
	        return item.Stats.CritPercent;
        }

        /// <summary>
        /// Calculate CritDamagePercent
        /// </summary>
        /// <returns></returns>
        internal double CritDamagePercent(ACDItem item)
        {
	        return item.Stats.CritDamagePercent;
        }

		/// <summary>
		/// Calculate LifeSteal%
		/// </summary>
		/// <returns></returns>
		internal double LifeStealPercent(ACDItem item)
		{
			return item.Stats.LifeSteal;
		}

		/// <summary>
		/// Calculate Armor points
		/// </summary>
		/// <returns></returns>
		internal double Armor(ACDItem item)
		{
			return item.Stats.Armor + item.Stats.ArmorBonus;
		}

		/// <summary>
		/// Calculate Resist All
		/// </summary>
		/// <returns></returns>
		internal double ResistAll(ACDItem item)
		{
			return item.Stats.ResistAll;
		}

		/// <summary>
		/// Calculate Life Perecent
		/// </summary>
		/// <returns></returns>
		internal double LifePercent(ACDItem item)
		{
			return item.Stats.LifePercent;
		}

        internal static HashSet<ItemType> OffHandTypes = new HashSet<ItemType>()
        {
            ItemType.Orb,
            ItemType.Mojo,
            ItemType.Quiver,
            ItemType.Shield,
            ItemType.CrusaderShield,
        };

        internal static HashSet<ItemType> ShieldTypes = new HashSet<ItemType>()
        {
            ItemType.Shield,
            ItemType.CrusaderShield,
        };

        internal static HashSet<ItemType> WeaponTypes = new HashSet<ItemType>()
        {
            ItemType.Axe,
            ItemType.Bow,
            ItemType.CeremonialDagger,
            ItemType.Crossbow,
            ItemType.Dagger,
            ItemType.Daibo,
            ItemType.FistWeapon,
            ItemType.Flail,
            ItemType.HandCrossbow,
            ItemType.Mace,
            ItemType.MightyWeapon,
            ItemType.Polearm,
            ItemType.Spear,
            ItemType.Staff,
            ItemType.Sword,
            ItemType.Wand,
        };

        internal static HashSet<ItemType> ArmorTypes = new HashSet<ItemType>()
        {
            ItemType.Belt,
            ItemType.Boots,
            ItemType.Bracer,
            ItemType.Chest,
            ItemType.Cloak,
            ItemType.Gloves,
            ItemType.Helm,
            ItemType.Legs,
            ItemType.MightyBelt,
            ItemType.Shoulder,
            ItemType.SpiritStone,
            ItemType.VoodooMask,
            ItemType.WizardHat,
        };

        internal static HashSet<ItemType> JewleryTypes = new HashSet<ItemType>()
        {
            ItemType.Amulet,
            ItemType.Ring,
        };

        internal static HashSet<ItemType> MiscTypes = new HashSet<ItemType>()
        {
            ItemType.CraftingPage,
            ItemType.CraftingPlan,
            ItemType.CraftingReagent,
            ItemType.FollowerSpecial,
            ItemType.Gem,
            ItemType.Potion,
            ItemType.Unknown,
        };
    }
}
