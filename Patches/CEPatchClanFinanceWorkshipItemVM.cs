using CaptivityEvents.Config;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace CaptivityEvents.Patches
{

    // TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance ClanFinanceWorkshopItemVM
    [HarmonyPatch(typeof(ClanFinanceWorkshopItemVM), "OnStoreOutputInWarehousePercentageUpdated")]
    internal class CEPatchClanFinanceWorkshipItemVM
    {
        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.Instance?.ProstitutionControl ?? true;

        [HarmonyPrefix]
        public static bool OnStoreOutputInWarehousePercentageUpdated(ClanFinanceWorkshopItemVM __instance, SelectorVM<WorkshopPercentageSelectorItemVM> selector)
        {
            if (__instance.Workshop.Tag.StartsWith("_brothel_"))
            {
                return false;
            }
            return true;
        }

    }
}
