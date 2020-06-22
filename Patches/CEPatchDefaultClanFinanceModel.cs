using System;
using CaptivityEvents.Brothel;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(DefaultClanFinanceModel), "CalculateClanIncome")]
    internal class CEPatchDefaultClanFinanceModel
    {
        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            return CESettings.Instance != null && CESettings.Instance.ProstitutionControl;
        }

        [HarmonyPostfix]
        private static void CalculateClanIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
            if (clan.IsEliminated) return;

            if (Clan.PlayerClan != clan) return;
            var num = 0;
            var num2 = 0;

            foreach (var brothel in CEBrothelBehavior.GetPlayerBrothels())
                if (brothel.IsRunning)
                {
                    var num3 = (int) (Math.Max(0, brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction());
                    num += num3;
                    if (applyWithdrawals && num3 > 0) brothel.ChangeGold(-num3);

                    if (num3 > 0 && Hero.MainHero.Clan.Leader.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) && applyWithdrawals) num2++;
                }

            goldChange.Add(num, new TextObject("{=CEBROTHEL1001}Brothel income."));
            if (Hero.MainHero.Clan.Leader.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) && applyWithdrawals) Hero.MainHero.Clan.AddRenown(num2 * DefaultPerks.Trade.ArtisanCommunity.PrimaryBonus);
        }
    }
}