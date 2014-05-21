using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System.Configuration;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Common.Xml;
using Zeta.XmlEngine;

namespace GearSwap
{

    [XmlElement("GearSwapSettings")]
    class GearSwapSettings : XmlSettings
    {
        private static GearSwapSettings _instance;
        private bool loggingEnabled;
        private int lowHealthPerc;
        private int magicFindPerc;
        private int shiMizuPerc;
        private double barricadeDistance;
        private double lowHealthDuration;
        private double pulseInterval;
        private bool coldEnabled;
        private bool fireEnabled;
        private bool arcaneEnabled;
        private bool poisonEnabled;
        private bool lightningEnabled;
        private bool eliteEnabled;
        private bool shrineEnabled;
        private bool wellEnabled;
        private bool lowHealthEnabled;
        private bool criticalHealthEnabled;
        private bool threeEnemiesEnabled;
        private bool fiveEnemiesEnabled;
        private bool magicFindEnabled;
        private bool barricadeEnabled;
        private bool immuneEnabled;
        private bool debugLoggingEnabled;
        private bool chestEnabled;
        private bool puzzleRingEnabled;
        private bool shiMizuEnabled;
       

        private static string _battleTagName;
        public static string BattleTagName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_battleTagName) && ZetaDia.Service.Hero.IsValid)
                    _battleTagName = ZetaDia.Service.Hero.BattleTagName;
                return _battleTagName;
            }
        }

        public GearSwapSettings() :
            base(Path.Combine(SettingsDirectory, "GearSwap", BattleTagName, "GearSwapSettings.xml"))
        {
            DumpActorSNO = new RelayCommand((parameter) => { Helpers.DumpActorSNO(); });
            IncreasePriority = new RelayCommand((parameter) => { ConfigHelpers.IncreasePriority(); });
            DecreasePriority = new RelayCommand((parameter) => { ConfigHelpers.DecreasePriority(); });
            ResetDefaults = new RelayCommand((parameter) => { ConfigHelpers.ResetDefaults(); });
        }

        public static GearSwapSettings Instance
        {
            get { return _instance ?? (_instance = new GearSwapSettings()); }
        }

        [XmlElement("LoggingEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool LoggingEnabled
        {
            get
            {
                return loggingEnabled;
            }
            set
            {
                loggingEnabled = value;
                OnPropertyChanged("LoggingEnabled");
            }
        }

        [XmlElement("DebugLoggingEnabled")]
        [DefaultValue(false)]
        [Setting]
        public bool DebugLoggingEnabled
        {
            get
            {
                return debugLoggingEnabled;
            }
            set
            {
                debugLoggingEnabled = value;
                OnPropertyChanged("DebugLoggingEnabled");
            }
        }

        [XmlElement("LowHealthPerc")]
        [DefaultValue(40)]
        [Setting]
        public int LowHealthPerc
        {
            get
            {
                return lowHealthPerc;
            }
            set
            {
                lowHealthPerc = value;
                OnPropertyChanged("LowHealthPerc");
            }
        }
        [XmlElement("MagicFindPerc")]
        [DefaultValue(15)]
        [Setting]
        public int MagicFindPerc
        {
            get
            {
                return magicFindPerc;
            }
            set
            {
                magicFindPerc = value;
                OnPropertyChanged("MagicFindPerc");
            }
        }
        [XmlElement("ShiMizuPerc")]
        [DefaultValue(25)]
        [Setting]
        public int ShiMizuPerc
        {
            get
            {
                return shiMizuPerc;
            }
            set
            {
                shiMizuPerc = value;
                OnPropertyChanged("ShiMizuPerc");
            }
        }
        [XmlElement("ColdEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ColdEnabled
        {
            get
            {
                return coldEnabled;
            }
            set
            {
                coldEnabled = value;
                OnPropertyChanged("ColdEnabled");
            }
        }
        [XmlElement("FireEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool FireEnabled
        {
            get
            {
                return fireEnabled;
            }
            set
            {
                fireEnabled = value;
                OnPropertyChanged("FireEnabled");
            }
        }
        [XmlElement("ArcaneEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ArcaneEnabled
        {
            get
            {
                return arcaneEnabled;
            }
            set
            {
                arcaneEnabled = value;
                OnPropertyChanged("ArcaneEnabled");
            }
        }
        [XmlElement("PoisonEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool PoisonEnabled
        {
            get
            {
                return poisonEnabled;
            }
            set
            {
                poisonEnabled = value;
                OnPropertyChanged("PoisonEnabled");
            }
        }
        [XmlElement("LightningEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool LightningEnabled
        {
            get
            {
                return lightningEnabled;
            }
            set
            {
                lightningEnabled = value;
                OnPropertyChanged("LightningEnabled");
            }
        }
        [XmlElement("EliteEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool EliteEnabled
        {
            get
            {
                return eliteEnabled;
            }
            set
            {
                eliteEnabled = value;
                OnPropertyChanged("EliteEnabled");
            }
        }
        [XmlElement("ShrineEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ShrineEnabled
        {
            get
            {
                return shrineEnabled;
            }
            set
            {
                shrineEnabled = value;
                OnPropertyChanged("ShrineEnabled");
            }
        }
        [XmlElement("WellEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool WellEnabled
        {
            get
            {
                return wellEnabled;
            }
            set
            {
                wellEnabled = value;
                OnPropertyChanged("WellEnabled");
            }
        }
        [XmlElement("LowHealthEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool LowHealthEnabled
        {
            get
            {
                return lowHealthEnabled;
            }
            set
            {
                lowHealthEnabled = value;
                OnPropertyChanged("LowHealthEnabled");
            }
        }
        [XmlElement("CriticalHealthEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool CriticalHealthEnabled
        {
            get
            {
                return criticalHealthEnabled;
            }
            set
            {
                criticalHealthEnabled = value;
                OnPropertyChanged("CriticalHealthEnabled");
            }
        }
        [XmlElement("ThreeEnemiesEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ThreeEnemiesEnabled
        {
            get
            {
                return threeEnemiesEnabled;
            }
            set
            {
                threeEnemiesEnabled = value;
                OnPropertyChanged("ThreeEnemiesEnabled");
            }
        }
        [XmlElement("FiveEnemiesEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool FiveEnemiesEnabled
        {
            get
            {
                return fiveEnemiesEnabled;
            }
            set
            {
                fiveEnemiesEnabled = value;
                OnPropertyChanged("FiveEnemiesEnabled");
            }
        }
        [XmlElement("MagicFindEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool MagicFindEnabled
        {
            get
            {
                return magicFindEnabled;
            }
            set
            {
                magicFindEnabled = value;
                OnPropertyChanged("MagicFindEnabled");
            }
        }
        [XmlElement("BarricadeEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool BarricadeEnabled
        {
            get
            {
                return barricadeEnabled;
            }
            set
            {
                barricadeEnabled = value;
                OnPropertyChanged("BarricadeEnabled");
            }
        }

        [XmlElement("ImmuneEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ImmuneEnabled
        {
            get
            {
                return immuneEnabled;
            }
            set
            {
                immuneEnabled = value;
                OnPropertyChanged("ImmuneEnabled");
            }
        }

        [XmlElement("BarricadeDistance")]
        [DefaultValue(5)]
        [Setting]
        public double BarricadeDistance
        {
            get
            {
                return barricadeDistance;
            }
            set
            {
                barricadeDistance = value;
                OnPropertyChanged("BarricadeDistance");
            }
        }

        [XmlElement("ChestEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ChestEnabled
        {
            get
            {
                return chestEnabled;
            }
            set
            {
                chestEnabled = value;
                OnPropertyChanged("ChestEnabled");
            }
        }

        [XmlElement("LowHealthDuration")]
        [DefaultValue(4)]
        [Setting]
        public double LowHealthDuration
        {
            get
            {
                return lowHealthDuration;
            }
            set
            {
                lowHealthDuration = value;
                OnPropertyChanged("LowHealthDuration");
            }
        }

        [XmlElement("PuzzleRingEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool PuzzleRingEnabled
        {
            get
            {
                return puzzleRingEnabled;
            }
            set
            {
                puzzleRingEnabled = value;
                OnPropertyChanged("PuzzleRingEnabled");
            }
        }
        [XmlElement("ShiMizuEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool ShiMizuEnabled
        {
            get
            {
                return shiMizuEnabled;
            }
            set
            {
                shiMizuEnabled = value;
                OnPropertyChanged("ShiMizuEnabled");
            }
        }
        [XmlElement("PulseInterval")]
        [DefaultValue(.5)]
        [Setting]
        public double PulseInterval
        {
            get
            {
                return pulseInterval;
            }
            set
            {
                pulseInterval = value;
                OnPropertyChanged("PulseInterval");
            }
        }
        public static ICommand DumpActorSNO
        {
            get;
            private set;
        }

        public static ICommand IncreasePriority
        {
            get;
            private set;
        }

        public static ICommand DecreasePriority
        {
            get;
            private set;
        }
        public static ICommand ResetDefaults
        {
            get;
            private set;
        }
    }
}
