using MCM.Abstractions.Base.Global;
using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using System.Linq;
using TaleWorlds.ModuleManager;

namespace CaptivityEvents.Config
{
    public class CESettingsIntegrations
    {
        private FluentGlobalSettings _settings;

        private static CESettingsIntegrations _instance;

        public static CESettingsIntegrations Instance
        {
            get
            {
                _instance ??= new CESettingsIntegrations();

                return _instance;
            }
        }

        public bool ActivateKLBShackles;
        public bool ActivatePrimaeNoctisBLord;
        public bool ActivateHotButter;

        public void InitializeSettings()
        {
            bool shouldRegister = false;


            ModuleInfo hotButter = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.Id.ToLower().StartsWith("hotbutter"));
            ModuleInfo klbShackles = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == "KLBShackles");
            ModuleInfo primaeNoctisBLord = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.Id.ToLower().StartsWith("primaenoctisblord"));

            if (klbShackles != null || hotButter != null || primaeNoctisBLord != null) shouldRegister = true;

            if (!shouldRegister) return;

            ISettingsBuilder builder = BaseSettingsBuilder.Create("CESettingsIntegrations", "Captivity Events Integrations");

            if (builder != null)
            {
                builder.SetFormat("json2").SetFolderName("Global").SetSubFolder("zCaptivityEvents");

                builder.CreateGroup("Integrations", groupBuilder =>
                                                    {
                                                        if (klbShackles != null)
                                                        {
                                                            groupBuilder.AddBool("KLBShackles", "KLBShackles (Slave Gear)", new ProxyRef<bool>(() => ActivateKLBShackles, o => ActivateKLBShackles = o), boolBuilder => boolBuilder.SetHintText("Enables equipment of slave gear on player-as-captive. (Make sure to double check if the extension is turned on in the launcher)").SetRequireRestart(false));
                                                        }

                                                        if (primaeNoctisBLord != null)
                                                        {
                                                            groupBuilder.AddBool("PrimaeNoctisBLord", "Primae Noctis (Laws and Stats)", new ProxyRef<bool>(() => ActivatePrimaeNoctisBLord, o => ActivatePrimaeNoctisBLord = o), boolBuilder => boolBuilder.SetHintText("Enables Laws to have a effect on the captivity and stats for sex. (Make sure to double check if the extension is turned on in the launcher)").SetRequireRestart(false));
                                                        }

                                                        if (hotButter != null)
                                                        {
                                                            groupBuilder.AddBool("HotButter", "Hot Butter (Animated Scenes)", new ProxyRef<bool>(() => ActivateHotButter, o => ActivateHotButter = o), boolBuilder => boolBuilder.SetHintText("Enables Custom Sex Scenes in Brothel/Other.  (Make sure to double check if the extension is turned on in the launcher)").SetRequireRestart(false));
                                                        }
                                                    });

                _settings?.Unregister();
                _settings = builder.BuildAsGlobal();
                _settings.Register();
            }
        }
    }
}