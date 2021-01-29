#define BETA
using CaptivityEvents.Brothel;
using CaptivityEvents.Config;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace CaptivityEvents.Patches
{
#if BETA
    [HarmonyPatch(typeof(DefaultClanFinanceModel), "CalculateClanIncomeInternal")]
#else
    [HarmonyPatch(typeof(DefaultClanFinanceModel), "CalculateClanIncome")]
#endif
    internal class CEPatchDefaultClanFinanceModel
    {
        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.Instance != null && CESettings.Instance.ProstitutionControl;

        [HarmonyPostfix]
        private static void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
            if (clan.IsEliminated) return;

            if (Clan.PlayerClan != clan) return;
            int num = 0;
            int num2 = 0;

            foreach (CEBrothel brothel in CEBrothelBehavior.GetPlayerBrothels())
            {
                if (brothel.IsRunning)
                {
                    int num3 = (int)(Math.Max(0, brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction());
                    num += num3;
                    if (applyWithdrawals && num3 > 0) brothel.ChangeGold(-num3);

                    if (num3 > 0 && Hero.MainHero.Clan.Leader.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) && applyWithdrawals) num2++;
                }
            }

            goldChange.Add(num, new TextObject("{=CEBROTHEL1001}Brothel income."));
            if (Hero.MainHero.Clan.Leader.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) && applyWithdrawals) Hero.MainHero.Clan.AddRenown(num2 * DefaultPerks.Trade.ArtisanCommunity.PrimaryBonus);
        }
    }
}