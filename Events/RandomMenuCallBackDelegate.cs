using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
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
        private readonly VariablesLoader _variables = new VariablesLoader();
        private readonly ScoresCalculation _score = new ScoresCalculation();
        private readonly Dynamics _dynamics = new Dynamics();

        internal RandomMenuCallBackDelegate(CEEvent listedEvent)
        {
            _listedEvent = listedEvent;
        }

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

            LoadBackgroundImage();

            MBTextManager.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale
                                              ? 1
                                              : 0);
        }

        internal bool RandomEventConditionMenuOption(MenuCallbackArgs args)
        {
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
            args = ReqSlavery(ref args);
            ReqProstitute(ref args);
            ReqSkill(ref args);
            ReqTrait(ref args);
            ReqGold(ref args);

            return true;
        }

        internal void RandomEventConsequenceMenuOption(MenuCallbackArgs args)
        {
            var h = new SharedCallBackHelper(_listedEvent, _option);
            var c = new CaptorSpecifics();

            h.ProceedToSharedCallBacks();

      
            ConsequenceImpregnation();
            ConsequenceGainRandomPrisoners();
            ConsequenceSoldEvents(ref args);

            // Kill Hero
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor)) { _dynamics.CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero); }
            // Random Event Trigger
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0)
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
                            var conditionMatched = CEEventChecker.FlagsDoMatchEventConditions(triggeredEvent, CharacterObject.PlayerCharacter);

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

                        try { GameMenu.SwitchToMenu(eventNames[number].Name); }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                            c.CECaptorContinue(args);
                        }
                    }
                    else { c.CECaptorContinue(args); }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                    c.CECaptorContinue(args);
                }
            }
            // Single Event Trigger
            else if (!string.IsNullOrEmpty(_option.TriggerEventName))
            {
                try
                {
                    var triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                    GameMenu.SwitchToMenu(triggeredEvent.Name);
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile("Couldn't find " + _option.TriggerEventName + " in events.");
                    c.CECaptorContinue(args);
                }
            }
            else { c.CECaptorContinue(args); }
        }

        


#region private


        private void ConsequenceSoldEvents(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.PartyBelongedTo.CurrentSettlement == null) return;
            ConsequenceSoldToSettlement();


            ConsequenceSoldToCaravan();

            // Work In Progress Sold Event
            ConsequenceSoldToNotable(ref args);
        }

        private void ConsequenceSoldToNotable(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable)) return;

            try
            {
                var settlement = PartyBase.MainParty.MobileParty.CurrentSettlement;
                var notable = settlement.Notables.Where(findFirstNotable => !findFirstNotable.IsFemale).GetRandomElement();
                CECampaignBehavior.ExtraProps.Owner = notable;
                new CaptiveSpecifics().CECaptivityChange(ref args, settlement.Party);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
        }

        private void ConsequenceSoldToCaravan()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) return;

            try
            {
                var party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan);
                if (party != null) MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void ConsequenceSoldToSettlement()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) return;

            try
            {
                //var party = Hero.MainHero.PartyBelongedTo.CurrentSettlement.Party;    //Warning: never used.
                PlayerCaptivity.StartCaptivity(Hero.MainHero.PartyBelongedTo.CurrentSettlement.Party);
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
                var i = new ImpregnationSystem();

                if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { i.ImpregnationChance(Hero.MainHero, _variables.GetIntFromXML(_option.PregnancyRiskModifier)); }
                else if (!string.IsNullOrEmpty(_listedEvent.PregnancyRiskModifier)) { i.ImpregnationChance(Hero.MainHero, _variables.GetIntFromXML(_listedEvent.PregnancyRiskModifier)); }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    i.ImpregnationChance(Hero.MainHero, 30);
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
            if (Hero.MainHero.Gold > _variables.GetIntFromXML(_option.ReqGoldBelow))
            {
                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
                args.IsEnabled = false;
            }
        }

        private void ReqGoldAbove(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.Gold >= _variables.GetIntFromXML(_option.ReqGoldAbove)) return;
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

        private void ReqSkill(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroSkill.IsStringNoneOrEmpty()) return;
            int skillLevel;

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

        private MenuCallbackArgs ReqSlavery(ref MenuCallbackArgs args)
        {
            var slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);

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

            return args;
        }

        private void ReqHeroSlaveLevelBelow(ref MenuCallbackArgs args, int slave)
        {
            if (slave <= _variables.GetIntFromXML(_option.ReqHeroSlaveLevelBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroSlaveLevelAbove(ref MenuCallbackArgs args, int slave)
        {
            if (slave >= _variables.GetIntFromXML(_option.ReqHeroSlaveLevelAbove)) return;
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
            if (Hero.MainHero.HitPoints <= _variables.GetIntFromXML(_option.ReqHeroHealthBelowPercentage)) return;
            args.Tooltip = GameTexts.FindText("str_CE_health", "high");
            args.IsEnabled = false;
        }

        private void ReqHeroHealthAbovePercentage(ref MenuCallbackArgs args)
        {
            if (Hero.MainHero.HitPoints >= _variables.GetIntFromXML(_option.ReqHeroHealthAbovePercentage)) return;
            args.Tooltip = GameTexts.FindText("str_CE_health", "low");
            args.IsEnabled = false;
        }

        private void ReqFemaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty()) ReqFemaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) ReqFemaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed "); }
        }

        private void ReqFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) <= _variables.GetIntFromXML(_option.ReqFemaleCaptivesBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqFemaleCaptivesAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqMaleCaptivesAbove.IsStringNoneOrEmpty()) ReqMaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) ReqMaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed "); }
        }

        private void ReqMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleCaptivesBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleCaptivesAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqCaptivesAbove.IsStringNoneOrEmpty()) ReqCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqCaptivesBelow.IsStringNoneOrEmpty()) ReqCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed "); }
        }

        private void ReqCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.NumberOfPrisoners <= _variables.GetIntFromXML(_option.ReqCaptivesBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.NumberOfPrisoners >= _variables.GetIntFromXML(_option.ReqCaptivesAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqFemaleTroopsAbove.IsStringNoneOrEmpty()) ReqFemaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleTroopsBelow.IsStringNoneOrEmpty()) ReqFemaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed "); }
        }

        private void ReqFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) <= _variables.GetIntFromXML(_option.ReqFemaleTroopsBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqFemaleTroopsAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqMaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqMaleTroopsAbove.IsStringNoneOrEmpty()) ReqMaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) ReqMaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed "); }
        }

        private void ReqMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleTroopsBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) >= _variables.GetIntFromXML(_option.ReqMaleTroopsAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqTroops(ref MenuCallbackArgs args)
        {
            try
            {
                if (!_option.ReqTroopsAbove.IsStringNoneOrEmpty()) ReqTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqTroopsBelow.IsStringNoneOrEmpty()) ReqTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed "); }
        }

        private void ReqTroopsBelow(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.NumberOfHealthyMembers <= _variables.GetIntFromXML(_option.ReqTroopsBelow)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqTroopsAbove(ref MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.NumberOfHealthyMembers >= _variables.GetIntFromXML(_option.ReqTroopsAbove)) return;
            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

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
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale > _variables.GetIntFromXML(_option.ReqMoraleBelow))) return;
            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMoraleAbove(ref MenuCallbackArgs args)
        {
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale < _variables.GetIntFromXML(_option.ReqMoraleAbove))) return;
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
                var level = 0;

                if (!string.IsNullOrEmpty(_option.GoldTotal)) level = _variables.GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = _variables.GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");
                MBTextManager.SetTextVariable("MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        private void GiveGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;
            var content = _score.AttractivenessScore(Hero.MainHero);
            content *= _option.MultipleRestrictedListOfConsequences.Count(consquence => { return consquence == RestrictedListOfConsequences.GiveGold; });
            MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
        }

        private void SoldToLordParty()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty)) return;

            try
            {
                var party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => { return mobileParty.IsLordParty; });
                if (party != null) MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }
        }

        private void SoldToCaravan()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan)) return;

            try
            {
                var party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                if (party != null) MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }
        }

        private void SoldToSettlement()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement)) return;

            try
            {
                var party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
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

        private void LoadBackgroundImage()
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
        }

#endregion
    }
}