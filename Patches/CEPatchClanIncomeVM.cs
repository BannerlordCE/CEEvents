using CaptivityEvents.Brothel;
using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(ClanIncomeVM), "RefreshList")]
    internal class CEPatchClanIncomeVM
    {
        public static MethodInfo GetDefaultIncome = AccessTools.Method(typeof(ClanIncomeVM), "GetDefaultIncome");
        public static MethodInfo OnIncomeSelection = AccessTools.Method(typeof(ClanIncomeVM), "OnIncomeSelection");

        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.Instance != null && CESettings.Instance.ProstitutionControl;

        [HarmonyPostfix]
        public static void RefreshList(ClanIncomeVM __instance)
        {
            foreach (CEBrothel brothel in CEBrothelBehavior.GetPlayerBrothels())
            {
                __instance.Incomes.Add(new CEBrothelClanFinanceItemVM(brothel, brothelIncome =>
                                                                               {
                                                                                   OnIncomeSelection.Invoke(__instance, new object[] { brothelIncome });
                                                                               }, __instance.OnRefresh));
            }

            __instance.RefreshTotalIncome();
            OnIncomeSelection.Invoke(__instance, new[] { GetDefaultIncome.Invoke(__instance, null) });
            __instance.RefreshValues();
        }
    }
}