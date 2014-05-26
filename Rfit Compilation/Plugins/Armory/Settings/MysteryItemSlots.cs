using System.ComponentModel;
using System.Configuration;
using System.IO;
using Zeta.Common.Xml;
using Zeta.XmlEngine;

namespace Armory.Settings
{
    [XmlElement("MysteryItemSlots")]
    public class MysteryItemSlots : XmlSettings
    {
        #region Singleton
        private static MysteryItemSlots _instance;

        public MysteryItemSlots() : base(Path.Combine(Path.Combine(SettingsDirectory, ArmorySettings.BattleTagName), "MysteryItemSlots.xml")) { }

        public static MysteryItemSlots Instance
        {
            get { return _instance ?? (_instance = new MysteryItemSlots()); }
            set { _instance = value; }
        }
        #endregion

        #region Fields
        private bool shoulders;
        private bool helm;
        private bool neck;
        private bool hands;
        private bool chest;
        private bool wrists;
        private bool _Ring;
        private bool waist;
        private bool ring2;
        private bool legs;
        private bool feet;
        private bool offHand;
        private bool oneHand;
        private bool twoHand;
        private bool randomOrder;
        private int minShardCount;
        #endregion

        [XmlElement("Shoulders")]
        [Setting]
        [DefaultValue(true)]
        public bool Shoulders
        {
            get { return shoulders; }
            set
            {
                if (shoulders != value)
                {
                    shoulders = value;
                    OnPropertyChanged("Shoulders");
                }
            }
        }

        [XmlElement("Helm")]
        [Setting]
        [DefaultValue(true)]
        public bool Helm
        {
            get { return helm; }
            set
            {
                if (helm != value)
                {
                    helm = value;
                    OnPropertyChanged("Helm");
                }
            }
        }

        [XmlElement("Neck")]
        [Setting]
        [DefaultValue(true)]
        public bool Neck
        {
            get { return neck; }
            set
            {
                if (neck != value)
                {
                    neck = value;
                    OnPropertyChanged("Neck");
                }
            }
        }

        [XmlElement("Hands")]
        [Setting]
        [DefaultValue(true)]
        public bool Hands
        {
            get { return hands; }
            set
            {
                if (hands != value)
                {
                    hands = value;
                    OnPropertyChanged("Hands");
                }
            }
        }

        [XmlElement("Chest")]
        [Setting]
        [DefaultValue(true)]
        public bool Chest
        {
            get { return chest; }
            set
            {
                if (chest != value)
                {
                    chest = value;
                    OnPropertyChanged("Chest");
                }
            }
        }

        [XmlElement("Wrists")]
        [Setting]
        [DefaultValue(true)]
        public bool Wrists
        {
            get { return wrists; }
            set
            {
                if (wrists != value)
                {
                    wrists = value;
                    OnPropertyChanged("Wrists");
                }
            }
        }

        [XmlElement("Ring")]
        [Setting]
        [DefaultValue(true)]
        public bool Ring
        {
            get { return _Ring; }
            set
            {
                if (_Ring != value)
                {
                    _Ring = value;
                    OnPropertyChanged("Ring");
                }
            }
        }

        [XmlElement("Waist")]
        [Setting]
        [DefaultValue(true)]
        public bool Waist
        {
            get { return waist; }
            set
            {
                if (waist != value)
                {
                    waist = value;
                    OnPropertyChanged("Waist");
                }
            }
        }

        [XmlElement("Legs")]
        [Setting]
        [DefaultValue(true)]
        public bool Legs
        {
            get { return legs; }
            set
            {
                if (legs != value)
                {
                    legs = value;
                    OnPropertyChanged("Legs");
                }
            }
        }

        [XmlElement("Feet")]
        [Setting]
        [DefaultValue(true)]
        public bool Feet
        {
            get { return feet; }
            set
            {
                if (feet != value)
                {
                    feet = value;
                    OnPropertyChanged("Feet");
                }
            }
        }

        [XmlElement("OffHand")]
        [Setting]
        [DefaultValue(true)]
        public bool OffHand
        {
            get { return offHand; }
            set
            {
                if (offHand != value)
                {
                    offHand = value;
                    OnPropertyChanged("OffHand");
                }
            }
        }

        [XmlElement("OneHand")]
        [Setting]
        [DefaultValue(true)]
        public bool OneHand
        {
            get { return oneHand; }
            set
            {
                if (oneHand != value)
                {
                    oneHand = value;
                    OnPropertyChanged("OneHand");
                }
            }
        }

        [XmlElement("TwoHand")]
        [Setting]
        [DefaultValue(false)]
        public bool TwoHand
        {
            get { return twoHand; }
            set
            {
                if (twoHand != value)
                {
                    twoHand = value;
                    OnPropertyChanged("TwoHand");
                }
            }
        }


        [XmlElement("RandomOrder")]
        [Setting]
        [DefaultValue(false)]
        public bool RandomOrder
        {
            get { return randomOrder; }
            set
            {
                if (randomOrder != value)
                {
                    randomOrder = value;
                    OnPropertyChanged("RandomOrder");
                }
            }
        }

        [XmlElement("MinShardCount")]
        [Setting]
        [DefaultValue(0)]
        public int MinShardCount
        {
            get { return minShardCount; }
            set
            {
                if (minShardCount != value)
                {
                    minShardCount = value;
                    OnPropertyChanged("MinShardCount");
                }
            }
        }

    }
}
