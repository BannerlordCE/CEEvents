using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions.Ref;
using MCM.Abstractions.Settings.Base.Global;
using System.Linq;
using TaleWorlds.ModuleManager;

namespace CaptivityEvents.Config
{
    public class CESettingsIntegrations
    {
        private FluentGlobalSettings _settings;

        private static CESettingsIntegrations _instance = null;

        public static CESettingsIntegrations Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CESettingsIntegrations();
                }
                return _instance;
            }
        }

        public bool ActivateKLBShackles = true;

        public bool ActivateHotButter = true;

        public void InitializeSettings()
        {
            bool shouldRegister = false;

            ModuleInfo KLBShackles = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "KLBShackles"; });
            ModuleInfo HotButter = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "hotbutter"; });

            if (KLBShackles != null || HotButter != null) shouldRegister = true;
            if (!shouldRegister) return;

            ISettingsBuilder builder = BaseSettingsBuilder.Create("CESettingsIntegrations", "Captivity Events Integrations");

            if (builder != null)
            {
                builder.SetFormat("json2").SetFolderName("Global").SetSubFolder("zCaptivityEvents");

                builder.CreateGroup("Integrations", groupBuilder =>
                {
                    if (KLBShackles != null)
                    {
                        groupBuilder.AddBool("KLBShackles", "KLBShackles (Slave Gear)", new ProxyRef<bool>(() => ActivateKLBShackles, o => ActivateKLBShackles = o), boolBuilder => boolBuilder.SetHintText("Enables equipment of slave gear on player-as-captive.").SetRequireRestart(false));
                    }

                    if (HotButter != null)
                    {
                        groupBuilder.AddBool("HotButter", "Hot Butter (Animated Scenes)", new ProxyRef<bool>(() => ActivateHotButter, o => ActivateHotButter = o), boolBuilder => boolBuilder.SetHintText("Enables Custom Sex Scenes in Brothel/Other.").SetRequireRestart(false));
                    }
                });

                if (_settings != null) _settings.Unregister();
                _settings = builder.BuildAsGlobal();
                _settings.Register();
            }
        }
    }
}