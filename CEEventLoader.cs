using CaptivityEvents.CampaignBehaviours;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Events
{
    internal class CEEventLoader
    {
        // Waiting Menus
        public static string CEWaitingList()
        {
            List<string> eventNames = new List<string>();

            if (CESubModule.CEWaitingList != null && CESubModule.CEWaitingList.Count > 0)
            {
                CECustomHandler.LogToFile("Having " + CESubModule.CEWaitingList.Count + " of events to weight and check conditions on.");

                foreach (CEEvent listEvent in CESubModule.CEWaitingList)
                {
                    string result = CEEventChecker.FlagsDoMatchEventConditions(listEvent, CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                    if (result == null)
                    {
                        int weightedChance = 10;
                        try
                        {
                            if (listEvent.WeightedChanceOfOccuring != null)
                            {
                                weightedChance = GetIntFromXML(listEvent.WeightedChanceOfOccuring);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring");
                        }
                        for (int a = weightedChance; a > 0; a--)
                        {
                            eventNames.Add(listEvent.Name);
                        }
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
                        int test = MBRandom.Random.Next(0, eventNames.Count - 1);
                        string randomWeightedChoice = eventNames[test];
                        return randomWeightedChoice;
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile("Waiting Menu: Something is broken?");
                }
            }
            CECustomHandler.LogToFile("Number of Filitered events is " + eventNames.Count);
            return null;
        }

        // Event Loaders
        public static void CELoadRandomEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            gameStarter.AddGameMenu(listedEvent.Name, listedEvent.Text, (MenuCallbackArgs args) =>
            {
                if (Hero.MainHero.IsFemale)
                {
                    args.MenuContext.SetBackgroundMeshName("wait_prisoner_female");
                }
                else
                {
                    args.MenuContext.SetBackgroundMeshName("wait_prisoner_male");
                }

                try
                {
                    string BackgroundName = listedEvent.BackgroundName;
                    if (BackgroundName != null && BackgroundName != "")
                    {
                        CESubModule.LoadTexture(BackgroundName);
                    }
                    else
                    {
                        CESubModule.LoadTexture("default_random");
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Failed to load background for " + listedEvent.Name);
                    CESubModule.LoadTexture("default_random");
                }

                MBTextManager.SetTextVariable("ISFEMALE", (Hero.MainHero.IsFemale) ? 1 : 0);
            }, GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.none, null);

            // Leave if no Options
            if (listedEvent.Options == null)
            {
                return;
            }
            // Sort Options
            List<Option> sorted = listedEvent.Options.OrderBy(item => GetIntFromXML(item.Order)).ToList();

            foreach (Option op in sorted)
            {
                gameStarter.AddGameMenuOption(listedEvent.Name, listedEvent.Name + op.Order, op.OptionText,
                (MenuCallbackArgs args) =>
                {
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                    {
                        try
                        {
                            PartyBase party = PartyBase.MainParty.MobileParty.CurrentSettlement.Party;
                            MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Settlement");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                    {
                        try
                        {
                            MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                            if (party != null)
                            {
                                MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name, false);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Caravan");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty))
                    {
                        try
                        {
                            MobileParty party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => { return mobileParty.IsLordParty; });
                            if (party != null)
                            {
                                MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name, false);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Lord");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
                    {
                        int content = AttractivenessScore(Hero.MainHero);
                        content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveGold; });
                        MBTextManager.SetTextVariable("MONEY_AMOUNT", content, false);
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.GoldTotal != null && op.GoldTotal != "")
                            {
                                level = GetIntFromXML(op.GoldTotal);
                            }
                            else if (listedEvent.GoldTotal != null && listedEvent.GoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.GoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing GoldTotal");
                            }
                            MBTextManager.SetTextVariable("MONEY_AMOUNT", level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid GoldTotal");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.BribeAndEscape))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Default;
                    }

                    // ReqMorale
                    try
                    {
                        if (!op.ReqMoraleAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.IsMobile && PartyBase.MainParty.MobileParty.Morale < CEEventLoader.GetIntFromXML(op.ReqMoraleAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMoraleBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.IsMobile && PartyBase.MainParty.MobileParty.Morale > CEEventLoader.GetIntFromXML(op.ReqMoraleBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed ");
                    }

                    // ReqTroops
                    try
                    {
                        if (!op.ReqTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfHealthyMembers < GetIntFromXML(op.ReqTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfHealthyMembers > GetIntFromXML(op.ReqTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed ");
                    }

                    // ReqMaleTroops
                    try
                    {
                        if (!op.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMaleTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed ");
                    }

                    // ReqFemaleTroops
                    try
                    {
                        if (!op.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqFemaleTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > GetIntFromXML(op.ReqFemaleTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed ");
                    }

                    // ReqCaptives
                    try
                    {
                        if (!op.ReqCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfPrisoners < GetIntFromXML(op.ReqCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfPrisoners > GetIntFromXML(op.ReqCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed ");
                    }

                    // ReqMaleCaptives
                    try
                    {
                        if (!op.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMaleCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed ");
                    }

                    // ReqFemaleCaptives
                    try
                    {
                        if (!op.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqFemaleCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > GetIntFromXML(op.ReqFemaleCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed ");
                    }

                    // ReqHeroHealthPercentage
                    try
                    {
                        if (op.ReqHeroHealthAbovePercentage != null && op.ReqHeroHealthAbovePercentage != "")
                        {
                            if (Hero.MainHero.HitPoints < GetIntFromXML(op.ReqHeroHealthAbovePercentage))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_health", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroHealthAbovePercentage / Failed ");
                    }
                    try
                    {
                        if (op.ReqHeroHealthBelowPercentage != null && op.ReqHeroHealthBelowPercentage != "")
                        {
                            if (Hero.MainHero.HitPoints > GetIntFromXML(op.ReqHeroHealthBelowPercentage))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_health", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroHealthBelowPercentage / Failed ");
                    }

                    // ReqSlavery
                    int slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);
                    try
                    {
                        if (op.ReqHeroSlaveLevelAbove != null && op.ReqHeroSlaveLevelAbove != "")
                        {
                            if (slave < GetIntFromXML(op.ReqHeroSlaveLevelAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqHeroSlaveLevelBelow != null && op.ReqHeroSlaveLevelBelow != "")
                        {
                            if (slave > GetIntFromXML(op.ReqHeroSlaveLevelBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelBelow / Failed ");
                    }

                    // ReqProstitute
                    int prostitute = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                    try
                    {
                        if (op.ReqHeroProstituteLevelAbove != null && op.ReqHeroProstituteLevelAbove != "")
                        {
                            if (prostitute < GetIntFromXML(op.ReqHeroProstituteLevelAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqHeroProstituteLevelBelow != null && op.ReqHeroProstituteLevelBelow != "")
                        {
                            if (prostitute > GetIntFromXML(op.ReqHeroProstituteLevelBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelBelow / Failed ");
                    }

                    // Req Skill
                    if (!op.ReqHeroSkill.IsStringNoneOrEmpty())
                    {
                        int SkillLevel = 0;
                        try
                        {
                            SkillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == op.ReqHeroSkill; }));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Captive");
                            SkillLevel = 0;
                        }

                        try
                        {
                            if (!op.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel < CEEventLoader.GetIntFromXML(op.ReqHeroSkillLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
                                    text.SetTextVariable("SKILL", op.ReqHeroSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove");
                        }
                        try
                        {
                            if (!op.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel > CEEventLoader.GetIntFromXML(op.ReqHeroSkillLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
                                    text.SetTextVariable("SKILL", op.ReqHeroSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow");
                        }
                    }

                    // Req Trait
                    if (!op.ReqHeroTrait.IsStringNoneOrEmpty())
                    {
                        int TraitLevel = 0;
                        try
                        {
                            TraitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(op.ReqHeroTrait));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Captive");
                            TraitLevel = 0;
                        }

                        try
                        {
                            if (op.ReqHeroTraitLevelAbove != null && op.ReqHeroTraitLevelAbove != "")
                            {
                                if (TraitLevel < CEEventLoader.GetIntFromXML(op.ReqHeroTraitLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_level", "low");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqHeroTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove");
                        }
                        try
                        {
                            if (op.ReqHeroTraitLevelBelow != null && op.ReqHeroTraitLevelBelow != "")
                            {
                                if (TraitLevel > CEEventLoader.GetIntFromXML(op.ReqHeroTraitLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_level", "high");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqHeroTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow");
                        }
                    }

                    // ReqGold
                    try
                    {
                        if (op.ReqGoldAbove != null && op.ReqGoldAbove != "")
                        {
                            if (Hero.MainHero.Gold < GetIntFromXML(op.ReqGoldAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqGoldBelow != null && op.ReqGoldBelow != "")
                        {
                            if (Hero.MainHero.Gold > GetIntFromXML(op.ReqGoldBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed ");
                    }

                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    //XP
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP))
                    {
                        try
                        {
                            string skillToLevel = "";
                            if (op.SkillToLevel != null && op.SkillToLevel != "")
                            {
                                skillToLevel = op.SkillToLevel;
                            }
                            else if (listedEvent.SkillToLevel != null && listedEvent.SkillToLevel != "")
                            {
                                skillToLevel = listedEvent.SkillToLevel;
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SkillToLevel");
                            }

                            foreach (SkillObject skillObject in SkillObject.All)
                            {
                                if (skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)
                                {
                                    GainSkills(skillObject, 50, 100);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("GiveXP Failed");
                        }
                    }
                    // Leave Spouse
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse))
                    {
                        ChangeSpouse(Hero.MainHero, null);
                    }
                    // Gold
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
                    {
                        int content = AttractivenessScore(Hero.MainHero);
                        int currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                        content += currentValue / 2;
                        content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveGold; });
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content, false);
                    }
                    // Change Gold
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.GoldTotal != null && op.GoldTotal != "")
                            {
                                level = GetIntFromXML(op.GoldTotal);
                            }
                            else if (listedEvent.GoldTotal != null && listedEvent.GoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.GoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing GoldTotal");
                            }

                            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid GoldTotal");
                        }
                    }
                    // ChangeTrait
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait))
                    {
                        try
                        {
                            int level = 0;
                            if (op.TraitTotal != null && op.TraitTotal != "")
                            {
                                level = GetIntFromXML(op.TraitTotal);
                            }
                            else if (listedEvent.TraitTotal != null && listedEvent.TraitTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.TraitTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing Trait TraitTotal");
                            }

                            if (op.TraitToLevel != null && op.TraitToLevel != "")
                            {
                                TraitModifier(Hero.MainHero, op.TraitToLevel, level);
                            }
                            else if (listedEvent.TraitToLevel != null && listedEvent.TraitToLevel != "")
                            {
                                TraitModifier(Hero.MainHero, listedEvent.TraitToLevel, level);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing TraitToLevel");
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Flags");
                        }
                    }
                    // ChangeSkill
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill))
                    {
                        try
                        {
                            int level = 0;
                            if (!op.SkillTotal.IsStringNoneOrEmpty())
                            {
                                level = GetIntFromXML(op.SkillTotal);
                            }
                            else if (!listedEvent.SkillTotal.IsStringNoneOrEmpty())
                            {
                                level = GetIntFromXML(listedEvent.SkillTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing Skill SkillTotal");
                            }

                            if (!op.SkillToLevel.IsStringNoneOrEmpty())
                            {
                                SkillModifier(Hero.MainHero, op.SkillToLevel, level);
                            }
                            else if (!listedEvent.SkillToLevel.IsStringNoneOrEmpty())
                            {
                                SkillModifier(Hero.MainHero, listedEvent.SkillToLevel, level);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SkillToLevel");
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Flags");
                        }
                    }

                    // Slavery Level
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel))
                    {
                        try
                        {
                            if (op.SlaveryTotal != null && op.SlaveryTotal != "")
                            {
                                VictimSlaveryModifier(GetIntFromXML(op.SlaveryTotal), Hero.MainHero);
                            }
                            else if (listedEvent.SlaveryTotal != null && listedEvent.SlaveryTotal != "")
                            {
                                VictimSlaveryModifier(GetIntFromXML(listedEvent.SlaveryTotal), Hero.MainHero);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SlaveryTotal");
                                VictimSlaveryModifier(1, Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid SlaveryTotal");
                        }
                    }
                    // Slavery Flags
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag))
                    {
                        VictimSlaveryModifier(1, Hero.MainHero, true, false, true);
                    }
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag))
                    {
                        VictimSlaveryModifier(0, Hero.MainHero, true, false, true);
                    }
                    // Prostitution Level
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel))
                    {
                        try
                        {
                            if (op.ProstitutionTotal != null && op.ProstitutionTotal != "")
                            {
                                VictimProstitutionModifier(GetIntFromXML(op.ProstitutionTotal), Hero.MainHero);
                            }
                            else if (listedEvent.ProstitutionTotal != null && listedEvent.ProstitutionTotal != "")
                            {
                                VictimProstitutionModifier(GetIntFromXML(listedEvent.ProstitutionTotal), Hero.MainHero);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing ProstitutionTotal");
                                VictimProstitutionModifier(1, Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ProstitutionTotal");
                        }
                    }
                    // Prostitution Flags
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag))
                    {
                        VictimProstitutionModifier(1, Hero.MainHero, true, false, true);
                    }
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag))
                    {
                        VictimProstitutionModifier(0, Hero.MainHero, true, false, true);
                    }
                    // Renown
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown))
                    {
                        try
                        {
                            if (op.RenownTotal != null && op.RenownTotal != "")
                            {
                                RenownModifier(GetIntFromXML(op.RenownTotal), Hero.MainHero);
                            }
                            else if (listedEvent.RenownTotal != null && listedEvent.RenownTotal != "")
                            {
                                RenownModifier(GetIntFromXML(listedEvent.RenownTotal), Hero.MainHero);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing RenownTotal");
                                RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid RenownTotal");
                        }
                    }
                    // ChangeHealth
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth))
                    {
                        try
                        {
                            if (op.HealthTotal != null && op.HealthTotal != "")
                            {
                                Hero.MainHero.HitPoints += GetIntFromXML(op.HealthTotal);
                            }
                            else if (listedEvent.HealthTotal != null && listedEvent.HealthTotal != "")
                            {
                                Hero.MainHero.HitPoints += GetIntFromXML(listedEvent.HealthTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Invalid HealthTotal");
                                Hero.MainHero.HitPoints += MBRandom.RandomInt(-20, 20);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing HealthTotal");
                        }
                    }
                    // ChangeMorale
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale))
                    {
                        try
                        {
                            if (op.MoraleTotal != null && op.MoraleTotal != "")
                            {
                                MoralChange(GetIntFromXML(op.MoraleTotal), PartyBase.MainParty);
                            }
                            else if (listedEvent.MoraleTotal != null && listedEvent.MoraleTotal != "")
                            {
                                MoralChange(GetIntFromXML(listedEvent.MoraleTotal), PartyBase.MainParty);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing MoralTotal");
                                MoralChange(MBRandom.RandomInt(-5, 5), PartyBase.MainParty);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid MoralTotal");
                        }
                    }

                    // Impregnation
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                    {
                        try
                        {
                            if (op.PregnancyRiskModifier != null && op.PregnancyRiskModifier != "")
                            {
                                ImpregnationChance(Hero.MainHero, GetIntFromXML(op.PregnancyRiskModifier), false);
                            }
                            else if (listedEvent.PregnancyRiskModifier != null && listedEvent.PregnancyRiskModifier != "")
                            {
                                ImpregnationChance(Hero.MainHero, GetIntFromXML(listedEvent.PregnancyRiskModifier), false);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                                ImpregnationChance(Hero.MainHero, 30, false);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid PregnancyRiskModifier");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners))
                    {
                        CEGainRandomPrisoners(PartyBase.MainParty);
                    }

                    if (Hero.MainHero.PartyBelongedTo.CurrentSettlement != null)
                    {
                        // Sold Events
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                        {
                            try
                            {
                                PartyBase party = Hero.MainHero.PartyBelongedTo.CurrentSettlement.Party;
                                PlayerCaptivity.StartCaptivity(Hero.MainHero.PartyBelongedTo.CurrentSettlement.Party);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Settlement");
                            }
                        }

                        // Sold To Caravan
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                        {
                            try
                            {
                                MobileParty party = PartyBase.MainParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                                if (party != null)
                                {
                                    MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name, false);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Caravan");
                            }
                        }

                        // Work In Progress Sold Event
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable))
                        {
                            try
                            {
                                Settlement settlement = PartyBase.MainParty.MobileParty.CurrentSettlement;
                                Hero notable = settlement.Notables.Where(findFirstNotable => { return !findFirstNotable.IsFemale; }).GetRandomElement();
                                CECampaignBehavior.extraVariables.Owner = notable;
                                CECaptivityChange(args, settlement.Party);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Settlement");
                            }
                        }
                    }

                    // Kill Hero
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor))
                    {
                        CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero);
                    }
                    // Random Event Trigger
                    else if (op.TriggerEvents != null && op.TriggerEvents.Length > 0)
                    {
                        List<CEEvent> eventNames = new List<CEEvent>();
                        try
                        {
                            foreach (TriggerEvent triggerEvent in op.TriggerEvents)
                            {
                                CEEvent triggeredEvent = eventList.Find(item => item.Name == triggerEvent.EventName);
                                if (triggeredEvent == null)
                                {
                                    CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                                    continue;
                                }

                                if (!triggerEvent.EventUseConditions.IsStringNoneOrEmpty() && triggerEvent.EventUseConditions == "True")
                                {
                                    string conditionMatched = CEEventChecker.FlagsDoMatchEventConditions(triggeredEvent, CharacterObject.PlayerCharacter);
                                    if (conditionMatched != null)
                                    {
                                        CECustomHandler.LogToFile(conditionMatched);
                                        continue;
                                    }
                                }

                                int weightedChance = 1;
                                try
                                {
                                    if (!triggerEvent.EventWeight.IsStringNoneOrEmpty())
                                    {
                                        weightedChance = GetIntFromXML(triggerEvent.EventWeight);
                                    }
                                    else
                                    {
                                        weightedChance = GetIntFromXML(triggeredEvent.WeightedChanceOfOccuring);
                                    }
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.LogToFile("Missing EventWeight");
                                }

                                for (int a = weightedChance; a > 0; a--)
                                {
                                    eventNames.Add(triggeredEvent);
                                }
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
                                    CECaptorContinue(args);
                                }
                            }
                            else
                            {
                                CECaptorContinue(args);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                            CECaptorContinue(args);
                        }
                    }
                    // Single Event Trigger
                    else if (op.TriggerEventName != null && op.TriggerEventName != "")
                    {
                        try
                        {
                            CEEvent triggeredEvent = eventList.Find(item => item.Name == op.TriggerEventName);
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + op.TriggerEventName + " in events.");
                            CECaptorContinue(args);
                        }
                    }
                    else
                    {
                        CECaptorContinue(args);
                    }
                }, false, GetIntFromXML(op.Order), false);
            }
        }

        public static void CELoadCaptiveEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu))
            {
                gameStarter.AddWaitGameMenu(listedEvent.Name, listedEvent.Text,
                  (MenuCallbackArgs args) =>
                  {
                      if (Hero.MainHero.IsFemale)
                      {
                          args.MenuContext.SetBackgroundMeshName("wait_prisoner_female");
                      }
                      else
                      {
                          args.MenuContext.SetBackgroundMeshName("wait_prisoner_male");
                      }

                      try
                      {
                          string BackgroundName = listedEvent.BackgroundName;
                          if (BackgroundName != null && BackgroundName != "")
                          {
                              CESubModule.LoadTexture(BackgroundName);
                          }
                      }
                      catch (Exception)
                      {
                          CECustomHandler.ForceLogToFile("Failed to load background for " + listedEvent.Name);
                      }

                      if (PlayerCaptivity.IsCaptive)
                      {
                          int captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;

                          TextObject text = args.MenuContext.GameMenu.GetText();
                          if (PlayerCaptivity.CaptorParty.Leader != null)
                          {
                              text.SetTextVariable("CAPTOR_NAME", PlayerCaptivity.CaptorParty.Leader.Name);
                              text.SetTextVariable("ISCAPTORFEMALE", PlayerCaptivity.CaptorParty.Leader.IsFemale ? 1 : 0);
                          }
                          else
                          {
                              text.SetTextVariable("CAPTOR_NAME", new TextObject("{=CESETTINGS0099}captor"));
                              text.SetTextVariable("ISCAPTORFEMALE", 0);
                          }

                          text.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale ? 1 : 0);

                          if (CECampaignBehavior.extraVariables.Owner != null)
                          {
                              text.SetTextVariable("OWNER_NAME", CECampaignBehavior.extraVariables.Owner.Name);
                          }
                          if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
                          {
                              text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Name);
                          }
                          else if (PlayerCaptivity.CaptorParty.IsSettlement)
                          {
                              text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);
                          }

                          if (PlayerCaptivity.CaptorParty.IsMobile)
                          {
                              text.SetTextVariable("ISCARAVAN", PlayerCaptivity.CaptorParty.MobileParty.IsCaravan ? 1 : 0);
                              text.SetTextVariable("ISBANDITS", PlayerCaptivity.CaptorParty.MobileParty.IsBandit || PlayerCaptivity.CaptorParty.MobileParty.IsBanditBossParty ? 1 : 0);
                              text.SetTextVariable("ISLORDPARTY", PlayerCaptivity.CaptorParty.MobileParty.IsLordParty ? 1 : 0);

                              text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);
                          }

                          if (captiveTimeInDays != 0)
                          {
                              text.SetTextVariable("DAYS", 1);
                              text.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);
                              text.SetTextVariable("PLURAL", (captiveTimeInDays > 1) ? 1 : 0);
                          }
                          else
                          {
                              text.SetTextVariable("DAYS", 0);
                          }
                      }

                      args.MenuContext.GameMenu.SetMenuAsWaitMenuAndInitiateWaiting();
                  },
                    (MenuCallbackArgs args) =>
                    {
                        return true;
                    },
                    (MenuCallbackArgs args) =>
                    {
                    },
                  (MenuCallbackArgs args, CampaignTime dt) =>
                  {
                      int captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;

                      TextObject text = args.MenuContext.GameMenu.GetText();

                      if (PlayerCaptivity.IsCaptive)
                      {
                          if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
                          {
                              text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Name);
                          }
                          else if (PlayerCaptivity.CaptorParty.IsSettlement)
                          {
                              text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);
                          }
                          else
                          {
                              text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);
                          }
                      }

                      if (captiveTimeInDays != 0)
                      {
                          text.SetTextVariable("DAYS", 1);
                          text.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);
                          text.SetTextVariable("PLURAL", (captiveTimeInDays > 1) ? 1 : 0);
                      }
                      else
                      {
                          text.SetTextVariable("DAYS", 0);
                      }

                      if (PlayerCaptivity.IsCaptive)
                      {
                          if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.IsActive)
                          {
                              PartyBase.MainParty.MobileParty.Position2D = PlayerCaptivity.CaptorParty.MobileParty.Position2D;
                          }
                          else if (PlayerCaptivity.CaptorParty.IsSettlement)
                          {
                              PartyBase.MainParty.MobileParty.Position2D = PlayerCaptivity.CaptorParty.Settlement.GatePosition;
                          }
                          PlayerCaptivity.CaptorParty.SetAsCameraFollowParty();

                          string eventToRun = Campaign.Current.Models.PlayerCaptivityModel.CheckCaptivityChange(Campaign.Current.CampaignDt);
                          if (!eventToRun.IsStringNoneOrEmpty())
                          {
                              GameMenu.SwitchToMenu(eventToRun);
                          }
                      }
                  },
                  GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption,
                  GameOverlays.MenuOverlayType.None, 0f, GameMenu.MenuFlags.none, null);
            }
            else
            {
                gameStarter.AddGameMenu(listedEvent.Name, listedEvent.Text, (MenuCallbackArgs args) =>
                {
                    try
                    {
                        string BackgroundName = listedEvent.BackgroundName;
                        if (!BackgroundName.IsStringNoneOrEmpty())
                        {
                            CESubModule.LoadTexture(BackgroundName);
                        }

                        if (listedEvent.BackgroundAnimation != null && listedEvent.BackgroundAnimation.Count > 0)
                        {
                            CESubModule.animationImageList = listedEvent.BackgroundAnimation;
                            CESubModule.animationIndex = 0;
                            CESubModule.animationPlayEvent = true;
                            float speed = 0.03f;
                            try
                            {
                                if (!listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty())
                                {
                                    speed = GetFloatFromXML(listedEvent.BackgroundAnimationSpeed);
                                }
                            }
                            catch (Exception e)
                            {
                                CECustomHandler.LogToFile("Failed to load BackgroundAnimationSpeed for " + listedEvent.Name + " : Exception: " + e.ToString());
                            }
                            CESubModule.animationSpeed = speed;
                        }
                        else
                        {
                            CESubModule.animationPlayEvent = false;
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to load background for " + listedEvent.Name);
                    }

                    if (PlayerCaptivity.IsCaptive)
                    {
                        int captiveTimeInDays = PlayerCaptivity.CaptiveTimeInDays;

                        TextObject text = args.MenuContext.GameMenu.GetText();
                        if (PlayerCaptivity.CaptorParty.Leader != null)
                        {
                            text.SetTextVariable("CAPTOR_NAME", PlayerCaptivity.CaptorParty.Leader.Name);
                            text.SetTextVariable("ISCAPTORFEMALE", PlayerCaptivity.CaptorParty.Leader.IsFemale ? 1 : 0);
                        }
                        else
                        {
                            text.SetTextVariable("CAPTOR_NAME", new TextObject("{=CESETTINGS0099}captor"));
                            text.SetTextVariable("ISCAPTORFEMALE", 0);
                        }

                        text.SetTextVariable("ISFEMALE", Hero.MainHero.IsFemale ? 1 : 0);

                        if (CECampaignBehavior.extraVariables.Owner != null)
                        {
                            text.SetTextVariable("OWNER_NAME", CECampaignBehavior.extraVariables.Owner.Name);
                        }

                        if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
                        {
                            text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Name);
                        }
                        else if (PlayerCaptivity.CaptorParty.IsSettlement)
                        {
                            text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);
                        }

                        if (PlayerCaptivity.CaptorParty.IsMobile)
                        {
                            text.SetTextVariable("ISCARAVAN", PlayerCaptivity.CaptorParty.MobileParty.IsCaravan ? 1 : 0);
                            text.SetTextVariable("ISBANDITS", PlayerCaptivity.CaptorParty.MobileParty.IsBandit || PlayerCaptivity.CaptorParty.MobileParty.IsBanditBossParty ? 1 : 0);
                            text.SetTextVariable("ISLORDPARTY", PlayerCaptivity.CaptorParty.MobileParty.IsLordParty ? 1 : 0);

                            text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);
                        }

                        if (captiveTimeInDays != 0)
                        {
                            text.SetTextVariable("DAYS", 1);
                            text.SetTextVariable("NUMBER_OF_DAYS", captiveTimeInDays);
                            text.SetTextVariable("PLURAL", (captiveTimeInDays > 1) ? 1 : 0);
                        }
                        else
                        {
                            text.SetTextVariable("DAYS", 0);
                        }
                    }
                }, GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.none, null);
            }

            // Leave if no Options
            if (listedEvent.Options == null)
            {
                return;
            }

            // Sort Options
            List<Option> sorted = listedEvent.Options.OrderBy(item => GetIntFromXML(item.Order)).ToList();

            foreach (Option op in sorted)
            {
                gameStarter.AddGameMenuOption(listedEvent.Name, listedEvent.Name + op.Order, op.OptionText,
                (MenuCallbackArgs args) =>
                {
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Escape;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
                    {
                        int content = AttractivenessScore(Hero.MainHero);
                        content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveGold; });
                        MBTextManager.SetTextVariable("MONEY_AMOUNT", content, false);
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.GoldTotal != null && op.GoldTotal != "")
                            {
                                level = GetIntFromXML(op.GoldTotal);
                            }
                            else if (listedEvent.GoldTotal != null && listedEvent.GoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.GoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing GoldTotal");
                            }
                            MBTextManager.SetTextVariable("MONEY_AMOUNT", level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid GoldTotal");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.CaptorGoldTotal != null && op.CaptorGoldTotal != "")
                            {
                                level = GetIntFromXML(op.CaptorGoldTotal);
                            }
                            else if (listedEvent.CaptorGoldTotal != null && listedEvent.CaptorGoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.CaptorGoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                            }
                            MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid CaptorGoldTotal");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                    {
                        try
                        {
                            PartyBase party;
                            if (!PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Party;
                            }
                            else
                            {
                                party = PlayerCaptivity.CaptorParty;
                            }
                            MBTextManager.SetTextVariable("BUYERSETTLEMENT", party.Name, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Settlement");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                    {
                        try
                        {
                            PartyBase party;
                            if (PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                party = PlayerCaptivity.CaptorParty.Settlement.Parties.First(mobileParty => { return mobileParty.IsCaravan; }).Party;
                            }
                            else
                            {
                                party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => { return mobileParty.IsCaravan; }).Party;
                            }

                            MBTextManager.SetTextVariable("BUYERCARAVAN", party.Name, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Caravan");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty))
                    {
                        try
                        {
                            PartyBase party;
                            if (PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                party = PlayerCaptivity.CaptorParty.Settlement.Parties.First(mobileParty => { return mobileParty.IsLordParty; }).Party;
                            }
                            else
                            {
                                party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.First(mobileParty => { return mobileParty.IsLordParty; }).Party;
                            }

                            MBTextManager.SetTextVariable("BUYERLORDPARTY", party.Name, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Lord");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.BribeAndEscape))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Default;
                    }

                    // ReqMorale
                    try
                    {
                        if (!op.ReqMoraleAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.Morale < CEEventLoader.GetIntFromXML(op.ReqMoraleAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMoralAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMoraleBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.Morale > CEEventLoader.GetIntFromXML(op.ReqMoraleBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMoralBelow / Failed ");
                    }

                    // ReqTroops
                    try
                    {
                        if (!op.ReqTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.NumberOfHealthyMembers < GetIntFromXML(op.ReqTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.NumberOfHealthyMembers > GetIntFromXML(op.ReqTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed ");
                    }

                    // ReqMaleTroops
                    try
                    {
                        if (!op.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMaleTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed ");
                    }

                    // ReqFemaleTroops
                    try
                    {
                        if (!op.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqFemaleTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > GetIntFromXML(op.ReqFemaleTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed ");
                    }

                    // ReqCaptives
                    try
                    {
                        if (!op.ReqCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.NumberOfPrisoners < GetIntFromXML(op.ReqCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.NumberOfPrisoners > GetIntFromXML(op.ReqCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed ");
                    }

                    // ReqMaleCaptives
                    try
                    {
                        if (!op.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMaleCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed ");
                    }

                    // ReqFemaleCaptives
                    try
                    {
                        if (!op.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqFemaleCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PlayerCaptivity.CaptorParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > GetIntFromXML(op.ReqFemaleCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed ");
                    }

                    if (PlayerCaptivity.CaptorParty.LeaderHero != null)
                    {
                        // ReqHeroCaptorRelation
                        try
                        {
                            if (op.ReqHeroCaptorRelationAbove != null && op.ReqHeroCaptorRelationAbove != "")
                            {
                                if (PlayerCaptivity.CaptorParty.LeaderHero.GetRelationWithPlayer() < GetFloatFromXML(op.ReqHeroCaptorRelationAbove))
                                {
                                    TextObject textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
                                    textResponse4.SetTextVariable("HERO", PlayerCaptivity.CaptorParty.LeaderHero.Name.ToString());
                                    args.Tooltip = textResponse4;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationAbove / Failed ");
                        }
                        try
                        {
                            if (op.ReqHeroCaptorRelationBelow != null && op.ReqHeroCaptorRelationBelow != "")
                            {
                                if (PlayerCaptivity.CaptorParty.LeaderHero.GetRelationWithPlayer() > GetFloatFromXML(op.ReqHeroCaptorRelationBelow))
                                {
                                    TextObject textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
                                    textResponse3.SetTextVariable("HERO", PlayerCaptivity.CaptorParty.LeaderHero.Name.ToString());
                                    args.Tooltip = textResponse3;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationBelow / Failed ");
                        }
                    }

                    // ReqHeroHealthPercentage
                    try
                    {
                        if (op.ReqHeroHealthAbovePercentage != null && op.ReqHeroHealthAbovePercentage != "")
                        {
                            if (Hero.MainHero.HitPoints < GetIntFromXML(op.ReqHeroHealthAbovePercentage))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_health", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroHealthAbovePercentage / Failed ");
                    }
                    try
                    {
                        if (op.ReqHeroHealthBelowPercentage != null && op.ReqHeroHealthBelowPercentage != "")
                        {
                            if (Hero.MainHero.HitPoints > GetIntFromXML(op.ReqHeroHealthBelowPercentage))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_health", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroHealthBelowPercentage / Failed ");
                    }

                    // ReqSlavery
                    int slave = Hero.MainHero.GetSkillValue(CESkills.Slavery);
                    try
                    {
                        if (op.ReqHeroSlaveLevelAbove != null && op.ReqHeroSlaveLevelAbove != "")
                        {
                            if (slave < GetIntFromXML(op.ReqHeroSlaveLevelAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqHeroSlaveLevelBelow != null && op.ReqHeroSlaveLevelBelow != "")
                        {
                            if (slave > GetIntFromXML(op.ReqHeroSlaveLevelBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_slavery_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroSlaveLevelBelow / Failed ");
                    }

                    // ReqProstitute
                    int prostitute = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                    try
                    {
                        if (op.ReqHeroProstituteLevelAbove != null && op.ReqHeroProstituteLevelAbove != "")
                        {
                            if (prostitute < GetIntFromXML(op.ReqHeroProstituteLevelAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqHeroProstituteLevelBelow != null && op.ReqHeroProstituteLevelBelow != "")
                        {
                            if (prostitute > GetIntFromXML(op.ReqHeroProstituteLevelBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_prostitution_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqHeroProstituteLevelBelow / Failed ");
                    }

                    // ReqTrait
                    if (!op.ReqHeroTrait.IsStringNoneOrEmpty())
                    {
                        int TraitLevel = 0;
                        try
                        {
                            TraitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(op.ReqCaptorTrait));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Captive");
                            TraitLevel = 0;
                        }

                        try
                        {
                            if (op.ReqHeroTraitLevelAbove != null && op.ReqHeroTraitLevelAbove != "")
                            {
                                if (TraitLevel < CEEventLoader.GetIntFromXML(op.ReqHeroTraitLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_level", "low");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqHeroTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove");
                        }
                        try
                        {
                            if (op.ReqHeroTraitLevelBelow != null && op.ReqHeroTraitLevelBelow != "")
                            {
                                if (TraitLevel > CEEventLoader.GetIntFromXML(op.ReqHeroTraitLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_level", "high");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqHeroTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow");
                        }
                    }

                    // ReqCaptorTrait
                    if (!op.ReqCaptorTrait.IsStringNoneOrEmpty())
                    {
                        if (PlayerCaptivity.CaptorParty.LeaderHero == null)
                        {
                            args.IsEnabled = false;
                        }
                        int TraitLevel = 0;
                        try
                        {
                            TraitLevel = PlayerCaptivity.CaptorParty.LeaderHero.GetTraitLevel(TraitObject.Find(op.ReqCaptorTrait));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Captor");
                            TraitLevel = 0;
                        }

                        try
                        {
                            if (op.ReqCaptorTraitLevelAbove != null && op.ReqCaptorTraitLevelAbove != "")
                            {
                                if (TraitLevel < CEEventLoader.GetIntFromXML(op.ReqCaptorTraitLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_captor_level", "low");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqCaptorTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove");
                        }
                        try
                        {
                            if (op.ReqCaptorTraitLevelBelow != null && op.ReqCaptorTraitLevelBelow != "")
                            {
                                if (TraitLevel > CEEventLoader.GetIntFromXML(op.ReqCaptorTraitLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_captor_level", "high");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqCaptorTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow");
                        }
                    }

                    // ReqSkill
                    if (!op.ReqHeroSkill.IsStringNoneOrEmpty())
                    {
                        int SkillLevel = 0;
                        try
                        {
                            SkillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == op.ReqHeroSkill; }));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Captive");
                            SkillLevel = 0;
                        }

                        try
                        {
                            if (!op.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel < CEEventLoader.GetIntFromXML(op.ReqHeroSkillLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
                                    text.SetTextVariable("SKILL", op.ReqHeroSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove");
                        }
                        try
                        {
                            if (!op.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel > CEEventLoader.GetIntFromXML(op.ReqHeroSkillLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
                                    text.SetTextVariable("SKILL", op.ReqHeroSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow");
                        }
                    }

                    // ReqCaptorSkill
                    if (!op.ReqCaptorSkill.IsStringNoneOrEmpty())
                    {
                        if (PlayerCaptivity.CaptorParty.LeaderHero == null)
                        {
                            args.IsEnabled = false;
                        }
                        int SkillLevel = 0;
                        try
                        {
                            SkillLevel = PlayerCaptivity.CaptorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == op.ReqCaptorSkill; }));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Captor");
                            SkillLevel = 0;
                        }

                        try
                        {
                            if (!op.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel < CEEventLoader.GetIntFromXML(op.ReqCaptorSkillLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_captor_level", "low");
                                    text.SetTextVariable("SKILL", op.ReqCaptorSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove");
                        }
                        try
                        {
                            if (op.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel > CEEventLoader.GetIntFromXML(op.ReqCaptorSkillLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_captor_level", "high");
                                    text.SetTextVariable("SKILL", op.ReqCaptorSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow");
                        }
                    }

                    // ReqGold
                    try
                    {
                        if (op.ReqGoldAbove != null && op.ReqGoldAbove != "")
                        {
                            if (Hero.MainHero.Gold < GetIntFromXML(op.ReqGoldAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqGoldBelow != null && op.ReqGoldBelow != "")
                        {
                            if (Hero.MainHero.Gold > GetIntFromXML(op.ReqGoldBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed ");
                    }

                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    //XP
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP))
                    {
                        try
                        {
                            string skillToLevel = "";
                            if (op.SkillToLevel != null && op.SkillToLevel != "")
                            {
                                skillToLevel = op.SkillToLevel;
                            }
                            else if (listedEvent.SkillToLevel != null && listedEvent.SkillToLevel != "")
                            {
                                skillToLevel = listedEvent.SkillToLevel;
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SkillToLevel");
                            }

                            foreach (SkillObject skillObject in SkillObject.All)
                            {
                                if (skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)
                                {
                                    GainSkills(skillObject, 50, 100);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("GiveXP Failed");
                        }
                    }
                    // Leave Spouse
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse))
                    {
                        ChangeSpouse(Hero.MainHero, null);
                    }
                    // Force Marry
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor))
                    {
                        if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.LeaderHero != null)
                        {
                            ChangeSpouse(Hero.MainHero, PlayerCaptivity.CaptorParty.LeaderHero);
                        }
                    }
                    // Change Clan
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeClan))
                    {
                        if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.LeaderHero != null)
                        {
                            ChangeClan(Hero.MainHero, PlayerCaptivity.CaptorParty.LeaderHero);
                        }
                    }
                    // Gold
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
                    {
                        int content = AttractivenessScore(Hero.MainHero);
                        int currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                        content += currentValue / 2;
                        content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveGold; });
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content, false);
                    }
                    // Change Gold
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.GoldTotal != null && op.GoldTotal != "")
                            {
                                level = GetIntFromXML(op.GoldTotal);
                            }
                            else if (listedEvent.GoldTotal != null && listedEvent.GoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.GoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing GoldTotal");
                            }

                            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid GoldTotal");
                        }
                    }
                    // ChangeTrait
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait))
                    {
                        try
                        {
                            int level = 0;
                            if (op.TraitTotal != null && op.TraitTotal != "")
                            {
                                level = GetIntFromXML(op.TraitTotal);
                            }
                            else if (listedEvent.TraitTotal != null && listedEvent.TraitTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.TraitTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing Trait TraitTotal");
                            }

                            if (op.TraitToLevel != null && op.TraitToLevel != "")
                            {
                                TraitModifier(Hero.MainHero, op.TraitToLevel, level);
                            }
                            else if (listedEvent.TraitToLevel != null && listedEvent.TraitToLevel != "")
                            {
                                TraitModifier(Hero.MainHero, listedEvent.TraitToLevel, level);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing TraitToLevel");
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Flags");
                        }
                    }

                    // ChangeSkill
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill))
                    {
                        try
                        {
                            int level = 0;
                            if (!op.SkillTotal.IsStringNoneOrEmpty())
                            {
                                level = GetIntFromXML(op.SkillTotal);
                            }
                            else if (!listedEvent.SkillTotal.IsStringNoneOrEmpty())
                            {
                                level = GetIntFromXML(listedEvent.SkillTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing Skill SkillTotal");
                            }

                            if (!op.SkillToLevel.IsStringNoneOrEmpty())
                            {
                                SkillModifier(Hero.MainHero, op.SkillToLevel, level);
                            }
                            else if (!listedEvent.SkillToLevel.IsStringNoneOrEmpty())
                            {
                                SkillModifier(Hero.MainHero, listedEvent.SkillToLevel, level);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SkillToLevel");
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Flags");
                        }
                    }

                    // Slavery Level
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel))
                    {
                        try
                        {
                            if (op.SlaveryTotal != null && op.SlaveryTotal != "")
                            {
                                VictimSlaveryModifier(GetIntFromXML(op.SlaveryTotal), Hero.MainHero);
                            }
                            else if (listedEvent.SlaveryTotal != null && listedEvent.SlaveryTotal != "")
                            {
                                VictimSlaveryModifier(GetIntFromXML(listedEvent.SlaveryTotal), Hero.MainHero);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SlaveryTotal");
                                VictimSlaveryModifier(1, Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid SlaveryTotal");
                        }
                    }
                    // Slavery Flags
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag))
                    {
                        VictimSlaveryModifier(1, Hero.MainHero, true, false, true);
                    }
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag))
                    {
                        VictimSlaveryModifier(0, Hero.MainHero, true, false, true);
                    }
                    // Prostitution Level
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel))
                    {
                        try
                        {
                            if (op.ProstitutionTotal != null && op.ProstitutionTotal != "")
                            {
                                VictimProstitutionModifier(GetIntFromXML(op.ProstitutionTotal), Hero.MainHero);
                            }
                            else if (listedEvent.ProstitutionTotal != null && listedEvent.ProstitutionTotal != "")
                            {
                                VictimProstitutionModifier(GetIntFromXML(listedEvent.ProstitutionTotal), Hero.MainHero);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing ProstitutionTotal");
                                VictimProstitutionModifier(1, Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ProstitutionTotal");
                        }
                    }
                    // Prostitution Flags
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag))
                    {
                        VictimProstitutionModifier(1, Hero.MainHero, true, false, true);
                    }
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag))
                    {
                        VictimProstitutionModifier(0, Hero.MainHero, true, false, true);
                    }
                    // Renown
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown))
                    {
                        try
                        {
                            if (op.RenownTotal != null && op.RenownTotal != "")
                            {
                                RenownModifier(GetIntFromXML(op.RenownTotal), Hero.MainHero);
                            }
                            else if (listedEvent.RenownTotal != null && listedEvent.RenownTotal != "")
                            {
                                RenownModifier(GetIntFromXML(listedEvent.RenownTotal), Hero.MainHero);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing RenownTotal");
                                RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid RenownTotal");
                        }
                    }
                    // ChangeHealth
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth))
                    {
                        try
                        {
                            if (op.HealthTotal != null && op.HealthTotal != "")
                            {
                                Hero.MainHero.HitPoints += GetIntFromXML(op.HealthTotal);
                            }
                            else if (listedEvent.HealthTotal != null && listedEvent.HealthTotal != "")
                            {
                                Hero.MainHero.HitPoints += GetIntFromXML(listedEvent.HealthTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Invalid HealthTotal");
                                Hero.MainHero.HitPoints += MBRandom.RandomInt(-20, 20);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing HealthTotal");
                        }
                    }
                    // ChangeMorale
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale))
                    {
                        try
                        {
                            if (op.MoraleTotal != null && op.MoraleTotal != "")
                            {
                                MoralChange(GetIntFromXML(op.MoraleTotal), PlayerCaptivity.CaptorParty);
                            }
                            else if (listedEvent.MoraleTotal != null && listedEvent.MoraleTotal != "")
                            {
                                MoralChange(GetIntFromXML(listedEvent.MoraleTotal), PlayerCaptivity.CaptorParty);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing MoralTotal");
                                MoralChange(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid MoralTotal");
                        }
                    }

                    // Impregnation By Leader
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
                    {
                        try
                        {
                            if (op.PregnancyRiskModifier != null && op.PregnancyRiskModifier != "")
                            {
                                CaptivityImpregnationChance(Hero.MainHero, GetIntFromXML(op.PregnancyRiskModifier), false, true);
                            }
                            else if (listedEvent.PregnancyRiskModifier != null && listedEvent.PregnancyRiskModifier != "")
                            {
                                CaptivityImpregnationChance(Hero.MainHero, GetIntFromXML(listedEvent.PregnancyRiskModifier), false, true);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                                CaptivityImpregnationChance(Hero.MainHero, 30, false, true);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid PregnancyRiskModifier");
                        }
                    }
                    // Impregnation
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                    {
                        try
                        {
                            if (op.PregnancyRiskModifier != null && op.PregnancyRiskModifier != "")
                            {
                                CaptivityImpregnationChance(Hero.MainHero, GetIntFromXML(op.PregnancyRiskModifier), false, false);
                            }
                            else if (listedEvent.PregnancyRiskModifier != null && listedEvent.PregnancyRiskModifier != "")
                            {
                                CaptivityImpregnationChance(Hero.MainHero, GetIntFromXML(listedEvent.PregnancyRiskModifier), false, false);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                                CaptivityImpregnationChance(Hero.MainHero, 30, false, false);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid PregnancyRiskModifier");
                        }
                    }

                    // Specific Captor
                    if (!PlayerCaptivity.CaptorParty.IsSettlement && PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.LeaderHero != null)
                    {
                        // Captor Relations
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation))
                        {
                            try
                            {
                                if (op.RelationTotal != null && op.RelationTotal != "")
                                {
                                    RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, GetIntFromXML(op.RelationTotal));
                                }
                                else
                                {
                                    RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, GetIntFromXML(listedEvent.RelationTotal));
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing RelationTotal");
                                RelationsModifier(PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, MBRandom.RandomInt(-5, 5));
                            }
                        }
                        // Captor Gold
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold))
                        {
                            int content = AttractivenessScore(Hero.MainHero);
                            int currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
                            content += currentValue / 2;
                            content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveCaptorGold; });
                            GiveGoldAction.ApplyBetweenCharacters(null, PlayerCaptivity.CaptorParty.LeaderHero, content, false);
                        }
                        // Captor Change Gold
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                        {
                            try
                            {
                                int level = 0;
                                if (op.CaptorGoldTotal != null && op.CaptorGoldTotal != "")
                                {
                                    level = GetIntFromXML(op.CaptorGoldTotal);
                                }
                                else if (listedEvent.CaptorGoldTotal != null && listedEvent.CaptorGoldTotal != "")
                                {
                                    level = GetIntFromXML(listedEvent.CaptorGoldTotal);
                                }
                                else
                                {
                                    CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                                }

                                GiveGoldAction.ApplyBetweenCharacters(null, PlayerCaptivity.CaptorParty.MobileParty.LeaderHero, level, false);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Invalid CaptorGoldTotal");
                            }
                        }
                        // Captor Trait
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait))
                        {
                            try
                            {
                                int level;
                                if (op.TraitTotal != null && op.TraitTotal != "")
                                {
                                    level = GetIntFromXML(op.TraitTotal);
                                }
                                else
                                {
                                    level = GetIntFromXML(listedEvent.TraitTotal);
                                }

                                if (op.TraitToLevel != null && op.TraitToLevel != "")
                                {
                                    TraitModifier(PlayerCaptivity.CaptorParty.LeaderHero, op.TraitToLevel, level);
                                }
                                else
                                {
                                    TraitModifier(PlayerCaptivity.CaptorParty.LeaderHero, listedEvent.TraitToLevel, level);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing Trait Flags");
                            }
                        }
                        // Captor Renown
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown))
                        {
                            try
                            {
                                if (op.RenownTotal != null && op.RenownTotal != "")
                                {
                                    RenownModifier(GetIntFromXML(op.RenownTotal), PlayerCaptivity.CaptorParty.LeaderHero);
                                }
                                else
                                {
                                    RenownModifier(GetIntFromXML(listedEvent.RenownTotal), PlayerCaptivity.CaptorParty.LeaderHero);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing RenownTotal");
                                RenownModifier(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty.LeaderHero);
                            }
                        }
                    }

                    // Sold Events
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToCaravan))
                    {
                        try
                        {
                            MobileParty party = null;
                            if (PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                CECampaignBehavior.extraVariables.Owner = null;
                                party = PlayerCaptivity.CaptorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                            }
                            else
                            {
                                CECampaignBehavior.extraVariables.Owner = null;
                                party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; });
                            }

                            if (party != null)
                            {
                                CECaptivityChange(args, party.Party);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Caravan");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToSettlement))
                    {
                        try
                        {
                            PartyBase party;
                            if (!PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Party;
                            }
                            else
                            {
                                party = PlayerCaptivity.CaptorParty;
                            }
                            CECampaignBehavior.extraVariables.Owner = null;
                            CECaptivityChange(args, party);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Settlement");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToLordParty))
                    {
                        try
                        {
                            MobileParty party = null;
                            if (PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                party = PlayerCaptivity.CaptorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; });
                            }
                            else
                            {
                                party = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; });
                            }

                            if (party != null)
                            {
                                CECaptivityChange(args, party.Party);
                                CECampaignBehavior.extraVariables.Owner = party.LeaderHero;
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Lord");
                        }
                    }

                    // Work In Progress Sold Event
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.SoldToNotable))
                    {
                        try
                        {
                            Settlement settlement;
                            Hero notable;
                            if (PlayerCaptivity.CaptorParty.IsSettlement)
                            {
                                settlement = PlayerCaptivity.CaptorParty.Settlement;
                            }
                            else
                            {
                                settlement = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement;
                            }

                            notable = settlement.Notables.Where(findFirstNotable => { return !findFirstNotable.IsFemale; }).GetRandomElement();
                            CECampaignBehavior.extraVariables.Owner = notable;
                            CECaptivityChange(args, settlement.Party);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Failed to get Settlement");
                        }
                    }

                    // Gain Random Prisoners
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners))
                    {
                        CEGainRandomPrisoners(PlayerCaptivity.CaptorParty);
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor) && PlayerCaptivity.CaptorParty.NumberOfAllMembers > 1)
                    {
                        if (PlayerCaptivity.CaptorParty.LeaderHero != null)
                        {
                            KillCharacterAction.ApplyByMurder(PlayerCaptivity.CaptorParty.LeaderHero, Hero.MainHero, true);
                        }
                        else
                        {
                            PlayerCaptivity.CaptorParty.MemberRoster.AddToCounts(PlayerCaptivity.CaptorParty.Leader, -1, false, 0, 0, true, -1);
                        }
                    }

                    // Kill Captor
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor) && PlayerCaptivity.CaptorParty.NumberOfAllMembers == 1)
                    {
                        if (!CESettings.Instance.SexualContent)
                        {
                            GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_captivity_escape_success" : "CE_captivity_escape_success_male");
                        }
                        else
                        {
                            GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_captivity_sexual_escape_success" : "CE_captivity_sexual_escape_success_male");
                        }

                        if (PlayerCaptivity.CaptorParty.LeaderHero != null)
                        {
                            KillCharacterAction.ApplyByMurder(PlayerCaptivity.CaptorParty.LeaderHero, Hero.MainHero, true);
                        }
                        if (PlayerCaptivity.CaptorParty.IsMobile)
                        {
                            DestroyPartyAction.Apply(null, PlayerCaptivity.CaptorParty.MobileParty);
                        }
                    }
                    // Kill Hero
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner))
                    {
                        CEKillPlayer(PlayerCaptivity.CaptorParty.LeaderHero);
                    }
                    // Random Event Trigger
                    else if (op.TriggerEvents != null && op.TriggerEvents.Length > 0)
                    {
                        List<CEEvent> eventNames = new List<CEEvent>();
                        try
                        {
                            foreach (TriggerEvent triggerEvent in op.TriggerEvents)
                            {
                                CEEvent triggeredEvent = eventList.Find(item => item.Name == triggerEvent.EventName);
                                if (triggeredEvent == null)
                                {
                                    CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                                    continue;
                                }

                                if (!triggerEvent.EventUseConditions.IsStringNoneOrEmpty() && triggerEvent.EventUseConditions == "True")
                                {
                                    string conditionMatched = CEEventChecker.FlagsDoMatchEventConditions(triggeredEvent, CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                                    if (conditionMatched != null)
                                    {
                                        CECustomHandler.LogToFile(conditionMatched);
                                        continue;
                                    }
                                }

                                int weightedChance = 1;
                                try
                                {
                                    if (!triggerEvent.EventWeight.IsStringNoneOrEmpty())
                                    {
                                        weightedChance = GetIntFromXML(triggerEvent.EventWeight);
                                    }
                                    else
                                    {
                                        weightedChance = GetIntFromXML(triggeredEvent.WeightedChanceOfOccuring);
                                    }
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.LogToFile("Missing EventWeight");
                                }

                                for (int a = weightedChance; a > 0; a--)
                                {
                                    eventNames.Add(triggeredEvent);
                                }
                            }

                            if (eventNames.Count > 0)
                            {
                                int number = MBRandom.Random.Next(0, eventNames.Count - 1);
                                try
                                {
                                    CEEvent triggeredEvent = eventNames[number];
                                    triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                                    GameMenu.SwitchToMenu(triggeredEvent.Name);
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                                    CECaptivityContinue(args);
                                }
                            }
                            else
                            {
                                CECaptivityContinue(args);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                            CECaptivityContinue(args);
                        }
                    }
                    // Single Event Trigger
                    else if (op.TriggerEventName != null && op.TriggerEventName != "")
                    {
                        try
                        {
                            CEEvent triggeredEvent = eventList.Find(item => item.Name == op.TriggerEventName);
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + op.TriggerEventName + " in events.");
                            CECaptivityContinue(args);
                        }
                    }
                    // Escape Event Trigger
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape))
                    {
                        try
                        {
                            if (op.EscapeChance != null && op.EscapeChance != "")
                            {
                                CECaptivityEscapeAttempt(args, GetIntFromXML(op.EscapeChance));
                            }
                            else
                            {
                                CECaptivityEscapeAttempt(args, GetIntFromXML(listedEvent.EscapeChance));
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing EscapeChance");
                            CECaptivityEscapeAttempt(args);
                        }
                    }
                    // Escape Trigger
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape))
                    {
                        CECaptivityEscape(args);
                    }
                    // Leave Trigger
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave))
                    {
                        CECaptivityLeave(args);
                    }
                    else
                    {
                        CECaptivityContinue(args);
                    }
                }, false, GetIntFromXML(op.Order), false);
            }
        }

        public static void CELoadCaptorEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            gameStarter.AddGameMenu(listedEvent.Name, listedEvent.Text, (MenuCallbackArgs args) =>
            {
                Hero captiveHero = null;
                try
                {
                    if (listedEvent.Captive != null)
                    {
                        if (listedEvent.Captive.IsHero)
                        {
                            captiveHero = listedEvent.Captive.HeroObject;
                        }
                        MBTextManager.SetTextVariable("CAPTIVE_NAME", listedEvent.Captive.Name, false);
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Hero doesn't exist");
                }

                TextObject text = args.MenuContext.GameMenu.GetText();
                if (MobileParty.MainParty.CurrentSettlement != null)
                {
                    text.SetTextVariable("SETTLEMENT_NAME", MobileParty.MainParty.CurrentSettlement.Name);
                }
                text.SetTextVariable("PARTY_NAME", MobileParty.MainParty.Name);
                text.SetTextVariable("CAPTOR_NAME", Hero.MainHero.Name);

                if (Hero.MainHero.IsFemale)
                {
                    args.MenuContext.SetBackgroundMeshName("wait_prisoner_female");
                }
                else
                {
                    args.MenuContext.SetBackgroundMeshName("wait_prisoner_male");
                }

                try
                {
                    string BackgroundName = listedEvent.BackgroundName;
                    if (BackgroundName != null && BackgroundName != "")
                    {
                        CESubModule.LoadTexture(BackgroundName);
                    }
                    else
                    {
                        CESubModule.LoadTexture("captor_default");
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Background failed to load on " + listedEvent.Name);
                }
            }, GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.none, null);

            // Sort Options
            List<Option> sorted = listedEvent.Options.OrderBy(item => GetIntFromXML(item.Order)).ToList();

            foreach (Option op in sorted)
            {
                gameStarter.AddGameMenuOption(listedEvent.Name, listedEvent.Name + op.Order, op.OptionText,
                (MenuCallbackArgs args) =>
                {
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerIsNotBusy))
                    {
                        if (PlayerEncounter.Current != null)
                        {
                            args.Tooltip = GameTexts.FindText("str_CE_busy_right_now", null);
                            args.IsEnabled = false;
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold))
                    {
                        int content = AttractivenessScore(listedEvent.Captive.HeroObject);
                        if (listedEvent.Captive.HeroObject != null)
                        {
                            int currentValue = listedEvent.Captive.HeroObject.GetSkillValue(CESkills.Prostitution);
                            content += currentValue / 2;
                        }
                        content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveCaptorGold; });
                        MBTextManager.SetTextVariable("MONEY_AMOUNT", content, false);
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.CaptorGoldTotal != null && op.CaptorGoldTotal != "")
                            {
                                level = GetIntFromXML(op.CaptorGoldTotal);
                            }
                            else if (listedEvent.CaptorGoldTotal != null && listedEvent.CaptorGoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.CaptorGoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                            }
                            MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid CaptorGoldTotal");
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon))
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Default;
                    }

                    // ReqHeroCaptorRelation
                    if (listedEvent.Captive.HeroObject != null)
                    {
                        try
                        {
                            if (op.ReqHeroCaptorRelationAbove != null && op.ReqHeroCaptorRelationAbove != "")
                            {
                                if (listedEvent.Captive.HeroObject.GetRelationWithPlayer() < GetFloatFromXML(op.ReqHeroCaptorRelationAbove))
                                {
                                    TextObject textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
                                    textResponse4.SetTextVariable("HERO", listedEvent.Captive.HeroObject.Name.ToString());
                                    args.Tooltip = textResponse4;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationAbove / Failed ");
                        }
                        try
                        {
                            if (op.ReqHeroCaptorRelationBelow != null && op.ReqHeroCaptorRelationBelow != "")
                            {
                                if (listedEvent.Captive.HeroObject.GetRelationWithPlayer() > GetFloatFromXML(op.ReqHeroCaptorRelationBelow))
                                {
                                    TextObject textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
                                    textResponse3.SetTextVariable("HERO", listedEvent.Captive.HeroObject.Name.ToString());
                                    args.Tooltip = textResponse3;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationBelow / Failed ");
                        }
                    }
                    // ReqMorale
                    try
                    {
                        if (!op.ReqMoraleAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.IsMobile && PartyBase.MainParty.MobileParty.Morale < CEEventLoader.GetIntFromXML(op.ReqMoraleAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMoraleBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.IsMobile && PartyBase.MainParty.MobileParty.Morale > CEEventLoader.GetIntFromXML(op.ReqMoraleBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed ");
                    }
                    // ReqTroops
                    try
                    {
                        if (!op.ReqTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfHealthyMembers < GetIntFromXML(op.ReqTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfHealthyMembers > GetIntFromXML(op.ReqTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed ");
                    }

                    // ReqMaleTroops
                    try
                    {
                        if (!op.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMaleTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed ");
                    }

                    // ReqFemaleTroops
                    try
                    {
                        if (!op.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqFemaleTroopsAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > GetIntFromXML(op.ReqFemaleTroopsBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed ");
                    }

                    // ReqCaptives
                    try
                    {
                        if (!op.ReqCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfPrisoners < GetIntFromXML(op.ReqCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.NumberOfPrisoners > GetIntFromXML(op.ReqCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed ");
                    }

                    // ReqMaleCaptives
                    try
                    {
                        if (!op.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqMaleCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqMaleCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed ");
                    }

                    // ReqFemaleCaptives
                    try
                    {
                        if (!op.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < GetIntFromXML(op.ReqFemaleCaptivesAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
                    }
                    try
                    {
                        if (!op.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty())
                        {
                            if (PartyBase.MainParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > GetIntFromXML(op.ReqFemaleCaptivesBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed ");
                    }

                    // ReqTrait
                    if (!op.ReqHeroTrait.IsStringNoneOrEmpty())
                    {
                        int TraitLevel = 0;
                        try
                        {
                            TraitLevel = listedEvent.Captive.GetTraitLevel(TraitObject.Find(op.ReqCaptorTrait));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Captive");
                            TraitLevel = 0;
                        }

                        try
                        {
                            if (op.ReqHeroTraitLevelAbove != null && op.ReqHeroTraitLevelAbove != "")
                            {
                                if (TraitLevel < CEEventLoader.GetIntFromXML(op.ReqHeroTraitLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_captive_level", "low");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqHeroTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove");
                        }
                        try
                        {
                            if (op.ReqHeroTraitLevelBelow != null && op.ReqHeroTraitLevelBelow != "")
                            {
                                if (TraitLevel > CEEventLoader.GetIntFromXML(op.ReqHeroTraitLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_captive_level", "high");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqHeroTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow");
                        }
                    }

                    // ReqCaptorTrait
                    if (!op.ReqCaptorTrait.IsStringNoneOrEmpty())
                    {
                        int TraitLevel = 0;
                        try
                        {
                            TraitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(op.ReqCaptorTrait));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Trait Captor");
                            TraitLevel = 0;
                        }

                        try
                        {
                            if (op.ReqCaptorTraitLevelAbove != null && op.ReqCaptorTraitLevelAbove != "")
                            {
                                if (TraitLevel < CEEventLoader.GetIntFromXML(op.ReqCaptorTraitLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_level", "low");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqCaptorTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove");
                        }
                        try
                        {
                            if (op.ReqCaptorTraitLevelBelow != null && op.ReqCaptorTraitLevelBelow != "")
                            {
                                if (TraitLevel > CEEventLoader.GetIntFromXML(op.ReqCaptorTraitLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_trait_level", "high");
                                    text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(op.ReqCaptorTrait));
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow");
                        }
                    }

                    // ReqSkill
                    if (!op.ReqHeroSkill.IsStringNoneOrEmpty())
                    {
                        int SkillLevel = 0;
                        try
                        {
                            SkillLevel = listedEvent.Captive.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == op.ReqHeroSkill; }));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Captive");
                            SkillLevel = 0;
                        }

                        try
                        {
                            if (!op.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel < CEEventLoader.GetIntFromXML(op.ReqHeroSkillLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_captive_level", "low");
                                    text.SetTextVariable("SKILL", op.ReqHeroSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove");
                        }
                        try
                        {
                            if (!op.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel > CEEventLoader.GetIntFromXML(op.ReqHeroSkillLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_captive_level", "high");
                                    text.SetTextVariable("SKILL", op.ReqHeroSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow");
                        }
                    }

                    // ReqCaptorSkill
                    if (!op.ReqCaptorSkill.IsStringNoneOrEmpty())
                    {
                        int SkillLevel = 0;
                        try
                        {
                            SkillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == op.ReqCaptorSkill; }));
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Captor");
                            SkillLevel = 0;
                        }

                        try
                        {
                            if (!op.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel < CEEventLoader.GetIntFromXML(op.ReqCaptorSkillLevelAbove))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
                                    text.SetTextVariable("SKILL", op.ReqCaptorSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove");
                        }
                        try
                        {
                            if (op.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel > CEEventLoader.GetIntFromXML(op.ReqCaptorSkillLevelBelow))
                                {
                                    TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
                                    text.SetTextVariable("SKILL", op.ReqCaptorSkill);
                                    args.Tooltip = text;
                                    args.IsEnabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow");
                        }
                    }

                    // ReqGold
                    try
                    {
                        if (op.ReqGoldAbove != null && op.ReqGoldAbove != "")
                        {
                            if (Hero.MainHero.Gold < GetIntFromXML(op.ReqGoldAbove))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed ");
                    }
                    try
                    {
                        if (op.ReqGoldBelow != null && op.ReqGoldBelow != "")
                        {
                            if (Hero.MainHero.Gold > GetIntFromXML(op.ReqGoldBelow))
                            {
                                args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
                                args.IsEnabled = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed ");
                    }

                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    Hero captiveHero = null;
                    try
                    {
                        if (listedEvent.Captive != null)
                        {
                            if (listedEvent.Captive.IsHero)
                            {
                                captiveHero = listedEvent.Captive.HeroObject;
                            }
                            MBTextManager.SetTextVariable("CAPTIVE_NAME", listedEvent.Captive.Name, false);
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Hero doesn't exist");
                    }

                    // Captor Gold
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold))
                    {
                        int content = AttractivenessScore(captiveHero);
                        int currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
                        content += currentValue / 2;
                        content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveCaptorGold; });
                        GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content, false);
                    }

                    // Captor Change Gold
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold))
                    {
                        try
                        {
                            int level = 0;
                            if (op.CaptorGoldTotal != null && op.CaptorGoldTotal != "")
                            {
                                level = GetIntFromXML(op.CaptorGoldTotal);
                            }
                            else if (listedEvent.CaptorGoldTotal != null && listedEvent.CaptorGoldTotal != "")
                            {
                                level = GetIntFromXML(listedEvent.CaptorGoldTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                            }

                            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level, false);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid CaptorGoldTotal");
                        }
                    }

                    // Captor Skill
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorSkill))
                    {
                        try
                        {
                            int level = 0;
                            if (!op.SkillTotal.IsStringNoneOrEmpty())
                            {
                                level = GetIntFromXML(op.SkillTotal);
                            }
                            else if (!listedEvent.SkillTotal.IsStringNoneOrEmpty())
                            {
                                level = GetIntFromXML(listedEvent.SkillTotal);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing Skill SkillTotal");
                            }

                            if (!op.SkillToLevel.IsStringNoneOrEmpty())
                            {
                                SkillModifier(Hero.MainHero, op.SkillToLevel, level);
                            }
                            else if (!listedEvent.SkillToLevel.IsStringNoneOrEmpty())
                            {
                                SkillModifier(Hero.MainHero, listedEvent.SkillToLevel, level);
                            }
                            else
                            {
                                CECustomHandler.LogToFile("Missing SkillToLevel");
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Invalid Skill Flags");
                        }
                    }
                    // Captor Trait
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait))
                    {
                        try
                        {
                            int level;
                            if (op.TraitTotal != null && op.TraitTotal != "")
                            {
                                level = GetIntFromXML(op.TraitTotal);
                            }
                            else
                            {
                                level = GetIntFromXML(listedEvent.TraitTotal);
                            }

                            if (op.TraitToLevel != null && op.TraitToLevel != "")
                            {
                                TraitModifier(Hero.MainHero, op.TraitToLevel, level);
                            }
                            else
                            {
                                TraitModifier(Hero.MainHero, listedEvent.TraitToLevel, level);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing Trait Flags");
                        }
                    }
                    // Captor Renown
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown))
                    {
                        try
                        {
                            if (op.RenownTotal != null && op.RenownTotal != "")
                            {
                                RenownModifier(GetIntFromXML(op.RenownTotal), Hero.MainHero);
                            }
                            else
                            {
                                RenownModifier(GetIntFromXML(listedEvent.RenownTotal), Hero.MainHero);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing RenownTotal");
                            RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                        }
                    }

                    // ChangeMorale
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale))
                    {
                        try
                        {
                            if (op.MoraleTotal != null && op.MoraleTotal != "")
                            {
                                MoralChange(GetIntFromXML(op.MoraleTotal), PartyBase.MainParty);
                            }
                            else
                            {
                                MoralChange(GetIntFromXML(listedEvent.MoraleTotal), PartyBase.MainParty);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing MoralTotal");
                            MoralChange(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty);
                        }
                    }

                    if (captiveHero != null)
                    {
                        // Leave Spouse
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse))
                        {
                            ChangeSpouse(captiveHero, null);
                        }
                        // Force Marry
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor))
                        {
                            ChangeSpouse(captiveHero, Hero.MainHero);
                        }
                        // Change Clan
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeClan))
                        {
                            ChangeClan(captiveHero, Hero.MainHero);
                        }

                        // Slavery Flags
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag))
                        {
                            VictimSlaveryModifier(1, captiveHero, true, false, true);
                        }
                        else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag))
                        {
                            VictimSlaveryModifier(0, captiveHero, true, false, true);
                        }
                        // Slavery Level
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel))
                        {
                            try
                            {
                                if (op.SlaveryTotal != null && op.SlaveryTotal != "")
                                {
                                    VictimSlaveryModifier(GetIntFromXML(op.SlaveryTotal), captiveHero);
                                }
                                else
                                {
                                    VictimSlaveryModifier(GetIntFromXML(listedEvent.SlaveryTotal), captiveHero);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing SlaveryTotal");
                                VictimSlaveryModifier(1, captiveHero);
                            }
                        }
                        // Prostitution Flags
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag))
                        {
                            VictimProstitutionModifier(1, captiveHero, true, false, true);
                        }
                        else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag))
                        {
                            VictimProstitutionModifier(0, captiveHero, true, false, true);
                        }
                        // Prostitution Level
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel))
                        {
                            try
                            {
                                if (op.ProstitutionTotal != null && op.ProstitutionTotal != "")
                                {
                                    VictimProstitutionModifier(GetIntFromXML(op.ProstitutionTotal), captiveHero);
                                }
                                else
                                {
                                    VictimProstitutionModifier(GetIntFromXML(listedEvent.ProstitutionTotal), captiveHero);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing ProstitutionTotal");
                                VictimProstitutionModifier(1, captiveHero);
                            }
                        }

                        // Relations
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation))
                        {
                            try
                            {
                                if (op.RelationTotal != null && op.RelationTotal != "")
                                {
                                    RelationsModifier(captiveHero, GetIntFromXML(op.RelationTotal));
                                }
                                else
                                {
                                    RelationsModifier(captiveHero, GetIntFromXML(listedEvent.RelationTotal));
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing RelationTotal");
                                RelationsModifier(captiveHero, MBRandom.RandomInt(-5, 5));
                            }
                        }
                        // Gold
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold))
                        {
                            int content = AttractivenessScore(captiveHero);
                            int currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
                            content += currentValue / 2;
                            content *= op.MultipleRestrictedListOfConsequences.Count((RestrictedListOfConsequences consquence) => { return consquence == RestrictedListOfConsequences.GiveGold; });
                            GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, content, false);
                        }

                        // Change Gold
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                        {
                            try
                            {
                                int level = 0;
                                if (op.GoldTotal != null && op.GoldTotal != "")
                                {
                                    level = GetIntFromXML(op.GoldTotal);
                                }
                                else if (listedEvent.GoldTotal != null && listedEvent.GoldTotal != "")
                                {
                                    level = GetIntFromXML(listedEvent.GoldTotal);
                                }
                                else
                                {
                                    CECustomHandler.LogToFile("Missing GoldTotal");
                                }

                                GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, level, false);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Invalid GoldTotal");
                            }
                        }

                        // Trait
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait))
                        {
                            try
                            {
                                int level;
                                if (op.TraitTotal != null && op.TraitTotal != "")
                                {
                                    level = GetIntFromXML(op.TraitTotal);
                                }
                                else
                                {
                                    level = GetIntFromXML(listedEvent.TraitTotal);
                                }

                                if (op.TraitToLevel != null && op.TraitToLevel != "")
                                {
                                    TraitModifier(captiveHero, op.TraitToLevel, level);
                                }
                                else
                                {
                                    TraitModifier(captiveHero, listedEvent.TraitToLevel, level);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing Trait Flags");
                            }
                        }

                        // Skill
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill))
                        {
                            try
                            {
                                int level = 0;
                                if (!op.SkillTotal.IsStringNoneOrEmpty())
                                {
                                    level = GetIntFromXML(op.SkillTotal);
                                }
                                else if (!listedEvent.SkillTotal.IsStringNoneOrEmpty())
                                {
                                    level = GetIntFromXML(listedEvent.SkillTotal);
                                }
                                else
                                {
                                    CECustomHandler.LogToFile("Missing Skill SkillTotal");
                                }

                                if (!op.SkillToLevel.IsStringNoneOrEmpty())
                                {
                                    SkillModifier(captiveHero, op.SkillToLevel, level);
                                }
                                else if (!listedEvent.SkillToLevel.IsStringNoneOrEmpty())
                                {
                                    SkillModifier(captiveHero, listedEvent.SkillToLevel, level);
                                }
                                else
                                {
                                    CECustomHandler.LogToFile("Missing SkillToLevel");
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Invalid Skill Flags");
                            }
                        }

                        // Renown
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown))
                        {
                            try
                            {
                                if (op.RenownTotal != null && op.RenownTotal != "")
                                {
                                    RenownModifier(GetIntFromXML(op.RenownTotal), captiveHero);
                                }
                                else
                                {
                                    RenownModifier(GetIntFromXML(listedEvent.RenownTotal), captiveHero);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing RenownTotal");
                                RenownModifier(MBRandom.RandomInt(-5, 5), captiveHero);
                            }
                        }
                        // ChangeHealth
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth))
                        {
                            try
                            {
                                if (op.HealthTotal != null && op.HealthTotal != "")
                                {
                                    captiveHero.HitPoints += GetIntFromXML(op.HealthTotal);
                                }
                                else
                                {
                                    captiveHero.HitPoints += GetIntFromXML(listedEvent.HealthTotal);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing HealthTotal");
                                captiveHero.HitPoints += MBRandom.RandomInt(-20, 20);
                            }
                        }

                        // Impregnation
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
                        {
                            try
                            {
                                if (op.PregnancyRiskModifier != null && op.PregnancyRiskModifier != "")
                                {
                                    CaptivityImpregnationChance(captiveHero, GetIntFromXML(op.PregnancyRiskModifier), false, true);
                                }
                                else
                                {
                                    CaptivityImpregnationChance(captiveHero, GetIntFromXML(listedEvent.PregnancyRiskModifier), false, true);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                                CaptivityImpregnationChance(captiveHero, 30, false, true);
                            }
                        }
                        else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                        {
                            try
                            {
                                if (op.PregnancyRiskModifier != null && op.PregnancyRiskModifier != "")
                                {
                                    CaptivityImpregnationChance(captiveHero, GetIntFromXML(op.PregnancyRiskModifier), false, false);
                                }
                                else
                                {
                                    CaptivityImpregnationChance(captiveHero, GetIntFromXML(listedEvent.PregnancyRiskModifier), false, false);
                                }
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                                CaptivityImpregnationChance(captiveHero, 30, false, false);
                            }
                        }

                        // Strip
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripHero))
                        {
                            InventoryManager.OpenScreenAsInventoryOf(Hero.MainHero.PartyBelongedTo.Party.MobileParty, captiveHero.CharacterObject);
                        }
                        if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Strip))
                        {
                            CEStripVictim(captiveHero);
                        }
                    }

                    // Escape
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape))
                    {
                        if (listedEvent.Captive.IsHero)
                        {
                            EndCaptivityAction.ApplyByEscape(listedEvent.Captive.HeroObject, null);
                        }
                        else
                        {
                            PartyBase.MainParty.PrisonRoster.AddToCounts(listedEvent.Captive, -1, false, 0, 0, true, -1);
                        }
                    }

                    // Gain Random Prisoners
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners))
                    {
                        CEGainRandomPrisoners(PartyBase.MainParty);
                    }

                    // Kill Prisoner
                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner))
                    {
                        if (listedEvent.Captive.IsHero)
                        {
                            KillCharacterAction.ApplyByExecution(listedEvent.Captive.HeroObject, Hero.MainHero, true);
                        }
                        else
                        {
                            PartyBase.MainParty.PrisonRoster.AddToCounts(listedEvent.Captive, -1, false, 0, 0, true, -1);
                        }
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor))
                    {
                        CEKillPlayer(listedEvent.Captive.HeroObject);
                    }
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillAllPrisoners))
                    {
                        CEKillPrisoners(args, PartyBase.MainParty.PrisonRoster.Count(), true);
                    }
                    // Kill Random
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillRandomPrisoners))
                    {
                        CEKillPrisoners(args);
                    }

                    if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RebelPrisoners))
                    {
                        CEPrisonerRebel(args);
                    }
                    else if (op.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.HuntPrisoners))
                    {
                        CEHuntPrisoners(args);
                    }
                    else if (op.TriggerEvents != null && op.TriggerEvents.Length > 0)
                    {
                        List<CEEvent> eventNames = new List<CEEvent>();
                        try
                        {
                            foreach (TriggerEvent triggerEvent in op.TriggerEvents)
                            {
                                CEEvent triggeredEvent = eventList.Find(item => item.Name == triggerEvent.EventName);
                                if (triggeredEvent == null)
                                {
                                    CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                                    continue;
                                }

                                if (!triggerEvent.EventUseConditions.IsStringNoneOrEmpty() && triggerEvent.EventUseConditions == "True")
                                {
                                    string conditionMatched = CEEventChecker.FlagsDoMatchEventConditions(triggeredEvent, listedEvent.Captive, PartyBase.MainParty);
                                    if (conditionMatched != null)
                                    {
                                        CECustomHandler.LogToFile(conditionMatched);
                                        continue;
                                    }
                                }

                                int weightedChance = 1;
                                try
                                {
                                    if (!triggerEvent.EventWeight.IsStringNoneOrEmpty())
                                    {
                                        weightedChance = GetIntFromXML(triggerEvent.EventWeight);
                                    }
                                    else
                                    {
                                        weightedChance = GetIntFromXML(triggeredEvent.WeightedChanceOfOccuring);
                                    }
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.LogToFile("Missing EventWeight");
                                }

                                for (int a = weightedChance; a > 0; a--)
                                {
                                    eventNames.Add(triggeredEvent);
                                }
                            }

                            if (eventNames.Count > 0)
                            {
                                int number = MBRandom.Random.Next(0, eventNames.Count - 1);
                                try
                                {
                                    CEEvent triggeredEvent = eventNames[number];
                                    triggeredEvent.Captive = listedEvent.Captive;
                                    GameMenu.SwitchToMenu(triggeredEvent.Name);
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                                    CECaptorContinue(args);
                                }
                            }
                            else
                            {
                                CECaptorContinue(args);
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                            CECaptorContinue(args);
                        }
                    }
                    else if (op.TriggerEventName != "" && op.TriggerEventName != null)
                    {
                        try
                        {
                            CEEvent triggeredEvent = eventList.Find(item => item.Name == op.TriggerEventName);
                            triggeredEvent.Captive = listedEvent.Captive;
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + op.TriggerEventName + " in events.");
                            CECaptorContinue(args);
                        }
                    }
                    else
                    {
                        CECaptorContinue(args);
                    }
                }, false, GetIntFromXML(op.Order), false);
            }
        }

        // Captive Specific Functions
        private static void CECaptivityContinue(MenuCallbackArgs args)
        {
            CESubModule.animationPlayEvent = false;
            try
            {
                if (PlayerCaptivity.CaptorParty != null)
                {
                    string waitingList = CEEventLoader.CEWaitingList();
                    if (waitingList != null)
                    {
                        GameMenu.SwitchToMenu(waitingList);
                    }
                    else
                    {
                        CESubModule.LoadTexture("default");
                        GameMenu.SwitchToMenu(PlayerCaptivity.CaptorParty.IsSettlement ? "settlement_wait" : "prisoner_wait");
                    }
                }
                else
                {
                    CESubModule.LoadTexture("default");
                    GameMenu.ExitToLast();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Critical Error: CECaptivityContinue : " + e.ToString());
            }
        }

        private static void CECaptivityEscapeAttempt(MenuCallbackArgs args, int escapeChance = 10)
        {
            if (MBRandom.Random.Next(100) < escapeChance + EscapeProwessScore(Hero.MainHero))
            {
                if (!CESettings.Instance.SexualContent)
                {
                    GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_captivity_escape_failure" : "CE_captivity_escape_failure_male");
                }
                else
                {
                    GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_captivity_sexual_escape_failure" : "CE_captivity_sexual_escape_failure_male");
                }
                return;
            }

            if (!CESettings.Instance.SexualContent)
            {
                GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_captivity_escape_success" : "CE_captivity_escape_success_male");
            }
            else
            {
                GameMenu.SwitchToMenu(Hero.MainHero.IsFemale ? "CE_captivity_sexual_escape_success" : "CE_captivity_sexual_escape_success_male");
            }
        }

        private static void CECaptivityLeave(MenuCallbackArgs args)
        {
            CESubModule.LoadTexture("default");
            PartyBase _captorParty = PlayerCaptivity.CaptorParty;
            CECampaignBehavior.extraVariables.Owner = null;

            if (_captorParty.IsSettlement && _captorParty.Settlement.IsTown)
            {
                if (Hero.MainHero.IsAlive)
                {
                    if (Hero.MainHero.IsWounded)
                    {
                        Hero.MainHero.HitPoints = 20;
                    }
                    if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.IsMobile)
                    {
                        PlayerCaptivity.CaptorParty.MobileParty.SetDoNotAttackMainParty(12);
                    }
                    PlayerEncounter.ProtectPlayerSide(1f);
                    MobileParty.MainParty.IsDisorganized = false;
                    PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, 1, true);
                }

                if (Campaign.Current.CurrentMenuContext != null)
                {
                    GameMenu.SwitchToMenu("town");
                }
                if (Hero.MainHero.IsAlive)
                {
                    Hero.MainHero.ChangeState(Hero.CharacterStates.Active);
                    Hero.MainHero.DaysLeftToRespawn = 0;
                }
                if (_captorParty.IsActive)
                {
                    _captorParty.PrisonRoster.RemoveTroop(Hero.MainHero.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
                }
                if (Hero.MainHero.IsAlive)
                {
                    MobileParty.MainParty.IsActive = true;
                    PartyBase.MainParty.SetAsCameraFollowParty();
                    MobileParty.MainParty.SetMoveModeHold();
                    SkillLevelingManager.OnMainHeroReleasedFromCaptivity(PlayerCaptivity.CaptivityStartTime.ElapsedHoursUntilNow);
                    PartyBase.MainParty.UpdateVisibilityAndInspected(true);
                }
                PlayerCaptivity.CaptorParty = null;
            }
            else
            {
                PlayerCaptivity.EndCaptivity();
            }
        }

        private static void CECaptivityEscape(MenuCallbackArgs args)
        {
            CECampaignBehavior.extraVariables.Owner = null;
            bool wasInSettlement = PlayerCaptivity.CaptorParty.IsSettlement;
            Settlement currentSettlement = PlayerCaptivity.CaptorParty.Settlement;
            TextObject textObject = GameTexts.FindText("str_CE_escape_success", (wasInSettlement) ? "settlement" : null);
            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);
            if (wasInSettlement)
            {
                string settlementname = (currentSettlement != null) ? currentSettlement.Name.ToString() : "ERROR";
                textObject.SetTextVariable("SETTLEMENT", settlementname);
            }

            CESubModule.LoadTexture("default");
            PlayerCaptivity.EndCaptivity();
        }

        private static void CECaptivityChange(MenuCallbackArgs args, PartyBase party)
        {
            try
            {
                PlayerCaptivity.CaptorParty = party;
                PlayerCaptivity.StartCaptivity(party);
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Failed to exception: " + e.Message.ToString() + " stacktrace: " + e.StackTrace.ToString());
            }
        }

        // Captor Specific Functions
        private static void CECaptorContinue(MenuCallbackArgs args)
        {
            if (CECampaignBehavior.extraVariables.menuToSwitchBackTo != null)
            {
                GameMenu.SwitchToMenu(CECampaignBehavior.extraVariables.menuToSwitchBackTo);
                CECampaignBehavior.extraVariables.menuToSwitchBackTo = null;
                if (CECampaignBehavior.extraVariables.currentBackgroundMeshNameToSwitchBackTo != null)
                {
                    args.MenuContext.SetBackgroundMeshName(CECampaignBehavior.extraVariables.currentBackgroundMeshNameToSwitchBackTo);
                    CECampaignBehavior.extraVariables.currentBackgroundMeshNameToSwitchBackTo = null;
                }
            }
            else
            {
                GameMenu.ExitToLast();
            }
            Campaign.Current.TimeControlMode = Campaign.Current.LastTimeControlMode;
            CESubModule.LoadTexture("default");
        }

        private static void CEKillPrisoners(MenuCallbackArgs args, int amount = 10, bool killHeros = false)
        {
            try
            {
                int prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount)
                {
                    amount = prisonerCount;
                }
                MobileParty.MainParty.PrisonRoster.KillNumberOfMenRandomly(amount, killHeros);
                TextObject textObject = GameTexts.FindText("str_CE_kill_prisoners", null);
                textObject.SetTextVariable("HERO", Hero.MainHero.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't kill any prisoners.");
            }
        }

        private static void CEPrisonerRebel(MenuCallbackArgs args)
        {
            TroopRoster releasedPrisoners = new TroopRoster();

            try
            {
                releasedPrisoners.Add(MobileParty.MainParty.PrisonRoster.ToFlattenedRoster());
                MobileParty.MainParty.PrisonRoster.Clear();
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't find anymore prisoners.");
            }

            if (!releasedPrisoners.IsEmpty())
            {
                try
                {
                    MobileParty prisonerParty = MBObjectManager.Instance.CreateObject<MobileParty>("Escaped_Captives");

                    TroopRosterElement leader = releasedPrisoners.FirstOrDefault(hasHero => { return hasHero.Character.IsHero; });
                    if (leader.Character != null)
                    {
                        Clan clan = leader.Character.HeroObject.Clan;
                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;
                        Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return settlement.OwnerClan == clan; });
                        if (nearest == null)
                        {
                            nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });
                        }
                        prisonerParty.InitializeMobileParty(new TextObject("{=CEEVENTS1107}Escaped Captives", null), defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, MobileParty.PartyTypeEnum.Lord, -1);
                        prisonerParty.MemberRoster.Clear();
                        prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());
                        prisonerParty.IsActive = true;
                        prisonerParty.Party.Owner = leader.Character.HeroObject;
                        prisonerParty.ChangePartyLeader(leader.Character, true);
                        prisonerParty.HomeSettlement = nearest;
                        prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position2D);
                    }
                    else
                    {
                        Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;
                        Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });
                        prisonerParty.InitializeMobileParty(new TextObject("{=CEEVENTS1107}Escaped Captives", null), defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, MobileParty.PartyTypeEnum.Bandit, -1);
                        prisonerParty.MemberRoster.Clear();
                        prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());
                        prisonerParty.IsActive = true;
                        prisonerParty.Party.Owner = clan.Leader;
                        prisonerParty.HomeSettlement = nearest;
                        prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position2D);
                    }

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.Aggressiveness = 0.2f;
                    prisonerParty.InitializePartyTrade(0);
                    prisonerParty.EnableAi();

                    prisonerParty.Party.Visuals.SetMapIconAsDirty();

                    Hero.MainHero.HitPoints += 40;
                    Campaign.Current.Parties.AddItem(prisonerParty.Party);

                    CECustomHandler.LogToFile(prisonerParty.Leader.Name.ToString());
                    PlayerEncounter.RestartPlayerEncounter(MobileParty.MainParty.Party, prisonerParty.Party, true);
                    GameMenu.SwitchToMenu("encounter");
                }
                catch (Exception)
                {
                    CECaptorContinue(args);
                }
            }
            else
            {
                CECaptorContinue(args);
            }
        }

        private static void CEHuntPrisoners(MenuCallbackArgs args, int amount = 20)
        {
            TroopRoster releasedPrisoners = new TroopRoster();
            TroopRosterElement test;

            amount = CESettings.Instance.AmountOfTroopsForHunt;

            try
            {
                for (int i = 0; i < amount; i++)
                {
                    test = MobileParty.MainParty.PrisonRoster.Where((troop) => { return !troop.Character.IsHero; }).GetRandomElement();
                    if (test.Character != null)
                    {
                        MobileParty.MainParty.PrisonRoster.RemoveTroop(test.Character);
                        releasedPrisoners.AddToCounts(test.Character, 1, true, 0);
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't find anymore prisoners.");
            }

            if (!releasedPrisoners.IsEmpty())
            {
                CECaptorContinue(args);

                try
                {
                    MobileParty prisonerParty = MBObjectManager.Instance.CreateObject<MobileParty>("Escaped_Captives");

                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters"); ;
                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;
                    Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

                    prisonerParty.InitializeMobileParty(new TextObject("{=CEEVENTS1107}Escaped Captives", null), defaultPartyTemplate, MobileParty.MainParty.Position2D, 0f, 0f, MobileParty.PartyTypeEnum.Bandit, -1);
                    prisonerParty.MemberRoster.Clear();
                    prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.IsActive = true;
                    prisonerParty.Party.Owner = clan.Leader;
                    prisonerParty.Aggressiveness = 0.2f;
                    prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position2D);
                    prisonerParty.HomeSettlement = nearest;
                    prisonerParty.InitializePartyTrade(0);
                    prisonerParty.EnableAi();
                    prisonerParty.Party.Visuals.SetMapIconAsDirty();

                    Hero.MainHero.HitPoints += 40;
                    Campaign.Current.Parties.AddItem(prisonerParty.Party);

                    CECustomHandler.LogToFile(prisonerParty.Leader.Name.ToString());
                    StartBattleAction.Apply(MobileParty.MainParty.Party, prisonerParty.Party);
                    PlayerEncounter.RestartPlayerEncounter(prisonerParty.Party, MobileParty.MainParty.Party, true);
                    PlayerEncounter.Update();

                    CESubModule.huntState = CESubModule.HuntState.StartHunt;
                    CampaignMission.OpenBattleMission(PlayerEncounter.GetBattleSceneForMapPosition(MobileParty.MainParty.Position2D));
                }
                catch (Exception)
                {
                    CEKillPrisoners(args, amount);
                }
            }
            else
            {
                CECaptorContinue(args);
            }
        }

        public static void CEStripVictim(Hero captive)
        {
            if (captive != null)
            {
                Equipment randomElement = new Equipment(false);
                ItemObject itemObjectBody = captive.IsFemale ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress") : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                Equipment randomElement2 = new Equipment(true);
                randomElement2.FillFrom(randomElement, false);

                if (CESettings.Instance.EventCaptorGearCaptives)
                {
                    CECampaignBehavior.AddReturnEquipment(captive, captive.BattleEquipment, captive.CivilianEquipment);
                }

                foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                {
                    try
                    {
                        if (!captive.BattleEquipment.GetEquipmentFromSlot(i).IsEmpty)
                        {
                            PartyBase.MainParty.ItemRoster.AddToCounts(captive.BattleEquipment.GetEquipmentFromSlot(i).Item, 1, true);
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        if (!captive.CivilianEquipment.GetEquipmentFromSlot(i).IsEmpty)
                        {
                            PartyBase.MainParty.ItemRoster.AddToCounts(captive.CivilianEquipment.GetEquipmentFromSlot(i).Item, 1, true);
                        }
                    }
                    catch (Exception) { }
                }

                EquipmentHelper.AssignHeroEquipmentFromEquipment(captive, randomElement);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(captive, randomElement2);
            }
        }

        // Variable Loaders
        public static int GetIntFromXML(string numpassed)
        {
            try
            {
                int number = 0;
                if (numpassed != null)
                {
                    if (numpassed.StartsWith("R"))
                    {
                        string[] splitPass = numpassed.Split(' ');

                        switch (splitPass.Length)
                        {
                            case 3:
                                int numberOne = int.Parse(splitPass[1]);
                                int numberTwo = int.Parse(splitPass[2]);

                                number = (numberOne < numberTwo) ? MBRandom.RandomInt(numberOne, numberTwo) : MBRandom.RandomInt(numberTwo, numberOne);
                                break;

                            case 2:
                                number = MBRandom.RandomInt(int.Parse(splitPass[1]));
                                break;

                            default:
                                number = MBRandom.RandomInt();
                                break;
                        }
                    }
                    else
                    {
                        number = int.Parse(numpassed);
                    }
                }

                return number;
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to parse " + numpassed);
                return 0;
            }
        }

        public static float GetFloatFromXML(string numpassed)
        {
            try
            {
                float number = 0f;
                if (numpassed != null)
                {
                    if (numpassed.StartsWith("R"))
                    {
                        string[] splitPass = numpassed.Split(' ');
                        switch (splitPass.Length)
                        {
                            case 3:
                                float numberOne = float.Parse(splitPass[1]);
                                float numberTwo = float.Parse(splitPass[2]);

                                number = (numberOne < numberTwo) ? MBRandom.RandomFloatRanged(numberOne, numberTwo) : MBRandom.RandomFloatRanged(numberTwo, numberOne);
                                break;

                            case 2:
                                number = MBRandom.RandomFloatRanged(float.Parse(splitPass[1]));
                                break;

                            default:
                                number = MBRandom.RandomFloat;
                                break;
                        }
                    }
                    else
                    {
                        number = float.Parse(numpassed);
                    }
                }
                return number;
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed to parse " + numpassed);
                return 0f;
            }
        }

        // Dynamic Functions
        private static void CEKillPlayer(Hero killer)
        {
            GameMenu.ExitToLast();
            try
            {
                if (killer != null)
                {
                    KillCharacterAction.ApplyByMurder(Hero.MainHero, killer, true);
                }
                else
                {
                    KillCharacterAction.ApplyByMurder(Hero.MainHero);
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed CEKillPlayer " + e.ToString());
            }
        }

        private static void CEGainRandomPrisoners(PartyBase party)
        {
            Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return settlement.IsVillage; });
            PartyTemplateObject villagerPartyTemplate = nearest.Culture.VillagerPartyTemplate;
            MBRandom.RandomInt(1, 10);
            party.AddPrisoner(nearest.Culture.VillageWoman, 10, 7);
            party.AddPrisoner(nearest.Culture.Villager, 10, 7);
        }

        private static void GainSkills(SkillObject skillObject, int amount, int chance, Hero hero = null)
        {
            if (MBRandom.Random.Next(30) < chance)
            {
                if (hero == null)
                {
                    hero = Hero.MainHero;
                }

                try
                {
                    hero.HeroDeveloper.AddSkillXp(skillObject, amount, true, true);
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Failed to add to skill");
                }

                //TextObject textObject = new TextObject("{HERO} has learned {SKILL_AMOUNT} {SKILL} XP.", null);
                //textObject.SetTextVariable("HERO", Hero.MainHero.Name);
                //Hero.MainHero.AddSkillXp(skilltoget, 1f);
                //textObject.SetTextVariable("SKILL", skilltoget.Name);
                //textObject.SetTextVariable("SKILL_AMOUNT", amount);
                //InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
        }

        private static void SetModifier(int amount, Hero hero, SkillObject skill, SkillObject flag, bool displayMessage = true, bool quickInformation = false)
        {
            if (amount == 0)
            {
                if ((displayMessage || quickInformation) && hero.GetSkillValue(skill) > 0)
                {
                    TextObject textObject = GameTexts.FindText("str_CE_level_start", null);
                    textObject.SetTextVariable("SKILL", skill.Name);
                    textObject.SetTextVariable("HERO", hero.Name);
                    if (hero.GetSkillValue(skill) > 1)
                    {
                        if (displayMessage)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                        }

                        if (quickInformation)
                        {
                            InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                        }
                    }
                }
                hero.SetSkillValue(skill, 0);
            }
            else
            {
                int currentValue = hero.GetSkillValue(skill);
                int valueToSet = currentValue + amount;
                if (valueToSet < 1)
                {
                    valueToSet = 1;
                }
                hero.SetSkillValue(skill, valueToSet);
                if (displayMessage || quickInformation)
                {
                    TextObject textObject = GameTexts.FindText("str_CE_level_skill", null);
                    textObject.SetTextVariable("HERO", hero.Name);
                    textObject.SetTextVariable("SKILL", skill.Name);
                    if (amount >= 0)
                    {
                        textObject.SetTextVariable("NEGATIVE", 0);
                        textObject.SetTextVariable("PLURAL", (amount >= 2) ? 1 : 0);
                    }
                    else
                    {
                        textObject.SetTextVariable("NEGATIVE", 1);
                        textObject.SetTextVariable("PLURAL", (amount <= -2) ? 1 : 0);
                    }
                    textObject.SetTextVariable("SKILL_AMOUNT", Math.Abs(amount));
                    textObject.SetTextVariable("TOTAL_AMOUNT", (currentValue + amount));
                    if (displayMessage)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                    }

                    if (quickInformation)
                    {
                        InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
            }
        }

        public static void VictimSlaveryModifier(int amount, Hero hero, bool updateFlag = false, bool displayMessage = true, bool quickInformation = false)
        {
            if (hero != null)
            {
                SkillObject SlaverySkill = CESkills.Slavery;
                SkillObject SlaveryFlag = CESkills.IsSlave;

                if (updateFlag)
                {
                    int currentLevel = hero.GetSkillValue(SlaveryFlag);
                    if (amount == 0)
                    {
                        if ((displayMessage || quickInformation) && currentLevel != 0)
                        {
                            TextObject textObject = GameTexts.FindText("str_CE_level_leave", null);
                            textObject.SetTextVariable("HERO", hero.Name);
                            textObject.SetTextVariable("OCCUPATION", SlaveryFlag.Name);
                            if (displayMessage)
                            {
                                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                            }

                            if (quickInformation)
                            {
                                InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                            }
                        }
                    }
                    else
                    {
                        if ((displayMessage || quickInformation) && currentLevel != 1)
                        {
                            TextObject textObject = GameTexts.FindText("str_CE_level_enter", null);
                            textObject.SetTextVariable("HERO", hero.Name);
                            textObject.SetTextVariable("OCCUPATION", SlaveryFlag.Name);
                            if (displayMessage)
                            {
                                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                            }

                            if (quickInformation)
                            {
                                InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                            }
                        }
                    }
                    hero.SetSkillValue(SlaveryFlag, amount);
                }
                else
                {
                    SetModifier(amount, hero, SlaverySkill, SlaveryFlag, true, false);
                }
            }
        }

        public static void VictimProstitutionModifier(int amount, Hero hero, bool updateFlag = false, bool displayMessage = true, bool quickInformation = false)
        {
            if (hero != null)
            {
                SkillObject ProstitutionSkill = CESkills.Prostitution;
                SkillObject ProstitutionFlag = CESkills.IsProstitute;

                if (updateFlag)
                {
                    int currentLevel = hero.GetSkillValue(ProstitutionFlag);
                    if (amount == 0)
                    {
                        if ((displayMessage || quickInformation) && currentLevel != 0)
                        {
                            TextObject textObject = GameTexts.FindText("str_CE_level_leave", null);
                            textObject.SetTextVariable("HERO", hero.Name);
                            textObject.SetTextVariable("OCCUPATION", ProstitutionFlag.Name);
                            if (displayMessage)
                            {
                                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                            }

                            if (quickInformation)
                            {
                                InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                            }
                        }
                    }
                    else
                    {
                        if ((displayMessage || quickInformation) && currentLevel != 1)
                        {
                            TextObject textObject = GameTexts.FindText("str_CE_level_enter", null);
                            textObject.SetTextVariable("HERO", hero.Name);
                            textObject.SetTextVariable("OCCUPATION", ProstitutionFlag.Name);
                            if (displayMessage)
                            {
                                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                            }

                            if (quickInformation)
                            {
                                InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                            }
                        }
                    }
                    hero.SetSkillValue(ProstitutionFlag, amount);
                }
                else
                {
                    SetModifier(amount, hero, ProstitutionSkill, ProstitutionFlag, displayMessage, quickInformation);
                }
            }
        }

        private static void SkillModifier(Hero hero, string skill, int amount = 0)
        {
            foreach (SkillObject skillObject in SkillObject.All)
            {
                if (skillObject.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skill)
                {
                    int currentSkillLevel = hero.GetSkillValue(skillObject);
                    int newNumber = currentSkillLevel + amount;
                    if (newNumber < 0)
                    {
                        newNumber = 0;
                    }

                    hero.SetSkillValue(skillObject, newNumber);
                    if (amount != 0)
                    {
                        TextObject textObject = GameTexts.FindText("str_CE_level_skill", null);
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("NEGATIVE", amount > 0 ? 0 : 1);
                        textObject.SetTextVariable("SKILL_AMOUNT", Math.Abs(amount));
                        textObject.SetTextVariable("PLURAL", amount > 1 || amount < 1 ? 1 : 0);
                        textObject.SetTextVariable("SKILL", skill.ToLower());
                        textObject.SetTextVariable("TOTAL_AMOUNT", newNumber);
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                    }
                }
            }
        }

        private static void TraitModifier(Hero hero, string trait, int amount = 0)
        {
            bool found = false;
            foreach (TraitObject traitObject in DefaultTraits.Personality)
            {
                if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                {
                    found = true;
                    int currentTraitLevel = hero.GetTraitLevel(traitObject);
                    int newNumber = currentTraitLevel + amount;
                    if (newNumber >= traitObject.MinValue && newNumber <= traitObject.MaxValue)
                    {
                        hero.SetTraitLevel(traitObject, newNumber);
                    }
                    if (amount != 0)
                    {
                        TextObject textObject = GameTexts.FindText("str_CE_trait_level", null);
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
                        textObject.SetTextVariable("TRAIT", CEStrings.FetchTraitString(trait));
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                    }
                }
            }

            if (!found)
            {
                foreach (TraitObject traitObject in DefaultTraits.SkillCategories)
                {
                    if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                    {
                        found = true;
                        int currentTraitLevel = hero.GetTraitLevel(traitObject);
                        int newNumber = currentTraitLevel + amount;
                        if (newNumber >= traitObject.MinValue && newNumber <= traitObject.MaxValue)
                        {
                            hero.SetTraitLevel(traitObject, newNumber);
                        }
                        if (amount != 0)
                        {
                            TextObject textObject = GameTexts.FindText("str_CE_trait_level", null);
                            textObject.SetTextVariable("HERO", hero.Name);
                            textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
                            textObject.SetTextVariable("TRAIT", CEStrings.FetchTraitString(trait));
                            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
                        }
                    }
                }
            }

            if (!found)
            {
                foreach (TraitObject traitObject in DefaultTraits.All)
                {
                    if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                    {
                        found = true;
                        int currentTraitLevel = hero.GetTraitLevel(traitObject);
                        int newNumber = currentTraitLevel + amount;
                        if (newNumber >= traitObject.MinValue && newNumber <= traitObject.MaxValue)
                        {
                            hero.SetTraitLevel(traitObject, newNumber);
                        }
                        if (amount != 0)
                        {
                            if (CESettings.Instance.LogToggle)
                            {
                                TextObject textObject = GameTexts.FindText("str_CE_trait_level", null);
                                textObject.SetTextVariable("HERO", hero.Name);
                                textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
                                textObject.SetTextVariable("TRAIT", trait);
                                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                            }
                        }
                    }
                }

                if (!found)
                {
                    CECustomHandler.ForceLogToFile("Unable to find : " + trait);
                }
            }
        }

        private static void MoralChange(int amount, PartyBase partyBase)
        {
            if (partyBase.IsMobile && amount != 0)
            {
                TextObject textObject = GameTexts.FindText("str_CE_morale_level", null);
                textObject.SetTextVariable("PARTY", partyBase.Name);
                textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                partyBase.MobileParty.RecentEventsMorale += amount;
            }
        }

        private static void RenownModifier(int amount, Hero hero)
        {
            if (hero != null && amount != 0)
            {
                hero.Clan.Renown += amount;
                if (hero.Clan.Renown < CESettings.Instance.RenownMin)
                {
                    hero.Clan.Renown = CESettings.Instance.RenownMin;
                }

                TextObject textObject = GameTexts.FindText("str_CE_renown_level", null);
                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
                textObject.SetTextVariable("AMOUNT", Math.Abs(amount));
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
            }
        }

        public static void RelationsModifier(Hero hero1, int relationChange, Hero hero2 = null)
        {
            if (hero1 != null && relationChange != 0)
            {
                if (hero2 == null)
                {
                    hero2 = Hero.MainHero;
                }

                Campaign.Current.Models.DiplomacyModel.GetHeroesForEffectiveRelation(hero1, hero2, out Hero hero3, out Hero hero4);
                int value = CharacterRelationManager.GetHeroRelation(hero3, hero4) + relationChange;
                value = MBMath.ClampInt(value, -100, 100);
                hero3.SetPersonalRelation(hero4, value);

                TextObject textObject = GameTexts.FindText("str_CE_relationship_level");
                textObject.SetTextVariable("PLAYER_HERO", hero2.Name);
                textObject.SetTextVariable("HERO", hero1.Name);
                textObject.SetTextVariable("POSITIVE", relationChange >= 0 ? 1 : 0);
                textObject.SetTextVariable("AMOUNT", Math.Abs(relationChange));
                textObject.SetTextVariable("TOTAL", value);
                InformationManager.AddQuickInformation(textObject, 0, hero1.CharacterObject, "event:/ui/notification/relation");
            }
        }

        private static void ChangeClan(Hero hero, Hero owner)
        {
            if (hero != null)
            {
                if (owner != null)
                {
                    hero.Clan = owner.Clan;
                }
            }
        }

        private static void ChangeSpouse(Hero hero, Hero spouseHero)
        {
            Hero heroSpouse = hero.Spouse;
            if (heroSpouse != null)
            {
                TextObject textObject = GameTexts.FindText("str_CE_spouse_leave", null);
                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("SPOUSE", heroSpouse.Name);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));

                if (heroSpouse.Father != null)
                {
                    heroSpouse.Clan = heroSpouse.Father.Clan;
                }
                else if (heroSpouse.Mother != null)
                {
                    heroSpouse.Clan = heroSpouse.Mother.Clan;
                }
                hero.Spouse = null;
            }

            if (spouseHero != null)
            {
                Hero spouseHeroSpouse = spouseHero.Spouse;
                if (spouseHeroSpouse != null)
                {
                    TextObject textObject3 = GameTexts.FindText("str_CE_spouse_leave", null);
                    textObject3.SetTextVariable("HERO", hero.Name);
                    textObject3.SetTextVariable("SPOUSE", spouseHeroSpouse.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                    if (spouseHeroSpouse.Father != null)
                    {
                        spouseHeroSpouse.Clan = spouseHeroSpouse.Father.Clan;
                    }
                    else if (spouseHeroSpouse.Mother != null)
                    {
                        spouseHeroSpouse.Clan = spouseHeroSpouse.Mother.Clan;
                    }
                    spouseHero.Spouse = null;
                }

                MarriageAction.Apply(hero, spouseHero, true);
            }
        }

        // Impregnation Systems
        public static void ImpregnationChance(Hero targetHero, int modifier = 0, bool forcePreg = false, Hero senderHero = null)
        {
            if (targetHero != null && targetHero.IsFemale && !targetHero.IsPregnant)
            {
                if (IsHeroAgeSuitableForPregnancy(targetHero) && CESettings.Instance.PregnancyToggle)
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) { modifier = 0; }

                    if (MBRandom.Random.Next(100) < (CESettings.Instance.AttractivenessSkill ? (AttractivenessScore(targetHero) / 20) + modifier : CESettings.Instance.PregnancyChance + modifier))
                    {
                        Hero randomSoldier;
                        if (senderHero != null)
                        {
                            randomSoldier = senderHero;
                        }
                        else if (targetHero.CurrentSettlement != null && targetHero.CurrentSettlement.MilitaParty != null && !targetHero.CurrentSettlement.MilitaParty.MemberRoster.IsEmpty())
                        {
                            Settlement settlementCurrent = targetHero.CurrentSettlement;
                            IEnumerable<TroopRosterElement> maleMembers = settlementCurrent.MilitaParty.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == false; });
                            if (maleMembers.Count() == 0)
                            {
                                return;
                            }

                            CharacterObject m = maleMembers.GetRandomElement().Character;
                            randomSoldier = HeroCreator.CreateSpecialHero(m, settlementCurrent, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                            randomSoldier.IsNoble = true;
                        }
                        else if (targetHero.PartyBelongedTo != null)
                        {
                            IEnumerable<TroopRosterElement> maleMembers = targetHero.PartyBelongedTo.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == false; });
                            if (maleMembers.Count() == 0)
                            {
                                return;
                            }

                            CharacterObject m = maleMembers.GetRandomElement().Character;
                            randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedTo.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                            randomSoldier.IsNoble = true;
                        }
                        else
                        {
                            CharacterObject m = CharacterObject.Templates.Where((characterObject) => { return characterObject.IsFemale == false && characterObject.Occupation == Occupation.Outlaw; }).GetRandomElement();
                            randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                            randomSoldier.IsNoble = true;
                        }

                        TextObject textObject3 = GameTexts.FindText("str_CE_impregnated", null);
                        textObject3.SetTextVariable("HERO", targetHero.Name);
                        textObject3.SetTextVariable("SPOUSE", randomSoldier.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                        CEHelper.spouseOne = randomSoldier;
                        CEHelper.spouseTwo = targetHero;
                        MakePregnantAction.Apply(targetHero);
                        CEHelper.spouseOne = CEHelper.spouseTwo = null;

                        //RelationsModifier(randomSoldier, 50, targetHero);

                        return;
                    }
                }
                else if (forcePreg)
                {
                    CharacterObject m = CharacterObject.Templates.Where((characterObject) => { return characterObject.IsFemale == false; }).GetRandomElement();
                    Hero randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.BornSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    Hero spouse = targetHero.Spouse;
                    targetHero.Spouse = randomSoldier;
                    MakePregnantAction.Apply(targetHero);
                    targetHero.Spouse = spouse;
                    TextObject textObject4 = new TextObject("{PLAYER_HERO} forced impregnated by {PLAYER_SPOUSE}.", null);
                    textObject4.SetTextVariable("PLAYER_HERO", targetHero.Name);
                    textObject4.SetTextVariable("PLAYER_SPOUSE", randomSoldier.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject4.ToString(), Colors.Magenta));
                }
            }
            else if (targetHero != null && !targetHero.IsFemale)
            {
                if (CESettings.Instance.PregnancyToggle)
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) { modifier = 0; }

                    if (MBRandom.Random.Next(100) < (CESettings.Instance.AttractivenessSkill ? (AttractivenessScore(targetHero) / 20) + modifier : CESettings.Instance.PregnancyChance + modifier))
                    {
                        Hero randomSoldier = null;
                        if (senderHero != null)
                        {
                            randomSoldier = senderHero;
                        }
                        else if (targetHero.CurrentSettlement != null && targetHero.CurrentSettlement.MilitaParty != null && !targetHero.CurrentSettlement.MilitaParty.MemberRoster.IsEmpty())
                        {
                            Settlement settlementCurrent = targetHero.CurrentSettlement;
                            IEnumerable<TroopRosterElement> femaleMembers = settlementCurrent.MilitaParty.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == true; });
                            if (femaleMembers.Count() == 0)
                            {
                                return;
                            }

                            do
                            {
                                CharacterObject m = femaleMembers.GetRandomElement().Character;
                                randomSoldier = HeroCreator.CreateSpecialHero(m, settlementCurrent, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);

                                randomSoldier.IsNoble = true;
                            }
                            while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                        }
                        else if (targetHero.PartyBelongedTo != null)
                        {
                            IEnumerable<TroopRosterElement> femaleMembers = targetHero.PartyBelongedTo.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == true; });
                            if (femaleMembers.Count() == 0)
                            {
                                return;
                            }

                            do
                            {
                                CharacterObject m = femaleMembers.GetRandomElement().Character;
                                randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedTo.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                                randomSoldier.IsNoble = true;
                            }
                            while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                        }
                        else
                        {
                            CharacterObject m = CharacterObject.Templates.Where((characterObject) => { return characterObject.IsFemale == true && characterObject.Occupation == Occupation.Outlaw; }).GetRandomElement();
                            randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                            randomSoldier.IsNoble = true;
                        }

                        TextObject textObject3 = GameTexts.FindText("str_CE_impregnated", null);
                        textObject3.SetTextVariable("HERO", randomSoldier.Name);
                        textObject3.SetTextVariable("SPOUSE", targetHero.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                        Hero spouse = targetHero.Spouse;
                        Hero randomSoldierSpouse = randomSoldier.Spouse;
                        targetHero.Spouse = randomSoldier;
                        randomSoldier.Spouse = targetHero;
                        MakePregnantAction.Apply(randomSoldier);

                        //RelationsModifier(randomSoldier, 50, targetHero);

                        targetHero.Spouse = spouse;
                        randomSoldier.Spouse = randomSoldierSpouse;

                        return;
                    }
                }
            }
        }

        public static void CaptivityImpregnationChance(Hero targetHero, int modifier = 0, bool forcePreg = false, bool lord = true, Hero captorHero = null)
        {
            if (targetHero != null && targetHero.IsFemale && !targetHero.IsPregnant)
            {
                if (IsHeroAgeSuitableForPregnancy(targetHero) && CESettings.Instance.PregnancyToggle)
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) { modifier = 0; }

                    if (MBRandom.Random.Next(100) < (CESettings.Instance.AttractivenessSkill ? (AttractivenessScore(targetHero) / 20) + modifier : CESettings.Instance.PregnancyChance + modifier))
                    {
                        Hero randomSoldier;
                        if (captorHero != null)
                        {
                            randomSoldier = captorHero;
                        }
                        else if (lord && CECampaignBehavior.extraVariables.Owner != null)
                        {
                            randomSoldier = CECampaignBehavior.extraVariables.Owner;
                        }
                        else if (lord && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null && targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero != null && !targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero.IsFemale)
                        {
                            randomSoldier = targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero;
                        }
                        else if (targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null)
                        {
                            IEnumerable<TroopRosterElement> maleMembers = targetHero.PartyBelongedToAsPrisoner.MobileParty.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == false; });
                            if (maleMembers.Count() == 0)
                            {
                                return;
                            }

                            CharacterObject m = maleMembers.GetRandomElement().Character;
                            randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                            randomSoldier.IsNoble = true;
                        }
                        else if (targetHero.PartyBelongedToAsPrisoner.IsSettlement && targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty != null && !targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty.MemberRoster.IsEmpty())
                        {
                            Settlement playerCaptor = targetHero.PartyBelongedToAsPrisoner.Settlement;
                            IEnumerable<TroopRosterElement> maleMembers = playerCaptor.MilitaParty.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == false; });
                            if (maleMembers.Count() == 0)
                            {
                                return;
                            }

                            CharacterObject m = maleMembers.GetRandomElement().Character;
                            randomSoldier = HeroCreator.CreateSpecialHero(m, playerCaptor, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                            randomSoldier.IsNoble = true;
                        }
                        else
                        {
                            CharacterObject m = CharacterObject.Templates.Where((characterObject) => { return characterObject.IsFemale == false && characterObject.Occupation == Occupation.Outlaw; }).GetRandomElement();
                            randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                            randomSoldier.IsNoble = true;
                        }

                        TextObject textObject3 = GameTexts.FindText("str_CE_impregnated", null);
                        textObject3.SetTextVariable("HERO", targetHero.Name);
                        textObject3.SetTextVariable("SPOUSE", randomSoldier.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                        Hero spouse = targetHero.Spouse;
                        Hero randomSoldierSpouse = randomSoldier.Spouse;
                        targetHero.Spouse = randomSoldier;
                        randomSoldier.Spouse = targetHero;
                        MakePregnantAction.Apply(targetHero);

                        //RelationsModifier(randomSoldier, 50, targetHero);

                        targetHero.Spouse = spouse;
                        randomSoldier.Spouse = randomSoldierSpouse;

                        return;
                    }
                }
                else if (forcePreg)
                {
                    CharacterObject m = CharacterObject.Templates.Where((characterObject) => { return characterObject.IsFemale == false; }).GetRandomElement();
                    Hero randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.BornSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    Hero spouse = targetHero.Spouse;
                    targetHero.Spouse = randomSoldier;
                    MakePregnantAction.Apply(targetHero);
                    targetHero.Spouse = spouse;
                    TextObject textObject4 = new TextObject("{PLAYER_HERO} forced impregnated by {PLAYER_SPOUSE}.", null);
                    textObject4.SetTextVariable("PLAYER_HERO", targetHero.Name);
                    textObject4.SetTextVariable("PLAYER_SPOUSE", randomSoldier.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject4.ToString(), Colors.Magenta));
                }
            }
            else if (targetHero != null && !targetHero.IsFemale)
            {
                if (CESettings.Instance.PregnancyToggle)
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) { modifier = 0; }

                    if (MBRandom.Random.Next(100) < (CESettings.Instance.AttractivenessSkill ? (AttractivenessScore(targetHero) / 20) + modifier : CESettings.Instance.PregnancyChance + modifier))
                    {
                        Hero randomSoldier = null;
                        if (captorHero != null)
                        {
                            randomSoldier = captorHero;
                        }
                        else if (lord && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null && targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero != null && targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero.IsFemale)
                        {
                            randomSoldier = targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero;
                        }
                        else if (targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null)
                        {
                            IEnumerable<TroopRosterElement> femaleMembers = targetHero.PartyBelongedToAsPrisoner.MobileParty.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == true; });
                            if (femaleMembers.Count() == 0)
                            {
                                return;
                            }

                            do
                            {
                                CharacterObject m = femaleMembers.GetRandomElement().Character;
                                randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                                randomSoldier.IsNoble = true;
                            }
                            while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                        }
                        else if (targetHero.PartyBelongedToAsPrisoner.IsSettlement && targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty != null && !targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty.MemberRoster.IsEmpty())
                        {
                            Settlement playerCaptor = targetHero.PartyBelongedToAsPrisoner.Settlement;
                            IEnumerable<TroopRosterElement> femaleMembers = playerCaptor.MilitaParty.MemberRoster.Where((characterObject) => { return characterObject.Character.IsFemale == true; });
                            if (femaleMembers.Count() == 0)
                            {
                                return;
                            }

                            do
                            {
                                CharacterObject m = femaleMembers.GetRandomElement().Character;
                                randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                                randomSoldier.IsNoble = true;
                            }
                            while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                        }
                        else
                        {
                            CharacterObject m = CharacterObject.Templates.Where((characterObject) => { return characterObject.IsFemale == true && characterObject.Occupation == Occupation.Outlaw; }).GetRandomElement();
                            randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement((Settlement x) => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                            randomSoldier.IsNoble = true;
                        }

                        TextObject textObject3 = GameTexts.FindText("str_CE_impregnated", null);
                        textObject3.SetTextVariable("HERO", randomSoldier.Name);
                        textObject3.SetTextVariable("SPOUSE", targetHero.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                        Hero spouse = targetHero.Spouse;
                        Hero randomSoldierSpouse = randomSoldier.Spouse;
                        targetHero.Spouse = randomSoldier;
                        randomSoldier.Spouse = targetHero;
                        MakePregnantAction.Apply(randomSoldier);

                        //RelationsModifier(randomSoldier, 50, targetHero);

                        targetHero.Spouse = spouse;
                        randomSoldier.Spouse = randomSoldierSpouse;

                        return;
                    }
                }
            }
        }

        private static bool IsHeroAgeSuitableForPregnancy(Hero hero)
        {
            return hero != null && hero.Age >= 18f && hero.Age <= 45f && !CECampaignBehavior.CheckIfPregnancyExists(hero);
        }

        // Score Calculations
        private static int AttractivenessScore(Hero targetHero)
        {
            if (targetHero != null)
            {
                int num = 0;
                if (targetHero.GetPerkValue(DefaultPerks.Medicine.PerfectHealth))
                {
                    num += 10;
                }
                if (targetHero.GetPerkValue(DefaultPerks.Steward.Prominence))
                {
                    num += 15;
                }
                if (targetHero.GetPerkValue(DefaultPerks.Charm.InBloom))
                {
                    num += 5;
                }

                return (targetHero.GetSkillValue(DefaultSkills.Charm) + targetHero.GetSkillValue(DefaultSkills.Athletics) / 2 + targetHero.GetSkillValue(DefaultSkills.Roguery) / 3 + targetHero.GetAttributeValue(CharacterAttributesEnum.Social) * 5 + num) / 2;
            }
            else
            {
                return 10;
            }
        }

        private static int EscapeProwessScore(Hero targetHero)
        {
            return (targetHero.GetSkillValue(DefaultSkills.Tactics) / 4 + targetHero.GetSkillValue(DefaultSkills.Roguery) / 4) / 4;
        }
    }
}