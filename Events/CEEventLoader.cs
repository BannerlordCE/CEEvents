using CaptivityEvents.Custom;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

namespace CaptivityEvents.Events
{
    internal class CEEventLoader
    {

        // Waiting Menus
        public static string CEWaitingList() => new WaitingList().CEWaitingList();

        // CEProgressMode
        private static GameMenu.MenuAndOptionType CEProgressMode(int state)
        {
            switch (state)
            {
                case 1:
                    return GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption;
                case 2:
                    return GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption;
                default:
                    return GameMenu.MenuAndOptionType.WaitMenuShowOnlyProgressOption;
            }
        }

        // Event Loaders
        public static void CELoadRandomEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            CEVariablesLoader variablesLoader = new CEVariablesLoader();
            RandomMenuCallBackDelegate rcb = new RandomMenuCallBackDelegate(listedEvent, null, eventList);

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
                    GameMenu.MenuFlags.none,
                    "CEEVENTS");
            }
            else
            {
                gameStarter.AddGameMenu(
                    listedEvent.Name,
                    listedEvent.Text,
                    rcb.RandomEventGameMenu,
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    GameMenu.MenuFlags.none,
                    "CEEVENTS");
            }

            if (listedEvent.Options == null) return; // Leave if no Options

            List<Option> sorted = listedEvent.Options.OrderBy(item => variablesLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (Option op in sorted)
            {
                RandomMenuCallBackDelegate mcb = new RandomMenuCallBackDelegate(listedEvent, op, eventList);
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
            CaptiveMenuCallBackDelegate cb = new CaptiveMenuCallBackDelegate(listedEvent);

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
                    GameMenu.MenuFlags.none,
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
                    GameMenu.MenuFlags.none,
                    "CEEVENTS");
            }
            else
            {
                gameStarter.AddGameMenu(
                    listedEvent.Name,
                    listedEvent.Text,
                    cb.CaptiveEventGameMenu,
                    TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                    GameMenu.MenuFlags.none,
                    "CEEVENTS");
            }

            if (listedEvent.Options == null) return; // Leave if no Options

            List<Option> sorted = listedEvent.Options.OrderBy(item => variablesLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (Option op in sorted)
            {
                CaptiveMenuCallBackDelegate mcb = new CaptiveMenuCallBackDelegate(listedEvent, op, eventList);
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
            CaptorMenuCallBackDelegate cb = new CaptorMenuCallBackDelegate(listedEvent);

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
                    GameMenu.MenuFlags.none,
                    "CEEVENTS");
            }
            else
            {
                gameStarter.AddGameMenu(
                listedEvent.Name,
                listedEvent.Text,
                cb.CaptorEventWaitGameMenu,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.None,
                GameMenu.MenuFlags.none,
                "CEEVENTS");
            }

            List<Option> sorted = listedEvent.Options.OrderBy(item => variablesLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (Option op in sorted)
            {
                CaptorMenuCallBackDelegate mcb = new CaptorMenuCallBackDelegate(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(
                    listedEvent.Name,
                    listedEvent.Name + op.Order,
                    op.OptionText,
                    mcb.CaptorEventOptionGameMenu,
                    mcb.CaptorConsequenceWaitGameMenu,
                    false,
                    variablesLoader.GetIntFromXML(op.Order),
                    false);
            }
        }
    }
}