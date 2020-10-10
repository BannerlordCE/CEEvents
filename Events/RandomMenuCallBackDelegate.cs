using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    public class RandomMenuCallBackDelegate
    {
        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;
        private readonly ScoresCalculation _score = new ScoresCalculation();
        private readonly Dynamics _dynamics = new Dynamics();

        internal RandomMenuCallBackDelegate(CEEvent listedEvent) => _listedEvent = listedEvent;

        internal RandomMenuCallBackDelegate(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
        }

        internal void RandomEventGameMenu(MenuCallbackArgs menuCallbackArgs)
        {
            menuCallbackArgs.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                                   ? "wait_prisoner_female"
                                                                   : "wait_prisoner_male");

            new SharedCallBackHelper(_listedEvent, _option).LoadBackgroundImage("default_random");

            MBTextManager.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale
                                              ? 1
                                              : 0);
        }

        internal bool RandomEventConditionMenuOption(MenuCallbackArgs args)
        {
            PlayerIsNotBusy(ref args);
            PlayerHasOpenSpaceForCompanions(ref args);
            Escaping(ref args);
            Leave(ref args);
            SoldToSettlement();
            SoldToCaravan();
            SoldToLordParty();
            GiveGold();
            ChangeGold();
            Wait(ref args);
            Trade(ref args);
            RansomAndBribe(ref args);
            BribeAndEscape(ref args);
            SubMenu(ref args);
            Continue(ref args);
            EmptyIcon(ref args);
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

            return true;
        }

        internal void RandomEventConsequenceMenuOption(MenuCallbackArgs args)
        {
            SharedCallBackHelper sharedCallBackHelper = new SharedCallBackHelper(_listedEvent, _option);
            CaptorSpecifics captorSpecifics = new CaptorSpecifics();

            //h.ProceedToSharedCallBacks();
            sharedCallBackHelper.ConsequenceXP();
            sharedCallBackHelper.ConsequenceLeaveSpouse();
            sharedCallBackHelper.ConsequenceGold();
            sharedCallBackHelper.ConsequenceChangeGold();
            sharedCallBackHelper.ConsequenceChangeTrait();
            sharedCallBackHelper.ConsequenceChangeSkill();
            sharedCallBackHelper.ConsequenceSlaveryLevel();
            sharedCallBackHelper.ConsequenceSlaveryFlags();
            sharedCallBackHelper.ConsequenceProstitutionLevel();
            sharedCallBackHelper.ConsequenceProstitutionFlags();
            sharedCallBackHelper.ConsequenceRenown();
            sharedCallBackHelper.ConsequenceChangeHealth();
            sharedCallBackHelper.ConsequenceChangeMorale();
            sharedCallBackHelper.ConsequenceSpawnTroop();
            sharedCallBackHelper.ConsequenceSpawnHero();

            ConsequenceImpregnation();
            ConsequenceGainRandomPrisoners();
            ConsequenceSoldEvents(ref args);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor)) _dynamics.CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero);
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0) ConsequenceRandomEventTrigger(ref args);
            else if (!string.IsNullOrEmpty(_option.TriggerEventName)) ConsequenceSingleEventTrigger(ref args); // Single Event Trigger 
            else captorSpecifics.CECaptorContinue(args);
        }


        #region private

        private void ConsequenceSingleEventTrigger(ref MenuCallbackArgs args)
        {
            try
            {
                CEEvent triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
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
            CaptorSpecifics captorSpecifics = new CaptorSpecifics();
            List<CEEvent> eventNames = new List<CEEvent>();

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

                    if (!triggerEvent.EventUseConditions.IsStringNoneOrEmpty() && triggerEvent.EventUseConditions == "True")
                    {
                        string conditionMatched = null;

                        if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
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
                        weightedChance = new CEVariablesLoader().GetIntFromXML(!triggerEvent.EventWeight.IsStringNoneOrEmpty()
                                                                      ? triggerEvent.EventWeight
                                                                      : triggeredEvent.WeightedChanceOfOccuring);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }

                    for (int a = weightedChance; a > 0; a--) eventNames.Add(triggeredEvent);
                }


                if (eventNames.Count > 0)
                {
                    int number = MBRandom.Random.Next(0, eventNames.Count - 1);

                    try
                    {
                        GameMenu.SwitchToMenu(eventNames[number].Name);
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

        private void ConsequenceSoldEvents(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.PartyBelongedTo.CurrentSettlement == null) return;
            ConsequenceSoldToSettlement(ref args);
            ConsequenceSoldToCaravan(ref args);
            ConsequenceSoldToNotable(ref args);
            ConsequenceSoldToLordParty(ref args);
        }

        internal void CERandomCaptivityChange(ref MenuCallbackArgs args, PartyBase party)
        {
            try
            {
                Hero prisonerCharacter = Hero.MainHero;
                prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
                prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
                while (PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter)) PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, -1, true);
                party.AddPrisoner(prisonerCharacter.CharacterObject, 1);

                if (prisonerCharacter == Hero.MainHero) PlayerCaptivity.StartCaptivity(party);
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
                Hero notable = settlement.Notables.Where(findFirstNotable => !findFirstNotable.IsFemale).GetRandomElement();
                CECampaignBehavior.ExtraProps.Owner = notable;

                PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                CERandomCaptivityChange(ref args, party);

            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceSoldToCaravan(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan);
                CERandomCaptivityChange(ref args, party.Party);

            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void ConsequenceSoldToLordParty(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty && !mobileParty.IsMainParty);
                CERandomCaptivityChange(ref args, party.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void ConsequenceSoldToSettlement(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) return;

            try
            {
                PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                CERandomCaptivityChange(ref args, party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceGainRandomPrisoners()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) _dynamics.CEGainRandomPrisoners(PartyBase.MainParty);
        }

        private void ConsequenceImpregnation()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk)) return;

            try
            {
                CEImpregnationSystem impregnationSystem = new CEImpregnationSystem();

                if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { impregnationSystem.ImpregnationChance(Hero.MainHero, new CEVariablesLoader().GetIntFromXML(_option.PregnancyRiskModifier)); }
                else if (!string.IsNullOrEmpty(_listedEvent.PregnancyRiskModifier)) { impregnationSystem.ImpregnationChance(Hero.MainHero, new CEVariablesLoader().GetIntFromXML(_listedEvent.PregnancyRiskModifier)); }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    impregnationSystem.ImpregnationChance(Hero.MainHero, 30);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
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

        private void ReqTrait(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroTrait.IsStringNoneOrEmpty()) return;
            int traitLevel;

            try { traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(_option.ReqHeroTrait)); }
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
        }

        private bool ReqSkillsLevelBelow(ref MenuCallbackArgs args, SkillObject skillRequired, int skillLevel, string max)
        {
            if (max.IsStringNoneOrEmpty()) return false;
            if (skillLevel <= new CEVariablesLoader().GetIntFromXML(max)) return false;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
            text.SetTextVariable("SKILL", skillRequired.Name);
            args.Tooltip = text;
            args.IsEnabled = false;

            return true;
        }

        private bool ReqSkillsLevelAbove(ref MenuCallbackArgs args, SkillObject skillRequired, int skillLevel, string min)
        {
            if (min.IsStringNoneOrEmpty()) return false;
            if (skillLevel >= new CEVariablesLoader().GetIntFromXML(min)) return false;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
            text.SetTextVariable("SKILL", skillRequired.Name);
            args.Tooltip = text;
            args.IsEnabled = false;

            return true;
        }

        private void ReqHeroSkill(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroSkill.IsStringNoneOrEmpty()) return;
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

        private void ReqProstitute(ref MenuCallbackArgs args)
        {
            int prostitute = Hero.MainHero.GetSkillValue(CESkills.Prostitution);

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

        private void ReqSlavery(ref MenuCallbackArgs args)
        {
            int slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroSlaveLevelAbove)) ReqHeroSlaveLevelAbove(ref args, slave);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroSlaveLevelBelow)) ReqHeroSlaveLevelBelow(ref args, slave);
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

        private void ReqHeroHealthPercentage(ref MenuCallbackArgs args)
        {
            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroHealthAbovePercentage)) ReqHeroHealthAbovePercentage(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthAbovePercentage / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroHealthBelowPercentage)) ReqHeroHealthBelowPercentage(ref args);
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
            if (_option.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroFemaleCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroFemaleCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        #endregion

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
            if (_option.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroMaleCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqMaleCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroMaleCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        #endregion

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
            if (_option.ReqCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        #endregion

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
            if (_option.ReqFemaleTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroFemaleTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqFemaleTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroFemaleTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion

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
            if (_option.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroMaleTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqMaleTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroMaleTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion

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
            if (_option.ReqTroopsBelow.IsStringNoneOrEmpty()) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; })) <= new CEVariablesLoader().GetIntFromXML(_option.ReqTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroTroopsBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroTroopsBelow.IsStringNoneOrEmpty()) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; })) <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqTroopsAbove.IsStringNoneOrEmpty()) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; })) >= new CEVariablesLoader().GetIntFromXML(_option.ReqTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqHeroTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroTroopsAbove.IsStringNoneOrEmpty()) return;
            if ((PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; })) >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        #endregion


        private void ReqMorale(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqMoraleAbove.IsStringNoneOrEmpty()) ReqMoraleAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed "); }

            try
            {
                if (!_option.ReqMoraleBelow.IsStringNoneOrEmpty()) ReqMoraleBelow(ref args);
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

        private void EmptyIcon(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;
        }

        private void Continue(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;
        }

        private void SubMenu(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
        }

        private void BribeAndEscape(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.BribeAndEscape)) args.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
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

        private void ChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");
                MBTextManager.SetTextVariable("MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        private void GiveGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;
            int content = _score.AttractivenessScore(Hero.MainHero);
            content *= _option.MultipleRestrictedListOfConsequences.Count(consquence => { return consquence == RestrictedListOfConsequences.GiveGold; });
            MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
        }

        private void SoldToLordParty()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) return;

            try
            {
                MobileParty party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty && !mobileParty.IsMainParty; });
                if (party != null) MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void SoldToCaravan()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) return;

            try
            {
                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                if (party != null) MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void SoldToSettlement()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) return;

            try
            {
                PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void Leave(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) args.optionLeaveType = GameMenuOption.LeaveType.Leave;
        }

        private void Escaping(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) args.optionLeaveType = GameMenuOption.LeaveType.Escape;
        }

        private void PlayerHasOpenSpaceForCompanions(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerAllowedCompanion)) return;
            if (!(Clan.PlayerClan.Companions.Count<Hero>() >= Clan.PlayerClan.CompanionLimit)) return;

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

        /*private void LoadBackgroundImage()
        {
            try
            {
                string backgroundName = _listedEvent.BackgroundName;

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
                    float speed = 0.03f;

                    try
                    {
                        if (!_listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = VariablesLoader.GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e) { CECustomHandler.LogToFile("Failed to load BackgroundAnimationSpeed for " + _listedEvent.Name + " : Exception: " + e); }

                    CESubModule.animationSpeed = speed;
                }
                else
                {
                    CESubModule.animationPlayEvent = false;
                    CESubModule.LoadTexture("default_random");
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed to load background for " + _listedEvent.Name);
                CESubModule.LoadTexture("default_random");
            }
        }*/

        #endregion
    }
}