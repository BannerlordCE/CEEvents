using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions.FluentBuilder.Implementation;
using MCM.Abstractions.Ref;
using MCM.Abstractions.Settings.Base.Global;
using System.Collections.Generic;

namespace CaptivityEvents
{
    public class CESettingsFlags
    {

        private FluentGlobalSettings _settings;

        private static CESettingsFlags _instance;

        public static CESettingsFlags Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CESettingsFlags();
                }
                return _instance;
            }
        }

        public Dictionary<string, bool> CustomFlags { get; set; } = new Dictionary<string, bool>();


        public void InitializeSettings(List<CECustom> ceFlags)
        {
            ISettingsBuilder builder = new DefaultSettingsBuilder("CaptivityEventsFlags", "Captivity Events Custom Flags")
                .SetFormat("json")
                .SetFolderName("zCaptivityEvents")
                .SetSubFolder("FlagSettings");

            foreach (CECustom module in ceFlags)
            {
                builder.CreateGroup("{=CESETTINGS0090}Custom Flags of " + module.CEModuleName, groupBuilder =>
                {
                    foreach (string flag in module.CEFlags)
                    {
                        if (!CustomFlags.ContainsKey(flag))
                        {
                            CustomFlags.Add(flag, true);
                            groupBuilder.AddBool(flag, flag, new ProxyRef<bool>(() => CustomFlags[flag], o => CustomFlags[flag] = o), boolBuilder => boolBuilder.SetHintText("{=CESETTINGS0091}Custom Flag Toggle"));
                        }
                    }
                });
            }

            _settings = builder.BuildAsGlobal();
            _settings.Register();
        }

    }
}
