using System;
using System.Linq;
using Zeta.Game;

namespace Armory
{
    public class Player
    {
        public static bool HasTwoHandEquipped { get; set; }
        public static bool HasOneHandEquipped { get; set; }
        public static bool HasShieldEquipped { get; set; }
        public static bool IsDualWielding { get; set; }
        public static int Level { get; set; }
        public static int ParagonLevel { get; set; }

        public static bool IsValid
        {
            get
            {
                return ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Me != null && ZetaDia.Me.IsValid;
            }
        }

        private static DateTime lastUpdate = DateTime.MinValue;
        public static void Update()
        {
            if (!IsValid)
                return;

            if (DateTime.UtcNow.Subtract(lastUpdate).TotalMilliseconds < 500)
                return;
            lastUpdate = DateTime.UtcNow;

            HasTwoHandEquipped = ZetaDia.Me.Inventory.Equipped.Any(i => i.IsTwoHand && i.InventorySlot == InventorySlot.LeftHand);
            HasOneHandEquipped = ZetaDia.Me.Inventory.Equipped.Any(i => i.IsOneHand && i.InventorySlot == InventorySlot.LeftHand);
            HasShieldEquipped = ZetaDia.Me.Inventory.Equipped.Any(i => i.ItemType == Zeta.Game.Internals.Actors.ItemType.CrusaderShield || i.ItemType == Zeta.Game.Internals.Actors.ItemType.Shield);
            IsDualWielding = ZetaDia.Me.Inventory.Equipped.Count(i => i.IsOneHand) == 2;
            Level = ZetaDia.Me.Level;
            ParagonLevel = ZetaDia.Me.ParagonLevel;
        }
    }
}
