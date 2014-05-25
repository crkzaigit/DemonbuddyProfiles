using System;
using System.Collections.Generic;
using System.Linq;
using Belphegor.Settings;
using Belphegor.Utilities;
using log4net;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Belphegor.Helpers
{
    /// <summary> Hotbar. </summary>
    /// <remarks> Nesox, 2013-10-02. </remarks>
    public static class Hotbar
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static readonly List<HotbarPower> _powers = new List<HotbarPower>();
        private static readonly WaitTimer UpdateTimer = WaitTimer.FiveSeconds;

        static Hotbar()
        {
            Pulsator.OnPulse += HotbarOnPulse;
        }

        /// <summary> Gets the powers. </summary>
        /// <value> The powers. </value>
        public static IEnumerable<HotbarPower> Powers
        {
            get { return _powers; }
        }

        private static void HotbarOnPulse(object sender, EventArgs e)
        {
            Update();
        }

        /// <summary> Updates this object. </summary>
        /// <remarks> Nesox, 2013-10-02. </remarks>
        public static void Update()
        {
            if (!UpdateTimer.IsFinished)
                return;
            using (new PerformanceLogger(BelphegorSettings.Instance.Debug.IsDebugHotbarCacheLog, "HotbarUpdate"))
            {
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld || ZetaDia.Me == null || ZetaDia.Me.CommonData == null)
                    return;

                UpdateTimer.Reset();
                _powers.Clear();

                var slots = new[]
                {
                    HotbarSlot.HotbarMouseLeft,
                    HotbarSlot.HotbarMouseRight,
                    HotbarSlot.HotbarSlot1,
                    HotbarSlot.HotbarSlot2,
                    HotbarSlot.HotbarSlot3,
                    HotbarSlot.HotbarSlot4
                };

                foreach (HotbarPower hotbarPower in slots.Select(slot => new HotbarPower(slot)))
                {
                    _powers.Add(hotbarPower);
                    if (BelphegorSettings.Instance.Debug.IsDebugHotbarCacheLog)
                        Log.DebugFormat("Added Power:{0} RuneIndex:{1}", hotbarPower.Power, hotbarPower.RuneIndex);
                }

                if (HasPower(SNOPower.Wizard_Archon))
                {
                    if (BelphegorSettings.Instance.Debug.IsDebugHotbarCacheLog)
                        Log.Debug("Found Archon manually adding skills");

                    _powers.Add(new HotbarPower(SNOPower.Wizard_Archon_ArcaneBlast, 0));
                    _powers.Add(new HotbarPower(SNOPower.Wizard_Archon_ArcaneStrike, 0));
                    _powers.Add(new HotbarPower(SNOPower.Wizard_Archon_DisintegrationWave, 0));

                    if (HasRune(SNOPower.Wizard_Archon, 1))
                        _powers.Add(new HotbarPower(SNOPower.Wizard_Archon_SlowTime, 0));
                    if (HasRune(SNOPower.Wizard_Archon, 2))
                        _powers.Add(new HotbarPower(SNOPower.Wizard_Archon_Teleport, 0));
                }
            }
        }

        /// <summary> Query if this object contains the given power. </summary>
        /// <remarks> Nesox, 2013-10-04. </remarks>
        /// <param name="power"> The SNOPower to test for containment. </param>
        /// <returns> true if the object is in this collection, false if not. </returns>
        public static bool HasPower(SNOPower power)
        {
            return Powers.Any(p => p.Power == power);
        }

        public static bool HasRune(SNOPower power, int runeIndex)
        {
            return Powers.Any(p => p.Power == power && p.RuneIndex == runeIndex);
        }
    }

    /// <summary> Hotbar power. </summary>
    /// <remarks> Nesox, 2013-10-02. </remarks>
    public class HotbarPower
    {
        /// <summary> Constructor. </summary>
        /// <remarks> Nesox, 2013-10-02. </remarks>
        /// <param name="slot"> The slot. </param>
        public HotbarPower(HotbarSlot slot)
        {
            Power = ZetaDia.CPlayer.GetPowerForSlot(slot);
            RuneIndex = ZetaDia.CPlayer.GetRuneIndexForSlot(slot);
        }

        public HotbarPower(SNOPower power, int index)
        {
            Power = power;
            RuneIndex = index;
        }

        /// <summary> Gets or sets the power. </summary>
        /// <value> The power. </value>
        public SNOPower Power { get; private set; }

        /// <summary> Gets or sets zero-based index of the run. </summary>
        /// <value> The run index. </value>
        public int RuneIndex { get; private set; }


        public static explicit operator SNOPower(HotbarPower power)
        {
            return power.Power;
        }
    }
}