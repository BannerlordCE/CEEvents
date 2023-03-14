using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CECustomSettings
    {
        public bool EventCaptiveOn { get; set; } = true;
        public float EventOccurrenceOther { get; set; } = 6f;
        public float EventOccurrenceSettlement { get; set; } = 6f;
        public float EventOccurrenceLord { get; set; } = 6f;
        public bool EventCaptorOn { get; set; } = true;
        public float EventOccurrenceCaptor { get; set; } = 12f;
        public bool EventCaptorDialogue { get; set; } = true;
        public bool EventCaptorNotifications { get; set; } = true;
        public bool EventCaptorCustomTextureNotifications { get; set; } = true;
        public bool EventRandomEnabled { get; set; } = true;
        public float EventRandomFireChance { get; set; } = 20f;
        public float EventOccurrenceRandom { get; set; } = 12f;
        public bool EventCaptorGearCaptives { get; set; } = true;
        public bool EventProstituteGear { get; set; } = true;
        public bool HuntLetPrisonersEscape { get; set; } = false;
        public float HuntBegins { get; set; } = 7f;
        public int AmountOfTroopsForHunt { get; set; } = 15;
        public bool PrisonerEscapeBehavior { get; set; } = true;
        public bool PrisonerHeroEscapeParty { get; set; } = true;
        public bool PrisonerHeroEscapeSettlement { get; set; } = true;
        public bool PrisonerHeroEscapeOther { get; set; } = false;
        public bool PrisonerNonHeroEscapeParty { get; set; } = true;
        public bool PrisonerNonHeroEscapeSettlement { get; set; } = true;
        public bool PrisonerNonHeroEscapeOther { get; set; } = false;
        public int PrisonerHeroEscapeChanceParty { get; set; } = 0;
        public int PrisonerHeroEscapeChanceSettlement { get; set; } = 0;
        public int PrisonerHeroEscapeChanceOther { get; set; } = 0;
        public int PrisonerNonHeroEscapeChanceParty { get; set; } = 0;
        public int PrisonerNonHeroEscapeChanceSettlement { get; set; } = 0;
        public int PrisonerNonHeroEscapeChanceOther { get; set; } = 0;
        public int BrothelHeroEscapeChance { get; set; } = 0;
        public int BrothelNonHeroEscapeChance { get; set; } = 0;
        public int EscapeAutoRansom { get; set; } = 0;
        public int BrothelOption { get; set; } = 1;
        public bool PrisonerExceeded { get; set; } = false;
        public bool NonSexualContent { get; set; } = true;
        public bool SexualContent { get; set; } = true;
        public bool CustomBackgrounds { get; set; } = true;
        public bool CommonControl { get; set; } = true;
        public bool ProstitutionControl { get; set; } = true;
        public bool SlaveryToggle { get; set; } = true;
        public bool FemdomControl { get; set; } = true;
        public bool BestialityControl { get; set; } = true;
        public bool RomanceControl { get; set; } = true;
        public bool StolenGear { get; set; } = true;
        public bool StolenGearQuest { get; set; } = true;
        public float StolenGearDuration { get; set; } = 10f;
        public float StolenGearChance { get; set; } = 99.9f;
        public int BetterOutFitChance { get; set; } = 25;
        public int WeaponChance { get; set; } = 75;
        public int WeaponBetterChance { get; set; } = 20;
        public bool WeaponSkill { get; set; } = true;
        public int RangedBetterChance { get; set; } = 5;
        public bool RangedSkill { get; set; } = true;
        public int HorseChance { get; set; } = 10;
        public bool HorseSkill { get; set; } = true;
        public bool PregnancyToggle { get; set; } = true;
        public bool PregnancyToggleFemalexFemale { get; set; } = true;
        public bool AttractivenessSkill { get; set; } = true;
        public int PregnancyChance { get; set; } = 20;
        public bool UsePregnancyModifiers { get; set; } = true;
        public float PregnancyDurationInDays { get; set; } = 14f;
        public bool PregnancyMessages { get; set; } = true;
        public int RenownChoice { get; set; } = 1;
        public float RenownMin { get; set; } = -150f;
        public bool LogToggle { get; set; } = true;
    }
}
