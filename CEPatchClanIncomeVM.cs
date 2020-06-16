using CaptivityEvents.Brothel;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.Library;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(ClanIncomeVM), "RefreshList")]
    internal class CEPatchClanIncomeVM
    {
        public static MethodInfo GetDefaultIncome = AccessTools.Method(typeof(ClanIncomeVM), "GetDefaultIncome");
        public static MethodInfo OnIncomeSelection = AccessTools.Method(typeof(ClanIncomeVM), "OnIncomeSelection");

        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            return CESettings.Instance.ProstitutionControl;
        }

        [HarmonyPostfix]
        public static void RefreshList(ClanIncomeVM __instance)
        {
            foreach (CEBrothel brothel in CEBrothelBehaviour.GetPlayerBrothels())
            {
                __instance.Incomes.Add(new CEBrothelClanFinanceItemVM(brothel, new Action<ClanFinanceIncomeItemBaseVM>((ClanFinanceIncomeItemBaseVM brothelIncome) =>
                {
                    OnIncomeSelection.Invoke(__instance, new object[] { brothelIncome } );
                }), new Action(__instance.OnRefresh)));
            }
            __instance.RefreshTotalIncome();
            OnIncomeSelection.Invoke(__instance, new object[] { GetDefaultIncome.Invoke(__instance, null) } );
            __instance.RefreshValues();
        }
    }
}
