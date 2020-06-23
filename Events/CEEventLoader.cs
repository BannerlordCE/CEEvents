using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;

namespace CaptivityEvents.Events
{
    internal class CEEventLoader
    {
        // Waiting Menus
        public static string CEWaitingList()
        {
            return new WaitingList().CEWaitingList();
        }

        // Event Loaders
        public static void CELoadRandomEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            var varLoader = new VariablesLoader();

            gameStarter.AddGameMenu(listedEvent.Name, listedEvent.Text, new RandomMenuCallBackDelegate(listedEvent).RandomEventGameMenu);

            if (listedEvent.Options == null) return; // Leave if no Options

            var sorted = listedEvent.Options.OrderBy(item => varLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (var op in sorted)
            {
                var mcb = new RandomMenuCallBackDelegate(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(listedEvent.Name, listedEvent.Name + op.Order, op.OptionText, mcb.RandomEventConditionMenuOption, mcb.RandomEventConsequenceMenuOption, false, varLoader.GetIntFromXML(op.Order));
            }
        }

        public static void CELoadCaptiveEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            var varLoader = new VariablesLoader();
            var cb = new CaptiveMenuCallBackDelegate(listedEvent);

            if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu)) gameStarter.AddWaitGameMenu(listedEvent.Name, listedEvent.Text, cb.CaptiveEventWaitGameMenu, cb.CaptiveConditionWaitGameMenu, cb.CaptiveConsequenceWaitGameMenu, cb.CaptiveTickWaitGameMenu, GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption);
            else gameStarter.AddGameMenu(listedEvent.Name, listedEvent.Text, cb.CaptiveEventGameMenu);

            if (listedEvent.Options == null) return; // Leave if no Options

            var sorted = listedEvent.Options.OrderBy(item => varLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (var op in sorted)
            {
                var mcb = new CaptiveMenuCallBackDelegate(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(listedEvent.Name, listedEvent.Name + op.Order, op.OptionText, mcb.CaptiveEventOptionGameMenu, mcb.CaptiveEventOptionConsequenceGameMenu, false, varLoader.GetIntFromXML(op.Order));
            }
        }

        public static void CELoadCaptorEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            var varLoader = new VariablesLoader();

            gameStarter.AddGameMenu(listedEvent.Name, listedEvent.Text, new CaptorMenuCallBackDelegate(listedEvent).CaptorEventWaitGameMenu);

            var sorted = listedEvent.Options.OrderBy(item => varLoader.GetIntFromXML(item.Order)).ToList(); // Sort Options

            foreach (var op in sorted)
            {
                var mcb = new CaptorMenuCallBackDelegate(listedEvent, op, eventList);
                gameStarter.AddGameMenuOption(listedEvent.Name, listedEvent.Name + op.Order, op.OptionText, mcb.CaptorEventOptionGameMenu, mcb.CaptorConsequenceWaitGameMenu, false, varLoader.GetIntFromXML(op.Order));
            }
        }
    }
}