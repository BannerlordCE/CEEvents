﻿#define V172
using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
#if V171
#else
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
#endif

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
                CEDelayedEvent delayedEvent = new CEDelayedEvent(PlayerEncounter.PlayerSurrender ? "taken_prisoner" : "defeated_and_taken_prisoner");
                CEHelper.AddDelayedEvent(delayedEvent);
            }
        }
    }
}
