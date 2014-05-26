using System.Collections.Generic;
using System.Linq;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Armory.Hero
{
    internal class Crusader : HeroClass
    {
        internal override List<ItemType> EquippableItemTypes
        {
            get { return equippableItemTypes; }
        }

        private static readonly List<ItemType> equippableItemTypes = new List<ItemType>()
        { 
            ItemType.Amulet,
            ItemType.Axe,
            ItemType.Belt,
            ItemType.Boots,
            ItemType.Bracer,
            ItemType.Chest,
            ItemType.Dagger,
            ItemType.Gloves,
            ItemType.Helm,
            ItemType.Legs,
            ItemType.Mace,
            ItemType.Polearm,
            ItemType.Ring,
            ItemType.Shield,
            ItemType.Shoulder,
            ItemType.Spear,
            ItemType.Staff,
            ItemType.Sword,

            ItemType.CrusaderShield,
            ItemType.Flail,
        };

        internal override List<ItemType> UnEquippableItemTypes
        {
            get { return unEquippableItemTypes; }
        }
        private static readonly List<ItemType> unEquippableItemTypes = new List<ItemType>()
        {
            ItemType.Bow,
            ItemType.CeremonialDagger,
            ItemType.Cloak,
            ItemType.Crossbow,
            ItemType.Daibo,
            ItemType.FistWeapon,
            ItemType.HandCrossbow,
            ItemType.MightyBelt,
            ItemType.MightyWeapon,
            ItemType.Mojo,
            ItemType.Orb,
            ItemType.Quiver,
            ItemType.SpiritStone,
            ItemType.VoodooMask,
            ItemType.Wand,
            ItemType.WizardHat,

            ItemType.CraftingPage,
            ItemType.CraftingPlan,
            ItemType.CraftingReagent,
            ItemType.FollowerSpecial,
            ItemType.Gem,
            ItemType.Potion,
            ItemType.Unknown,
        };

        internal override bool CanEquipItem(ItemWrapper item, InventorySlot slot)
        {
            bool hasHevenlyStrength = HeroClass.PassiveSkills.Contains(SNOPower.X1_Crusader_Passive_HeavenlyStrength);
            
            // We're a crusader and do NOT have Heavenly Strength
            if (!(hasHevenlyStrength && (item.IsTwoHand || item.IsShield)))
            {
                // Don't try to equip shields if we have a 2H equipped
                if (item.IsShield && Player.HasTwoHandEquipped)
                    return false;

                // Don't try to equip 1H into RightHand if we have a 2H equipped
                if (Player.HasTwoHandEquipped && slot == InventorySlot.RightHand)
                    return false;
            }

            if (item.IsOneHand && slot == InventorySlot.RightHand)
                return false;

            bool isCrusaderItem = false;

            // Hax
            string internalName = item.InternalName.ToLower().Replace("x1_", "");
            if (CrusaderItemNames.Any(n => internalName.Contains(n)))
                isCrusaderItem = true;

            return isCrusaderItem || EquippableItemTypes.Contains(item.ItemType);
        }

        internal override double Strength
        {
            get { return 10 * (Level + ParagonLevel); }
        }

        private static HashSet<string> CrusaderItemNames = new HashSet<string>()
        {
            "flail", "crushield",
        };
    }
}