using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using Zeta.Common.Xml;
using Zeta.Game;
using Zeta.XmlEngine;

namespace KadalaSpree
{
    [XmlElement("KadalaSpreeSettings")]
    class KadalaSpreeSettings : XmlSettings
    {
        private static KadalaSpreeSettings _instance;

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

        public KadalaSpreeSettings() :
            base(Path.Combine(SettingsDirectory, "KadalaSpree", BattleTagName, "KadalaSpree.xml"))
        {
        }

        public static KadalaSpreeSettings Instance
        {
            get { return _instance ?? (_instance = new KadalaSpreeSettings()); }
        }

        [XmlElement("OneHandItem")]
        [DefaultValue(true)]
        [Setting]
        public bool OneHandItem
        {
            get
            {
                return _oneHandItem;
            }
            set
            {
                _oneHandItem = value;
                OnPropertyChanged("OneHandItem");
            }
        }
        private bool _oneHandItem;

        [XmlElement("TwoHandItem")]
        [DefaultValue(true)]
        [Setting]
        public bool TwoHandItem
        {
            get
            {
                return _twoHandItem;
            }
            set
            {
                _twoHandItem = value;
                OnPropertyChanged("TwoHandItem");
            }
        }
        private bool _twoHandItem;

        [XmlElement("Mojo")]
        [DefaultValue(true)]
        [Setting]
        public bool Mojo
        {
            get
            {
                return _mojo;
            }
            set
            {
                _mojo = value;
                OnPropertyChanged("Mojo");
            }
        }
        private bool _mojo;

        [XmlElement("Quiver")]
        [DefaultValue(true)]
        [Setting]
        public bool Quiver
        {
            get
            {
                return _quiver;
            }
            set
            {
                _quiver = value;
                OnPropertyChanged("Quiver");
            }
        }
        private bool _quiver;

        [XmlElement("Orb")]
        [DefaultValue(true)]
        [Setting]
        public bool Orb
        {
            get
            {
                return _orb;
            }
            set
            {
                _orb = value;
                OnPropertyChanged("Orb");
            }
        }
        private bool _orb;

        [XmlElement("Helm")]
        [DefaultValue(true)]
        [Setting]
        public bool Helm
        {
            get
            {
                return _helm;
            }
            set
            {
                _helm = value;
                OnPropertyChanged("Helm");
            }
        }
        private bool _helm;

        [XmlElement("Gloves")]
        [DefaultValue(true)]
        [Setting]
        public bool Gloves
        {
            get
            {
                return _gloves;
            }
            set
            {
                _gloves = value;
                OnPropertyChanged("Gloves");
            }
        }
        private bool _gloves;

        [XmlElement("Boots")]
        [DefaultValue(true)]
        [Setting]
        public bool Boots
        {
            get
            {
                return _boots;
            }
            set
            {
                _boots = value;
                OnPropertyChanged("Boots");
            }
        }
        private bool _boots;

        [XmlElement("Chest")]
        [DefaultValue(true)]
        [Setting]
        public bool Chest
        {
            get
            {
                return _chest;
            }
            set
            {
                _chest = value;
                OnPropertyChanged("Chest");
            }
        }
        private bool _chest;

        [XmlElement("Belt")]
        [DefaultValue(true)]
        [Setting]
        public bool Belt
        {
            get
            {
                return _belt;
            }
            set
            {
                _belt = value;
                OnPropertyChanged("Belt");
            }
        }
        private bool _belt;

        [XmlElement("Shoulders")]
        [DefaultValue(true)]
        [Setting]
        public bool Shoulders
        {
            get
            {
                return _shoulders;
            }
            set
            {
                _shoulders = value;
                OnPropertyChanged("Shoulders");
            }
        }
        private bool _shoulders;

        [XmlElement("Pants")]
        [DefaultValue(true)]
        [Setting]
        public bool Pants
        {
            get
            {
                return _pants;
            }
            set
            {
                _pants = value;
                OnPropertyChanged("Pants");
            }
        }
        private bool _pants;

        [XmlElement("Bracers")]
        [DefaultValue(true)]
        [Setting]
        public bool Bracers
        {
            get
            {
                return _bracers;
            }
            set
            {
                _bracers = value;
                OnPropertyChanged("Bracers");
            }
        }
        private bool _bracers;

        [XmlElement("Shield")]
        [DefaultValue(true)]
        [Setting]
        public bool Shield
        {
            get
            {
                return _shield;
            }
            set
            {
                _shield = value;
                OnPropertyChanged("Shield");
            }
        }
        private bool _shield;

        [XmlElement("Ring")]
        [DefaultValue(true)]
        [Setting]
        public bool Ring
        {
            get
            {
                return _ring;
            }
            set
            {
                _ring = value;
                OnPropertyChanged("Ring");
            }
        }
        private bool _ring;

        [XmlElement("Amulet")]
        [DefaultValue(true)]
        [Setting]
        public bool Amulet
        {
            get
            {
                return _amulet;
            }
            set
            {
                _amulet = value;
                OnPropertyChanged("Amulet");
            }
        }
        private bool _amulet;

        [XmlElement("Debug")]
        [DefaultValue(false)]
        [Setting]
        public bool Debug
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
                OnPropertyChanged("Debug");
            }
        }
        private bool _debug;

        [XmlElement("SaveUntilThreshold")]
        [DefaultValue(false)]
        [Setting]
        public bool SaveUntilThreshold
        {
            get
            {
                return _saveUntilThreshold;
            }
            set
            {
                _saveUntilThreshold = value;
                OnPropertyChanged("SaveUntilThreshold");
            }
        }
        private bool _saveUntilThreshold;

        [XmlElement("MinimumBloodShards")]
        [DefaultValue(25)]
        [Setting]
        public int MinimumBloodShards
        {
            get
            {
                return _minimumBloodShards;
            }
            set
            {
                if (_minimumBloodShards != value)
                {
                    _minimumBloodShards = value;
                    OnPropertyChanged("MinimumBloodShards");
                }
            }
        }
        private int _minimumBloodShards;

        [XmlElement("ForceSpreeThreshold")]
        [DefaultValue(400)]
        [Setting]
        public int ForceSpreeThreshold
        {
            get
            {
                return _forceSpreeThreshold;
            }
            set
            {
                if (_forceSpreeThreshold != value)
                {
                    _forceSpreeThreshold = value;
                    OnPropertyChanged("ForceSpreeThreshold");
                }
            }
        }
        private int _forceSpreeThreshold;


        [XmlElement("SelectedGambleSlots")]
        public List<BloodShardGambleItems> SelectedGambleSlots
        {
            get 
            {
                var list = new List<BloodShardGambleItems>();

                if(TwoHandItem) list.Add(BloodShardGambleItems.TwoHandItem);
                if(OneHandItem) list.Add(BloodShardGambleItems.OneHandItem);
                if(Mojo) list.Add(BloodShardGambleItems.Mojo);
                if(Quiver) list.Add(BloodShardGambleItems.Quiver);
                if(Orb) list.Add(BloodShardGambleItems.Orb);
                if(Helm) list.Add(BloodShardGambleItems.Helm);
                if(Gloves) list.Add(BloodShardGambleItems.Gloves);
                if(Boots) list.Add(BloodShardGambleItems.Boots);
                if(Chest) list.Add(BloodShardGambleItems.Chest);
                if(Belt) list.Add(BloodShardGambleItems.Belt);
                if(Shoulders) list.Add(BloodShardGambleItems.Shoulders);
                if(Pants) list.Add(BloodShardGambleItems.Pants);
                if(Bracers) list.Add(BloodShardGambleItems.Bracers);
                if(Shield) list.Add(BloodShardGambleItems.Shield);
                if(Ring) list.Add(BloodShardGambleItems.Ring);
                if(Amulet) list.Add(BloodShardGambleItems.Amulet);

                return list;
            }
        }



    }
}
