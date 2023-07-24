using HarmonyLib;
using SandBox.CampaignBehaviors;

namespace CaptivityEvents.Patches
{

    [HarmonyPatch(typeof(AlleyCampaignBehavior))]
    internal class CEPatchAlleyCampaignBehavior
    {
        [HarmonyPatch("OnPlayerRetreatedFromMission")]
        [HarmonyPostfix]
        private static void OnPlayerRetreatedFromMission()
        {
            if (CEPersistence.battleState == CEPersistence.BattleState.AfterBattle)
            {
                CEPersistence.playerSurrendered = true;
            }
        }

        [HarmonyPatch("OnPlayerDiedInMission")]
        [HarmonyPostfix]
        private static void OnPlayerDiedInMission()
        {
            if (CEPersistence.battleState == CEPersistence.BattleState.AfterBattle)
            {
                CEPersistence.playerDied = true;
            }
        }
    }
}
