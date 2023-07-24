using CaptivityEvents.Custom;
using MCM.Abstractions.Base.Global;
using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using System.Collections.Generic;
using System.Linq;

namespace CaptivityEvents.Config
{
    public class CESettingsEvent
    {
        public string WeightedChanceOfOccurring = "";

        public string BackgroundName = "";
    }

    public class CESettingsEvents
    {
        private FluentGlobalSettings _settings;

        private static CESettingsEvents _instance = null;

        public static CESettingsEvents Instance
        {
            get
            {
                _instance ??= new CESettingsEvents();
                return _instance;
            }
        }

        public Dictionary<string, bool> EventToggle { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, CESettingsEvent> EventSettings { get; set; } = new Dictionary<string, CESettingsEvent>();

        public void InitializeSettings(List<CECustomModule> moduleCustoms, List<CEEvent> callableEvents)
        {
            ISettingsBuilder builder = BaseSettingsBuilder.Create("CaptivityEventsCustomEvents", "Captivity Events Optional Events");

            EventToggle = new Dictionary<string, bool>();
            EventSettings = new Dictionary<string, CESettingsEvent>();

            int eventModuleId = 0;
            int eventId = 0;

            if (builder != null)
            {
                builder.SetFormat("json2").SetFolderName("Global").SetSubFolder("zCaptivityEvents");

                foreach (CECustomModule module in moduleCustoms)
                {
                    eventModuleId += 1;
                    eventId = 0;
                    foreach (CEEvent currentEvent in module.CEEvents)
                    {
                        eventId += 1;

                        if (!EventToggle.ContainsKey(currentEvent.Name) && callableEvents.Exists((item) => item.Name == currentEvent.Name))
                        {
                            string folderName = null;

                            if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                            {
                                folderName = module.CEModuleName + "/{=CESETTINGS0098}Captive";
                            }
                            else if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                            {
                                folderName = module.CEModuleName + "/{=CESETTINGS0099}Captor";
                            }
                            else if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                            {
                                folderName = module.CEModuleName + "/{=CESETTINGS0088}Random";
                            }

                            if (folderName == null) continue;

                            if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution))
                            {
                                folderName += "/{=CESETTINGS1034}Prostitution Events";
                            }
                            else if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Slavery))
                            {
                                folderName += "/{=CESETTINGS1042}Slavery Events";
                            }
                            else if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Common))
                            {
                                folderName += "/{=CESETTINGS1028}Common Events";
                            }
                            else
                            {
                                folderName += "/{=CESETTINGS1122}Other Events";
                            }

                            folderName += "/" + currentEvent.Name;

                            builder.CreateGroup(folderName, groupBuilder =>
                            {
                                EventToggle.Add(currentEvent.Name, true);
                                EventSettings.Add(currentEvent.Name, new CESettingsEvent());

                                string hintText = currentEvent.Text.Length <= 300 ? currentEvent.Text : (currentEvent.Text.Substring(0, 300) + "...");

                                groupBuilder.AddToggle(currentEvent.Name + "_" + eventModuleId + "_" + eventId + "_toggle", "{=CESETTINGS1123}Event", new ProxyRef<bool>(() => EventToggle[currentEvent.Name], o => EventToggle[currentEvent.Name] = o), boolBuilder => boolBuilder.SetHintText(hintText).SetRequireRestart(false).SetOrder(0));

                                groupBuilder.AddText(currentEvent.Name + "_" + eventModuleId + "_" + eventId + "_weight", "{=CESETTINGS1124}Custom Event Frequency", new ProxyRef<string>(() => EventSettings[currentEvent.Name].WeightedChanceOfOccurring, o => EventSettings[currentEvent.Name].WeightedChanceOfOccurring = o), stringBuilder => stringBuilder.SetHintText("{=CESETTINGS1126}Default is " + currentEvent.WeightedChanceOfOccurring).SetRequireRestart(false).SetOrder(1));

                                groupBuilder.AddText(currentEvent.Name + "_" + eventModuleId + "_" + eventId + "_image", "{=CESETTINGS1125}Custom Event Image", new ProxyRef<string>(() => EventSettings[currentEvent.Name].BackgroundName, o => EventSettings[currentEvent.Name].BackgroundName = o), stringBuilder => stringBuilder.SetHintText("{=CESETTINGS1126}Default is " + (currentEvent.Backgrounds != null ? currentEvent.Backgrounds.ToString() : currentEvent.BackgroundName)).SetRequireRestart(false).SetOrder(2));
                            });
                        }
                    }
                }
                _settings?.Unregister();
                _settings = builder.BuildAsGlobal();
                _settings.Register();
            }
        }
    }
}