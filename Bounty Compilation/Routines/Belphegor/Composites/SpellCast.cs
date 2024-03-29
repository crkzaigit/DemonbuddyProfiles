﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Belphegor.Helpers;
using Belphegor.Settings;
using Belphegor.Utilities;
using log4net;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;

namespace Belphegor.Composites
{
    public class SpellCast : Composite
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        static SpellCast()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpellCast" /> class.
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="posRetriever">The pos retriever.</param>
        /// <param name="dynWorldRetriever">The dyn world retriever.</param>
        /// <param name="targetRetriever">The target retriever.</param>
        /// <param name="extra">The extra.</param>
        /// <param name="suceedRunner">The suceed runner.</param>
        /// <remarks> Created 2012-06-18 </remarks>
        public SpellCast(SNOPower power, ValueRetriever<Vector3> posRetriever = null,
            ValueRetriever<int> dynWorldRetriever = null, ValueRetriever<int> targetRetriever = null,
            ValueRetriever<bool> extra = null, ActionSucceedDelegate suceedRunner = null)
        {
            Power = power;
            PositionRetriever = posRetriever;
            DynamicWorldIdRetriever = dynWorldRetriever;
            TargetGuidRetriever = targetRetriever;
            ExtraCondition = extra;
            SucceedRunner = suceedRunner;
        }

        /// <summary>
        ///     Initializes a new instance of the SpellCast class.
        /// </summary>
        public SpellCast(SNOPower power, ValueRetriever<Vector3> positionRetriever = null,
            ValueRetriever<int> dynamicWorldIdRetriever = null,
            ValueRetriever<int> targetGuidRetriever = null, ValueRetriever<bool> extraCondition = null,
            ActionSucceedDelegate succeedRunner = null, ValueRetriever<bool> keepSpamming = null,
            ContextChangeHandler contextChangeHandler = null)
        {
            Power = power;
            PositionRetriever = positionRetriever;
            DynamicWorldIdRetriever = dynamicWorldIdRetriever;
            TargetGuidRetriever = targetGuidRetriever;
            ExtraCondition = extraCondition;
            KeepSpamming = keepSpamming;
            ContextChangeHandler = contextChangeHandler;
            SucceedRunner = succeedRunner;
        }

        private SNOPower Power { get; set; }

        private ContextChangeHandler ContextChangeHandler { get; set; }

        private ValueRetriever<Vector3> PositionRetriever { get; set; }

        private ValueRetriever<int> DynamicWorldIdRetriever { get; set; }

        private ValueRetriever<int> TargetGuidRetriever { get; set; }

        private ValueRetriever<bool> ExtraCondition { get; set; }

        private ValueRetriever<bool> KeepSpamming { get; set; }

        private ActionSucceedDelegate SucceedRunner { get; set; }

        #region Overrides of Composite

        private static readonly Stopwatch CastLogSw = new Stopwatch();
        private bool _isSpamming;

        public override RunStatus Tick(object context)
        {
            if (!Hotbar.HasPower(Power))
                return RunStatus.Failure;

            if (ContextChangeHandler != null && context == null)
            {
                CombatTargeting.Instance.Pulse();
                context = ContextChangeHandler(null);
            }
            if (context == null)
                return RunStatus.Failure;

            bool minReqs = ExtraCondition == null || ExtraCondition(context);
            if (!minReqs)
                return RunStatus.Failure;

            if (!_isSpamming)
            {
                using (new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugCanCastLogging, "CanCast"))
                {
                    bool canCast = PowerManager.CanCast(Power);
                    if (!canCast)
                        return RunStatus.Failure;
                }
            }

            Vector3 clickPosition = PositionRetriever != null ? PositionRetriever(context) : Vector3.Zero;
            int worldId = DynamicWorldIdRetriever != null ? DynamicWorldIdRetriever(context) : 0;
            int acdGuid = TargetGuidRetriever != null ? TargetGuidRetriever(context) : -1;
            bool keepSpamming = KeepSpamming != null && KeepSpamming(context);

            bool castSucceeded = ZetaDia.Me.UsePower(Power, clickPosition, worldId, acdGuid);


            if (!castSucceeded)
                return RunStatus.Failure;

            if (BelphegorSettings.Instance.Debug.IsDebugCastLoggingActive && !keepSpamming)
            {
                CastLogSw.Stop();
                string castOn = acdGuid == -1
                    ? "at location " + clickPosition
                    : "on Unit " + acdGuid.ToString(CultureInfo.InvariantCulture);
                Log.InfoFormat("Using Power: {0} {1}, delay from last cast is {2}ms", Power.ToString(), castOn,
                    CastLogSw.ElapsedMilliseconds);
                CastLogSw.Restart();
            }

            if (!_isSpamming && SucceedRunner != null)
            {
                SucceedRunner(context);
            }

            if (keepSpamming)
            {
                _isSpamming = true;
                if (BelphegorSettings.Instance.Debug.IsDebugCastLoggingActive)
                {
                    CastLogSw.Stop();
                    string castOn = acdGuid == -1
                        ? "at location " + clickPosition
                        : "on Unit " + acdGuid.ToString(CultureInfo.InvariantCulture);
                    Log.InfoFormat("Using Power: Is Spamming {0} {1}, delay from last cast is {2}ms",
                        Power.ToString(), castOn, CastLogSw.ElapsedMilliseconds);
                    CastLogSw.Restart();
                }
                return RunStatus.Running;
            }
            _isSpamming = false;
            return RunStatus.Success;
        }

        protected override IEnumerable<RunStatus> Execute(object context)
        {
            yield return Tick(context);
        }

        #endregion
    }

    public class SelfCast : SpellCast
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfCast" /> class.
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="extra">The extra.</param>
        /// <param name="succeedRunner">The succeed runner.</param>
        /// <param name="keepSpamming">Casts the spell multiple times for one second</param>
        /// <param name="contextChangeHandler"></param>
        /// <remarks>Created 2012-06-18</remarks>
        public SelfCast(SNOPower power, ValueRetriever<bool> extra = null, ActionSucceedDelegate succeedRunner = null,
            ValueRetriever<bool> keepSpamming = null, ContextChangeHandler contextChangeHandler = null)
            : base(
                power, null, ctx => ZetaDia.CurrentWorldDynamicId, null, extra, succeedRunner, keepSpamming,
                contextChangeHandler)
        {
        }
    }

    public class CastOnUnit : SpellCast
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CastOnUnit" /> class.
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="targetRetriever">The target retriever.</param>
        /// <param name="extra">The extra.</param>
        /// <param name="succeedRunner">The succeed runner.</param>
        /// <param name="keepSpamming">Casts the spell multiple times for one second</param>
        /// <param name="contextChangeHandler"></param>
        /// <remarks> Created 2012-06-18 </remarks>
        public CastOnUnit(SNOPower power, ValueRetriever<int> targetRetriever, ValueRetriever<bool> extra = null,
            ActionSucceedDelegate succeedRunner = null, ValueRetriever<bool> keepSpamming = null,
            ContextChangeHandler contextChangeHandler = null)
            : base(power, null, null, targetRetriever, extra, succeedRunner, keepSpamming, contextChangeHandler)
        {
        }
    }

    public class CastAtLocation : SpellCast
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SpellCast" /> class.
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="posRetriever">The pos retriever.</param>
        /// <param name="extra">The extra.</param>
        /// <param name="succeedRunner">The succeed runner.</param>
        /// <param name="keepSpamming">Casts the spell multiple times for one second</param>
        /// <param name="contextChangeHandler"></param>
        /// <remarks>Created 2012-06-18</remarks>
        public CastAtLocation(SNOPower power, ValueRetriever<Vector3> posRetriever, ValueRetriever<bool> extra = null,
            ActionSucceedDelegate succeedRunner = null, ValueRetriever<bool> keepSpamming = null,
            ContextChangeHandler contextChangeHandler = null)
            : base(
                power, posRetriever, ctx => ZetaDia.CurrentWorldDynamicId, null, extra, succeedRunner, keepSpamming,
                contextChangeHandler)
        {
        }
    }
}