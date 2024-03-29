﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using Trinity.Cache;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    /// <summary>
    /// Trinity DemonBuddy Plugin 
    /// </summary>
    public partial class Trinity
    {
        public Version Version
        {
            get
            {
                return new Version(1, 9, 8);
            }
        }

        public string Author
        {
            get
            {
                return "rrrix + darkfriend77 + GilesSmith + Community Devs";
            }
        }

        public string Description
        {
            get
            {
                return string.Format("Trinity v{0}", Version);
            }
        }

        private static bool MouseLeft()
        {
            return (System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left;
        }

        /// <summary>
        /// Receive Pulse event from DemonBuddy.
        /// </summary>
        public void OnPulse()
        {
            using (new PerformanceLogger("OnPulse"))
            {
                try
                {
                    // At Login screen, derp!
                    var loginSubmitButton = Zeta.Game.Internals.UIElement.FromName("Root.NormalLayer.BattleNetLogin_main.LayoutRoot.LoginContainer.SubmitButton");
                    if (loginSubmitButton != null && loginSubmitButton.IsValid && loginSubmitButton.IsVisible && BotMain.IsRunning)
                    {
                        new Thread(() => {
                            BotMain.Stop();
                            Thread.Sleep(1000);
                            BotMain.Start();
                        })
                        {
                            IsBackground = true,
                            Name = "LoginScreenBouncer",
                        }.Start();

                        return;
                    }

                    if (ZetaDia.Me == null)
                        return;

                    if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
                        return;

                    GameUI.SafeClickUIButtons();

                    if (ZetaDia.Me.IsDead)
                        return;

                    using (new PerformanceLogger("LazyRaiderClickToPause"))
                    {

                        if (Settings.Advanced.LazyRaiderClickToPause && !BotMain.IsPaused)
                        {
                            BotMain.PauseWhile(MouseLeft);
                        }
                    }

                    // See if we should update the stats file
                    if (DateTime.UtcNow.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                    {
                        ItemStatsLastPostedReport = DateTime.UtcNow;
                        OutputReport();
                    }

                    // Recording of all the XML's in use this run
                    UsedProfileManager.RecordProfile();

                    Monk_MaintainTempestRush();
                }
                catch (AccessViolationException)
                {
                    // woof! 
                }
                catch (Exception ex)
                {
                    Logger.Log(LogCategory.UserInformation, "Exception in Pulse: {0}", ex.ToString());
                }
            }
        }

        /// <summary>
        /// Called when user Enable the plugin.
        /// </summary>
        public void OnEnabled()
        {
            try
            {
                Logger.Log("OnEnable start");
                DateTime dateOnEnabledStart = DateTime.UtcNow;

                BotMain.OnStart += TrinityBotStart;
                BotMain.OnStop += TrinityBotStop;

                SetWindowTitle();

                if (!Directory.Exists(FileManager.PluginPath))
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Fatal Error - cannot enable plugin. Invalid path: {0}", FileManager.PluginPath);
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, @"Plugin should be installed to \<DemonBuddyFolder>\Plugins\Trinity\");
                }
                else
                {
                    PluginCheck.Start();

                    _isPluginEnabled = true;

                    // Settings are available after this... 
                    LoadConfiguration();

                    Navigator.PlayerMover = new PlayerMover();
                    BotManager.SetUnstuckProvider();
                    GameEvents.OnPlayerDied += TrinityOnDeath;
                    GameEvents.OnGameJoined += TrinityOnJoinGame;
                    GameEvents.OnGameLeft += TrinityOnLeaveGame;
                    GameEvents.OnItemSold += ItemEvents.TrinityOnItemSold;
                    GameEvents.OnItemSalvaged += ItemEvents.TrinityOnItemSalvaged;
                    GameEvents.OnItemStashed += ItemEvents.TrinityOnItemStashed;
                    GameEvents.OnItemIdentificationRequest += ItemEvents.TrinityOnOnItemIdentificationRequest;
                    GameEvents.OnGameChanged += GameEvents_OnGameChanged;
                    GameEvents.OnWorldChanged += GameEvents_OnWorldChanged;

                    CombatTargeting.Instance.Provider = new BlankCombatProvider();
                    LootTargeting.Instance.Provider = new BlankLootProvider();
                    ObstacleTargeting.Instance.Provider = new BlankObstacleProvider();

                    if (Settings.Loot.ItemFilterMode != Config.Loot.ItemFilterMode.DemonBuddy)
                    {
                        ItemManager.Current = new TrinityItemManager();
                    }

                    // Safety check incase DB "OnStart" event didn't fire properly
                    if (BotMain.IsRunning)
                    {
                        TrinityBotStart(null);
                        if (ZetaDia.IsInGame)
                            TrinityOnJoinGame(null, null);
                    }

                    BeginInvoke(BotManager.SetBotTicksPerSecond);

                    UI.UILoader.PreLoadWindowContent();

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ENABLED: {0} now in action!", Description);
                }

                if (StashRule != null)
                {
                    // reseting stash rules
                    BeginInvoke(() => StashRule.reset());
                }

                Logger.LogDebug("OnEnable took {0}ms", DateTime.UtcNow.Subtract(dateOnEnabledStart).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in OnEnable: " + ex);
            }
        }

        /// <summary>
        /// Called when user disable the plugin.
        /// </summary>
        public void OnDisabled()
        {
            _isPluginEnabled = false;

            BotManager.ReplaceTreeHooks();

            Navigator.PlayerMover = new DefaultPlayerMover();
            Navigator.StuckHandler = new DefaultStuckHandler();
            CombatTargeting.Instance.Provider = new DefaultCombatTargetingProvider();
            LootTargeting.Instance.Provider = new DefaultLootTargetingProvider();
            ObstacleTargeting.Instance.Provider = new DefaultObstacleTargetingProvider();
            Navigator.SearchGridProvider = new MainGridProvider();

            GameEvents.OnPlayerDied -= TrinityOnDeath;
            BotMain.OnStop -= TrinityBotStop;

            GameEvents.OnPlayerDied -= TrinityOnDeath;
            GameEvents.OnGameJoined -= TrinityOnJoinGame;
            GameEvents.OnGameLeft -= TrinityOnLeaveGame;
            GameEvents.OnItemSold -= ItemEvents.TrinityOnItemSold;
            GameEvents.OnItemSalvaged -= ItemEvents.TrinityOnItemSalvaged;
            GameEvents.OnItemStashed -= ItemEvents.TrinityOnItemStashed;
            GameEvents.OnItemIdentificationRequest -= ItemEvents.TrinityOnOnItemIdentificationRequest;
            GameEvents.OnGameChanged -= GameEvents_OnGameChanged;
            GameEvents.OnWorldChanged -= GameEvents_OnWorldChanged;

            ItemManager.Current = new LootRuleItemManager();

            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "");
            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "DISABLED: Trinity is now shut down...");
            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "");
            GenericCache.Shutdown();
            GenericBlacklist.Shutdown();

        }

        /// <summary>
        /// Called when DemonBuddy shut down.
        /// </summary>
        public void OnShutdown()
        {
            GenericCache.Shutdown();
            GenericBlacklist.Shutdown();
            PluginCheck.Shutdown();
        }

        /// <summary>
        /// Called when DemonBuddy initialize the plugin.
        /// </summary>
        public void OnInitialize()
        {
            PluginCheck.CheckAndInstallTrinityRoutine();
            Logger.Log("Initialized v{0}", Version);
        }

        public string Name
        {
            get
            {
                return "Trinity";
            }
        }

        public bool Equals(IPlugin other)
        {
            return (other.Name == Name) && (other.Version == Version);
        }

        private static Trinity _instance;
        public static Trinity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Trinity();
                }
                return _instance;
            }
        }

        public Trinity()
        {
            _instance = this;
            PluginCheck.CheckAndInstallTrinityRoutine();
        }


        private static DateTime _lastWindowTitleTick = DateTime.MinValue;
        private static Window _mainWindow;
        internal static void SetWindowTitle(string profileName = "")
        {
            if (DateTime.UtcNow.Subtract(_lastWindowTitleTick).TotalMilliseconds < 1000)
                return;

            _lastWindowTitleTick = DateTime.UtcNow;

            if (_mainWindow == null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => _mainWindow = Application.Current.MainWindow));
            }

            if (_mainWindow == null || !ZetaDia.Service.IsValid || !ZetaDia.Service.Platform.IsValid || !ZetaDia.Service.Platform.IsConnected)
                return;

            string battleTagName = "";
            if (Settings.Advanced.ShowBattleTag)
            {
                try
                {
                    battleTagName = "- " + FileManager.BattleTagName + " ";
                }
                catch
                { }
            }

            string windowTitle = "DB " + battleTagName + "- PID:" + System.Diagnostics.Process.GetCurrentProcess().Id;

            if (profileName.Trim() != String.Empty)
            {
                windowTitle += " - " + profileName;
            }

            BeginInvoke(() =>
            {
                try
                {
                    if (_mainWindow != null && !string.IsNullOrWhiteSpace(windowTitle))
                    {
                        _mainWindow.Title = windowTitle;
                    }
                }
                catch
                {
                }
            });
        }

        internal static void BeginInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }

    }
}
