using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Events;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace CaptivityEvents.Helper
{
    internal class CEConsole
    {
        internal CEBrothelBehavior BrothelBehavior { get; set; }


        public CEConsole(CEBrothelBehavior brothelBehavior)
        {
            BrothelBehavior = brothelBehavior;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("force_fire_event", "captivity")]
        public string ForceFireEvent(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckParameters(strings, 0) && CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.force_fire_event [EventName] [CaptiveName]\".";
                var flag = false;

                var eventname = "";
                string heroname = null;

                if (CampaignCheats.CheckParameters(strings, 1))
                {
                    eventname = strings[0];

                    if (eventname.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }
                else if (CampaignCheats.CheckParameters(strings, 2))
                {
                    eventname = strings[0];
                    heroname = strings[1];

                    if (eventname.IsStringNoneOrEmpty() || heroname.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }

                if (!flag) return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";
                string result;

                if (PlayerCaptivity.IsCaptive)
                {
                    result = CEEventManager.FireSpecificEvent(eventname, true);

                    switch (result)
                    {
                        case "$FAILEDTOFIND":
                            return "Failed to load event list.";

                        case "$EVENTNOTFOUND":
                            return "Event not found.";

                        case "$EVENTCONDITIONSNOTMET":
                            return "Event conditions are not met.";

                        default:
                            if (result.StartsWith("$")) return result.Substring(1);

                            if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptive)
                            {
                                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                if (!mapStateCaptive.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                GameMenu.SwitchToMenu(result);

                                return "Successfully force launched event.";
                            }
                            else
                            {
                                return "Failed to launch event, incorrect game state.";
                            }
                    }
                }

                result = CEEventManager.FireSpecificEventRandom(eventname, true);

                switch (result)
                {
                    case "$FAILEDTOFIND":
                        return "Failed to load event list.";

                    case "$EVENTNOTFOUND":
                    case "$EVENTCONDITIONSNOTMET":
                        if (PartyBase.MainParty.NumberOfPrisoners > 0)
                        {
                            result = CEEventManager.FireSpecificEventPartyLeader(eventname, true, heroname);

                            switch (result)
                            {
                                case "$FAILEDTOFIND":
                                    return "Failed to load event list.";

                                case "$FAILTOFINDHERO":
                                    return "Failed to find specified captive in party : " + heroname;

                                case "$EVENTNOTFOUND":
                                    return "Event not found.";

                                case "$EVENTCONDITIONSNOTMET":
                                    return "No captives meet the event conditions.";

                                default:
                                    if (result.StartsWith("$")) return result.Substring(1);

                                    if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptor)
                                    {
                                        Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                        if (!mapStateCaptor.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                        GameMenu.SwitchToMenu(result);

                                        return "Successfully force launched event.";
                                    }
                                    else
                                    {
                                        return "Failed to launch event, incorrect game state.";
                                    }
                            }
                        }
                        else
                        {
                            return "Please add more prisoners to your party.\n\"Format is \"campaign.add_prisoner [PositiveNumber] [TroopName]\".";
                        }
                    default:
                        if (result.StartsWith("$")) return result.Substring(1);

                        if (Game.Current.GameStateManager.ActiveState is MapState mapStateRandom)
                        {
                            Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                            if (!mapStateRandom.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                            GameMenu.SwitchToMenu(result);

                            return "Successfully force launched event.";
                        }
                        else
                        {
                            return "Failed to launch event, incorrect game state.";
                        }
                }

                return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("fire_event", "captivity")]
        public string FireEvent(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckParameters(strings, 0) && CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.fire_ceevent [EventName] [CaptiveName]\".";

                var flag = false;

                var eventname = "";
                string heroname = null;

                if (CampaignCheats.CheckParameters(strings, 1))
                {
                    eventname = strings[0];

                    if (eventname.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }
                else if (CampaignCheats.CheckParameters(strings, 2))
                {
                    eventname = strings[0];
                    heroname = strings[1];

                    if (eventname.IsStringNoneOrEmpty() || heroname.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }

                if (!flag) return "Wrong input.\nFormat is \"captivity.fire_ceevent [EventName] [CaptiveName]\".";
                string result;

                if (PlayerCaptivity.IsCaptive)
                {
                    result = CEEventManager.FireSpecificEvent(eventname);

                    switch (result)
                    {
                        case "$FAILEDTOFIND":
                            return "Failed to load event list.";

                        case "$EVENTNOTFOUND":
                            return "Event not found.";

                        case "$EVENTCONDITIONSNOTMET":
                            return "Event conditions are not met.";

                        default:
                            if (result.StartsWith("$")) return result.Substring(1);

                            if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptive)
                            {
                                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                                if (!mapStateCaptive.AtMenu)
                                {
                                    GameMenu.ActivateGameMenu("prisoner_wait");
                                }
                                else
                                {
                                    CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = mapStateCaptive.GameMenuId;
                                    CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = mapStateCaptive.MenuContext.CurrentBackgroundMeshName;
                                }

                                GameMenu.SwitchToMenu(result);

                                return "Successfully launched event.";
                            }
                            else
                            {
                                return "Failed to launch event, incorrect game state.";
                            }
                    }
                }

                result = CEEventManager.FireSpecificEventRandom(eventname);

                switch (result)
                {
                    case "$FAILEDTOFIND":
                        return "Failed to load event list.";

                    case "$EVENTNOTFOUND":
                    case "$EVENTCONDITIONSNOTMET":
                        if (PartyBase.MainParty.NumberOfPrisoners > 0)
                        {
                            result = CEEventManager.FireSpecificEventPartyLeader(eventname, false, heroname);

                            switch (result)
                            {
                                case "$FAILEDTOFIND":
                                    return "Failed to load event list.";

                                case "$FAILTOFINDHERO":
                                    return "Failed to find specified captive in party : " + heroname;

                                case "$EVENTNOTFOUND":
                                    return "Event not found.";

                                case "$EVENTCONDITIONSNOTMET":
                                    return "No captives meet the event conditions.";

                                default:
                                    if (result.StartsWith("$")) return result.Substring(1);

                                    if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptor)
                                    {
                                        Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                        if (!mapStateCaptor.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                        GameMenu.SwitchToMenu(result);

                                        return "Successfully launched event.";
                                    }
                                    else
                                    {
                                        return "Failed to launch event, incorrect game state.";
                                    }
                            }
                        }
                        else
                        {
                            return "Please add more prisoners to your party.\n\"Format is \"campaign.add_prisoner [PositiveNumber] [TroopName]\".";
                        }
                    default:
                        if (result.StartsWith("$")) return result.Substring(1);

                        if (Game.Current.GameStateManager.ActiveState is MapState mapStateRandom)
                        {
                            Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                            if (!mapStateRandom.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                            GameMenu.SwitchToMenu(result);

                            return "Successfully launched event.";
                        }
                        else
                        {
                            return "Failed to launch event, incorrect game state.";
                        }
                }

                return "Wrong input.\nFormat is \"captivity.fire_ceevent [EventName] [CaptiveName]\".";
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("list_events", "captivity")]
        public string ListEvents(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.list_events [SEARCH_TERM]\".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                if (CESubModule.CEEvents == null || CESubModule.CEEvents.Count <= 0) return "Failed to load event list.";
                var text = "";
                var searchActive = !searchTerm.IsStringNoneOrEmpty();

                if (searchActive) searchTerm = searchTerm.ToLower();

                foreach (var ceEvent in CESubModule.CEEvents)
                    if (searchActive)
                    {
                        if (ceEvent.Name.ToLower().IndexOf(searchTerm) != -1) text = text + ceEvent.Name + "\n";
                    }
                    else
                    {
                        text = text + ceEvent.Name + "\n";
                    }

                return text;
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("current_status", "captivity")]
        public string CurrentStatus(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.current_status [SEARCH_HERO]\".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                var hero = searchTerm.IsStringNoneOrEmpty()
                    ? Hero.MainHero
                    : Campaign.Current.Heroes.FirstOrDefault(heroToFind => { return heroToFind.Name.ToString() == searchTerm; });

                return hero == null
                    ? "Hero not found."
                    : CEEventChecker.CheckFlags(hero.CharacterObject, PlayerCaptivity.CaptorParty);
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reset_status", "captivity")]
        public string ResetStatus(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.reset_status [SEARCH_HERO]\".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                var hero = searchTerm.IsStringNoneOrEmpty()
                    ? Hero.MainHero
                    : Campaign.Current.Heroes.FirstOrDefault(heroToFind => heroToFind.Name.ToString() == searchTerm);

                if (hero == null) return "Hero not found.";

                try
                {
                    CEEventLoader.VictimProstitutionModifier(0, hero, true);
                    CEEventLoader.VictimProstitutionModifier(0, hero, false, false);
                    CEEventLoader.VictimSlaveryModifier(0, hero, true);
                    CEEventLoader.VictimSlaveryModifier(0, hero, false, false);
                    CECampaignBehavior.ExtraProps.ResetVariables();

                    return "Successfully reset status";
                }
                catch (Exception)
                {
                    return "Failed";
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("clear_pregnancies", "captivity")]
        public string ClearPregnancies(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.clear_pregnancies \".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                try
                {
                    var successful = CECampaignBehavior.ClearPregnancyList();

                    return successful
                        ? "Successfully cleared pregnancies of Captivity Events"
                        : "Failed to Clear";
                }
                catch (Exception)
                {
                    return "Failed";
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("clean_save", "captivity")]
        public string CleanSave(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.clean_save \".";

                try
                {
                    var successful = CECampaignBehavior.ClearPregnancyList();
                    BrothelBehavior.Session.CleanList();
                    ResetStatus(new List<string>());

                    return successful
                        ? "Successfully cleaned save of captivity events data. Save the game now."
                        : "Failed to Clean";
                }
                catch (Exception)
                {
                    return "Failed";
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("play_sound", "captivity")]
        public string PlaySound(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckHelp(strings) && CampaignCheats.CheckParameters(strings, 1)) return "Format is \"captivity.play_sound [SOUND_ID]\".";

                var searchTerm = strings[0];

                var id = 0;

                try
                {
                    if (!searchTerm.IsStringNoneOrEmpty()) id = SoundEvent.GetEventIdFromString(searchTerm);

                    if (id == 0) return "Sound not found.";
                }
                catch
                {
                    return "Sound not found.";
                }

                try
                {
                    if (Game.Current.GameStateManager.ActiveState is MissionState)
                    {
                        Mission.Current.MakeSound(id, Agent.Main.Frame.origin, true, false, -1, -1);

                        return string.Empty;
                    }

                    SoundEvent.PlaySound2D(id);

                    return string.Empty;
                }
                catch (Exception)
                {
                    return "Failed to play sound";
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }
    }
}