using System.Collections.Generic;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
namespace Armory.Hero
{
    internal class Barbarian : HeroClass
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
  
            ItemType.MightyBelt,
            ItemType.MightyWeapon,

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
            ItemType.Mojo,
            ItemType.Orb,
            ItemType.Quiver,
            ItemType.SpiritStone,
            ItemType.VoodooMask,
            ItemType.Wand,
            ItemType.WizardHat,
            ItemType.CrusaderShield,
            ItemType.Flail,

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
            return EquippableItemTypes.Contains(item.ItemType);
        }

        internal override double Strength
        {
            get { return 10 * (Level + ParagonLevel); }
        }

    }
}