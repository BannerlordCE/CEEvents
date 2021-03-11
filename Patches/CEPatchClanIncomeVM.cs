#define BETA
using CaptivityEvents.Brothel;
using CaptivityEvents.Config;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Patches
{
    // TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories ClanIncomeVM
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
#if BETA
            MBBindingList<ClanFinanceIncomeItemBaseVM> Incomes = new MBBindingList<ClanFinanceIncomeItemBaseVM>();

            foreach (ClanFinanceWorkshopItemVM itemVM in __instance.Incomes)
            {
                Incomes.Add(itemVM);
            }
#endif


            foreach (CEBrothel brothel in CEBrothelBehavior.GetPlayerBrothels())
            {

                CEBrothelClanFinanceItemVM brothelFinanceItemVM = new CEBrothelClanFinanceItemVM(brothel, brothelIncome => { OnIncomeSelection.Invoke(__instance, new object[] { brothelIncome }); }, __instance.OnRefresh);

#if BETA
                Incomes.Add(brothelFinanceItemVM);
#else
                __instance.Incomes.Add(brothelFinanceItemVM);
#endif
            }

            // For Nice Purposes of Workshop Number being 1 don't really care about the limit
            int count = CEBrothelBehavior.GetPlayerBrothels().Count;
            GameTexts.SetVariable("STR1", GameTexts.FindText("str_CE_properties", null));
            GameTexts.SetVariable("LEFT", Hero.MainHero.OwnedWorkshops.Count + count);
            GameTexts.SetVariable("RIGHT", Campaign.Current.Models.WorkshopModel.GetMaxWorkshopCountForPlayer() + count);
            GameTexts.SetVariable("STR2", GameTexts.FindText("str_LEFT_over_RIGHT_in_paranthesis", null));
            __instance.WorkshopText = GameTexts.FindText("str_STR1_space_STR2", null).ToString();

#if BETA
            PropertyInfo fi = __instance.GetType().GetProperty("TotalIncome", BindingFlags.Instance | BindingFlags.Public);
            if (fi != null) fi.SetValue(__instance, Incomes.Sum((ClanFinanceIncomeItemBaseVM i) => i.Income));

            OnIncomeSelection.Invoke(__instance, new[] { Incomes.FirstOrDefault<ClanFinanceIncomeItemBaseVM>() });
            __instance.RefreshValues();
            __instance.OnPropertyChangedWithValue(Incomes, "Incomes");
#else
            __instance.RefreshTotalIncome();
            OnIncomeSelection.Invoke(__instance, new[] { GetDefaultIncome.Invoke(__instance, null) });
            __instance.RefreshValues();
#endif
        }
    }

#if BETA
    [HarmonyPatch(typeof(ClanIncomeVM), "Incomes", MethodType.Getter)]
    internal static class CEPatchClanIncomeVMIncomes
    {
        public static MethodInfo OnIncomeSelection = AccessTools.Method(typeof(ClanIncomeVM), "OnIncomeSelection");

        [HarmonyPostfix]
        public static void getIncomes(ClanIncomeVM __instance, ref MBBindingList<ClanFinanceIncomeItemBaseVM> __result)
        {
            MBBindingList<ClanFinanceIncomeItemBaseVM> Incomes = new MBBindingList<ClanFinanceIncomeItemBaseVM>();

            foreach (ClanFinanceWorkshopItemVM itemVM in __instance.Incomes)
            {
                Incomes.Add(itemVM);
            }

            foreach (CEBrothel brothel in CEBrothelBehavior.GetPlayerBrothels())
            {
                CEBrothelClanFinanceItemVM brothelFinanceItemVM = new CEBrothelClanFinanceItemVM(brothel, brothelIncome => { OnIncomeSelection.Invoke(__instance, new object[] { brothelIncome }); }, __instance.OnRefresh);
                Incomes.Add(brothelFinanceItemVM);
            }

            __result = Incomes;
        }
    }
#endif
}