﻿
namespace Trinity.Config.Combat
{
    public enum GoblinPriority
    {
        Ignore = 0,
        Normal = 1,
        Prioritize = 2,
        Kamikaze = 3
    }
    public enum TempestRushOption
    {
        MovementOnly = 0,
        ElitesGroupsOnly = 1,
        CombatOnly = 2,
        Always = 3,
        TrashOnly = 4,
    }
    public enum WizardKiteOption
    {
        Anytime,
        ArchonOnly,
        NormalOnly
    }

    public enum WizardArchonCancelOption
    {
        Never,
        Timer,
        RebuffArmor,
        RebuffMagicWeaponFamiliar,
    }

    public enum DestructibleIgnoreOption
    {
        ForceIgnore,
        OnlyIfStuck,
        DestroyAll
    }

    public enum BarbarianWOTBMode
    {
        HardElitesOnly,
        Normal,
        WhenReady
    }

    public enum BarbarianSprintMode
    {
        Always,
        CombatOnly,
        MovementOnly
    }

    public enum DemonHunterVaultMode
    {
        Always,
        CombatOnly,
        MovementOnly
    }

    public enum KiteMode
    {
        Never,
        Always,
        Bosses,
        Elites,
    }

    public enum TrinityItemQuality
    {
        Invalid = -1,
        None = 0,
        Inferior,
        Common,
        Magic,
        Rare,
        Legendary,
        Set
    }
}
