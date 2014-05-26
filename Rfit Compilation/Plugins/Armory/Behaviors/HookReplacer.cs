using System;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.TreeSharp;

namespace Armory.Behaviors
{
    public class HookReplacer
    {
        private static List<Composite> _originalComposites;

        private static Composite _armoryBehaviors;

        private const string HookName = "BotBehavior";

        public static bool UsingCustomBehavior { get; set; }
        internal static void InsertHooks()
        {
            if (!TreeHooks.Instance.Hooks.ContainsKey(HookName))
                return;

            if (_armoryBehaviors == null)
            {
                _armoryBehaviors = 
                    new PrioritySelector(
                        MysteryVendor.CreateBehavior(), 
                        IdentifyBehavior.CreateBehavior()
                    );
            }
            
            Logger.Log("Inserting Armory Behaviors into " + HookName + " Hook");
            UsingCustomBehavior = true;
            TreeHooks.Instance.InsertHook(HookName, 0, _armoryBehaviors);
        }
        internal static void RemoveHooks()
        {
            if (_armoryBehaviors != null && UsingCustomBehavior)
            {
                Logger.Log("Removing Armory behaviors from " + HookName + " Hook");
                TreeHooks.Instance.RemoveHook(HookName, _armoryBehaviors);
                UsingCustomBehavior = false;
            }
        }

        internal static void Instance_OnHooksCleared(object sender, EventArgs e)
        {
            InsertHooks();
        }
    }
}
