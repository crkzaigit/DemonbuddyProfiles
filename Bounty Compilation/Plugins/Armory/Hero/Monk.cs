using System.Collections.Generic;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
namespace Armory.Hero
{
    internal class Monk : HeroClass
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
            ItemType.Daibo,
            ItemType.FistWeapon,
            ItemType.Gloves,
            ItemType.Helm,
            ItemType.Legs,
            ItemType.Mace,
            ItemType.Polearm,
            ItemType.Ring,
            ItemType.Shield,
            ItemType.Shoulder,
            ItemType.Spear,
            ItemType.SpiritStone,
            ItemType.Staff,
            ItemType.Sword,
        };

        internal override List<ItemType> UnEquippableItemTypes
        {
            get { return unEquippableItemTypes; }
        }
        private static readonly List<ItemType> unEquippableItemTypes = new List<ItemType>()
        {
            ItemType.Bow,
            ItemType.CeremonialDagger,
            ItemType.HandCrossbow,
            ItemType.Cloak,
            ItemType.Crossbow,
            ItemType.Mojo,
            ItemType.Orb,
            ItemType.MightyBelt,
            ItemType.MightyWeapon,
            ItemType.Quiver,
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