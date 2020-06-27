using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    public class CaptiveMenuCallBackDelegate
    {
        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;

        private readonly VariablesLoader _variables = new VariablesLoader();
        private readonly Dynamics _dynamics = new Dynamics();
        private readonly ScoresCalculation _score = new ScoresCalculation();
        private readonly ImpregnationSystem _impregnation = new ImpregnationSystem();
        private readonly CaptiveSpecifics _captive = new CaptiveSpecifics();

        internal CaptiveMenuCallBackDelegate(CEEvent listedEvent)
        {
            _listedEvent = listedEvent;
        }

        internal CaptiveMenuCallBackDelegate(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
        }

        internal void CaptiveEventWaitGameMenu(MenuCallbackArgs args)
        {
            args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                       ? "wait_prisoner_female"
                                                       : "wait_prisoner_male");

            //LoadBackgroundImage();
            new SharedCallBackHelper(_listedEvent, _option).LoadBackgroundImage("default_random");

            if (PlayerCaptivity.IsCaptive) SetCaptiveTextVariables(ref args);

            args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
        }

        internal bool CaptiveConditionWaitGameMenu(MenuCallbackArgs args)
        {
            return true;
        }

        internal void CaptiveConsequenceWaitGameMenu(MenuCallbackArgs args) { }

        internal void CaptiveTickWaitGameMenu(MenuCallbackArgs args, CampaignTime dt) //Warning: dt unused.
        {
            var captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;
            var text = args.MenuContext.GameMenu.GetText();

            SetCaptiveTimeInDays(captiveTimeInDays, ref text);

            if (!PlayerCaptivity.IsCaptive) return;

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null) text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Name);
            else if (PlayerCaptivity.CaptorParty.IsSettlement) text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);
            else text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.IsActive) PartyBase.MainParty.MobileParty.Position2D = PlayerCaptivity.CaptorParty.MobileParty.Position2D;
            else if (PlayerCaptivity.CaptorParty.IsSettlement) PartyBase.MainParty.MobileParty.Position2D = PlayerCaptivity.CaptorParty.Settlement.GatePosition;
            PlayerCaptivity.CaptorParty.SetAsCameraFollowParty();

            var eventToRun = Campaign.Current.Models.PlayerCaptivityModel.CheckCaptivityChange(Campaign.Current.CampaignDt);
            if (!eventToRun.IsStringNoneOrEmpty()) GameMenu.SwitchToMenu(eventToRun);
        }

        internal void CaptiveEventGameMenu(MenuCallbackArgs args)
        {
            new SharedCallBackHelper(_listedEvent, _option).LoadBackgroundImage("captive_default");

            SetCaptiveTextVariables(ref args);
        }

        internal bool CaptiveEventOptionGameMenu(MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) args.optionLeaveType = GameMenuOption.LeaveType.Escape;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) args.optionLeaveType = GameMenuOption.LeaveType.Leave;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) SetGiveGold();
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) SetChangeGold();
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) SetChangeCaptorGold();
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) SetSoldToSettlement();
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) SetSoldToCaravan();
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) SetSoldToLordParty();

            SetLeaveType(ref args);

            ReqMorale(ref args);
            ReqTroops(ref args);
            ReqMaleTroops(ref args);
            ReqFemaleTroops(ref args);
            ReqCaptives(ref args);
            ReqMaleCaptives(ref args);
            ReqFemaleCaptives(ref args);
            if (PlayerCaptivity.CaptorParty.LeaderHero != null) ReqHeroCaptorRelation(ref args);
            ReqHeroHealthPercentage(ref args);
            ReqSlavery(ref args);
            ReqProstitute(ref args);
            ReqTrait(ref args);
            ReqCaptorTrait(ref args);
            ReqSkill(ref args);
            ReqCaptorSkill(ref args);
            ReqGold(ref args);

            return true;
        }

        internal void CaptiveEventOptionConsequenceGameMenu(MenuCallbackArgs args)
        {
            var h = new SharedCallBackHelper(_listedEvent, _option);

            //h.ProceedToSharedCallBacks();
            h.ConsequenceXP();
            h.ConsequenceLeaveSpouse();
            h.ConsequenceGold();
            h.ConsequenceChangeGold();
            h.ConsequenceChangeTrait();
            h.ConsequenceChangeSkill();
            h.ConsequenceSlaveryLevel();
            h.ConsequenceSlaveryFlags();
            h.ConsequenceProstitutionLevel();
            h.ConsequenceProstitutionFlags();
            h.ConsequenceRenown();
            h.ConsequenceChangeHealth();
            h.ConsequenceChangeMorale();

            ConsequenceForceMarry();
            ConsequenceChangeClan();
            ConsequenceImpregnationByLeader();
            ConsequenceImpregnation();

            ConsequenceSpecificCaptor();
            ConsequenceSoldEvents(ref args);
            ConsequenceGainRandomPrisoners();
            ConsequenceEscaping();

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor) && PlayerCaptivity.CaptorParty.NumberOfAllMembers == 1) ConsequenceKillCaptor();
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner)) _dynamics.CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero);
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0) ConsequenceRandomEventTrigger(ref args);
            else if (!string.IsNullOrEmpty(_option.TriggerEventName)) ConsequenceSingleEventTrigger(ref args);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape)) ConsequenceEscapeEventTrigger(ref args);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) _captive.CECaptivityEscape(ref args);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) _captive.CECaptivityLeave(ref args);
            else _captive.CECaptivityContinue(ref args);
        }

        

#region private

        private static void ConsequenceEscaping()
        {
            if (!CESettings.Instance.SexualContent)
                GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                          ? "CE_captivity_escape_success"
                                          : "CE_captivity_escape_success_male");
            else
                GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                          ? "CE_captivity_sexual_escape_success"
                                          : "CE_captivity_sexual_escape_success_male");
        }

        private void ConsequenceKillCaptor()
        {
            

            if (PlayerCaptivity.CaptorParty.LeaderHero != null) KillCharacterAction.ApplyByMurder(PlayerCaptivity.CaptorParty.LeaderHero, Hero.MainHero);

            if (PlayerCaptivity.CaptorParty.IsMobile) DestroyPartyAction.Apply(null, PlayerCaptivity.CaptorParty.MobileParty);
        }

        private void ConsequenceRandomEventTrigger(ref MenuCallbackArgs args)
        {
            var eventNames = new List<CEEvent>();

            try
            {
                foreach (var triggerEvent in _option.TriggerEvents)
                {
                    var triggeredEvent = _eventList.Find(item => item.Name == triggerEvent.EventName);

                    if (triggeredEvent == null)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");

                        continue;
                    }

                    if (!triggerEvent.EventUseConditions.IsStringNoneOrEmpty() && triggerEvent.EventUseConditions == "True")
                    {
                        var conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);

                        if (conditionMatched != null)
                        {
                            CECustomHandler.LogToFile(conditionMatched);

                            continue;
                        }
                    }

                    var weightedChance = 1;

                    try
                    {
                        weightedChance = _variables.GetIntFromXML(!triggerEvent.EventWeight.IsStringNoneOrEmpty()
                                                                      ? triggerEvent.EventWeight
                                                                      : triggeredEvent.WeightedChanceOfOccuring);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }

                    for (var a = weightedChance; a > 0; a--) eventNames.Add(triggeredEvent);
                }

                if (eventNames.Count > 0)
                {
                    var number = MBRandom.Random.Next(0, eventNames.Count - 1);

                    try
                    {
                        var triggeredEvent = eventNames[number];
                        triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                        GameMenu.SwitchToMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                        _captive.CECaptivityContinue(ref args);
                    }
                }
                else { _captive.CECaptivityContinue(ref args); }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                _captive.CECaptivityContinue(ref args);
            }
        }

        private void ConsequenceSingleEventTrigger(ref MenuCallbackArgs args)
        {
            try
            {
                var triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                GameMenu.SwitchToMenu(triggeredEvent.Name);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Couldn't find " + _option.TriggerEventName + " in events.");
                _captive.CECaptivityContinue(ref args);
            }
        }

        private void ConsequenceEscapeEventTrigger(ref MenuCallbackArgs args)
        {
            try
            {
                _captive.CECaptivityEscapeAttempt(ref args, !string.IsNullOrEmpty(_option.EscapeChance)
                                                      ? _variables.GetIntFromXML(_option.EscapeChance)
                                                      : _variables.GetIntFromXML(_listedEvent.EscapeChance));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing EscapeChance");
                _captive.CECaptivityEscapeAttempt(ref args);
            }
        }

        private void ConsequenceGainRandomPrisoners()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) _dynamics.CEGainRandomPrisoners(PlayerCaptivity.CaptorParty);

            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor) || PlayerCaptivity.CaptorParty.NumberOfAllMembers <= 1) return;

            if (PlayerCaptivity.CaptorParty.LeaderHero != null) KillCharacterAction.ApplyByMurder(PlayerCaptivity.CaptorParty.LeaderHero, Hero.MainHero);
            else PlayerCaptivity.CaptorParty.MemberRoster.AddToCounts(PlayerCaptivity.CaptorParty.Leader, -1);
        }

        private void ConsequenceSoldEvents(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) ConsequenceSoldToCaravan(ref args);
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) ConsequenceSoldToSettlement(ref args);
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) ConsequenceSoldToLordParty(ref args);
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable)) ConsequenceSoldToNotable(ref args); // Work In Progress Sold Event
        }

        private void ConsequenceSoldToNotable(ref MenuCallbackArgs args)
        {
            try
            {
                var settlement = PlayerCaptivity.CaptorParty.IsSettlement
                    ? PlayerCaptivity.CaptorParty.Settlement
                    : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement;

                var notable = settlement.Notables.Where(findFirstNotable => !findFirstNotable.IsFemale).GetRandomElement();
                CECampaignBehavior.ExtraProps.Owner = notable;
                _captive.CECaptivityChange(ref args, settlement.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceSoldToLordParty(ref MenuCallbackArgs args)
        {
            try
            {
                MobileParty party = null;

                party = PlayerCaptivity.CaptorParty.IsSettlement
                    ? PlayerCaptivity.CaptorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty)
                    : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty);

                if (party == null) return;

                _captive.CECaptivityChange(ref args, party.Party);
                CECampaignBehavior.ExtraProps.Owner = party.LeaderHero;
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void ConsequenceSoldToSettlement(ref MenuCallbackArgs args)
        {
            try
            {
                var party = !PlayerCaptivity.CaptorParty.IsSettlement
                    ? PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Party
                    : PlayerCaptivity.CaptorParty;

                CECampaignBehavior.ExtraProps.Owner = null;
                _captive.CECaptivityChange(ref args, party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceSoldToCaravan(ref MenuCallbackArgs args)
        {
            try
            {
                MobileParty party = null;

                if (PlayerCaptivity.CaptorParty.IsSettlement)
                {
                    CECampaignBehavior.ExtraProps.Owner = null;
                    party = PlayerCaptivity.CaptorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan);
                }
                else
                {
                    CECampaignBehavior.ExtraProps.Owner = null;
                    party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan);
                }

                if (party != null) _captive.CECaptivityChange(ref args, party.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void ConsequenceSpecificCaptor()
        {
            if (PlayerCaptivity.CaptorParty.IsSettlement || !PlayerCaptivity.CaptorParty.IsMobile || PlayerCaptivity.CaptorParty.MobileParty.LeaderHero == null) return;

            ConsequenceSpecificCaptorRelations();
            ConsequenceSpecificCaptorGold();
            ConsequenceSpecificCaptorChangeGold();
            ConsequenceSpecificCaptorTrait();
            ConsequenceSpecificCaptorRenown();
        }

        private void ConsequenceSpecificCaptorRenown()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown)) return;

            try
            {
                _dynamics.RenownModifier(!string.IsNullOrEmpty(_option.RenownTotal)
                                             ? _variables.GetIntFromXML(_option.RenownTotal)
                                             : _variables.GetIntFromXML(_listedEvent.RenownTotal), PlayerCaptivity.CaptorParty.LeaderHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RenownTotal");
                _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty.LeaderHero);
            }
        }

        private void ConsequenceSpecificCaptorTrait()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait)) return;

            try
            {
                var level = _variables.GetIntFromXML(!string.IsNullOrEmpty(_option.TraitTotal)
                                                         ? _option.TraitTotal
                                                         : _listedEvent.TraitTotal);

                _dynamics.TraitModifier(PlayerCaptivity.CaptorParty.LeaderHero, !string.IsNullOrEmpty(_option.TraitToLevel)
                                            ? _option.TraitToLevel
                                            : _listedEvent.TraitToLevel, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing Trait Flags"); }
        }

        private void ConsequenceSpecificCaptorChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) return;

            try
            {
                var level = 0;

                if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = _variables.GetIntFromXML(_option.CaptorGoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = _variables.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                else CECustomHandler.LogToFile("Missing CaptorGoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }
        }

        private void ConsequenceSpecificCaptorGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold)) return;

            var content = _score.AttractivenessScore(Hero.MainHero);
            var currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
            GiveGoldAction.ApplyBetweenCharacters(null, PlayerCaptivity.CaptorParty.LeaderHero, content);
        }

        private void ConsequenceSpecificCaptorRelations()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation)) return;

            try
            {
                _dynamics.RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, !string.IsNullOrEmpty(_option.RelationTotal)
                                                ? _variables.GetIntFromXML(_option.RelationTotal)
                                                : _variables.GetIntFromXML(_listedEvent.RelationTotal));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RelationTotal");
                _dynamics.RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, MBRandom.RandomInt(-5, 5));
            }
        }

        private void ConsequenceImpregnation()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { _impregnation.CaptivityImpregnationChance(Hero.MainHero, _variables.GetIntFromXML(_option.PregnancyRiskModifier), false, false); }
                else if (!string.IsNullOrEmpty(_listedEvent.PregnancyRiskModifier)) { _impregnation.CaptivityImpregnationChance(Hero.MainHero, _variables.GetIntFromXML(_listedEvent.PregnancyRiskModifier), false, false); }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(Hero.MainHero, 30, false, false);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
        }

        private void ConsequenceImpregnationByLeader()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { _impregnation.CaptivityImpregnationChance(Hero.MainHero, _variables.GetIntFromXML(_option.PregnancyRiskModifier)); }
                else if (!string.IsNullOrEmpty(_listedEvent.PregnancyRiskModifier)) { _impregnation.CaptivityImpregnationChance(Hero.MainHero, _variables.GetIntFromXML(_listedEvent.PregnancyRiskModifier)); }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(Hero.MainHero, 30);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
        }

        
        
        

        

        

        

        private void ConsequenceChangeClan()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeClan)) return;

            if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.LeaderHero != null) _dynamics.ChangeClan(Hero.MainHero, PlayerCaptivity.CaptorParty.LeaderHero);
        }

        private void ConsequenceForceMarry()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor)) return;

            if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.LeaderHero != null) _dynamics.ChangeSpouse(Hero.MainHero, PlayerCaptivity.CaptorParty.LeaderHero);
        }

        

        

        private void ReqGold(ref MenuCallbackArgs args)
        {
            try
            {
                if (!string.IsNullOrEmpty(_option.ReqGoldAbove)) ReqGoldAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqGoldBelow)) ReqGoldBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed "); }
        }

        private void ReqGoldBelow(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.Gold <= _variables.GetIntFromXML(_option.ReqGoldBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
            args.IsEnabled = false;
        }

        private void ReqGoldAbove(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.Gold >= _variables.GetIntFromXML(_option.ReqGoldAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
            args.IsEnabled = false;
        }

        private void ReqCaptorSkill(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptorSkill.IsStringNoneOrEmpty()) return;

            if (PlayerCaptivity.CaptorParty.LeaderHero == null) args.IsEnabled = false;
            var skillLevel = 0;

            try { skillLevel = PlayerCaptivity.CaptorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqCaptorSkill)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill Captor");
                skillLevel = 0;
            }

            try
            {
                if (!_option.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty()) ReqCaptorSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove"); }

            try
            {
                if (_option.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty()) ReqCaptorSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow"); }
        }

        private void ReqCaptorSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (skillLevel <= _variables.GetIntFromXML(_option.ReqCaptorSkillLevelBelow)) return;

            var text = GameTexts.FindText("str_CE_skill_captor_level", "high");
            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (skillLevel >= _variables.GetIntFromXML(_option.ReqCaptorSkillLevelAbove)) return;

            var text = GameTexts.FindText("str_CE_skill_captor_level", "low");
            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqSkill(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroSkill.IsStringNoneOrEmpty()) return;

            var skillLevel = 0;

            try { skillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqHeroSkill)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill Captive");
                skillLevel = 0;
            }

            try
            {
                if (!_option.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty()) ReqHeroSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove"); }

            try
            {
                if (!_option.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty()) ReqHeroSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow"); }
        }

        private void ReqHeroSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (skillLevel <= _variables.GetIntFromXML(_option.ReqHeroSkillLevelBelow)) return;

            var text = GameTexts.FindText("str_CE_skill_level", "high");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (skillLevel >= _variables.GetIntFromXML(_option.ReqHeroSkillLevelAbove)) return;

            var text = GameTexts.FindText("str_CE_skill_level", "low");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorTrait(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptorTrait.IsStringNoneOrEmpty()) return;

            if (PlayerCaptivity.CaptorParty.LeaderHero == null) args.IsEnabled = false;
            var traitLevel = 0;

            try { traitLevel = PlayerCaptivity.CaptorParty.LeaderHero.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captor");
                traitLevel = 0;
            }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqCaptorTraitLevelAbove)) ReqCaptorTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove"); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqCaptorTraitLevelBelow)) ReqCaptorTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow"); }
        }

        private void ReqCaptorTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (traitLevel <= _variables.GetIntFromXML(_option.ReqCaptorTraitLevelBelow)) return;

            var text = GameTexts.FindText("str_CE_trait_captor_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (traitLevel >= _variables.GetIntFromXML(_option.ReqCaptorTraitLevelAbove)) return;

            var text = GameTexts.FindText("str_CE_trait_captor_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqTrait(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroTrait.IsStringNoneOrEmpty()) return;

            var traitLevel = 0;

            try { traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captive");
                traitLevel = 0;
            }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroTraitLevelAbove)) ReqHeroTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove"); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroTraitLevelBelow)) ReqHeroTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow"); }
        }

        private void ReqHeroTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (traitLevel <= _variables.GetIntFromXML(_option.ReqHeroTraitLevelBelow)) return;

            var text = GameTexts.FindText("str_CE_trait_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (traitLevel >= _variables.GetIntFromXML(_option.ReqHeroTraitLevelAbove)) return;

            var text = GameTexts.FindText("str_CE_trait_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqProstitute(ref MenuCallbackArgs args)
        {
            var prostitute = Hero.MainHero.GetSkillValue(CESkills.Prostitution);

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroProstituteLevelAbove)) ReqHeroProstituteLevelAbove(ref args, prostitute);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroProstituteLevelBelow)) ReqHeroProstituteLevelBelow(ref args, prostitute);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelBelow / Failed "); }
        }

        private void ReqHeroProstituteLevelBelow(ref MenuCallbackArgs args, int prostitute)
        {
            if (prostitute <= _variables.GetIntFromXML(_option.ReqHeroProstituteLevelBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroProstituteLevelAbove(ref MenuCallbackArgs args, int prostitute)
        {
            if (prostitute >= _variables.GetIntFromXML(_option.ReqHeroProstituteLevelAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "low");
            args.IsEnabled = false;
        }

        private void SetGiveGold()
        {
            var content = new ScoresCalculation().AttractivenessScore(Hero.MainHero);
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
        }

        private void SetChangeGold()
        {
            try
            {
                var level = 0;

                if (!string.IsNullOrEmpty(_option.GoldTotal)) level = _variables.GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = _variables.GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");
                MBTextManager.SetTextVariable("MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        private void SetChangeCaptorGold()
        {
            try
            {
                var level = 0;

                if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = _variables.GetIntFromXML(_option.CaptorGoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = _variables.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                else CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }
        }

        private static void SetSoldToSettlement()
        {
            try
            {
                var party = !PlayerCaptivity.CaptorParty.IsSettlement
                    ? PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Party
                    : PlayerCaptivity.CaptorParty;

                MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private static void SetSoldToCaravan()
        {
            try
            {
                var party = PlayerCaptivity.CaptorParty.IsSettlement
                    ? PlayerCaptivity.CaptorParty.Settlement.Parties.First(mobileParty => mobileParty.IsCaravan).Party
                    : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => mobileParty.IsCaravan).Party;

                MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private static void SetSoldToLordParty()
        {
            try
            {
                var party = PlayerCaptivity.CaptorParty.IsSettlement
                    ? PlayerCaptivity.CaptorParty.Settlement.Parties.First(mobileParty => mobileParty.IsLordParty).Party
                    : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => mobileParty.IsLordParty).Party;

                MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void SetLeaveType(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait)) args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade)) args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe)) args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.BribeAndEscape)) args.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;
        }

        private void ReqSlavery(ref MenuCallbackArgs args)
        {
            var slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroSlaveLevelAbove)) SetReqHeroSlaveLevelAbove(ref args, slave);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroSlaveLevelBelow)) SetReqHeroSlaveLevelBelow(ref args, slave);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelBelow / Failed "); }
        }

        private void SetReqHeroSlaveLevelBelow(ref MenuCallbackArgs args, int slave)
        {
            if (slave <= _variables.GetIntFromXML(_option.ReqHeroSlaveLevelBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqHeroSlaveLevelAbove(ref MenuCallbackArgs args, int slave)
        {
            if (slave >= _variables.GetIntFromXML(_option.ReqHeroSlaveLevelAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroHealthPercentage(ref MenuCallbackArgs args)
        {
            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroHealthAbovePercentage)) SetReqHeroHealthAbovePercentage(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthAbovePercentage / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroHealthBelowPercentage)) SetReqHeroHealthBelowPercentage(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthBelowPercentage / Failed "); }
        }

        private void ReqHeroCaptorRelation(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqHeroCaptorRelationAbove.IsStringNoneOrEmpty()) SetReqHeroCaptorRelationAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationAbove / Failed "); }

            try
            {
                if (!_option.ReqHeroCaptorRelationBelow.IsStringNoneOrEmpty()) SetReqHeroCaptorRelationBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationBelow / Failed "); }
        }

        private void ReqFemaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty()) SetReqFemaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) SetReqFemaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed "); }
        }

        private void ReqMaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqMaleCaptivesAbove.IsStringNoneOrEmpty()) SetReqMaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) SetReqMaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed "); }
        }

        private void ReqCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqCaptivesAbove.IsStringNoneOrEmpty()) SetReqCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqCaptivesBelow.IsStringNoneOrEmpty()) SetReqCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed "); }
        }

        private void ReqFemaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqFemaleTroopsAbove.IsStringNoneOrEmpty()) SetReqFemaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleTroopsBelow.IsStringNoneOrEmpty()) SetReqFemaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed "); }
        }

        private void ReqMaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqMaleTroopsAbove.IsStringNoneOrEmpty()) SetReqMaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) SetReqMaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed "); }
        }

        private void ReqTroops(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqTroopsAbove.IsStringNoneOrEmpty()) SetReqTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqTroopsBelow.IsStringNoneOrEmpty()) SetReqTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed "); }
        }

        private void ReqMorale(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqMoraleAbove.IsStringNoneOrEmpty()) SetReqMoraleAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoralAbove / Failed "); }

            try
            {
                if (!_option.ReqMoraleBelow.IsStringNoneOrEmpty()) SetReqMoraleBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoralBelow / Failed "); }
        }

        private void SetReqHeroHealthBelowPercentage(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.HitPoints <= _variables.GetIntFromXML(_option.ReqHeroHealthBelowPercentage)) return;

            args.Tooltip = GameTexts.FindText("str_CE_health", "high");
            args.IsEnabled = false;
        }

        private void SetReqHeroHealthAbovePercentage(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.HitPoints >= _variables.GetIntFromXML(_option.ReqHeroHealthAbovePercentage)) return;

            args.Tooltip = GameTexts.FindText("str_CE_health", "low");
            args.IsEnabled = false;
        }

        private void SetReqHeroCaptorRelationBelow(ref MenuCallbackArgs args)
        {
            if (!(PlayerCaptivity.CaptorParty.LeaderHero.GetRelationWithPlayer() > _variables.GetFloatFromXML(_option.ReqHeroCaptorRelationBelow))) return;

            var textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
            textResponse3.SetTextVariable("HERO", PlayerCaptivity.CaptorParty.LeaderHero.Name.ToString());
            args.Tooltip = textResponse3;
            args.IsEnabled = false;
        }

        private void SetReqHeroCaptorRelationAbove(ref MenuCallbackArgs args)
        {
            if (!(PlayerCaptivity.CaptorParty.LeaderHero.GetRelationWithPlayer() < _variables.GetFloatFromXML(_option.ReqHeroCaptorRelationAbove))) return;

            var textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
            textResponse4.SetTextVariable("HERO", PlayerCaptivity.CaptorParty.LeaderHero.Name.ToString());
            args.Tooltip = textResponse4;
            args.IsEnabled = false;
        }

        private void SetReqFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) <= _variables.GetIntFromXML(_option.ReqFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqFemaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void SetReqMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void SetReqCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.NumberOfPrisoners <= _variables.GetIntFromXML(_option.ReqCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.NumberOfPrisoners >= _variables.GetIntFromXML(_option.ReqCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void SetReqFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) <= _variables.GetIntFromXML(_option.ReqFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void SetReqMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleTroopsBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }


        private void SetReqTroopsBelow(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.NumberOfHealthyMembers <= _variables.GetIntFromXML(_option.ReqTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqTroopsAbove(ref MenuCallbackArgs args)
        {
            if (PlayerCaptivity.CaptorParty.NumberOfHealthyMembers >= _variables.GetIntFromXML(_option.ReqTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void SetReqMoraleBelow(ref MenuCallbackArgs args)
        {
            if (!PlayerCaptivity.CaptorParty.IsMobile || !(PlayerCaptivity.CaptorParty.MobileParty.Morale > _variables.GetIntFromXML(_option.ReqMoraleBelow))) return;

            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
            args.IsEnabled = false;
        }

        private void SetReqMoraleAbove(ref MenuCallbackArgs args)
        {
            if (!PlayerCaptivity.CaptorParty.IsMobile || !(PlayerCaptivity.CaptorParty.MobileParty.Morale < _variables.GetIntFromXML(_option.ReqMoraleAbove))) return;

            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
            args.IsEnabled = false;
        }

        private void SetCaptiveTimeInDays(int captiveTimeInDays, ref TextObject text)
        {
            if (captiveTimeInDays != 0)
            {
                text.SetTextVariable("DAYS", 1);
                text.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);

                text.SetTextVariable("PLURAL", captiveTimeInDays > 1
                                         ? 1
                                         : 0);
            }
            else { text.SetTextVariable("DAYS", 0); }
        }

        private void SetCaptiveTextVariables(ref MenuCallbackArgs args)
        {
            if (!PlayerCaptivity.IsCaptive) return;
            
            var captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;
            var text = args.MenuContext.GameMenu.GetText();

            if (PlayerCaptivity.CaptorParty.Leader != null)
            {
                text.SetTextVariable("CAPTOR_NAME", PlayerCaptivity.CaptorParty.Leader.Name);

                text.SetTextVariable("ISCAPTORFEMALE", PlayerCaptivity.CaptorParty.Leader.IsFemale
                                         ? 1
                                         : 0);
            }
            else
            {
                text.SetTextVariable("CAPTOR_NAME", new TextObject("{=CESETTINGS0099}captor"));
                text.SetTextVariable("ISCAPTORFEMALE", 0);
            }

            text.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale
                                     ? 1
                                     : 0);

            if (CECampaignBehavior.ExtraProps.Owner != null) text.SetTextVariable("OWNER_NAME", CECampaignBehavior.ExtraProps.Owner.Name);

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null) text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Name);
            else if (PlayerCaptivity.CaptorParty.IsSettlement) text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);

            if (PlayerCaptivity.CaptorParty.IsMobile)
            {
                text.SetTextVariable("ISCARAVAN", PlayerCaptivity.CaptorParty.MobileParty.IsCaravan
                                         ? 1
                                         : 0);

                text.SetTextVariable("ISBANDITS", PlayerCaptivity.CaptorParty.MobileParty.IsBandit || PlayerCaptivity.CaptorParty.MobileParty.IsBanditBossParty
                                         ? 1
                                         : 0);

                text.SetTextVariable("ISLORDPARTY", PlayerCaptivity.CaptorParty.MobileParty.IsLordParty
                                         ? 1
                                         : 0);

                text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);
            }

            SetCaptiveTimeInDays(captiveTimeInDays, ref text);
        }

        /*private void LoadBackgroundImage()
        {
            try
            {
                var backgroundName = _listedEvent.BackgroundName;

                if (!backgroundName.IsStringNoneOrEmpty())
                {
                    CESubModule.animationPlayEvent = false;
                    CESubModule.LoadTexture(backgroundName);
                }
                else if (_listedEvent.BackgroundAnimation != null && _listedEvent.BackgroundAnimation.Count > 0)
                {
                    CESubModule.animationImageList = _listedEvent.BackgroundAnimation;
                    CESubModule.animationIndex = 0;
                    CESubModule.animationPlayEvent = true;
                    var speed = 0.03f;

                    try
                    {
                        if (!_listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = _variables.GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e) { CECustomHandler.ForceLogToFile("Failed to load BackgroundAnimationSpeed for " + _listedEvent.Name + " : Exception: " + e); }

                    CESubModule.animationSpeed = speed;
                }
                else { CESubModule.animationPlayEvent = false; }
            }
            catch (Exception) { CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name); }
        }*/

#endregion
    }
}