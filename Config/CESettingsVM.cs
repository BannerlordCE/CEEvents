using System;
using System.Collections.Generic;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace CaptivityEvents.Config
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class MappedAttribute : Attribute
    {
        public MappedAttribute()
        {
        }
    }

    public class CESettingsVM : ViewModel
    {
        private class SettingsSnapshot
        {
            public bool EventCaptiveOn;
            public float EventOccurrenceOther;
            public float EventOccurrenceSettlement;
            public float EventOccurrenceLord;
            public bool EventCaptorOn;
            public float EventOccurrenceCaptor;
            public bool EventCaptorDialogue;
            public bool EventCaptorGearCaptives;
            public bool HuntLetPrisonersEscape;
            public bool EventCaptorNotifications;
            public bool EventCaptorCustomTextureNotifications;
            public bool EventRandomEnabled;
            public float EventOccurrenceRandom;
            public float EventRandomFireChance;
            public int BrothelOptionIndex;
            public bool LogToggle;
            public bool PrisonerEscapeBehavior;
            public bool PrisonerHeroEscapeParty;
            public bool PrisonerHeroEscapeSettlement;
            public bool PrisonerHeroEscapeOther;
            public float HuntBegins;
            public int AmountOfTroopsForHunt;
            public bool StolenGear;
            public bool StolenGearQuest;
            public float StolenGearDuration;
            public float StolenGearChance;
            public int BetterOutFitChance;
            public int WeaponChance;
            public bool WeaponSkill;
            public bool HorseSkill;
            public bool PregnancyToggle;
            public int PregnancyChance;
            public bool UsePregnancyModifiers;
            public float PregnancyDurationInDays;
            public bool PregnancyMessages;
            public bool AttractivenessSkill;
            public float RenownMin;
            public bool ProstitutionControl;
            public bool SlaveryToggle;
            public bool FemdomControl;
            public bool BestialityControl;
            public bool RomanceControl;
            public bool CustomBackgrounds;
            public int EventAmountOfImagesToPreload;
        }

        private readonly SettingsSnapshot _initial;

        public CESettingsVM()
        {
            GeneralOptions = new CESettingsVMCategory(this, new TextObject("General", null), GeneralList, false);
            CaptureOptions = new CESettingsVMCategory(this, new TextObject("Capture", null), CaptureList, false);
            EscapeOptions = new CESettingsVMCategory(this, new TextObject("Escape", null), EscapeList, false);
            GearOptions = new CESettingsVMCategory(this, new TextObject("Gear", null), GearList, false);
            PregnancyOptions = new CESettingsVMCategory(this, new TextObject("Pregnancy", null), PregnancyList, false);
            TuningOptions = new CESettingsVMCategory(this, new TextObject("Tuning", null), TuningList, false);
            EventsListOptions = new CESettingsVMCategory(this, new TextObject("Event List", null), EventsList, false);
            CustomFlagsOptions = new CESettingsVMCategory(this, new TextObject("Custom Flags", null), CustomFlagList, false);
            IntegrationsOptions = new CESettingsVMCategory(this, new TextObject("Integrations Options", null), IntegrationsList, false);

            _initial = new SettingsSnapshot
            {
                EventCaptiveOn = CESettings.Instance.EventCaptiveOn,
                EventOccurrenceOther = CESettings.Instance.EventOccurrenceOther,
                EventOccurrenceSettlement = CESettings.Instance.EventOccurrenceSettlement,
                EventOccurrenceLord = CESettings.Instance.EventOccurrenceLord,
                EventCaptorOn = CESettings.Instance.EventCaptorOn,
                EventOccurrenceCaptor = CESettings.Instance.EventOccurrenceCaptor,
                EventCaptorDialogue = CESettings.Instance.EventCaptorDialogue,
                EventCaptorGearCaptives = CESettings.Instance.EventCaptorGearCaptives,
                HuntLetPrisonersEscape = CESettings.Instance.HuntLetPrisonersEscape,
                EventCaptorNotifications = CESettings.Instance.EventCaptorNotifications,
                EventCaptorCustomTextureNotifications = CESettings.Instance.EventCaptorCustomTextureNotifications,
                EventRandomEnabled = CESettings.Instance.EventRandomEnabled,
                EventOccurrenceRandom = CESettings.Instance.EventOccurrenceRandom,
                EventRandomFireChance = CESettings.Instance.EventRandomFireChance,
                BrothelOptionIndex = CESettings.Instance.BrothelOption.SelectedIndex,
                LogToggle = CESettings.Instance.LogToggle,
                PrisonerEscapeBehavior = CESettings.Instance.PrisonerEscapeBehavior,
                PrisonerHeroEscapeParty = CESettings.Instance.PrisonerHeroEscapeParty,
                PrisonerHeroEscapeSettlement = CESettings.Instance.PrisonerHeroEscapeSettlement,
                PrisonerHeroEscapeOther = CESettings.Instance.PrisonerHeroEscapeOther,
                HuntBegins = CESettings.Instance.HuntBegins,
                AmountOfTroopsForHunt = CESettings.Instance.AmountOfTroopsForHunt,
                StolenGear = CESettings.Instance.StolenGear,
                StolenGearQuest = CESettings.Instance.StolenGearQuest,
                StolenGearDuration = CESettings.Instance.StolenGearDuration,
                StolenGearChance = CESettings.Instance.StolenGearChance,
                BetterOutFitChance = CESettings.Instance.BetterOutFitChance,
                WeaponChance = CESettings.Instance.WeaponChance,
                WeaponSkill = CESettings.Instance.WeaponSkill,
                HorseSkill = CESettings.Instance.HorseSkill,
                PregnancyToggle = CESettings.Instance.PregnancyToggle,
                PregnancyChance = CESettings.Instance.PregnancyChance,
                UsePregnancyModifiers = CESettings.Instance.UsePregnancyModifiers,
                PregnancyDurationInDays = CESettings.Instance.PregnancyDurationInDays,
                PregnancyMessages = CESettings.Instance.PregnancyMessages,
                AttractivenessSkill = CESettings.Instance.AttractivenessSkill,
                RenownMin = CESettings.Instance.RenownMin,
                ProstitutionControl = CESettings.Instance.ProstitutionControl,
                SlaveryToggle = CESettings.Instance.SlaveryToggle,
                FemdomControl = CESettings.Instance.FemdomControl,
                BestialityControl = CESettings.Instance.BestialityControl,
                RomanceControl = CESettings.Instance.RomanceControl,
                CustomBackgrounds = CESettings.Instance.CustomBackgrounds,
                EventAmountOfImagesToPreload = CESettings.Instance.EventAmountOfImagesToPreload,
            };
        }

        private IEnumerable<ICEOptionData> GeneralList
        {
            get
            {
                yield return new CEManagedBooleanOptionData("EventCaptiveOn", "{=CESETTINGS1000}Turn on Captive Events", "{=CESETTINGS1000}Turn on Captive Events", CESettings.Instance.EventCaptiveOn ? 1f : 0f, (value) =>
                {
                    return CESettings.Instance.EventCaptiveOn ? 1f : 0f;
                    //CESettings.Instance.EventCaptiveOn = value == 1f;
                    //return value;
                });

                yield return new CEManagedNumericOptionData("EventOccurrenceOther", "{=CESETTINGS1002}Event wait between occurrences in Traveling Party", "{=CESETTINGS1003}How often should an event occur while in a regular party. (Game time in between events)", CESettings.Instance.EventOccurrenceOther, (value) =>
                {
                    CESettings.Instance.EventOccurrenceOther = value;
                    return value;
                }, 1f, 24f);

                yield return new CEManagedNumericOptionData("EventOccurrenceSettlement", "{=CESETTINGS1004}Event wait between occurrences in Settlement", "{=CESETTINGS1005}How should an event occur in settlements. (Prostitution affected too) (Game time in between events)", CESettings.Instance.EventOccurrenceSettlement, (value) =>
                {
                    CESettings.Instance.EventOccurrenceSettlement = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedNumericOptionData("EventOccurrenceLord", "{=CESETTINGS1006}Event wait between occurrences in Lord's Party", "{=CESETTINGS1007}How often should an event occur in a lord's party. (Game time in between events)", CESettings.Instance.EventOccurrenceLord, (value) =>
                {
                    CESettings.Instance.EventOccurrenceLord = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedBooleanOptionData("EventCaptorOn", "{=CESETTINGS1001}Turn on Captor Events", "{=CESETTINGS1001}Turn on Captor Events", CESettings.Instance.EventCaptorOn ? 1f : 0f, (value) =>
                {
                    return CESettings.Instance.EventCaptorOn ? 1f : 0f;

                    //CESettings.Instance.EventCaptorOn = value == 1f;
                    //return value;
                });

                yield return new CEManagedNumericOptionData("EventOccurrenceCaptor", "{=CESETTINGS1008}Event wait between occurrences while Captor", "{=CESETTINGS1009}How often should an event occur while Captor. (Game time in between events)", CESettings.Instance.EventOccurrenceCaptor, (value) =>
                {
                    CESettings.Instance.EventOccurrenceCaptor = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedBooleanOptionData("EventCaptorGearCaptives", "{=CESETTINGS1018}Captives Gear (Captor)", "{=CESETTINGS1019}Captive Heroes who have been stripped gain their gear back after escape.", CESettings.Instance.EventCaptorGearCaptives ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.EventCaptorGearCaptives = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("HuntLetPrisonersEscape", "{=CESETTINGS1094}Allow escape during hunt", "{=CESETTINGS1095}Allows prisoners to escape if not killed or wounded in the hunt", CESettings.Instance.HuntLetPrisonersEscape ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.HuntLetPrisonersEscape = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("EventCaptorNotifications", "{=CESETTINGS1010}Event Map Notifications", "{=CESETTINGS1011}If events will fire as map notifications for captor/random.", CESettings.Instance.EventCaptorNotifications ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.EventCaptorNotifications = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("EventRandomEnabled", "Random Events Enabled", "Random events are events that do not require captives.", CESettings.Instance.EventRandomEnabled ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.EventRandomEnabled = value == 1f;
                    return value;
                });

                yield return new CEManagedNumericOptionData("EventOccurrenceRandom", "{=CESETTINGS0083}Event wait between occurrences while Random", "{=CESETTINGS0084}How often should an event occur while Random. (Game time in between events)", CESettings.Instance.EventOccurrenceRandom, (value) =>
                {
                    CESettings.Instance.EventOccurrenceRandom = value;
                    return value;
                }, 1f, 100f);

                List<SelectionData> selectedDataBrothel =
                [
                    new SelectionData(false, new TextObject("{=CESETTINGS1117}Any").ToString()),
                 new SelectionData(false, new TextObject("{=CESETTINGS1118}Female").ToString()),
                 new SelectionData(false, new TextObject("{=CESETTINGS1119}Male").ToString())
                ];

                yield return new CEManagedSelectionOptionData("BrothelOption", "{=CESETTINGS1120}Brothel Prisoners Allowed", "{=CESETTINGS1121}Allows the gender to be prisoners in the brothel", CESettings.Instance.BrothelOption.SelectedIndex, (value) =>
                {
                    CESettings.Instance.EventRandomEnabled = value == 1f;
                    return value;
                }, 1, selectedDataBrothel);

                yield return new CEManagedBooleanOptionData("LogToggle", "{=CESETTINGS1088}Logging Toggle (Slows Down The Game)", "{=CESETTINGS1089}Log the events (Debug Mode)", CESettings.Instance.LogToggle ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.LogToggle = value == 1f;
                    return value;
                });

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> EventsList
        {
            get
            {
                yield break;
            }
        }

        private IEnumerable<ICEOptionData> CaptureList
        {
            get
            {
                yield return new CEManagedNumericOptionData("EventOccurrenceOther", "Event wait (Party)", "How often should an event occur while in a regular party.", CESettings.Instance.EventOccurrenceOther, (value) =>
                {
                    CESettings.Instance.EventOccurrenceOther = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedNumericOptionData("EventOccurrenceSettlement", "Event wait (Settlement)", "How often should an event occur in settlements.", CESettings.Instance.EventOccurrenceSettlement, (value) =>
                {
                    CESettings.Instance.EventOccurrenceSettlement = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedNumericOptionData("EventOccurrenceLord", "Event wait (Lord)", "How often should an event occur in lord parties.", CESettings.Instance.EventOccurrenceLord, (value) =>
                {
                    CESettings.Instance.EventOccurrenceLord = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedNumericOptionData("EventOccurrenceCaptor", "Event wait (Captor)", "How often should an event occur while captor.", CESettings.Instance.EventOccurrenceCaptor, (value) =>
                {
                    CESettings.Instance.EventOccurrenceCaptor = value;
                    return value;
                }, 1f, 100f);

                yield return new CEManagedBooleanOptionData("EventCaptorDialogue", "Captor Dialogue", "Enable captor dialogue interactions.", CESettings.Instance.EventCaptorDialogue ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.EventCaptorDialogue = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("EventCaptorNotifications", "Captor Notifications", "Show notifications for captor events.", CESettings.Instance.EventCaptorNotifications ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.EventCaptorNotifications = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("EventCaptorCustomTextureNotifications", "Custom Texture Notifications", "Use custom textures in captor event notifications.", CESettings.Instance.EventCaptorCustomTextureNotifications ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.EventCaptorCustomTextureNotifications = value == 1f;
                    return value;
                });

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> EscapeList
        {
            get
            {
                yield return new CEManagedBooleanOptionData("PrisonerEscapeBehavior", "Prisoner Escape Enabled", "Allow prisoners to attempt escape.", CESettings.Instance.PrisonerEscapeBehavior ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.PrisonerEscapeBehavior = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("PrisonerHeroEscapeParty", "Hero Escape (Party)", "Allow hero prisoners to escape from traveling parties.", CESettings.Instance.PrisonerHeroEscapeParty ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.PrisonerHeroEscapeParty = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("PrisonerHeroEscapeSettlement", "Hero Escape (Settlement)", "Allow hero prisoners to escape from settlements.", CESettings.Instance.PrisonerHeroEscapeSettlement ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.PrisonerHeroEscapeSettlement = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("PrisonerHeroEscapeOther", "Hero Escape (Other)", "Allow hero prisoners to escape from other locations.", CESettings.Instance.PrisonerHeroEscapeOther ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.PrisonerHeroEscapeOther = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("HuntLetPrisonersEscape", "Allow escape during hunt", "Allows prisoners to escape if not killed or wounded in the hunt", CESettings.Instance.HuntLetPrisonersEscape ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.HuntLetPrisonersEscape = value == 1f;
                    return value;
                });

                yield return new CEManagedNumericOptionData("HuntBegins", "Hunt Start Day", "Days after capture before hunt begins.", CESettings.Instance.HuntBegins, (value) =>
                {
                    CESettings.Instance.HuntBegins = (int)value;
                    return value;
                }, 1f, 30f);

                yield return new CEManagedNumericOptionData("AmountOfTroopsForHunt", "Troops for Hunt", "Number of troops sent on hunt.", CESettings.Instance.AmountOfTroopsForHunt, (value) =>
                {
                    CESettings.Instance.AmountOfTroopsForHunt = (int)value;
                    return value;
                }, 1f, 200f);

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> GearList
        {
            get
            {
                yield return new CEManagedBooleanOptionData("StolenGear", "Stolen Gear System", "Enable captives losing gear when captured.", CESettings.Instance.StolenGear ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.StolenGear = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("StolenGearQuest", "Stolen Gear Quest", "Create quest to recover stolen gear.", CESettings.Instance.StolenGearQuest ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.StolenGearQuest = value == 1f;
                    return value;
                });

                yield return new CEManagedNumericOptionData("StolenGearDuration", "Gear Quest Duration (Days)", "Days before stolen gear quest expires.", CESettings.Instance.StolenGearDuration, (value) =>
                {
                    CESettings.Instance.StolenGearDuration = (int)value;
                    return value;
                }, 1f, 60f);

                yield return new CEManagedNumericOptionData("StolenGearChance", "Stolen Gear Chance (%)", "Percent chance gear is stolen on capture.", CESettings.Instance.StolenGearChance, (value) =>
                {
                    CESettings.Instance.StolenGearChance = value;
                    return value;
                }, 0f, 100f);

                yield return new CEManagedNumericOptionData("BetterOutFitChance", "Better Outfit Chance (%)", "Percent chance captive gets better outfit.", CESettings.Instance.BetterOutFitChance, (value) =>
                {
                    CESettings.Instance.BetterOutFitChance = (int)value;
                    return value;
                }, 0f, 100f);

                yield return new CEManagedNumericOptionData("WeaponChance", "Weapon Chance (%)", "Percent chance captive gains weapon.", CESettings.Instance.WeaponChance, (value) =>
                {
                    CESettings.Instance.WeaponChance = (int)value;
                    return value;
                }, 0f, 100f);

                yield return new CEManagedBooleanOptionData("WeaponSkill", "Weapon Skill Boost", "Captive gains weapon skill levels.", CESettings.Instance.WeaponSkill ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.WeaponSkill = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("HorseSkill", "Horse Skill Boost", "Captive gains horse riding skill levels.", CESettings.Instance.HorseSkill ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.HorseSkill = value == 1f;
                    return value;
                });

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> PregnancyList
        {
            get
            {
                yield return new CEManagedBooleanOptionData("PregnancyToggle", "Pregnancy System Enabled", "Enable pregnancy mechanic for female captives.", CESettings.Instance.PregnancyToggle ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.PregnancyToggle = value == 1f;
                    return value;
                });

                yield return new CEManagedNumericOptionData("PregnancyChance", "Pregnancy Chance (%)", "Percent chance female becomes pregnant.", CESettings.Instance.PregnancyChance, (value) =>
                {
                    CESettings.Instance.PregnancyChance = (int)value;
                    return value;
                }, 0f, 100f);

                yield return new CEManagedBooleanOptionData("UsePregnancyModifiers", "Pregnancy Modifiers", "Apply difficulty/age modifiers to pregnancy chance.", CESettings.Instance.UsePregnancyModifiers ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.UsePregnancyModifiers = value == 1f;
                    return value;
                });

                yield return new CEManagedNumericOptionData("PregnancyDurationInDays", "Pregnancy Duration (Days)", "Number of days pregnancy lasts.", CESettings.Instance.PregnancyDurationInDays, (value) =>
                {
                    CESettings.Instance.PregnancyDurationInDays = (int)value;
                    return value;
                }, 1f, 365f);

                yield return new CEManagedBooleanOptionData("PregnancyMessages", "Pregnancy Messages", "Show pregnancy-related messages and notifications.", CESettings.Instance.PregnancyMessages ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.PregnancyMessages = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("AttractivenessSkill", "Attractiveness Skill", "Captive gains attractiveness skill levels.", CESettings.Instance.AttractivenessSkill ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.AttractivenessSkill = value == 1f;
                    return value;
                });

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> TuningList
        {
            get
            {
                yield return new CEManagedNumericOptionData("EventRandomFireChance", "Random Fire Chance (%)", "Percent chance random event triggers.", CESettings.Instance.EventRandomFireChance, (value) =>
                {
                    CESettings.Instance.EventRandomFireChance = (int)value;
                    return value;
                }, 0f, 100f);

                yield return new CEManagedNumericOptionData("EventAmountOfImagesToPreload", "Preload Images", "Number of images to preload into memory.", CESettings.Instance.EventAmountOfImagesToPreload, (value) =>
                {
                    CESettings.Instance.EventAmountOfImagesToPreload = (int)value;
                    return value;
                }, 0f, 500f);

                yield return new CEManagedNumericOptionData("RenownMin", "Renown Minimum", "Minimum renown required for certain events.", CESettings.Instance.RenownMin, (value) =>
                {
                    CESettings.Instance.RenownMin = (int)value;
                    return value;
                }, 0f, 10000f);

                yield return new CEManagedBooleanOptionData("ProstitutionControl", "Prostitution Events", "Enable prostitution-related events.", CESettings.Instance.ProstitutionControl ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.ProstitutionControl = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("SlaveryToggle", "Slavery Events", "Enable slavery-related events.", CESettings.Instance.SlaveryToggle ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.SlaveryToggle = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("FemdomControl", "Femdom Events", "Enable female domination events.", CESettings.Instance.FemdomControl ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.FemdomControl = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("BestialityControl", "Bestiality Events", "Enable bestiality-related events.", CESettings.Instance.BestialityControl ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.BestialityControl = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("RomanceControl", "Romance Events", "Enable romance-related events.", CESettings.Instance.RomanceControl ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.RomanceControl = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("CustomBackgrounds", "Custom Backgrounds", "Use custom backgrounds in events.", CESettings.Instance.CustomBackgrounds ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.CustomBackgrounds = value == 1f;
                    return value;
                });

                yield return new CEManagedBooleanOptionData("LogToggle", "Debug Logging", "Enable debug log output for troubleshooting.", CESettings.Instance.LogToggle ? 1f : 0f, (value) =>
                {
                    CESettings.Instance.LogToggle = value == 1f;
                    return value;
                });

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> CustomFlagList
        {
            get
            {
                yield break;
            }
        }

        private IEnumerable<ICEOptionData> IntegrationsList
        {
            get
            {
                yield break;
            }
        }

        public float GetConfig(ICEOptionData data) => 0;

        public void SetConfig(ICEOptionData data, float value)
        {
        }

        private void ExecuteDone()
        {
            PersistSettings();
            ScreenManager.PopScreen();
        }

        private void PersistSettings()
        {
            // Persist current settings into XML so it can be reloaded externally if needed.
            try
            {
                var s = CESettings.Instance;
                string root = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Modules", "zCaptivityEvents", "ModuleData");
                System.IO.Directory.CreateDirectory(root);
                string file = System.IO.Path.Combine(root, "CESettingsRuntime.xml");

                var doc = new System.Xml.Linq.XDocument(
                    new System.Xml.Linq.XElement("CESettings",
                        new System.Xml.Linq.XElement("EventCaptiveOn", s.EventCaptiveOn),
                        new System.Xml.Linq.XElement("EventOccurrenceOther", s.EventOccurrenceOther),
                        new System.Xml.Linq.XElement("EventOccurrenceSettlement", s.EventOccurrenceSettlement),
                        new System.Xml.Linq.XElement("EventOccurrenceLord", s.EventOccurrenceLord),
                        new System.Xml.Linq.XElement("EventCaptorOn", s.EventCaptorOn),
                        new System.Xml.Linq.XElement("EventOccurrenceCaptor", s.EventOccurrenceCaptor),
                        new System.Xml.Linq.XElement("EventCaptorDialogue", s.EventCaptorDialogue),
                        new System.Xml.Linq.XElement("EventCaptorGearCaptives", s.EventCaptorGearCaptives),
                        new System.Xml.Linq.XElement("HuntLetPrisonersEscape", s.HuntLetPrisonersEscape),
                        new System.Xml.Linq.XElement("EventCaptorNotifications", s.EventCaptorNotifications),
                        new System.Xml.Linq.XElement("EventCaptorCustomTextureNotifications", s.EventCaptorCustomTextureNotifications),
                        new System.Xml.Linq.XElement("EventRandomEnabled", s.EventRandomEnabled),
                        new System.Xml.Linq.XElement("EventRandomFireChance", s.EventRandomFireChance),
                        new System.Xml.Linq.XElement("EventOccurrenceRandom", s.EventOccurrenceRandom),
                        new System.Xml.Linq.XElement("BrothelOptionIndex", s.BrothelOption.SelectedIndex),
                        new System.Xml.Linq.XElement("LogToggle", s.LogToggle),
                        new System.Xml.Linq.XElement("PrisonerEscapeBehavior", s.PrisonerEscapeBehavior),
                        new System.Xml.Linq.XElement("PrisonerHeroEscapeParty", s.PrisonerHeroEscapeParty),
                        new System.Xml.Linq.XElement("PrisonerHeroEscapeSettlement", s.PrisonerHeroEscapeSettlement),
                        new System.Xml.Linq.XElement("PrisonerHeroEscapeOther", s.PrisonerHeroEscapeOther),
                        new System.Xml.Linq.XElement("HuntBegins", s.HuntBegins),
                        new System.Xml.Linq.XElement("AmountOfTroopsForHunt", s.AmountOfTroopsForHunt),
                        new System.Xml.Linq.XElement("StolenGear", s.StolenGear),
                        new System.Xml.Linq.XElement("StolenGearQuest", s.StolenGearQuest),
                        new System.Xml.Linq.XElement("StolenGearDuration", s.StolenGearDuration),
                        new System.Xml.Linq.XElement("StolenGearChance", s.StolenGearChance),
                        new System.Xml.Linq.XElement("BetterOutFitChance", s.BetterOutFitChance),
                        new System.Xml.Linq.XElement("WeaponChance", s.WeaponChance),
                        new System.Xml.Linq.XElement("WeaponSkill", s.WeaponSkill),
                        new System.Xml.Linq.XElement("HorseSkill", s.HorseSkill),
                        new System.Xml.Linq.XElement("PregnancyToggle", s.PregnancyToggle),
                        new System.Xml.Linq.XElement("PregnancyChance", s.PregnancyChance),
                        new System.Xml.Linq.XElement("UsePregnancyModifiers", s.UsePregnancyModifiers),
                        new System.Xml.Linq.XElement("PregnancyDurationInDays", s.PregnancyDurationInDays),
                        new System.Xml.Linq.XElement("PregnancyMessages", s.PregnancyMessages),
                        new System.Xml.Linq.XElement("AttractivenessSkill", s.AttractivenessSkill),
                        new System.Xml.Linq.XElement("RenownMin", s.RenownMin),
                        new System.Xml.Linq.XElement("ProstitutionControl", s.ProstitutionControl),
                        new System.Xml.Linq.XElement("SlaveryToggle", s.SlaveryToggle),
                        new System.Xml.Linq.XElement("FemdomControl", s.FemdomControl),
                        new System.Xml.Linq.XElement("BestialityControl", s.BestialityControl),
                        new System.Xml.Linq.XElement("RomanceControl", s.RomanceControl),
                        new System.Xml.Linq.XElement("CustomBackgrounds", s.CustomBackgrounds),
                        new System.Xml.Linq.XElement("EventAmountOfImagesToPreload", s.EventAmountOfImagesToPreload)
                    )
                );
                doc.Save(file);
            }
            catch (Exception ex)
            {
                TaleWorlds.Library.Debug.Print($"[CE] PersistSettings XML failed: {ex.Message}");
            }
        }
        private void ExecuteCancel()
        {
            CESettings.Instance.EventCaptiveOn = _initial.EventCaptiveOn;
            CESettings.Instance.EventOccurrenceOther = _initial.EventOccurrenceOther;
            CESettings.Instance.EventOccurrenceSettlement = _initial.EventOccurrenceSettlement;
            CESettings.Instance.EventOccurrenceLord = _initial.EventOccurrenceLord;
            CESettings.Instance.EventCaptorOn = _initial.EventCaptorOn;
            CESettings.Instance.EventOccurrenceCaptor = _initial.EventOccurrenceCaptor;
            CESettings.Instance.EventCaptorDialogue = _initial.EventCaptorDialogue;
            CESettings.Instance.EventCaptorGearCaptives = _initial.EventCaptorGearCaptives;
            CESettings.Instance.HuntLetPrisonersEscape = _initial.HuntLetPrisonersEscape;
            CESettings.Instance.EventCaptorNotifications = _initial.EventCaptorNotifications;
            CESettings.Instance.EventCaptorCustomTextureNotifications = _initial.EventCaptorCustomTextureNotifications;
            CESettings.Instance.EventRandomEnabled = _initial.EventRandomEnabled;
            CESettings.Instance.EventOccurrenceRandom = _initial.EventOccurrenceRandom;
            CESettings.Instance.EventRandomFireChance = _initial.EventRandomFireChance;
            CESettings.Instance.BrothelOption.SelectedIndex = _initial.BrothelOptionIndex;
            CESettings.Instance.LogToggle = _initial.LogToggle;
            CESettings.Instance.PrisonerEscapeBehavior = _initial.PrisonerEscapeBehavior;
            CESettings.Instance.PrisonerHeroEscapeParty = _initial.PrisonerHeroEscapeParty;
            CESettings.Instance.PrisonerHeroEscapeSettlement = _initial.PrisonerHeroEscapeSettlement;
            CESettings.Instance.PrisonerHeroEscapeOther = _initial.PrisonerHeroEscapeOther;
            CESettings.Instance.HuntBegins = _initial.HuntBegins;
            CESettings.Instance.AmountOfTroopsForHunt = _initial.AmountOfTroopsForHunt;
            CESettings.Instance.StolenGear = _initial.StolenGear;
            CESettings.Instance.StolenGearQuest = _initial.StolenGearQuest;
            CESettings.Instance.StolenGearDuration = _initial.StolenGearDuration;
            CESettings.Instance.StolenGearChance = _initial.StolenGearChance;
            CESettings.Instance.BetterOutFitChance = _initial.BetterOutFitChance;
            CESettings.Instance.WeaponChance = _initial.WeaponChance;
            CESettings.Instance.WeaponSkill = _initial.WeaponSkill;
            CESettings.Instance.HorseSkill = _initial.HorseSkill;
            CESettings.Instance.PregnancyToggle = _initial.PregnancyToggle;
            CESettings.Instance.PregnancyChance = _initial.PregnancyChance;
            CESettings.Instance.UsePregnancyModifiers = _initial.UsePregnancyModifiers;
            CESettings.Instance.PregnancyDurationInDays = _initial.PregnancyDurationInDays;
            CESettings.Instance.PregnancyMessages = _initial.PregnancyMessages;
            CESettings.Instance.AttractivenessSkill = _initial.AttractivenessSkill;
            CESettings.Instance.RenownMin = _initial.RenownMin;
            CESettings.Instance.ProstitutionControl = _initial.ProstitutionControl;
            CESettings.Instance.SlaveryToggle = _initial.SlaveryToggle;
            CESettings.Instance.FemdomControl = _initial.FemdomControl;
            CESettings.Instance.BestialityControl = _initial.BestialityControl;
            CESettings.Instance.RomanceControl = _initial.RomanceControl;
            CESettings.Instance.CustomBackgrounds = _initial.CustomBackgrounds;
            CESettings.Instance.EventAmountOfImagesToPreload = _initial.EventAmountOfImagesToPreload;
            ScreenManager.PopScreen();
        }

        public CESettingsVMCategory GeneralOptions { get; }

        public CESettingsVMCategory CaptureOptions { get; }

        public CESettingsVMCategory EscapeOptions { get; }

        public CESettingsVMCategory GearOptions { get; }

        public CESettingsVMCategory PregnancyOptions { get; }

        public CESettingsVMCategory TuningOptions { get; }

        public CESettingsVMCategory EventsListOptions { get; }

        public CESettingsVMCategory CustomFlagsOptions { get; }

        public CESettingsVMCategory IntegrationsOptions { get; }
    }
}