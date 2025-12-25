using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace CaptivityEvents.Incidents
{
    public class IncidentHandlerDelegateRandom
    {
        private readonly CEEvent _ceEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;
        private readonly SharedCallBackHelper _sharedCallBackHelper;
        private readonly CECompanionSystem _companionSystem;
        private readonly Dynamics _dynamics = new();
        private readonly ScoresCalculation _score = new();
        private readonly CEImpregnationSystem _impregnation = new();
        private readonly CEVariablesLoader _variableLoader = new();

        internal IncidentHandlerDelegateRandom(CEEvent ceEvent, List<CEEvent> eventList)
        {
            _ceEvent = ceEvent;
            _eventList = eventList;
            _sharedCallBackHelper = new SharedCallBackHelper(ceEvent, null, eventList);
            _companionSystem = new CECompanionSystem(ceEvent, null);
        }

        internal IncidentHandlerDelegateRandom(CEEvent ceEvent, Option option, List<CEEvent> eventList)
        {
            _ceEvent = ceEvent;
            _option = option;
            _eventList = eventList;
            _sharedCallBackHelper = new SharedCallBackHelper(ceEvent, option, eventList);
            _companionSystem = new CECompanionSystem(ceEvent, option);
        }

        internal Func<TextObject, bool> CreateCondition()
        {
            // Random event: check if player is not a captive
            return text =>
            {
                if (Hero.MainHero.IsPrisoner) return false; // Player must not be captive for random events
                string result = new CEEventChecker(_ceEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);

                if (result != null) return false;
                new MenuCallBackDelegateRandom(_ceEvent, _eventList).InitRandomTextVariables();
                return true;
            };
        }

        internal void ExecuteConsequences()
        {
            // Random event consequences
            _sharedCallBackHelper.ConsequenceGiveItem();
            _sharedCallBackHelper.ConsequenceGold();
            _sharedCallBackHelper.ConsequenceChangeGold();
            _sharedCallBackHelper.ConsequenceChangeTrait();
            _sharedCallBackHelper.ConsequenceChangeSkill();
            _sharedCallBackHelper.ConsequenceSlaveryLevel();
            _sharedCallBackHelper.ConsequenceSlaveryFlags();
            _sharedCallBackHelper.ConsequenceProstitutionLevel();
            _sharedCallBackHelper.ConsequenceProstitutionFlags();
            _sharedCallBackHelper.ConsequenceRenown();
            _sharedCallBackHelper.ConsequenceChangeHealth();
            _sharedCallBackHelper.ConsequenceChangeMorale();
            _sharedCallBackHelper.ConsequenceSpawnTroop();
            _sharedCallBackHelper.ConsequenceSpawnHero();
            _sharedCallBackHelper.ConsequenceStripPlayer();
            _sharedCallBackHelper.ConsequencePlaySound();

            _sharedCallBackHelper.ConsequenceGiveBirth();
            _sharedCallBackHelper.ConsequenceAbort();
            _sharedCallBackHelper.ConsequencePlayScene();
            _sharedCallBackHelper.ConsequenceDelayedEvent();
            _sharedCallBackHelper.ConsequenceMission();
            _sharedCallBackHelper.ConsequenceTeleportPlayer();
            _sharedCallBackHelper.ConsequenceDamageParty(PartyBase.MainParty);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StartBattle))
            {
                _sharedCallBackHelper.ConsequenceStartBattle(() => { /* Continue */ }, 2);
            }
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0)
            {
                // Handle trigger events for random context
                ExecuteRandomEventTrigger();
            }
            else if (!string.IsNullOrWhiteSpace(_option.TriggerEventName))
            {
                // Handle single event trigger for random context
                ExecuteSingleEventTrigger();
            }
        }

        private void ExecuteRandomEventTrigger()
        {
            CaptorSpecifics captorSpecifics = new();
            List<CEEvent> eventNames = [];

            try
            {
                foreach (TriggerEvent triggerEvent in _option.TriggerEvents)
                {
                    CEEvent triggeredEvent = _eventList.Find(item => item.Name == triggerEvent.EventName);

                    if (triggeredEvent == null)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(triggerEvent.EventUseConditions) && triggerEvent.EventUseConditions.ToLower() != "false")
                    {
                        CEEvent conditionEvent = triggeredEvent;

                        if (triggerEvent.EventUseConditions.ToLower() != "true")
                        {
                            conditionEvent = _eventList.Find(item => item.Name == triggerEvent.EventUseConditions);

                            if (conditionEvent == null)
                            {
                                CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventUseConditions + " in events.");
                                continue;
                            }
                        }

                        string conditionMatched = null;

                        if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                        {
                            conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
                        }

                        if (conditionMatched != null)
                        {
                            CECustomHandler.LogToFile(conditionMatched);
                            continue;
                        }
                    }

                    int weightedChance = 1;

                    try
                    {
                        weightedChance = new CEVariablesLoader().GetIntFromXML(!string.IsNullOrWhiteSpace(triggerEvent.EventWeight) ? triggerEvent.EventWeight : triggeredEvent.WeightedChanceOfOccurring);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }

                    for (int a = weightedChance; a > 0; a--) eventNames.Add(triggeredEvent);
                }

                if (eventNames.Count > 0)
                {
                    int number = CEHelper.HelperMBRandom(0, eventNames.Count);

                    try
                    {
                        CEEvent triggeredEvent = eventNames[number];
                        triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                        triggeredEvent.SavedCompanions = _ceEvent.SavedCompanions;
                        CEHelper.SafeActivateGameMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                        captorSpecifics.CECaptorContinue(null);
                    }
                }
                else { captorSpecifics.CECaptorContinue(null); }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                captorSpecifics.CECaptorContinue(null);
            }
        }

        private void ExecuteSingleEventTrigger()
        {
            try
            {
                CEEvent triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                triggeredEvent.SavedCompanions = _ceEvent.SavedCompanions;
                CEHelper.SafeSwitchToMenu(triggeredEvent.Name);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Couldn't find " + _option.TriggerEventName + " in events.");
                new CaptorSpecifics().CECaptorContinue(null);
            }
        }
    }
}
