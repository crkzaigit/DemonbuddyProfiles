﻿using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Trinity.Config.Loot
{
    [DataContract(Namespace = "")]
    public class TownRunSetting : ITrinitySetting<TownRunSetting>, INotifyPropertyChanged, IExtensibleDataObject
    {
        #region Fields
        private int _WeaponScore;
        private int _ArmorScore;
        private int _JewelryScore;
        private SalvageOption _SalvageWhiteItemOption;
        private SalvageOption _SalvageBlueItemOption;
        private SalvageOption _SalvageYellowItemOption;
        private SalvageOption _SalvageLegendaryItemOption;
        private int _FreeBagSlots;
        private int _FreeBagSlotsInTown;
        private bool _OpenHoradricCaches;
        private bool _StashWhites;
        private bool _StashBlues;
        private bool _ForceSalvageRares;
        private bool _KeepLegendaryUnid;
        private bool _SellExtraPotions;
        #endregion Fields

        #region Events
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion Events

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TownRunSetting" /> class.
        /// </summary>
        public TownRunSetting()
        {
            Reset();
        }
        #endregion Constructors

        #region Properties
        [DataMember(IsRequired = false)]
        [DefaultValue(70000)]
        public int WeaponScore
        {
            get
            {
                return _WeaponScore;
            }
            set
            {
                if (_WeaponScore != value)
                {
                    _WeaponScore = value;
                    OnPropertyChanged("WeaponScore");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(16000)]
        public int ArmorScore
        {
            get
            {
                return _ArmorScore;
            }
            set
            {
                if (_ArmorScore != value)
                {
                    _ArmorScore = value;
                    OnPropertyChanged("ArmorScore");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(15000)]
        public int JewelryScore
        {
            get
            {
                return _JewelryScore;
            }
            set
            {
                if (_JewelryScore != value)
                {
                    _JewelryScore = value;
                    OnPropertyChanged("JewelryScore");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.Salvage)]
        public SalvageOption SalvageWhiteItemOption
        {
            get
            {
                return _SalvageWhiteItemOption;
            }
            set
            {
                if (_SalvageWhiteItemOption != value)
                {
                    _SalvageWhiteItemOption = value;
                    OnPropertyChanged("SalvageWhiteItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.Salvage)]
        public SalvageOption SalvageBlueItemOption
        {
            get
            {
                return _SalvageBlueItemOption;
            }
            set
            {
                if (_SalvageBlueItemOption != value)
                {
                    _SalvageBlueItemOption = value;
                    OnPropertyChanged("SalvageBlueItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.Salvage)]
        public SalvageOption SalvageYellowItemOption
        {
            get
            {
                return _SalvageYellowItemOption;
            }
            set
            {
                if (_SalvageYellowItemOption != value)
                {
                    _SalvageYellowItemOption = value;
                    OnPropertyChanged("SalvageYellowItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(SalvageOption.Salvage)]
        public SalvageOption SalvageLegendaryItemOption
        {
            get
            {
                return _SalvageLegendaryItemOption;
            }
            set
            {
                if (_SalvageLegendaryItemOption != value)
                {
                    _SalvageLegendaryItemOption = value;
                    OnPropertyChanged("SalvageLegendaryItemOption");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(1)]
        public int FreeBagSlots
        {
            get
            {
                return _FreeBagSlots;
            }
            set
            {
                if (_FreeBagSlots != value)
                {
                    _FreeBagSlots = value;
                    OnPropertyChanged("FreeBagSlots");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(30)]
        public int FreeBagSlotsInTown
        {
            get
            {
                return _FreeBagSlotsInTown;
            }
            set
            {
                if (_FreeBagSlotsInTown != value)
                {
                    _FreeBagSlotsInTown = value;
                    OnPropertyChanged("FreeBagSlotsInTown");
                }
            }
        }


        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool OpenHoradricCaches
        {
            get
            {
                return _OpenHoradricCaches;
            }
            set
            {
                if (_OpenHoradricCaches != value)
                {
                    _OpenHoradricCaches = value;
                    OnPropertyChanged("OpenHoradricCaches");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool StashWhites
        {
            get
            {
                return _StashWhites;
            }
            set
            {
                if (_StashWhites != value)
                {
                    _StashWhites = value;
                    OnPropertyChanged("StashWhites");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool StashBlues
        {
            get
            {
                return _StashBlues;
            }
            set
            {
                if (_StashBlues != value)
                {
                    _StashBlues = value;
                    OnPropertyChanged("StashBlues");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool ForceSalvageRares
        {
            get
            {
                return _ForceSalvageRares;
            }
            set
            {
                if (_ForceSalvageRares != value)
                {
                    _ForceSalvageRares = value;
                    OnPropertyChanged("ForceSalvageRares");
                }
            }
        }

        [DataMember(IsRequired = false)]
        [DefaultValue(false)]
        public bool KeepLegendaryUnid
        {
            get
            {
                return _KeepLegendaryUnid;
            }
            set
            {
                if (_KeepLegendaryUnid != value)
                {
                    _KeepLegendaryUnid = value;
                    OnPropertyChanged("KeepLegendaryUnid");
                }
            }
        }
        [DataMember(IsRequired = false)]
        [DefaultValue(true)]
        public bool SellExtraPotions
        {
            get
            {
                return _SellExtraPotions;
            }
            set
            {
                if (_SellExtraPotions != value)
                {
                    _SellExtraPotions = value;
                    OnPropertyChanged("SellExtraPotions");
                }
            }
        }
        #endregion Properties

        #region Methods
        public void Reset()
        {
            TrinitySetting.Reset(this);
        }

        public void CopyTo(TownRunSetting setting)
        {
            TrinitySetting.CopyTo(this, setting);
        }

        public TownRunSetting Clone()
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
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            FreeBagSlots = 1;
            FreeBagSlotsInTown = 6;
            OpenHoradricCaches = true;
            SellExtraPotions = true;
            SalvageWhiteItemOption = SalvageOption.Salvage;
            SalvageBlueItemOption = SalvageOption.Salvage;
            SalvageYellowItemOption = SalvageOption.Salvage;
            SalvageLegendaryItemOption = SalvageOption.Salvage;
        }

        /// <summary>
        /// This will run after deserialization, for settings migration
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // This is required for migrating from "None/InfernoOnly/All" to "Sell/Salvage" options

            switch (SalvageWhiteItemOption)
            {
                case SalvageOption.None:
                case SalvageOption.InfernoOnly:
                case SalvageOption.All:
                    SalvageWhiteItemOption = SalvageOption.Salvage;
                    break;
            }

            switch (SalvageBlueItemOption)
            {
                case SalvageOption.None:
                case SalvageOption.InfernoOnly:
                case SalvageOption.All:
                    SalvageBlueItemOption = SalvageOption.Salvage;
                    break;
            }

            switch (SalvageYellowItemOption)
            {
                case SalvageOption.None:
                case SalvageOption.InfernoOnly:
                case SalvageOption.All:
                    SalvageYellowItemOption = SalvageOption.Salvage;
                    break;
            }

            switch (SalvageLegendaryItemOption)
            {
                case SalvageOption.None:
                case SalvageOption.InfernoOnly:
                case SalvageOption.All:
                    SalvageLegendaryItemOption = SalvageOption.Salvage;
                    break;
            }

        }

        #endregion Methods

        public ExtensionDataObject ExtensionData
        {
            get
            {
                return null;
            }
            set
            {
                //_ExtendedData = value;
            }
        }
    }
}
