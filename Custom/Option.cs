using System;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;
using CaptivityEvents.Enums;

namespace CaptivityEvents.Custom
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public class Option
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string TriggerEventName { get; set; }

        [XmlArrayItem("TriggerEvent", IsNullable = true)]
        public TriggerEvent[] TriggerEvents { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Order { get; set; }

        [XmlArrayItem("RestrictedListOfConsequences", IsNullable = false)]
        public RestrictedListOfConsequences[] MultipleRestrictedListOfConsequences { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string OptionText { get; set; }

        public string PregnancyRiskModifier { get; set; }
        public string EscapeChance { get; set; }
        public string ReqHeroPartyHaveItem { get; set; }
        public string ReqCaptorPartyHaveItem { get; set; }
        public string ReqHeroHealthBelowPercentage { get; set; }
        public string ReqHeroHealthAbovePercentage { get; set; }
        public string ReqHeroCaptorRelationAbove { get; set; }
        public string ReqHeroCaptorRelationBelow { get; set; }
        public string ReqHeroSlaveLevelAbove { get; set; }
        public string ReqHeroSlaveLevelBelow { get; set; }
        public string ReqHeroProstituteLevelAbove { get; set; }
        public string ReqHeroProstituteLevelBelow { get; set; }
        public string ReqHeroTraitLevelAbove { get; set; }
        public string ReqHeroTraitLevelBelow { get; set; }
        public string ReqCaptorTraitLevelAbove { get; set; }
        public string ReqCaptorTraitLevelBelow { get; set; }
        public string ReqHeroSkillLevelAbove { get; set; }
        public string ReqHeroSkillLevelBelow { get; set; }
        public string ReqCaptorSkillLevelAbove { get; set; }
        public string ReqCaptorSkillLevelBelow { get; set; }
        public string ReqMoraleAbove { get; set; }
        public string ReqMoraleBelow { get; set; }
        public string ReqTroopsAbove { get; set; }
        public string ReqTroopsBelow { get; set; }
        public string ReqMaleTroopsAbove { get; set; }
        public string ReqMaleTroopsBelow { get; set; }
        public string ReqFemaleTroopsAbove { get; set; }
        public string ReqFemaleTroopsBelow { get; set; }
        public string ReqCaptivesAbove { get; set; }
        public string ReqCaptivesBelow { get; set; }
        public string ReqFemaleCaptivesAbove { get; set; }
        public string ReqFemaleCaptivesBelow { get; set; }
        public string ReqMaleCaptivesAbove { get; set; }
        public string ReqMaleCaptivesBelow { get; set; }
        public string ReqGoldAbove { get; set; }
        public string ReqGoldBelow { get; set; }
        public string SkillToLevel { get; set; }
        public string ReqCaptorSkill { get; set; }
        public string ReqHeroSkill { get; set; }
        public string TraitToLevel { get; set; }
        public string ReqCaptorTrait { get; set; }
        public string ReqHeroTrait { get; set; }
        public string GoldTotal { get; set; }
        public string CaptorGoldTotal { get; set; }
        public string MoraleTotal { get; set; }
        public string RelationTotal { get; set; }
        public string HealthTotal { get; set; }
        public string RenownTotal { get; set; }
        public string ProstitutionTotal { get; set; }
        public string SlaveryTotal { get; set; }
        public string TraitTotal { get; set; }
        public string SkillTotal { get; set; }
    }
}