﻿#define V127

using CaptivityEvents.Brothel;
using CaptivityEvents.Config;
using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using System;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanFinance;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Actions;

namespace CaptivityEvents.Patches
{

    // TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories ClanIncomeVM
    [HarmonyPatch(typeof(ClanIncomeVM), "RefreshList")]
    internal class CEPatchClanIncomeVM
    {
        public static MethodInfo GetDefaultIncome = AccessTools.Method(typeof(ClanIncomeVM), "GetDefaultIncome");
        public static MethodInfo OnIncomeSelection = AccessTools.Method(typeof(ClanIncomeVM), "OnIncomeSelection");

        public static AccessTools.FieldRef<ClanIncomeVM, Action<ClanCardSelectionInfo>> _openCardSelectionPopup = AccessTools.FieldRefAccess<ClanIncomeVM, Action<ClanCardSelectionInfo>>("_openCardSelectionPopup");

        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.Instance?.ProstitutionControl ?? true;


        [HarmonyPostfix]
        public static void RefreshList(ClanIncomeVM __instance)
        {
            try
            {
                foreach (CEBrothel brothel in CEBrothelBehavior.GetPlayerBrothels())
                {
                    Workshop workshop = new(brothel.Settlement, "_brothel_" + (brothel.Settlement.StringId ?? ""));
                    WorkshopType workshopType = WorkshopType.Find("brewery");

                    workshop.InitializeWorkshop(brothel.Owner, workshopType);

                    try
                    {

                        CEBrothelClanFinanceItemVM brothelFinanceItemVM = new(
                            brothel,
                            workshop,
                            brothelIncome => { OnIncomeSelection.Invoke(__instance, [brothelIncome]); },
                            __instance.OnRefresh,
                            _openCardSelectionPopup.Invoke(__instance));

                        __instance.Incomes.Add(brothelFinanceItemVM);
                    }
                    catch (Exception ex)
                    {
                        CECustomHandler.ForceLogToFile("CEPatchClanIncomeVM brothelFinanceItemVM: " + ex);
                    }

                    Hero.MainHero.RemoveOwnedWorkshop(workshop);
                }

                // For Nice Purposes of Workshop Number being 1 don't really care about the limit
                int count = CEBrothelBehavior.GetPlayerBrothels().Count;
                GameTexts.SetVariable("STR1", GameTexts.FindText("str_CE_properties", null));
                GameTexts.SetVariable("LEFT", Hero.MainHero.OwnedWorkshops.Count + count);
                GameTexts.SetVariable("RIGHT", Campaign.Current.Models.WorkshopModel.GetMaxWorkshopCountForClanTier(Clan.PlayerClan.Tier) + count);
                GameTexts.SetVariable("STR2", GameTexts.FindText("str_LEFT_over_RIGHT_in_paranthesis", null));
                __instance.WorkshopText = GameTexts.FindText("str_STR1_space_STR2", null).ToString();

                __instance.RefreshTotalIncome();
                OnIncomeSelection.Invoke(__instance, [GetDefaultIncome.Invoke(__instance, null)]);
                __instance.RefreshValues();
            } 
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("CEPatchClanIncomeVM RefreshList: " + e);
            }
        }
    }

}