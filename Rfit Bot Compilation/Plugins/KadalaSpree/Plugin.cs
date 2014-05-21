using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Zeta.Game.Internals;


namespace KadalaSpree
{

    #region Enum

    /// <summary>
    /// All slot types for gambling
    /// Credit to HerbFunk, who i copied this from.
    /// </summary>
    public enum BloodShardGambleItems
    {
        None = 0,
        OneHandItem,
        TwoHandItem,
        Quiver,
        Orb,
        Mojo,
        Helm,
        Gloves,
        Boots,
        Chest,
        Belt,
        Shoulders,
        Pants,
        Bracers,
        Shield,
        Ring,
        Amulet
    }

    public enum Destination
    {
        None = 0,
        Kadala,
        Orek,
        Enchantress,
        Waypoint,
        Point,
        NephalemObelisk
    }

    #endregion

    public class Plugin : IPlugin
    {

        private const int TIMEOUT = 30;
        private const int MIN_BACKPACK_SLOTS_EMPTY = 2;
        private const int INTERACT_RANGE = 7;
        private const int MIN_GAMBLE_DELAY = 150;
        private const int MAX_GAMBLE_DELAY = 350;
        private const int PULSE_INTERVAL = 5000;
        private const int MAX_FAILED_PURCHASE_ATTEMPTS = 5;
        private const int MAX_ACTOR_SEARCH_DISTANCE = 500;
        private const int MAX_RESTARTS = 5;

        public static string NAME = "KadalaSpree";
        public static Version VERSION = new Version(1, 0, 11);
        public static string AUTHOR = "XZJV with code by rrrix & HerbFunk";
        public static string DESCRIPTION = "Buys stuff from kadala when doing a town run";

        #region Plugin Definition

        public string Name { get { return NAME; } }
        public Version Version { get { return VERSION; } }
        public string Author { get { return AUTHOR; } }
        public string Description { get { return DESCRIPTION; } }
        public bool Equals(IPlugin other) { return (other.Name == Name) && (other.Version == Version); }
        public System.Windows.Window DisplayWindow { get { return Config.GetDisplayWindow(); } }

        #endregion

        #region Variables etc

        internal static bool _isDone = false;
        internal static bool _townRunInitialized = false;
        internal static bool _townRunFinished = true;
        internal static int _nextItemId = -1;
        internal static int _failedPurchaseAttempts = 0;
        internal static int _lastBackpackItemsCount = 0;
        internal static int _restartCount = 0;
        internal static Random rnd = new Random();
        internal static DateTime _startTime = DateTime.MinValue;
        internal static DateTime _lastGambleTime = DateTime.MinValue;
        internal static DateTime _endSpreeTime = DateTime.MinValue;
        internal static DiaUnit _kadala;
        internal static Stopwatch pulseTimer = new Stopwatch();
        internal static Destination _destination;
        internal static DiaObject _destinationActor;
        internal static Vector3 _destinationPosition;
        internal static BloodShardGambleItems _nextItemSlot = BloodShardGambleItems.None;
        internal static List<Destination> _visited = new List<Destination>();
        internal static List<BloodShardGambleItems> _validGambleSlots = new List<BloodShardGambleItems>();
        internal static List<ACDItem> Elements = new List<ACDItem>();
        internal static Action AlwaysSucceed { get { return new Action(ret => RunStatus.Success); } }
        internal static Action AlwaysFail { get { return new Action(ret => RunStatus.Failure); } }

        #endregion

        #region DB Events, Overrides

        public void OnEnabled() { 
            BotMain.OnStart += KadalaSpreeOnStart;
            GameEvents.OnGameChanged += KadalaSpreeOnGameChanged;
        }       
 
        public void OnDisabled() {  
            BotMain.OnStart -= KadalaSpreeOnStop;
            GameEvents.OnGameChanged -= KadalaSpreeOnGameChanged;
        }

        public void OnInitialize() 
        {
            Logger.Log("v{0} Initialized!", Version);
        }

        public void OnShutdown() 
        {
            Logger.Log("Shutdown!");           
        }
        
        public void OnPulse() 
        { 
            if (!pulseTimer.IsRunning) { 
                pulseTimer.Start(); 
            };

            CheckBloodShardThreshold();
        }

        public void KadalaSpreeOnGameChanged(object sender, EventArgs e)
        {
            ResetKadala();
        }

        public void KadalaSpreeOnStart(IBot bot)
        {
            ResetKadala();
            
            try
            {
                // This is where we broadcast our pirate signal and hack into the TownRun
                // Insert early in the TownRun PrioritySelect; plugin will fall-through 
                // when needed to allow lower ranked behaviors (stash/sell/salvage) to run.
                VendorRunPrioritySelector.InsertChild(3, KadalaMaster());                
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to bind Kadala sequence {0}", ex);
            }
        }

        public void KadalaSpreeOnStop(IBot bot)
        {
            try
            {
                VendorRunPrioritySelector.Children.Remove(KadalaMaster());
                ResetKadala();
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to remove Kadala sequence from town run. {0}", ex);
            }
        }

        #endregion

        #region Reset

        /// <summary>
        /// Reset the Spree behavior
        /// </summary>
        public void ResetKadala()
        {
            Logger.Debug("Reseting KadalaSpree");
            _isDone = false;
            _townRunInitialized = false;                
            _startTime = DateTime.MinValue;
            _kadala = null;
            _failedPurchaseAttempts = 0;
            _destination = Destination.None;
            _visited = new List<Destination>();
            pulseTimer.Stop();
            ResetValidSlots();
        }

        /// <summary>
        /// Reset the Master behavior
        /// </summary>
        internal static void ResetKadalaMaster()
        {
            Logger.Debug("Reseting KadalaMaster");
            _restartCount = 0;
            _townRunFinished = true;
        }

        /// <summary>
        /// Reset the collection of items to buy to the slots checked in settings.
        /// </summary>
        internal static void ResetValidSlots()
        {
            _validGambleSlots = KadalaSpreeSettings.Instance.SelectedGambleSlots;

            if (!_validGambleSlots.Any())
            {
                Logger.Log("No slots were selected to gamble for, check your settings");
                _isDone = true;
            }
        }

        #endregion

        /// <summary>
        /// Checks if we should force a town run every PULSE_INTERVAL, while not in town
        /// </summary>
        private void CheckBloodShardThreshold()
        {
            if (pulseTimer.ElapsedMilliseconds > PULSE_INTERVAL)
            {
                if (ZetaDia.CPlayer != null && ZetaDia.CPlayer.IsValid && !ZetaDia.IsInTown
                    && !ZetaDia.IsLoadingWorld && !ZetaDia.IsPlayingCutscene && !IsCastingOrChanneling)
                {
                    if (IsAboveShardThreshold)
                    {
                        if (IsAboveMinimumShards)
                        {
                            ResetKadala();
                            ResetKadalaMaster();
                            Zeta.Bot.Logic.BrainBehavior.ForceTownrun(string.Format("Enough bloodshards for a spree! Shards={0} Threshold={1}", ZetaDia.CPlayer.BloodshardCount, KadalaSpreeSettings.Instance.ForceSpreeThreshold), false);
                        }
                        else
                        {
                            Logger.Debug("Enough shards to force town run {0}. However, minimum bloodshards is set too high {1}", KadalaSpreeSettings.Instance.ForceSpreeThreshold, KadalaSpreeSettings.Instance.MinimumBloodShards);
                        }
                    }
                    Logger.Debug("Checking Bloodshard Count - Shards={0} Threshold={1} MinShards={2} SaveUntilThreshold={3}", ZetaDia.CPlayer.BloodshardCount, KadalaSpreeSettings.Instance.ForceSpreeThreshold, KadalaSpreeSettings.Instance.MinimumBloodShards, KadalaSpreeSettings.Instance.SaveUntilThreshold);

                }

                pulseTimer.Reset();
            }

            if (!_townRunFinished && !ZetaDia.IsInTown && !Zeta.Bot.Logic.BrainBehavior.IsVendoring)
            {
                ResetKadala();
                ResetKadalaMaster();
                Logger.Debug("Town Run is Finished");
            }
        }        

        /// <summary>
        /// Determines if a gambling spree should be performed or
        /// if other townrun tasks should be allowed to run.
        /// </summary>
        public Composite KadalaMaster()
        {
            return
            new Sequence(

                new Action(ret =>
                {
                    if (_townRunFinished)
                    {
                        _townRunFinished = false;
                    }

                    Logger.Debug(">>>> _isDone={0} Restarts={3}/{4} IsSpaceForItem={5} IsDoneVendoring={1} EnoughShards={6} KadalaSpree()={2}", _isDone, IsDoneVendoring, !_isDone || IsDoneVendoring, _restartCount, MAX_RESTARTS, IsSpaceForItem, IsAboveMinimumShards);

                    // Check if we should do another spree
                    if (_isDone && ZetaDia.IsInTown && IsSpaceForItem && IsDoneVendoring && IsAboveMinimumShards && !KadalaSpreeSettings.Instance.SaveUntilThreshold)
                    {
                        if (_restartCount <= MAX_RESTARTS)
                        {
                            Logger.Debug("We still have shards to spend, restarting spree");
                            _restartCount++;
                            ResetKadala();
                        }
                        else
                        {
                            Logger.Debug("Restart limit reached!");
                            return RunStatus.Failure;
                        }
                    }

                    return RunStatus.Success;
                }),

                // Only run a spree if it has already started, or if we're not doing other stuff like stashing/selling
                new Decorator(ret => !_isDone || IsDoneVendoring,
                    KadalaSpree()
                )

            );
        }

        /// <summary>
        /// Behavior for moving to Kadala and gambling for items 
        /// </summary>
        public static Composite KadalaSpree()
        {
            return new Sequence(

                new DecoratorContinue(ret => _isDone, AlwaysFail),

                // First time stuff
                new DecoratorContinue(ret => !_townRunInitialized,
                    new Action(ret =>
                    {
                        Logger.Debug("-- Kadala Spree Started --");
                        _startTime = DateTime.UtcNow;
                        _townRunInitialized = true;
                        _townRunFinished = false;
                    })
                ),

                // Make sure we are in town, alive etc
                new DecoratorContinue(ret => !IsPlayerValid,
                    new Action(ret =>
                    {
                        Logger.Debug("Character is Invalid!");
                        _startTime = DateTime.UtcNow;
                        _isDone = true;
                    })
                ),

                // Make sure we are in town, alive etc
                new DecoratorContinue(ret => !ZetaDia.IsInTown,
                    new Action(ret =>
                    {
                        Logger.Debug("Not in Town!");
                        _startTime = DateTime.UtcNow;
                        _isDone = true;
                    })
                ),

                // Check if we should be saving up our shards
                new DecoratorContinue(ret => KadalaSpreeSettings.Instance.SaveUntilThreshold && !IsAboveShardThreshold,
                    new Action(ret =>
                    {
                        Logger.Debug("Skipping Kadala because we're saving shards - required={0} current={1}", KadalaSpreeSettings.Instance.ForceSpreeThreshold, ZetaDia.CPlayer.BloodshardCount);
                        _isDone = true;
                    })
                ),

                // Check timeout
                new DecoratorContinue(ret => HasTimedOut,
                    new Action(ret =>
                    {
                        Logger.Debug("Timeout of {0} seconds was reached!", TIMEOUT);
                        _isDone = true;
                    })
                ),

                // Check bag slots
                new DecoratorContinue(ret => !IsSpaceForItem,
                    new Action(ret =>
                    {
                        Logger.Debug("Not enough space in backpack for Gambling");
                        _isDone = true;
                    })
                ),

                // Check Bloodshard Count
                new DecoratorContinue(ret => !IsAboveMinimumShards,
                    new Action(ret =>
                    {
                        Logger.Debug("Not Enough Blood Shards for Gambling - Current={0}, Minimum={1} ", ZetaDia.CPlayer.BloodshardCount, KadalaSpreeSettings.Instance.MinimumBloodShards);
                        _isDone = true;
                    })
                ),

                new DecoratorContinue(ret => !_isDone,

                    new PrioritySelector(

                        // Where are we going?
                        new Decorator(ret => _destination == Destination.None,
                            SelectDestination()
                        ),

                        Move(),

                        // Loop back around if we're not near kadala; ensures we select a new destination.
                        new Decorator(ret => !IsKadalaValid || !IsWithinRange(_kadala.Position), AlwaysSucceed),

                        // Wait for bot to stop moving
                        new Decorator(ret => ZetaDia.Me.Movement.IsMoving,
                            new Wait(TimeSpan.FromMilliseconds(500), ret => false, AlwaysFail)
                        ),

                        new Action(ret=>{
                            Logger.Debug("IsVendorWindowOpen={0} VendorWindow.IsVisible={1}, VendorWindow.IsValid={2}", !IsVendorWindowOpen, UIElements.VendorWindow.IsValid, UIElements.VendorWindow.IsVisible);
                            return RunStatus.Failure;
                        }),

                        // Is kadala shop window open?
                        new Decorator(ret => !IsVendorWindowOpen,
                            new Sequence(
                                InteractWithKadala(),                             
                                new WaitContinue(1, ret => false, AlwaysSucceed)
                            )
                        ),

                        // Get the store elements to click on from the vendor window
                        new Decorator(ret => !AreVendorElementsValid,
                            new Action(ret => UpdateVendorElements())
                        ),

                        new Decorator(ret=>IsVendorWindowOpen,
                            new Sequence(
                                new Action(ret => Logger.Debug("Vendor Window is Open")),
                                Gamble()
                            )                           
                        )
                        
                    )

                ),

                new DecoratorContinue(ret => _isDone,
                    new Action(ret =>
                    {
                        Logger.Debug("-- Kadala Spree Finished --");
                        _endSpreeTime = DateTime.UtcNow;
                        return RunStatus.Failure;
                    })
                )

            );
        }

        /// <summary>
        /// Finds a target to move to nearby, preferably Kadala, but will select 
        /// other targets until kadala is found or all valid targets have been visited
        /// </summary>
        internal static Composite SelectDestination()
        {
            return new Sequence(

                new DecoratorContinue(ret => _visited.Contains(Destination.Kadala), AlwaysFail),

                new PrioritySelector(

                    new Decorator(ret => Find(Destination.Kadala),
                        new Action(ret => _destination = Destination.Kadala)),

                    new Decorator(ret => !_visited.Contains(Destination.Waypoint) && Find(Destination.Waypoint),
                        new Action(ret => _destination = Destination.Waypoint)),

                    new Decorator(ret => !_visited.Contains(Destination.Orek) && Find(Destination.Orek),
                        new Action(ret => _destination = Destination.Orek)),

                    new Decorator(ret => !_visited.Contains(Destination.NephalemObelisk) && Find(Destination.NephalemObelisk),
                        new Action(ret => _destination = Destination.NephalemObelisk)),

                    new Action(ret => 
                    {
                        Logger.Log("Failed all attempts to find Kadala");
                        _isDone = true;
                    })
                )
            );
        }

        /// <summary>
        /// Buy as many items as we can from Kadala
        /// </summary>
        private static Composite Gamble()
        {
            return new Action(ret =>
            {

                if (_failedPurchaseAttempts >= MAX_FAILED_PURCHASE_ATTEMPTS)
                {
                    Logger.Debug("Failed purchase limit reached!");
                    _isDone = true;
                    return RunStatus.Success;
                }

                if (IsAboveMinimumShards && IsSpaceForItem && !HasTimedOut)
                {
                    if (CanGamble)
                    {
                        CheckLastPurchaseFailed();
                        ChooseNextItem();
                        BuyNextItem();
                    }
                    return RunStatus.Running;
                }
                return RunStatus.Success;

            });

        }

        #region Actors

        /// <summary>
        /// Searches for the DiaObject of a 'Destination'
        /// </summary>
        public static bool Find(Destination destination)
        {
            
            _destinationActor = null;

            var actorId = DestinationAndActorId.FirstOrDefault(row => row.Key == destination).Value;            

            if (actorId > 0)
            {
                var actor = ZetaDia.Actors.GetActorsOfType<DiaObject>(true).FirstOrDefault<DiaObject>(a => a.ActorSNO == actorId);

                if (actor != null && actor.IsValid && actor.Position.Distance2D(ZetaDia.Me.Position) < MAX_ACTOR_SEARCH_DISTANCE)
                {
                    _destination = destination;
                    _destinationActor = actor;
                    _destinationPosition = actor.Position;

                    if (destination == Destination.Kadala)
                    {
                        _kadala = actor as DiaUnit;
                    }

                    Logger.Debug("Found {0}!", destination);

                    return true;
                }
            }

            Logger.Debug("Couldnt Find {0}!", destination);

            return false;

        }

        /// <summary>
        /// Talk to Kadala
        /// </summary>
        private static Composite InteractWithKadala()
        {
            return new Action(ret =>
            {
                try
                {
                    Logger.Debug("Talking to Kadala");

                    if (_kadala != null && _kadala.IsValid)
                    {
                        _kadala.Interact();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to Interact with Kadala {0}", ex);
                }
                return RunStatus.Success;
            });
        }

        #endregion

        #region Movement

        /// <summary>
        /// Handles moving to Waypoint
        /// Credit to rrrix, whom i copied most of this this from. 
        /// </summary>
        private static Composite Move()
        {
            return
            new Sequence(

                // Dont go anywhere if we havent selected a place to go.
                new DecoratorContinue(ret => _destination == Destination.None, AlwaysFail),

                // Stop if we're already within interact range
                new DecoratorContinue(ret => IsWithinRange(_destinationPosition),
                    DestinationReached()
                ),

                // Keep moving
                new Switch<MoveResult>(ret => MoveTo(_destinationPosition, _destination.ToString()),
                    new SwitchArgument<MoveResult>(MoveResult.ReachedDestination,
                        DestinationReached()
                    ),
                    new SwitchArgument<MoveResult>(MoveResult.Moved,
                        new Sequence(
                            new Action(ret => Logger.Debug("Moving to {0}: x={1} y={2} z={3} distance={4}", _destination, _destinationPosition.X, _destinationPosition.Y, _destinationPosition.Z, _destinationPosition.Distance2D(ZetaDia.Me.Position)))
                        )
                    ),
                    new SwitchArgument<MoveResult>(MoveResult.PathGenerationFailed,
                        new Sequence(
                            new Action(ret => Logger.Log("Move Failed: {0}!", ret)),
                            new Action(ret => _isDone = true)
                        )
                    ),
                    new SwitchArgument<MoveResult>(MoveResult.Failed,
                        new Sequence(
                            new Action(ret => Logger.Log("Move Failed: {0}!", ret)),
                            new Action(ret => _isDone = true)
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Update the destination and log that we have arrived
        /// </summary>
        public static Action DestinationReached()
        {
            return new Action(ret =>
            {
                Logger.Debug("Reached {0}, marking as visited!", _destination);

                if (!_visited.Contains(_destination))
                {
                    _visited.Add(_destination);
                }

                _destination = Destination.None;

                return RunStatus.Failure;
            });
        }

        /// <summary>
        /// Moves to a given position
        /// Credit to rrrix, whom i copied this from.
        /// </summary>
        public static MoveResult MoveTo(Vector3 destination, string destinationName = null, bool useRaycast = true, bool useNavigator = false)
        {
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.Me.IsDead || ZetaDia.IsLoadingWorld || !ZetaDia.Service.IsValid || !ZetaDia.WorldInfo.IsValid)
            {
                return MoveResult.Failed;
            }
            try
            {
                return Navigator.MoveTo(destination, destinationName, useRaycast);
            }
            catch (Exception ex)
            {
                Logger.Log("{0}", ex);
                return MoveResult.Failed;
            }
        }

        #endregion

        #region Items

        /// <summary>
        /// Decide what we should buy from the store
        /// </summary>
        internal static RunStatus ChooseNextItem()
        {
            if (!_validGambleSlots.Any())
            {
                ResetValidSlots();
            }

            _nextItemSlot = _validGambleSlots[rnd.Next(_validGambleSlots.Count)];
            _nextItemId = GetGambleItemDynamicID(_nextItemSlot);

            if (_nextItemId != -1)
            {
                _validGambleSlots.Remove(_nextItemSlot);
                _lastGambleTime = DateTime.UtcNow;
                return RunStatus.Success;
            }
            else
            {
                return RunStatus.Failure;
            }
        }

        /// <summary>
        /// Buys the item from the store
        /// </summary>
        internal static RunStatus BuyNextItem()
        {
            try
            {
                if (_nextItemId != -1)
                {

                    Logger.Debug("Buying: {0}", _nextItemSlot);

                    _lastBackpackItemsCount = BackPackItemCount;

                    ZetaDia.Me.Inventory.BuyItem(_nextItemId);

                    _nextItemId = -1;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception Buying Item ", ex);
            }
            return RunStatus.Success;
        }

        /// <summary>
        /// Get a shop window UI element DynamicId
        /// Credit to HerbFunk, who i copied this from.
        /// </summary>
        internal static int GetGambleItemDynamicID(BloodShardGambleItems type)
        {
            if (!AreVendorElementsValid)
            {
                UpdateVendorElements();
            }

            var internalName = InternalNameAndEnumType.FirstOrDefault(i => i.Value == type).Key;

            var element = Elements.Find(e => e.InternalName.StartsWith(internalName));

            if (element != null && element.IsValid)
            {
                return element.DynamicId;
            }

            return -1;
        }

        /// <summary>
        /// Gets the shop window UI elements
        /// Credit to HerbFunk, who i copied this idea from.
        /// </summary>
        internal static void UpdateVendorElements()
        {
            Elements.Clear();
            foreach (var i in ZetaDia.Actors.ACDList)
            {
                try
                {
                    if (i is ACDItem)
                    {
                        var item = i as ACDItem;

                        if (item.InternalName.Contains("PH_"))
                        {
                            Logger.Debug("Found ACDItem InternalName={0} Name={1}", item.InternalName, item.Name);
                            Elements.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception: {0}", ex);
                }
            }
        }

        /// <summary>
        /// Checks if the last item we tried to purchase, was actually purchased
        /// </summary>
        private static void CheckLastPurchaseFailed()
        {
            if (_lastBackpackItemsCount == BackPackItemCount)
            {
                Logger.Log("Failed to buy item");
                _failedPurchaseAttempts++;
            }
        }

        #endregion       

        #region Conditions

        /// <summary>
        /// Checks if bot is within INTERACT_RANGE of a position
        /// </summary>
        /// <param name="position">Vector3 of the position to be within range of</param>
        private static bool IsWithinRange(Vector3 position)
        {
            return position != null && position != new Vector3() && position.Distance2D(ZetaDia.Me.Position) <= INTERACT_RANGE;
        }

        /// <summary>
        /// Checks if we can add a TwoSlot item to the backpack.
        /// </summary>
        public static bool IsSpaceForItem
        {
            get
            {
                var notfound = new Vector2(-1, -1);

                var unset = new Vector2(-2, -2);

                var twoItemLocation = FindValidBackpackLocation(true, MIN_BACKPACK_SLOTS_EMPTY);

                var result = (twoItemLocation != notfound && twoItemLocation != unset && ZetaDia.Me.Inventory.NumFreeBackpackSlots >= MIN_BACKPACK_SLOTS_EMPTY);

                if (!result)
                {
                    Logger.Debug("Backpack check - SpaceForTwoSlotItem={0} OneSlotSpaces={1} ItemCount={2}", (twoItemLocation != unset && twoItemLocation != notfound), ZetaDia.Me.Inventory.NumFreeBackpackSlots, BackPackItemCount);
                }

                return (twoItemLocation != unset && twoItemLocation != notfound);
            }
        }

        /// <summary>
        /// Checks if we have the minimum bloodshards available.
        /// </summary>
        public static bool IsAboveMinimumShards 
        { 
            get 
            {
                return ZetaDia.CPlayer.BloodshardCount >= KadalaSpreeSettings.Instance.MinimumBloodShards;
            } 
        }

        /// <summary>
        /// Checks if we have enough shards to force a townrun
        /// </summary>
        public static bool IsAboveShardThreshold 
        { 
            get 
            {
                return ZetaDia.CPlayer.BloodshardCount >= KadalaSpreeSettings.Instance.ForceSpreeThreshold;
            } 
        }

        /// <summary>
        /// Checks time since last purchase to avoid buying items at maximum speed.
        /// </summary>
        public static bool CanGamble
        {
            get
            {
                return (_lastGambleTime == DateTime.MinValue || DateTime.UtcNow.Subtract(_lastGambleTime).TotalMilliseconds > rnd.Next(MIN_GAMBLE_DELAY, MAX_GAMBLE_DELAY));
            }
        }

        /// <summary>
        /// Checks current time elapsed against timeout setting
        /// </summary>
        public static bool HasTimedOut
        {
            get
            {
                return DateTime.UtcNow.Subtract(_startTime).TotalSeconds >= TIMEOUT;
            }
        }

        /// <summary>
        /// Make sure we're in a state to gamble
        /// </summary>
        public static bool IsPlayerValid
        {
            get
            {
                return ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && ZetaDia.Me != null && ZetaDia.Me.CommonData != null;
            }
        }

        /// <summary>
        /// Make sure we're in a state to gamble
        /// </summary>
        public static bool IsCastingOrChanneling
        {
            get
            {
                return CurrentAnimationState == AnimationState.Casting || CurrentAnimationState == AnimationState.Channeling;
            }
        }

        /// <summary>
        /// Checks if vendor window UIElements are up to date
        /// </summary>
        public static bool AreVendorElementsValid
        {
            get
            {
                try
                {
                    if (Elements != null && Elements.Count>0)
                    {
                        foreach(var ele in Elements)
                        {
                            if (ele == null || !ele.IsValid)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    Logger.Log("Exception in ElementsAreValid: {0}", ex);                    
                }
                return false;
            }
        }

        /// <summary>
        /// Checks if Kadala Vendor window is currently open
        /// </summary>
        public static bool IsVendorWindowOpen 
        { 
            get 
            {
                try 
                { 
                    var window = UIElement.FromHash(0xA83F2BC15AC524D7); 
                    return window !=null && window.IsValid && window.IsVisible;
                }
                catch(Exception ex) 
                { 
                    Logger.Log("Exception finding UIElement Vendor Window {0}", ex);
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if local reference to Kadala DiaUnit exists and is valid
        /// </summary>
        public static bool IsKadalaValid
        {
            get
            {
                return _kadala != null && _kadala.IsValid;
            }
        }

        /// <summary>
        /// Returns true if our bags have been emptied
        /// </summary>
        public bool IsDoneVendoring
        {
            get
            {
                // There should be nothing but potions in the backpack after town run is complete. 
                var nonPotions = ZetaDia.Me.Inventory.Backpack.Where(i => i.ItemType != ItemType.Potion);
                var nonPotionCount = 0;

                if(nonPotions.Any())
                {
                    nonPotionCount = nonPotions.Count();

                    // Get a mapping of protected slots
                    bool[,] backpackSlotBlocked = new bool[10, 6];
                    foreach (InventorySquare square in CharacterSettings.Instance.ProtectedBagSlots)
                    {
                        backpackSlotBlocked[square.Column, square.Row] = true;
                    }

                    foreach (var item in nonPotions)
                    {
                        int row = item.InventoryRow;
                        int col = item.InventoryColumn;

                        // If non-potion is in protected slot, ignore it.
                        if (backpackSlotBlocked[col, row])
                        {
                            nonPotionCount--;
                        }
                    }

                }
                
                //Logger.Debug("IsDoneVendoring={0} nonPotions={1} nonPotionCount={2}", (nonPotionCount<=0), nonPotions.Any(), nonPotionCount);

                return (nonPotionCount<=0);
            }
        }

        #endregion

        #region Utilities

        private static AnimationState lastAnimationState = AnimationState.Invalid;
        internal static AnimationState CurrentAnimationState
        {
            get
            {
                try
                {
                    using (ZetaDia.Memory.AcquireFrame())
                    {
                        lastAnimationState = ZetaDia.Me.CommonData.AnimationState;
                    }
                }
                catch
                {

                }
                return lastAnimationState;
            }
        }

        public static int BackPackItemCount
        {
            get
            {
                return ZetaDia.Me.Inventory.Backpack.Count(i => i.IsValid);
            }
        }

        /// <summary>
        /// Extracts the DB Town Run behavior from VendorRun hook.
        /// </summary>
        internal PrioritySelector VendorRunPrioritySelector
        {
            get
            {
                Decorator VendorRunDecorator = TreeHooks.Instance.Hooks["VendorRun"].First() as Decorator;

                if (VendorRunDecorator != null)
                {
                    if (VendorRunDecorator.DecoratedChild is Sequence)
                    {
                        //Trinity Modified TownRun
                        Sequence VendorRunSequence = VendorRunDecorator.DecoratedChild as Sequence;
                        return VendorRunSequence.Children.First() as PrioritySelector;

                    }
                    else if (VendorRunDecorator.DecoratedChild is PrioritySelector)
                    {
                        // Original Unbesmirched TownRun
                        return VendorRunDecorator.DecoratedChild as PrioritySelector;
                    }
                }
                return new PrioritySelector();
            }
        }

        private static int _lastBackPackCount;
        private static int _lastProtectedSlotsCount;
        private static Vector2 _lastBackPackLocation = new Vector2(-2, -2);

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        /// <param name="isOriginalTwoSlot"></param>
        /// <returns></returns>
        internal static Vector2 FindValidBackpackLocation(bool isOriginalTwoSlot, int minBagSlotsAvailable)
        {
                try
                {
                    if (_lastBackPackLocation != new Vector2(-2, -2) &&
                        _lastBackPackCount == ZetaDia.Me.Inventory.Backpack.Count(i => i.IsValid) &&
                        _lastProtectedSlotsCount == CharacterSettings.Instance.ProtectedBagSlots.Count)
                    {
                        return _lastBackPackLocation;
                    }

                    bool[,] BackpackSlotBlocked = new bool[10, 6];

                    int freeBagSlots = 60;

                    _lastProtectedSlotsCount = CharacterSettings.Instance.ProtectedBagSlots.Count;
                    _lastBackPackCount = ZetaDia.Me.Inventory.Backpack.Count(i => i.IsValid);

                    // Block off the entire of any "protected bag slots"
                    foreach (InventorySquare square in CharacterSettings.Instance.ProtectedBagSlots)
                    {
                        BackpackSlotBlocked[square.Column, square.Row] = true;
                        freeBagSlots--;
                    }

                    // Map out all the items already in the backpack
                    foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                    {
                        if (!item.IsValid)
                            continue;

                        int row = item.InventoryRow;
                        int col = item.InventoryColumn;

                        // Slot is already protected, don't double count
                        if (!BackpackSlotBlocked[col, row])
                        {
                            BackpackSlotBlocked[col, row] = true;
                            freeBagSlots--;
                        }

                        if (!item.IsTwoSquareItem)
                            continue;

                        // Slot is already protected, don't double count
                        if (BackpackSlotBlocked[col, row + 1])
                            continue;

                        freeBagSlots--;
                        BackpackSlotBlocked[col, row + 1] = true;
                    }

                    bool noFreeSlots = freeBagSlots < 1;
                    int unprotectedSlots = 60 - _lastProtectedSlotsCount;

                    // Use count of Unprotected slots if FreeBagSlots is higher than unprotected slots
                    int minFreeSlots = Math.Min(minBagSlotsAvailable, unprotectedSlots);

                    // free bag slots is less than required
                    if (noFreeSlots || freeBagSlots < minFreeSlots)
                    {
                        _lastBackPackLocation = new Vector2(-1, -1);
                        return _lastBackPackLocation;
                    }
                    // 10 columns
                    for (int col = 0; col <= 9; col++)
                    {
                        // 6 rows
                        for (int row = 0; row <= 5; row++)
                        {
                            // Slot is blocked, skip
                            if (BackpackSlotBlocked[col, row])
                                continue;

                            // Not a two slotitem, slot not blocked, use it!
                            if (!isOriginalTwoSlot)
                            {
                                _lastBackPackLocation = new Vector2(col, row);
                                return _lastBackPackLocation;
                            }

                            // Is a Two Slot, Can't check for 2 slot items on last row
                            if (row == 5)
                                continue;

                            // Is a Two Slot, check row below
                            if (BackpackSlotBlocked[col, row + 1])
                                continue;

                            _lastBackPackLocation = new Vector2(col, row);
                            return _lastBackPackLocation;
                        }
                    }

                    // no free slot
                    _lastBackPackLocation = new Vector2(-1, -1);
                    return _lastBackPackLocation;
                }
                catch (Exception ex)
                {
                    Logger.Log("Error in finding backpack slot {0}", ex.ToString());
                    return new Vector2(1, 1);
                }
        }

        #endregion

        #region Reference

        public static Dictionary<string, BloodShardGambleItems> InternalNameAndEnumType = new Dictionary<string, BloodShardGambleItems>()
        {
            { "PH_1HWeapon", BloodShardGambleItems.OneHandItem },
            { "PH_2HWeapon", BloodShardGambleItems.TwoHandItem },
            { "PH_Quiver", BloodShardGambleItems.Quiver },
            { "PH_Orb", BloodShardGambleItems.Orb },
            { "PH_Mojo", BloodShardGambleItems.Mojo },
            { "PH_Helm", BloodShardGambleItems.Helm },
            { "PH_Gloves", BloodShardGambleItems.Gloves },
            { "PH_Boots", BloodShardGambleItems.Boots },
            { "PH_ChestArmor", BloodShardGambleItems.Chest },
            { "PH_Belt", BloodShardGambleItems.Belt },
            { "PH_Shoulders", BloodShardGambleItems.Shoulders },
            { "PH_Pants", BloodShardGambleItems.Pants },
            { "PH_Bracers", BloodShardGambleItems.Bracers },
            { "PH_Shield", BloodShardGambleItems.Shield },
            { "PH_Ring", BloodShardGambleItems.Ring },
            { "PH_Amulet", BloodShardGambleItems.Amulet }
        };

        public static Dictionary<Destination, int> DestinationAndActorId = new Dictionary<Destination, int>()
        {
            { Destination.Kadala,361241 },
            { Destination.Orek,363744 },
            { Destination.Waypoint,6442 },
            { Destination.Enchantress,4062 },
            { Destination.NephalemObelisk,364715 }            
        };


        #endregion

    }

    /// <summary>
    /// Basic Logger Class
    /// Credit to rrrix, who i copied this from. 
    /// </summary>
    public static class Logger
    {
        private static readonly log4net.ILog Logging = Zeta.Common.Logger.GetLoggerInstanceForType();

        public static void Log(string message, params object[] args)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            Logging.InfoFormat(string.Format("[{0} {1}] ", KadalaSpree.Plugin.NAME, KadalaSpree.Plugin.VERSION) + string.Format(message, args), type.Name);
        }

        public static void Log(string message)
        {
            Log(message, string.Empty);
        }

        public static void Debug(string message, params object[] args)
        {
            if (KadalaSpreeSettings.Instance.Debug)
            {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                Logging.InfoFormat(string.Format("[{0} {1}] ", KadalaSpree.Plugin.NAME, KadalaSpree.Plugin.VERSION) + string.Format(message, args), type.Name);
            }
        }

        public static void Debug(string message)
        {
            if (KadalaSpreeSettings.Instance.Debug)
            {
                Log(message, string.Empty);
            }
        }

    }


}
