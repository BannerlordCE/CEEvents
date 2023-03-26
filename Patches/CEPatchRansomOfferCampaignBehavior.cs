#define V102

using System.Linq;
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
            { return false; } 
            return true; // make sure you only skip if really necessary
        }
    }
}
