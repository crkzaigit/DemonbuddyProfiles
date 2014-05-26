using System;
using System.Collections.Generic;
using System.Linq;
using Armory.Settings;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Armory
{
    internal class ItemEvaluator
    {
        private static readonly HashSet<int> DequippedItems = new HashSet<int>();
        private static DateTime _lastEvaluatedItems = DateTime.MinValue;
        private static readonly HashSet<Tuple<InventorySlot, int>> CheckedItems = new HashSet<Tuple<InventorySlot, int>>();

        private const int CheckIntervalMillseconds = 1000;

        private static int _lastBackpackCount = -1;
        private static int _lastStashCount = -1;

        private static readonly HashSet<InventorySlot> HirelingSlots = new HashSet<InventorySlot>
        {
            InventorySlot.HirelingLeftFinger,
            InventorySlot.HirelingLeftHand,
            InventorySlot.HirelingNeck,
            InventorySlot.HirelingRightFinger,
            InventorySlot.HirelingRightHand,
            InventorySlot.HirelingSpecial
        };

        /// <summary>
        /// Iterates through 
        /// </summary>
        public static void EvaluateItems()
        {
            if (!ArmorySettings.Instance.EnableEquipper)
                return;

            if (!Player.IsValid)
                return;

            if (Armory.ActiveHero == null)
                return;

            //Only pulse out of combat
            if (Zeta.Bot.CombatTargeting.Instance.FirstObject != null)
                return;

            if (DateTime.UtcNow.Subtract(_lastEvaluatedItems).TotalMilliseconds < CheckIntervalMillseconds && !GameUI.ElementIsVisible(GameUI.StashTab1))
                return;

            _lastEvaluatedItems = DateTime.UtcNow;

            // Stash isn't open
            if (!GameUI.ElementIsVisible(GameUI.StashTab1))
            {
                int backpackCount = ZetaDia.Me.Inventory.Backpack.Count();
                if (_lastBackpackCount == backpackCount)
                {
                    return;
                }
                _lastBackpackCount = backpackCount;
            }
            // Stash is open
            else
            {
                int stashCount = ZetaDia.Me.Inventory.StashItems.Count();
                if (_lastStashCount == stashCount)
                {
                    return;
                }
                _lastStashCount = stashCount;
            }

            // counts are different for backpack or stash, continue

            Player.Update();

            // Iterate through every possible inventory slot
            foreach (InventorySlot slot in Enum.GetValues(typeof(InventorySlot)).Cast<InventorySlot>())
            {
                if (HirelingSlots.Contains(slot))
                    continue;

                if (slot == InventorySlot.None)
                    continue;

                ACDItem item;
                if (!EvaluateItemSlot(slot, out item))
                    continue;

                if (item == null || !item.IsValid)
                    continue;

                EquipItem(slot, item);
            }
        }

        private static void EquipItem(InventorySlot slot, ACDItem item)
        {
            DequippedItems.Add(item.ACDGuid);
            Logger.Log("Equipping item {0} in slot {1}", item.Name, slot);
            ZetaDia.Me.Inventory.EquipItem(item.DynamicId, slot);
            Player.Update();
        }

        public static bool EvaluateItemSlot(InventorySlot slot, out ACDItem foundItem)
        {
            foundItem = default(ACDItem);

            IEnumerable<ACDItem> itemsList;
            if (Settings.CheckStash && GameUI.ElementIsVisible(GameUI.StashTab1))
            {
                itemsList = ZetaDia.Me.Inventory.StashItems;
            }
            else
            {
                itemsList = ZetaDia.Me.Inventory.Backpack;
            }

            itemsList = (from i in itemsList
                         where
                         !CheckedItems.Contains(new Tuple<InventorySlot, int>(slot, i.ACDGuid)) &&
                         !DequippedItems.Contains(i.ACDGuid) &&
                         !i.IsUnidentified &&
                         i.ValidInventorySlots.Contains(slot)
                         select i).ToList();

            foreach (ACDItem item in itemsList)
            {
                if (ShouldEquipItem(slot, item))
                {
                    foundItem = item;
                    return true;
                }
            }
            return false;
        }

        public static bool ShouldEquipItem(InventorySlot slot, ACDItem acdItem)
        {
            ItemWrapper item = new ItemWrapper(acdItem);

            CheckedItems.Add(new Tuple<InventorySlot, int>(slot, item.ACDGuid));

            // Two handed weapons can never go into RightHand
            if (slot == InventorySlot.RightHand && item.IsTwoHand)
                return false;

            if (!Settings.AllowTwoHand && item.IsTwoHand)
                return false;

            if (!Settings.AllowShields && item.IsShield)
                return false;

            if (!Settings.DualWield && item.IsOneHand && slot == InventorySlot.RightHand)
                return false;

            if (Settings.DisableAt70 && Player.Level >= 70)
                return false;

            if (IsSlotProtected(slot))
                return false;

            // Is an equippable item type for our Hero?
            if (!Armory.ActiveHero.CanEquipItem(item, slot))
                return false;

            // We meet the level requirements?
            // This shit is broken in DB/D3 memory reading. Between levels 61-70 it'll report weird numbers!
            //if (acdItem.Level > ZetaDia.Me.Level)
            //{
            //    Logger.Debug("We don't meet the level requirements for {0} ({1})", item.Name, acdItem.Level);
            //    return false;
            //}

            // Check if our slot is empty
            bool isSlotEmpty = ZetaDia.Me.Inventory.Equipped.All(i => i.InventorySlot != slot);

            bool isSlotDamaged = !isSlotEmpty && ZetaDia.Me.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == slot).DurabilityCurrent < 3;
            if (isSlotDamaged)
                return false;

            if (isSlotEmpty && !item.IsShield && !Player.HasTwoHandEquipped)
            {
                Logger.Log("{0} is an Upgrade -  Slot is Empty", item.Name);
                return true;
            }

            float damage, toughness, healing;
            Logger.Debug("Evaluating {0} - {1}", slot, item.Name);

            if (slot == InventorySlot.RightFinger)
            {
                // Alt item slot for Right Ring finger
                acdItem.GetStatChanges(out damage, out healing, out toughness, true);
            }
            else if (slot == InventorySlot.RightHand && !Player.HasTwoHandEquipped && (Player.IsDualWielding || isSlotEmpty))
            {
                // Alt Item Slot for Right Hand if dual wielding or slot is empty and no 2H equipped
                acdItem.GetStatChanges(out damage, out healing, out toughness, true);
            }
            else
            {
                acdItem.GetStatChanges(out damage, out healing, out toughness);
            }

            bool isUpgrade = CalculateUpgrade(damage, toughness, healing);


            if (isUpgrade)
            {
                Logger.Log("{0} is an Upgrade! Damage={1:0.0}% Toughness={2:0.0}% Healing={3:0.0}%", item.Name, damage * 100, toughness * 100, healing * 100);
            }
            return isUpgrade;
        }

        private static bool CalculateUpgrade(float damage, float toughness, float healing)
        {
            bool isUpgrade = false;

            // Additive Damage Formula - Damage reduced up to 5% but toughness compensates
            if (Settings.UseAdditiveDamageFormula &&
                damage > Settings.AdditiveDamageFormulaDamage &&
                damage + toughness > Settings.AdditiveDamageFormulaDamageToughness)
                isUpgrade = true;

            // Healing Formula - Damage reduced max 15% but toughness buff is "good enough" and healing is > 0%
            if (Settings.UseHealingUpgradeFormula &&
                damage > Settings.HealingUpgradeFormulaDamage &&
                damage + toughness > Settings.HealingUpgradeFormulaDamageToughness &&
                healing > Settings.HealingUpgradeFormulaHealing)
                isUpgrade = true;

            // Basic Upgrade Formula
            if (Settings.UseBasicUpgradeFormula &&
                damage > Settings.BasicUpgradeFormulaDamage &&
                toughness > Settings.BasicUpgradeFormulaToughness)
                isUpgrade = true;

            /*
             *  Want to DIY? Uncomment below as needed!
             */
            // Additive Damage Formula - Damage reduced up to 5% but toughness compensates
            //if (damage > -0.05f && damage + toughness > 0)
            //    isUpgrade = true;

            // Healing Formula - Damage reduced max 15% but toughness buff is "good enough" and healing is > 0%
            //if (damage > -0.15f && damage + toughness > 0 && healing > 0)
            //    isUpgrade = true;

            // Basic Upgrade Formula
            //if (damage > 0 && toughness > 0)
            //    isUpgrade = true;

            return isUpgrade;
        }

        private static ArmorySettings Settings
        {
            get { return ArmorySettings.Instance; }
        }

        private static bool IsSlotProtected(InventorySlot slot)
        {
            switch (slot)
            {
                default:
                    return false;
                case InventorySlot.Head:
                    return Settings.ProtectedSlots.Helm;
                case InventorySlot.Torso:
                    return Settings.ProtectedSlots.Chest;
                case InventorySlot.RightHand:
                    return Settings.ProtectedSlots.OffHand;
                case InventorySlot.LeftHand:
                    return Settings.ProtectedSlots.MainHand;
                case InventorySlot.Hands:
                    return Settings.ProtectedSlots.Hands;
                case InventorySlot.Waist:
                    return Settings.ProtectedSlots.Waist;
                case InventorySlot.Feet:
                    return Settings.ProtectedSlots.Feet;
                case InventorySlot.Shoulders:
                    return Settings.ProtectedSlots.Shoulders;
                case InventorySlot.Legs:
                    return Settings.ProtectedSlots.Legs;
                case InventorySlot.Bracers:
                    return Settings.ProtectedSlots.Wrists;
                case InventorySlot.RightFinger:
                    return Settings.ProtectedSlots.Ring2;
                case InventorySlot.LeftFinger:
                    return Settings.ProtectedSlots.Ring1;
                case InventorySlot.Neck:
                    return Settings.ProtectedSlots.Neck;
            }
        }

        public static void Reset()
        {
            _lastBackpackCount = 0;
            _lastStashCount = 0;
            DequippedItems.Clear();
            _lastEvaluatedItems = DateTime.MinValue;
            CheckedItems.Clear();
        }

        public static readonly List<ItemType> GenericEquippableItemTypes = new List<ItemType>
        {
            ItemType.Amulet,
            ItemType.Belt,
            ItemType.Boots,
            ItemType.Bracer,
            ItemType.Chest,
            ItemType.Gloves,
            ItemType.Helm,
            ItemType.Legs,
            ItemType.Ring,
            ItemType.Shield,
            ItemType.Shoulder,
        };

        public static readonly List<ItemType> GenericClassItemTypes = new List<ItemType>
        {
            ItemType.Axe,
            ItemType.Bow,
            ItemType.CeremonialDagger,
            ItemType.Cloak,
            ItemType.Crossbow,
            ItemType.Dagger,
            ItemType.Daibo,
            ItemType.FistWeapon,
            ItemType.HandCrossbow,
            ItemType.Mace,
            ItemType.MightyBelt,
            ItemType.MightyWeapon,
            ItemType.Mojo,
            ItemType.Orb,
            ItemType.Polearm,
            ItemType.Quiver,
            ItemType.Spear,
            ItemType.SpiritStone,
            ItemType.Staff,
            ItemType.Sword,
            ItemType.VoodooMask,
            ItemType.Wand,
            ItemType.WizardHat,
        };

        public static readonly List<ItemType> GenericUnEquippableItemTypes = new List<ItemType>
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