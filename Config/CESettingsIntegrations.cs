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
            ISettingsBuilder builder = BaseSettingsBuilder.Create("CESettingsIntegrations", "Captivity Events Integrations");

            bool shouldRegister = false;

            if (builder != null)
            {

#if BETA
                ModuleInfo KLBShackles = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "KLBShackles"; });
#else
                ModuleInfo KLBShackles = ModuleInfo.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "zCaptivityEvents"; });
#endif

                builder.SetFormat("json2").SetFolderName("Global").SetSubFolder("zCaptivityEvents");

                if (KLBShackles != null)
                {
                    builder.CreateGroup("KLBShackles", groupBuilder =>
                    {
                        groupBuilder.AddBool("toggleKLBShackles", "Toggle KLBShackles", new ProxyRef<bool>(() => ActivateKLBShackles, o => ActivateKLBShackles = o), boolBuilder => boolBuilder.SetHintText("Toggles ActivateKLBShackles Integration").SetRequireRestart(false));
                    });
                    shouldRegister = true;
                }

                if (_settings != null) _settings.Unregister();
                _settings = builder.BuildAsGlobal();
                if (shouldRegister) _settings.Register();
            }
        }
    }
}
