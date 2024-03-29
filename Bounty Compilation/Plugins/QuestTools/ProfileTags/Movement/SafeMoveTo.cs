﻿using System;
using QuestTools.Helpers;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Game;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace QuestTools.ProfileTags.Movement
{
    /// <summary>
    ///     This custom behavior will fail-safe if it cannot generate a path, and will also use a point of "LocalNavDistance"
    ///     away between the player and destination
    ///     as its temporary destination. Usually a distance between 150-250 is ideal for pathing, and works in situations like
    ///     random dungeons and between New Tristram and anywhere in that world.
    /// </summary>
    [XmlElement("TrinityMoveTo")]
    [XmlElement("SafeMoveTo")]
    internal class SafeMoveToTag : ProfileBehavior
    {
        public SafeMoveToTag() { }

        private const double MaxNavPointAgeMs = 15000;
        private readonly QTNavigator _qtNavigator = new QTNavigator();
        private bool _isDone;
        private DateTime _lastGeneratedNavPoint = DateTime.MinValue;
        private MoveResult _lastMoveResult = default(MoveResult);
        private Vector3 _navTarget;
        private DateTime _tagStartTime;

        public override bool IsDone
        {
            get { return !IsActiveQuestStep || _isDone; }
        }

        [XmlAttribute("pathPrecision")]
        public int PathPrecision { get; set; }

        [XmlAttribute("straightLinePathing")]
        public bool StraightLinePathing { get; set; }

        [XmlAttribute("useNavigator")]
        public bool UseNavigator { get; set; }

        [XmlAttribute("x")]
        public float X { get; set; }

        [XmlAttribute("y")]
        public float Y { get; set; }

        [XmlAttribute("z")]
        public float Z { get; set; }

        public Vector3 Position
        {
            get { return new Vector3(X, Y, Z); }
        }

        /// <summary>
        ///     This is used for very distance Position coordinates; where Demonbuddy cannot make a client-side pathing request
        ///     and has to contact the server. A value too large (over 300) will sometimes cause pathing requests to fail
        ///     (PathGenerationFailed).
        /// </summary>
        [XmlAttribute("localNavDistance")]
        [XmlAttribute("pathPointLimit")]
        [XmlAttribute("raycastDistance")]
        public int PathPointLimit { get; set; }

        /// <summary>
        ///     This will set a time in seconds that this tag is allowed to run for
        /// </summary>
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }

        [XmlAttribute("allowLongDistance")]
        public bool AllowLongDistance { get; set; }

        /// <summary>
        ///     Main SafeMoveTo behavior
        /// </summary>
        /// <returns></returns>
        protected override Composite CreateBehavior()
        {
            return new Sequence(
                new DecoratorContinue(ret => ZetaDia.Me.IsDead,
                    new Action(ret => RunStatus.Failure)
                    ),
                new Action(ret => GameUI.SafeClickUIButtons()),
                new PrioritySelector(
                    new Decorator(ctx => _tagStartTime != DateTime.UtcNow && DateTime.UtcNow.Subtract(_tagStartTime).TotalSeconds > Timeout,
                        new Sequence(
                            new Action(ret => Logger.Log("Timeout of {0} seconds exceeded for Profile Behavior (start: {1} now: {2}) {3}", Timeout, _tagStartTime.ToLocalTime(), DateTime.Now, Status())),
                            new Action(ret => _isDone = true)
                            )
                        ),
                    new Decorator(ctx => !AllowLongDistance && Position.Distance2D(ZetaDia.Me.Position) > 1500,
                        new Sequence(
                            new Action(ret => Logger.Log("Error! Destination distance is {0}", Position.Distance2D(ZetaDia.Me.Position))),
                            new Action(ret => _isDone = true)
                            )
                        ),
                    new Switch<MoveResult>(ret => Move(),
                        new SwitchArgument<MoveResult>(MoveResult.ReachedDestination,
                            new Sequence(
                                new Action(ret => Logger.Log("ReachedDestination! {0}", Status())),
                                new Action(ret => _isDone = true)
                                )
                            ),
                        new SwitchArgument<MoveResult>(MoveResult.PathGenerationFailed,
                            new Sequence(
                                new Action(ret => Logger.Log("Move Failed: {0}! {1}", ret, Status())),
                                new Action(ret => _isDone = true)
                                )
                            )
                        )
                    )
                );
        }

        private MoveResult Move()
        {
            if (Position.Distance2D(ZetaDia.Me.Position) <= PathPrecision)
                return MoveResult.ReachedDestination;

            _navTarget = Position;

            double timeSinceLastGenerated = DateTime.UtcNow.Subtract(_lastGeneratedNavPoint).TotalMilliseconds;
            if (Position.Distance2D(ZetaDia.Me.Position) > PathPointLimit && timeSinceLastGenerated > MaxNavPointAgeMs)
            {
                // generate a local client pathing point
                _navTarget = MathEx.CalculatePointFrom(ZetaDia.Me.Position, Position, Position.Distance2D(ZetaDia.Me.Position) - PathPointLimit);
            }
            MoveResult moveResult;
            if (StraightLinePathing)
            {
                // just "Click" 
                Navigator.PlayerMover.MoveTowards(Position);
                moveResult = MoveResult.Moved;
            }
            else
            {
                // Use the Navigator or PathFinder
                moveResult = _qtNavigator.MoveTo(_navTarget, Status());
            }
            LogStatus();

            return moveResult;
        }

        public override void OnStart()
        {
            if (PathPrecision == 0)
                PathPrecision = 15;
            if (PathPointLimit == 0)
                PathPointLimit = 250;
            if (Timeout == 0)
                Timeout = 180;

            _lastGeneratedNavPoint = DateTime.MinValue;
            _lastMoveResult = MoveResult.Moved;
            _tagStartTime = DateTime.UtcNow;

            PositionCache.Cache.Clear();

            Navigator.Clear();
            Logger.Log("Initialized {0}", Status());
        }

        private void LogStatus()
        {
            if (QuestTools.EnableDebugLogging)
            {
                double distance = Math.Round(Position.Distance2D(ZetaDia.Me.Position)/10.0, 0)*10;

                Logger.Debug("Distance to target: {0:0} {1}", distance, Status());
            }
        }

        /// <summary>
        ///     Returns a friendly string of variables for logging purposes
        /// </summary>
        /// <returns></returns>
        private String Status()
        {
            if (QuestToolsSettings.Instance.DebugEnabled)
                return String.Format("questId=\"{0}\" stepId=\"{1}\" x=\"{2}\" y=\"{3}\" z=\"{4}\" pathPrecision={5} MoveResult={6} statusText={7}",
                    ZetaDia.CurrentQuest.QuestSNO, ZetaDia.CurrentQuest.StepId, X, Y, Z, PathPrecision, _lastMoveResult, StatusText);

            return string.Empty;
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            _lastGeneratedNavPoint = DateTime.MinValue;
            _lastMoveResult = MoveResult.Moved;
            _tagStartTime = DateTime.MinValue;
        }
    }
}