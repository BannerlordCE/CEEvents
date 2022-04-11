#define V172
using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
#if V171
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
#else
using TaleWorlds.CampaignSystem.CampaignBehaviors;
#endif

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
