using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace CaptivityEvents.Events
{
    public class WaitingList
    {
        public string CEWaitingList()
        {
            List<string> eventNames = new List<string>();

            if (CEPersistence.CEWaitingList != null && CEPersistence.CEWaitingList.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CEPersistence.CEWaitingList.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CEPersistence.CEWaitingList)
                {
                    string result = new CEEventChecker(listEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);

                    if (result == null)
                    {
                        int weightedChance = 10;

                        try
                        {
                            if (listEvent.WeightedChanceOfOccuring != null) weightedChance = new CEVariablesLoader().GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                        }

                        for (int a = weightedChance; a > 0; a--) eventNames.Add(listEvent.Name);
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
                        int test = MBRandom.Random.Next(0, eventNames.Count);
                        string randomWeightedChoice = eventNames[test];
                        CECustomHandler.LogToFile("CEWaitingList Choice is " + randomWeightedChoice);
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
