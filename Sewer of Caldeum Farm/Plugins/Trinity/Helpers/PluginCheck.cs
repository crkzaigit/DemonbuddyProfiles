﻿using System.IO;
using System.Linq;
using System.Threading;
using Trinity.Technicals;
using Zeta.Bot;

namespace Trinity.Helpers
{
    public class PluginCheck
    {
        private static Thread PluginCheckWatcher;

        /// <summary>
        /// Starts the watcher thread
        /// </summary>
        public static void Start()
        {
            Shutdown();

            if (PassedAllChecks)
                return;

            if (PluginCheckWatcher == null)
            {
                PluginCheckWatcher = new Thread(PluginChecker)
                {
                    Name = "Trinity PluginCheck",
                    IsBackground = true
                };
                PluginCheckWatcher.Start();
                Logger.LogDebug("Plugin Check Watcher thread started");
            }
        }

        /// <summary>
        /// Stops the watcher thread if its running
        /// </summary>
        public static void Shutdown()
        {
            if (PluginCheckWatcher != null)
            {
                if (PluginCheckWatcher.IsAlive)
                    PluginCheckWatcher.Abort();
                PluginCheckWatcher = null;
            }
        }

        private static bool passedAllChecks = false;

        /// <summary>
        /// Whether or not we have passed all checks - set by PluginChecker()
        /// </summary>
        public static bool PassedAllChecks
        {
            get { return PluginCheck.passedAllChecks; }
            private set { PluginCheck.passedAllChecks = value; }
        }

        /// <summary>
        /// Used to check and fix the status of the Plugin (Enabled/Disabled) and the Combat Routine (and routine version)
        /// </summary>
        private static void PluginChecker()
        {
            while (!PassedAllChecks)
            {
                while (!BotMain.IsRunning)
                {
                    Thread.Sleep(250);
                    PassedAllChecks = false;
                }

                if (!Trinity.IsPluginEnabled)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "WARNING: Trinity Plugin is NOT YET ENABLED. Bot start detected");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Ignore this message if you are not currently using Trinity.");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                    break;
                }

                lock (RoutineManager.Current)
                {
                    bool latestTrinityRoutineSelected = RoutineManager.Current.Name.Equals(FileManager.TrinityName);

                    if (!IsLatestRoutineInstalled)
                    {
                        latestTrinityRoutineSelected = false;
                        InstallTrinityRoutine();
                    }

                    if (!latestTrinityRoutineSelected)
                    {
                        SelectTrinityRoutine();
                    }

                    if (Trinity.IsPluginEnabled && latestTrinityRoutineSelected && BotMain.IsRunning)
                    {
                        PassedAllChecks = true;
                    }
                }

                Thread.Sleep(250);
            }

            Logger.LogDebug("Plugin and Routine checks passed!");
        }

        /// <summary>
        /// Check for the latest routine and install if if needed
        /// </summary>
        public static void CheckAndInstallTrinityRoutine()
        {
            if (!IsLatestRoutineInstalled)
            {
                InstallTrinityRoutine();
            }
        }

        /// <summary>
        /// Installs the latest version of the Trinity routine 
        /// </summary>
        private static void InstallTrinityRoutine()
        {
            FileManager.CleanupOldRoutines();

            Logger.LogNormal("Combat routine is not installed or is not latest version, installing! {0}", FileManager.GetFileHeader(FileManager.CombatRoutineSourcePath));
            FileManager.CopyFile(FileManager.CombatRoutineSourcePath, FileManager.CombatRoutineDestinationPath);

            RoutineManager.Reload();
        }

        /// <summary>
        /// Selects the Trinity routine in the RoutineManager
        /// </summary>
        private static void SelectTrinityRoutine()
        {
            if (!IsLatestRoutineInstalled)
            {
                return;
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke((System.Action)(() =>
                {
                    Logger.LogNormal("Stopping bot to select latest routine");
                    BotMain.Stop();

                    CombatRoutine trinityRoutine = (CombatRoutine)RoutineManager.Routines.FirstOrDefault(r => r.Name == "Trinity");
                    RoutineManager.Current = trinityRoutine;

                    Logger.LogNormal("Routine selected, starting bot");
                    BotMain.Start();
                }));
        }

        /// <summary>
        /// Checks if the latest Trinity Routine is installed
        /// </summary>
        public static bool IsLatestRoutineInstalled
        {
            get
            {
                if (!File.Exists(FileManager.CombatRoutineSourcePath))
                {
                    return false;
                }
                if (!File.Exists(FileManager.CombatRoutineDestinationPath))
                {
                    return false;
                }

                return FileManager.CompareFileHeader(FileManager.CombatRoutineSourcePath, FileManager.CombatRoutineDestinationPath);
            }
        }


    }
}
