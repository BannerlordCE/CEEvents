using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(GameMenu), "ActivateGameMenu")]
    class CEPatchGameMenu
    {

        [HarmonyPostfix]
        private static void ActivateGameMenu(string menuId)
        {

            switch (menuId)
            {
                case "defeated_and_taken_prisoner":
                    if (CESettings.Instance.SexualContent)
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_defeated_and_taken_prisoner_sexual" : "CE_defeated_and_taken_prisoner_sexual_male");
                    else
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_defeated_and_taken_prisoner" : "CE_defeated_and_taken_prisoner_male");

                    if (Game.Current.GameStateManager.ActiveState is MapState ms1)
                        ms1.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale ? "wait_prisoner_female" : "wait_prisoner_male");
                    break;
                case "taken_prisoner":
                    if (CESettings.Instance.SexualContent)
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_taken_prisoner_sexual" : "CE_taken_prisoner_sexual_male");
                    else
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_taken_prisoner" : "CE_taken_prisoner_male");

                    if (Game.Current.GameStateManager.ActiveState is MapState ms2)
                        ms2.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale ? "wait_prisoner_female" : "wait_prisoner_male");
                    break;
                default:
                    break;
            }

        }
    }
}
