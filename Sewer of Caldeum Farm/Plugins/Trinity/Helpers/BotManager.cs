﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Trinity.Config;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Helpers
{
    public class BotManager
    {
        private static TrinitySetting Settings { get { return Trinity.Settings; } }

        private static readonly Dictionary<string, Composite> OriginalHooks = new Dictionary<string, Composite>();

        /// <summary>
        /// This will replace the main BehaviorTree hooks for Combat, Vendoring, and Looting.
        /// </summary>
        internal static void ReplaceTreeHooks()
        {
            if (Trinity.IsPluginEnabled)
            {
                // This is the do-all-be-all god-head all encompasing piece of trinity
                StoreAndReplaceHook("Combat", new Decorator(Trinity.TargetCheck, Trinity.HandleTargetAction()));

                // We still want the main VendorRun logic, we're just going to take control of *when* this logic kicks in
                var vendorDecorator = TreeHooks.Instance.Hooks["VendorRun"][0] as Decorator;
                if (vendorDecorator != null)
                    StoreAndReplaceHook("VendorRun", new Decorator(TownRun.TownRunCanRun, TownRun.TownRunWrapper(vendorDecorator.Children[0])));

                // Loot tree is now empty and never runs (Loot is handled through combat)
                // This is for special out of combat handling like Horadric Cache
                Composite lootComposite = TreeHooks.Instance.Hooks["Loot"][0];
                StoreAndReplaceHook("Loot", Composites.CreateLootBehavior(lootComposite));

            }
            else
            {
                ReplaceHookWithOriginal("Combat");
                ReplaceHookWithOriginal("VendorRun");
                ReplaceHookWithOriginal("Loot");
            }
        }

        private static void StoreAndReplaceHook(string hookName, Composite behavior)
        {
            if (!OriginalHooks.ContainsKey(hookName))
                OriginalHooks.Add(hookName, TreeHooks.Instance.Hooks[hookName][0]);

            TreeHooks.Instance.ReplaceHook(hookName, behavior);
        }

        private static void ReplaceHookWithOriginal(string hook)
        {
            if (OriginalHooks.ContainsKey(hook))
            {
                TreeHooks.Instance.ReplaceHook(hook, OriginalHooks[hook]);
            }
        }


        internal static void SetBotTicksPerSecond()
        {
            // Carguy's ticks-per-second feature
            if (Settings.Advanced.TPSEnabled)
            {
                BotMain.TicksPerSecond = Settings.Advanced.TPSLimit;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Bot TPS set to {0}", Settings.Advanced.TPSLimit);
            }
            else
            {
                BotMain.TicksPerSecond = 10;
                //BotMain.TicksPerSecond = Int32.MaxValue;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Reset bot TPS to default", Settings.Advanced.TPSLimit);
            }
        }

        internal static void SetItemManagerProvider()
        {
            if (Settings.Loot.ItemFilterMode != Config.Loot.ItemFilterMode.DemonBuddy)
            {
                ItemManager.Current = new TrinityItemManager();
            }
            else
            {
                ItemManager.Current = new LootRuleItemManager();
            }
        }

        internal static void SetUnstuckProvider()
        {
            if (Settings.Advanced.UnstuckerEnabled)
            {
                Navigator.StuckHandler = new StuckHandler();
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Trinity Unstucker", true);
            }
            else
            {
                Navigator.StuckHandler = new DefaultStuckHandler();
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Default Demonbuddy Unstucker", true);
            }
        }

        internal static void Exit()
        {
            ZetaDia.Memory.Process.Kill();

            try
            {
                if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
                {
                    Application.Current.Dispatcher.Invoke(new System.Action(Exit));
                    return;
                }

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

    }
}
