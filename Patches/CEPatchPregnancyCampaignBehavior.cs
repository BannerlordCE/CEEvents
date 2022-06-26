#define V180

using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(PregnancyCampaignBehavior))]
    internal class CEPatchPregnancyCampaignBehavior
    {
        [HarmonyPatch("ChildConceived")]
        [HarmonyPrefix]
        private static bool ChildConceived(Hero mother) => CEHelper.spouseOne == null && CEHelper.spouseTwo == null;
    }
}