using CaptivityEvents.Brothel;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.Library;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(ClanIncomeVM), "RefreshList")]
    internal class CEPatchClanIncomeVM
    {
        public static MethodInfo RefreshTotalIncome = AccessTools.Method(typeof(ClanIncomeVM), "RefreshTotalIncome");
        public static MethodInfo OnIncomeSelection = AccessTools.Method(typeof(ClanIncomeVM), "OnIncomeSelection");
        public static MethodInfo RefreshValues = AccessTools.Method(typeof(ClanIncomeVM), "RefreshValues");
        public static MethodInfo GetDefaultIncome = AccessTools.Method(typeof(ClanIncomeVM), "GetDefaultIncome");

        //public static AccessTools.FieldRef<Hero, Hero> _spouse = AccessTools.FieldRefAccess<Hero, Hero>("_spouse");
        //public static AccessTools.FieldRef<Hero, List<Hero>> _exSpouses = AccessTools.FieldRefAccess<Hero, List<Hero>>("_exSpouses");

        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            return CESettings.Instance.ProstitutionControl;
        }

        [HarmonyPostfix]
        private static void RefreshList(ClanIncomeVM __instance)
        {

            foreach (CEBrothel brothel in CEBrothelBehaviour.GetPlayerBrothels())
            {
                __instance.Incomes.Add(new CEBrothelClanFinanceItemVM(brothel, new Action<ClanFinanceIncomeItemBaseVM>((param) =>
                {
                    object[] paramToUse = new object[1];
                    paramToUse[0] = param;
                    OnIncomeSelection.Invoke(__instance, paramToUse);
                }), new Action(__instance.OnRefresh)));
            }
            __instance.RefreshTotalIncome();
            object[] parameters = new object[1];
            parameters[0] = GetDefaultIncome.Invoke(__instance, null);
            OnIncomeSelection.Invoke(__instance, parameters);
            __instance.RefreshValues();
        }
    }
}
