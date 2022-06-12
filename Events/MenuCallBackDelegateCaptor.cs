#define V180

using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;

namespace CaptivityEvents.Events
{
    public class MenuCallBackDelegateCaptor
    {
        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;
        private readonly SharedCallBackHelper _sharedCallBackHelper;
        private readonly CECompanionSystem _companionSystem;
        private readonly Dynamics _dynamics = new();
        private readonly ScoresCalculation _score = new();
        private readonly CEImpregnationSystem _impregnation = new();
        private readonly CEVariablesLoader _variableLoader = new();

        private float _timer = 0;
        private float _max = 0;

        private readonly CaptorSpecifics _captor = new();

        internal MenuCallBackDelegateCaptor(CEEvent listedEvent, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _eventList = eventList;
            _sharedCallBackHelper = new SharedCallBackHelper(listedEvent, null, eventList);
            _companionSystem = new CECompanionSystem(listedEvent, null, eventList);
        }

        internal MenuCallBackDelegateCaptor(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
            _sharedCallBackHelper = new SharedCallBackHelper(listedEvent, option, eventList);
            _companionSystem = new CECompanionSystem(listedEvent, option, eventList);
        }

        #region Progress Event

        internal void CaptorProgressInitWaitGameMenu(MenuCallbackArgs args)
        {
            if (args.MenuContext != null)
            {
                args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                           ? "wait_captive_female"
                                           : "wait_captive_male");
            }

            _sharedCallBackHelper.LoadBackgroundImage("captor_default", _listedEvent.Captive);
            _sharedCallBackHelper.ConsequencePlaySound(true);

            MBTextManager.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale
                                            ? 1
                                            : 0);

            if (MobileParty.MainParty.CurrentSettlement != null)
            {
                MBTextManager.SetTextVariable("SETTLEMENT_NAME", MobileParty.MainParty.CurrentSettlement.Name);
            }

            try
            {
                if (_listedEvent.SavedCompanions != null)
                {
                    foreach (KeyValuePair<string, Hero> item in _listedEvent.SavedCompanions)
                    {
                        MBTextManager.SetTextVariable("COMPANION_NAME_" + item.Key, item.Value?.Name);
                        MBTextManager.SetTextVariable("COMPANIONISFEMALE_" + item.Key, item.Value.IsFemale ? 1 : 0);
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to SetCaptiveTextVariables for " + _listedEvent.Name);
            }

            if (_listedEvent.Captive != null)
            {
                MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive.Name);
                MBTextManager.SetTextVariable("ISCAPTIVEFEMALE", _listedEvent.Captive.IsFemale ? 1 : 0);
            }

            // END HERE
            if (_listedEvent.ProgressEvent != null)
            {
                _max = _variableLoader.GetFloatFromXML(_listedEvent.ProgressEvent.TimeToTake);
                _timer = 0f;

                CEHelper.progressEventExists = true;
                CEHelper.notificationCaptorExists = false;
                CEHelper.notificationEventExists = false;
            }
            else
            {
                CECustomHandler.ForceLogToFile("Missing Progress Event Settings in " + _listedEvent.Name);
            }
        }

        internal bool CaptorProgressConditionWaitGameMenu(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            return true;
        }

        internal void CaptorProgressConsequenceWaitGameMenu(MenuCallbackArgs args)
        {
            if (_listedEvent.ProgressEvent.TriggerEvents != null && _listedEvent.ProgressEvent.TriggerEvents.Length > 0)
            {
                ConsequenceRandomEventTriggerProgress(ref args);
            }
            else if (!string.IsNullOrWhiteSpace(_listedEvent.ProgressEvent.TriggerEventName))
            {
                ConsequenceSingleEventTriggerProgress(ref args);
            }
        }

        internal void CaptorProgressTickWaitGameMenu(MenuCallbackArgs args, CampaignTime dt)
        {
            _timer += dt.CurrentHourInDay;

            if (_timer / _max == 1)
            {
                CEHelper.progressEventExists = false;
            }

            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(_timer / _max);

            PartyBase.MainParty.MobileParty.SetMoveModeHold();
        }

        #endregion Progress Event

        #region Regular Event

        internal void CaptorEventWaitGameMenu(MenuCallbackArgs args)
        {
            InitSetNames(ref args);
            _sharedCallBackHelper.LoadBackgroundImage("captor_default", _listedEvent.Captive);
            _sharedCallBackHelper.ConsequencePlaySound(true);
        }

        internal bool CaptorEventOptionGameMenu(MenuCallbackArgs args)
        {
            PlayerIsNotBusy(ref args);
            PlayerHasOpenSpaceForCompanions(ref args);
            LeaveTypes(ref args);
            InitGiveCaptorGold();
            InitCaptorGoldTotal();
            ReqHeroCaptorRelation(ref args);
            ReqMorale(ref args);
            ReqTroops(ref args);
            ReqMaleTroops(ref args);
            ReqFemaleTroops(ref args);
            ReqCaptives(ref args);
            ReqMaleCaptives(ref args);
            ReqFemaleCaptives(ref args);
            ReqHeroTrait(ref args);
            ReqCaptorTrait(ref args);
            ReqHeroSkill(ref args);
            ReqHeroSkills(ref args);
            ReqCaptorSkill(ref args);
            ReqCaptorSkills(ref args);
            ReqGold(ref args);
            return true;
        }

        internal void CaptorConsequenceGameMenu(MenuCallbackArgs args)
        {
            Hero captiveHero = null;

            try
            {
                if (_listedEvent.Captive != null)
                {
                    if (_listedEvent.Captive.IsHero) captiveHero = _listedEvent.Captive.HeroObject;
                    MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive.Name);
                    MBTextManager.SetTextVariable("ISCAPTIVEFEMALE", _listedEvent.Captive.IsFemale ? 1 : 0);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Hero doesn't exist"); }

            _sharedCallBackHelper.ConsequencePlaySound();
            ConsequenceCaptorLeaveSpouse();
            ConsequenceCaptorGold(captiveHero);
            ConsequenceCaptorChangeGold();
            ConsequenceCaptorSkill();
            ConsequenceCaptorTrait();
            ConsequenceCaptorRenown();
            ConsequenceChangeMorale();

            ConsequenceChangeClan(captiveHero);
            ConsequenceChangeKingdom(captiveHero);

            if (captiveHero != null)
            {
                ConsequenceLeaveSpouse(captiveHero);
                ConsequenceForceMarry(captiveHero);
                ConsequenceSlaveryFlags(captiveHero);
                ConsequenceSlaveryLevel(captiveHero);
                ConsequenceProstitutionFlags(captiveHero);
                ConsequenceProstitutionLevel(captiveHero);
                ConsequenceRelations(captiveHero);
                ConsequenceGold(captiveHero);
                ConsequenceChangeGold(captiveHero);
                ConsequenceTrait(captiveHero);
                ConsequenceSkill(captiveHero);
                ConsequenceRenown(captiveHero);
                ConsequenceChangeHealth(captiveHero);
                ConsequenceImpregnation(captiveHero);
                ConsequenceStrip(captiveHero);
                ConsequenceMakeHeroCompanion(captiveHero);
            }

            ConsequenceCompanions();
            ConsequenceSpawnTroop();
            ConsequenceSpawnHero();


            ConsequenceGainRandomPrisoners();

            ConsequenceEscape();
            ConsequenceRelease(ref args);
            ConsequenceWoundPrisoner(ref args);
            ConsequenceKillPrisoner(ref args);
            ConsequenceWoundTroops(ref args);
            ConsequenceKillTroops(ref args);
            ConsequenceJoinParty();

            _sharedCallBackHelper.ConsequenceDelayedEvent();
            _sharedCallBackHelper.ConsequenceMission();
            _sharedCallBackHelper.ConsequenceTeleportPlayer();

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripHero) && captiveHero != null)
            {
                try
                {
                    if (CESettings.Instance?.EventCaptorGearCaptives ?? true) CECampaignBehavior.AddReturnEquipment(captiveHero, captiveHero.BattleEquipment, captiveHero.CivilianEquipment);

                    MobileParty.MainParty.MemberRoster.AddToCounts(captiveHero.CharacterObject, 1, false);

                    InventoryManager.OpenScreenAsInventoryOf(MobileParty.MainParty, captiveHero.CharacterObject);

                    CEPersistence.removeHero = captiveHero;
                    CEPersistence.captiveInventoryStage = 1;
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("StripHero. Failed" + e.ToString());
                }
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StartBattle))
            {
                _sharedCallBackHelper.ConsequenceStartBattle(() => { _captor.CECaptorContinue(args); }, 1);
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RebelPrisoners)) { _captor.CECaptorPrisonerRebel(args); }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.HuntPrisoners)) { _captor.CECaptorHuntPrisoners(args); }
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0)
            {
                ConsequenceRandomEventTrigger(ref args);
            }
            else if (!string.IsNullOrWhiteSpace(_option.TriggerEventName))
            {
                ConsequenceSingleEventTrigger(ref args);
            }
            else { _captor.CECaptorContinue(args); }
        }

        #endregion Regular Event

        #region Consequences

        private void ConsequenceCompanions()
        {
            try
            {
                _companionSystem.ConsequenceCompanions(CharacterObject.PlayerCharacter, PartyBase.MainParty);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceCompanions. Failed" + e.ToString());
            }
        }

        private void ConsequenceRandomEventTriggerProgress(ref MenuCallbackArgs args)
        {
            List<CEEvent> eventNames = new();

            try
            {
                foreach (TriggerEvent triggerEvent in _listedEvent.ProgressEvent.TriggerEvents)
                {
                    CEEvent triggeredEvent = _eventList.Find(item => item.Name == triggerEvent.EventName);

                    if (triggeredEvent == null)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                        InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + triggerEvent.EventName + " in events.", Colors.Red));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(triggerEvent.EventUseConditions) && triggerEvent.EventUseConditions.ToLower() == "true")
                    {
                        string conditionMatched = null;
                        if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(_listedEvent.Captive, PartyBase.MainParty);
                        }
                        else if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
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
                        weightedChance = _variableLoader.GetIntFromXML(!string.IsNullOrWhiteSpace(triggerEvent.EventWeight)
                                                                     ? triggerEvent.EventWeight
                                                                     : triggeredEvent.WeightedChanceOfOccuring);
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
                        triggeredEvent.Captive = _listedEvent.Captive;
                        triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                        GameMenu.ActivateGameMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                        InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + eventNames[number] + " in events.", Colors.Red));
                        _captor.CECaptorContinue(args);
                    }
                }
                else { _captor.CECaptorContinue(args); }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                _captor.CECaptorContinue(args);
            }
        }

        private void ConsequenceSingleEventTriggerProgress(ref MenuCallbackArgs args)
        {
            try
            {
                CEEvent triggeredEvent = _eventList.Find(item => item.Name == _listedEvent.ProgressEvent.TriggerEventName);
                triggeredEvent.Captive = _listedEvent.Captive;
                triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                GameMenu.SwitchToMenu(triggeredEvent.Name);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Couldn't find " + _listedEvent.ProgressEvent.TriggerEventName + " in events.");
                InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + _option.TriggerEventName + " in events.", Colors.Red));
                _captor.CECaptorContinue(args);
            }
        }

        private void ConsequenceRandomEventTrigger(ref MenuCallbackArgs args)
        {
            List<CEEvent> eventNames = new();

            try
            {
                foreach (TriggerEvent triggerEvent in _option.TriggerEvents)
                {
                    CEEvent triggeredEvent = _eventList.Find(item => item.Name == triggerEvent.EventName);

                    if (triggeredEvent == null)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                        InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + triggerEvent.EventName + " in events.", Colors.Red));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(triggerEvent.EventUseConditions) && triggerEvent.EventUseConditions.ToLower() == "true")
                    {
                        string conditionMatched = null;
                        if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(_listedEvent.Captive, PartyBase.MainParty);
                        }
                        else if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
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
                        weightedChance = _variableLoader.GetIntFromXML(!string.IsNullOrWhiteSpace(triggerEvent.EventWeight)
                                                                     ? triggerEvent.EventWeight
                                                                     : triggeredEvent.WeightedChanceOfOccuring);
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
                        triggeredEvent.Captive = _listedEvent.Captive;
                        triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                        GameMenu.ActivateGameMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                        InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + eventNames[number] + " in events.", Colors.Red));
                        _captor.CECaptorContinue(args);
                    }
                }
                else { _captor.CECaptorContinue(args); }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                _captor.CECaptorContinue(args);
            }
        }

        private void ConsequenceSingleEventTrigger(ref MenuCallbackArgs args)
        {
            try
            {
                CEEvent triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                triggeredEvent.Captive = _listedEvent.Captive;
                triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                GameMenu.SwitchToMenu(triggeredEvent.Name);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Couldn't find " + _option.TriggerEventName + " in events.");
                InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + _option.TriggerEventName + " in events.", Colors.Red));
                _captor.CECaptorContinue(args);
            }
        }

        private void ConsequenceJoinParty()
        {
            if (!(_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.JoinCaptor))) return;

            try
            {
                if (_listedEvent.Captive.IsHero)
                {
                    EndCaptivityAction.ApplyByReleasing(_listedEvent.Captive.HeroObject);
                    AddHeroToPartyAction.Apply(_listedEvent.Captive.HeroObject, PartyBase.MainParty.MobileParty, true);
                }
                else
                {
                    PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
                    PartyBase.MainParty.MemberRoster.AddToCounts(_listedEvent.Captive, 1);
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure of JoinParty: " + e.ToString());
            }
        }

        private void ConsequenceWoundPrisoner(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundPrisoner))
            {
                if (_listedEvent.Captive.IsHero)
                {
                    _listedEvent.Captive.HeroObject.MakeWounded(Hero.MainHero);
                }
                else
                {
                    PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
                    PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, 1, false, 1);
                }
            }

            // Wound Player
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundCaptor)) Hero.MainHero.MakeWounded(_listedEvent.Captive.HeroObject);
            // Wound All
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundAllPrisoners)) _captor.CECaptorWoundPrisoners(args, PartyBase.MainParty.PrisonRoster.Count);
            // Wound Random
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundRandomPrisoners)) _captor.CECaptorWoundPrisoners(args);
        }

        private void ConsequenceKillPrisoner(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner))
            {
                if (_listedEvent.Captive.IsHero) KillCharacterAction.ApplyByExecution(_listedEvent.Captive.HeroObject, Hero.MainHero);
                else PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
            }

            // Kill Player
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor)) _dynamics.CEKillPlayer(_listedEvent.Captive.HeroObject);
            // Kill All
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillAllPrisoners)) _captor.CECaptorKillPrisoners(args, PartyBase.MainParty.PrisonRoster.Count, true);
            // Kill Random
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillRandomPrisoners)) _captor.CECaptorKillPrisoners(args);
        }

        private void ConsequenceWoundTroops(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundRandomTroops)) _dynamics.CEWoundTroops(PartyBase.MainParty);
        }

        private void ConsequenceKillTroops(ref MenuCallbackArgs args)
        {
            // Kill Random
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillRandomTroops)) _dynamics.CEKillTroops(PartyBase.MainParty);
        }

        private void ConsequenceGainRandomPrisoners()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) _dynamics.CEGainRandomPrisoners(PartyBase.MainParty);
        }

        private void ConsequenceRelease(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ReleaseRandomPrisoners)) _captor.CECaptorReleasePrisoners(args);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ReleaseAllPrisoners)) _captor.CECaptorReleasePrisoners(args, PartyBase.MainParty.PrisonRoster.Count, true);
        }

        private void ConsequenceEscape()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) return;

            try
            {
                if (_listedEvent.Captive.IsHero) EndCaptivityAction.ApplyByEscape(_listedEvent.Captive.HeroObject);
                else PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure of Captor Escape: " + e.ToString());
            }
        }

        private void ConsequenceStrip(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Strip)) _captor.CECaptorStripVictim(captiveHero);
        }

        private void ConsequenceMakeHeroCompanion(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.MakeHeroCompanion)) _captor.CECaptorMakeHeroCompanion(captiveHero);
        }

        private void ConsequenceImpregnation(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
            {
                try
                {
                    _impregnation.CaptivityImpregnationChance(captiveHero, !string.IsNullOrWhiteSpace(_option.PregnancyRiskModifier)
                                                      ? _variableLoader.GetIntFromXML(_option.PregnancyRiskModifier)
                                                      : _variableLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier));
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(captiveHero, 30);
                }
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
            {
                try
                {
                    _impregnation.CaptivityImpregnationChance(captiveHero, !string.IsNullOrWhiteSpace(_option.PregnancyRiskModifier)
                                                      ? _variableLoader.GetIntFromXML(_option.PregnancyRiskModifier)
                                                      : _variableLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier), false, false);
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(captiveHero, 30, false, false);
                }
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationByPlayer))
            {
                try
                {
                    _impregnation.CaptivityImpregnationChance(captiveHero, !string.IsNullOrWhiteSpace(_option.PregnancyRiskModifier)
                                                      ? _variableLoader.GetIntFromXML(_option.PregnancyRiskModifier)
                                                      : _variableLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier), false, false, Hero.MainHero);
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(captiveHero, 30, false, false);
                }
            }
        }

        private void ConsequenceChangeHealth(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth)) return;

            try
            {
                captiveHero.HitPoints += !string.IsNullOrWhiteSpace(_option.HealthTotal)
                    ? _variableLoader.GetIntFromXML(_option.HealthTotal)
                    : _variableLoader.GetIntFromXML(_listedEvent.HealthTotal);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing HealthTotal");
                captiveHero.HitPoints += MBRandom.RandomInt(-20, 20);
            }
        }

        private void ConsequenceRenown(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown)) return;

            try
            {
                _dynamics.RenownModifier(!string.IsNullOrWhiteSpace(_option.RenownTotal)
                                                  ? _variableLoader.GetIntFromXML(_option.RenownTotal)
                                                  : _variableLoader.GetIntFromXML(_listedEvent.RenownTotal), captiveHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RenownTotal");
                _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), captiveHero);
            }
        }

        private void ConsequenceSkill(Hero captiveHero)
        {
            try
            {
                if (_option.SkillsToLevel != null && _option.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _option.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (skillToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = _variableLoader.GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(skillToLevel.ByXP);

                        _dynamics.SkillModifier(captiveHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else if (_listedEvent.SkillsToLevel != null && _listedEvent.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _listedEvent.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (skillToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = _variableLoader.GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(skillToLevel.ByXP);

                        _dynamics.SkillModifier(captiveHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill)) return;

                    int level = 0;
                    int xp = 0;

                    if (!string.IsNullOrWhiteSpace(_option.SkillTotal)) level = _variableLoader.GetIntFromXML(_option.SkillTotal);
                    else if (!string.IsNullOrWhiteSpace(_option.SkillXPTotal)) xp = _variableLoader.GetIntFromXML(_option.SkillXPTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillTotal)) level = _variableLoader.GetIntFromXML(_listedEvent.SkillTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillXPTotal)) xp = _variableLoader.GetIntFromXML(_listedEvent.SkillXPTotal);
                    else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                    if (!string.IsNullOrWhiteSpace(_option.SkillToLevel)) _dynamics.SkillModifier(captiveHero, _option.SkillToLevel, level, xp);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillToLevel)) _dynamics.SkillModifier(captiveHero, _listedEvent.SkillToLevel, level, xp);
                    else CECustomHandler.LogToFile("Missing SkillToLevel");
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        internal void ConsequenceTrait(Hero captiveHero)
        {
            try
            {
                if (_option.TraitsToLevel != null && _option.TraitsToLevel.Count(TraitToLevel => TraitToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (TraitToLevel traitToLevel in _option.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (traitToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(captiveHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else if (_listedEvent.TraitsToLevel != null && _listedEvent.TraitsToLevel.Count(TraitsToLevel => TraitsToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (TraitToLevel traitToLevel in _listedEvent.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (traitToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(captiveHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait)) return;

                    int level = 0;
                    int xp = 0;

                    if (!string.IsNullOrWhiteSpace(_option.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.TraitTotal);
                    else if (!string.IsNullOrWhiteSpace(_option.TraitXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_option.TraitXPTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.TraitXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitXPTotal);
                    else CECustomHandler.LogToFile("Missing Trait TraitTotal");

                    if (!string.IsNullOrWhiteSpace(_option.TraitToLevel)) _dynamics.TraitModifier(captiveHero, _option.TraitToLevel, level, xp);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.TraitToLevel)) _dynamics.TraitModifier(captiveHero, _listedEvent.TraitToLevel, level, xp);
                    else CECustomHandler.LogToFile("Missing TraitToLevel");
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }

        private void ConsequenceChangeGold(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrWhiteSpace(_option.GoldTotal)) level = _variableLoader.GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrWhiteSpace(_listedEvent.GoldTotal)) level = _variableLoader.GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        private void ConsequenceGold(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;

            int content = _score.AttractivenessScore(captiveHero);
            int currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, content);
        }

        private void ConsequenceRelations(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation)) return;
            bool InformationMessage = !_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            try
            {
                _dynamics.RelationsModifier(captiveHero, !string.IsNullOrWhiteSpace(_option.RelationTotal)
                                               ? _variableLoader.GetIntFromXML(_option.RelationTotal)
                                               : _variableLoader.GetIntFromXML(_listedEvent.RelationTotal), null, InformationMessage && !NoMessages, !InformationMessage && !NoMessages);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RelationTotal");
            }
        }

        private void ConsequenceProstitutionLevel(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel)) return;

            try
            {
                _dynamics.VictimProstitutionModifier(!string.IsNullOrWhiteSpace(_option.ProstitutionTotal)
                                                        ? _variableLoader.GetIntFromXML(_option.ProstitutionTotal)
                                                        : _variableLoader.GetIntFromXML(_listedEvent.ProstitutionTotal), captiveHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ProstitutionTotal");
                _dynamics.VictimProstitutionModifier(1, captiveHero);
            }
        }

        private void ConsequenceProstitutionFlags(Hero captiveHero)
        {
            bool InformationMessage = !_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) _dynamics.VictimProstitutionModifier(1, captiveHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) _dynamics.VictimProstitutionModifier(0, captiveHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
        }

        private void ConsequenceSlaveryLevel(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel)) return;

            try
            {
                _dynamics.VictimSlaveryModifier(!string.IsNullOrWhiteSpace(_option.SlaveryTotal)
                                                   ? _variableLoader.GetIntFromXML(_option.SlaveryTotal)
                                                   : _variableLoader.GetIntFromXML(_listedEvent.SlaveryTotal), captiveHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing SlaveryTotal");
                _dynamics.VictimSlaveryModifier(1, captiveHero);
            }
        }

        private void ConsequenceSlaveryFlags(Hero captiveHero)
        {
            bool InformationMessage = !_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) _dynamics.VictimSlaveryModifier(1, captiveHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) _dynamics.VictimSlaveryModifier(0, captiveHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
        }

        private void ConsequenceSpawnTroop()
        {
            if (_option.SpawnTroops != null)
            {
                new CESpawnSystem().SpawnTheTroops(_option.SpawnTroops, PartyBase.MainParty);
            }
        }

        private void ConsequenceSpawnHero()
        {
            if (_option.SpawnHeroes != null)
            {
                new CESpawnSystem().SpawnTheHero(_option.SpawnHeroes, PartyBase.MainParty);
            }
        }

        private void ConsequenceChangeKingdom(Hero captiveHero)
        {
            if (_option.KingdomOptions != null) _dynamics.KingdomChange(_option.KingdomOptions, captiveHero, Hero.MainHero);
        }

        private void ConsequenceChangeClan(Hero captiveHero)
        {
            if (_option.ClanOptions != null) _dynamics.ClanChange(_option.ClanOptions, captiveHero, Hero.MainHero);
        }

        private void ConsequenceForceMarry(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor)) _dynamics.ChangeSpouse(captiveHero, Hero.MainHero);
        }

        private void ConsequenceLeaveSpouse(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) _dynamics.ChangeSpouse(captiveHero, null);
        }

        private void ConsequenceChangeMorale()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            try
            {
                _dynamics.MoraleChange(!string.IsNullOrWhiteSpace(_option.MoraleTotal)
                                         ? _variableLoader.GetIntFromXML(_option.MoraleTotal)
                                         : _variableLoader.GetIntFromXML(_listedEvent.MoraleTotal), PartyBase.MainParty);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing MoralTotal");
                _dynamics.MoraleChange(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty);
            }
        }

        private void ConsequenceCaptorRenown()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown)) return;

            try
            {
                _dynamics.RenownModifier(!string.IsNullOrWhiteSpace(_option.RenownTotal)
                                            ? _variableLoader.GetIntFromXML(_option.RenownTotal)
                                            : _variableLoader.GetIntFromXML(_listedEvent.RenownTotal), Hero.MainHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RenownTotal");
                _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
            }
        }

        private void ConsequenceCaptorTrait()
        {
            try
            {
                if (_option.TraitsToLevel != null && _option.TraitsToLevel.Count(TraitToLevel => TraitToLevel.Ref.ToLower() == "captor") != 0)
                {
                    foreach (TraitToLevel traitToLevel in _option.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (traitToLevel.Ref.ToLower() != "captor") continue;
                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(Hero.MainHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else if (_listedEvent.TraitsToLevel != null && _listedEvent.TraitsToLevel.Count(TraitsToLevel => TraitsToLevel.Ref.ToLower() == "captor") != 0)
                {
                    foreach (TraitToLevel traitToLevel in _listedEvent.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (traitToLevel.Ref.ToLower() != "captor") continue;
                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(Hero.MainHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait)) return;

                    int level = 0;
                    int xp = 0;

                    if (!string.IsNullOrWhiteSpace(_option.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.TraitTotal);
                    else if (!string.IsNullOrWhiteSpace(_option.TraitXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_option.TraitXPTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.TraitXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitXPTotal);
                    else CECustomHandler.LogToFile("Missing Trait TraitTotal");

                    if (!string.IsNullOrWhiteSpace(_option.TraitToLevel)) _dynamics.TraitModifier(Hero.MainHero, _option.TraitToLevel, level, xp);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.TraitToLevel)) _dynamics.TraitModifier(Hero.MainHero, _listedEvent.TraitToLevel, level, xp);
                    else CECustomHandler.LogToFile("Missing TraitToLevel");
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }

        private void ConsequenceCaptorSkill()
        {
            try
            {
                if (_option.SkillsToLevel != null && _option.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "captor") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _option.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (skillToLevel.Ref.ToLower() != "captor") continue;
                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = _variableLoader.GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(skillToLevel.ByXP);

                        _dynamics.SkillModifier(Hero.MainHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else if (_listedEvent.SkillsToLevel != null && _listedEvent.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "captor") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _listedEvent.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (skillToLevel.Ref.ToLower() != "captor") continue;
                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = _variableLoader.GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(skillToLevel.ByXP);

                        _dynamics.SkillModifier(Hero.MainHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorSkill)) return;

                    int level = 0;
                    int xp = 0;

                    if (!string.IsNullOrWhiteSpace(_option.SkillTotal)) level = _variableLoader.GetIntFromXML(_option.SkillTotal);
                    else if (!string.IsNullOrWhiteSpace(_option.SkillXPTotal)) xp = _variableLoader.GetIntFromXML(_option.SkillXPTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillTotal)) level = _variableLoader.GetIntFromXML(_listedEvent.SkillTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillXPTotal)) xp = _variableLoader.GetIntFromXML(_listedEvent.SkillXPTotal);
                    else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                    if (!string.IsNullOrWhiteSpace(_option.SkillToLevel)) _dynamics.SkillModifier(Hero.MainHero, _option.SkillToLevel, level, xp);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillToLevel)) _dynamics.SkillModifier(Hero.MainHero, _listedEvent.SkillToLevel, level, xp);
                    else CECustomHandler.LogToFile("Missing SkillToLevel");
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        private void ConsequenceCaptorChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrWhiteSpace(_option.CaptorGoldTotal)) level = _variableLoader.GetIntFromXML(_option.CaptorGoldTotal);
                else if (!string.IsNullOrWhiteSpace(_listedEvent.CaptorGoldTotal)) level = _variableLoader.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                else CECustomHandler.LogToFile("Missing CaptorGoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }
        }

        private void ConsequenceCaptorGold(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold)) return;
            int content = _score.AttractivenessScore(captiveHero);
            int currentValue = captiveHero != null ? captiveHero.GetSkillValue(CESkills.Prostitution) : 2;
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
        }

        private void ConsequenceCaptorLeaveSpouse()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptorLeaveSpouse)) return;
            _dynamics.ChangeSpouse(Hero.MainHero, null);
        }

        #endregion Consequences

        #region Requirements

        #region ReqGold

        private void ReqGold(ref MenuCallbackArgs args)
        {
            try
            {
                ReqGoldAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed "); }

            try
            {
                ReqGoldBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed "); }
        }

        private void ReqGoldBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqGoldBelow)) return;
            if (Hero.MainHero.Gold <= _variableLoader.GetIntFromXML(_option.ReqGoldBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
            args.IsEnabled = false;
        }

        private void ReqGoldAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqGoldAbove)) return;
            if (Hero.MainHero.Gold >= _variableLoader.GetIntFromXML(_option.ReqGoldAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqGold

        #region ReqSkills

        private void ReqCaptorSkills(ref MenuCallbackArgs args)
        {
            if (_option.SkillsRequired == null) return;

            foreach (SkillRequired skillRequired in _option.SkillsRequired)
            {
                if (skillRequired.Ref == "Hero") continue;

                SkillObject foundSkill = CESkills.FindSkill(skillRequired.Id);

                if (foundSkill == null)
                {
                    CECustomHandler.ForceLogToFile("Could not find " + skillRequired.Id);
                    return;
                }

                int skillLevel = Hero.MainHero.GetSkillValue(foundSkill);

                try
                {
                    if (ReqSkillsLevelAbove(ref args, foundSkill, skillLevel, skillRequired.Min, "str_CE_skill_level")) break;
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid SkillRequiredAbove"); }

                try
                {
                    if (ReqSkillsLevelBelow(ref args, foundSkill, skillLevel, skillRequired.Max, "str_CE_skill_level")) break;
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid SkillRequiredBelow"); }
            }
        }

        private void ReqHeroSkills(ref MenuCallbackArgs args)
        {
            if (_option.SkillsRequired == null) return;

            foreach (SkillRequired skillRequired in _option.SkillsRequired)
            {
                if (skillRequired.Ref == "Captor") continue;

                SkillObject foundSkill = CESkills.FindSkill(skillRequired.Id);

                if (foundSkill == null)
                {
                    CECustomHandler.ForceLogToFile("Could not find " + skillRequired.Id);
                    return;
                }

                int skillLevel = _listedEvent.Captive.GetSkillValue(foundSkill);

                try
                {
                    if (ReqSkillsLevelAbove(ref args, foundSkill, skillLevel, skillRequired.Min, "str_CE_skill_captive_level")) break;
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid SkillRequiredAbove"); }

                try
                {
                    if (ReqSkillsLevelBelow(ref args, foundSkill, skillLevel, skillRequired.Max, "str_CE_skill_captive_level")) break;
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid SkillRequiredBelow"); }
            }
        }

        private bool ReqSkillsLevelBelow(ref MenuCallbackArgs args, SkillObject skillRequired, int skillLevel, string max, string type)
        {
            if (string.IsNullOrWhiteSpace(max)) return false;
            if (skillLevel <= _variableLoader.GetIntFromXML(max)) return false;

            TextObject text = GameTexts.FindText(type, "high");
            text.SetTextVariable("SKILL", skillRequired.Name);
            args.Tooltip = text;
            args.IsEnabled = false;

            return true;
        }

        private bool ReqSkillsLevelAbove(ref MenuCallbackArgs args, SkillObject skillRequired, int skillLevel, string min, string type)
        {
            if (string.IsNullOrWhiteSpace(min)) return false;
            if (skillLevel >= _variableLoader.GetIntFromXML(min)) return false;

            TextObject text = GameTexts.FindText(type, "low");
            text.SetTextVariable("SKILL", skillRequired.Name);
            args.Tooltip = text;
            args.IsEnabled = false;

            return true;
        }

        #endregion ReqSkills

        #region ReqCaptorSkill

        private void ReqCaptorSkill(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptorSkill)) return;

            int skillLevel = ReqCaptorSkill();

            try
            {
                ReqCaptorSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove"); }

            try
            {
                ReqCaptorSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow"); }
        }

        private void ReqCaptorSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (!string.IsNullOrWhiteSpace(_option.ReqCaptorSkillLevelBelow)) return;
            if (skillLevel <= _variableLoader.GetIntFromXML(_option.ReqCaptorSkillLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptorSkillLevelAbove)) return;
            if (skillLevel >= _variableLoader.GetIntFromXML(_option.ReqCaptorSkillLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int ReqCaptorSkill()
        {
            int skillLevel = 0;

            try
            {
                SkillObject foundSkill = CESkills.FindSkill(_option.ReqCaptorSkill);
                if (foundSkill == null)
                    CECustomHandler.LogToFile("Invalid Skill Captor");
                else
                    skillLevel = Hero.MainHero.GetSkillValue(foundSkill);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill Captor");
                skillLevel = 0;
            }

            return skillLevel;
        }

        #endregion ReqCaptorSkill

        #region ReqHeroSkill

        private void ReqHeroSkill(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroSkill)) return;

            int skillLevel = ReqHeroSkill();

            try
            {
                ReqHeroSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove"); }

            try
            {
                ReqHeroSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow"); }
        }

        private void ReqHeroSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroSkillLevelBelow)) return;
            if (skillLevel <= _variableLoader.GetIntFromXML(_option.ReqHeroSkillLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_captive_level", "high");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroSkillLevelAbove)) return;
            if (skillLevel >= _variableLoader.GetIntFromXML(_option.ReqHeroSkillLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_captive_level", "low");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int ReqHeroSkill()
        {
            int skillLevel = 0;

            try
            {
                SkillObject foundSkill = CESkills.FindSkill(_option.ReqHeroSkill);
                if (foundSkill == null)
                    CECustomHandler.LogToFile("Invalid Skill Captive");
                else
                    skillLevel = _listedEvent.Captive.GetSkillValue(foundSkill);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill Captive");
                skillLevel = 0;
            }

            return skillLevel;
        }

        #endregion ReqHeroSkill

        #region CaptorTrait

        private void ReqCaptorTrait(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptorTrait)) return;

            int traitLevel = SetCaptorTraitLevel();

            try
            {
                ReqCaptorTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove"); }

            try
            {
                ReqCaptorTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow"); }
        }

        private void ReqCaptorTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptorTraitLevelBelow)) return;
            if (traitLevel <= _variableLoader.GetIntFromXML(_option.ReqCaptorTraitLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptorTraitLevelAbove)) return;
            if (traitLevel >= _variableLoader.GetIntFromXML(_option.ReqCaptorTraitLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int SetCaptorTraitLevel()
        {
            int traitLevel;

            try
            {
                traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.All.Single((TraitObject traitObject) => traitObject.StringId == _option.ReqCaptorTrait));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captor");
                traitLevel = 0;
            }

            return traitLevel;
        }

        #endregion CaptorTrait

        #region CaptiveTrait

        private void ReqHeroTrait(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTrait)) return;

            int traitLevel = SetCaptiveTraitLevel();

            try
            {
                ReqHeroTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove"); }

            try
            {
                ReqHeroTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow"); }
        }

        private void ReqHeroTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTraitLevelBelow)) return;
            if (traitLevel <= _variableLoader.GetIntFromXML(_option.ReqHeroTraitLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_captive_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTraitLevelAbove)) return;
            if (traitLevel >= _variableLoader.GetIntFromXML(_option.ReqHeroTraitLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_captive_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int SetCaptiveTraitLevel()
        {
            int traitLevel;

            try
            {
                traitLevel = _listedEvent.Captive.GetTraitLevel(TraitObject.All.Single((TraitObject traitObject) => traitObject.StringId == _option.ReqCaptorTrait));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captive");
                traitLevel = 0;
            }

            return traitLevel;
        }

        #endregion CaptiveTrait

        #region ReqFemaleCaptives

        private void ReqFemaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                ReqFemaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed "); }

            try
            {
                ReqHeroFemaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroFemaleCaptivesAbove / Failed "); }

            try
            {
                ReqFemaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed "); }

            try
            {
                ReqHeroFemaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroFemaleCaptivesBelow / Failed "); }
        }

        private void ReqFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqFemaleCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) <= _variableLoader.GetIntFromXML(_option.ReqFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= _variableLoader.GetIntFromXML(_option.ReqHeroFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqFemaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqFemaleCaptivesAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroFemaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqFemaleCaptives

        #region ReqMaleCaptives

        private void ReqMaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                ReqMaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed "); }

            try
            {
                ReqHeroMaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleHeroCaptivesAbove / Failed "); }

            try
            {
                ReqMaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed "); }

            try
            {
                ReqHeroMaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleHeroCaptivesBelow / Failed "); }
        }

        private void ReqMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMaleCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqMaleCaptivesAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroMaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqMaleCaptives

        #region ReqCaptives

        private void ReqCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                ReqCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed "); }

            try
            {
                ReqHeroCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptivesAbove / Failed "); }

            try
            {
                ReqCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed "); }

            try
            {
                ReqHeroCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptivesBelow / Failed "); }
        }

        private void ReqCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) <= _variableLoader.GetIntFromXML(_option.ReqCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= _variableLoader.GetIntFromXML(_option.ReqHeroCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) >= _variableLoader.GetIntFromXML(_option.ReqCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqCaptives

        #region ReqFemaleTroops

        private void ReqFemaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                ReqFemaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed "); }

            try
            {
                ReqHeroFemaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroFemaleTroopsAbove / Failed "); }

            try
            {
                ReqFemaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed "); }

            try
            {
                ReqHeroFemaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroFemaleTroopsBelow / Failed "); }
        }

        private void ReqFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqFemaleTroopsBelow)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) <= _variableLoader.GetIntFromXML(_option.ReqFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleTroopsBelow)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= _variableLoader.GetIntFromXML(_option.ReqHeroFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqFemaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqFemaleTroops

        #region ReqMaleTroops

        private void ReqMaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                ReqMaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed "); }

            try
            {
                ReqHeroMaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroMaleTroopsAbove / Failed "); }

            try
            {
                ReqMaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed "); }

            try
            {
                ReqHeroMaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroMaleTroopsBelow / Failed "); }
        }

        private void ReqMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMaleTroopsBelow)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleTroopsBelow)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= _variableLoader.GetIntFromXML(_option.ReqHeroMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqMaleTroops

        #region ReqTroops

        private void ReqTroops(ref MenuCallbackArgs args)
        {
            try
            {
                ReqTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed "); }

            try
            {
                ReqHeroTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroTroopsAbove / Failed "); }

            try
            {
                ReqTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed "); }

            try
            {
                ReqHeroTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroTroopsBelow / Failed "); }
        }

        private void ReqTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqTroopsBelow)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; })) <= _variableLoader.GetIntFromXML(_option.ReqTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTroopsBelow)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; })) <= _variableLoader.GetIntFromXML(_option.ReqHeroTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqTroopsAbove)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; })) >= _variableLoader.GetIntFromXML(_option.ReqTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTroopsAbove)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; })) >= _variableLoader.GetIntFromXML(_option.ReqHeroTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqTroops

        #region ReqMorale

        private void ReqMorale(ref MenuCallbackArgs args)
        {
            try
            {
                ReqMoraleAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed "); }

            try
            {
                ReqMoraleBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed "); }
        }

        private void ReqMoraleBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMoraleBelow)) return;
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale > _variableLoader.GetIntFromXML(_option.ReqMoraleBelow))) return;

            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMoraleAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMoraleAbove)) return;
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale < _variableLoader.GetIntFromXML(_option.ReqMoraleAbove))) return;

            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqMorale

        #region ReqHeroCaptorRelation

        private void ReqHeroCaptorRelation(ref MenuCallbackArgs args)
        {
            if (_listedEvent?.Captive?.HeroObject == null) return;

            try
            {
                ReqHeroCaptorRelationAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationAbove / Failed "); }

            try
            {
                if (ReqHeroCaptorRelationBelow(ref args)) return;
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationBelow / Failed "); }
        }

        private bool ReqHeroCaptorRelationBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroCaptorRelationBelow)) return true;

            if (!(_listedEvent.Captive.HeroObject.GetRelationWithPlayer() > _variableLoader.GetFloatFromXML(_option.ReqHeroCaptorRelationBelow))) return false;
            TextObject textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
            textResponse3.SetTextVariable("HERO", _listedEvent.Captive.HeroObject.Name.ToString());
            args.Tooltip = textResponse3;
            args.IsEnabled = false;

            return false;
        }

        private void ReqHeroCaptorRelationAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroCaptorRelationAbove)) return;
            if (!(_listedEvent.Captive.HeroObject.GetRelationWithPlayer() < _variableLoader.GetFloatFromXML(_option.ReqHeroCaptorRelationAbove))) return;

            TextObject textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
            textResponse4.SetTextVariable("HERO", _listedEvent.Captive.HeroObject.Name.ToString());
            args.Tooltip = textResponse4;
            args.IsEnabled = false;
        }

        #endregion ReqHeroCaptorRelation

        #endregion Requirements

        #region Icons

        private void LeaveTypes(ref MenuCallbackArgs args)
        {
            Wait(ref args);
            Trade(ref args);
            RansomAndBribe(ref args);
            Submenu(ref args);
            Continue(ref args);
            Default(ref args);
        }

        private void Default(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;
        }

        private void Continue(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;
        }

        private void Submenu(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
        }

        private void RansomAndBribe(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe)) args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
        }

        private void Trade(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade)) args.optionLeaveType = GameMenuOption.LeaveType.Trade;
        }

        private void Wait(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait)) args.optionLeaveType = GameMenuOption.LeaveType.Wait;
        }

        #endregion Icons

        #region Init Options

        private void InitCaptorGoldTotal()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrWhiteSpace(_option.CaptorGoldTotal)) level = _variableLoader.GetIntFromXML(_option.CaptorGoldTotal);
                else if (!string.IsNullOrWhiteSpace(_listedEvent.CaptorGoldTotal)) level = _variableLoader.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                else CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }
        }

        private void InitGiveCaptorGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold)) return;

            int content = _score.AttractivenessScore(_listedEvent.Captive.HeroObject);
            if (_listedEvent.Captive.HeroObject != null) content += _listedEvent.Captive.HeroObject.GetSkillValue(CESkills.Prostitution) / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
            MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", content);
        }

        private void InitSetNames(ref MenuCallbackArgs args)
        {
            try
            {
                if (_listedEvent.Captive != null)
                {
                    //Hero captiveHero = null;
                    //if (_listedEvent.Captive.IsHero) captiveHero = _listedEvent.Captive.HeroObject; //WARNING: captiveHero never used
                    MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive?.Name);
                    MBTextManager.SetTextVariable("ISCAPTIVEFEMALE", _listedEvent.Captive?.IsFemale == true ? 1 : 0);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Hero doesn't exist"); }

            TextObject text = args.MenuContext.GameMenu.GetText();
            if (MobileParty.MainParty.CurrentSettlement != null) text.SetTextVariable("SETTLEMENT_NAME", MobileParty.MainParty.CurrentSettlement.Name);
            text.SetTextVariable("PARTY_NAME", MobileParty.MainParty.Name);
            text.SetTextVariable("CAPTOR_NAME", Hero.MainHero.Name);

            try
            {
                if (_listedEvent.SavedCompanions != null)
                {
                    foreach (KeyValuePair<string, Hero> item in _listedEvent.SavedCompanions)
                    {
                        text.SetTextVariable("COMPANION_NAME_" + item.Key, item.Value?.Name);
                        text.SetTextVariable("COMPANIONISFEMALE_" + item.Key, item.Value.IsFemale ? 1 : 0);
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to SetNames for " + _listedEvent.Name);
            }

            if (args.MenuContext != null)
            {
                args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                           ? "wait_prisoner_female"
                                                           : "wait_prisoner_male");
            }
        }

        #endregion Init Options

        #region CustomConsequencesReq

        private void PlayerIsNotBusy(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerIsNotBusy)) return;
            if (PlayerEncounter.Current == null) return;

            args.Tooltip = GameTexts.FindText("str_CE_busy_right_now");
            args.IsEnabled = false;
        }

        private void PlayerHasOpenSpaceForCompanions(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerAllowedCompanion)) return;
            if (!(Clan.PlayerClan.Companions.Count<Hero>() >= Clan.PlayerClan.CompanionLimit)) return;

            args.Tooltip = GameTexts.FindText("str_CE_companions_too_many");
            args.IsEnabled = false;
        }

        #endregion CustomConsequencesReq
    }
}