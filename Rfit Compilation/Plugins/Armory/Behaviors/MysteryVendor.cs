using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Armory.Settings;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Armory.Behaviors
{
    public class MysteryVendor
    {
        // Kadala ActorId
        private const int X1RandomItemNpc = 361241;

        public enum MysteryItemType
        {
            MysteryWeapon_1H,
            MysteryWeapon_2H,
            MysteryAmulet,
            MysteryBelt,
            MysteryBoots,
            MysteryBracers,
            MysteryChestArmor,
            MysteryGloves,
            MysteryHelm,
            MysteryMojo,
            MysteryOrb,
            MysteryPants,
            MysteryQuiver,
            MysteryRing,
            MysteryShield,
            MysteryShoulders,
        }

        public enum PuchaseMode
        {
            Ordered,
            Random
        }

        /// <summary>
        /// List of ActorSNO's for each Mystery Item Type
        /// </summary>
        public static Dictionary<MysteryItemType, int> MysteryItemIds = new Dictionary<MysteryItemType, int>
        {
            {MysteryItemType.MysteryWeapon_1H,   377355}, 
            {MysteryItemType.MysteryWeapon_2H,   377356}, 
            {MysteryItemType.MysteryAmulet,      377353}, 
            {MysteryItemType.MysteryBelt,        377349}, 
            {MysteryItemType.MysteryBoots,       377347}, 
            {MysteryItemType.MysteryBracers,     377351}, 
            {MysteryItemType.MysteryChestArmor,  377345}, 
            {MysteryItemType.MysteryGloves,      377346}, 
            {MysteryItemType.MysteryHelm,        377344}, 
            {MysteryItemType.MysteryMojo,        377359}, 
            {MysteryItemType.MysteryOrb,         377358}, 
            {MysteryItemType.MysteryPants,       377350}, 
            {MysteryItemType.MysteryQuiver,      377360}, 
            {MysteryItemType.MysteryRing,        377352}, 
            {MysteryItemType.MysteryShield,      377357}, 
            {MysteryItemType.MysteryShoulders,   377348},
        };

        public static Dictionary<MysteryItemType, int> MysteryItemCosts = new Dictionary<MysteryItemType, int>
        {
            {MysteryItemType.MysteryWeapon_1H,   15}, 
            {MysteryItemType.MysteryWeapon_2H,   15}, 
            {MysteryItemType.MysteryAmulet,      20}, 
            {MysteryItemType.MysteryBelt,         5}, 
            {MysteryItemType.MysteryBoots,        5}, 
            {MysteryItemType.MysteryBracers,      5}, 
            {MysteryItemType.MysteryChestArmor,   5}, 
            {MysteryItemType.MysteryGloves,       5}, 
            {MysteryItemType.MysteryHelm,         5}, 
            {MysteryItemType.MysteryMojo,         5}, 
            {MysteryItemType.MysteryOrb,          5}, 
            {MysteryItemType.MysteryPants,        5}, 
            {MysteryItemType.MysteryQuiver,       5}, 
            {MysteryItemType.MysteryRing,        10}, 
            {MysteryItemType.MysteryShield,       5}, 
            {MysteryItemType.MysteryShoulders,    5},
        };

        private static DiaUnit _randomItemVendor;
        private static DiaUnit RandomItemVendor
        {
            get
            {
                if (_randomItemVendor != null && _randomItemVendor.IsValid)
                    return _randomItemVendor;

                _randomItemVendor =
                (from u in ZetaDia.Actors.GetActorsOfType<DiaUnit>(true)
                 where u.IsValid && u.ActorSNO == X1RandomItemNpc
                 select u).FirstOrDefault();
                return _randomItemVendor;
            }
        }

        private static Vector3 MyPosition { get { return ZetaDia.Me.Position; } }

        private static DateTime _lastCheckedVendor = DateTime.MinValue;
        public static Composite CreateBehavior()
        {
            return
            new Decorator(ret => CanRunMysteryVendor(),
                new Action(ret => MysteryVendorAction())
            );
        }

        private static RunStatus MysteryVendorAction()
        {
            if (MyPosition.Distance2DSqr(RandomItemVendor.Position) >= 8f * 8f)
            {
                Logger.Log("Moving to Kadala");
                Navigator.MoveTo(RandomItemVendor.Position);
                return RunStatus.Running;
            }
            if (!IsMerchantWindowOpen)
            {
                Logger.Log("Opening Kadala Window");
                RandomItemVendor.Interact();
                return RunStatus.Running;
            }
            return BuyItems();
        }


        private static bool CanRunMysteryVendor()
        {
            return ArmorySettings.Instance.EnableMysteryVendor && ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Me.IsValid && ZetaDia.IsInTown && 
                RandomItemVendor != null && RandomItemVendor.IsValid &&
                ZetaDia.CPlayer.BloodshardCount > ArmorySettings.Instance.MysteryItemSlots.MinShardCount &&
                GetAllowedItemIds().Count > 0;
        }

        public static bool IsMerchantWindowOpen
        {
            get
            {
                var merchantWindowElement = UIElement.FromName("Root.NormalLayer.shop_dialog_mainPage");

                if (merchantWindowElement != null && merchantWindowElement.IsValid && merchantWindowElement.IsVisible)
                    return true;

                return false;
            }
        }

        private static int _itemIndex = -1;
        private static Random _randomIndexer;
        public static RunStatus BuyItems()
        {
            if (ZetaDia.CPlayer.BloodshardCount == 0)
                return RunStatus.Failure;

            if (!HasFreeBackPackSlots())
                return RunStatus.Failure;

            if (!IsMerchantWindowOpen)
                return RunStatus.Failure;

            if (_randomIndexer == null)
                _randomIndexer = new Random(DateTime.UtcNow.Millisecond);

            List<int> allowedIds = GetAllowedItemIds();

            var items = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).Where(i => i.IsValid && allowedIds.Contains(i.ActorSNO)).OrderBy(i => i.ActorSNO).ToList();

            if (items.Count == 0)
                return RunStatus.Failure;

            if (MysteryItemSlots.Instance.RandomOrder)
            {
                _itemIndex = _randomIndexer.Next(0, items.Count - 1);
            }
            else
            {
                _itemIndex++;
                if (_itemIndex >= items.Count)
                    _itemIndex = 0;
            }

            var item = items[_itemIndex];

            if (item.IsValid)
            {
                Logger.Log("Buying {0}", item.Name);
                ZetaDia.Me.Inventory.BuyItem(item.DynamicId);
            }

            return RunStatus.Running;
        }

        private static List<int> GetAllowedItemIds()
        {
            List<int> allowedIds = new List<int>();

            int currentShardCount = ZetaDia.CPlayer.BloodshardCount;

            if (MysteryItemSlots.Instance.OneHand && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryWeapon_1H])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryWeapon_1H]);

            if (MysteryItemSlots.Instance.TwoHand && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryWeapon_2H])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryWeapon_2H]);

            if (MysteryItemSlots.Instance.Neck && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryAmulet])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryAmulet]);

            if (MysteryItemSlots.Instance.Waist && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryBelt])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryBelt]);

            if (MysteryItemSlots.Instance.Feet && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryBoots])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryBoots]);

            if (MysteryItemSlots.Instance.Wrists && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryBracers])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryBracers]);

            if (MysteryItemSlots.Instance.Chest && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryChestArmor])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryChestArmor]);

            if (MysteryItemSlots.Instance.Helm && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryHelm])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryHelm]);

            if (MysteryItemSlots.Instance.Hands && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryGloves])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryGloves]);

            if (MysteryItemSlots.Instance.Shoulders && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryShoulders])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryShoulders]);

            if (MysteryItemSlots.Instance.Ring && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryRing])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryRing]);

            if (MysteryItemSlots.Instance.Legs && currentShardCount >= MysteryItemCosts[MysteryItemType.MysteryPants])
                allowedIds.Add(MysteryItemIds[MysteryItemType.MysteryPants]);

            if (MysteryItemSlots.Instance.OffHand && GetOffHandId() != 0 && currentShardCount >= 5)
                allowedIds.Add(GetOffHandId());
            return allowedIds;
        }

        public static bool HasFreeBackPackSlots()
        {
            bool[,] slotsUsed = new bool[10, 6];

            foreach (var item in ZetaDia.Me.Inventory.Backpack.Where(i => i.IsValid))
            {
                int x = item.InventoryColumn, y = item.InventoryRow;
                slotsUsed[x, y] = true;

                if (item.IsTwoSquareItem)
                {
                    slotsUsed[x, y + 1] = true;
                }
            }

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 5; y++) // Only checking 5 rows, for 2 slot items
                {
                    if (slotsUsed[x, y])
                        continue;

                    if (slotsUsed[x, y + 1])
                        continue;

                    return true;
                }
            }
            // Could not find space for a 2 slot item
            return false;
        }

        public static int GetOffHandId()
        {
            int id = 0;
            switch (ZetaDia.Me.ActorClass)
            {
                case ActorClass.Barbarian:
                case ActorClass.Crusader:
                case ActorClass.Monk:
                    id = MysteryItemIds[MysteryItemType.MysteryShield];
                    break;
                case ActorClass.DemonHunter:
                    id = MysteryItemIds[MysteryItemType.MysteryQuiver];
                    break;
                case ActorClass.Witchdoctor:
                    id = MysteryItemIds[MysteryItemType.MysteryMojo];
                    break;
                case ActorClass.Wizard:
                    id = MysteryItemIds[MysteryItemType.MysteryOrb];
                    break;
            }
            return id;
        }

    }
}
