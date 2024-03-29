﻿using System;

namespace Trinity.Config.Loot
{
    public enum SalvageOption
    {
        Sell,
        Salvage,
        None,
        InfernoOnly,
        All
    }

    public enum ItemRuleType
    {
        Config,
        Soft,
        Hard,
        Custom
    }

    public enum ItemFilterMode
    {
        TrinityOnly,
        TrinityWithItemRules,
        DemonBuddy
    }

    [Flags]
    public enum TrinityGemType
    {
        None = 1, 
        Emerald = 2,
        Topaz = 4,
        Amethyst = 8,
        Ruby = 16,
        Diamond = 32,
        All = Emerald | Topaz | Amethyst | Ruby | Diamond
    }

    public enum ItemRuleLogLevel
    {
        None = 0,
        Normal = 1,
        Magic = 2,
        Rare = 3,
        Legendary = 4
    }
}
