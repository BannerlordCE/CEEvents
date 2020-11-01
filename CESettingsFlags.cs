using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions.Ref;
using MCM.Abstractions.Settings.Base.Global;
using System.Collections.Generic;

namespace CaptivityEvents
{
    public class CESettingsFlags
    {

        private FluentGlobalSettings _settings;

        private static CESettingsFlags _instance = null;

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

        public void InitializeSettings(List<CECustom> moduleCustoms)
        {
            ISettingsBuilder builder = BaseSettingsBuilder.Create("CaptivityEventsFlags", "Captivity Events Custom Flags");

            if (builder != null)
            {
                builder.SetFormat("json2").SetFolderName("zCaptivityEvents").SetSubFolder("FlagSettings");

                foreach (CECustom module in moduleCustoms)
                {

                    builder.CreateGroup("{=CESETTINGS0090}Custom Flags of " + module.CEModuleName, groupBuilder =>
                    {
                        foreach (CEFlagNode flag in module.CEFlags)
                        {
                            if (!CustomFlags.ContainsKey(flag.Id))
                            {
                                CustomFlags.Add(flag.Id, flag.DefaultValue);
                                groupBuilder.AddBool(flag.Id, flag.Name, new ProxyRef<bool>(() => CustomFlags[flag.Id], o => CustomFlags[flag.Id] = o), boolBuilder => boolBuilder.SetHintText(flag.HintText).SetRequireRestart(false));
                            }
                        }
                        foreach (CESkillNode skillNode in module.CESkills)
                        {
                            CESkills.AddCustomSkill(skillNode);
                        }
                    });
                }
                _settings = builder.BuildAsGlobal();
                _settings.Register();

            }
            else
            {
                foreach (CECustom module in moduleCustoms)
                {
                    foreach (CEFlagNode flag in module.CEFlags)
                    {
                        if (!CustomFlags.ContainsKey(flag.Id))
                        {
                            CustomFlags.Add(flag.Id, flag.DefaultValue);
                        }
                    }
                    foreach (CESkillNode skillNode in module.CESkills)
                    {
                        CESkills.AddCustomSkill(skillNode);
                    }
                }
            }
        }
    }
}
