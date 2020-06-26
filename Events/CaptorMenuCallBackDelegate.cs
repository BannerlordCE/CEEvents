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
    public class CaptorMenuCallBackDelegate
    {
        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;

        internal CaptorMenuCallBackDelegate(CEEvent listedEvent)
        {
            _listedEvent = listedEvent;
        }

        internal CaptorMenuCallBackDelegate(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
        }

        internal void CaptorEventWaitGameMenu(MenuCallbackArgs args)
        {
            var varLoader = new VariablesLoader();

            SetNames(ref args);

            /*try
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
                        if (!_listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = varLoader.GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e) { CECustomHandler.ForceLogToFile("Failed to load BackgroundAnimationSpeed for " + _listedEvent.Name + " : Exception: " + e); }

                    CESubModule.animationSpeed = speed;
                }
                else
                {
                    CESubModule.animationPlayEvent = false;
                    CESubModule.LoadTexture("captor_default");
                }
            }
            catch (Exception) { CECustomHandler.ForceLogToFile("Background failed to load on " + _listedEvent.Name); }*/
            new SharedCallBackHelper(_listedEvent, _option).LoadBackgroundImage("captor_default");
        }

        private void SetNames(ref MenuCallbackArgs args)
        {
            try
            {
                if (_listedEvent.Captive != null)
                {
                    //Hero captiveHero = null;
                    //if (_listedEvent.Captive.IsHero) captiveHero = _listedEvent.Captive.HeroObject; //WARNING: captiveHero never used
                    MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive.Name);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Hero doesn't exist"); }

            var text = args.MenuContext.GameMenu.GetText();
            if (MobileParty.MainParty.CurrentSettlement != null) text.SetTextVariable("SETTLEMENT_NAME", MobileParty.MainParty.CurrentSettlement.Name);
            text.SetTextVariable("PARTY_NAME", MobileParty.MainParty.Name);
            text.SetTextVariable("CAPTOR_NAME", Hero.MainHero.Name);

            args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                       ? "wait_prisoner_female"
                                                       : "wait_prisoner_male");
        }

        internal bool CaptorEventOptionGameMenu(MenuCallbackArgs args)
        {
            var varLoader = new VariablesLoader();
            var score = new ScoresCalculation();

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerIsNotBusy))
                if (PlayerEncounter.Current != null)
                {
                    args.Tooltip = GameTexts.FindText("str_CE_busy_right_now");
                    args.IsEnabled = false;
                }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold))
            {
                var content = score.AttractivenessScore(_listedEvent.Captive.HeroObject);

                if (_listedEvent.Captive.HeroObject != null)
                {
                    var currentValue = _listedEvent.Captive.HeroObject.GetSkillValue(CESkills.Prostitution);
                    content += currentValue / 2;
                }

                content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
                MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
            }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                try
                {
                    var level = 0;

                    if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = varLoader.GetIntFromXML(_option.CaptorGoldTotal);
                    else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = varLoader.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                    else CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                    MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", level);
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait)) args.optionLeaveType = GameMenuOption.LeaveType.Wait;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade)) args.optionLeaveType = GameMenuOption.LeaveType.Trade;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe)) args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;


            ReqHeroCaptorRelation(ref args);

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

            // ReqTrait
            if (!_option.ReqHeroTrait.IsStringNoneOrEmpty())
            {
                var traitLevel = 0;

                try { traitLevel = _listedEvent.Captive.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
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
                            var text = GameTexts.FindText("str_CE_trait_captive_level", "low");
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
                            var text = GameTexts.FindText("str_CE_trait_captive_level", "high");
                            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow"); }
            }

            // ReqCaptorTrait
            if (!_option.ReqCaptorTrait.IsStringNoneOrEmpty())
            {
                var traitLevel = 0;

                try { traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid Trait Captor");
                    traitLevel = 0;
                }

                try
                {
                    if (!string.IsNullOrEmpty(_option.ReqCaptorTraitLevelAbove))
                        if (traitLevel < varLoader.GetIntFromXML(_option.ReqCaptorTraitLevelAbove))
                        {
                            var text = GameTexts.FindText("str_CE_trait_level", "low");
                            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove"); }

                try
                {
                    if (!string.IsNullOrEmpty(_option.ReqCaptorTraitLevelBelow))
                        if (traitLevel > varLoader.GetIntFromXML(_option.ReqCaptorTraitLevelBelow))
                        {
                            var text = GameTexts.FindText("str_CE_trait_level", "high");
                            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow"); }
            }

            // ReqSkill
            if (!_option.ReqHeroSkill.IsStringNoneOrEmpty())
            {
                var skillLevel = 0;

                try { skillLevel = _listedEvent.Captive.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqHeroSkill)); }
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
                            var text = GameTexts.FindText("str_CE_skill_captive_level", "low");
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
                            var text = GameTexts.FindText("str_CE_skill_captive_level", "high");
                            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow"); }
            }

            // ReqCaptorSkill
            if (!_option.ReqCaptorSkill.IsStringNoneOrEmpty())
            {
                var skillLevel = 0;

                try { skillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqCaptorSkill)); }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid Skill Captor");
                    skillLevel = 0;
                }

                try
                {
                    if (!_option.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < varLoader.GetIntFromXML(_option.ReqCaptorSkillLevelAbove))
                        {
                            var text = GameTexts.FindText("str_CE_skill_level", "low");
                            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove"); }

                try
                {
                    if (_option.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty())
                        if (skillLevel > varLoader.GetIntFromXML(_option.ReqCaptorSkillLevelBelow))
                        {
                            var text = GameTexts.FindText("str_CE_skill_level", "high");
                            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
                            args.Tooltip = text;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow"); }
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


        internal void CaptorConsequenceWaitGameMenu(MenuCallbackArgs args)
        {
            var score = new ScoresCalculation();
            var varLoader = new VariablesLoader();
            var dynamics = new Dynamics();
            var i = new ImpregnationSystem();
            var c = new CaptorSpecifics();
            Hero captiveHero = null;

            try
            {
                if (_listedEvent.Captive != null)
                {
                    if (_listedEvent.Captive.IsHero) captiveHero = _listedEvent.Captive.HeroObject;
                    MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive.Name);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Hero doesn't exist"); }

            // Captor Gold
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold))
            {
                var content = score.AttractivenessScore(captiveHero);
                var currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
                content += currentValue / 2;
                content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
            }

            // Captor Change Gold
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                try
                {
                    var level = 0;

                    if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = varLoader.GetIntFromXML(_option.CaptorGoldTotal);
                    else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = varLoader.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                    else CECustomHandler.LogToFile("Missing CaptorGoldTotal");

                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }

            // Captor Skill
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorSkill))
                try
                {
                    var level = 0;

                    if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = varLoader.GetIntFromXML(_option.SkillTotal);
                    else if (!_listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = varLoader.GetIntFromXML(_listedEvent.SkillTotal);
                    else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                    if (!_option.SkillToLevel.IsStringNoneOrEmpty()) dynamics.SkillModifier(Hero.MainHero, _option.SkillToLevel, level);
                    else if (!_listedEvent.SkillToLevel.IsStringNoneOrEmpty()) dynamics.SkillModifier(Hero.MainHero, _listedEvent.SkillToLevel, level);
                    else CECustomHandler.LogToFile("Missing SkillToLevel");
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }

            // Captor Trait
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait))
                try
                {
                    var level = varLoader.GetIntFromXML(!string.IsNullOrEmpty(_option.TraitTotal)
                                                            ? _option.TraitTotal
                                                            : _listedEvent.TraitTotal);

                    dynamics.TraitModifier(Hero.MainHero, !string.IsNullOrEmpty(_option.TraitToLevel)
                                               ? _option.TraitToLevel
                                               : _listedEvent.TraitToLevel, level);
                }
                catch (Exception) { CECustomHandler.LogToFile("Missing Trait Flags"); }

            // Captor Renown
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown))
                try
                {
                    dynamics.RenownModifier(!string.IsNullOrEmpty(_option.RenownTotal)
                                                ? varLoader.GetIntFromXML(_option.RenownTotal)
                                                : varLoader.GetIntFromXML(_listedEvent.RenownTotal), Hero.MainHero);
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing RenownTotal");
                    dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                }

            // ChangeMorale
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale))
                try
                {
                    dynamics.MoralChange(!string.IsNullOrEmpty(_option.MoraleTotal)
                                             ? varLoader.GetIntFromXML(_option.MoraleTotal)
                                             : varLoader.GetIntFromXML(_listedEvent.MoraleTotal), PartyBase.MainParty);
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing MoralTotal");
                    dynamics.MoralChange(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty);
                }

            if (captiveHero != null)
            {
                // Leave Spouse
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) dynamics.ChangeSpouse(captiveHero, null);

                // Force Marry
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor)) dynamics.ChangeSpouse(captiveHero, Hero.MainHero);

                // Change Clan
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeClan)) dynamics.ChangeClan(captiveHero, Hero.MainHero);

                // Slavery Flags
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) dynamics.VictimSlaveryModifier(1, captiveHero, true, false, true);
                else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) dynamics.VictimSlaveryModifier(0, captiveHero, true, false, true);

                // Slavery Level
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel))
                    try
                    {
                        dynamics.VictimSlaveryModifier(!string.IsNullOrEmpty(_option.SlaveryTotal)
                                                           ? varLoader.GetIntFromXML(_option.SlaveryTotal)
                                                           : varLoader.GetIntFromXML(_listedEvent.SlaveryTotal), captiveHero);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing SlaveryTotal");
                        dynamics.VictimSlaveryModifier(1, captiveHero);
                    }

                // Prostitution Flags
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) dynamics.VictimProstitutionModifier(1, captiveHero, true, false, true);
                else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) dynamics.VictimProstitutionModifier(0, captiveHero, true, false, true);

                // Prostitution Level
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel))
                    try
                    {
                        dynamics.VictimProstitutionModifier(!string.IsNullOrEmpty(_option.ProstitutionTotal)
                                                                ? varLoader.GetIntFromXML(_option.ProstitutionTotal)
                                                                : varLoader.GetIntFromXML(_listedEvent.ProstitutionTotal), captiveHero);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ProstitutionTotal");
                        dynamics.VictimProstitutionModifier(1, captiveHero);
                    }

                // Relations
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation))
                    try
                    {
                        dynamics.RelationsModifier(captiveHero, !string.IsNullOrEmpty(_option.RelationTotal)
                                                       ? varLoader.GetIntFromXML(_option.RelationTotal)
                                                       : varLoader.GetIntFromXML(_listedEvent.RelationTotal));
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing RelationTotal");
                        dynamics.RelationsModifier(captiveHero, MBRandom.RandomInt(-5, 5));
                    }

                // Gold
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
                {
                    var content = score.AttractivenessScore(captiveHero);
                    var currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
                    content += currentValue / 2;
                    content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
                    GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, content);
                }

                // Change Gold
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                    try
                    {
                        var level = 0;

                        if (!string.IsNullOrEmpty(_option.GoldTotal)) level = varLoader.GetIntFromXML(_option.GoldTotal);
                        else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = varLoader.GetIntFromXML(_listedEvent.GoldTotal);
                        else CECustomHandler.LogToFile("Missing GoldTotal");

                        GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, level);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }

                // Trait
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait))
                    try
                    {
                        var level = varLoader.GetIntFromXML(!string.IsNullOrEmpty(_option.TraitTotal)
                                                                ? _option.TraitTotal
                                                                : _listedEvent.TraitTotal);

                        dynamics.TraitModifier(captiveHero, !string.IsNullOrEmpty(_option.TraitToLevel)
                                                   ? _option.TraitToLevel
                                                   : _listedEvent.TraitToLevel, level);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing Trait Flags"); }

                // Skill
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill))
                    try
                    {
                        var level = 0;

                        if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = varLoader.GetIntFromXML(_option.SkillTotal);
                        else if (!_listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = varLoader.GetIntFromXML(_listedEvent.SkillTotal);
                        else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                        if (!_option.SkillToLevel.IsStringNoneOrEmpty()) dynamics.SkillModifier(captiveHero, _option.SkillToLevel, level);
                        else if (!_listedEvent.SkillToLevel.IsStringNoneOrEmpty()) dynamics.SkillModifier(captiveHero, _listedEvent.SkillToLevel, level);
                        else CECustomHandler.LogToFile("Missing SkillToLevel");
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }

                // Renown
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown))
                    try
                    {
                        dynamics.RenownModifier(!string.IsNullOrEmpty(_option.RenownTotal)
                                                    ? varLoader.GetIntFromXML(_option.RenownTotal)
                                                    : varLoader.GetIntFromXML(_listedEvent.RenownTotal), captiveHero);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing RenownTotal");
                        dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), captiveHero);
                    }

                // ChangeHealth
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth))
                    try
                    {
                        captiveHero.HitPoints += !string.IsNullOrEmpty(_option.HealthTotal)
                            ? varLoader.GetIntFromXML(_option.HealthTotal)
                            : varLoader.GetIntFromXML(_listedEvent.HealthTotal);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing HealthTotal");
                        captiveHero.HitPoints += MBRandom.RandomInt(-20, 20);
                    }

                // Impregnation
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
                    try
                    {
                        i.CaptivityImpregnationChance(captiveHero, !string.IsNullOrEmpty(_option.PregnancyRiskModifier)
                                                          ? varLoader.GetIntFromXML(_option.PregnancyRiskModifier)
                                                          : varLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier));
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                        i.CaptivityImpregnationChance(captiveHero, 30);
                    }
                else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                    try
                    {
                        i.CaptivityImpregnationChance(captiveHero, !string.IsNullOrEmpty(_option.PregnancyRiskModifier)
                                                          ? varLoader.GetIntFromXML(_option.PregnancyRiskModifier)
                                                          : varLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier), false, false);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                        i.CaptivityImpregnationChance(captiveHero, 30, false, false);
                    }

                // Strip
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Strip)) c.CEStripVictim(captiveHero);
            }

            // Escape
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape))
            {
                if (_listedEvent.Captive.IsHero) EndCaptivityAction.ApplyByEscape(_listedEvent.Captive.HeroObject);
                else PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
            }

            // Gain Random Prisoners
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) dynamics.CEGainRandomPrisoners(PartyBase.MainParty);

            // Kill Prisoner
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner))
            {
                if (_listedEvent.Captive.IsHero) KillCharacterAction.ApplyByExecution(_listedEvent.Captive.HeroObject, Hero.MainHero);
                else PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
            }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor)) dynamics.CEKillPlayer(_listedEvent.Captive.HeroObject);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillAllPrisoners)) c.CEKillPrisoners(args, PartyBase.MainParty.PrisonRoster.Count(), true);
            // Kill Random
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillRandomPrisoners)) c.CEKillPrisoners(args);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripHero) && captiveHero != null)
            {
                if (CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(captiveHero, captiveHero.BattleEquipment, captiveHero.CivilianEquipment);
                InventoryManager.OpenScreenAsInventoryOf(Hero.MainHero.PartyBelongedTo.Party.MobileParty, captiveHero.CharacterObject);
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RebelPrisoners)) { c.CEPrisonerRebel(args); }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.HuntPrisoners)) { c.CEHuntPrisoners(args); }
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
                            var conditionMatched = CEEventChecker.FlagsDoMatchEventConditions(triggeredEvent, _listedEvent.Captive, PartyBase.MainParty);

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

                        try
                        {
                            var triggeredEvent = eventNames[number];
                            triggeredEvent.Captive = _listedEvent.Captive;
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
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
            else if (!string.IsNullOrEmpty(_option.TriggerEventName))
            {
                try
                {
                    var triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                    triggeredEvent.Captive = _listedEvent.Captive;
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

        private void ReqHeroCaptorRelation(ref MenuCallbackArgs args)
        {
            if (_listedEvent.Captive.HeroObject == null) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.ReqHeroCaptorRelationAbove)) ReqHeroCaptorRelationAbove(ref args);
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
            if (string.IsNullOrEmpty(_option.ReqHeroCaptorRelationBelow)) return true;

            if (!(_listedEvent.Captive.HeroObject.GetRelationWithPlayer() > new VariablesLoader().GetFloatFromXML(_option.ReqHeroCaptorRelationBelow))) return false;
            var textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
            textResponse3.SetTextVariable("HERO", _listedEvent.Captive.HeroObject.Name.ToString());
            args.Tooltip = textResponse3;
            args.IsEnabled = false;

            return false;
        }

        private void ReqHeroCaptorRelationAbove(ref MenuCallbackArgs args)
        {
            if (!(_listedEvent.Captive.HeroObject.GetRelationWithPlayer() < new VariablesLoader().GetFloatFromXML(_option.ReqHeroCaptorRelationAbove))) return;
            var textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
            textResponse4.SetTextVariable("HERO", _listedEvent.Captive.HeroObject.Name.ToString());
            args.Tooltip = textResponse4;
            args.IsEnabled = false;
        }

#endregion
    }
}