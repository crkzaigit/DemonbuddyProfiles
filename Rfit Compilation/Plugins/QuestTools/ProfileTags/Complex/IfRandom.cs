﻿using System;
using System.Linq;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags
{
    // TrinityIfRandom only runs the container stuff if the given id is the given value
    [XmlElement("TrinityIfRandom")]
    [XmlElement("IfRandom")]
    public class IfRandomTag : BaseComplexNodeTag
    {
        public IfRandomTag() { }
        protected override Composite CreateBehavior()
        {
            return
             new Decorator(ret => !IsDone,
                 new PrioritySelector(
                     base.GetNodes().Select(b => b.Behavior).ToArray()
                 )
             );
        }

        public override bool GetConditionExec()
        {
            int oldValue;

            // If the dictionary value doesn't even exist, FAIL!
            if (!RandomRoll.RandomIds.TryGetValue(ID, out oldValue))
                return false;

            // Ok, do the results match up what we want? then SUCCESS!
            return oldValue == Result;
        }

        [XmlAttribute("id")]
        public int ID { get; set; }

        [XmlAttribute("result")]
        public int Result { get; set; }

        public Func<bool> Conditional { get; set; }

    }
}
