#define BETA
using MCM.Abstractions.FluentBuilder;
using MCM.Abstractions.Ref;
using MCM.Abstractions.Settings.Base.Global;
using System.Linq;
#if BETA
using TaleWorlds.ModuleManager;
#endif

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

        public bool ActivateKLBShackles = false;

        public void InitializeSettings()
        {
            bool shouldRegister = false;

#if BETA
            ModuleInfo KLBShackles = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "KLBShackles"; });
#else
            ModuleInfo KLBShackles = ModuleInfo.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "zCaptivityEvents"; });
#endif

            if (KLBShackles != null) shouldRegister = true;
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
                });

                if (_settings != null) _settings.Unregister();
                _settings = builder.BuildAsGlobal();
                _settings.Register();
            }
        }
    }
}
