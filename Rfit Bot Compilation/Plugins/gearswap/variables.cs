using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using Zeta;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;


namespace GearSwap
{
    partial class Plugin : IPlugin
    {
        public static WaitTimer _pulseTimer = new WaitTimer(TimeSpan.FromMilliseconds(pulseInterval * 1000));
        public static WaitTimer _30SecTimer = new WaitTimer(TimeSpan.FromSeconds(30));
        public static WaitTimer _archewTimer = new WaitTimer(TimeSpan.FromSeconds(31));
        public static WaitTimer _beckonTimer = new WaitTimer(TimeSpan.FromSeconds(31));
        public static WaitTimer _lowHealthDuration = new WaitTimer(TimeSpan.FromMilliseconds(lowHealthDuration * 1000));

        public static List<Gear> gearList = new List<Gear>();
        public static List<Gear> originalGear = new List<Gear>();
        public static List<Condition> statuses = new List<Condition>();
        public static List<Condition> defaultStatuses = new List<Condition>();
        public static bool priorityUpdated = false;
        public static bool debugLoggingEnabled = false;
        public static bool DebugLoggingEnabled { get { return debugLoggingEnabled; } set { debugLoggingEnabled = value; } }
        public static bool loggingEnabled = true;
        public static bool LoggingEnabled { get { return loggingEnabled; } set { loggingEnabled = value; } }
        public static int lowHealthPerc = 40;
        public static int LowHealthPerc { get { return lowHealthPerc; } set { lowHealthPerc = value; } }
        public static int magicFindPerc = 15;
        public static int MagicFindPerc { get { return magicFindPerc; } set { magicFindPerc = value; } }
        public static int shiMizuPerc = 25;
        public static int ShiMizuPerc { get { return shiMizuPerc; } set { shiMizuPerc = value; } }
        public static double barricadeDistance = 5;
        public static double BarricadeDistance { get { return barricadeDistance; } set { barricadeDistance = value; } }
        public static bool coldEnabled = true;
        public static bool ColdEnabled { get { return coldEnabled; } set { coldEnabled = value; } }
        public static bool fireEnabled = true;
        public static bool FireEnabled { get { return fireEnabled; } set { fireEnabled = value; } }
        public static bool arcaneEnabled = true;
        public static bool ArcaneEnabled { get { return arcaneEnabled; } set { arcaneEnabled = value; } }
        public static bool poisonEnabled = true;
        public static bool PoisonEnabled { get { return poisonEnabled; } set { poisonEnabled = value; } }
        public static bool lightningEnabled = true;
        public static bool LightningEnabled { get { return lightningEnabled; } set { lightningEnabled = value; } }
        public static bool eliteEnabled = true;
        public static bool EliteEnabled { get { return eliteEnabled; } set { eliteEnabled = value; } }
        public static bool shrineEnabled = true;
        public static bool ShrineEnabled { get { return shrineEnabled; } set { shrineEnabled = value; } }
        public static bool wellEnabled = true;
        public static bool WellEnabled { get { return wellEnabled; } set { wellEnabled = value; } }
        public static bool lowHealthEnabled = true;
        public static bool LowHealthEnabled { get { return lowHealthEnabled; } set { lowHealthEnabled = value; } }
        public static bool criticalHealthEnabled = true;
        public static bool CriticalHealthEnabled { get { return criticalHealthEnabled; } set { criticalHealthEnabled = value; } }
        public static bool threeEnemiesEnabled = true;
        public static bool ThreeEnemiesEnabled { get { return threeEnemiesEnabled; } set { threeEnemiesEnabled = value; } }
        public static bool fiveEnemiesEnabled = true;
        public static bool FiveEnemiesEnabled { get { return fiveEnemiesEnabled; } set { fiveEnemiesEnabled = value; } }
        public static bool magicFindEnabled = true;
        public static bool MagicFindEnabled { get { return magicFindEnabled; } set { magicFindEnabled = value; } }
        public static bool barricadeEnabled = true;
        public static bool BarricadeEnabled { get { return barricadeEnabled; } set { barricadeEnabled = value; } }
        public static bool immmuneEnabled = true;
        public static bool ImmmuneEnabled { get { return immmuneEnabled; } set { immmuneEnabled = value; } }
        public static bool chestEnabled = true;
        public static bool ChestEnabled { get { return chestEnabled; } set { chestEnabled = value; } }
        public static double lowHealthDuration = 4;
        public static double LowHealthDuration { get { return lowHealthDuration; } set { lowHealthDuration = value; } }
        public static double pulseInterval = .5;
        public static double PulseInterval { get { return pulseInterval; } set { pulseInterval = value; } }
        public static bool puzzleRingEnabled = true;
        public static bool PuzzleRingEnabled { get { return puzzleRingEnabled; } set { puzzleRingEnabled = value; } }
        public static bool shiMizuEnabled = true;
        public static bool ShiMizuEnabled { get { return shiMizuEnabled; } set { shiMizuEnabled = value; } }
        public static void updateSettings()
        {
            GearSwapSettings settings = new GearSwapSettings();
            loggingEnabled = settings.LoggingEnabled;
            debugLoggingEnabled = settings.DebugLoggingEnabled;
            lowHealthPerc = settings.LowHealthPerc;
            magicFindPerc = settings.MagicFindPerc;
            coldEnabled = settings.ColdEnabled;
            fireEnabled = settings.FireEnabled;
            arcaneEnabled = settings.ArcaneEnabled;
            poisonEnabled = settings.PoisonEnabled;
            lightningEnabled = settings.LightningEnabled;
            eliteEnabled = settings.EliteEnabled;
            shrineEnabled = settings.ShrineEnabled;
            wellEnabled = settings.WellEnabled;
            lowHealthEnabled = settings.LowHealthEnabled;
            criticalHealthEnabled = settings.CriticalHealthEnabled;
            threeEnemiesEnabled = settings.ThreeEnemiesEnabled;
            fiveEnemiesEnabled = settings.FiveEnemiesEnabled;
            magicFindEnabled = settings.MagicFindEnabled;
            barricadeEnabled = settings.BarricadeEnabled;
            immmuneEnabled = settings.ImmuneEnabled;
            barricadeDistance = settings.BarricadeDistance;
            chestEnabled = settings.ChestEnabled;
            lowHealthDuration = settings.LowHealthDuration;
            pulseInterval = settings.PulseInterval;
            puzzleRingEnabled = settings.PuzzleRingEnabled;
            shiMizuEnabled = settings.ShiMizuEnabled;
            _lowHealthDuration.WaitTime = TimeSpan.FromMilliseconds(lowHealthDuration * 1000);
            _pulseTimer.WaitTime = TimeSpan.FromMilliseconds(pulseInterval * 1000);
            _pulseTimer.Update();
            if (priorityUpdated)
                ConfigHelpers.UpdatePriorityList();

        }
        public void LoadConfig()
        {
            GearSwapSettings settings = new GearSwapSettings();
            loggingEnabled = settings.LoggingEnabled;
            lowHealthPerc = settings.LowHealthPerc;
            magicFindPerc = settings.MagicFindPerc;
            coldEnabled = settings.ColdEnabled;
            fireEnabled = settings.FireEnabled;
            arcaneEnabled = settings.ArcaneEnabled;
            poisonEnabled = settings.PoisonEnabled;
            lightningEnabled = settings.LightningEnabled;
            eliteEnabled = settings.EliteEnabled;
            shrineEnabled = settings.ShrineEnabled;
            wellEnabled = settings.WellEnabled;
            lowHealthEnabled = settings.LowHealthEnabled;
            criticalHealthEnabled = settings.CriticalHealthEnabled;
            threeEnemiesEnabled = settings.ThreeEnemiesEnabled;
            fiveEnemiesEnabled = settings.FiveEnemiesEnabled;
            magicFindEnabled = settings.MagicFindEnabled;
            barricadeEnabled = settings.BarricadeEnabled;
            immmuneEnabled = settings.ImmuneEnabled;
            barricadeDistance = settings.BarricadeDistance;
            debugLoggingEnabled = settings.DebugLoggingEnabled;
            chestEnabled = settings.ChestEnabled;
            lowHealthDuration = settings.LowHealthDuration;
            pulseInterval = settings.PulseInterval;
            puzzleRingEnabled = settings.PuzzleRingEnabled;
            shiMizuEnabled = settings.ShiMizuEnabled;
            _lowHealthDuration.WaitTime = TimeSpan.FromMilliseconds(lowHealthDuration * 1000);
            _pulseTimer.WaitTime = TimeSpan.FromMilliseconds(pulseInterval * 1000);
            _pulseTimer.Update();
            WriteToLog("Settings Loaded");
        }
    
    
    
    }
}