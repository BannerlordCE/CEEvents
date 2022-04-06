using CaptivityEvents.Custom;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

namespace CaptivityEvents.Events
{
    internal class CEEventLoader
    {
        /// <summary>
        /// Checks which type of progress mode to display.
        /// </summary>
        /// <param name="state">Progress Mode</param>
        /// <returns>MenuAndOptionType</returns>
        private static GameMenu.MenuAndOptionType CEProgressMode(int state)
        {
            return state switch
            {
                1 => GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                2 => GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption,
                _ => GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption,
            };
        }

        #region Event Loader
        public static void CELoadRandomEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            CEVariablesLoader variablesLoader = new CEVariablesLoader();
            MenuCallBackDelegateRandom rcb = new MenuCallBackDelegateRandom(listedEvent, eventList);

            if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.ProgressMenu))
            {
                gameStarter.AddWaitGameMenu(listedEvent.Name,
                    listedEvent.Text,
                    rcb.RandomProgressInitWaitGameMenu,
                    rcb.RandomProgressConditionWaitGameMenu,
                    rcb.RandomProgressConsequenceWaitGameMenu,
                    rcb.RandomProgressTickWaitGameMenu,
                    CEProgressMode(variablesLoader.GetIntFromXML(listedEvent.ProgressEvent.DisplayProgressMode)),
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    variablesLoader.GetFloatFromXML(listedEvent.ProgressEvent.TimeToTake),
                    GameMenu.MenuFlags.None,
                    "CEEVENTS");
            }
            else
            {
                gameStarter.AddGameMenu(
                    listedEvent.Name,
                    listedEvent.Text,
                    rcb.RandomEventGameMenu,
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    GameMenu.MenuFlags.None,
                    "CEEVENTS");
            }

            if (listedEvent.Options == null) return; // Leave if no Options

            List<Option> sorted = listedEvent.Options.OrderBy(item => variablesLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (Option op in sorted)
            {
                MenuCallBackDelegateRandom mcb = new MenuCallBackDelegateRandom(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(
                    listedEvent.Name,
                    listedEvent.Name + op.Order,
                    op.OptionText,
                    mcb.RandomEventConditionMenuOption,
                    mcb.RandomEventConsequenceMenuOption,
                    false, variablesLoader.GetIntFromXML(op.Order));
            }
        }
        public static void CELoadCaptiveEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            CEVariablesLoader variablesLoader = new CEVariablesLoader();
            MenuCallBackDelegateCaptive cb = new MenuCallBackDelegateCaptive(listedEvent, eventList);

            if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.ProgressMenu))
            {
                gameStarter.AddWaitGameMenu(listedEvent.Name,
                    listedEvent.Text,
                    cb.CaptiveProgressInitWaitGameMenu,
                    cb.CaptiveProgressConditionWaitGameMenu,
                    cb.CaptiveProgressConsequenceWaitGameMenu,
                    cb.CaptiveProgressTickWaitGameMenu,
                    CEProgressMode(variablesLoader.GetIntFromXML(listedEvent.ProgressEvent.DisplayProgressMode)),
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    variablesLoader.GetFloatFromXML(listedEvent.ProgressEvent.TimeToTake),
                    GameMenu.MenuFlags.None,
                    "CEEVENTS");
            }
            else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu))
            {
                gameStarter.AddWaitGameMenu(
                    listedEvent.Name,
                    listedEvent.Text,
                    cb.CaptiveInitWaitGameMenu,
                    cb.CaptiveConditionWaitGameMenu,
                    cb.CaptiveConsequenceWaitGameMenu,
                    cb.CaptiveTickWaitGameMenu,
                    GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption,
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    0,
                    GameMenu.MenuFlags.None,
                    "CEEVENTS");
            }
            else
            {
                gameStarter.AddGameMenu(
                    listedEvent.Name,
                    listedEvent.Text,
                    cb.CaptiveEventGameMenu,
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    GameMenu.MenuFlags.None,
                    "CEEVENTS");
            }

            if (listedEvent.Options == null) return; // Leave if no Options

            List<Option> sorted = listedEvent.Options.OrderBy(item => variablesLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (Option op in sorted)
            {
                MenuCallBackDelegateCaptive mcb = new MenuCallBackDelegateCaptive(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(
                    listedEvent.Name,
                    listedEvent.Name + op.Order,
                    op.OptionText,
                    mcb.CaptiveEventOptionGameMenu,
                    mcb.CaptiveEventOptionConsequenceGameMenu,
                    false,
                    variablesLoader.GetIntFromXML(op.Order));
            }
        }
        public static void CELoadCaptorEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            CEVariablesLoader variablesLoader = new CEVariablesLoader();
            MenuCallBackDelegateCaptor cb = new MenuCallBackDelegateCaptor(listedEvent, eventList);

            if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.ProgressMenu))
            {
                gameStarter.AddWaitGameMenu(listedEvent.Name,
                    listedEvent.Text,
                    cb.CaptorProgressInitWaitGameMenu,
                    cb.CaptorProgressConditionWaitGameMenu,
                    cb.CaptorProgressConsequenceWaitGameMenu,
                    cb.CaptorProgressTickWaitGameMenu,
                    CEProgressMode(variablesLoader.GetIntFromXML(listedEvent.ProgressEvent.DisplayProgressMode)),
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    variablesLoader.GetFloatFromXML(listedEvent.ProgressEvent.TimeToTake),
                    GameMenu.MenuFlags.None,
                    "CEEVENTS");
            }
            else
            {
                gameStarter.AddGameMenu(
                listedEvent.Name,
                listedEvent.Text,
                cb.CaptorEventWaitGameMenu,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                GameMenu.MenuFlags.None,
                "CEEVENTS");
            }

            List<Option> sorted = listedEvent.Options.OrderBy(item => variablesLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (Option op in sorted)
            {
                MenuCallBackDelegateCaptor mcb = new MenuCallBackDelegateCaptor(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(
                    listedEvent.Name,
                    listedEvent.Name + op.Order,
                    op.OptionText,
                    mcb.CaptorEventOptionGameMenu,
                    mcb.CaptorConsequenceGameMenu,
                    false,
                    variablesLoader.GetIntFromXML(op.Order),
                    false);
            }
        }
        #endregion

    }
}