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
    partial class gearSwap : IPlugin
    {
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();
        public string Author { get { return "borderjs"; } }
        public string Description { get { return "This plugin will equip gear based on statuses found in the environment"; } }
        public string Name { get { return "GearSwap"; } }
        public Version Version { get { return new Version(1, 0); } }
        
        public System.Windows.Window DisplayWindow
        {
            get { return Config.GetDisplayWindow(); }
        }

        public static List<gear> gearList = new List<gear>();
        public static List<gear> originalGear = new List<gear>();
        public static List<condition> statuses = new List<condition>();
        public static List<condition> defaultStatuses = new List<condition>();
        public static bool priorityUpdated = false;
        public static bool debugLoggingEnabled = false;
        public static bool DebugLoggingEnabled { get { return debugLoggingEnabled; } set { debugLoggingEnabled = value; } }
        public static bool loggingEnabled = true;
        public static bool LoggingEnabled { get { return loggingEnabled; } set { loggingEnabled = value; } }
        public static int lowHealthPerc = 40;
        public static int LowHealthPerc { get { return lowHealthPerc; } set { lowHealthPerc = value; } }
        public static int magicFindPerc = 15;
        public static int MagicFindPerc { get { return magicFindPerc; } set { magicFindPerc = value; } }
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
            if (priorityUpdated)
                updatePriorityList();

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
            Log.Info("[GearSwap] Settings Loaded");
        }


        public static void writeToLog(string msg)
        {
            if (loggingEnabled)
            {
                Log.Info("[GearSwap] " + msg);
            }
        }
        public static void writeToLog(string msg, Exception e)
        {
            if (debugLoggingEnabled)
            {
                Log.Info("[GearSwap] " + msg);
                Log.Info("[GearSwap] " + e);
            }
        }
       
        public void OnInitialize()
        {
            Log.Info("[GearSwap] " + Version + " Initialized");
        }
        public void OnShutdown()
        {
            Log.Info("[GearSwap] Shutdown");
        }

        public void OnEnabled()
        {
            Log.Info("[GearSwap] Plugin Enabled");
            ZetaDia.Memory.TemporaryCacheState(false);
            setGearList();
            LoadConfig();
            GameEvents.OnGameLeft += cleanUp;
            GameEvents.OnGameJoined += prep;

        }

        public void OnDisabled()
        {
            Log.Info("[GearSwap] Plugin Disabled");
            GameEvents.OnGameLeft -= cleanUp;
            GameEvents.OnGameJoined += prep;
        }

        public bool Equals(IPlugin other)
        {
            return (other.Name == Name) && (other.Version == Version);
        }

        public void OnPulse()
        {
            // Don't do anything if we're dead or switching worlds
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || !ZetaDia.CPlayer.IsValid || ZetaDia.Me.IsDead || ZetaDia.IsLoadingWorld)
            {
                return;
            }

            //check for affixes every 1 second.
            if (_1SecTimer.IsFinished)
            {
                resetArea();
                checkArea();
            }

        }

        private static WaitTimer _1SecTimer = new WaitTimer(TimeSpan.FromMilliseconds(1000));
        private static WaitTimer _30SecTimer = new WaitTimer(TimeSpan.FromSeconds(30));
        private static WaitTimer _archewTimer = new WaitTimer(TimeSpan.FromSeconds(31));
        private static WaitTimer _beckonTimer = new WaitTimer(TimeSpan.FromSeconds(31));
        private static WaitTimer _poxTimer = new WaitTimer(TimeSpan.FromSeconds(11));
    }
}