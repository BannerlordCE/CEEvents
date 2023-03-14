#define V102

using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;


namespace CaptivityEvents.Events
{
    internal class CEEventManager
    {
        // Flags and Conditions
        public static void PrintDebugInGameTextMessage(string v)
        {
            TextObject textObject = new(v);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
        }

        #region Return Specifics Events

        public static string FireSpecificEvent(string specificEvent, bool force = false)
        {
            List<string> eventNames = new();

            string flag = "$FAILEDTOFIND";

            if (CEPersistence.CEEventList == null || CEPersistence.CEEventList.Count <= 0) return flag;
            specificEvent = specificEvent.ToLower();

            CEEvent foundevent = CEPersistence.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == specificEvent);

            if (foundevent != null)
            {
                if (!force && foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                {
                    string result = new CEEventChecker(foundevent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);

                    if (result == null) flag = foundevent.Name;
                    else flag = "$" + result;
                }
                else if (force)
                {
                    flag = foundevent.Name;
                }
                else
                {
                    flag = "$EVENTCONDITIONSNOTMET";
                }
            }
            else
            {
                flag = "$EVENTNOTFOUND";
            }

            return flag;
        }

        public static string FireSpecificEventRandom(string specificEvent, out CEEvent ceEvent, bool force = false)
        {
            List<string> eventNames = new();

            string flag = "$FAILEDTOFIND";
            ceEvent = null;

            if (CEPersistence.CEEventList == null || CEPersistence.CEEventList.Count <= 0) return flag;
            specificEvent = specificEvent.ToLower();
            CEEvent foundevent = CEPersistence.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == specificEvent);

            if (foundevent != null)
            {
                if (foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                {
                    string result = new CEEventChecker(foundevent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);

                    if (force || result == null)
                    {
                        flag = foundevent.Name;
                        ceEvent = foundevent;
                    }
                    else
                    {
                        flag = "$" + result;
                    }
                }
                else
                {
                    flag = "$EVENTCONDITIONSNOTMET";
                }
            }
            else
            {
                flag = "$EVENTNOTFOUND";
            }

            return flag;
        }

        public static string FireSpecificEventPartyLeader(string specificEvent, out CEEvent ceEvent, bool force = false, string heroname = null)
        {
            List<string> eventNames = new();

            string flag = "$FAILEDTOFIND";
            ceEvent = null;

            if (CEPersistence.CEEventList == null || CEPersistence.CEEventList.Count <= 0) return flag;
            specificEvent = specificEvent.ToLower();
            CEEvent foundevent = CEPersistence.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == specificEvent);

            if (foundevent != null)
            {
                if (heroname == null)
                {
                    foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.PrisonRoster.GetTroopRoster())
                    {
                        if (troopRosterElement.Character != null)
                        {
                            if (foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                            {
                                string result = new CEEventChecker(foundevent).FlagsDoMatchEventConditions(troopRosterElement.Character, PartyBase.MainParty);

                                if (force || result == null)
                                {
                                    foundevent.Captive = troopRosterElement.Character;
                                    ceEvent = foundevent;
                                    return foundevent.Name;
                                }

                                flag = "$" + result;
                            }
                        }
                    }
                }
                else
                {
                    TroopRosterElement specificTroopRosterElement = PartyBase.MainParty.PrisonRoster.GetTroopRoster().FirstOrDefault(troopRosterElement => troopRosterElement.Character != null && troopRosterElement.Character.Name.ToString() == heroname);

                    if (specificTroopRosterElement.Character == null) return "$FAILTOFINDHERO";

                    CharacterObject specificCaptive = specificTroopRosterElement.Character;

                    if (!foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor)) return flag;
                    string result = new CEEventChecker(foundevent).FlagsDoMatchEventConditions(specificCaptive, PartyBase.MainParty);

                    if (force || result == null)
                    {
                        foundevent.Captive = specificCaptive;

                        return foundevent.Name;
                    }

                    flag = "$" + result;
                }
            }
            else
            {
                flag = "$EVENTNOTFOUND";
            }

            return flag;
        }

        #endregion Return Specifics Events

        #region Return Random Events

        public static CEEvent ReturnWeightedChoiceOfEventsRandom()
        {
            List<CEEvent> events = new();
            int CurrentOrder = 0;

            if (CEPersistence.CECallableEvents != null && CEPersistence.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CEPersistence.CECallableEvents.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CEPersistence.CECallableEvents)
                {
                    if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                    {
                        string result = new CEEventChecker(listEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);

                        if (result == null)
                        {
                            int weightedChance = 10;

                            if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.IgnoreAllOther))
                            {
                                CECustomHandler.LogToFile("IgnoreAllOther detected - autofire " + listEvent.Name);
                                return listEvent;
                            }

                            int OrderToCall = 0;
                            if (!string.IsNullOrEmpty(listEvent.OrderToCall))
                            {
                                OrderToCall = new CEVariablesLoader().GetIntFromXML(listEvent.OrderToCall);
                            }

                            if (OrderToCall < CurrentOrder)
                            {
                                CECustomHandler.LogToFile("OrderToCall - " + OrderToCall + " was less than CurrentOrder - " + CurrentOrder + " for " + listEvent.Name);
                                continue;
                            }
                            else if (OrderToCall > CurrentOrder)
                            {
                                events.Clear();
                                CurrentOrder = OrderToCall;
                            }

                            if (!string.IsNullOrEmpty(listEvent.WeightedChanceOfOccuring))
                            {
                                weightedChance = new CEVariablesLoader().GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                            }

                            for (int a = weightedChance; a > 0; a--) events.Add(listEvent);
                        }
                        else
                        {
                            CECustomHandler.LogToFile(result);
                        }
                    }
                }

                CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);

                try
                {
                    if (events.Count > 0) return events.GetRandomElement();
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Something is broken?");
                    PrintDebugInGameTextMessage("Something Broken...?");
                }
            }

            CECustomHandler.LogToFile("Number of Filitered events is " + events.Count);

            return null;
        }

        public static CEEvent ReturnWeightedChoiceOfEvents()
        {
            List<CEEvent> events = new();
            int CurrentOrder = 0;

            if (CEPersistence.CECallableEvents != null && CEPersistence.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CEPersistence.CECallableEvents.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CEPersistence.CECallableEvents)
                {
                    if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                    {
                        string result = new CEEventChecker(listEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);

                        if (result == null)
                        {
                            int weightedChance = 10;

                            if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.IgnoreAllOther))
                            {
                                CECustomHandler.LogToFile("IgnoreAllOther detected - autofire " + listEvent.Name);
                                return listEvent;
                            }

                            int OrderToCall = 0;
                            if (!string.IsNullOrEmpty(listEvent.OrderToCall))
                            {
                                OrderToCall = new CEVariablesLoader().GetIntFromXML(listEvent.OrderToCall);
                            }

                            if (OrderToCall < CurrentOrder)
                            {
                                CECustomHandler.LogToFile("OrderToCall - " + OrderToCall + " was less than CurrentOrder - " + CurrentOrder + " for " + listEvent.Name);
                                continue;
                            }
                            else if (OrderToCall > CurrentOrder)
                            {
                                events.Clear();
                                CurrentOrder = OrderToCall;
                            }

                            if (!string.IsNullOrEmpty(listEvent.WeightedChanceOfOccuring))
                            {
                                weightedChance = new CEVariablesLoader().GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                            }

                            for (int a = weightedChance; a > 0; a--) events.Add(listEvent);
                        }
                        else
                        {
                            CECustomHandler.LogToFile(result);
                        }
                    }
                }

                CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);

                try
                {
                    if (events.Count > 0) return events.GetRandomElement();
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Something is broken?");
                    PrintDebugInGameTextMessage("Something Broken...?");
                }
            }

            CECustomHandler.LogToFile("Number of Filitered events is " + events.Count);

            return null;
        }

        public static CEEvent ReturnWeightedChoiceOfEventsPartyLeader(CharacterObject captive)
        {
            List<CEEvent> events = new();

            int CurrentOrder = 0;
            CECustomHandler.LogToFile("Number of Filitered events is " + events.Count);

            if (CEPersistence.CECallableEvents == null || CEPersistence.CECallableEvents.Count <= 0) return null;
            CECustomHandler.LogToFile("Having " + CEPersistence.CECallableEvents.Count + " of events to weight and check conditions on.");

            foreach (CEEvent listEvent in CEPersistence.CECallableEvents)
            {
                if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                {
                    string result = new CEEventChecker(listEvent).FlagsDoMatchEventConditions(captive, PartyBase.MainParty);

                    if (result == null)
                    {
                        int weightedChance = 10;

                        if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.IgnoreAllOther))
                        {
                            CECustomHandler.LogToFile("IgnoreAllOther detected - autofire " + listEvent.Name);
                            return listEvent;
                        }

                        int OrderToCall = 0;
                        if (!string.IsNullOrEmpty(listEvent.OrderToCall))
                        {
                            OrderToCall = new CEVariablesLoader().GetIntFromXML(listEvent.OrderToCall);
                        }

                        if (OrderToCall < CurrentOrder)
                        {
                            CECustomHandler.LogToFile("OrderToCall - " + OrderToCall + " was less than CurrentOrder - " + CurrentOrder + " for " + listEvent.Name);
                            continue;
                        }
                        else if (OrderToCall > CurrentOrder)
                        {
                            events.Clear();
                            CurrentOrder = OrderToCall;
                        }

                        if (!string.IsNullOrEmpty(listEvent.WeightedChanceOfOccuring))
                        {
                            weightedChance = new CEVariablesLoader().GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                        }
                        else
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                        }

                        for (int a = weightedChance; a > 0; a--) events.Add(listEvent);
                    }
                    else
                    {
                        CECustomHandler.LogToFile(result);
                    }
                }
            }

            CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);

            try
            {
                if (events.Count > 0)
                {
                    CEEvent randomEvent = events.GetRandomElement();
                    randomEvent.Captive = captive;
                    return randomEvent;
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("eventNames.Count Broken : " + e);
                PrintDebugInGameTextMessage("eventNames.Count Broken");
            }

            return null;
        }

        #endregion Return Random Events
    }
}