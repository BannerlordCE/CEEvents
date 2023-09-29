#define V115

using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;

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
                PlayerEncounter.LeaveEncounter = true;
                CEDelayedEvent delayedEvent = new(PlayerEncounter.PlayerSurrender ? "taken_prisoner" : "defeated_and_taken_prisoner");
                CEHelper.AddDelayedEvent(delayedEvent);
            }
        }
    }
}