using CaptivityEvents.Custom;
using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions.Ref;
using MCM.Abstractions.Settings.Base.Global;
using System.Collections.Generic;
using System.Linq;

namespace CaptivityEvents.Config
{
    public class CESettingsEvents
    {

        private FluentGlobalSettings _settings;

        private static CESettingsEvents _instance = null;

        public static CESettingsEvents Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CESettingsEvents();
                }
                return _instance;
            }
        }

        public Dictionary<string, bool> EventToggle { get; set; } = new Dictionary<string, bool>();

        public void InitializeSettings(List<CECustomModule> moduleCustoms, List<CEEvent> callableEvents)
        {
            ISettingsBuilder builder = BaseSettingsBuilder.Create("CaptivityEventsCustomEvents", "Captivity Events Optional Events");

            EventToggle = new Dictionary<string, bool>();

            if (builder != null)
            {
                builder.SetFormat("json2").SetFolderName("zCaptivityEvents").SetSubFolder("EventSettings");

                foreach (CECustomModule module in moduleCustoms)
                {
                    foreach (CEEvent currentEvent in module.CEEvents)
                    {
                        if (!EventToggle.ContainsKey(currentEvent.Name) && callableEvents.Exists((item) => item.Name == currentEvent.Name))
                        {
                            if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                            {
                                builder.CreateGroup(module.CEModuleName + "/{=CESETTINGS0098}Captive", groupBuilder =>
                                {

                                    EventToggle.Add(currentEvent.Name, true);
                                    groupBuilder.AddBool(currentEvent.Name, currentEvent.Name, new ProxyRef<bool>(() => EventToggle[currentEvent.Name], o => EventToggle[currentEvent.Name] = o), boolBuilder => boolBuilder.SetHintText(currentEvent.Text).SetRequireRestart(false));

                                });
                            }
                            else if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                            {
                                builder.CreateGroup(module.CEModuleName + "/{=CESETTINGS0099}Captor", groupBuilder =>
                                {

                                    EventToggle.Add(currentEvent.Name, true);
                                    groupBuilder.AddBool(currentEvent.Name, currentEvent.Name, new ProxyRef<bool>(() => EventToggle[currentEvent.Name], o => EventToggle[currentEvent.Name] = o), boolBuilder => boolBuilder.SetHintText(currentEvent.Text).SetRequireRestart(false));

                                });
                            }
                            else if (currentEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                            {
                                builder.CreateGroup(module.CEModuleName + "/{=CESETTINGS0088}Random", groupBuilder =>
                                {

                                    EventToggle.Add(currentEvent.Name, true);
                                    groupBuilder.AddBool(currentEvent.Name, currentEvent.Name, new ProxyRef<bool>(() => EventToggle[currentEvent.Name], o => EventToggle[currentEvent.Name] = o), boolBuilder => boolBuilder.SetHintText(currentEvent.Text).SetRequireRestart(false));

                                });
                            }
                        }
                    }
                }
                if (_settings != null) _settings.Unregister();
                _settings = builder.BuildAsGlobal();
                _settings.Register();
            }
        }
    }
}
