using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

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
                    if (CESettings.Instance.SexualContent) { GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_defeated_and_taken_prisoner_sexual" : "CE_defeated_and_taken_prisoner_sexual_male"); }
                    else { GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_defeated_and_taken_prisoner" : "CE_defeated_and_taken_prisoner_male"); }
                    break;
                case "taken_prisoner":
                    if (CESettings.Instance.SexualContent) { GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_taken_prisoner_sexual" : "CE_taken_prisoner_sexual_male"); }
                    else { GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_taken_prisoner" : "CE_taken_prisoner_male"); }
                    break;
                default:
                    break;
            }

        }
    }
}
