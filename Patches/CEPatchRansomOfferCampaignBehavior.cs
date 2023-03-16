#define V102

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
            else if (hero.Clan == null) {return false;}
            return true; // make sure you only skip if really necessary
        }

    }
}
