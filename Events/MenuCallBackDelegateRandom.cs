#define V127

using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;


namespace CaptivityEvents.Events
{
    public class MenuCallBackDelegateRandom
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

        internal MenuCallBackDelegateRandom(CEEvent listedEvent, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _eventList = eventList;
            _sharedCallBackHelper = new SharedCallBackHelper(listedEvent, null, eventList);
            _companionSystem = new CECompanionSystem(listedEvent, null, eventList);
        }

        internal MenuCallBackDelegateRandom(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
            _sharedCallBackHelper = new SharedCallBackHelper(listedEvent, option, eventList);
            _companionSystem = new CECompanionSystem(listedEvent, option, eventList);
        }

        #region Progress Event

        internal void RandomProgressInitWaitGameMenu(MenuCallbackArgs args)
        {
            args.MenuContext?.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                           ? "wait_captive_female"
                                           : "wait_captive_male");

            _sharedCallBackHelper.LoadBackgroundImage("default_random");
            _sharedCallBackHelper.ConsequencePlaySound(true);

            MBTextManager.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale
                                            ? 1
                                            : 0);

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
                CECustomHandler.ForceLogToFile("Failed to RandomProgressInitWaitGameMenu for " + _listedEvent.Name);
            }

            // ENDS
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

        internal bool RandomProgressConditionWaitGameMenu(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            return true;
        }

        internal void RandomProgressConsequenceWaitGameMenu(MenuCallbackArgs args)
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

        internal void RandomProgressTickWaitGameMenu(MenuCallbackArgs args, CampaignTime dt)
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

        internal void RandomEventGameMenu(MenuCallbackArgs args)
        {
            args.MenuContext?.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                                       ? "wait_prisoner_female"
                                                                       : "wait_prisoner_male");

            _sharedCallBackHelper.LoadBackgroundImage("default_random");
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
                CECustomHandler.ForceLogToFile("Failed to RandomEventGameMenu for " + _listedEvent.Name);
            }
        }

        internal bool RandomEventConditionMenuOption(MenuCallbackArgs args)
        {
            PlayerIsNotBusy(ref args);
            PlayerHasOpenSpaceForCompanions(ref args);

            _sharedCallBackHelper.InitIcons(ref args);
            _sharedCallBackHelper.InitGiveItem();

            InitSoldToSettlement();
            InitSoldToCaravan();
            InitSoldToTradeShip();
            InitSoldToLordParty();
            InitRemoveOwner();
            InitGiveGold();
            InitChangeGold();

            ReqMorale(ref args);
            ReqTroops(ref args);
            ReqMaleTroops(ref args);
            ReqFemaleTroops(ref args);
            ReqCaptives(ref args);
            ReqMaleCaptives(ref args);
            ReqFemaleCaptives(ref args);
            ReqHeroHealthPercentage(ref args);
            ReqSlavery(ref args);
            ReqProstitute(ref args);
            ReqHeroSkill(ref args);
            ReqHeroSkills(ref args);
            ReqTrait(ref args);
            ReqGold(ref args);

            return _sharedCallBackHelper.ShouldHide(ref args);
        }

        internal void RandomEventConsequenceMenuOption(MenuCallbackArgs args)
        {
            CaptorSpecifics captorSpecifics = new();
            _sharedCallBackHelper.ConsequenceGiveItem();
            _sharedCallBackHelper.ConsequenceXP();
            _sharedCallBackHelper.ConsequenceLeaveSpouse();
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

            ConsequenceCompanions();
            ConsequenceChangeClan();
            ConsequenceChangeKingdom();
            ConsequenceImpregnation();
            ConsequenceGainRandomPrisoners();
            ConsequenceRemoveOwner();
            ConsequenceCapturedByParty(ref args);
            ConsequenceSoldEvents(ref args);
            ConsequenceWoundTroops();
            ConsequenceKillTroops();

            _sharedCallBackHelper.ConsequenceGiveBirth();
            _sharedCallBackHelper.ConsequenceAbort();
            _sharedCallBackHelper.ConsequencePlayScene();
            _sharedCallBackHelper.ConsequenceDelayedEvent();
            _sharedCallBackHelper.ConsequenceMission();
            _sharedCallBackHelper.ConsequenceTeleportPlayer();
            _sharedCallBackHelper.ConsequenceDamageParty(PartyBase.MainParty);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor))
            {
                _dynamics.CEKillPlayer(null);
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StartBattle))
            {
                _sharedCallBackHelper.ConsequenceStartBattle(() =>
                {
                    captorSpecifics.CECaptorContinue(args);
                }, 2);
            }
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0)
            {
                ConsequenceRandomEventTrigger(ref args);
            }
            else if (!string.IsNullOrWhiteSpace(_option.TriggerEventName))
            {
                ConsequenceSingleEventTrigger(ref args); // Single Event Trigger
            }
            else
            {
                captorSpecifics.CECaptorContinue(args);
            }
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
                CECustomHandler.ForceLogToFile("ConsequenceRandomCompanions. Failed" + e.ToString());
            }
        }

        private void ConsequenceRandomEventTriggerProgress(ref MenuCallbackArgs args)
        {
            CaptorSpecifics captorSpecifics = new();
            List<CEEvent> eventNames = [];

            try
            {
                foreach (TriggerEvent triggerEvent in _listedEvent.ProgressEvent.TriggerEvents)
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
                        if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                        {
                            conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                        }
                        else if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                        {
                            conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
                        }

                        if (conditionMatched != null)
                        {
                            CECustomHandler.LogToFile(conditionMatched);
                            continue;
                        }
                    }

                    int weightedChance = 0;

                    try
                    {
                        weightedChance = new CEVariablesLoader().GetIntFromXML(!string.IsNullOrWhiteSpace(triggerEvent.EventWeight)
                                                                      ? triggerEvent.EventWeight
                                                                      : triggeredEvent.WeightedChanceOfOccurring);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }

                    if (weightedChance == 0) weightedChance = 1;

                    for (int a = weightedChance; a > 0; a--) eventNames.Add(triggeredEvent);
                }

                if (eventNames.Count > 0)
                {
                    int number = CEHelper.HelperMBRandom(0, eventNames.Count);

                    try
                    {
                        CEEvent triggeredEvent = eventNames[number];
                        triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                        triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                        GameMenu.ActivateGameMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                        captorSpecifics.CECaptorContinue(args);
                    }
                }
                else { captorSpecifics.CECaptorContinue(args); }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                captorSpecifics.CECaptorContinue(args);
            }
        }

        private void ConsequenceSingleEventTriggerProgress(ref MenuCallbackArgs args)
        {
            try
            {
                CEEvent triggeredEvent = _eventList.Find(item => item.Name == _listedEvent.ProgressEvent.TriggerEventName);
                triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                GameMenu.SwitchToMenu(triggeredEvent.Name);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Couldn't find " + _listedEvent.ProgressEvent.TriggerEventName + " in events.");
                new CaptorSpecifics().CECaptorContinue(args);
            }
        }

        private void ConsequenceSingleEventTrigger(ref MenuCallbackArgs args)
        {
            try
            {
                CEEvent triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                GameMenu.SwitchToMenu(triggeredEvent.Name);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Couldn't find " + _option.TriggerEventName + " in events.");
                new CaptorSpecifics().CECaptorContinue(args);
            }
        }

        private void ConsequenceRandomEventTrigger(ref MenuCallbackArgs args)
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

                        if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                        {
                            conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                        }
                        else if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
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
                        weightedChance = new CEVariablesLoader().GetIntFromXML(!string.IsNullOrWhiteSpace(triggerEvent.EventWeight)
                                                                      ? triggerEvent.EventWeight
                                                                      : triggeredEvent.WeightedChanceOfOccurring);
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
                        triggeredEvent.SavedCompanions = _listedEvent.SavedCompanions;
                        GameMenu.ActivateGameMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                        captorSpecifics.CECaptorContinue(args);
                    }
                }
                else { captorSpecifics.CECaptorContinue(args); }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                captorSpecifics.CECaptorContinue(args);
            }
        }

        private void ConsequenceWoundTroops()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundRandomTroops))
            {
                _dynamics.CEWoundTroops(PartyBase.MainParty);
            }
            else
            {
            }
        }

        private void ConsequenceKillTroops()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillRandomTroops))
            {
                _dynamics.CEKillTroops(PartyBase.MainParty);
            }

            else
            {
            }
        }

        private void ConsequenceCapturedByParty(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CapturePlayer)) return;
            try
            {
                TroopRoster enemyTroops = TroopRoster.CreateDummyTroopRoster();

                // Make sure there is atleast one troop
                CharacterObject characterObject1 = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>().GetRandomElementWithPredicate(item => item.Occupation == Occupation.Soldier);
                enemyTroops.AddToCounts(characterObject1, 1, false, 0, 0, true, -1);

                foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.MemberRoster.GetTroopRoster())
                {
                    if (!troopRosterElement.Character.IsPlayerCharacter)
                    {
                        if (troopRosterElement.Character.IsHero && troopRosterElement.Character.HeroObject.IsPlayerCompanion)
                        {
                            troopRosterElement.Character.HeroObject.ChangeState(Hero.CharacterStates.Fugitive);
                            if (troopRosterElement.Character.HeroObject.PartyBelongedToAsPrisoner != null)
                            {
                                EndCaptivityAction.ApplyByEscape(troopRosterElement.Character.HeroObject, null);
                            }
                        }
                        else
                        {
                            enemyTroops.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, troopRosterElement.WoundedNumber, troopRosterElement.Xp, true, -1);
                        }
                    }
                }

                foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.PrisonRoster.GetTroopRoster())
                {
                    if (troopRosterElement.Character.IsHero)
                    {
                        EndCaptivityAction.ApplyByEscape(troopRosterElement.Character.HeroObject);
                        continue;
                    }
                    PartyBase.MainParty.PrisonRoster.RemoveTroop(troopRosterElement.Character, 1);
                }

                PartyBase.MainParty.MemberRoster.RemoveIf((TroopRosterElement t) => !t.Character.IsPlayerCharacter);

                if (PartyBase.MainParty.SiegeEvent != null)
                {
                    LiftSiegeAction.GetGameAction(PartyBase.MainParty.MobileParty);
                }

                if (!enemyTroops.GetTroopRoster().IsEmpty())
                {
                    //SpawnAPartyInFaction
                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                    clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                    Settlement nearest = SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), settlement => { return true; });

                    MobileParty customParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false, null, CEHelper.GetSpawnPositionAroundSettlement(nearest));

                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                    customParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position, 0.5f, 0.1f);
                    customParty.Party.SetCustomName(new TextObject("Bandits", null));

                    customParty.MemberRoster.Clear();
                    customParty.MemberRoster.Add(enemyTroops);

                    // InitBanditParty
                    customParty.Party.SetVisualAsDirty();
                    customParty.ActualClan = clan;

                    customParty.IsActive = true;
                    customParty.Party.SetCustomOwner(clan.Leader);

                    // CreatePartyTrade
                    float totalStrength = customParty.Party.CalculateCurrentStrength();
                    int initialGold = (int)(10f * customParty.Party.MemberRoster.TotalManCount * (0.5f + 1f * MBRandom.RandomFloat));
                    customParty.InitializePartyTrade(initialGold);

                    foreach (ItemObject itemObject in Items.All)
                    {
                        if (itemObject.IsFood)
                        {
                            int num2 = MBRandom.RoundRandomized(customParty.MemberRoster.TotalManCount * (1f / itemObject.Value) * 8f * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
                            if (num2 > 0)
                            {
                                customParty.ItemRoster.AddToCounts(itemObject, num2);
                            }
                        }
                    }

                    customParty.Aggressiveness = 1f - 0.2f * MBRandom.RandomFloat;
                    customParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position, customParty.NavigationCapability);

                    ConsequenceRandomCaptivityChange(ref args, customParty.Party);
                }
            }
            catch (Exception e) { CECustomHandler.LogToFile("Failed ConsequenceCapturedByParty" + e); }
        }

        private void ConsequenceSoldEvents(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.PartyBelongedTo?.CurrentSettlement == null) return;
            ConsequenceSoldToSettlement(ref args);
            ConsequenceSoldToTradeShip(ref args);
            ConsequenceSoldToCaravan(ref args);
            ConsequenceSoldToNotable(ref args);
            ConsequenceSoldToLordParty(ref args);
        }

        private void ConsequenceRandomCaptivityChange(ref MenuCallbackArgs args, PartyBase party)
        {
            // TakePrisonerAction
            try
            {
                Hero prisonerCharacter = Hero.MainHero;

                if (prisonerCharacter.IsPrisoner)
                {
                    prisonerCharacter.PartyBelongedToAsPrisoner?.PrisonRoster.RemoveTroop(prisonerCharacter.CharacterObject, 1, default, 0);
                    prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
                    prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
                    party.AddPrisoner(prisonerCharacter.CharacterObject, 1);
                    if (prisonerCharacter == Hero.MainHero) PlayerCaptivity.StartCaptivity(party);
                }
                else
                {
                    prisonerCharacter.PartyBelongedTo?.MemberRoster.RemoveTroop(prisonerCharacter.CharacterObject, 1, default, 0);
                    prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
                    prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
                    party.AddPrisoner(prisonerCharacter.CharacterObject, 1);

                    if (prisonerCharacter == Hero.MainHero) PlayerCaptivity.StartCaptivity(party);
                }
                CEHelper.delayedEvents.Clear();
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Failed to exception: " + e.Message + " stacktrace: " + e.StackTrace);
            }
        }

        private void ConsequenceSoldToNotable(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable)) return;

            try
            {
                Settlement settlement = PartyBase.MainParty.MobileParty.CurrentSettlement;
                Hero notable = settlement.Notables.GetRandomElementWithPredicate(findFirstNotable => !findFirstNotable.IsFemale);
                //Hero notable = settlement.Notables.Where(findFirstNotable => !findFirstNotable.IsFemale).GetRandomElement();
                CECampaignBehavior.ExtraProps.Owner = notable;

                PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                ConsequenceRandomCaptivityChange(ref args, party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceSoldToTradeShip(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToTradeShip)) return;
            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan && mobileParty.IsCurrentlyAtSea);
                ConsequenceRandomCaptivityChange(ref args, party.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Trade Ship"); }
        }

        private void ConsequenceSoldToCaravan(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan && !mobileParty.IsCurrentlyAtSea);
                ConsequenceRandomCaptivityChange(ref args, party.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void ConsequenceSoldToLordParty(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty && !mobileParty.IsMainParty);
                ConsequenceRandomCaptivityChange(ref args, party.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void ConsequenceSoldToSettlement(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) return;

            try
            {
                PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                ConsequenceRandomCaptivityChange(ref args, party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceRemoveOwner()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveOwner)) return;
            try
            {
                CECampaignBehavior.ExtraProps.Owner = null;
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Remove Owner"); }
        }

        private void ConsequenceGainRandomPrisoners()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) _dynamics.CEGainRandomPrisoners(PartyBase.MainParty);
        }

        private void ConsequenceChangeClan()
        {
            if (_option.ClanOptions != null) _dynamics.ClanChange(_option.ClanOptions, Hero.MainHero, null);
        }

        private void ConsequenceChangeKingdom()
        {
            if (_option.KingdomOptions != null) _dynamics.KingdomChange(_option.KingdomOptions, Hero.MainHero, null);
        }

        private void ConsequenceImpregnation()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk)) return;

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.PregnancyRiskModifier))
                {
                    _impregnation.ImpregnationChance(Hero.MainHero, new CEVariablesLoader().GetIntFromXML(_option.PregnancyRiskModifier));
                }
                else if (!string.IsNullOrWhiteSpace(_listedEvent.PregnancyRiskModifier))
                {
                    _impregnation.ImpregnationChance(Hero.MainHero, new CEVariablesLoader().GetIntFromXML(_listedEvent.PregnancyRiskModifier));
                }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.ImpregnationChance(Hero.MainHero, 30);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
        }

        #endregion Consequences

        #region Requirements

        #region ReqGold

        private void ReqGold(ref MenuCallbackArgs args)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqGoldAbove)) ReqGoldAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed "); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqGoldBelow)) ReqGoldBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed "); }
        }

        private void ReqGoldBelow(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.Gold > new CEVariablesLoader().GetIntFromXML(_option.ReqGoldBelow))
            {
                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
                args.IsEnabled = false;
            }
        }

        private void ReqGoldAbove(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.Gold >= new CEVariablesLoader().GetIntFromXML(_option.ReqGoldAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqGold

        #region ReqTrait

        private void ReqTrait(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTrait)) return;
            int traitLevel;

            try
            {
                traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.All.Single((TraitObject traitObject) => traitObject.StringId == _option.ReqHeroTrait));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captive");
                traitLevel = 0;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroTraitLevelAbove)) ReqHeroTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove"); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroTraitLevelBelow)) ReqHeroTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow"); }
        }

        private void ReqHeroTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (traitLevel <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTraitLevelBelow)) return;
            TextObject text = GameTexts.FindText("str_CE_trait_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (traitLevel >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTraitLevelAbove)) return;
            TextObject text = GameTexts.FindText("str_CE_trait_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        #endregion ReqTrait

        #region ReqHeroSkills

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

                try
                {

                    int skillLevel = Hero.MainHero.GetSkillValue(foundSkill);

                    try
                    {
                        if (ReqSkillsLevelAbove(ref args, foundSkill, skillLevel, skillRequired.Min)) break;
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid SkillRequiredAbove"); }

                    try
                    {
                        if (ReqSkillsLevelBelow(ref args, foundSkill, skillLevel, skillRequired.Max)) break;
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid SkillRequiredBelow"); }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid GetSkillValue"); }
            }
        }

        private bool ReqSkillsLevelBelow(ref MenuCallbackArgs args, SkillObject skillRequired, int skillLevel, string max)
        {
            if (string.IsNullOrWhiteSpace(max)) return false;
            if (skillLevel <= new CEVariablesLoader().GetIntFromXML(max)) return false;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
            text.SetTextVariable("SKILL", skillRequired.Name);
            args.Tooltip = text;
            args.IsEnabled = false;

            return true;
        }

        private bool ReqSkillsLevelAbove(ref MenuCallbackArgs args, SkillObject skillRequired, int skillLevel, string min)
        {
            if (string.IsNullOrWhiteSpace(min)) return false;
            if (skillLevel >= new CEVariablesLoader().GetIntFromXML(min)) return false;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
            text.SetTextVariable("SKILL", skillRequired.Name);
            args.Tooltip = text;
            args.IsEnabled = false;

            return true;
        }

        #endregion ReqHeroSkills

        #region ReqHeroSkill

        private void ReqHeroSkill(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroSkill)) return;
            int skillLevel = 0;

            try
            {
                SkillObject foundSkill = CESkills.FindSkill(_option.ReqHeroSkill);
                if (foundSkill == null)
                    CECustomHandler.LogToFile("Invalid Skill");
                else
                    skillLevel = Hero.MainHero.GetSkillValue(foundSkill);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill");
                skillLevel = 0;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroSkillLevelAbove)) ReqHeroSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove"); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroSkillLevelBelow)) ReqHeroSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow"); }
        }

        private void ReqHeroSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (skillLevel <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroSkillLevelBelow)) return;
            TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (skillLevel >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroSkillLevelAbove)) return;
            TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        #endregion ReqHeroSkill

        #region ReqProstitute

        private void ReqProstitute(ref MenuCallbackArgs args)
        {
            int prostitute = Hero.MainHero.GetSkillValue(CESkills.Prostitution);

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroProstituteLevelAbove)) ReqHeroProstituteLevelAbove(ref args, prostitute);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroProstituteLevelBelow)) ReqHeroProstituteLevelBelow(ref args, prostitute);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelBelow / Failed "); }
        }

        private void ReqHeroProstituteLevelBelow(ref MenuCallbackArgs args, int prostitute)
        {
            if (prostitute <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroProstituteLevelBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroProstituteLevelAbove(ref MenuCallbackArgs args, int prostitute)
        {
            if (prostitute >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroProstituteLevelAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqProstitute

        #region ReqSlavery

        private void ReqSlavery(ref MenuCallbackArgs args)
        {
            int slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroSlaveLevelAbove)) ReqHeroSlaveLevelAbove(ref args, slave);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroSlaveLevelBelow)) ReqHeroSlaveLevelBelow(ref args, slave);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelBelow / Failed "); }
        }

        private void ReqHeroSlaveLevelBelow(ref MenuCallbackArgs args, int slave)
        {
            if (slave <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroSlaveLevelBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroSlaveLevelAbove(ref MenuCallbackArgs args, int slave)
        {
            if (slave >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroSlaveLevelAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqSlavery

        #region ReqHeroHealthPercentage

        private void ReqHeroHealthPercentage(ref MenuCallbackArgs args)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroHealthAbovePercentage)) ReqHeroHealthAbovePercentage(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthAbovePercentage / Failed "); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqHeroHealthBelowPercentage)) ReqHeroHealthBelowPercentage(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthBelowPercentage / Failed "); }
        }

        private void ReqHeroHealthBelowPercentage(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.HitPoints <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroHealthBelowPercentage)) return;
            args.Tooltip = GameTexts.FindText("str_CE_health", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroHealthAbovePercentage(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.HitPoints >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroHealthAbovePercentage)) return;
            args.Tooltip = GameTexts.FindText("str_CE_health", "low");
            args.IsEnabled = false;
        }

        #endregion ReqHeroHealthPercentage

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
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqFemaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleCaptivesAbove)) return;

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
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleCaptivesAbove)) return;

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
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroCaptivesBelow)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroCaptivesAbove)) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroCaptivesAbove)) return;

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
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleTroopsBelow)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqFemaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroFemaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleTroopsAbove)) return;

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
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleTroopsBelow)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqMaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroMaleTroopsAbove)) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleTroopsAbove)) return;

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
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; })) <= new CEVariablesLoader().GetIntFromXML(_option.ReqTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroTroopsBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTroopsBelow)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; })) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqTroopsAbove)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; })) >= new CEVariablesLoader().GetIntFromXML(_option.ReqTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroTroopsAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrWhiteSpace(_option.ReqHeroTroopsAbove)) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; })) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqTroops

        #region ReqMorale

        private void ReqMorale(ref MenuCallbackArgs args)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqMoraleAbove)) ReqMoraleAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed "); }

            try
            {
                if (!string.IsNullOrWhiteSpace(_option.ReqMoraleBelow)) ReqMoraleBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed "); }
        }

        private void ReqMoraleBelow(ref MenuCallbackArgs args)
        {
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale > new CEVariablesLoader().GetIntFromXML(_option.ReqMoraleBelow))) return;
            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMoraleAbove(ref MenuCallbackArgs args)
        {
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale < new CEVariablesLoader().GetIntFromXML(_option.ReqMoraleAbove))) return;
            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
            args.IsEnabled = false;
        }

        #endregion ReqMorale

        #endregion Requirements

        #region Init Options

        private void InitChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrWhiteSpace(_option.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrWhiteSpace(_listedEvent.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");
                MBTextManager.SetTextVariable("MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        private void InitGiveGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;
            int content = _score.AttractivenessScore(Hero.MainHero);
            content *= _option.MultipleRestrictedListOfConsequences.Count(consquence => { return consquence == RestrictedListOfConsequences.GiveGold; });
            MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
        }

        private void InitSoldToLordParty()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) return;

            try
            {
                MobileParty party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty && !mobileParty.IsMainParty; });
                if (party != null) MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void InitSoldToTradeShip()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToTradeShip)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan && mobileParty.IsCurrentlyAtSea; });
                if (party != null) MBTextManager.SetTextVariable("BUYERTRADESHIP", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Ship"); }
        }

        private void InitRemoveOwner()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveOwner)) return;

            try
            {
                if (CECampaignBehavior.ExtraProps.Owner != null) MBTextManager.SetTextVariable("PREVIOUSOWNER", CECampaignBehavior.ExtraProps.Owner.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Owner"); }
        }


        private void InitSoldToCaravan()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan && !mobileParty.IsCurrentlyAtSea; });
                if (party != null) MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void InitSoldToSettlement()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) return;

            try
            {
                PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        #endregion Init Options

        #region CustomConsequencesReq

        private void PlayerHasOpenSpaceForCompanions(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerAllowedCompanion)) return;
            if (!(Clan.PlayerClan.Companions.Count() >= Clan.PlayerClan.CompanionLimit)) return;

            args.Tooltip = GameTexts.FindText("str_CE_companions_too_many");
            args.IsEnabled = false;
        }

        private void PlayerIsNotBusy(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerIsNotBusy)) return;
            if (PlayerEncounter.Current == null) return;

            args.Tooltip = GameTexts.FindText("str_CE_busy_right_now");
            args.IsEnabled = false;
        }

        #endregion CustomConsequencesReq
    }
}