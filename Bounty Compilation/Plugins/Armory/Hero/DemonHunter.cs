using System.Collections.Generic;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
namespace Armory.Hero
{
    internal class DemonHunter : HeroClass
    {
        internal override List<ItemType> EquippableItemTypes
        {
            get { return equippableItemTypes; }
        }

        private static readonly List<ItemType> equippableItemTypes = new List<ItemType>()
        { 
            ItemType.Amulet,
            ItemType.Belt,
            ItemType.Boots,
            ItemType.Bow,
            ItemType.Bracer,
            ItemType.Chest,
            ItemType.Cloak,
            ItemType.Crossbow,
            ItemType.Gloves,
            ItemType.HandCrossbow,
            ItemType.Helm,
            ItemType.Legs,
            ItemType.Quiver,
            ItemType.Ring,
            ItemType.Shoulder,
        };

        internal override List<ItemType> UnEquippableItemTypes
        {
            get { return unEquippableItemTypes; }
        }
        private static readonly List<ItemType> unEquippableItemTypes = new List<ItemType>()
        {
            ItemType.Axe,
            ItemType.CeremonialDagger,
            ItemType.Dagger,
            ItemType.Daibo,
            ItemType.FistWeapon,
            ItemType.Mace,
            ItemType.MightyBelt,
            ItemType.MightyWeapon,
            ItemType.Mojo,
            ItemType.Orb,
            ItemType.Polearm,
            ItemType.Shield,
            ItemType.Spear,
            ItemType.Staff,
            ItemType.Sword,
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

        internal override double Dexterity
        {
            get { return 10 * (Level + ParagonLevel); }
        }
    }
}