﻿using System;
using QuestTools.Helpers;
using Zeta.Bot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace QuestTools.ProfileTags
{
    [XmlElement("QuestToolsSetVariable")]
    public class QuestToolsVariableTag : ProfileBehavior
    {
        public QuestToolsVariableTag() { }
        private bool _isDone;
        public override bool IsDone { get { return !IsActiveQuestStep || _isDone; } }

        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlAttribute("value")]
        public string Value { get; set; }

        public enum Keys
        {
            DebugLogging,
        }

        public override void OnStart()
        {
            Logger.Log("QuestToolsSetVariable tag started, key={0} value={1}", Key, Value);
        }

        protected override Composite CreateBehavior()
        {
            return new Action(ret => _isDone = true);
        }

        public bool SafeCompareKey(string input, Keys key)
        {
            bool result = input.ToUpper().Trim() == key.ToString().ToUpper().Trim();
            //Logging.Write("[QuestTools] Comparing Keys {0} and {1}, result is {2}", input, key.ToString(), result);
            return result;
        }

        public bool SafeCompareString(string string1, string string2)
        {
            
            bool result = string1.ToUpper().Trim() == string2.ToUpper().Trim();
            //Logging.Write("[QuestTools] Comparing Keys {0} and {1}, result is {2}", string1, string2.ToString(), result);
            return result;
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
        }
    }
}
