using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Models;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelBehavior : CampaignBehaviorBase
    {
        private static readonly Location _Brothel = new Location("_Brothel", new TextObject("{=CEEVENTS1099}Brothel"), new TextObject("{=CEEVENTS1099}Brothel"), 30, true, false, "CanIfSettlementAccessModelLetsPlayer", "CanAlways", "CanAlways", "CanIfGrownUpMaleOrHero", new[] {"empire_house_c_tavern_a", "", "", ""}, null);

        private bool _isBrothelInitialized;

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            if (CESettings.Instance != null && !CESettings.Instance.ProstitutionControl) return;

            // Option Added To Town
            campaignGameStarter.AddGameMenuOption("town", "town_brothel", "{=CEEVENTS1100}Go to the _Brothel district", CanGoToBrothelDistrictOnCondition, delegate
                                                                                                                                                           {
                                                                                                                                                               GameMenu.SwitchToMenu("town_brothel");
                                                                                                                                                           }, false, 1);

            campaignGameStarter.AddGameMenu("town_brothel", "{=CEEVENTS1098}You are in the _Brothel district", BrothelDistrictOnInit, GameOverlays.MenuOverlayType.SettlementWithCharacters);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_visit", "{=CEEVENTS1101}Visit the _Brothel", VisitBrothelOnCondition, VisitBrothelOnConsequence, false, 0);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_prostitution", "{=CEEVENTS1102}Become a prostitute at the _Brothel", ProstitutionMenuJoinOnCondition, ProstitutionMenuJoinOnConsequence, false, 1);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_some_captives", "{=CEEVENTS1097}Sell some captives to the _Brothel", SellPrisonerOneStackOnCondition, delegate
                                                                                                                                                                                           {
                                                                                                                                                                                               ChooseRansomPrisoners();
                                                                                                                                                                                           }, false, 2);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_all_prisoners", "{=CEEVENTS1096}Sell all captives to the _Brothel ({RANSOM_AMOUNT}{GOLD_ICON})", SellPrisonersCondition, delegate
                                                                                                                                                                                                              {
                                                                                                                                                                                                                  SellAllPrisoners();
                                                                                                                                                                                                              }, false, 3);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_back", "{=qWAmxyYz}Back to town center", BackOnCondition, delegate
                                                                                                                                          {
                                                                                                                                              GameMenu.SwitchToMenu("town");
                                                                                                                                          }, true, 4);
        }

        // Ransom Functions
        private static int GetRansomValueOfAllPrisoners()
        {
            return Enumerable.Sum(PartyBase.MainParty.PrisonRoster, troopRosterElement => troopRosterElement.Character.PrisonerRansomValue(Hero.MainHero) * troopRosterElement.Number);
        }

        private static bool SellPrisonersCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count <= 0) return false;

            var ransomValueOfAllPrisoners = GetRansomValueOfAllPrisoners();
            MBTextManager.SetTextVariable("RANSOM_AMOUNT", ransomValueOfAllPrisoners);
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            return true;
        }

        private static void SellAllPrisoners()
        {
            SellPrisonersAction.ApplyForAllPrisoners(MobileParty.MainParty, MobileParty.MainParty.PrisonRoster, Settlement.CurrentSettlement);
            GameMenu.SwitchToMenu("town_brothel");
        }

        private static void ChooseRansomPrisoners()
        {
            GameMenu.SwitchToMenu("town_brothel");
            PartyScreenManager.OpenScreenAsRansom();
        }

        private static bool SellPrisonerOneStackOnCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count <= 0) return false;

            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            return true;
        }

        // Back Condition
        private static bool BackOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;

            return true;
        }

        // Brothel District Menu
        private static bool CheckAndOpenNextLocation(MenuCallbackArgs args)
        {
            if (Campaign.Current.GameMenuManager.NextLocation == null || !(GameStateManager.Current.ActiveState is MapState)) return false;

            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, Campaign.Current.GameMenuManager.PreviousLocation);
            Campaign.Current.GameMenuManager.SetNextMenu("town_brothel");
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;

            return true;
        }

        // Brothel Menu
        private static void BrothelDistrictOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            var settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty.CurrentSettlement;
            _Brothel.SetOwnerComplex(settlement.LocationComplex);

            switch (settlement.Culture.GetCultureCode())
            {
                case CultureCode.Sturgia:
                    _Brothel.SetSceneName(0, "sturgia_house_a_interior_tavern");

                    break;

                case CultureCode.Vlandia:
                    _Brothel.SetSceneName(0, "vlandia_tavern_interior_a");

                    break;

                case CultureCode.Aserai:
                    _Brothel.SetSceneName(0, "arabian_house_new_c_interior_b_tavern");

                    break;

                case CultureCode.Empire:
                    _Brothel.SetSceneName(0, "empire_house_c_tavern_a");

                    break;

                case CultureCode.Battania:
                    _Brothel.SetSceneName(0, "battania_tavern_interior_b");

                    break;

                case CultureCode.Khuzait:
                    _Brothel.SetSceneName(0, "khuzait_tavern_a");

                    break;

                case CultureCode.Invalid:
                    break;
                case CultureCode.Nord:
                    break;
                case CultureCode.Darshi:
                    break;
                case CultureCode.Vakken:
                    break;
                case CultureCode.AnyOtherCulture:
                    break;
                default:
                    _Brothel.SetSceneName(0, "empire_house_c_tavern_a");

                    break;
            }

            Campaign.Current.GameMenuManager.MenuLocations.Add(_Brothel);

            if (CheckAndOpenNextLocation(args)) return;
            args.MenuTitle = new TextObject("{=CEEVENTS1099}Brothel");
        }

        public static bool CanGoToBrothelDistrictOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;

            return true;
        }

        public static bool VisitBrothelOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Mission;

            return true;
        }

        private static void VisitBrothelOnConsequence(MenuCallbackArgs args)
        {
            if (((TownEncounter) PlayerEncounter.LocationEncounter).IsAmbush)
            {
                GameMenu.ActivateGameMenu("menu_town_thugs_start");

                return;
            }

            Campaign.Current.GameMenuManager.NextLocation = _Brothel;
            Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation);
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;
        }

        public static bool ProstitutionMenuJoinOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;

            if (!CEContext.brothelFlagFemale && Hero.MainHero.IsFemale || !CEContext.brothelFlagMale && !Hero.MainHero.IsFemale) return false;

            return !Campaign.Current.IsMainHeroDisguised;
        }

        public static void ProstitutionMenuJoinOnConsequence(MenuCallbackArgs args)
        {
            var ProstitueFlag = CESkills.IsProstitute;
            Hero.MainHero.SetSkillValue(ProstitueFlag, 1);
            var ProstitutionSkill = CESkills.Prostitution;

            if (Hero.MainHero.GetSkillValue(ProstitutionSkill) < 100) Hero.MainHero.SetSkillValue(ProstitutionSkill, 100);
            var textObject = GameTexts.FindText("str_CE_join_prostitution");
            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            InformationManager.AddQuickInformation(textObject, 0, CharacterObject.PlayerCharacter, "event:/ui/notification/relation");

            CEPlayerCaptivityModel.CaptureOverride = true;
            var capturerParty = SettlementHelper.FindNearestSettlement(settlement => settlement.IsTown).Party;
            var prisonerCharacter = Hero.MainHero;
            prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
            prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
            while (PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter)) PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, -1, true);
            capturerParty.AddPrisoner(prisonerCharacter.CharacterObject, 1);
            if (prisonerCharacter == Hero.MainHero) PlayerCaptivity.StartCaptivity(capturerParty);
            var test = CEEventLoader.CEWaitingList();
            GameMenu.ExitToLast();
            if (test != null) GameMenu.ActivateGameMenu(test);
        }


        // Brothel Mission
        public void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            var settlement = PlayerEncounter.LocationEncounter.Settlement;

            if (CampaignMission.Current.Location != _Brothel || _isBrothelInitialized) return;

            //LocationComplex.Current.AddPassage(settlement.LocationComplex.GetLocationWithId("center"), _Brothel);
            AddPeopleToTownTavern(settlement, unusedUsablePointCount);
            _isBrothelInitialized = true;
        }

        private static LocationCharacter CreateTavernkeeper(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            var owner = MBObjectManager.Instance.CreateObject<CharacterObject>();

            if (DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
            {
                var templateToCopy = CharacterObject.CreateFrom(culture.TavernWench);
                owner.Culture = templateToCopy.Culture;
                owner.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
                owner.CurrentFormationClass = templateToCopy.CurrentFormationClass;
                owner.DefaultFormationGroup = templateToCopy.DefaultFormationGroup;
                owner.StaticBodyPropertiesMin = templateToCopy.StaticBodyPropertiesMin;
                owner.StaticBodyPropertiesMax = templateToCopy.StaticBodyPropertiesMax;
                owner.IsFemale = true;
                owner.Level = templateToCopy.Level;
                owner.HairTags = templateToCopy.HairTags;
                owner.BeardTags = templateToCopy.BeardTags;
                owner.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());
                owner.Name = new TextObject("{=CEEVENTS1050}Brothel Assistant");
                owner.StringId = "brothel_assistant";
            }
            else
            {
                var templateToCopy2 = CharacterObject.CreateFrom(culture.Tavernkeeper);
                owner.Culture = templateToCopy2.Culture;
                owner.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge);
                owner.CurrentFormationClass = templateToCopy2.CurrentFormationClass;
                owner.DefaultFormationGroup = templateToCopy2.DefaultFormationGroup;
                owner.StaticBodyPropertiesMin = templateToCopy2.StaticBodyPropertiesMin;
                owner.StaticBodyPropertiesMax = templateToCopy2.StaticBodyPropertiesMax;
                owner.IsFemale = true;
                owner.Level = templateToCopy2.Level;
                owner.HairTags = templateToCopy2.HairTags;
                owner.BeardTags = templateToCopy2.BeardTags;
                owner.InitializeEquipmentsOnLoad(templateToCopy2.AllEquipments.ToList());
                owner.Name = new TextObject("{=CEEVENTS1066}Brothel Owner");
                owner.StringId = "brothel_owner";
            }


            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)).Equipment(culture.FemaleDancer.AllEquipments.GetRandomElement()), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "spawnpoint_tavernkeeper", true, relation, "as_human_tavern_keeper", true);
        }

        private static LocationCharacter CreateRansomBroker(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            BasicCharacterObject owner = CharacterObject.CreateFrom(culture.RansomBroker);
            owner.Name = new TextObject("{=CEEVENTS1065}Slave Trader");
            owner.StringId = "brothel_slaver";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors, "npc_common", true, relation, null, true);
        }

        private static LocationCharacter CreateMusician(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(culture.Musician)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "musician", true, relation, "as_human_musician", true, true);
        }

        private static LocationCharacter CreateTownsManForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            var templateToCopy = CharacterObject.CreateFrom(culture.Townsman);
            var townsman = MBObjectManager.Instance.CreateObject<CharacterObject>();
            townsman.Culture = templateToCopy.Culture;
            townsman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townsman.CurrentFormationClass = templateToCopy.CurrentFormationClass;
            townsman.DefaultFormationGroup = templateToCopy.DefaultFormationGroup;
            townsman.StaticBodyPropertiesMin = templateToCopy.StaticBodyPropertiesMin;
            townsman.StaticBodyPropertiesMax = templateToCopy.StaticBodyPropertiesMax;
            townsman.IsFemale = templateToCopy.IsFemale;
            townsman.Level = templateToCopy.Level;
            townsman.Name = templateToCopy.Name;
            townsman.StringId = customerStrings.GetRandomElement();
            townsman.HairTags = templateToCopy.HairTags;
            townsman.BeardTags = templateToCopy.BeardTags;
            townsman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());

            string actionSetCode;

            if (culture != null && (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")) actionSetCode = "as_human_villager_in_aserai_tavern";
            else actionSetCode = "as_human_villager_in_tavern";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townsman)).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_common", true, relation, actionSetCode, true);
        }

        private static LocationCharacter CreateTavernWench(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            var templateToCopy = CharacterObject.CreateFrom(culture.TavernWench);
            var townswoman = MBObjectManager.Instance.CreateObject<CharacterObject>();
            townswoman.Culture = templateToCopy.Culture;
            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townswoman.CurrentFormationClass = templateToCopy.CurrentFormationClass;
            townswoman.DefaultFormationGroup = templateToCopy.DefaultFormationGroup;
            townswoman.StaticBodyPropertiesMin = templateToCopy.StaticBodyPropertiesMin;
            townswoman.StaticBodyPropertiesMax = templateToCopy.StaticBodyPropertiesMax;
            townswoman.IsFemale = templateToCopy.IsFemale;
            townswoman.Level = templateToCopy.Level;
            townswoman.Name = new TextObject("{=CEEVENTS1093}Server");
            townswoman.StringId = "brothel_server";
            townswoman.HairTags = templateToCopy.HairTags;
            townswoman.BeardTags = templateToCopy.BeardTags;
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());


            var agentData = new AgentData(new SimpleAgentOrigin(townswoman)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge));

            return new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "sp_tavern_wench", true, relation, "as_human_barmaid", true) {PrefabNamesForBones = {{agentData.AgentMonster.OffHandItemBoneIndex, "kitchen_pitcher_b_tavern"}}};
        }

        private static LocationCharacter CreateDancer(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            var templateToCopy = CharacterObject.CreateFrom(culture.FemaleDancer);
            var townswoman = MBObjectManager.Instance.CreateObject<CharacterObject>();
            townswoman.Culture = templateToCopy.Culture;
            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townswoman.CurrentFormationClass = templateToCopy.CurrentFormationClass;
            townswoman.DefaultFormationGroup = templateToCopy.DefaultFormationGroup;
            townswoman.StaticBodyPropertiesMin = templateToCopy.StaticBodyPropertiesMin;
            townswoman.StaticBodyPropertiesMax = templateToCopy.StaticBodyPropertiesMax;
            townswoman.IsFemale = templateToCopy.IsFemale;
            townswoman.Level = templateToCopy.Level;
            townswoman.Name = new TextObject("{=CEEVENTS1095}Prostitute");
            townswoman.StringId = prostituteStrings.GetRandomElement();
            townswoman.HairTags = templateToCopy.HairTags;
            townswoman.BeardTags = templateToCopy.BeardTags;
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_dancer", true, relation, "as_human_female_dancer", true);
        }

        private static LocationCharacter CreateTownsWomanForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            var templateToCopy = CharacterObject.CreateFrom(culture.FemaleDancer);
            var townswoman = MBObjectManager.Instance.CreateObject<CharacterObject>();
            townswoman.Culture = templateToCopy.Culture;
            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townswoman.CurrentFormationClass = templateToCopy.CurrentFormationClass;
            townswoman.DefaultFormationGroup = templateToCopy.DefaultFormationGroup;
            townswoman.StaticBodyPropertiesMin = templateToCopy.StaticBodyPropertiesMin;
            townswoman.StaticBodyPropertiesMax = templateToCopy.StaticBodyPropertiesMax;
            townswoman.IsFemale = templateToCopy.IsFemale;
            townswoman.Level = templateToCopy.Level;
            townswoman.Name = new TextObject("{=CEEVENTS1095}Prostitute");
            townswoman.StringId = prostituteStrings.GetRandomElement();
            townswoman.HairTags = templateToCopy.HairTags;
            townswoman.BeardTags = templateToCopy.BeardTags;
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());

            string actionSetCode;

            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait") actionSetCode = "as_human_villager_in_aserai_tavern";
            else actionSetCode = "as_human_villager_in_tavern";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman, -1, Banner.CreateRandomBanner())).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_common", true, relation, actionSetCode, true);
        }

        private void AddPeopleToTownTavern(Settlement settlement, Dictionary<string, int> unusedUsablePointCount)
        {
            unusedUsablePointCount.TryGetValue("npc_common", out var num);
            num -= 3;

            _Brothel.AddLocationCharacters(CreateTavernkeeper, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            _Brothel.AddLocationCharacters(CreateTavernWench, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            _Brothel.AddLocationCharacters(CreateMusician, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            _Brothel.AddLocationCharacters(CreateRansomBroker, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            unusedUsablePointCount.TryGetValue("npc_dancer", out var dancers);
            if (dancers > 0) _Brothel.AddLocationCharacters(CreateDancer, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, dancers);


            if (num <= 0) return;

            var num2 = (int) (num * 0.2f);
            if (num2 > 0) _Brothel.AddLocationCharacters(CreateTownsManForTavern, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num2);
            var num3 = (int) (num * 0.3f);
            if (num3 > 0) _Brothel.AddLocationCharacters(CreateTownsWomanForTavern, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num3);
        }

        protected void AddDialogs(CampaignGameStarter campaignGameStarter)
        {
            // Owner Dialogue Confident Prostitute
            campaignGameStarter.AddDialogLine("prostitute_requirements_owner", "start", "cprostitute_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", () => ConversationWithProstituteIsOwner() && ConversationWithConfidentProstitute(), null);

            campaignGameStarter.AddPlayerLine("cprostitute_owner_00_yes", "cprostitute_owner_00", "prostitute_service_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null);

            campaignGameStarter.AddPlayerLine("cprostitute_owner_00_nevermind", "cprostitute_owner_00", "close_window", "{=CEBROTHEL1002}Continue as you were.", null, null);


            // Owner Dialogue Tired Prostitute
            campaignGameStarter.AddDialogLine("prostitute_requirements_owner", "start", "tprostitute_owner_00", "{=CEBROTHEL1006}Hello {?PLAYER.GENDER}milady{?}my lord{\\?}, I think I need a break.", () => ConversationWithProstituteIsOwner() && ConversationWithTiredProstitute(), null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_yes", "tprostitute_owner_00", "tprostitute_service_01_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_no", "tprostitute_owner_00", "tprostitute_service_01_no_response", "{=CEBROTHEL1004}No, there is plenty of customers waiting, you must continue working.", null, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_break", "tprostitute_owner_00", "tprostitute_owner_00_break_response", "{=CEBROTHEL1003}Go take a break.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_owner_00_break_r", "tprostitute_owner_00_break_response", "close_window", "{=CEBROTHEL1005}Thank you {?PLAYER.GENDER}milady{?}my lord{\\?}.", null, null);

            // Requirements not met
            campaignGameStarter.AddDialogLine("prostitute_requirements_not_met", "start", "close_window", "{=CEBROTHEL1009}Sorry {?PLAYER.GENDER}milady{?}my lord{\\?}, I am currently very busy.", ConversationWithProstituteNotMetRequirements, null);

            campaignGameStarter.AddDialogLine("customer_requirements_not_met", "start", "customer_00", "{=CEBROTHEL1010}What do you want? Cannot you see that I am trying to enjoy the whores here? [ib:normal][rb:unsure]", () => { return ConversationWithCustomerNotMetRequirements() && ConversationWithConfidentCustomer(); }, null);

            campaignGameStarter.AddPlayerLine("customer_00_nevermind", "customer_00", "prostitute_service_no_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            // Confident Customer 00
            campaignGameStarter.AddDialogLine("ccustomer_00_start", "start", "ccustomer_00", "{=CEBROTHEL1014}Well hello there you {?PLAYER.GENDER}fine whore{?}stud{\\?}, would you like {AMOUNT} denars for your services? [ib:confident][rb:very_positive]", () => { return RandomizeConversation(2) && PriceWithProstitute() && ConversationWithConfidentCustomer(); }, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_service", "ccustomer_00", "close_window", "{=CEBROTHEL1015}Yes, my lord I can do that.", null, ConversationCustomerConsequenceSex);

            campaignGameStarter.AddPlayerLine("ccustomer_00_rage", "ccustomer_00", "ccustomer_00_rage_reply", "{=CEBROTHEL1016}Excuse me, I don't work here!", null, null);

            campaignGameStarter.AddDialogLine("ccustomer_00_rage_reply_r", "ccustomer_00_rage_reply", "close_window", "{=!}{RESPONSE_STRING}", ConversationWithCustomerRandomResponseRage, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_nevermind", "ccustomer_00", "close_window", "{=CEBROTHEL1017}Sorry sir, I have to leave.", null, null);

            // Tried Customer 01
            campaignGameStarter.AddDialogLine("tcustomer_00_start", "start", "tcustomer_00", "{=CEBROTHEL1012}Yes? [ib:normal][rb:unsure]", () => ConversationWithTiredCustomer() || ConversationWithConfidentCustomer(), null);

            campaignGameStarter.AddPlayerLine("tcustomer_00_service", "tcustomer_00", "tcustomer_00_talk_service", "{=CEBROTHEL1013}Would you like my services for {AMOUNT} denars?", () => { return PriceWithProstitute() && !ConversationWithCustomerNotMetRequirements(); }, null);

            campaignGameStarter.AddPlayerLine("tcustomer_00_nevermind", "tcustomer_00", "close_window", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            campaignGameStarter.AddDialogLine("tcustomer_00_service_r", "tcustomer_00_talk_service", "close_window", "{=!}{RESPONSE_STRING}", ConversationWithCustomerRandomResponse, null);


            // Confident Prostitute Extra Replies
            campaignGameStarter.AddPlayerLine("prostitute_service_yes", "prostitute_service", "prostitute_service_yes_response", "{=CEBROTHEL1018}That's a fair price I'd say.", null, null, 100, ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("prostitute_service_no", "prostitute_service", "prostitute_service_no_response", "{=CEBROTHEL1019}That's too much, no thanks.", null, null);

            campaignGameStarter.AddDialogLine("prostitute_service_yes_response_id", "prostitute_service_yes_response", "close_window", "{=CEBROTHEL1020}Follow me sweetie. [ib:normal][rb:positive]", null, ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("prostitute_service_no_response_id", "prostitute_service_no_response", "close_window", "{=CEBROTHEL1021}Stop wasting my time then.[ib:aggressive][rb:unsure]", null, null);

            // Dialogue with Confidient Prostitute 00
            campaignGameStarter.AddDialogLine("cprostitute_talk_00", "start", "cprostitute_talk_00_response", "{=CEBROTHEL1022}Hey {?PLAYER.GENDER}beautiful{?}handsome{\\?} want to have some fun? [ib:confident][rb:very_positive]", () => { return RandomizeConversation(3) && ConversationWithConfidentProstitute(); }, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_00_service_r", "cprostitute_talk_00_response", "cprostitute_talk_00_service", "{=CEBROTHEL1023}Yeah, I could go for a bit of refreshment.", null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_00_nevermind_r", "cprostitute_talk_00_response", "prostitute_service_no_response", "{=CEBROTHEL1024}No, I am fine.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_00_service_ar", "cprostitute_talk_00_service", "prostitute_service", "{=CEBROTHEL1025}Sounds good, that'll be {AMOUNT} denars. [ib:confident][rb:very_positive]", PriceWithProstitute, null);

            // Dialogue with Confidient Prostitute 01
            campaignGameStarter.AddDialogLine("cprostitute_talk_01", "start", "cprostitute_talk_01_response", "{=CEBROTHEL1026}Hey {?PLAYER.GENDER}cutie{?}handsome{\\?} you look like you need some companionship.[ib:confident][rb:very_positive]", () => { return RandomizeConversation(3) && ConversationWithConfidentProstitute(); }, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_01_service_r", "cprostitute_talk_01_response", "cprostitute_talk_01_service", "{=CEBROTHEL1027}I have been awfully lonely...", null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_01_nevermind_r", "cprostitute_talk_01_response", "prostitute_service_no_response", "{=CEBROTHEL1028}No thanks.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_01_service_ar", "cprostitute_talk_01_service", "prostitute_service", "{=CEBROTHEL1029}I'm sorry to hear that, let's change that tonight for {AMOUNT} denars. [ib:confident][rb:very_positive]", PriceWithProstitute, null);

            // Dialogue with Confidient Prostitute 02
            campaignGameStarter.AddDialogLine("cprostitute_talk_02", "start", "cprostitute_talk_02_response", "{=CEBROTHEL1030}Is there something I can help you with this evening?[ib:confident][rb:very_positive]", ConversationWithConfidentProstitute, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_02_service_r", "cprostitute_talk_02_response", "cprostitute_talk_02_service", "{=CEBROTHEL1031}Yeah, I think you can help me with the problem I'm having.", null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_02_nevermind_r", "cprostitute_talk_02_response", "cprostitute_service_02_no_response", "{=CEBROTHEL1032}Maybe later.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_02_service_ar", "cprostitute_talk_02_service", "cprostitute_service_02", "{=CEBROTHEL1033}Perfect, my \"treatment\" costs {AMOUNT} denars for a full dose.[ib:confident][rb:very_positive]", PriceWithProstitute, null);

            campaignGameStarter.AddPlayerLine("cprostitute_service_02_yes", "cprostitute_service_02", "cprostitute_service_02_yes_response", "{=CEBROTHEL1034}Sounds like my kind of cure.", null, null, 100, ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("cprostitute_service_02_no", "cprostitute_service_02", "cprostitute_service_02_no_response", "{=CEBROTHEL1035}You know, my condition isn't that bad.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_service_02_yes_response_id", "cprostitute_service_02_yes_response", "close_window", "{=CEBROTHEL1036}Let's go to the doctor's office so you can be treated.[ib:confident][rb:very_positive]", null, ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("cprostitute_service_02_no_response_id", "cprostitute_service_02_no_response", "close_window", "{=CEBROTHEL1037}See ya around.[ib:confident][rb:very_positive]", null, null);

            // Dialogue with Tired Prostitute 00
            campaignGameStarter.AddDialogLine("tprostitute_talk_00", "start", "tprostitute_talk_response_00", "{=CEBROTHEL1038}What do you want?[ib:closed][rb:unsure]", () => { return RandomizeConversation(2) && ConversationWithTiredProstitute(); }, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_00", "tprostitute_talk_response_00", "tprostitute_service_accept_00", "{=CEBROTHEL1039}I'd like your services for the evening.", null, null);
            campaignGameStarter.AddPlayerLine("tprostitute_nevermind_00", "tprostitute_talk_response_00", "prostitute_service_no_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_accept_00_r", "tprostitute_service_accept_00", "tprostitute_service_00", "{=CEBROTHEL1040}Fine, but it's going to be {AMOUNT} denars up front.[ib:closed][rb:unsure]", PriceWithProstitute, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_00_yes", "tprostitute_service_00", "tprostitute_service_00_yes_response", "{=CEBROTHEL1041}Very well.", null, null, 100, ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("tprostitute_service_00_no", "tprostitute_service_00", "prostitute_service_no_response", "{=CEBROTHEL1042}That's a bit too much.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_00_yes_response_id", "tprostitute_service_00_yes_response", "close_window", "{=CEBROTHEL1043}Right this way...[ib:closed][rb:unsure]", null, ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("tprostitute_service_00_no_response_id", "tprostitute_service_00_no_response", "close_window", "{=CEBROTHEL1044}Thank goodness...[ib:closed][rb:unsure]", null, null);

            //  Dialogue with Tired Prostitute 01
            campaignGameStarter.AddDialogLine("tprostitute_talk_01", "start", "tprostitute_talk_response_01", "{=CEBROTHEL1045}Is there something you want?[ib:closed][rb:unsure]", ConversationWithTiredProstitute, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_01", "tprostitute_talk_response_01", "tprostitute_service_accept_01", "{=CEBROTHEL1046}How much for your time?", null, null);
            campaignGameStarter.AddPlayerLine("tprostitute_nevermind_01", "tprostitute_talk_response_01", "tprostitute_service_01_no_response", "{=CEBROTHEL1047}Not as this moment.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_accept_01_r", "tprostitute_service_accept_01", "tprostitute_service_01", "{=CEBROTHEL1048}Ok well... {AMOUNT} denars sounds about right.[ib:closed][rb:unsure]", PriceWithProstitute, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_01_yes", "tprostitute_service_01", "tprostitute_service_01_yes_response", "{=CEBROTHEL1049}Alright, here you go.", null, null, 100, ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("tprostitute_service_01_no", "tprostitute_service_01", "tprostitute_service_01_no_response", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_01_yes_response_id", "tprostitute_service_01_yes_response", "close_window", "{=CEBROTHEL1051}Follow me to the back.[ib:closed][rb:unsure]", null, ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("tprostitute_service_01_no_response_id", "tprostitute_service_01_no_response", "close_window", "{=CEBROTHEL1052}Ugh...[ib:closed][rb:unsure]", null, null);

            // Dialogue With Owner 00
            campaignGameStarter.AddDialogLine("ce_owner_talk_00", "start", "ce_owner_response_00", "{=CEBROTHEL1053}Oh, a valued customer, how can I help you today?[ib:confident][rb:very_positive]", ConversationWithBrothelOwnerBeforeSelling, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_01", "ce_owner_response_00", "ce_owner_buy_00", "{=CEBROTHEL1054}I would like to buy your establishment.", ConversationWithBrothelOwnerShowBuy, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_04", "ce_owner_response_00", "ce_owner_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_buy_00_r", "ce_owner_buy_00", "ce_owner_buy_response", "{=CEBROTHEL1056}I am selling this establishment for {AMOUNT} denars.", PriceWithBrothel, null);

            campaignGameStarter.AddPlayerLine("ce_owner_buy_yes", "ce_owner_buy_response", "ce_owner_business_complete", "{=CEBROTHEL1049}Alright, here you go.", null, ConversationBoughtBrothel, 100, ConversationHasEnoughMoneyForBrothel);
            campaignGameStarter.AddPlayerLine("ce_owner_buy_no", "ce_owner_buy_response", "ce_owner_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_business_complete_response", "ce_owner_business_complete", "close_window", "{=CEBROTHEL1057}A pleasure doing business. [ib:confident][rb:very_positive]", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_exit_response", "ce_owner_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Brothel Owner 01
            campaignGameStarter.AddDialogLine("ce_owner_talk_01", "start", "close_window", "{=CEBROTHEL1059}Let me prepare the establishment, it will be ready for you soon.", ConversationWithBrothelOwnerAfterSelling, null);

            // Dialogue With Assistant 00
            campaignGameStarter.AddDialogLine("ce_assistant_talk_00", "start", "ce_assistant_response_00", "{=CEBROTHEL1060}Hello boss, how can I help you today?[ib:confident][rb:very_positive]", ConversationWithBrothelAssistantBeforeSelling, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_01", "ce_assistant_response_00", "ce_assistant_sell_00", "{=CEBROTHEL1061}I would like to sell our establishment.", null, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_04", "ce_assistant_response_00", "ce_assistant_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_sell_00_r", "ce_assistant_sell_00", "ce_assistant_sell_response", "{=CEBROTHEL1062}We can sell this establishment for {AMOUNT} denars.", PriceWithBrothel, null);

            campaignGameStarter.AddPlayerLine("ce_assistant_sell_yes", "ce_assistant_sell_response", "ce_assistant_business_complete", "{=CEBROTHEL1018}That's a fair price I'd say.", null, ConversationSoldBrothel);
            campaignGameStarter.AddPlayerLine("ce_assistant_sell_no", "ce_assistant_sell_response", "ce_assistant_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_business_complete_r", "ce_assistant_business_complete", "close_window", "{=CEBROTHEL1063}It's been a pleasure working for you! [ib:confident][rb:very_positive]", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_exit_00_r", "ce_assistant_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Assistance 01
            campaignGameStarter.AddDialogLine("ce_assistant_talk_01", "start", "close_window", "{=CEBROTHEL1064}The new owner will arrive here shortly, I will just clean up things for now.", ConversationWithBrothelAssistantAfterSelling, null);
        }


        private bool RandomizeConversation(int divider)
        {
            return MBRandom.RandomFloatRanged(0, 100) < 100 / divider;
        }

        // Owner Conditions
        private bool ConversationWithBrothelAssistantAfterSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && !DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        private bool ConversationWithBrothelOwnerAfterSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        private bool ConversationWithBrothelAssistantBeforeSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        private bool ConversationWithBrothelOwnerBeforeSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        private bool ConversationWithBrothelOwnerShowBuy()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !Campaign.Current.IsMainHeroDisguised;
        }

        private void ConversationBoughtBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, brothelCost);
            BrothelInteraction(Settlement.CurrentSettlement, true);
        }

        private void ConversationSoldBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, brothelCost);
            BrothelInteraction(Settlement.CurrentSettlement, false);
        }

        private bool ConversationHasEnoughMoneyForBrothel(out TextObject text)
        {
            text = TextObject.Empty;

            if (Hero.MainHero.Gold >= brothelCost) return true;
            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        private bool PriceWithBrothel()
        {
            try
            {
                MBTextManager.SetTextVariable("AMOUNT", CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant"
                                                  ? new TextObject(GetPlayerBrothel(Settlement.CurrentSettlement).Capital.ToString())
                                                  : new TextObject(brothelCost.ToString()));
            }
            catch (Exception) { }

            return true;
        }

        // Prostitute Conditions
        private static readonly string[] prostituteStrings = {"prostitute_confident", "prostitute_confident", "prostitute_tired"};

        private bool ConversationWithProstitute()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_regular";
        }

        private bool ConversationWithProstituteIsOwner()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("prostitute") && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        private bool ConversationWithProstituteNotMetRequirements()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("prostitute") && Campaign.Current.IsMainHeroDisguised;
        }

        private bool ConversationWithConfidentProstitute()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_confident";
        }

        private bool ConversationWithTiredProstitute()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_tired";
        }

        private bool ConversationHasEnoughForService(out TextObject text)
        {
            text = TextObject.Empty;

            if (Hero.MainHero.Gold >= prostitutionCost) return true;

            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        private bool PriceWithProstitute()
        {
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(prostitutionCost.ToString()));

            return true;
        }

        private void ConversationProstituteConsequenceSex()
        {
            try
            {
                if (!DoesOwnBrothelInSettlement(Settlement.CurrentSettlement)) GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, prostitutionCost);

                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");

                        break;

                    case CultureCode.Vlandia:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");

                        break;

                    case CultureCode.Aserai:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");

                        break;

                    case CultureCode.Empire:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");

                        break;

                    case CultureCode.Battania:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_wodden_straw_a");

                        break;
                    case CultureCode.Khuzait:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_f");

                        break;
                    case CultureCode.Invalid:
                        break;
                    case CultureCode.Nord:
                        break;
                    case CultureCode.Darshi:
                        break;
                    case CultureCode.Vakken:
                        break;
                    case CultureCode.AnyOtherCulture:
                        break;
                    default:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");

                        break;
                }

                CESubModule.AgentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => { return agent.Character == CharacterObject.OneToOneConversationCharacter; });
                CESubModule.brothelState = CESubModule.BrothelState.Start;
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e);
            }
        }

        // Customer Conditions
        private static readonly string[] customerStrings = {"customer_confident", "customer_tired"};

        private static readonly string[] responses = {"{=CEBROTHEL1019}That's too much, no thanks.", "{=CEBROTHEL1049}Alright, here you go."};

        private static readonly string[] rageResponses = {"{=CEBROTHEL1065}Well perhaps you should, you sure look like a {?PLAYER.GENDER}whore{?}prostitute{\\?}!", "{=CEBROTHEL1066}My apologies, {?PLAYER.GENDER}milady{?}my lord{\\?}!"};

        private bool ConversationWithCustomerNotMetRequirements()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("customer") && (!Hero.MainHero.IsFemale || Campaign.Current.IsMainHeroDisguised);
        }

        private bool ConversationWithConfidentCustomer()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "customer_confident";
        }

        private bool ConversationWithTiredCustomer()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "customer_tired";
        }

        private void ConversationCustomerConsequenceSex()
        {
            try
            {
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, prostitutionCost);
                var ProstitutionSkill = CESkills.Prostitution;
                if (Hero.MainHero.GetSkillValue(ProstitutionSkill) < 100) Hero.MainHero.SetSkillValue(ProstitutionSkill, 100);
                CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(1, 10), Hero.MainHero, false, true, true);

                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");

                        break;

                    case CultureCode.Vlandia:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");

                        break;

                    case CultureCode.Aserai:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");

                        break;

                    case CultureCode.Empire:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");

                        break;

                    case CultureCode.Battania:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");

                        break;
                    case CultureCode.Khuzait:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_b");

                        break;
                    case CultureCode.Invalid:
                        break;
                    case CultureCode.Nord:
                        break;
                    case CultureCode.Darshi:
                        break;
                    case CultureCode.Vakken:
                        break;
                    case CultureCode.AnyOtherCulture:
                        break;
                    default:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");

                        break;
                }

                CESubModule.AgentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => agent.Character == CharacterObject.OneToOneConversationCharacter);
                CESubModule.brothelState = CESubModule.BrothelState.Start;
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e);
            }
        }


        private bool ConversationWithCustomerRandomResponse()
        {
            if (MBRandom.RandomInt(0, 100) > 20)
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(responses[0]));
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(responses[1]));
                ConversationCustomerConsequenceSex();
            }

            return true;
        }

        private bool ConversationWithCustomerRandomResponseRage()
        {
            MBTextManager.SetTextVariable("RESPONSE_STRING", MBRandom.RandomInt(0, 100) > 40
                                              ? new TextObject(rageResponses[0])
                                              : new TextObject(rageResponses[1]));

            return true;
        }

        // Session
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty) return;

            _Brothel.RemoveAllCharacters();
            _isBrothelInitialized = false;
        }

        public void DailyTick()
        {
            _orderedDrinkThisDayInSettlement = null;

            try
            {
                foreach (var brothel in from brothel in GetPlayerBrothels()
                                        let town = brothel.Settlement.Town
                                        where !town.IsRebeling
                                        where brothel.IsRunning
                                        select brothel)
                {
                    brothel.ChangeGold(MBRandom.RandomInt(50, 400));

                    if (brothel.Capital >= 0 || Hero.MainHero.Gold >= Math.Abs(brothel.Capital)) continue;

                    if (brothel.Settlement.OwnerClan == Clan.PlayerClan)
                    {
                        var textObject3 = new TextObject("{CEBROTHEL0998}The _Brothel of {SETTLEMENT} has gone bankrupted, and is no longer active.");
                        textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                        brothel.IsRunning = false;
                    }
                    else
                    {
                        var textObject3 = new TextObject("{CEBROTHEL0999}The _Brothel of {SETTLEMENT} has gone bankrupted, and has been requisitioned.");
                        textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                        BrothelInteraction(brothel.Settlement, false);
                    }
                }
            }
            catch (Exception) { }
        }

        public void WeeklyTick()
        {
            var ProstitutionSkill = CESkills.Prostitution;
            _hasBoughtTunToParty = false;

            if (Hero.MainHero.GetSkillValue(ProstitutionSkill) > 500) CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(-300, -200), Hero.MainHero, false, false);
            else if (Hero.MainHero.GetSkillValue(ProstitutionSkill) > 100) CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(-40, -10), Hero.MainHero, false, false);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
        }

        public static CEBrothel GetPlayerBrothel(Settlement settlement)
        {
            return _brothelList.FirstOrDefault(brothelData => brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero);
        }

        public static List<CEBrothel> GetPlayerBrothels()
        {
            try
            {
                return _brothelList.FindAll(brothelData => brothelData.Owner == Hero.MainHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogMessage("Failed to get player owned brothels.");

                return new List<CEBrothel>();
            }
        }

        public static bool DoesOwnBrothelInSettlement(Settlement settlement)
        {
            return _brothelList.Exists(brothelData => brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero);
        }

        public static void AddBrothelData(Settlement settlement)
        {
            _brothelList.Add(new CEBrothel(settlement));
        }

        public static bool ContainsBrothelData(Settlement settlement)
        {
            try
            {
                var data = _brothelList.FirstOrDefault(brothelData => brothelData.Settlement.StringId == settlement.StringId);
                if (data == null) AddBrothelData(settlement);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void BrothelInteraction(Settlement settlement, bool flagToPurchase)
        {
            try
            {
                if (ContainsBrothelData(settlement))
                    _brothelList.Where(brothel => brothel.Settlement.StringId == settlement.StringId).Select(brothel =>
                                                                                                             {
                                                                                                                 brothel.Owner = flagToPurchase
                                                                                                                     ? Hero.MainHero
                                                                                                                     : null;
                                                                                                                 brothel.Capital = brothel.InitialCapital;

                                                                                                                 return brothel;
                                                                                                             }).ToList();
            }
            catch (Exception) { }
        }

        public static List<CharacterObject> FetchBrothelPrisoners(Settlement settlement)
        {
            try
            {
                var testLocation = _brothelList.FirstOrDefault(brothel => brothel.Settlement.StringId == settlement.StringId);

                if (testLocation != null) return testLocation.CaptiveProstitutes;
            }
            catch (Exception) { }

            return null;
        }

        public static void RemovePrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null) return;

                if (!ContainsBrothelData(settlement)) return;

                _brothelList.Where(brothel => brothel.Settlement.StringId == settlement.StringId).Select(brothel =>
                                                                                                         {
                                                                                                             brothel.CaptiveProstitutes.Remove(prisoner);

                                                                                                             return brothel;
                                                                                                         }).ToList();
                MobileParty.MainParty.PrisonRoster.AddToCounts(prisoner, 1, prisoner.IsHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogMessage("Failed to RemovePrisoner");
            }
        }

        public static void AddPrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null) return;

                if (!ContainsBrothelData(settlement)) return;

                _brothelList.Where(brothel => brothel.Settlement.StringId == settlement.StringId).Select(brothel =>
                                                                                                         {
                                                                                                             brothel.CaptiveProstitutes.Add(prisoner);

                                                                                                             return brothel;
                                                                                                         }).ToList();
                MobileParty.MainParty.PrisonRoster.RemoveTroop(prisoner);
            }
            catch (Exception)
            {
                CECustomHandler.LogMessage("Failed to AddPrisoner");
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("SettlementsThatPlayerHasSpy", ref SettlementsThatPlayerHasSpy);
            dataStore.SyncData("_orderedDrinkThisDayInSettlement", ref _orderedDrinkThisDayInSettlement);
            dataStore.SyncData("_orderedDrinkThisVisit", ref _orderedDrinkThisVisit);
            dataStore.SyncData("_hasMetWithRansomBroker", ref _hasMetWithRansomBroker);
            dataStore.SyncData("_hasBoughtTunToParty", ref _hasBoughtTunToParty);
            dataStore.SyncData("_CEbrothelList", ref _brothelList);
        }

        public static void CleanList()
        {
            _brothelList = new List<CEBrothel>();
        }

        private static List<CEBrothel> _brothelList = new List<CEBrothel>();

        private List<Settlement> SettlementsThatPlayerHasSpy = new List<Settlement>();

        private const int prostitutionCost = 60;

        private const int brothelCost = 5000;

        private Settlement _orderedDrinkThisDayInSettlement;

        private bool _orderedDrinkThisVisit;

        private bool _hasMetWithRansomBroker;

        private bool _hasBoughtTunToParty;
    }
}