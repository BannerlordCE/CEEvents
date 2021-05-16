using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior))]
    internal class CEPatchPregnancyCampaignBehavior
    {
        [HarmonyPatch("ChildConceived")]
        [HarmonyPrefix]
        private static bool ChildConceived(Hero mother)
        {
            return CEHelper.spouseOne == null && CEHelper.spouseTwo == null;
        }
    }
}
