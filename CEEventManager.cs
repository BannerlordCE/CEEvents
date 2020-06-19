using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    internal class CEEventManager
    {
        // Flags and Conditions
        private static void PrintDebugInGameTextMessage(string v)
        {
            var textObject = new TextObject(v);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
        }

        public static string FireSpecificEvent(string specificEvent, bool force = false)
        {
            var eventNames = new List<string>();

            var flag = "$FAILEDTOFIND";
            if (CESubModule.CEEventList == null || CESubModule.CEEventList.Count <= 0) return flag;
            specificEvent = specificEvent.ToLower();
            var foundevent = CESubModule.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == specificEvent);

            if (foundevent != null)
            {
                if (!force && foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                {
                    var result = CEEventChecker.FlagsDoMatchEventConditions(foundevent, CharacterObject.PlayerCharacter,
                        PlayerCaptivity.CaptorParty);
                    if (result == null)
                        flag = foundevent.Name;
                    else
                        flag = "$" + result;
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

        public static string FireSpecificEventRandom(string specificEvent, bool force = false)
        {
            var eventNames = new List<string>();

            var flag = "$FAILEDTOFIND";
            if (CESubModule.CEEventList == null || CESubModule.CEEventList.Count <= 0) return flag;
            specificEvent = specificEvent.ToLower();
            var foundevent = CESubModule.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == specificEvent);

            if (foundevent != null)
            {
                if (foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                {
                    var result =
                        CEEventChecker.FlagsDoMatchEventConditions(foundevent, CharacterObject.PlayerCharacter);
                    if (force || result == null)
                        flag = foundevent.Name;
                    else
                        flag = "$" + result;
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

        public static string FireSpecificEventPartyLeader(string specificEvent, bool force = false,
            string heroname = null)
        {
            var eventNames = new List<string>();

            var flag = "$FAILEDTOFIND";
            if (CESubModule.CEEventList == null || CESubModule.CEEventList.Count <= 0) return flag;
            specificEvent = specificEvent.ToLower();
            var foundevent = CESubModule.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == specificEvent);

            if (foundevent != null)
            {
                if (heroname == null)
                {
                    foreach (var character in PartyBase.MainParty.PrisonRoster.Troops)
                    {
                        if (!foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                            continue;
                        var result =
                            CEEventChecker.FlagsDoMatchEventConditions(foundevent, character, PartyBase.MainParty);
                        if (force || result == null)
                        {
                            foundevent.Captive = character;
                            return foundevent.Name;
                        }

                        flag = "$" + result;
                    }
                }
                else
                {
                    var specificCaptive =
                        PartyBase.MainParty.PrisonRoster.Troops.FirstOrDefault(charaterobject =>
                            charaterobject.Name.ToString() == heroname);

                    if (specificCaptive == null) return "$FAILTOFINDHERO";

                    if (!foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                        return flag;
                    var result =
                        CEEventChecker.FlagsDoMatchEventConditions(foundevent, specificCaptive, PartyBase.MainParty);
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

        public static CEEvent ReturnWeightedChoiceOfEventsRandom()
        {
            var events = new List<CEEvent>();

            if (CESubModule.CECallableEvents != null && CESubModule.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CECallableEvents.Count +
                                          " of events to weight and check conditions on.");

                foreach (var listEvent in CESubModule.CECallableEvents)
                {
                    if (!listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random)) continue;

                    var result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, CharacterObject.PlayerCharacter);
                    if (result == null)
                    {
                        var weightedChance = 10;
                        try
                        {
                            weightedChance = CEEventLoader.GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                        }

                        for (var a = weightedChance; a > 0; a--) events.Add(listEvent);
                    }
                    else
                    {
                        CECustomHandler.LogToFile(result);
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

            CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);
            return null;
        }

        public static CEEvent ReturnWeightedChoiceOfEvents()
        {
            var events = new List<CEEvent>();

            if (CESubModule.CECallableEvents != null && CESubModule.CECallableEvents.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CECallableEvents.Count +
                                          " of events to weight and check conditions on.");

                foreach (var listEvent in CESubModule.CECallableEvents)
                {
                    if (!listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive)) continue;

                    var result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, CharacterObject.PlayerCharacter,
                        PlayerCaptivity.CaptorParty);
                    if (result == null)
                    {
                        var weightedChance = 10;
                        try
                        {
                            if (listEvent.WeightedChanceOfOccuring != null)
                                weightedChance = CEEventLoader.GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                        }

                        for (var a = weightedChance; a > 0; a--) events.Add(listEvent);
                    }
                    else
                    {
                        CECustomHandler.LogToFile(result);
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

            CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);
            return null;
        }

        public static CEEvent ReturnWeightedChoiceOfEventsPartyLeader(CharacterObject captive)
        {
            var events = new List<CEEvent>();

            CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);

            if (CESubModule.CECallableEvents == null || CESubModule.CECallableEvents.Count <= 0) return null;
            CECustomHandler.LogToFile("Having " + CESubModule.CECallableEvents.Count +
                                      " of events to weight and check conditions on.");

            foreach (var listEvent in CESubModule.CECallableEvents)
            {
                if (!listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor)) continue;
                var result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, captive, PartyBase.MainParty);
                if (result == null)
                {
                    var weightedChance = 10;
                    try
                    {
                        weightedChance = CEEventLoader.GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                    }

                    for (var a = weightedChance; a > 0; a--) events.Add(listEvent);
                }
                else
                {
                    CECustomHandler.LogToFile(result);
                }
            }

            CECustomHandler.LogToFile("Number of Filtered events is " + events.Count);

            try
            {
                if (events.Count > 0) return events.GetRandomElement();
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("eventNames.Count Broken : " + e);
                PrintDebugInGameTextMessage("eventNames.Count Broken");
            }

            return null;
        }
    }
}