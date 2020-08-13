using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Events;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(GameMenu))]
    internal class CEPatchGameMenu
    {

        [HarmonyPatch("ActivateGameMenu")]
        [HarmonyPostfix]
        private static void ActivateGameMenu(string menuId)
        {
            // 1.4.3 Doesn't call this (wait for hotfix)
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
                case "menu_captivity_castle_taken_prisoner":
                    if (CESettings.Instance.SexualContent)
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_menu_captivity_castle_taken_prisoner_sexual" : "CE_menu_captivity_castle_taken_prisoner_sexual_male");
                    else
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_menu_captivity_castle_taken_prisoner" : "CE_menu_captivity_castle_taken_prisoner_male");

                    if (Game.Current.GameStateManager.ActiveState is MapState ms3)
                        ms3.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale ? "wait_prisoner_female" : "wait_prisoner_male");
                    break;
                case "menu_captivity_end_by_party_removed":
                case "menu_captivity_end_by_ally_party_saved":
                case "menu_captivity_end_no_more_enemies":
                case "menu_escape_captivity_during_battle":
                    CECampaignBehavior.ExtraProps.Owner = null;
                    new Dynamics().VictimSlaveryModifier(0, Hero.MainHero, true);
                    new Dynamics().VictimProstitutionModifier(0, Hero.MainHero, true);
                    break;
                case "menu_captivity_transfer_to_town":
                    break;
                default:
                    break;
            }
        }

        [HarmonyPatch("SwitchToMenu")]
        [HarmonyPostfix]
        private static void SwitchToMenu(string menuId)
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
                case "menu_captivity_castle_taken_prisoner":
                    if (CESettings.Instance.SexualContent)
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_menu_captivity_castle_taken_prisoner_sexual" : "CE_menu_captivity_castle_taken_prisoner_sexual_male");
                    else
                        GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_menu_captivity_castle_taken_prisoner" : "CE_menu_captivity_castle_taken_prisoner_male");

                    if (Game.Current.GameStateManager.ActiveState is MapState ms3)
                        ms3.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale ? "wait_prisoner_female" : "wait_prisoner_male");
                    break;
                case "menu_captivity_end_by_party_removed":
                case "menu_captivity_end_by_ally_party_saved":
                case "menu_captivity_end_no_more_enemies":
                case "menu_escape_captivity_during_battle":
                    CECampaignBehavior.ExtraProps.Owner = null;
                    new Dynamics().VictimSlaveryModifier(0, Hero.MainHero, true);
                    new Dynamics().VictimProstitutionModifier(0, Hero.MainHero, true);
                    break;
                case "menu_captivity_transfer_to_town":
                    break;
                default:
                    break;
            }
        }
    }
}
