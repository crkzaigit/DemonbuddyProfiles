﻿using System;
using System.Diagnostics;
using Zeta.Bot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace QuestTools.ProfileTags
{
    // * TrinityUseReset - Resets a UseOnce tag as if it has never been used
    [XmlElement("TrinityRandomWait")]
    [XmlElement("RandomWait")]
    public class RandomWaitTag : ProfileBehavior
    {
        public RandomWaitTag() { }
        private bool _isDone;
        private int _delay;
        private readonly Stopwatch _timer = new Stopwatch();

        public override bool IsDone
        {
            get { return _isDone; }
        }

        protected override Composite CreateBehavior()
        {
            return
            new Sequence(
                new Action(ret => _delay = new Random().Next(Min, Max)),
                new Action(ret => Logger.Log("Random Wait - Taking a break for {0:3} seconds.", _delay)),
                new Action(ctx => DoRandomWait()),
                new Action(ret => _isDone = true)
            );
        }

        private RunStatus DoRandomWait()
        {
            if (!_timer.IsRunning)
            {
                _timer.Start();
                return RunStatus.Running;
            }
            if (_timer.IsRunning && _timer.ElapsedMilliseconds < _delay)
            {
                return RunStatus.Running;
            }

            _timer.Reset();
            return RunStatus.Success;
        }


        [XmlAttribute("min")]
        public int Min { get; set; }

        [XmlAttribute("max")]
        public int Max { get; set; }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
        }
    }
}
