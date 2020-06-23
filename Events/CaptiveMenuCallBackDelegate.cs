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
            var varLoader = new VariablesLoader();

            args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                       ? "wait_prisoner_female"
                                                       : "wait_prisoner_male");

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
                        if (!_listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = varLoader.GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e) { CECustomHandler.ForceLogToFile("Failed to load BackgroundAnimationSpeed for " + _listedEvent.Name + " : Exception: " + e); }

                    CESubModule.animationSpeed = speed;
                }
                else { CESubModule.animationPlayEvent = false; }
            }
            catch (Exception) { CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name); }

            if (PlayerCaptivity.IsCaptive)
            {
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

            args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
        }

        internal bool CaptiveConditionWaitGameMenu(MenuCallbackArgs args)
        {
            return true;
        }

        internal void CaptiveConsequenceWaitGameMenu(MenuCallbackArgs args) { }

        internal void CaptiveTickWaitGameMenu(MenuCallbackArgs args, CampaignTime dt)
        {
            var captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;

            var text = args.MenuContext.GameMenu.GetText();

            if (PlayerCaptivity.IsCaptive)
            {
                if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null) text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Name);
                else if (PlayerCaptivity.CaptorParty.IsSettlement) text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);
                else text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);
            }

            if (captiveTimeInDays != 0)
            {
                text.SetTextVariable("DAYS", 1);
                text.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);

                text.SetTextVariable("PLURAL", captiveTimeInDays > 1
                                         ? 1
                                         : 0);
            }
            else { text.SetTextVariable("DAYS", 0); }

            if (!PlayerCaptivity.IsCaptive) return;

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.IsActive) PartyBase.MainParty.MobileParty.Position2D = PlayerCaptivity.CaptorParty.MobileParty.Position2D;
            else if (PlayerCaptivity.CaptorParty.IsSettlement) PartyBase.MainParty.MobileParty.Position2D = PlayerCaptivity.CaptorParty.Settlement.GatePosition;
            PlayerCaptivity.CaptorParty.SetAsCameraFollowParty();

            var eventToRun = Campaign.Current.Models.PlayerCaptivityModel.CheckCaptivityChange(Campaign.Current.CampaignDt);
            if (!eventToRun.IsStringNoneOrEmpty()) GameMenu.SwitchToMenu(eventToRun);
        }

        internal void CaptiveEventGameMenu(MenuCallbackArgs args)
        {
            try
            {
                var varLoader = new VariablesLoader();
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
                else { CESubModule.animationPlayEvent = false; }
            }
            catch (Exception) { CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name); }

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

        internal bool CaptiveEventOptionGameMenu(MenuCallbackArgs args)
        {
            var varLoader = new VariablesLoader();

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) args.optionLeaveType = GameMenuOption.LeaveType.Escape;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) args.optionLeaveType = GameMenuOption.LeaveType.Leave;

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
            {
                var content = new ScoresCalculation().AttractivenessScore(Hero.MainHero);
                content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
                MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
            }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                try
                {
                    var level = 0;

                    if (!string.IsNullOrEmpty(_option.GoldTotal)) level = varLoader.GetIntFromXML(_option.GoldTotal);
                    else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = varLoader.GetIntFromXML(_listedEvent.GoldTotal);
                    else CECustomHandler.LogToFile("Missing GoldTotal");
                    MBTextManager.SetTextVariable("MONEY_AMOUNT", level);
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }

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

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                try
                {
                    PartyBase party;

                    party = !PlayerCaptivity.CaptorParty.IsSettlement
                        ? PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Party
                        : PlayerCaptivity.CaptorParty;
                    MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                try
                {
                    PartyBase party;

                    party = PlayerCaptivity.CaptorParty.IsSettlement
                        ? PlayerCaptivity.CaptorParty.Settlement.Parties.First(mobileParty => mobileParty.IsCaravan).Party
                        : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => mobileParty.IsCaravan).Party;

                    MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty))
                try
                {
                    var party = PlayerCaptivity.CaptorParty.IsSettlement
                        ? PlayerCaptivity.CaptorParty.Settlement.Parties.First(mobileParty => mobileParty.IsLordParty).Party
                        : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => mobileParty.IsLordParty).Party;

                    MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }

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
                    if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.Morale < varLoader.GetIntFromXML(_option.ReqMoraleAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoralAbove / Failed "); }

            try
            {
                if (!_option.ReqMoraleBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.Morale > varLoader.GetIntFromXML(_option.ReqMoraleBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoralBelow / Failed "); }

            // ReqTroops
            try
            {
                if (!_option.ReqTroopsAbove.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.NumberOfHealthyMembers < varLoader.GetIntFromXML(_option.ReqTroopsAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqTroopsBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.NumberOfHealthyMembers > varLoader.GetIntFromXML(_option.ReqTroopsBelow))
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
                    if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < varLoader.GetIntFromXML(_option.ReqMaleTroopsAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleTroopsBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < varLoader.GetIntFromXML(_option.ReqMaleTroopsBelow))
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
                    if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < varLoader.GetIntFromXML(_option.ReqFemaleTroopsAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > varLoader.GetIntFromXML(_option.ReqFemaleTroopsBelow))
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
                    if (PlayerCaptivity.CaptorParty.NumberOfPrisoners < varLoader.GetIntFromXML(_option.ReqCaptivesAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqCaptivesBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.NumberOfPrisoners > varLoader.GetIntFromXML(_option.ReqCaptivesBelow))
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
                    if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < varLoader.GetIntFromXML(_option.ReqMaleCaptivesAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqMaleCaptivesBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < varLoader.GetIntFromXML(_option.ReqMaleCaptivesBelow))
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
                    if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < varLoader.GetIntFromXML(_option.ReqFemaleCaptivesAbove))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed "); }

            try
            {
                if (!_option.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty())
                    if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > varLoader.GetIntFromXML(_option.ReqFemaleCaptivesBelow))
                    {
                        args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                        args.IsEnabled = false;
                    }
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed "); }

            if (PlayerCaptivity.CaptorParty.LeaderHero != null)
            {
                // ReqHeroCaptorRelation
                try
                {
                    if (!string.IsNullOrEmpty(_option.ReqHeroCaptorRelationAbove))
                        if (PlayerCaptivity.CaptorParty.LeaderHero.GetRelationWithPlayer() < varLoader.GetFloatFromXML(_option.ReqHeroCaptorRelationAbove))
                        {
                            var textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
                            textResponse4.SetTextVariable("HERO", PlayerCaptivity.CaptorParty.LeaderHero.Name.ToString());
                            args.Tooltip = textResponse4;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationAbove / Failed "); }

                try
                {
                    if (!string.IsNullOrEmpty(_option.ReqHeroCaptorRelationBelow))
                        if (PlayerCaptivity.CaptorParty.LeaderHero.GetRelationWithPlayer() > varLoader.GetFloatFromXML(_option.ReqHeroCaptorRelationBelow))
                        {
                            var textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
                            textResponse3.SetTextVariable("HERO", PlayerCaptivity.CaptorParty.LeaderHero.Name.ToString());
                            args.Tooltip = textResponse3;
                            args.IsEnabled = false;
                        }
                }
                catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationBelow / Failed "); }
            }

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

            // ReqTrait
            if (!_option.ReqHeroTrait.IsStringNoneOrEmpty())
            {
                var traitLevel = 0;

                try { traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
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

            // ReqCaptorTrait
            if (!_option.ReqCaptorTrait.IsStringNoneOrEmpty())
            {
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
                    if (!string.IsNullOrEmpty(_option.ReqCaptorTraitLevelAbove))
                        if (traitLevel < varLoader.GetIntFromXML(_option.ReqCaptorTraitLevelAbove))
                        {
                            var text = GameTexts.FindText("str_CE_trait_captor_level", "low");
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
                            var text = GameTexts.FindText("str_CE_trait_captor_level", "high");
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

                try { skillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst(skill => { return skill.StringId == _option.ReqHeroSkill; })); }
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

            // ReqCaptorSkill
            if (!_option.ReqCaptorSkill.IsStringNoneOrEmpty())
            {
                if (PlayerCaptivity.CaptorParty.LeaderHero == null) args.IsEnabled = false;
                var skillLevel = 0;

                try { skillLevel = PlayerCaptivity.CaptorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst(skill => { return skill.StringId == _option.ReqCaptorSkill; })); }
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
                            var text = GameTexts.FindText("str_CE_skill_captor_level", "low");
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
                            var text = GameTexts.FindText("str_CE_skill_captor_level", "high");
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

        internal void CaptiveEventOptionConsequenceGameMenu(MenuCallbackArgs args)
        {
            var varLoader = new VariablesLoader();
            var dynamics = new Dynamics();
            var score = new ScoresCalculation();
            var i = new ImpregnationSystem();
            var c = new CaptiveSpecifics();

            //XP
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP))
                try
                {
                    var skillToLevel = "";

                    if (!string.IsNullOrEmpty(_option.SkillToLevel)) skillToLevel = _option.SkillToLevel;
                    else if (!string.IsNullOrEmpty(_listedEvent.SkillToLevel)) skillToLevel = _listedEvent.SkillToLevel;
                    else CECustomHandler.LogToFile("Missing SkillToLevel");

                    foreach (var skillObject in SkillObject.All.Where(skillObject => skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)) dynamics.GainSkills(skillObject, 50, 100);
                }
                catch (Exception) { CECustomHandler.LogToFile("GiveXP Failed"); }

            // Leave Spouse
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) dynamics.ChangeSpouse(Hero.MainHero, null);

            // Force Marry
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor))
                if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.LeaderHero != null)
                    dynamics.ChangeSpouse(Hero.MainHero, PlayerCaptivity.CaptorParty.LeaderHero);

            // Change Clan
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeClan))
                if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.LeaderHero != null)
                    dynamics.ChangeClan(Hero.MainHero, PlayerCaptivity.CaptorParty.LeaderHero);

            // Gold
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
            {
                var content = score.AttractivenessScore(Hero.MainHero);
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
                    else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = varLoader.GetIntFromXML(_listedEvent.GoldTotal);
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
                    else if (!string.IsNullOrEmpty(_listedEvent.TraitTotal)) level = varLoader.GetIntFromXML(_listedEvent.TraitTotal);
                    else CECustomHandler.LogToFile("Missing Trait TraitTotal");

                    if (!string.IsNullOrEmpty(_option.TraitToLevel)) dynamics.TraitModifier(Hero.MainHero, _option.TraitToLevel, level);
                    else if (!string.IsNullOrEmpty(_listedEvent.TraitToLevel)) dynamics.TraitModifier(Hero.MainHero, _listedEvent.TraitToLevel, level);
                    else CECustomHandler.LogToFile("Missing TraitToLevel");
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }

            // ChangeSkill
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill))
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

            // Slavery Level
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel))
                try
                {
                    if (!string.IsNullOrEmpty(_option.SlaveryTotal)) { dynamics.VictimSlaveryModifier(varLoader.GetIntFromXML(_option.SlaveryTotal), Hero.MainHero); }
                    else if (!string.IsNullOrEmpty(_listedEvent.SlaveryTotal)) { dynamics.VictimSlaveryModifier(varLoader.GetIntFromXML(_listedEvent.SlaveryTotal), Hero.MainHero); }
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
                    else if (!string.IsNullOrEmpty(_listedEvent.ProstitutionTotal)) { dynamics.VictimProstitutionModifier(varLoader.GetIntFromXML(_listedEvent.ProstitutionTotal), Hero.MainHero); }
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
                    else if (!string.IsNullOrEmpty(_listedEvent.RenownTotal)) { dynamics.RenownModifier(varLoader.GetIntFromXML(_listedEvent.RenownTotal), Hero.MainHero); }
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
                    else if (!string.IsNullOrEmpty(_listedEvent.HealthTotal)) { Hero.MainHero.HitPoints += varLoader.GetIntFromXML(_listedEvent.HealthTotal); }
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
                    if (!string.IsNullOrEmpty(_option.MoraleTotal)) { dynamics.MoralChange(varLoader.GetIntFromXML(_option.MoraleTotal), PlayerCaptivity.CaptorParty); }
                    else if (!string.IsNullOrEmpty(_listedEvent.MoraleTotal)) { dynamics.MoralChange(varLoader.GetIntFromXML(_listedEvent.MoraleTotal), PlayerCaptivity.CaptorParty); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing MoralTotal");
                        dynamics.MoralChange(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }

            // Impregnation By Leader
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
                try
                {
                    if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { i.CaptivityImpregnationChance(Hero.MainHero, varLoader.GetIntFromXML(_option.PregnancyRiskModifier)); }
                    else if (!string.IsNullOrEmpty(_listedEvent.PregnancyRiskModifier)) { i.CaptivityImpregnationChance(Hero.MainHero, varLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier)); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                        i.CaptivityImpregnationChance(Hero.MainHero, 30);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }

            // Impregnation
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                try
                {
                    if (!string.IsNullOrEmpty(_option.PregnancyRiskModifier)) { i.CaptivityImpregnationChance(Hero.MainHero, varLoader.GetIntFromXML(_option.PregnancyRiskModifier), false, false); }
                    else if (!string.IsNullOrEmpty(_listedEvent.PregnancyRiskModifier)) { i.CaptivityImpregnationChance(Hero.MainHero, varLoader.GetIntFromXML(_listedEvent.PregnancyRiskModifier), false, false); }
                    else
                    {
                        CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                        i.CaptivityImpregnationChance(Hero.MainHero, 30, false, false);
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }

            // Specific Captor
            if (!PlayerCaptivity.CaptorParty.IsSettlement && PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.LeaderHero != null)
            {
                // Captor Relations
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation))
                    try
                    {
                        dynamics.RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, !string.IsNullOrEmpty(_option.RelationTotal)
                                                       ? varLoader.GetIntFromXML(_option.RelationTotal)
                                                       : varLoader.GetIntFromXML(_listedEvent.RelationTotal));
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing RelationTotal");
                        dynamics.RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, MBRandom.RandomInt(-5, 5));
                    }

                // Captor Gold
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold))
                {
                    var content = score.AttractivenessScore(Hero.MainHero);
                    var currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                    content += currentValue / 2;
                    content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
                    GiveGoldAction.ApplyBetweenCharacters(null, PlayerCaptivity.CaptorParty.LeaderHero, content);
                }

                // Captor Change Gold
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                    try
                    {
                        var level = 0;

                        if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = varLoader.GetIntFromXML(_option.CaptorGoldTotal);
                        else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = varLoader.GetIntFromXML(_listedEvent.CaptorGoldTotal);
                        else CECustomHandler.LogToFile("Missing CaptorGoldTotal");

                        GiveGoldAction.ApplyBetweenCharacters(null, PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, level);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }

                // Captor Trait
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait))
                    try
                    {
                        var level = varLoader.GetIntFromXML(!string.IsNullOrEmpty(_option.TraitTotal)
                                                                ? _option.TraitTotal
                                                                : _listedEvent.TraitTotal);

                        dynamics.TraitModifier(PlayerCaptivity.CaptorParty.LeaderHero, !string.IsNullOrEmpty(_option.TraitToLevel)
                                                   ? _option.TraitToLevel
                                                   : _listedEvent.TraitToLevel, level);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing Trait Flags"); }

                // Captor Renown
                if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown))
                    try
                    {
                        if (!string.IsNullOrEmpty(_option.RenownTotal)) dynamics.RenownModifier(varLoader.GetIntFromXML(_option.RenownTotal), PlayerCaptivity.CaptorParty.LeaderHero);
                        else dynamics.RenownModifier(varLoader.GetIntFromXML(_listedEvent.RenownTotal), PlayerCaptivity.CaptorParty.LeaderHero);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing RenownTotal");
                        dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty.LeaderHero);
                    }
            }

            // Sold Events
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                try
                {
                    MobileParty party = null;

                    if (PlayerCaptivity.CaptorParty.IsSettlement)
                    {
                        CECampaignBehavior.ExtraProps.Owner = null;
                        party = PlayerCaptivity.CaptorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                    }
                    else
                    {
                        CECampaignBehavior.ExtraProps.Owner = null;
                        party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                    }

                    if (party != null) c.CECaptivityChange(args, party.Party);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Caravan"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                try
                {
                    var party = !PlayerCaptivity.CaptorParty.IsSettlement
                        ? PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Party
                        : PlayerCaptivity.CaptorParty;

                    CECampaignBehavior.ExtraProps.Owner = null;
                    c.CECaptivityChange(args, party);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty))
                try
                {
                    MobileParty party = null;

                    party = PlayerCaptivity.CaptorParty.IsSettlement
                        ? PlayerCaptivity.CaptorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty)
                        : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty);

                    if (party != null)
                    {
                        c.CECaptivityChange(args, party.Party);
                        CECampaignBehavior.ExtraProps.Owner = party.LeaderHero;
                    }
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Lord"); }

            // Work In Progress Sold Event
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable))
                try
                {
                    var settlement = PlayerCaptivity.CaptorParty.IsSettlement
                        ? PlayerCaptivity.CaptorParty.Settlement
                        : PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement;

                    var notable = settlement.Notables.Where(findFirstNotable => !findFirstNotable.IsFemale).GetRandomElement();
                    CECampaignBehavior.ExtraProps.Owner = notable;
                    c.CECaptivityChange(args, settlement.Party);
                }
                catch (Exception) { CECustomHandler.LogToFile("Failed to get Settlement"); }

            // Gain Random Prisoners
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) dynamics.CEGainRandomPrisoners(PlayerCaptivity.CaptorParty);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor) && PlayerCaptivity.CaptorParty.NumberOfAllMembers > 1)
            {
                if (PlayerCaptivity.CaptorParty.LeaderHero != null) KillCharacterAction.ApplyByMurder(PlayerCaptivity.CaptorParty.LeaderHero, Hero.MainHero);
                else PlayerCaptivity.CaptorParty.MemberRoster.AddToCounts(PlayerCaptivity.CaptorParty.Leader, -1);
            }

            // Kill Captor
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor) && PlayerCaptivity.CaptorParty.NumberOfAllMembers == 1)
            {
                if (!CESettings.Instance.SexualContent)
                    GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                              ? "CE_captivity_escape_success"
                                              : "CE_captivity_escape_success_male");
                else
                    GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                              ? "CE_captivity_sexual_escape_success"
                                              : "CE_captivity_sexual_escape_success_male");

                if (PlayerCaptivity.CaptorParty.LeaderHero != null) KillCharacterAction.ApplyByMurder(PlayerCaptivity.CaptorParty.LeaderHero, Hero.MainHero);

                if (PlayerCaptivity.CaptorParty.IsMobile) DestroyPartyAction.Apply(null, PlayerCaptivity.CaptorParty.MobileParty);
            }
            // Kill Hero
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner)) { dynamics.CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero); }
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
                            var conditionMatched = CEEventChecker.FlagsDoMatchEventConditions(triggeredEvent, CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);

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
                            triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                            c.CECaptivityContinue(args);
                        }
                    }
                    else { c.CECaptivityContinue(args); }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                    c.CECaptivityContinue(args);
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
                    c.CECaptivityContinue(args);
                }
            }
            // Escape Event Trigger
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape))
            {
                try
                {
                    c.CECaptivityEscapeAttempt(args, !string.IsNullOrEmpty(_option.EscapeChance)
                                                   ? varLoader.GetIntFromXML(_option.EscapeChance)
                                                   : varLoader.GetIntFromXML(_listedEvent.EscapeChance));
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing EscapeChance");
                    c.CECaptivityEscapeAttempt(args);
                }
            }
            // Escape Trigger
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) { c.CECaptivityEscape(args); }
            // Leave Trigger
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) { c.CECaptivityLeave(args); }
            else { c.CECaptivityContinue(args); }
        }
    }
}