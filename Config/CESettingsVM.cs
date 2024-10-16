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
        public CESettingsVM()
        {
            GeneralOptions = new CESettingsVMCategory(this, new TextObject("General", null), GeneralList, false);
            EventsListOptions = new CESettingsVMCategory(this, new TextObject("Event List", null), EventsList, false);
            CustomFlagsOptions = new CESettingsVMCategory(this, new TextObject("Custom Flags", null), CustomFlagList, false);
            IntegrationsOptions = new CESettingsVMCategory(this, new TextObject("Integrations Options", null), IntegrationsList, false);
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

        private void ExecuteDone() => ScreenManager.PopScreen();

        private void ExecuteCancel() => ScreenManager.PopScreen();

        public CESettingsVMCategory GeneralOptions { get; }

        public CESettingsVMCategory CaptiveOptions { get; }

        public CESettingsVMCategory CaptorOptions { get; }

        public CESettingsVMCategory RandomOptions { get; }

        public CESettingsVMCategory EventsListOptions { get; }

        public CESettingsVMCategory CustomFlagsOptions { get; }

        public CESettingsVMCategory IntegrationsOptions { get; }
    }
}