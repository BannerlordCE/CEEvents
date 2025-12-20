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
            if (CEPersistence.CurrentBattleState == CEPersistence.BattleState.AfterBattle)
            {
                CEPersistence.PlayerSurrendered = true;
            }
        }

        [HarmonyPatch("OnPlayerDiedInMission")]
        [HarmonyPostfix]
        private static void OnPlayerDiedInMission()
        {
            if (CEPersistence.CurrentBattleState == CEPersistence.BattleState.AfterBattle)
            {
                CEPersistence.PlayerDied = true;
            }
        }
    }
}
