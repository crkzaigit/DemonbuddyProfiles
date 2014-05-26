using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Belphegor.Dynamics;
using Belphegor.GUI;
using Belphegor.Helpers;
using Belphegor.Routines;
using Belphegor.Settings;
using Belphegor.Utilities;
using log4net;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Common.Helpers;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace Belphegor
{
    public class Belphegor : CombatRoutine
    {
        public Belphegor()
        {
            Instance = this;
        }

        public static Belphegor Instance { get; private set; }

        #region Overrides of CombatRoutine

        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private static Version _version;
        private static readonly WaitTimer WorldTransferTimeoutTimer = WaitTimer.FiveSeconds;
        private static bool _eventsAttached;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Composite _buff;
        private Composite _combat;
        private Window _configWindow;
        private Helpers.CachedValue<SNOPower> _destroyPowerCache;
        private ActorClass _lastClass = ActorClass.Invalid;
        private Composite _movement;

        /// <summary>
        ///     Gets the name of this <see cref="CombatRoutine" />.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override string Name
        {
            get { return "Belphegor"; }
        }

        public static Version Version
        {
            get { return _version ?? (_version = Assembly.GetExecutingAssembly().GetName().Version); }
        }

        /// <summary>
        ///     Gets the configuration window.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override Window ConfigWindow
        {
            get
            {
                if (_configWindow != null) return _configWindow;
                _configWindow = new ConfigWindow("Configuration", Name,
                    "Version " + Version, 450, 500,
                    BelphegorSettings.Instance);
                _configWindow.Closed += ConfigWindowClosed;
                return _configWindow;
            }
        }

        /// <summary>
        ///     Gets the class.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override ActorClass Class
        {
            get
            {
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                {
                    // Return none if we are oog to make sure we can start the bot anytime.
                    return ActorClass.Invalid;
                }

                return ZetaDia.Me.ActorClass;
            }
        }

        public override float DestroyObjectDistance
        {
            get { return BelphegorSettings.Instance.DestroyObjectDistance; }
        }

        /// <summary> Gets the destroy object power. </summary>
        /// <value> The destroy object power. </value>
        public override SNOPower DestroyObjectPower
        {
            get
            {
                if (_destroyPowerCache == null)
                {
                    _destroyPowerCache = new Helpers.CachedValue<SNOPower>(
                        () => ZetaDia.CPlayer.GetPowerForSlot(HotbarSlot.HotbarMouseLeft),
                        TimeSpan.FromSeconds(10)
                        );
                }

                return _destroyPowerCache.Value;
            }
        }

        /// <summary>
        ///     Gets me.
        /// </summary>
        /// <remarks>Created 2012-05-08</remarks>
        public DiaActivePlayer Me
        {
            get { return ZetaDia.Me; }
        }

        /// <summary>
        ///     Gets the combat behavior.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override Composite Combat
        {
            get { return _combat; }
        }

        public override Composite Buff
        {
            get { return _buff; }
        }

        public Composite Movement
        {
            get { return _movement; }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            if (_configWindow != null)
                _configWindow.Close();
            Pulsator.OnPulse -= SetBehaviorPulse;


            GameEvents.OnLevelUp -= Monk.MonkOnLevelUp;
            GameEvents.OnLevelUp -= Wizard.WizardOnLevelUp;
            GameEvents.OnLevelUp -= WitchDoctor.WitchDoctorOnLevelUp;
            GameEvents.OnLevelUp -= DemonHunter.DemonHunterOnLevelUp;
            GameEvents.OnLevelUp -= Barbarian.BarbarianOnLevelUp;
            GameEvents.OnLevelUp -= Crusader.CrusaderOnLevelUp;
            GameEvents.OnWorldTransferStart -= HandleWorldTransfer;
            GameEvents.OnGameChanged -= OnWorldChanged;
            Log.Debug("Events Detached");
            _eventsAttached = false;
        }

        private void ConfigWindowClosed(object sender, EventArgs e)
        {
            BelphegorSettings.Instance.SaveAll();
            _configWindow.Closed -= ConfigWindowClosed;
            _configWindow = null;
        }

        /// <summary>
        ///     Initializes this <see cref="CombatRoutine" />.
        /// </summary>
        /// <remarks>Created 2012-04-03</remarks>
        public override void Initialize()
        {
            if (!_eventsAttached)
            {
                Log.Debug("Events Attached");
                GameEvents.OnLevelUp += Monk.MonkOnLevelUp;
                GameEvents.OnLevelUp += Wizard.WizardOnLevelUp;
                GameEvents.OnLevelUp += WitchDoctor.WitchDoctorOnLevelUp;
                GameEvents.OnLevelUp += DemonHunter.DemonHunterOnLevelUp;
                GameEvents.OnLevelUp += Barbarian.BarbarianOnLevelUp;
                GameEvents.OnLevelUp += Crusader.CrusaderOnLevelUp;
                GameEvents.OnWorldTransferStart += HandleWorldTransfer;
                GameEvents.OnWorldChanged += OnWorldChanged;
                _eventsAttached = true;
            }

            if (!CreateBehaviors())
            {
                BotMain.Stop();
                return;
            }

            _lastClass = Class;
            Pulsator.OnPulse += SetBehaviorPulse;
            Navigator.PlayerMover = new BelphegorPlayerMover();

            CombatTargeting.Instance.Provider = BelphegorCombatTargetingProvider.Instance;
            ObstacleTargeting.Instance.Provider = BelphegorObstacleTargetingProvider.Instance;

            Log.Info("Behaviors created");
        }

        private void OnWorldChanged(object sender, EventArgs eventArgs)
        {
            if (!ZetaDia.IsLoadingWorld)
            {
                Log.DebugFormat("World Changed To {0}, reload profile", ZetaDia.WorldInfo.Name);
                //Taken from trinity should resolve bugs with profiles
                string currentProfilePath = ProfileManager.CurrentProfile.Path;
                ProfileManager.Load(currentProfilePath);
                Navigator.SearchGridProvider.Update();
            }
        }

        private static void HandleWorldTransfer(object sender, EventArgs e)
        {
            WorldTransferTimeoutTimer.Reset();

            //Reload the profile on map change
            Profile current = ProfileManager.CurrentProfile;
            Log.DebugFormat("Reloading the profile on map change {0}", current.Name);
            ProfileManager.Load(current.Path);
        }

        public void SetBehaviorPulse(object sender, EventArgs args)
        {
            if (!WorldTransferTimeoutTimer.IsFinished)
                return;

            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Me != null && ZetaDia.Me.CommonData != null)
            {
                if (_combat == null || ZetaDia.Me.IsValid && Class != _lastClass)
                {
                    if (!CreateBehaviors())
                    {
                        BotMain.Stop();
                        return;
                    }

                    Log.Info("Behaviors created");
                    _lastClass = Class;
                }
            }
            Avoidance.IsAvoidanceCacheResetRequired = true;
        }

        public bool CreateBehaviors()
        {
            int count;

            _combat = CompositeBuilder.GetComposite(Class, BehaviorType.Combat, out count);
            if (count == 0 || _combat == null)
            {
                Log.InfoFormat("Combat support for {0} is not currently implemented.", Class);
                return false;
            }
            _combat = new Sequence(
                new Action(ctx =>
                {
                    if (BelphegorSettings.Instance.Debug.IsDebugTreeExecutionLoggingActive)
                    {
                        _stopwatch.Restart();
                    }
                }),
                _combat,
                new Action(ctx =>
                {
                    if (BelphegorSettings.Instance.Debug.IsDebugTreeExecutionLoggingActive)
                    {
                        _stopwatch.Stop();
                        Log.DebugFormat("Tree execution took {0}ms.", _stopwatch.ElapsedMilliseconds);
                    }
                })
                );

            _buff = CompositeBuilder.GetComposite(Class, BehaviorType.Buff, out count);
            if (count == 0 || _buff == null)
            {
                Log.InfoFormat("Buff support for {0} is not currently implemented.", Class);
            }

            _movement = CompositeBuilder.GetComposite(Class, BehaviorType.Movement, out count);
            if (count == 0 || _movement == null)
            {
                Log.InfoFormat("Movement support for {0} is not currently implemented.", Class);
            }

            Hotbar.Update();

            return true;
        }

        #endregion
    }
}