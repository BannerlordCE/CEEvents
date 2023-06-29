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

        private static CESettingsIntegrations _instance = null;

        public static CESettingsIntegrations Instance
        {
            get
            {
                _instance ??= new CESettingsIntegrations();
                return _instance;
            }
        }

        public bool ActivateKLBShackles = false;
        public bool ActivatePrimaeNoctisBLord = false;
        public bool ActivateHotButter = false;

        public void InitializeSettings()
        {
            bool shouldRegister = false;



            ModuleInfo KLBShackles = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "KLBShackles"; });
            ModuleInfo HotButter = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "hotbutterscenes" || searchInfo.Id == "hotbutter"; });
            ModuleInfo PrimaeNoctisBLord = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "PrimaeNoctisBLord"; });

            if (KLBShackles != null || HotButter != null || PrimaeNoctisBLord != null) shouldRegister = true;
            if (!shouldRegister) return;

            ISettingsBuilder builder = BaseSettingsBuilder.Create("CESettingsIntegrations", "Captivity Events Integrations");

            if (builder != null)
            {
                builder.SetFormat("json2").SetFolderName("Global").SetSubFolder("zCaptivityEvents");

                builder.CreateGroup("Integrations", groupBuilder =>
                {
                    if (KLBShackles != null)
                    {
                        groupBuilder.AddBool("KLBShackles", "KLBShackles (Slave Gear)", new ProxyRef<bool>(() => ActivateKLBShackles, o => ActivateKLBShackles = o), boolBuilder => boolBuilder.SetHintText("Enables equipment of slave gear on player-as-captive. (Make sure to double check if the extension is turned on in the launcher)").SetRequireRestart(false));
                    }

                    if (PrimaeNoctisBLord != null)
                    {
                        groupBuilder.AddBool("PrimaeNoctisBLord", "Primae Noctis (Laws and Stats)", new ProxyRef<bool>(() => ActivatePrimaeNoctisBLord, o => ActivatePrimaeNoctisBLord = o), boolBuilder => boolBuilder.SetHintText("Enables Laws to have a effect on the captivity and stats for sex. (Make sure to double check if the extension is turned on in the launcher)").SetRequireRestart(false));
                    } 

                    if (HotButter != null)
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