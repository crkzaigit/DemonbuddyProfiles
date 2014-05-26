using System.ComponentModel;
using System.Configuration;
using System.IO;
using Belphegor.GUI;
using Zeta.Common.Xml;
using Zeta.XmlEngine;

namespace Belphegor.Settings
{
    [XmlElement("CrusaderSettings")]
    internal class CrusaderSettings : XmlSettings
    {
        public CrusaderSettings()
            : base(Path.Combine(Path.Combine(SettingsDirectory, "Belphegor"), "CrusaderSettings.xml"))
        {
        }

        [XmlElement("MaximumRange")]
        [DisplayName("Maximum Range")]
        [Category("Crusader")]
        [DefaultValue(15f)]
        [Description("The maximum range for attacks.")]
        [Setting]
        [Limit(7, 50)]
        public float MaximumRange { get; set; }

        #region Secondary

        [XmlElement("SweepAttackAoECount")]
        [DisplayName("Sweep Attack AoE Count")]
        [Category("Secondary")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int SweepAttackAoECount { get; set; }

        [XmlElement("BlessedHammerAoECount")]
        [DisplayName("Blessed Hammer AoE Count")]
        [Category("Secondary")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int BlessedHammerkAoECount { get; set; }

        #endregion

        #region Defensive

        [XmlElement("JudgmentAoECount")]
        [DisplayName("Judgment AoE Count")]
        [Category("Defensive")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int JudgmentAoECount { get; set; }

        [XmlElement("ShieldGlareAoECount")]
        [DisplayName("Shield Glare AoE Count")]
        [Category("Defensive")]
        [DefaultValue(3)]
        [Setting]
        [Limit(1, 10)]
        public int ShieldGlareAoECount { get; set; }

        [XmlElement("IronSkinHpPct")]
        [DisplayName("Iron Skin Health Pct")]
        [Category("Defensive")]
        [DefaultValue(0.5)]
        [Setting]
        [Limit(0, 1)]
        public double IronSkinHpPct { get; set; }

        [XmlElement("ConsecrationHpPct")]
        [DisplayName("Consecration Health Pct")]
        [Category("Defensive")]
        [DefaultValue(0.5)]
        [Setting]
        [Limit(0, 1)]
        public double ConsecrationHpPct { get; set; }

        #endregion

        #region Laws

        [XmlElement("LawsOfHopeHpPct")]
        [DisplayName("Laws Of Hope Health Pct")]
        [Category("Laws")]
        [DefaultValue(0.5)]
        [Setting]
        [Limit(0, 1)]
        public double LawsOfHopeHpPct { get; set; }

        [XmlElement("LawsOfJusticepPct")]
        [DisplayName("Laws Of Justice Health Pct")]
        [Category("Laws")]
        [DefaultValue(0.5)]
        [Setting]
        [Limit(0, 1)]
        public double LawsOfJusticeHpPct { get; set; }

        #endregion

        #region Conviction

        [XmlElement("BombardmentAoECount")]
        [DisplayName("Bombardment AoE Count")]
        [Category("Conviction")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int BombardmentAoECount { get; set; }

        [XmlElement("FallingSwordAoECount")]
        [DisplayName("Falling Sword AoE Count")]
        [Category("Conviction")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int FallingSwordAoECount { get; set; }

        [XmlElement("HeavensFuryAoECount")]
        [DisplayName("Heavens Fury AoE Count")]
        [Category("Conviction")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int HeavensFuryAoECount { get; set; }

        #endregion

        #region Utility

        [XmlElement("CondemnAoECount")]
        [DisplayName("Condemn AoE Count")]
        [Category("Utility")]
        [DefaultValue(5)]
        [Setting]
        [Limit(1, 10)]
        public int CondemnAoECount { get; set; }

        [XmlElement("SteedChargeOOC")]
        [DisplayName("Steed Charge OOC")]
        [Description("Use Steed Charge Out Of Combat.")]
        [Category("Utility")]
        [DefaultValue(true)]
        [Setting]
        public bool SteedChargeOOC { get; set; }

        [XmlElement("SteedChargeRange")]
        [DisplayName("Steed Charge Range")]
        [Description("The Minimum range before using Steed Charge.")]
        [Category("Utility")]
        [DefaultValue(15f)]
        [Setting]
        [Limit(1, 50)]
        public float SteedChargeMinRange { get; set; }

        #endregion
    }
}