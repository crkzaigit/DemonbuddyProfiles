﻿using Zeta.Bot.Profile.Composites;
using Zeta.XmlEngine;

namespace QuestTools.ProfileTags.Depreciated
{
    [XmlElement("TrinityIfSNOInRange")]
    public class TrinityIfSNOInRange : IfTag
    {
        public TrinityIfSNOInRange() { }
        public override void OnStart()
        {
            Logger.LogError("TrinityIfSNOInRange is decpreciated. Use <If condition=\"ActorExistsAt(actorId, x, y, z, range)\" /> instead.");
            base.OnStart();
        }
    }
}
