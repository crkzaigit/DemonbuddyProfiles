﻿using System;
using System.Collections.Generic;
using System.Threading;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity
    {
        // How many total leave games, for stat-tracking?
        public static int TotalGamesJoined = 0;
        // How many total leave games, for stat-tracking?
        public static int TotalLeaveGames = 0;
        public static int TotalProfileRecycles = 0;

        /// <summary>
        /// This is wired up by Plugin.OnEnabled, and called when the bot is started
        /// </summary>
        /// <param name="bot"></param>
        private static void TrinityBotStart(IBot bot)
        {
            Logger.Log("Bot Starting");
            DateTime timeBotStart = DateTime.UtcNow;

            V.ValidateLoad();

            // Recording of all the XML's in use this run
            try
            {
                string sThisProfile = GlobalSettings.Instance.LastProfile;
                if (sThisProfile != CurrentProfile)
                {
                    ProfileHistory.Add(sThisProfile);
                    CurrentProfile = sThisProfile;
                    if (FirstProfile == "")
                        FirstProfile = sThisProfile;
                }
            }
            catch
            {
            }

            HasMappedPlayerAbilities = false;
            if (!MaintainStatTracking)
            {
                ItemStatsWhenStartedBot = DateTime.UtcNow;
                ItemStatsLastPostedReport = DateTime.UtcNow;
                MaintainStatTracking = true;
            }
            else
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                    "Note: Maintaining item stats from previous run. To reset stats fully, please restart DB.");
            }

            BeginInvoke(UsedProfileManager.RefreshProfileBlacklists);
            UsedProfileManager.SetProfileInWindowTitle();

            BotManager.ReplaceTreeHooks();

            PlayerMover.TimeLastRecordedPosition = DateTime.UtcNow;
            PlayerMover.LastRestartedGame = DateTime.UtcNow;
            Logger.Log("Bot Starting, Resetting Gold Inactivity Timer");
            GoldInactivity.Instance.ResetCheckGold();

            if (CharacterSettings.Instance.KillRadius < 20)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                    "WARNING: Low Kill Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    CharacterSettings.Instance.KillRadius);
            }

            if (CharacterSettings.Instance.LootRadius < 50)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                    "WARNING: Low Gold Loot Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    CharacterSettings.Instance.LootRadius);
            }

            if (Settings.Loot.ItemFilterMode == Config.Loot.ItemFilterMode.TrinityWithItemRules)
            {
                try
                {
                    if (StashRule == null)
                        StashRule = new Interpreter();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error configuring ItemRules Interpreter: " + ex);
                }
            }

            Logger.LogDebug("Trinity BotStart took {0:0}ms", DateTime.UtcNow.Subtract(timeBotStart).TotalMilliseconds);
        }

        private void GameEvents_OnGameChanged(object sender, EventArgs e)
        {
            ClearCachesOnGameChange(sender, e);

            // reload the profile juuuuuuuuuuuust in case Demonbuddy missed it... which it is known to do on disconnects
            string currentProfilePath = ProfileManager.CurrentProfile.Path;
            ProfileManager.Load(currentProfilePath);
            Navigator.SearchGridProvider.Update();
            ResetEverythingNewGame();
            UsedProfileManager.SetProfileInWindowTitle();
        }

        void GameEvents_OnWorldChanged(object sender, EventArgs e)
        {
            CacheData.FullClear();
        }

        // When the bot stops, output a final item-stats report so it is as up-to-date as can be
        private void TrinityBotStop(IBot bot)
        {
            // Issue final reports
            OutputReport();
            BacktrackList = new SortedList<int, Vector3>();
            TotalBacktracks = 0;
            PlayerMover.TotalAntiStuckAttempts = 1;
            PlayerMover.vSafeMovementLocation = Vector3.Zero;
            PlayerMover.LastPosition = Vector3.Zero;
            PlayerMover.TimesReachedStuckPoint = 0;
            PlayerMover.TimeLastRecordedPosition = DateTime.MinValue;
            PlayerMover.LastGeneratedStuckPosition = DateTime.MinValue;
            DeathsThisRun = 0;
            CacheData.FullClear();
        }

        private void TrinityOnDeath(object src, EventArgs mea)
        {
            if (DateTime.UtcNow.Subtract(LastDeathTime).TotalSeconds > 10)
            {
                LastDeathTime = DateTime.UtcNow;
                TotalDeaths++;
                DeathsThisRun++;
                CacheData.AbilityLastUsed = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);
                BacktrackList = new SortedList<int, Vector3>();
                TotalBacktracks = 0;
                PlayerMover.TotalAntiStuckAttempts = 1;
                PlayerMover.vSafeMovementLocation = Vector3.Zero;

                // Reset pre-townrun position if we die
                TownRun.PreTownRunPosition = Vector3.Zero;
                TownRun.PreTownRunWorldId = -1;
                SpellHistory.HistoryQueue.Clear();
            }
        }


        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void TrinityOnJoinGame(object src, EventArgs mea)
        {
            TotalGamesJoined++;
            ResetEverythingNewGame();
        }

        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void TrinityOnLeaveGame(object src, EventArgs mea)
        {
            TotalLeaveGames++;
            ResetEverythingNewGame();
        }

        public static void ResetEverythingNewGame()
        {
            // In-thread stuff
            V.ValidateLoad();

            // Out of thread Async stuff
            BeginInvoke(new Action(() =>
                {

                    Logger.Log("New Game - resetting everything");

                    Trinity.IsReadyToTownRun = false;
                    Trinity.ForceVendorRunASAP = false;
                    TownRun.TownRunCheckTimer.Reset();
                    TownRun.SendEmailNotification();
                    TownRun.PreTownRunPosition = Vector3.Zero;
                    TownRun.PreTownRunWorldId = -1;
                    TownRun.WasVendoring = false;

                    CacheData.AbilityLastUsed.Clear();
                    SpellHistory.HistoryQueue.Clear();

                    DeathsThisRun = 0;
                    LastDeathTime = DateTime.UtcNow;
                    _hashsetItemStatsLookedAt = new HashSet<string>();
                    _hashsetItemPicksLookedAt = new HashSet<string>();
                    _hashsetItemFollowersIgnored = new HashSet<string>();

                    Blacklist60Seconds = new HashSet<int>();
                    Blacklist90Seconds = new HashSet<int>();
                    Blacklist15Seconds = new HashSet<int>();
                    BacktrackList = new SortedList<int, Vector3>();
                    TotalBacktracks = 0;
                    HasMappedPlayerAbilities = false;
                    PlayerMover.TotalAntiStuckAttempts = 1;
                    PlayerMover.vSafeMovementLocation = Vector3.Zero;
                    PlayerMover.LastPosition = Vector3.Zero;
                    PlayerMover.TimesReachedStuckPoint = 0;
                    PlayerMover.TimeLastRecordedPosition = DateTime.MinValue;
                    PlayerMover.LastGeneratedStuckPosition = DateTime.MinValue;
                    PlayerMover.TimesReachedMaxUnstucks = 0;
                    PlayerMover.CancelUnstuckerForSeconds = 0;
                    PlayerMover.LastCancelledUnstucker = DateTime.MinValue;
                    NavHelper.UsedStuckSpots = new List<GridPoint>();

                    CacheData.FullClear();

                    // Reset all the caches
                    ProfileHistory = new List<string>();
                    CurrentProfile = "";
                    FirstProfile = "";

                    Logger.Log("New Game, resetting Gold Inactivity Timer");
                    GoldInactivity.Instance.ResetCheckGold();

                    CombatBase.IsQuestingMode = false;

                    GenericCache.ClearCache();
                    GenericBlacklist.ClearBlacklist();
                }));
        }
    }
}