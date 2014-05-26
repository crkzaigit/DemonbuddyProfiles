using System;
using System.Windows;
using Armory.Behaviors;
using Zeta.Common;
using Zeta.Common.Plugins;

namespace Armory
{
    public class Plugin : IPlugin
    {
        public const string AUTHOR = "rrrix, Gniller, Nuok";
        public const string DESCRIPTION = "Auto Equips Items";
        public const string NAME = "Armory";

        public Version Version
        {
            get { return Armory.PluginVersion; }
        }

        public void OnPulse()
        {
            if (!Behaviors.HookReplacer.UsingCustomBehavior)
            {
                Behaviors.HookReplacer.InsertHooks();
            }
            ItemEvaluator.EvaluateItems();
        }

        public string Author
        {
            get { return AUTHOR; }
        }

        public string Description
        {
            get { return DESCRIPTION; }
        }

        public Window DisplayWindow
        {
            get { return Settings.UI.GetDisplayWindow(); }
        }

        public string Name
        {
            get { return NAME; }
        }

        public void OnDisabled()
        {
            Behaviors.HookReplacer.RemoveHooks();
            Logger.Log("v{0}  Disabled", Version);
        }

        public void OnEnabled()
        {
            Logger.Log("v{0}  Enabled", Version);
            ItemEvaluator.Reset();
            TreeHooks.Instance.OnHooksCleared += HookReplacer.Instance_OnHooksCleared;
        }

        public void OnInitialize()
        {
            Logger.Log("v{0} Initialized", Version);
        }

        public void OnShutdown()
        {
            Logger.Log("v{0} Shutdown", Version);
        }

        public bool Equals(IPlugin other)
        {
            return other.Name == Name;
        }
    }
}