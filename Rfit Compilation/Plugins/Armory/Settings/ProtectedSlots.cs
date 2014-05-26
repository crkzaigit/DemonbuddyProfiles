using System.ComponentModel;
using System.Configuration;
using System.IO;
using Zeta.Common.Xml;
using Zeta.XmlEngine;

namespace Armory.Settings
{
    [XmlElement("ProtectedSlots")]
    public class ProtectedSlots : XmlSettings
    {
        #region Singleton
        private static ProtectedSlots _instance;

        public ProtectedSlots() : base(Path.Combine(Path.Combine(SettingsDirectory, ArmorySettings.BattleTagName), "ProtectedSlots.xml")) { }

        public static ProtectedSlots Instance
        {
            get { return _instance ?? (_instance = new ProtectedSlots()); }
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
        private bool ring1;
        private bool waist;
        private bool ring2;
        private bool legs;
        private bool feet;
        private bool mainHand;
        private bool offHand;
        #endregion

        [XmlElement("Shoulders")]
        [Setting]
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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

        [XmlElement("Ring1")]
        [Setting]
        [DefaultValue(false)]
        public bool Ring1
        {
            get { return ring1; }
            set
            {
                if (ring1 != value)
                {
                    ring1 = value;
                    OnPropertyChanged("Ring1");
                }
            }
        }

        [XmlElement("Waist")]
        [Setting]
        [DefaultValue(false)]
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

        [XmlElement("Ring2")]
        [Setting]
        [DefaultValue(false)]
        public bool Ring2
        {
            get { return ring2; }
            set
            {
                if (ring2 != value)
                {
                    ring2 = value;
                    OnPropertyChanged("Ring2");
                }
            }
        }

        [XmlElement("Legs")]
        [Setting]
        [DefaultValue(false)]
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
        [DefaultValue(false)]
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

        [XmlElement("MainHand")]
        [Setting]
        [DefaultValue(false)]
        public bool MainHand
        {
            get { return mainHand; }
            set
            {
                if (mainHand != value)
                {
                    mainHand = value;
                    OnPropertyChanged("MainHand");
                }
            }
        }

        [XmlElement("OffHand")]
        [Setting]
        [DefaultValue(false)]
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
    }
}
