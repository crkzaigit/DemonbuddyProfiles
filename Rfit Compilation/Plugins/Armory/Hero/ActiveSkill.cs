using System.Linq;
using Zeta.Game.Internals.Actors;

namespace Armory.Hero
{
    /// <summary>
    ///     Represents an actively equipped skill
    /// </summary>
    internal class ActiveSkill
    {
        public ActiveSkill()
        {
            Power = SNOPower.None;
            RuneIndex = -999;
            Slot = HotbarSlot.Invalid;
        }

        public ActiveSkill(SNOPower power, int runeIndex, HotbarSlot slot)
        {
            Power = power;
            RuneIndex = runeIndex;
            Slot = slot;
        }

        public SNOPower Power { get; set; }
        public int RuneIndex { get; set; }
        public HotbarSlot Slot { get; set; }

        /// <summary>
        ///     Creates an ActiveSkill from a HotbarSlot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static ActiveSkill GetSkillFromSlot(HotbarSlot slot)
        {
            SNOPower power = HeroClass.GetPowerFromSlot(slot);
            if (power == SNOPower.None)
                return new ActiveSkill();

            int runeIndex = HeroClass.GetRuneIndexFromSlot(slot);

            return new ActiveSkill
            {
                Power = power,
                RuneIndex = runeIndex,
                Slot = slot
            };
        }


        /// <summary>
        ///     Gets an ActiveSkill from an equipped Power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static ActiveSkill GetSkillFromPower(SNOPower power)
        {
            if (!HeroClass.ActiveSkills.Any(s => s == power))
                return new ActiveSkill();

            HotbarSlot slot = HeroClass.GetSlotFromPower(power);
            int runeIndex = HeroClass.GetRuneIndexFromSlot(slot);

            return new ActiveSkill
            {
                Power = power,
                RuneIndex = runeIndex,
                Slot = slot
            };
        }
    }
}