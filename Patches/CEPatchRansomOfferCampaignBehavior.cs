#define V120

using System.Linq;
using CaptivityEvents.Config;
using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(RansomOfferCampaignBehavior))]
    internal class CEPatchRansomOfferCampaignBehavior
    {
        [HarmonyPatch("ConsiderRansomPrisoner")]
        [HarmonyPrefix]
        static bool ConsiderRansomPrisoner(Hero hero)
        {
            if (hero == null)
            {
                return false; // skips the original and its expensive calculations
            }
            else if (hero.Clan == null) { return false; }
            else if (Clan.BanditFactions.Contains(hero.Clan) || hero.PartyBelongedToAsPrisoner == TaleWorlds.CampaignSystem.Party.PartyBase.MainParty) 
            { return false; } //always skip Bandits and Main Hero's prisoners
            else if ((CESettings.Instance?.PrisonerEscapeBehavior ?? true) && hero.CurrentSettlement?.OwnerClan == TaleWorlds.CampaignSystem.Party.PartyBase.MainParty.MobileParty.ActualClan)
            { return false; } //CE override Escape Behavior skips any prisoner in your Clan's prisons
            else if (CESettings.Instance?.PrisonerHeroEscapeSettlement ?? true) 
            { return false; } //CE override Prisoner Settlement Behavior makes dungeons escape-proof
            return true;
        }
    }
}
