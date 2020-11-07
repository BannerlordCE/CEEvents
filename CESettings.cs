using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Dropdown;
using MCM.Abstractions.Settings.Base;
using MCM.Abstractions.Settings.Base.Global;
using System;
using System.Collections.Generic;

namespace CaptivityEvents
{
    public interface ICustomSettingsProvider
    {
        bool EventCaptiveOn { get; set; }
        float EventOccurrenceOther { get; set; }
        float EventOccurrenceSettlement { get; set; }
        float EventOccurrenceLord { get; set; }
        bool EventCaptorOn { get; set; }
        float EventOccurrenceCaptor { get; set; }
        bool EventCaptorDialogue { get; set; }
        bool EventCaptorNotifications { get; set; }
        bool EventCaptorCustomTextureNotifications { get; set; }
        bool EventRandomEnabled { get; set; }
        float EventRandomFireChance { get; set; }
        bool EventCaptorGearCaptives { get; set; }
        bool EventProstituteGear { get; set; }
        bool HuntLetPrisonersEscape { get; set; }
        float HuntBegins { get; set; }
        int AmountOfTroopsForHunt { get; set; }
        bool PrisonerEscapeBehavior { get; set; }
        int PrisonerHeroEscapeChanceParty { get; set; }
        int PrisonerHeroEscapeChanceSettlement { get; set; }
        int PrisonerHeroEscapeChanceOther { get; set; }
        int PrisonerNonHeroEscapeChanceParty { get; set; }
        int PrisonerNonHeroEscapeChanceSettlement { get; set; }
        int PrisonerNonHeroEscapeChanceOther { get; set; }
        DropdownDefault<string> EscapeAutoRansom { get; set; }
        DropdownDefault<string> BrothelOption { get; set; }
        bool PrisonerExceeded { get; set; }
        bool NonSexualContent { get; set; }
        bool SexualContent { get; set; }
        bool CommonControl { get; set; }
        bool ProstitutionControl { get; set; }
        bool SlaveryToggle { get; set; }
        bool FemdomControl { get; set; }
        bool BestialityControl { get; set; }
        bool RomanceControl { get; set; }
        bool StolenGear { get; set; }
        bool StolenGearQuest { get; set; }
        float StolenGearDuration { get; set; }
        float StolenGearChance { get; set; }
        int BetterOutFitChance { get; set; }
        int WeaponChance { get; set; }
        int WeaponBetterChance { get; set; }
        bool WeaponSkill { get; set; }
        int RangedBetterChance { get; set; }
        bool RangedSkill { get; set; }
        int HorseChance { get; set; }
        bool HorseSkill { get; set; }
        bool PregnancyToggle { get; set; }
        bool AttractivenessSkill { get; set; }
        int PregnancyChance { get; set; }
        bool UsePregnancyModifiers { get; set; }
        float PregnancyDurationInDays { get; set; }
        bool PregnancyMessages { get; set; }
        float RenownMin { get; set; }
        bool LogToggle { get; set; }
    }

    public class HardcodedCustomSettings : ICustomSettingsProvider
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
        public bool EventCaptorGearCaptives { get; set; } = true;
        public bool EventProstituteGear { get; set; } = true;
        public bool HuntLetPrisonersEscape { get; set; } = false;
        public float HuntBegins { get; set; } = 7f;
        public int AmountOfTroopsForHunt { get; set; } = 15;
        public bool PrisonerEscapeBehavior { get; set; } = true;
        public int PrisonerHeroEscapeChanceParty { get; set; } = 0;
        public int PrisonerHeroEscapeChanceSettlement { get; set; } = 0;
        public int PrisonerHeroEscapeChanceOther { get; set; } = -1;
        public int PrisonerNonHeroEscapeChanceParty { get; set; } = 0;
        public int PrisonerNonHeroEscapeChanceSettlement { get; set; } = 0;
        public int PrisonerNonHeroEscapeChanceOther { get; set; } = -1;
        public DropdownDefault<string> EscapeAutoRansom { get; set; } = new DropdownDefault<string>(new string[] {
            "{=CESETTINGS1115}Off",
            "{=CESETTINGS1114}Disabled For Player",
            "{=CESETTINGS1116}On"
        }, 0);
        public DropdownDefault<string> BrothelOption { get; set; } = new DropdownDefault<string>(new string[]
        {
            "{=CESETTINGS1117}Any",
            "{=CESETTINGS1118}Female",
            "{=CESETTINGS1119}Male"
        }, 1);
        public bool PrisonerExceeded { get; set; } = false;
        public bool NonSexualContent { get; set; } = true;
        public bool SexualContent { get; set; } = true;
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
        public bool AttractivenessSkill { get; set; } = true;
        public int PregnancyChance { get; set; } = 20;
        public bool UsePregnancyModifiers { get; set; } = true;
        public float PregnancyDurationInDays { get; set; } = 14f;
        public bool PregnancyMessages { get; set; } = true;
        public float RenownMin { get; set; } = -150f;
        public bool LogToggle { get; set; } = false;
    }

    public class CESettingsCustom : AttributeGlobalSettings<CESettingsCustom>, ICustomSettingsProvider
    {
        public override string Id => "CaptivityEventsSettings";
        public override string DisplayName => "Captivity Events";
        public override string FolderName => "zCaptivityEvents";
        public override string FormatType => "json2";

        public override IDictionary<string, Func<BaseSettings>> GetAvailablePresets()
        {
            IDictionary<string, Func<BaseSettings>> basePresets = base.GetAvailablePresets(); // include the 'Default' preset that MCM provides
            basePresets.Add("Developer Mode", () => new CESettingsCustom { LogToggle = true });
            basePresets.Add("Hard Mode", () => new CESettingsCustom { StolenGear = true, StolenGearChance = 30f, BetterOutFitChance = 10, RenownMin = -300f });
            basePresets.Add("Easy Mode", () => new CESettingsCustom { StolenGear = false, RenownMin = 0f });

            return basePresets;
        }

        [SettingPropertyBool("{=CESETTINGS1000}Turn on Captive Events", Order = 1, RequireRestart = true)]
        [SettingPropertyGroup("{=CESETTINGS0098}Captive")]
        public bool EventCaptiveOn { get; set; } = true;

        [SettingPropertyFloatingInteger("{=CESETTINGS1002}Event wait between occurances in Traveling Party", 1f, 24f, "#0", Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1003}How often should an event occur while in a regular party. (Gametime in between events)")]
        [SettingPropertyGroup("{=CESETTINGS0098}Captive")]
        public float EventOccurrenceOther { get; set; } = 6f;

        [SettingPropertyFloatingInteger("{=CESETTINGS1004}Event wait between occurances in Settlement", 1f, 24f, "#0", Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1005}How should an event occur in settlements. (Prostitution affected too) (Gametime in between events)")]
        [SettingPropertyGroup("{=CESETTINGS0098}Captive")]
        public float EventOccurrenceSettlement { get; set; } = 6f;

        [SettingPropertyFloatingInteger("{=CESETTINGS1006}Event wait between occurances in Lord's Party", 1f, 24f, "#0", Order = 4, RequireRestart = false, HintText = "{=CESETTINGS1007}How often should an event occur in a lord's party. (Gametime in between events)")]
        [SettingPropertyGroup("{=CESETTINGS0098}Captive")]
        public float EventOccurrenceLord { get; set; } = 6f;

        [SettingPropertyBool("{=CESETTINGS1000}Turn on Captor Events", Order = 1, RequireRestart = true)]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventCaptorOn { get; set; } = true;

        [SettingPropertyFloatingInteger("{=CESETTINGS1008}Event wait between occurances while Captor", 1f, 100f, "#0", Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1009}How often should an event occur while Captor. (Gametime in between events)")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public float EventOccurrenceCaptor { get; set; } = 12f;

        [SettingPropertyBool("{=CESETTINGS1096}Prisoner Dialogue", Order = 3, RequireRestart = true, HintText = "{=CESETTINGS1097}Overwrites the default prisoner conversation menu.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventCaptorDialogue { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1010}Event Map Notifications", Order = 4, RequireRestart = false, HintText = "{=CESETTINGS1011}If events will fire as map notifications for captor.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventCaptorNotifications { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1016}Notifications Textures", RequireRestart = true, Order = 5, HintText = "{=CESETTINGS1017}Default Notifications textures are replaced by a custom texture notifications.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventCaptorCustomTextureNotifications { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1012}Random Events Enabled", Order = 6, RequireRestart = false, HintText = "{=CESETTINGS1013}Random events are events that do not require captives.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventRandomEnabled { get; set; } = true;

        [SettingPropertyFloatingInteger("{=CESETTINGS1014}Random Events Ratio", -1f, 101f, "0", Order = 7, RequireRestart = false, HintText = "{=CESETTINGS1015}If captives in party how often should random events fire over captor.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public float EventRandomFireChance { get; set; } = 20f;

        [SettingPropertyBool("{=CESETTINGS1018}Captives Gear (Captor)", Order = 8, RequireRestart = false, HintText = "{=CESETTINGS1019}Captive Heroes who have been stripped gain their gear back after escape.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventCaptorGearCaptives { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1112}Toggle Brothel Prisoner's Clothing (Hero)", Order = 9, RequireRestart = false, HintText = "{=CESETTINGS1113}Changes the brothel prisoner's clothing to the settlement's culture.")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool EventProstituteGear { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1094}Allow escape during hunt", Order = 10, RequireRestart = false, HintText = "{=CESETTINGS1095}Allows prisoners to escape if not killed or wounded in the hunt")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public bool HuntLetPrisonersEscape { get; set; } = false;

        [SettingPropertyFloatingInteger("{=CESETTINGS1080}Hunting begins time after mission load", 5f, 60f, "0", Order = 11, RequireRestart = false, HintText = "{=CESETTINGS1081}Seconds to wait until hunt begins")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public float HuntBegins { get; set; } = 7f;

        [SettingPropertyInteger("{=CESETTINGS1082}Max amount of prisoners to spawn for hunt", 1, 100, Order = 12, RequireRestart = false, HintText = "{=CESETTINGS1083}Amount of prisoners that will spawn for hunt")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public int AmountOfTroopsForHunt { get; set; } = 15;

        [SettingPropertyDropdown("{=CESETTINGS1120}Brothel Prisoners Allowed", Order = 8, RequireRestart = true, HintText = "{=CESETTINGS1121}Allows the gender to be prisoners in the brothel")]
        [SettingPropertyGroup("{=CESETTINGS0099}Captor")]
        public DropdownDefault<string> BrothelOption { get; set; } = new DropdownDefault<string>(new string[]
        {
            "{=CESETTINGS1117}Any",
            "{=CESETTINGS1118}Female",
            "{=CESETTINGS1119}Male"
        }, 1);

        [SettingPropertyBool("{=CESETTINGS1020}Modified Prisoner Escape Behavior", Order = 1, RequireRestart = true, HintText = "{=CESETTINGS1021}Use modified behaviour in game for prisoner escape, Turn off for compatability with mods that effect prisoner behavior.")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public bool PrisonerEscapeBehavior { get; set; } = true;

        [SettingPropertyInteger("{=CESETTINGS1098}Hero Prisoner Chance (Party)", -1, 100, Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1099}Hero prisoner daily escape chance from player's party, -1 means use regular calculation")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public int PrisonerHeroEscapeChanceParty { get; set; } = 0;

        [SettingPropertyInteger("{=CESETTINGS1100}Hero Prisoner Chance (Settlement)", -1, 100, Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1101}Hero prisoner daily escape chance from player's settlements, -1 means use regular calculation")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public int PrisonerHeroEscapeChanceSettlement { get; set; } = 0;

        [SettingPropertyInteger("{=CESETTINGS1102}Hero Prisoner Chance (Other)", -1, 100, Order = 4, RequireRestart = false, HintText = "{=CESETTINGS1103}Hero prisoner daily escape chance from non-player sources, -1 means use regular calculation")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public int PrisonerHeroEscapeChanceOther { get; set; } = -1;

        [SettingPropertyInteger("{=CESETTINGS1104}Regular Prisoner Escape Chance (Party)", -1, 100, Order = 5, RequireRestart = false, HintText = "{=CESETTINGS1105}Regular prisoner escape chance from player's party, -1 means use regular calculation, active only when prisoner exceeded")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public int PrisonerNonHeroEscapeChanceParty { get; set; } = 0;

        [SettingPropertyInteger("{=CESETTINGS1106}Regular Prisoner Escape Chance (Settlement)", -1, 100, Order = 6, RequireRestart = false, HintText = "{=CESETTINGS1107}Regular prisoner escape chance from player's settlements, -1 means use regular calculation, active only when prisoner exceeded")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public int PrisonerNonHeroEscapeChanceSettlement { get; set; } = 0;

        [SettingPropertyInteger("{=CESETTINGS1108}Regular Prisoner Escape Chance (Others)", -1, 100, Order = 7, RequireRestart = false, HintText = "{=CESETTINGS1109}Regular prisoner escape chance from non-player sources, -1 means use regular calculation, active only when prisoner exceeded")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public int PrisonerNonHeroEscapeChanceOther { get; set; } = -1;

        [SettingPropertyDropdown("{=CESETTINGS1026}Games Default Auto Ransom Behavior", Order = 8, RequireRestart = true, HintText = "{=CESETTINGS1027}Allow the games default behaviour regarding auto-ransom")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public DropdownDefault<string> EscapeAutoRansom { get; set; } = new DropdownDefault<string>(new string[] {
            "{=CESETTINGS1115}Off",
            "{=CESETTINGS1114}Disabled For Player",
            "{=CESETTINGS1116}On"
        }, 0);

        [SettingPropertyBool("{=CESETTINGS1110}Games Default Exceeded Prisoners System", Order = 9, RequireRestart = false, HintText = "{=CESETTINGS1111}Allows the games default behaviour regarding exceeded prisoner system, Hourly escape chance based on default 10% or above chances")]
        [SettingPropertyGroup("{=CESETTINGS0097}Escape")]
        public bool PrisonerExceeded { get; set; } = false;

        // WILL BE REMOVED STARTS

        [SettingPropertyBool("{=CESETTINGS1030}Non Sexual Content", Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1031}Should non sexual content events be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool NonSexualContent { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1032}Sexual Content", Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1033}Should sexual content events be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool SexualContent { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1028}Common Events", Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1029}Should events tagged with common be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool CommonControl { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1034}Prostitution Events", Order = 4, RequireRestart = true, HintText = "{=CESETTINGS1035}Should Prostitution events be enabled. Enables Brothel.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool ProstitutionControl { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1042}Slavery Events", Order = 5, RequireRestart = false, HintText = "{=CESETTINGS1043}Should Slavery events be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool SlaveryToggle { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1036}Femdom Events", Order = 6, RequireRestart = false, HintText = "{=CESETTINGS1037}Should Female Domination events be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool FemdomControl { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1038}Bestiality Events", Order = 7, RequireRestart = false, HintText = "{=CESETTINGS1039}Should Bestiality events be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool BestialityControl { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1040}Romance Events", Order = 8, RequireRestart = false, HintText = "{=CESETTINGS1041}Should Romance events be enabled.")]
        [SettingPropertyGroup("{=CESETTINGS0096}Events")]
        public bool RomanceControl { get; set; } = true;

        // WILL BE REMOVED ENDS

        [SettingPropertyBool("{=CESETTINGS1044}Stolen Gear", Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1045}Should the captor take the player's gear.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear")]
        public bool StolenGear { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1046}Stolen Gear Quest", Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1047}Should quest activate to retrieve stolen gear.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1093}Quest")]
        public bool StolenGearQuest { get; set; } = true;

        [SettingPropertyFloatingInteger("{=CESETTINGS1048}Stolen Gear Quest Duration", 1f, 50f, "0", Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1049}How long should stolen gear quest last.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1093}Quest")]
        public float StolenGearDuration { get; set; } = 10f;

        [SettingPropertyFloatingInteger("{=CESETTINGS1050}Stolen Gear Quest Chance", 0f, 100f, "0", Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1051}Chance someone finds your stolen gear.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1093}Quest")]
        public float StolenGearChance { get; set; } = 99.9f;

        [SettingPropertyInteger("{=CESETTINGS1052}Better OutFit Chance", 0, 100, Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1053}Likelyhood of receiving a better outfit (Given based on captors culture).")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1090}Outfit")]
        public int BetterOutFitChance { get; set; } = 25;

        [SettingPropertyInteger("{=CESETTINGS1054}Weapon Chance", 0, 100, Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1055}Likelyhood of receiving an weapon.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1091}Weapons")]
        public int WeaponChance { get; set; } = 75;

        [SettingPropertyInteger("{=CESETTINGS1056}Better Weapon Chance", 0, 100, Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1057}Likelyhood of receiving an better weapon.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1091}Weapons")]
        public int WeaponBetterChance { get; set; } = 20;

        [SettingPropertyBool("{=CESETTINGS1058}Weapon Skill Calculation (Ignores Chance)", Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1059}(One Handed, Two Handed, Polearm Skill) is calculated for the player to receive a better weapon or not, ignores chance.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1091}Weapons")]
        public bool WeaponSkill { get; set; } = true;

        [SettingPropertyInteger("{=CESETTINGS1060}Ranged Weapon Chance", 0, 100, Order = 4, RequireRestart = false, HintText = "{=CESETTINGS1061}Likelyhood of receiving an better ranged weapons.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1091}Weapons")]
        public int RangedBetterChance { get; set; } = 5;

        [SettingPropertyBool("{=CESETTINGS1062}Ranged Calculation (Ignores Chance)", Order = 5, RequireRestart = false, HintText = "{=CESETTINGS1063}(Bow, Crossbow, Throwing) is calculated for the player to receive an one-handed weapon or not, ignores chance.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1091}Weapons")]
        public bool RangedSkill { get; set; } = true;

        [SettingPropertyInteger("{=CESETTINGS1064}Horse Chance", 0, 100, Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1065}Likelyhood of receiving an horse.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1092}Horse")]
        public int HorseChance { get; set; } = 10;

        [SettingPropertyBool("{=CESETTINGS1066}Horse Skill Calculation (Ignores Chance)", Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1067}Horse Skill is calculates if the player receives a horse or not, ignores chance.")]
        [SettingPropertyGroup("{=CESETTINGS0094}Gear/{=CESETTINGS1092}Horse")]
        public bool HorseSkill { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1076}Pregnancy Toggle", Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1077}Allows impregnation by captor.")]
        [SettingPropertyGroup("{=CESETTINGS0093}Pregnancy")]
        public bool PregnancyToggle { get; set; } = true;

        [SettingPropertyBool("{=CESETTINGS1070}Attractiveness Calculation (Ignores Chance)", Order = 2, RequireRestart = false, HintText = "{=CESETTINGS1071}Perks (Perfect Health, Prominence, InBloom) and Charm level calculates impregnation chance, ignores chance.")]
        [SettingPropertyGroup("{=CESETTINGS0093}Pregnancy")]
        public bool AttractivenessSkill { get; set; } = true;

        [SettingPropertyInteger("{=CESETTINGS1068}Pregnancy Chance", 0, 100, Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1069}Likelyhood of impregnation, overrides various events")]
        [SettingPropertyGroup("{=CESETTINGS0093}Pregnancy")]
        public int PregnancyChance { get; set; } = 20;

        [SettingPropertyBool("{=CESETTINGS1072}Use PregnancyModifiers", RequireRestart = false, Order = 4, HintText = "{=CESETTINGS1073}Use event pregnancy modifiers on top of the current chance or calculation.")]
        [SettingPropertyGroup("{=CESETTINGS0093}Pregnancy")]
        public bool UsePregnancyModifiers { get; set; } = true;

        [SettingPropertyFloatingInteger("{=CESETTINGS1074}Pregnancy Duration In Days", 1f, 200f, "0", Order = 5, RequireRestart = false, HintText = "{=CESETTINGS1075}Days Impregnation by captor lasts.")]
        [SettingPropertyGroup("{=CESETTINGS0093}Pregnancy")]
        public float PregnancyDurationInDays { get; set; } = 14f;

        [SettingPropertyBool("{=CESETTINGS1078}Pregnancy Messages", Order = 6, RequireRestart = false, HintText = "{=CESETTINGS1079}Allows daily pregnancy messages by Captivity Events.")]
        [SettingPropertyGroup("{=CESETTINGS0093}Pregnancy")]
        public bool PregnancyMessages { get; set; } = true;

        [SettingPropertyFloatingInteger("{=CESETTINGS1084}Renown Min", -1000f, 1000f, "0", Order = 1, RequireRestart = false, HintText = "{=CESETTINGS1085}Renown can only drop to this point.")]
        [SettingPropertyGroup("{=CESETTINGS0095}Other")]
        public float RenownMin { get; set; } = -150f;

        [SettingPropertyBool("{=CESETTINGS1088}Logging Toggle (Slows Down The Game)", Order = 3, RequireRestart = false, HintText = "{=CESETTINGS1089}Log the events (Debug Mode)")]
        [SettingPropertyGroup("{=CESETTINGS0095}Other")]
        public bool LogToggle { get; set; } = false;
    }

    public class CESettings
    {
        private static ICustomSettingsProvider _provider = null;

        public static ICustomSettingsProvider Instance
        {
            get
            {
                if (CESettingsCustom.Instance != null) return CESettingsCustom.Instance;
                if (_provider != null) return _provider;
                _provider = new HardcodedCustomSettings();
                return _provider;

            }
        }
    }
}