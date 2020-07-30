using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(PlayerCaptivity))]
    internal class CEPatchPlayerCaptivity
    {

        [HarmonyPatch("StartCaptivity")]
        [HarmonyPostfix]
        private static void StartCaptivity(PartyBase captorParty)
        {
            // 1.4.3 Fix
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = false;
            }
        }
    }
}
