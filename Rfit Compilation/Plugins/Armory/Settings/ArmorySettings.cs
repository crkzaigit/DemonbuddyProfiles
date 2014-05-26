using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows.Input;
using Zeta.Common.Xml;
using Zeta.Game;
using Zeta.XmlEngine;

namespace Armory.Settings
{
    [XmlElement("AmorySettings")]
    public class ArmorySettings : XmlSettings
    {
        #region Singleton
        private static ArmorySettings _instance;

        public ArmorySettings()
            : base(Path.Combine(Path.Combine(SettingsDirectory, BattleTagName), "ArmorySettings.xml"))
        {

        }

        public static ArmorySettings Instance
        {
            get { return _instance ?? (_instance = new ArmorySettings()); }
        }
        #endregion

        #region fields
        private bool _EnableEquipper;
        private bool _DisableAt70;
        private bool _IdentifyItems;
        private bool _CheckStash;

        private bool _UseBasicUpgradeFormula;
        private float _BasicUpgradeFormulaDamage;
        private float _BasicUpgradeFormulaToughness;

        private bool _UseAdditiveDamageFormula;
        private float _AdditiveDamageFormulaDamage;
        private float _AdditiveDamageFormulaDamageToughness;

        private bool _UseHealingUpgradeFormula;
        private float _HealingUpgradeFormulaDamage;
        private float _HealingUpgradeFormulaDamageToughness;
        private float _HealingUpgradeFormulaHealing;

        private bool _allowTwoHand;
        private bool _allowShields;
        private bool _dualWield;

        private bool _EnableMysteryVendor;

        private ICommand _resetCommand;
        #endregion

        private static string _battleTagName;
        public static string BattleTagName
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_battleTagName) && ZetaDia.Service.Hero.IsValid)
                        _battleTagName = ZetaDia.Service.Hero.BattleTagName;
                    else if (string.IsNullOrWhiteSpace(_battleTagName) && !ZetaDia.Service.Hero.IsValid)
                        _battleTagName = "";
                    return _battleTagName;
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Default: false
        /// </summary>
        [XmlElement("EnableEquipper")]
        [Setting]
        [DefaultValue(true)]
        public bool EnableEquipper
        {
            get { return _EnableEquipper; }
            set
            {
                if (_EnableEquipper == value)
                    return;
                _EnableEquipper = value;
                OnPropertyChanged("EnableEquipper");
            }
        }

        [XmlElement("DisableAt70")]
        [Setting]
        [DefaultValue(false)]
        public bool DisableAt70
        {
            get { return _DisableAt70; }
            set
            {
                if (_DisableAt70 != value)
                {
                    _DisableAt70 = value;
                    OnPropertyChanged("DisableAt70");
                }
            }
        }

        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("IdentifyItems")]
        [Setting]
        [DefaultValue(true)]
        public bool IdentifyItems
        {
            get { return _IdentifyItems; }
            set
            {
                if (_IdentifyItems != value)
                {
                    _IdentifyItems = value;
                    OnPropertyChanged("IdentifyItems");
                }
            }
        }


        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("CheckStash")]
        [Setting]
        [DefaultValue(true)]
        public bool CheckStash
        {
            get { return _CheckStash; }
            set
            {
                if (_CheckStash != value)
                {
                    _CheckStash = value;
                    OnPropertyChanged("CheckStash");
                }
            }
        }

        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("UseBasicUpgradeFormula")]
        [Setting]
        [DefaultValue(true)]
        public bool UseBasicUpgradeFormula
        {
            get { return _UseBasicUpgradeFormula; }
            set
            {
                if (_UseBasicUpgradeFormula != value)
                {
                    _UseBasicUpgradeFormula = value;
                    OnPropertyChanged("UseBasicUpgradeFormula");
                }
            }
        }

        /// <summary>
        /// Default: 0
        /// </summary>
        [XmlElement("BasicUpgradeFormulaDamage")]
        [Setting]
        [DefaultValue(0)]
        public float BasicUpgradeFormulaDamage
        {
            get { return _BasicUpgradeFormulaDamage; }
            set
            {
                if (_BasicUpgradeFormulaDamage != value)
                {
                    _BasicUpgradeFormulaDamage = value;
                    OnPropertyChanged("BasicUpgradeFormulaDamage");
                }
            }
        }

        /// <summary>
        /// Default: 0
        /// </summary>
        [XmlElement("BasicUpgradeFormulaToughness")]
        [Setting]
        [DefaultValue(0)]
        public float BasicUpgradeFormulaToughness
        {
            get { return _BasicUpgradeFormulaToughness; }
            set
            {
                if (_BasicUpgradeFormulaToughness != value)
                {
                    _BasicUpgradeFormulaToughness = value;
                    OnPropertyChanged("BasicUpgradeFormulaToughness");
                }
            }
        }


        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("UseAdditiveDamageFormula")]
        [Setting]
        [DefaultValue(true)]
        public bool UseAdditiveDamageFormula
        {
            get { return _UseAdditiveDamageFormula; }
            set
            {
                if (_UseAdditiveDamageFormula != value)
                {
                    _UseAdditiveDamageFormula = value;
                    OnPropertyChanged("UseAdditiveDamageFormula");
                }
            }
        }

        /// <summary>
        /// Default: -0.05f
        /// </summary>
        [XmlElement("AdditiveDamageFormulaDamage")]
        [Setting]
        [DefaultValue(-0.05f)]
        public float AdditiveDamageFormulaDamage
        {
            get { return _AdditiveDamageFormulaDamage; }
            set
            {
                if (_AdditiveDamageFormulaDamage != value)
                {
                    _AdditiveDamageFormulaDamage = value;
                    OnPropertyChanged("AdditiveDamageFormulaDamage");
                }
            }
        }

        /// <summary>
        /// Default: 0
        /// </summary>
        [XmlElement("AdditiveDamageFormulaDamageToughness")]
        [Setting]
        [DefaultValue(0)]
        public float AdditiveDamageFormulaDamageToughness
        {
            get { return _AdditiveDamageFormulaDamageToughness; }
            set
            {
                if (_AdditiveDamageFormulaDamageToughness != value)
                {
                    _AdditiveDamageFormulaDamageToughness = value;
                    OnPropertyChanged("AdditiveDamageFormulaDamageToughness");
                }
            }
        }

        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("UseHealingUpgradeFormula")]
        [Setting]
        [DefaultValue(true)]
        public bool UseHealingUpgradeFormula
        {
            get { return _UseHealingUpgradeFormula; }
            set
            {
                if (_UseHealingUpgradeFormula != value)
                {
                    _UseHealingUpgradeFormula = value;
                    OnPropertyChanged("UseHealingUpgradeFormula");
                }
            }
        }


        /// <summary>
        /// Default: -0.15f
        /// </summary>
        [XmlElement("HealingUpgradeFormulaDamage")]
        [Setting]
        [DefaultValue(-0.15f)]
        public float HealingUpgradeFormulaDamage
        {
            get { return _HealingUpgradeFormulaDamage; }
            set
            {
                if (_HealingUpgradeFormulaDamage != value)
                {
                    _HealingUpgradeFormulaDamage = value;
                    OnPropertyChanged("HealingUpgradeFormulaDamage");
                }
            }
        }

        /// <summary>
        /// Default: 0
        /// </summary>
        [XmlElement("HealingUpgradeFormulaDamageToughness")]
        [Setting]
        [DefaultValue(0)]
        public float HealingUpgradeFormulaDamageToughness
        {
            get { return _HealingUpgradeFormulaDamageToughness; }
            set
            {
                if (_HealingUpgradeFormulaDamageToughness != value)
                {
                    _HealingUpgradeFormulaDamageToughness = value;
                    OnPropertyChanged("HealingUpgradeFormulaDamageToughness");
                }
            }
        }

        /// <summary>
        /// Default: 0
        /// </summary>
        [XmlElement("HealingUpgradeFormulaHealing")]
        [Setting]
        [DefaultValue(0)]
        public float HealingUpgradeFormulaHealing
        {
            get { return _HealingUpgradeFormulaHealing; }
            set
            {
                if (_HealingUpgradeFormulaHealing != value)
                {
                    _HealingUpgradeFormulaHealing = value;
                    OnPropertyChanged("HealingUpgradeFormulaHealing");
                }
            }
        }

        /// <summary>
        /// Default: 0
        /// </summary>
        [XmlElement("EnableMysteryVendor")]
        [Setting]
        [DefaultValue(true)]
        public bool EnableMysteryVendor
        {
            get { return _EnableMysteryVendor; }
            set
            {
                if (_EnableMysteryVendor != value)
                {
                    _EnableMysteryVendor = value;
                    OnPropertyChanged("EnableMysteryVendor");
                }
            }
        }

        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("AllowTwoHand")]
        [Setting]
        [DefaultValue(false)]
        public bool AllowTwoHand
        {
            get { return _allowTwoHand; }
            set
            {
                if (_allowTwoHand != value)
                {
                    _allowTwoHand = value;
                    OnPropertyChanged("AllowTwoHand");
                }
            }
        }


        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("AllowShields")]
        [Setting]
        [DefaultValue(true)]
        public bool AllowShields
        {
            get { return _allowShields; }
            set
            {
                if (_allowShields != value)
                {
                    _allowShields = value;
                    OnPropertyChanged("AllowShields");
                }
            }
        }


        /// <summary>
        /// Default: true
        /// </summary>
        [XmlElement("DualWield")]
        [Setting]
        [DefaultValue(true)]
        public bool DualWield
        {
            get { return _dualWield; }
            set
            {
                if (_dualWield != value)
                {
                    _dualWield = value;
                    OnPropertyChanged("DualWield");
                }
            }
        }


        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                {
                    _resetCommand = new RelayCommand(parameter => Instance.SetDefaults());
                }

                return _resetCommand;
            }
            private set
            {
                _resetCommand = value;
            }
        }

        public ProtectedSlots ProtectedSlots
        {
            get
            {
                return ProtectedSlots.Instance;
            }
            set
            {
                ProtectedSlots.Instance = value;
            }
        }

        public MysteryItemSlots MysteryItemSlots
        {
            get
            {
                return MysteryItemSlots.Instance;
            }
            set
            {
                MysteryItemSlots.Instance = value;
            }
        }
    }
}
