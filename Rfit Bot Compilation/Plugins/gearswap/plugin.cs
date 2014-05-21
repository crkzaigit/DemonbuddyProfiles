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
        private static readonly log4net.ILog Log = Logger.GetLoggerInstanceForType();
        public string Author { get { return "borderjs"; } }
        public string Description { get { return "This plugin will equip gear based on statuses found in the environment"; } }
        public string Name { get { return "GearSwap"; } }
        public Version Version { get { return new Version(1, 7); } }
        
        public System.Windows.Window DisplayWindow
        {
            get { return Config.GetDisplayWindow(); }
        }

        public static void WriteToLog(string msg)
        {
            if (loggingEnabled)
            {
                Log.Info("[GearSwap] " + msg);
            }
        }
        public static void WriteToLog(string msg, Exception e)
        {
            if (debugLoggingEnabled)
            {
                Log.Info("[GearSwap] " + msg);
                Log.Info("[GearSwap] " + e);
            }
        }
       
        public void OnInitialize()
        {
           WriteToLog(Version + " Initialized");
        }
        public void OnShutdown()
        {
            WriteToLog("Shutdown");
        }

        public void OnEnabled()
        {
            WriteToLog("Plugin Enabled");
            ZetaDia.Memory.TemporaryCacheState(false);
            SetGearList();
            LoadConfig();
            GameEvents.OnGameLeft += CleanUp;
            GameEvents.OnGameJoined += Prep;

        }

        public void OnDisabled()
        {
            WriteToLog("Plugin Disabled");
            GameEvents.OnGameLeft -= CleanUp;
            GameEvents.OnGameJoined += Prep;
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
            if (_pulseTimer.IsFinished)
            {
                ResetArea();
                Checks.CheckArea();
            }

        }


    }
}