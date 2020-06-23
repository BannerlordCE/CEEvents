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
    public class RandomMenuCallBackDelegate
    {
        private readonly CEEvent __listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;

        internal RandomMenuCallBackDelegate(CEEvent _listedEvent)
        {
            __listedEvent = _listedEvent;
        }

        internal RandomMenuCallBackDelegate(CEEvent _listedEvent, Option option, List<CEEvent> eventList)
        {
            __listedEvent = _listedEvent;
            _option = option;
            _eventList = eventList;
        }


        internal void RandomEventGameMenu(MenuCallbackArgs args)
        {
            var varLoader = new VariablesLoader();

            args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                       ? "wait_prisoner_female"
                                                       : "wait_prisoner_male");

            try
            {
                var backgroundName = __listedEvent.BackgroundName;

                if (!backgroundName.IsStringNoneOrEmpty())
                {
                    CESubModule.animationPlayEvent = false;
                    CESubModule.LoadTexture(backgroundName);
                }
                else if (__listedEvent.BackgroundAnimation != null && __listedEvent.BackgroundAnimation.Count > 0)
                {
                    CESubModule.animationImageList = __listedEvent.BackgroundAnimation;
                    CESubModule.animationIndex = 0;
                    CESubModule.animationPlayEvent = true;
                    var speed = 0.03f;

                    try
                    {
                        if (!__listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = varLoader.GetFloatFromXML(__listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e) { CECustomHandler.LogToFile("Failed to load BackgroundAnimationSpeed for " + __listedEvent.Name + " : Exception: " + e); }

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
                CECustomHandler.LogToFile("Failed to load background for " + __listedEvent.Name);
                CESubModule.LoadTexture("default_random");
            }

            MBTextManager.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale
                                              ? 1
                                              : 0);
        }

        internal bool RandomEventConditionMenuOption(MenuCallbackArgs args)
        {
            var varLoader = new VariablesLoader();
            var score = new ScoresCalculation();

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) args.optionLeaveType = GameMenuOption.LeaveType.Escape;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) args.optionLeaveType = GameMenuOption.LeaveType.Leave;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                try
                {
                    var party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                    MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                try
                {
                    var party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                    if (party != null) MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty))
                try
                {
                    var party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => { return mobileParty.IsLordParty; });
                    if (party != null) MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
            {
                var content = score.AttractivenessScore(Hero.MainHero);
                content *= _option.MultipleRestrictedListOfConsequences.Count(consquence => { return consquence == RestrictedListOfConsequences.GiveGold; });
                MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
            }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                try
                {
                    var level = 0;

                    if (!string.IsNullOrEmpty(_option.GoldTotal)) level = varLoader.GetIntFromXML(_option.GoldTotal);
                    else if (!string.IsNullOrEmpty(__listedEvent.GoldTotal)) level = varLoader.GetIntFromXML(__listedEvent.GoldTotal);
                    else CECustomHandler.LogToFile("Missing GoldTotal");
                    MBTextManager.SetTextVariable("MONEY_AMOUNT", level);
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait)) args.optionLeaveType = GameMenuOption.LeaveType.Wait;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade)) args.optionLeaveType = GameMenuOption.LeaveType.Trade;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe)) args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.BribeAndEscape)) args.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;

            // ReqMorale
            try
            {
                if (!_option.ReqMoraleAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.IsMobile && PartyBase.MainParty.MobileParty.Morale < varLoader.GetIntFromXML(_option.ReqMoraleAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed "); }

            try
            {
                if (!_option.ReqMoraleBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.IsMobile && PartyBase.MainParty.MobileParty.Morale > varLoader.GetIntFromXML(_option.ReqMoraleBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed "); }

            // ReqTroops
            try
            {
                if (!_option.ReqTroopsAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.NumberOfHealthyMembers < varLoader.GetIntFromXML(_option.ReqTroopsAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqTroopsBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.NumberOfHealthyMembers > varLoader.GetIntFromXML(_option.ReqTroopsBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed "); }

            // ReqMaleTroops
            try
            {
                if (!_option.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < varLoader.GetIntFromXML(_option.ReqMaleTroopsAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleTroopsBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < varLoader.GetIntFromXML(_option.ReqMaleTroopsBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed "); }

            // ReqFemaleTroops
            try
            {
                if (!_option.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < varLoader.GetIntFromXML(_option.ReqFemaleTroopsAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > varLoader.GetIntFromXML(_option.ReqFemaleTroopsBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed "); }

            // ReqCaptives
            try
            {
                if (!_option.ReqCaptivesAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.NumberOfPrisoners < varLoader.GetIntFromXML(_option.ReqCaptivesAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqCaptivesBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.NumberOfPrisoners > varLoader.GetIntFromXML(_option.ReqCaptivesBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed "); }

            // ReqMaleCaptives
            try
            {
                if (!_option.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < varLoader.GetIntFromXML(_option.ReqMaleCaptivesAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleCaptivesBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < varLoader.GetIntFromXML(_option.ReqMaleCaptivesBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed "); }

            // ReqFemaleCaptives
            try
            {
                if (!_option.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < varLoader.GetIntFromXML(_option.ReqFemaleCaptivesAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty())
                    if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > varLoader.GetIntFromXML(_option.ReqFemaleCaptivesBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed "); }

            // ReqHeroHealthPercentage
            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroHealthAbovePercentage))
                    if (Hero.MainHero.HitPoints < varLoader.GetIntFromXML(_option.ReqHeroHealthAbovePercentage))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_health", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthAbovePercentage / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroHealthBelowPercentage))
                    if (Hero.MainHero.HitPoints > varLoader.GetIntFromXML(_option.ReqHeroHealthBelowPercentage))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_health", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroHealthBelowPercentage / Failed "); }

            // ReqSlavery
            var slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroSlaveLevelAbove))
                    if (slave < varLoader.GetIntFromXML(_option.ReqHeroSlaveLevelAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroSlaveLevelBelow))
                    if (slave > varLoader.GetIntFromXML(_option.ReqHeroSlaveLevelBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelBelow / Failed "); }

            // ReqProstitute
            var prostitute = Hero.MainHero.GetSkillValue(CESkills.Prostitution);

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroProstituteLevelAbove))
                    if (prostitute < varLoader.GetIntFromXML(_option.ReqHeroProstituteLevelAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroProstituteLevelBelow))
                    if (prostitute > varLoader.GetIntFromXML(_option.ReqHeroProstituteLevelBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelBelow / Failed "); }

            // Req Skill
            if (!_option.ReqHeroSkill.IsStringNoneOrEmpty())
            {
                int skillLevel;

                try { skillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqHeroSkill)); }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid Skill Captive");
                    skillLevel = 0;
                }

                try
                {
                    if (!_option.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < varLoader.GetIntFromXML(_option.ReqHeroSkillLevelAbove))
                        {
                            var text = GameTexts.FindText("str_CE_skill_level", "low");
                            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove"); }

                try
                {
                    if (!_option.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty())
                        if (skillLevel > varLoader.GetIntFromXML(_option.ReqHeroSkillLevelBelow))
                        {
                            var text = GameTexts.FindText("str_CE_skill_level", "high");
                            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow"); }
            }

            // Req Trait
            if (!_option.ReqHeroTrait.IsStringNoneOrEmpty())
            {
                int traitLevel;

                try { traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(_option.ReqHeroTrait)); }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid Trait Captive");
                    traitLevel = 0;
                }

                try
                {
                    if (!string.IsNullOrEmpty(_option.ReqHeroTraitLevelAbove))
                        if (traitLevel < varLoader.GetIntFromXML(_option.ReqHeroTraitLevelAbove))
                        {
                            var text = GameTexts.FindText("str_CE_trait_level", "low");
                            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove"); }

                try
                {
                    if (!string.IsNullOrEmpty(_option.ReqHeroTraitLevelBelow))
                        if (traitLevel > varLoader.GetIntFromXML(_option.ReqHeroTraitLevelBelow))
                        {
                            var text = GameTexts.FindText("str_CE_trait_level", "high");
                            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow"); }
            }

            // ReqGold
            try
            {
                if (!string.IsNullOrEmpty(_option.ReqGoldAbove))
                    if (Hero.MainHero.Gold < varLoader.GetIntFromXML(_option.ReqGoldAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed "); }

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqGoldBelow))
                    if (Hero.MainHero.Gold > varLoader.GetIntFromXML(_option.ReqGoldBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed "); }

            return true;
        }

        internal void RandomEventConsequenceMenuOption(MenuCallbackArgs args)
        {
            var dynamics = new Dynamics();
            var varLoader = new VariablesLoader();
            var score = new ScoresCalculation();

            //XP
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP))
                try
                {
                    var skillToLevel = "";

                    if (!string.IsNullOrEmpty(_option.SkillToLevel)) skillToLevel = _option.SkillToLevel;
                    else if (!string.IsNullOrEmpty(__listedEvent.SkillToLevel)) skillToLevel = __listedEvent.SkillToLevel;
                    else CECustomHandler.LogToFile("Missing SkillToLevel");

                    foreach (var skillObject in SkillObject.All)
                        if (skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)
                            dynamics.GainSkills(skillObject, 50, 100);
                }
                catch (Exception) { CECustomHandler.LogToFile("GiveXP Failed"); }

            // Leave Spouse
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) dynamics.ChangeSpouse(Hero.MainHero, null);

            // Gold
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
            {
                var content = new ScoresCalculation().AttractivenessScore(Hero.MainHero);
                var currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                content += currentValue / 2;
                content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
            }

            // Change Gold
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                try
                {
                    var level = 0;

                    if (!string.IsNullOrEmpty(_option.GoldTotal)) level = varLoader.GetIntFromXML(_option.GoldTotal);
                    else if (!string.IsNullOrEmpty(__listedEvent.GoldTotal)) level = varLoader.GetIntFromXML(__listedEvent.GoldTotal);
                    else CECustomHandler.LogToFile("Missing GoldTotal");

                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }

            // ChangeTrait
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait))
                try
                {
                    var level = 0;

                    if (!string.IsNullOrEmpty(_option.TraitTotal)) level = varLoader.GetIntFromXML(_option.TraitTotal);
                    else if (!string.IsNullOrEmpty(__listedEvent.TraitTotal)) level = varLoader.GetIntFromXML(__listedEvent.TraitTotal);
                    else CECustomHandler.LogToFile("Missing Trait TraitTotal");

                    if (!string.IsNullOrEmpty(_option.TraitToLevel)) dynamics.TraitModifier(Hero.MainHero, _option.TraitToLevel, level);
                    else if (!string.IsNullOrEmpty(__listedEvent.TraitToLevel)) dynamics.TraitModifier(Hero.MainHero, __listedEvent.TraitToLevel, level);
                    else CECustomHandler.LogToFile("Missing TraitToLevel");
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }

            // ChangeSkill
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill))
                try
                {
                    var level = 0;

                    if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = varLoader.GetIntFromXML(_option.SkillTotal);
                    else if (!__listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = varLoader.GetIntFromXML(__listedEvent.SkillTotal);
                    else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                    if (!_option.SkillToLevel.IsStringNoneOrEmpty()) dynamics.SkillModifier(Hero.MainHero, _option.SkillToLevel, level);
                    else if (!__listedEvent.SkillToLevel.IsStringNoneOrEmpty()) dynamics.SkillModifier(Hero.MainHero, __listedEvent.SkillToLevel, level);
                    else CECustomHandler.LogToFile("Missing SkillToLevel");
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }

            // Slavery Level
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel))
                try
                {
                    if (!string.IsNullOrEmpty(_option.SlaveryTotal)) { dynamics.VictimSlaveryModifier(varLoader.GetIntFromXML(_option.SlaveryTotal), Hero.MainHero); }
                    else if (!string.IsNullOrEmpty(__listedEvent.SlaveryTotal)) { dynamics.VictimSlaveryModifier(varLoader.GetIntFromXML(__listedEvent.SlaveryTotal), Hero.MainHero); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing SlaveryTotal");
                        dynamics.VictimSlaveryModifier(1, Hero.MainHero);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid SlaveryTotal"); }

            // Slavery Flags
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) dynamics.VictimSlaveryModifier(1, Hero.MainHero, true, false, true);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) dynamics.VictimSlaveryModifier(0, Hero.MainHero, true, false, true);

            // Prostitution Level
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel))
                try
                {
                    if (!string.IsNullOrEmpty(_option.ProstitutionTotal)) { dynamics.VictimProstitutionModifier(varLoader.GetIntFromXML(_option.ProstitutionTotal), Hero.MainHero); }
                    else if (!string.IsNullOrEmpty(__listedEvent.ProstitutionTotal)) { dynamics.VictimProstitutionModifier(varLoader.GetIntFromXML(__listedEvent.ProstitutionTotal), Hero.MainHero); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing ProstitutionTotal");
                        dynamics.VictimProstitutionModifier(1, Hero.MainHero);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ProstitutionTotal"); }

            // Prostitution Flags
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) dynamics.VictimProstitutionModifier(1, Hero.MainHero, true, false, true);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) dynamics.VictimProstitutionModifier(0, Hero.MainHero, true, false, true);

            // Renown
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown))
                try
                {
                    if (!string.IsNullOrEmpty(_option.RenownTotal)) { dynamics.RenownModifier(varLoader.GetIntFromXML(_option.RenownTotal), Hero.MainHero); }
                    else if (!string.IsNullOrEmpty(__listedEvent.RenownTotal)) { dynamics.RenownModifier(varLoader.GetIntFromXML(__listedEvent.RenownTotal), Hero.MainHero); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing RenownTotal");
                        dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid RenownTotal"); }

            // ChangeHealth
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth))
                try
                {
                    if (!string.IsNullOrEmpty(_option.HealthTotal)) { Hero.MainHero.HitPoints += varLoader.GetIntFromXML(_option.HealthTotal); }
                    else if (!string.IsNullOrEmpty(__listedEvent.HealthTotal)) { Hero.MainHero.HitPoints += varLoader.GetIntFromXML(__listedEvent.HealthTotal); }
                    else
                    {
                        CECustomHandler.LogToFile("Invalid HealthTotal");
                        Hero.MainHero.HitPoints += MBRandom.RandomInt(-20, 20);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Missing HealthTotal"); }

            // ChangeMorale
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale))
                try
                {
                    if (!string.IsNullOrEmpty(_option.MoraleTotal)) { dynamics.MoralChange(varLoader.GetIntFromXML(_option.MoraleTotal), PartyBase.MainParty); }
                    else if (!string.IsNullOrEmpty(__listedEvent.MoraleTotal)) { dynamics.MoralChange(varLoader.GetIntFromXML(__listedEvent.MoraleTotal), PartyBase.MainParty); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing MoralTotal");
                        dynamics.MoralChange(MBRandom.RandomInt(-5, 5), PartyBase.MainParty);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }

            // Impregnation
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                try
                {
                    var i = new ImpregnationSystem();

                    if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { i.ImpregnationChance(Hero.MainHero, varLoader.GetIntFromXML(_option.PregnancyRiskModifier)); }
                    else if (!string.IsNullOrEmpty(__listedEvent.PregnancyRiskModifier)) { i.ImpregnationChance(Hero.MainHero, varLoader.GetIntFromXML(__listedEvent.PregnancyRiskModifier)); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                        i.ImpregnationChance(Hero.MainHero, 30);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) dynamics.CEGainRandomPrisoners(PartyBase.MainParty);

            var c = new CaptorSpecifics();

            if (Hero.MainHero.PartyBelongedTo.CurrentSettlement != null)
            {
                // Sold Events
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                    try
                    {
                        //var party = Hero.MainHero.PartyBelongedTo.CurrentSettlement.Party;    //Warning: never used.
                        PlayerCaptivity.StartCaptivity(Hero.MainHero.PartyBelongedTo.CurrentSettlement.Party);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }

                // Sold To Caravan
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                    try
                    {
                        var party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan);
                        if (party != null) MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }

                // Work In Progress Sold Event
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable))
                    try
                    {
                        var settlement = PartyBase.MainParty.MobileParty.CurrentSettlement;
                        var notable = settlement.Notables.Where(findFirstNotable => !findFirstNotable.IsFemale).GetRandomElement();
                        CECampaignBehavior.ExtraProps.Owner = notable;
                        new CaptiveSpecifics().CECaptivityChange(ref args, settlement.Party);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }
            }

            // Kill Hero
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor)) { dynamics.CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero); }
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
                            weightedChance = varLoader.GetIntFromXML(!triggerEvent.EventWeight.IsStringNoneOrEmpty()
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
    }
}