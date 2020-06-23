using System;
using System.Collections.Generic;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace CaptivityEvents.Events
{
    public class WaitingList
    {
        public string CEWaitingList()
        {
            var eventNames = new List<string>();

            if (CESubModule.CEWaitingList != null && CESubModule.CEWaitingList.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CEWaitingList.Count + " of events to weight and check conditions on.");

                foreach (var listEvent in CESubModule.CEWaitingList)
                {
                    var result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);

                    if (result == null)
                    {
                        var weightedChance = 10;

                        try
                        {
                            if (listEvent.WeightedChanceOfOccuring != null) weightedChance = new VariablesLoader().GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                        }

                        for (var a = weightedChance; a > 0; a--) eventNames.Add(listEvent.Name);
                    }
                    else
                    {
                        CECustomHandler.LogToFile(result);
                    }
                }

                CECustomHandler.LogToFile("Number of Filtered events is " + eventNames.Count);

                try
                {
                    if (eventNames.Count > 0)
                    {
                        var test = MBRandom.Random.Next(0, eventNames.Count - 1);
                        var randomWeightedChoice = eventNames[test];

                        return randomWeightedChoice;
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile("Waiting Menu: Something is broken?");
                }
            }

            CECustomHandler.LogToFile("Number of Filtered events is " + eventNames.Count);

            return null;
        }
    }
}
