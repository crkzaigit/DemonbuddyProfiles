﻿using System.ComponentModel;
using System.Runtime.Serialization;
using Trinity.Technicals;

namespace Trinity.Config
{
    [DataContract(Namespace = "")]
    public class AdvancedSetting : ITrinitySetting<AdvancedSetting>, INotifyPropertyChanged
    {
        #region Fields
        private bool _LazyRaiderClickToPause;
        private bool _UnstuckerEnabled;
        private bool _AllowRestartGame;
        private bool _TPSEnabled;
        private int _TPSLimit;
        private int _CacheRefreshRate;
        private bool _LogStuckLocation;
        private bool _DebugInStatusBar;
        private LogCategory _LogCategories;
        private bool _GoldInactivityEnabled;
        private int _GoldInactivityTimer;
        private bool _LogDroppedItems;
        private bool _OutputReports;
        private bool _ItemRulesLogs;
        private bool _ShowBattleTag;
        private bool _DisableAllMovement;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedSetting" /> class.
        /// </summary>
        public AdvancedSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(LogCategory.UserInformation)]
        public LogCategory LogCategories
        {
            get
            {
                return _LogCategories;
            }
            set
            {
                if (_LogCategories != value)
                {
                    _LogCategories = value;
                    OnPropertyChanged("LogCategories");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool LazyRaiderClickToPause
        {
            get
            {
                return _LazyRaiderClickToPause;
            }
            set
            {
                if (_LazyRaiderClickToPause != value)
                {
                    _LazyRaiderClickToPause = value;
                    OnPropertyChanged("LazyRaiderClickToPause");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool UnstuckerEnabled
        {
            get
            {
                return _UnstuckerEnabled;
            }
            set
            {
                if (_UnstuckerEnabled != value)
                {
                    _UnstuckerEnabled = value;
                    OnPropertyChanged("UnstuckerEnabled");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool AllowRestartGame
        {
            get
            {
                return _AllowRestartGame;
            }
            set
            {
                if (_AllowRestartGame != value)
                {
                    _AllowRestartGame = value;
                    OnPropertyChanged("AllowRestartGame");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool GoldInactivityEnabled
        {
            get
            {
                return _GoldInactivityEnabled;
            }
            set
            {
                if (_GoldInactivityEnabled != value)
                {
                    _GoldInactivityEnabled = value;
                    OnPropertyChanged("GoldInactivityEnabled");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(600)]
        public int GoldInactivityTimer
        {
            get
            {
                return _GoldInactivityTimer;
            }
            set
            {
                if (_GoldInactivityTimer != value)
                {
                    _GoldInactivityTimer = value;
                    OnPropertyChanged("GoldInactivityTimer");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool TPSEnabled
        {
            get
            {
                return _TPSEnabled;
            }
            set
            {
                if (_TPSEnabled != value)
                {
                    _TPSEnabled = value;
                    OnPropertyChanged("TPSEnabled");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(8)]
        public int TPSLimit
        {
            get
            {
                return _TPSLimit;
            }
            set
            {
                if (_TPSLimit != value)
                {
                    _TPSLimit = value;
                    OnPropertyChanged("TPSLimit");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(300)]
        public int CacheRefreshRate
        {
            get
            {
                return _CacheRefreshRate;
            }
            set
            {
                if (_CacheRefreshRate != value)
                {
                    _CacheRefreshRate = value;
                    OnPropertyChanged("CacheRefreshRate");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool DebugInStatusBar
        {
            get
            {
                return _DebugInStatusBar;
            }
            set
            {
                if (_DebugInStatusBar != value)
                {
                    _DebugInStatusBar = value;
                    OnPropertyChanged("DebugInStatusBar");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool LogStuckLocation
        {
            get
            {
                return _LogStuckLocation;
            }
            set
            {
                if (_LogStuckLocation != value)
                {
                    _LogStuckLocation = value;
                    OnPropertyChanged("LogStuckLocation");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool LogDroppedItems
        {
            get
            {
                return _LogDroppedItems;
            }
            set
            {
                if (_LogDroppedItems != value)
                {
                    _LogDroppedItems = value;
                    OnPropertyChanged("LogDroppedItems");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool OutputReports
        {
            get
            {
                return _OutputReports;
            }
            set
            {
                if (_OutputReports != value)
                {
                    _OutputReports = value;
                    OnPropertyChanged("OutputReports");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool ItemRulesLogs
        {
            get
            {
                return _ItemRulesLogs;
            }
            set
            {
                if (_ItemRulesLogs != value)
                {
                    _ItemRulesLogs = value;
                    OnPropertyChanged("ItemRulesLogs");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool ShowBattleTag
        {
            get
            {
                return _ShowBattleTag;
            }
            set
            {
                if (_ShowBattleTag != value)
                {
                    _ShowBattleTag = value;
                    OnPropertyChanged("ShowBattleTag");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool DisableAllMovement
        {
            get
            {
                return _DisableAllMovement;
            }
            set
            {
                if (_DisableAllMovement != value)
                {
                    _DisableAllMovement = value;
                    OnPropertyChanged("DisableAllMovement");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(AdvancedSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public AdvancedSetting Clone()
        {
            return TrinitySetting.Clone(this);
        }

        /// <summary>
        /// Called when property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// This will set default values for new settings if they were not present in the serialized XML (otherwise they will be the type defaults)
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing()]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            this._CacheRefreshRate = 100;
            this._OutputReports = true;
            this._ItemRulesLogs = true;
            this._LogDroppedItems = true;
        }
        #endregion Methods
    }
}
