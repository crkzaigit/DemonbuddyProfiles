using System;

namespace Belphegor.Utilities
{
    [Flags]
    public enum BehaviorType
    {
        Buff = 0x1,
        Movement = 0x2,
        Pull = 0x8,
        Combat = 0x40,

        All = Buff | Pull | Combat
    }

    public enum Pet
    {
        MysticAlly,
        Gargantuan,
        ZombieDogs,
        DH_Companion,
        DH_Sentry,
        DH_Caltrops,
        DH_SpikeTrap,
        Hydra,
        Enchantress,
        Templer,
        Scoundrel,
    }
}