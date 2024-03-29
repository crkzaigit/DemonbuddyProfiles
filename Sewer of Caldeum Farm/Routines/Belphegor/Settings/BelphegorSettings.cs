﻿using System.ComponentModel;
using System.Configuration;
using System.IO;
using Belphegor.GUI;
using log4net;
using Zeta.Common;
using Zeta.Common.Xml;
using Zeta.XmlEngine;

namespace Belphegor.Settings
{
    [XmlElement("BelphegorSettings")]
    internal class BelphegorSettings : XmlSettings
    {
        private static BelphegorSettings _instance;
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        public BelphegorSettings() :
            base(Path.Combine(Path.Combine(SettingsDirectory, "Belphegor"), "BelphegorSettings.xml"))
        {
        }

        public static BelphegorSettings Instance
        {
            get { return _instance ?? (_instance = new BelphegorSettings()); }
        }

        [XmlElement("DestroyObjectDistance")]
        [DisplayName("Destroy Object Distance")]
        [Description("Distance to Distroy Object's")]
        [DefaultValue(15f)]
        [Setting]
        [Limit(0, 40)]
        public float DestroyObjectDistance { get; set; }

        [XmlElement("EnableClusterCounts")]
        [DisplayName("Enables clusters, used for AoE situations")]
        [DefaultValue(true)]
        [Setting]
        public bool EnableClusterCounts { get; set; }


        [XmlElement("EnableLevelUpSkilling")]
        [DisplayName("Auto Assign Skills On Level Up")]
        [Description("Assigns skills when you level up automatically")]
        [DefaultValue(true)]
        [Setting]
        public bool EnableLevelUpSkilling { get; set; }

        #region Health

        [XmlElement("HealthPotionPct")]
        [DisplayName("Health Potion %")]
        [Description("Health % to use Health Potions")]
        [DefaultValue(0.4)]
        [Category("Health")]
        [Setting]
        [Limit(0, 1)]
        public double HealthPotionPct { get; set; }

        [XmlElement("MoveToHealthGlobe")]
        [DefaultValue(true)]
        [DisplayName("Pickup health globes")]
        [Description("Whether we should move to health globe when the char is below the treshold.")]
        [Category("Health")]
        [Setting]
        public bool GetHealthGlobe { get; set; }

        [XmlElement("HealthGlobeHP")]
        [DefaultValue(0.5)]
        [DisplayName("Health globe treshold")]
        [Description("Indicates how much health should be left before moving to health globes.")]
        [Category("Health")]
        [Setting]
        [Limit(0, 1)]
        public double HealthGlobeHP { get; set; }

        [XmlElement("UseHealthWell")]
        [DefaultValue(false)]
        [DisplayName("Use Health Well's")]
        [Description("Whether we should move to health globe when the char is below the treshold.")]
        [Category("Health")]
        [Setting]
        public bool UseHealthWell { get; set; }

        [XmlElement("HealthWellHP")]
        [DefaultValue(0.5)]
        [DisplayName("Health Well treshold")]
        [Description("Indicates how much health should be left before moving to use health well.")]
        [Category("Health")]
        [Setting]
        [Limit(0, 1)]
        public double HealthWellHP { get; set; }

        [XmlElement("HealthWellDistance")]
        [DefaultValue(15)]
        [DisplayName("Health Well distance")]
        [Description("Indicates how far away a health well can be to be considered for use.")]
        [Category("Health")]
        [Setting]
        [Limit(10, 100)]
        public float HealthWellDistance { get; set; }

        [XmlElement("HealthGlobeDistance")]
        [DefaultValue(15)]
        [DisplayName("Health Globe distance")]
        [Description("Indicates how far away a health globe can be to be considered for pickup.")]
        [Category("Health")]
        [Setting]
        [Limit(0, 100)]
        public float HealthGlobeDistance { get; set; }

        #endregion

        #region Experience Boosts

        [XmlElement("PoolOfReflectionDistance")]
        [DefaultValue(40)]
        [DisplayName("Pool Of Reflection Distance")]
        [Description("Indicates how far away a Pool Of Reflection can be to be considered for use.")]
        [Category("Experience")]
        [Setting]
        [Limit(0, 100)]
        public float PoolOfReflectionDistance { get; set; }

        [XmlElement("UsePoolOfReflection")]
        [DefaultValue(true)]
        [DisplayName("Use Pool Of Reflection")]
        [Description("Uses pools of reflection to get the exp buff.")]
        [Category("Experience")]
        [Setting]
        [Limit(0, 100)]
        public bool UsePoolOfReflection { get; set; }

        [XmlElement("MoveToPowerGlobe")]
        [DefaultValue(true)]
        [DisplayName("Pickup Power globes")]
        [Description("Whether we should move to power globes.")]
        [Category("Experience")]
        [Setting]
        public bool GetPowerGlobe { get; set; }

        #endregion

        #region Class Late-Loading Wrappers

        private AvoidanceSettings _avoidanceSettings;
        private BarbarianSettings _barbSettings;
        private CrusaderSettings _crusaderSettigns;
        private DebugSettings _debugSettings;
        private DemonHunterSettings _dhSettings;
        private KitingSettings _kitingSettings;
        private MonkSettings _monkSettings;
        private WitchDoctorSettings _wdSettings;
        private WizardSettings _wizSettings;

        [Browsable(false)]
        public BarbarianSettings Barbarian
        {
            get { return _barbSettings ?? (_barbSettings = new BarbarianSettings()); }
        }

        [Browsable(false)]
        public CrusaderSettings Crusader
        {
            get { return _crusaderSettigns ?? (_crusaderSettigns = new CrusaderSettings()); }
        }

        [Browsable(false)]
        public DemonHunterSettings DemonHunter
        {
            get { return _dhSettings ?? (_dhSettings = new DemonHunterSettings()); }
        }

        [Browsable(false)]
        public MonkSettings Monk
        {
            get { return _monkSettings ?? (_monkSettings = new MonkSettings()); }
        }

        [Browsable(false)]
        public WitchDoctorSettings WitchDoctor
        {
            get { return _wdSettings ?? (_wdSettings = new WitchDoctorSettings()); }
        }

        [Browsable(false)]
        public WizardSettings Wizard
        {
            get { return _wizSettings ?? (_wizSettings = new WizardSettings()); }
        }

        [Browsable(false)]
        public AvoidanceSettings Avoidance
        {
            get { return _avoidanceSettings ?? (_avoidanceSettings = new AvoidanceSettings()); }
        }

        [Browsable(false)]
        public KitingSettings Kiting
        {
            get { return _kitingSettings ?? (_kitingSettings = new KitingSettings()); }
        }

        [Browsable(false)]
        public DebugSettings Debug
        {
            get { return _debugSettings ?? (_debugSettings = new DebugSettings()); }
        }

        #endregion

        /// <summary>
        ///     Saves all the tabs at once
        /// </summary>
        public void SaveAll()
        {
            Save();

            Barbarian.Save();
            Crusader.Save();
            DemonHunter.Save();
            Monk.Save();
            WitchDoctor.Save();
            Wizard.Save();

            Debug.Save();
            Avoidance.Save();
            Kiting.Save();

            Log.Info("Saved All Settings");
        }
    }
}