using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    internal class CEEventManager
    {
        // Flags and Conditions
        public static void PrintDebugInGameTextMessage(string v)
        {
            TextObject textObject = new TextObject(v, null);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
        }

        public static string FireSpecificEvent(string specificEvent, bool force = false)
        {
            List<string> eventNames = new List<string>();

            string flag = "$FAILEDTOFIND";
            if (CESubModule.CEEventList != null && CESubModule.CEEventList.Count > 0)
            {
                specificEvent = specificEvent.ToLower();
                CEEvent foundevent = CESubModule.CEEventList.FirstOrDefault((CEEvent ceevent) => ceevent.Name.ToLower() == specificEvent);

                if (foundevent != null)
                {
                    if (!force && foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                    {
                        string result = CEEventChecker.FlagsDoMatchEventConditions(foundevent, CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                        if (result == null)
                        {
                            flag = foundevent.Name;
                        }
                        else
                        {
                            flag = "$" + result;
                        }
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
            }

            return flag;
        }

        public static string FireSpecificEventRandom(string specificEvent, bool force = false)
        {
            List<string> eventNames = new List<string>();

            string flag = "$FAILEDTOFIND";
            if (CESubModule.CEEventList != null && CESubModule.CEEventList.Count > 0)
            {
                specificEvent = specificEvent.ToLower();
                CEEvent foundevent = CESubModule.CEEventList.FirstOrDefault((CEEvent ceevent) => ceevent.Name.ToLower() == specificEvent);

                if (foundevent != null)
                {
                    if (foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                    {
                        string result = CEEventChecker.FlagsDoMatchEventConditions(foundevent, CharacterObject.PlayerCharacter);
                        if (force || result == null)
                        {
                            flag = foundevent.Name;
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
            }

            return flag;
        }

        public static string FireSpecificEventPartyLeader(string specificEvent, bool force = false, string heroname = null)
        {
            List<string> eventNames = new List<string>();

            string flag = "$FAILEDTOFIND";
            if (CESubModule.CEEventList != null && CESubModule.CEEventList.Count > 0)
            {
                specificEvent = specificEvent.ToLower();
                CEEvent foundevent = CESubModule.CEEventList.FirstOrDefault((CEEvent ceevent) => ceevent.Name.ToLower() == specificEvent);

                if (foundevent != null)
                {
                    if (heroname == null)
                    {
                        foreach (CharacterObject character in PartyBase.MainParty.PrisonRoster.Troops)
                        {
                            if (foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                            {
                                string result = CEEventChecker.FlagsDoMatchEventConditions(foundevent, character, PartyBase.MainParty);
                                if (force || result == null)
                                {
                                    foundevent.Captive = character;
                                    return foundevent.Name;
                                }
                                else
                                {
                                    flag = "$" + result;
                                }
                            }
                        }
                    }
                    else
                    {
                        CharacterObject specificCaptive = PartyBase.MainParty.PrisonRoster.Troops.FirstOrDefault((CharacterObject charaterobject) => charaterobject.Name.ToString() == heroname);

                        if (specificCaptive == null)
                        {
                            return "$FAILTOFINDHERO";
                        }
                        if (foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                        {
                            string result = CEEventChecker.FlagsDoMatchEventConditions(foundevent, specificCaptive, PartyBase.MainParty);
                            if (force || result == null)
                            {
                                foundevent.Captive = specificCaptive;
                                return foundevent.Name;
                            }
                            else
                            {
                                flag = "$" + result;
                            }
                        }
                    }
                }
                else
                {
                    flag = "$EVENTNOTFOUND";
                }
            }

            return flag;
        }

        public static CEEvent ReturnWeightedChoiceOfEventsRandom()
        {
            List<CEEvent> events = new List<CEEvent>();

            if (CESubModule.CECallableEvents != null && CESubModule.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CECallableEvents.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CESubModule.CECallableEvents)
                {
                    if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                    {
                        string result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, CharacterObject.PlayerCharacter, null);
                        if (result == null)
                        {
                            int weightedChance = 10;
                            try
                            {
                                weightedChance = CEEventLoader.GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                            }
                            for (int a = weightedChance; a > 0; a--)
                            {
                                events.Add(listEvent);
                            }
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
                        return events.GetRandomElement();
                    }
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
            List<CEEvent> events = new List<CEEvent>();

            if (CESubModule.CECallableEvents != null && CESubModule.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CECallableEvents.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CESubModule.CECallableEvents)
                {
                    if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                    {
                        string result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                        if (result == null)
                        {
                            int weightedChance = 10;
                            try
                            {
                                if (listEvent.WeightedChanceOfOccuring != null)
                                {
                                    weightedChance = CEEventLoader.GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                            }
                            for (int a = weightedChance; a > 0; a--)
                            {
                                events.Add(listEvent);
                            }
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
                        return events.GetRandomElement();
                    }
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
            List<CEEvent> events = new List<CEEvent>();

            CECustomHandler.LogToFile("Number of Filitered events is " + events.Count);

            if (CESubModule.CECallableEvents != null && CESubModule.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CECallableEvents.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CESubModule.CECallableEvents)
                {
                    if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                    {
                        string result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, captive, PartyBase.MainParty);
                        if (result == null)
                        {
                            int weightedChance = 10;
                            try
                            {
                                weightedChance = CEEventLoader.GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                            }
                            for (int a = weightedChance; a > 0; a--)
                            {
                                events.Add(listEvent);
                            }
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
                        return events.GetRandomElement();
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("eventNames.Count Broken : " + e.ToString());
                    PrintDebugInGameTextMessage("eventNames.Count Broken");
                }
            }
            return null;
        }
    }
}