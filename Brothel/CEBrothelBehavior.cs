using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static Location _brothel = new Location("brothel", new TextObject("{=CEEVENTS1099}Brothel"), new TextObject("{=CEEVENTS1099}Brothel"), 30, true, false, "CanAlways", "CanAlways", "CanNever", "CanNever", new[] { "empire_house_c_tavern_a", "", "", "" }, null);

        public static bool _isBrothelInitialized;

        #region GameMenu
        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            if (CESettings.Instance == null) return;
            if (!CESettings.Instance.ProstitutionControl) return;

            // Option Added To Town
            campaignGameStarter.AddGameMenuOption("town", "town_brothel", "{=CEEVENTS1100}Go to the brothel district", CanGoToBrothelDistrictOnCondition, delegate { try { GameMenu.SwitchToMenu("town_brothel"); } catch (Exception) { GameMenu.SwitchToMenu("town"); } }, false, 1);

            campaignGameStarter.AddGameMenu("town_brothel", "{=CEEVENTS1098}You are in the brothel district", BrothelDistrictOnInit, GameOverlays.MenuOverlayType.SettlementWithBoth);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_visit", "{=CEEVENTS1101}Visit the brothel", VisitBrothelOnCondition, VisitBrothelOnConsequence, false, 0);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_prostitution", "{=!}{JOIN_STRING}", ProstitutionMenuJoinOnCondition, ProstitutionMenuJoinOnConsequence, false, 1);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_some_captives", "{=CEEVENTS1097}Sell some captives to the slaver", SellPrisonerOneStackOnCondition, delegate { ChooseRansomPrisoners(); }, false, 2);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_all_prisoners", "{=CEEVENTS1096}Sell all captives to the slaver ({RANSOM_AMOUNT}{GOLD_ICON})", SellPrisonersCondition, delegate { SellAllPrisoners(); }, false, 3);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_manage_prisoners", "{=CEEVENTS1175}Manage brothel captives", ManagePrisonerCondition, delegate { ManagePrisoners(); }, false, 4);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_back", "{=qWAmxyYz}Back to town center", BackOnCondition, delegate { GameMenu.SwitchToMenu("town"); }, true, 5);
        }

        // Ransom Functions
        private static int GetRansomValueOfAllPrisoners()
        {
            int num = 0;

            foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.PrisonRoster.GetTroopRoster()) num += Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(troopRosterElement.Character, Hero.MainHero) * troopRosterElement.Number;

            return num;
        }

        private static bool SellPrisonersCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count <= 0) return false;

            int ransomValueOfAllPrisoners = GetRansomValueOfAllPrisoners();
            MBTextManager.SetTextVariable("RANSOM_AMOUNT", ransomValueOfAllPrisoners);
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            return true;
        }

        private static bool SellPrisonerOneStackOnCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count <= 0) return false;
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            return true;
        }

        private static void SellAllPrisoners()
        {
            SellPrisonersAction.ApplyForAllPrisoners(MobileParty.MainParty, MobileParty.MainParty.PrisonRoster, Settlement.CurrentSettlement);
            GameMenu.SwitchToMenu("town_brothel");
        }

        // New Manage
        private static bool ManagePrisonerCondition(MenuCallbackArgs args)
        {
            if (Campaign.Current.IsMainHeroDisguised) return false;

            if (!DoesOwnBrothelInSettlement(Settlement.CurrentSettlement)) return false;

            args.optionLeaveType = GameMenuOption.LeaveType.Manage;

            return true;
        }

        private static void ManagePrisoners()
        {
            GameMenu.SwitchToMenu("town_brothel");
            ManageProstitutes();
        }

        /// <summary>
        /// Takes Example from OpenScreenAsManagePrisoners
        /// </summary>
        private static void ManageProstitutes()
        {
            PartyScreenLogic _partyScreenLogic = new PartyScreenLogic();

            try
            {
                // Reflection
                FieldInfo fi = PartyScreenManager.Instance.GetType().GetField("_currentMode", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(PartyScreenManager.Instance, PartyScreenMode.PrisonerManage);

                TroopRoster prisonRoster = TroopRoster.CreateDummyTroopRoster();
                List<CharacterObject> prisoners = FetchBrothelPrisoners(Hero.MainHero.CurrentSettlement);
                foreach (CharacterObject prisoner in prisoners)
                {
                    prisonRoster.AddToCounts(prisoner, 1, false);
                }

                int lefPartySizeLimit = 10 - prisonRoster.Count;
                TextObject textObject = new TextObject("{=CEBROTHEL0984}The brothel of {SETTLEMENT}", null);
                textObject.SetTextVariable("SETTLEMENT", Hero.MainHero.CurrentSettlement.Name);

                _partyScreenLogic.Initialize(TroopRoster.CreateDummyTroopRoster(), prisonRoster, MobileParty.MainParty, true, textObject, lefPartySizeLimit, new PartyPresentationDoneButtonDelegate(ManageBrothelDoneHandler), new TextObject("{=aadTnAEg}Manage Prisoners", null), false);

                _partyScreenLogic.InitializeTrade(PartyScreenLogic.TransferState.NotTransferable, PartyScreenLogic.TransferState.Transferable, PartyScreenLogic.TransferState.NotTransferable);

                _partyScreenLogic.SetTroopTransferableDelegate(new PartyScreenLogic.IsTroopTransferableDelegate(BrothelTroopTransferableDelegate));


                PartyState partyState = Game.Current.GameStateManager.CreateState<PartyState>();
                partyState.InitializeLogic(_partyScreenLogic);

                fi = PartyScreenManager.Instance.GetType().GetField("_partyScreenLogic", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(PartyScreenManager.Instance, _partyScreenLogic);

                Game.Current.GameStateManager.PushState(partyState, 0);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to launch ManageProstitutes : " + e);
            }
        }

        private static bool BrothelTroopTransferableDelegate(CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
        {
            switch (CESettings.Instance.BrothelOption.SelectedIndex)
            {
                case 0:
                    return true;
                case 2:
                    return !character.IsFemale;
                default:
                    return character.IsFemale;
            }
        }

        private static bool ManageBrothelDoneHandler(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, List<MobileParty> leftParties = null, List<MobileParty> rightParties = null)
        {
            SetBrothelPrisoners(Hero.MainHero.CurrentSettlement, leftPrisonRoster);
            return true;
        }


        // Ends Here

        private static void ChooseRansomPrisoners()
        {
            GameMenu.SwitchToMenu("town_brothel");
            PartyScreenManager.OpenScreenAsRansom();
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

            try
            {
                PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, Campaign.Current.GameMenuManager.PreviousLocation);
                Campaign.Current.GameMenuManager.SetNextMenu("town_brothel");
                Campaign.Current.GameMenuManager.NextLocation = null;
                Campaign.Current.GameMenuManager.PreviousLocation = null;
            }
            catch (Exception)
            {
                args.IsEnabled = false;
            }
            return true;
        }

        // Brothel Menu
        private static void BrothelDistrictOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            Settlement settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty.CurrentSettlement;

            try
            {
                // Location Complex need to add to to prevent crashing on overlay menu
                FieldInfo fi = LocationComplex.Current.GetType().GetField("_locations", BindingFlags.Instance | BindingFlags.NonPublic);
                Dictionary<string, Location> _locations = (Dictionary<string, Location>)fi.GetValue(LocationComplex.Current);

                if (_locations.ContainsKey("brothel"))
                {
                    _locations.Remove("brothel");
                }

                _brothel.SetOwnerComplex(settlement.LocationComplex);

                switch (settlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        _brothel.SetSceneName(0, "sturgia_house_a_interior_tavern");
                        break;
                    case CultureCode.Vlandia:
                        _brothel.SetSceneName(0, "vlandia_tavern_interior_a");
                        break;
                    case CultureCode.Aserai:
                        _brothel.SetSceneName(0, "arabian_house_new_c_interior_b_tavern");
                        break;
                    case CultureCode.Empire:
                        _brothel.SetSceneName(0, "empire_house_c_tavern_a");
                        break;
                    case CultureCode.Battania:
                        _brothel.SetSceneName(0, "battania_tavern_interior_b");
                        break;
                    case CultureCode.Khuzait:
                        _brothel.SetSceneName(0, "khuzait_tavern_a");
                        break;
                    case CultureCode.Nord:
                    case CultureCode.Darshi:
                    case CultureCode.Vakken:
                    case CultureCode.AnyOtherCulture:
                    case CultureCode.Invalid:
                    default:
                        _brothel.SetSceneName(0, "empire_house_c_tavern_a");
                        break;
                }
                List<CharacterObject> brothelPrisoners = FetchBrothelPrisoners(Settlement.CurrentSettlement);
                _brothel.RemoveAllCharacters();
                foreach (CharacterObject brothelPrisoner in brothelPrisoners)
                {
                    if (!brothelPrisoner.IsHero) continue;
                    _brothel.AddCharacter(CreateBrothelPrisoner(brothelPrisoner, Settlement.CurrentSettlement.Culture, LocationCharacter.CharacterRelations.Neutral));
                }

                _locations.Add("brothel", _brothel);
                if (fi != null) fi.SetValue(LocationComplex.Current, _locations);

                Campaign.Current.GameMenuManager.MenuLocations.Add(LocationComplex.Current.GetLocationWithId("brothel"));
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to load LocationComplex Brothel Statue (Corrupt Save)");
            }

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
            if (((TownEncounter)PlayerEncounter.LocationEncounter).IsAmbush)
            {
                GameMenu.ActivateGameMenu("menu_town_thugs_start");
                return;
            }

            try
            {
                Campaign.Current.GameMenuManager.NextLocation = LocationComplex.Current.GetLocationWithId("brothel");
                Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
                PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation);
                Campaign.Current.GameMenuManager.NextLocation = null;
                Campaign.Current.GameMenuManager.PreviousLocation = null;
            }
            catch (Exception)
            {
                GameMenu.SwitchToMenu("town_brothel");
            }
        }

        public static bool ProstitutionMenuJoinOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            MBTextManager.SetTextVariable("JOIN_STRING", DoesOwnBrothelInSettlement(Settlement.CurrentSettlement) ? "{=CEBROTHEL0978}Assist the prostitutes at your brothel" : "{=CEEVENTS1102}Become a prostitute at the brothel");
            if (!CEHelper.brothelFlagFemale && Hero.MainHero.IsFemale || !CEHelper.brothelFlagMale && !Hero.MainHero.IsFemale) return false;
            if (Campaign.Current.IsMainHeroDisguised) return false;
            return true;
        }

        public static void ProstitutionMenuJoinOnConsequence(MenuCallbackArgs args)
        {
            SkillObject ProstitueFlag = CESkills.IsProstitute;
            Hero.MainHero.SetSkillValue(ProstitueFlag, 1);
            SkillObject ProstitutionSkill = CESkills.Prostitution;

            if (Hero.MainHero.GetSkillValue(ProstitutionSkill) < 100) Hero.MainHero.SetSkillValue(ProstitutionSkill, 100);
            TextObject textObject = GameTexts.FindText("str_CE_join_prostitution");
            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            InformationManager.AddQuickInformation(textObject, 0, CharacterObject.PlayerCharacter, "event:/ui/notification/relation");

            PartyBase capturerParty = SettlementHelper.FindNearestSettlement(settlement => settlement.IsTown).Party;
            Hero prisonerCharacter = Hero.MainHero;
            prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
            prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
            while (PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter)) PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, -1, true);
            capturerParty.AddPrisoner(prisonerCharacter.CharacterObject, 1);
            if (prisonerCharacter == Hero.MainHero) PlayerCaptivity.StartCaptivity(capturerParty);
            string waitingMenu = CEEventLoader.CEWaitingList();
            GameMenu.ExitToLast();
            if (waitingMenu != null) GameMenu.ActivateGameMenu(waitingMenu);
        }
        #endregion

        #region Mission

        public void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            if (CampaignMission.Current.Location.StringId != "brothel" || _isBrothelInitialized) return;
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            AddPeopleToTownTavern(settlement, unusedUsablePointCount);
            _isBrothelInitialized = true;
        }

        public static CharacterObject HelperCreateFrom(CharacterObject character, bool traitsAndSkills) => CharacterObject.CreateFrom(character, traitsAndSkills);

        private static LocationCharacter CreateTavernkeeper(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject owner = MBObjectManager.Instance.CreateObject<CharacterObject>();

            if (DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
            {
                owner = HelperCreateFrom(culture.TavernWench, true);

                owner.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
                owner.IsFemale = true;
                owner.Name = new TextObject("{=CEEVENTS1050}Brothel Assistant");
                owner.StringId = "brothel_assistant";
            }
            else
            {
                owner = HelperCreateFrom(culture.Tavernkeeper, true);

                owner.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge);
                owner.IsFemale = true;
                owner.Name = new TextObject("{=CEEVENTS1066}Brothel Owner");
                owner.StringId = "brothel_owner";
            }

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)).Equipment(culture.FemaleDancer.AllEquipments.GetRandomElement()), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "spawnpoint_tavernkeeper", true, relation, "as_human_tavern_keeper", true);
        }

        private static LocationCharacter CreateRansomBroker(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            BasicCharacterObject owner = HelperCreateFrom(culture.RansomBroker, true);
            owner.Name = new TextObject("{=CEEVENTS1065}Slave Trader");
            owner.StringId = "brothel_slaver";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors, "npc_common", true, relation, null, true);
        }

        private static LocationCharacter CreateMusician(CultureObject culture, LocationCharacter.CharacterRelations relation) => new LocationCharacter(new AgentData(new SimpleAgentOrigin(culture.Musician)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "musician", true, relation, "as_human_musician", true, true);

        private static LocationCharacter CreateTownsManForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject townsman = HelperCreateFrom(culture.Townsman, true);
            townsman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townsman.StringId = CustomerStrings.GetRandomElement();

            string actionSetCode;

            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait") actionSetCode = "as_human_villager_in_aserai_tavern";
            else actionSetCode = "as_human_villager_in_tavern";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townsman)).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_common", true, relation, actionSetCode, true);
        }

        private static LocationCharacter CreateTavernWench(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject townswoman = HelperCreateFrom(culture.TavernWench, true);

            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townswoman.Name = new TextObject("{=CEEVENTS1093}Server");
            townswoman.StringId = "bar_maid";

            AgentData agentData = new AgentData(new SimpleAgentOrigin(townswoman)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge));

            return new LocationCharacter(agentData, SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "sp_tavern_wench", true, relation, "as_human_barmaid", true) { PrefabNamesForBones = { { agentData.AgentMonster.OffHandItemBoneIndex, "kitchen_pitcher_b_tavern" } } };
        }

        private static LocationCharacter CreateDancer(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject townswoman = HelperCreateFrom(culture.FemaleDancer, true);

            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townswoman.Name = new TextObject("{=CEEVENTS1095}Prostitute");
            townswoman.StringId = prostituteStrings.GetRandomElement();

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_dancer", true, relation, "as_human_female_dancer", true);
        }

        private static LocationCharacter CreateTownsWomanForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject townswoman = HelperCreateFrom(culture.FemaleDancer, true);

            townswoman.Age = MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge);
            townswoman.Name = new TextObject("{=CEEVENTS1095}Prostitute");
            townswoman.StringId = prostituteStrings.GetRandomElement();

            string actionSetCode;

            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait") actionSetCode = "as_human_villager_in_aserai_tavern";
            else actionSetCode = "as_human_villager_in_tavern";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman, -1, Banner.CreateRandomBanner())).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_common", true, relation, actionSetCode, true);
        }

        private static LocationCharacter CreateBrothelPrisoner(CharacterObject prisoner, CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            if (prisoner.Age < 21) prisoner.Age = 21;
            prisoner.HeroObject.StayingInSettlementOfNotable = Settlement.CurrentSettlement;

            string actionSetCode;
            if (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait") actionSetCode = "as_human_villager_in_aserai_tavern";
            else actionSetCode = "as_human_villager_in_tavern";

            Equipment RandomCivilian = culture.FemaleDancer.CivilianEquipments.GetRandomElementInefficiently();
            return new LocationCharacter(new AgentData(new PartyAgentOrigin(null, prisoner, -1, default, false)).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age((int)prisoner.Age).CivilianEquipment(true).Equipment(RandomCivilian), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_common", true, relation, actionSetCode, false);

        }

        private void AddPeopleToTownTavern(Settlement settlement, Dictionary<string, int> unusedUsablePointCount)
        {
            LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateTavernkeeper, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateTavernWench, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateMusician, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateRansomBroker, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            unusedUsablePointCount.TryGetValue("npc_dancer", out int dancers);
            if (dancers > 0) LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateDancer, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, dancers);

            unusedUsablePointCount.TryGetValue("npc_common", out int num);
            num -= 3;

            if (num <= 0) return;

            int num2 = (int)(num * 0.2f);
            if (num2 > 0) LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateTownsManForTavern, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num2);

            int num3 = (int)(num * 0.3f);

            int num4 = num - num2;

            List<CharacterObject> brothelPrisoners = FetchBrothelPrisoners(settlement);
            foreach (CharacterObject brothelPrisoner in brothelPrisoners)
            {
                if (brothelPrisoner.IsHero) num3--;
                else num4--;
            }

            if (num3 > 0)
            {
                LocationComplex.Current.GetLocationWithId("brothel").AddLocationCharacters(CreateTownsWomanForTavern, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, Math.Max(num4, num3));
            }
        }

        #endregion

        #region Dialogues

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

            // Maid Dialogue 00
            campaignGameStarter.AddDialogLine("ce_maid_talk_00", "start", "ce_maid_response_00", "{=CEBROTHEL1097}Hello {?PLAYER.GENDER}milady{?}my lord{\\?}, what can I get for you?", () => { return RandomizeConversation(2) && ConversationWithMaid(); }, null);

            campaignGameStarter.AddPlayerLine("ce_maid_response_00_00", "ce_maid_response_00", "ce_drink_menu_00", "{=CEBROTHEL1086}I am looking for something to drink. What do you have?", null, null);
            campaignGameStarter.AddPlayerLine("ce_maid_response_00_01", "ce_maid_response_00", "ce_specific_00", "{=CEBROTHEL1083}I'm looking for someone specific.", null, null);

            campaignGameStarter.AddPlayerLine("ce_maid_response_00_02", "ce_maid_response_00", "ce_maid_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            // Maid Dialogue 01
            campaignGameStarter.AddDialogLine("ce_maid_talk_01", "start", "ce_maid_response_01", "{=CEBROTHEL1078}Would you like something to drink, {?PLAYER.GENDER}milady{?}my lord{\\?}?", ConversationWithMaid, null);

            campaignGameStarter.AddPlayerLine("ce_maid_response_01_00", "ce_maid_response_01", "ce_drink_menu_00", "{=CEBROTHEL1086}Yes, what would you recommend?", null, null);
            campaignGameStarter.AddPlayerLine("ce_maid_response_01_01", "ce_maid_response_01", "ce_specific_00", "{=CEBROTHEL1083}I'm looking for someone specific.", null, null);
            campaignGameStarter.AddPlayerLine("ce_maid_response_01_02", "ce_maid_response_01", "ce_maid_exit_00", "{=CEBROTHEL1028}No thanks.", null, null);

            // Maid Dialogue 02
            campaignGameStarter.AddDialogLine("ce_maid_talk_02", "ce_repeat_maid", "ce_maid_response_00", "{=CEBROTHEL0981}Anything else {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", null, null);

            // Drink
            campaignGameStarter.AddDialogLine("ce_drink_menu_00_00", "ce_drink_menu_00", "ce_drink_menu_01", "{=CEBROTHEL1079}I recommend the mead, {?PLAYER.GENDER}milady{?}my lord{\\?}. Finished brewing just this morning. We also have ale and wine.", () => { return RandomizeConversation(3); }, null);
            campaignGameStarter.AddDialogLine("ce_drink_menu_00_01", "ce_drink_menu_00", "ce_drink_menu_01", "{=CEBROTHEL1080}I recommend the wine, {?PLAYER.GENDER}milady{?}my lord{\\?}. These last few bottles've been quite popular among the other patrons. We also have ale and mead.", () => { return RandomizeConversation(3); }, null);
            campaignGameStarter.AddDialogLine("ce_drink_menu_00_02", "ce_drink_menu_00", "ce_drink_menu_01", "{=CEBROTHEL1089}Care for a mug of fresh ale, {?PLAYER.GENDER}milady{?}my lord{\\?}? We also have mead and wine.", null, null);

            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_00", "ce_drink_menu_01", "ce_maid_business_drink", "{=CEBROTHEL1087}I will have the mead.", () => { return !ConversationWithMaidIsOwner(); }, null);
            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_01", "ce_drink_menu_01", "ce_maid_business_drink", "{=CEBROTHEL1088}I will have the wine.", () => { return !ConversationWithMaidIsOwner(); }, null);
            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_02", "ce_drink_menu_01", "ce_maid_business_drink", "{=CEBROTHEL1090}I will have the ale.", () => { return !ConversationWithMaidIsOwner(); }, null);

            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_00", "ce_drink_menu_01", "ce_maid_business_complete_owner", "{=CEBROTHEL1087}I will have the mead.", () => { return ConversationWithMaidIsOwner(); }, ConversationBoughtDrink);
            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_01", "ce_drink_menu_01", "ce_maid_business_complete_owner", "{=CEBROTHEL1088}I will have the wine.", () => { return ConversationWithMaidIsOwner(); }, ConversationBoughtDrink);
            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_02", "ce_drink_menu_01", "ce_maid_business_complete_owner", "{=CEBROTHEL1090}I will have the ale.", () => { return ConversationWithMaidIsOwner(); }, ConversationBoughtDrink);

            campaignGameStarter.AddPlayerLine("ce_drink_menu_01_03", "ce_drink_menu_01", "ce_repeat_maid", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            // Specific
            campaignGameStarter.AddDialogLine("ce_specific_00_00", "ce_specific_00", "ce_repeat_maid", "{=CEBROTHEL1084}Looking for someone specific? I'm sure your assistant will be happy to direct you, {?PLAYER.GENDER}milady{?}my lord{\\?}.", ConversationWithMaidIsOwner, null);
            campaignGameStarter.AddDialogLine("ce_specific_00_01", "ce_specific_00", "ce_repeat_maid", "{=CEBROTHEL1085}Looking for someone specific? I'm sure the owner will be happy to direct you, {?PLAYER.GENDER}milady{?}my lord{\\?}.", null, null);

            // Response Drink 
            campaignGameStarter.AddDialogLine("ce_maid_business_drink_response", "ce_maid_business_drink", "ce_maid_business_drink_00", "{=CEBROTHEL1091}That will be {AMOUNT} denars. [ib: confident][rb: very_positive]", PriceWithMaid, null);

            campaignGameStarter.AddPlayerLine("ce_maid_business_drink_00_yes", "ce_maid_business_drink_00", "ce_maid_business_complete", "{=CEBROTHEL1049}Alright, here you go.", null, ConversationBoughtDrink, 100, ConversationHasEnoughForDrinks);
            campaignGameStarter.AddPlayerLine("ce_maid_business_drink_00_no", "ce_maid_business_drink_00", "ce_maid_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_maid_business_complete_response_owner", "ce_maid_business_complete_owner", "close_window", "{=CEBROTHEL0980}Of course {?PLAYER.GENDER}milady{?}my lord{\\?}.", null, null);
            campaignGameStarter.AddDialogLine("ce_maid_business_complete_response", "ce_maid_business_complete", "close_window", "{=CEBROTHEL1057}A pleasure doing business. [ib:confident][rb:very_positive]", null, null);
            campaignGameStarter.AddDialogLine("ce_maid_exit_response", "ce_maid_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Owner 00
            campaignGameStarter.AddDialogLine("ce_owner_talk_00", "start", "ce_owner_response_00", "{=CEBROTHEL1053}Oh, a valued customer, how can I help you today?[ib:confident][rb:very_positive]", ConversationWithBrothelOwnerBeforeSelling, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_01", "ce_owner_response_00", "ce_owner_buy_00", "{=CEBROTHEL1054}I would like to buy your establishment.", ConversationWithBrothelOwnerShowBuy, null);
            campaignGameStarter.AddPlayerLine("ce_op_response_02", "ce_owner_response_00", "ce_owner_party_00", "{=CEBROTHEL1068}I would like to purchase some services for my party.", ConversationWithBrothelOwnerShowPartyBuy, null);
            campaignGameStarter.AddPlayerLine("ce_op_response_04", "ce_owner_response_00", "ce_owner_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_buy_00_r", "ce_owner_buy_00", "ce_owner_buy_response", "{=CEBROTHEL1056}I am selling this establishment for {AMOUNT} denars.", PriceWithBrothel, null);

            campaignGameStarter.AddPlayerLine("ce_owner_buy_yes", "ce_owner_buy_response", "ce_owner_business_complete", "{=CEBROTHEL1049}Alright, here you go.", null, ConversationBoughtBrothel, 100, ConversationHasEnoughMoneyForBrothel);
            campaignGameStarter.AddPlayerLine("ce_owner_buy_no", "ce_owner_buy_response", "ce_owner_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_party_00_r", "ce_owner_party_00", "ce_owner_party_response", "{=CEBROTHEL1072}I can bring some ladies but that will be {AMOUNT} denars.", PriceWithParty, null);

            campaignGameStarter.AddPlayerLine("ce_party_buy_yes", "ce_owner_party_response", "ce_owner_business_complete", "{=CEBROTHEL1049}Alright, here you go.", null, ConversationBoughtParty, 100, ConversationHasEnoughForPartyService);
            campaignGameStarter.AddPlayerLine("ce_party_buy_no", "ce_owner_party_response", "ce_owner_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_business_complete_response", "ce_owner_business_complete", "close_window", "{=CEBROTHEL1057}A pleasure doing business. [ib:confident][rb:very_positive]", null, null);
            campaignGameStarter.AddDialogLine("ce_owner_exit_response", "ce_owner_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Brothel Owner 01
            campaignGameStarter.AddDialogLine("ce_owner_talk_01", "start", "close_window", "{=CEBROTHEL1059}Let me prepare the establishment, it will be ready for you soon.", ConversationWithBrothelOwnerAfterSelling, null);


            // Dialogue With Assistant 00
            campaignGameStarter.AddDialogLine("ce_assistant_talk_00", "start", "ce_assistant_response_00", "{=CEBROTHEL1060}Hello boss, how can I help you today?[ib:confident][rb:very_positive]", ConversationWithBrothelAssistantBeforeSelling, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_01", "ce_assistant_response_00", "ce_assistant_sell_00", "{=CEBROTHEL1061}I would like to sell our establishment.", null, null);
            campaignGameStarter.AddPlayerLine("ce_ap_response_02", "ce_assistant_response_00", "ce_assistant_manage_00", "{=CEBROTHEL0982}I would like to manage our establishment.", null, null);
            campaignGameStarter.AddPlayerLine("ce_ap_response_03", "ce_assistant_response_00", "ce_assistant_party_00", "{=CEBROTHEL1069}Send some ladies to boost the morale of my party.", ConversationWithBrothelAssistantShowPartyBuy, null);
            campaignGameStarter.AddPlayerLine("ce_ap_response_04", "ce_assistant_response_00", "ce_assistant_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_sell_00_r", "ce_assistant_sell_00", "ce_assistant_sell_response", "{=CEBROTHEL1062}We can sell this establishment for {AMOUNT} denars.", PriceWithBrothel, null);

            campaignGameStarter.AddDialogLine("ce_assistant_party_00_n", "ce_assistant_party_00", "ce_assistant_response_00", "{=CEBROTHEL1071}Sorry, {?PLAYER.GENDER}milady{?}my lord{\\?} everyone are currently busy.", () => _hasBoughtProstituteToParty, null);

            campaignGameStarter.AddDialogLine("ce_assistant_choice_00_r", "ce_assistant_party_00", "ce_assistant_choice_00", "{=CEBROTHEL1073}Who would you like to send first?", () => !_hasBoughtProstituteToParty, CheckInBrothelCaptives);

            campaignGameStarter.AddRepeatablePlayerLine("ce_assistant_choice_01_r", "ce_assistant_choice_00", "ce_assistant_choice_01", "{=CEBROTHEL1075}Send {HERO.LINK}.", ConditionalSendBrothelCaptive, SendBrothelCaptive);

            campaignGameStarter.AddPlayerLine("ce_assistant_choice_02_r", "ce_assistant_choice_00", "ce_assistant_choice_01", "{=CEBROTHEL1074}Send {NAME} the regular.", ConditionalRandomName, null);

            campaignGameStarter.AddDialogLine("ce_assistant_party_00_r", "ce_assistant_choice_01", "ce_assistant_manage_00_response", "{=CEBROTHEL1070}Yes, {?PLAYER.GENDER}milady{?}my lord{\\?} I will send her first.", null, ConversationBoughtParty);

            campaignGameStarter.AddDialogLine("ce_assistant_manage_00_r", "ce_assistant_manage_00", "ce_assistant_manage_00_response", "{=CEBROTHEL0980}Of course {?PLAYER.GENDER}milady{?}my lord{\\?}.", null, ManageProstitutes);
            campaignGameStarter.AddDialogLine("ce_assistant_manage_00_response_r", "ce_assistant_manage_00_response", "ce_assistant_response_00", "{=CEBROTHEL0981}Anything else {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", null, null);

            campaignGameStarter.AddPlayerLine("ce_assistant_sell_yes", "ce_assistant_sell_response", "ce_assistant_business_complete", "{=CEBROTHEL1018}That's a fair price I'd say.", null, ConversationSoldBrothel);
            campaignGameStarter.AddPlayerLine("ce_assistant_sell_no", "ce_assistant_sell_response", "ce_assistant_manage_00_response", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_business_complete_r", "ce_assistant_business_complete", "close_window", "{=CEBROTHEL1063}It's been a pleasure working for you! [ib:confident][rb:very_positive]", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_exit_00_r", "ce_assistant_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Assistance 01
            campaignGameStarter.AddDialogLine("ce_assistant_talk_01", "start", "close_window", "{=CEBROTHEL1064}The new owner will arrive here shortly, I will just clean up things for now.", ConversationWithBrothelAssistantAfterSelling, null);

            // Owner Dialogue Captives 
            // Positive Intro
            campaignGameStarter.AddDialogLine("captive_requirements_owner_positive_00", "start", "ccaptive_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", () => RandomizeConversation(2) && ConversationWithPositiveCaptive(), null);
            campaignGameStarter.AddDialogLine("captive_requirements_owner_positive_00", "lord_introduction", "ccaptive_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", () => RandomizeConversation(2) && ConversationWithPositiveCaptive(), null);

            campaignGameStarter.AddDialogLine("captive_requirements_owner_positive_01", "start", "ccaptive_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", () => ConversationWithPositiveCaptive(), null);
            campaignGameStarter.AddDialogLine("captive_requirements_owner_positive_01", "lord_introduction", "ccaptive_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", () => ConversationWithPositiveCaptive(), null);

            // Negative Intro
            campaignGameStarter.AddDialogLine("captive_requirements_owner_00", "start", "ccaptive_owner_00", "{=CEBROTHEL1067}This is no place for me, {?PLAYER.GENDER}milady{?}my lord{\\?}! What do you want?[ib:closed][rb:negative]", () => RandomizeConversation(2) && ConversationWithCaptive(), null);
            campaignGameStarter.AddDialogLine("captive_requirements_owner_00", "lord_introduction", "ccaptive_owner_00", "{=CEBROTHEL1067}This is no place for me, {?PLAYER.GENDER}milady{?}my lord{\\?}! What do you want?[ib:closed][rb:negative]", () => RandomizeConversation(2) && ConversationWithCaptive(), null);

            campaignGameStarter.AddDialogLine("captive_requirements_owner_01", "start", "ccaptive_owner_00", "{=CEBROTHEL1067}This is no place for me, {?PLAYER.GENDER}milady{?}my lord{\\?}! What do you want?[ib:closed][rb:negative]", () => ConversationWithCaptive(), null);
            campaignGameStarter.AddDialogLine("captive_requirements_owner_01", "lord_introduction", "ccaptive_owner_00", "{=CEBROTHEL1067}This is no place for me, {?PLAYER.GENDER}milady{?}my lord{\\?}! What do you want?[ib:closed][rb:negative]", () => ConversationWithCaptive(), null);

            // Player Choices 
            campaignGameStarter.AddPlayerLine("ccaptive_owner_00_yes", "ccaptive_owner_00", "ccaptive_service_00_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null);
            //campaignGameStarter.AddPlayerLine("ccaptive_owner_00_free", "ccaptive_owner_00", "ccaptive_service_00_free_response", "{=CEEVENTS1177}You are free to go.", null, null);
            campaignGameStarter.AddPlayerLine("ccaptive_owner_00_nevermind", "ccaptive_owner_00", "ccaptive_service_00_nevermind_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null);


            // Positive
            campaignGameStarter.AddDialogLine("ccaptive_service_00_yes_response_id_positive", "ccaptive_service_00_yes_response", "close_window", "{=CEBROTHEL1020}Follow me sweetie. [ib:normal][rb:positive]", () => ConversationWithPositiveCaptive(), ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("ccaptive_service_00_nevermind_response_id_positive", "ccaptive_service_00_nevermind_response", "close_window", "{=CEBROTHEL1037}See ya around.[ib:confident][rb:very_positive]", () => ConversationWithPositiveCaptive(), null);

            // Negative
            campaignGameStarter.AddDialogLine("ccaptive_service_00_yes_response_id", "ccaptive_service_00_yes_response", "close_window", "{=CEBROTHEL1043}Right this way...[ib:closed][rb:unsure]", null, ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("ccaptive_service_00_nevermind_response_id_positive", "ccaptive_service_00_nevermind_response", "close_window", "{=CEBROTHEL1037}See ya around.[ib:confident][rb:very_positive]", () => ConversationWithPositiveCaptive(), null);

            campaignGameStarter.AddDialogLine("ccaptive_service_00_nevermind_response_id", "ccaptive_service_00_nevermind_response", "close_window", "{=CEBROTHEL1044}Thank goodness...[ib:closed][rb:unsure]", null, null);

            campaignGameStarter.AddDialogLine("ccaptive_service_00_free_response_id", "ccaptive_service_00_free_response", "close_window", "{=CEBROTHEL1077}Thank you {?PLAYER.GENDER}milady{?}my lord{\\?}!", null, null);

        }


        private bool RandomizeConversation(int divider) => MBRandom.RandomFloatRanged(0, 100) < 100 / divider;

        // Owner Conditions
        private bool ConversationWithBrothelAssistantAfterSelling() => CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && !DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);

        private bool ConversationWithBrothelOwnerAfterSelling() => CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);

        private bool ConversationWithBrothelAssistantBeforeSelling() => CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);

        private bool ConversationWithBrothelOwnerBeforeSelling() => CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);

        private bool ConversationWithBrothelOwnerShowBuy() => CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !Campaign.Current.IsMainHeroDisguised;

        private bool ConversationWithBrothelOwnerShowPartyBuy()
        {
            int numberOfMen = PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !Campaign.Current.IsMainHeroDisguised && !_hasBoughtProstituteToParty && numberOfMen > 1;
        }

        private bool ConversationWithBrothelAssistantShowPartyBuy()
        {
            int numberOfMen = PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            return !Campaign.Current.IsMainHeroDisguised && numberOfMen > 1;
        }

        private void ConversationBoughtBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, brothelCost);
            BrothelInteraction(Settlement.CurrentSettlement, true);
        }

        private void ConversationSoldBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, GetPlayerBrothel(Settlement.CurrentSettlement).Capital);
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
        private static readonly string[] prostituteStrings = { "prostitute_confident", "prostitute_confident", "prostitute_tired" };

        private bool ConversationWithCaptive() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner && ContainsPrisoner(Hero.OneToOneConversationHero.CharacterObject);

        private bool ConversationWithPositiveCaptive() => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner && ContainsPrisoner(Hero.OneToOneConversationHero.CharacterObject) && (Hero.OneToOneConversationHero.GetSkillValue(CESkills.Slavery) > 50 || Hero.OneToOneConversationHero.GetSkillValue(CESkills.Prostitution) > 70);

        private bool ConversationWithProstitute() => CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_regular";

        private bool ConversationWithMaidIsOwner() => CharacterObject.OneToOneConversationCharacter.StringId == "bar_maid" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        private bool ConversationWithMaid() => CharacterObject.OneToOneConversationCharacter.StringId == "bar_maid";

        private bool ConversationWithProstituteIsOwner() => CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("prostitute") && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);

        private bool ConversationWithProstituteNotMetRequirements() => CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("prostitute") && Campaign.Current.IsMainHeroDisguised;

        private bool ConversationWithConfidentProstitute() => CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_confident";

        private bool ConversationWithTiredProstitute() => CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_tired";

        private bool ConversationHasEnoughForService(out TextObject text)
        {
            text = TextObject.Empty;

            if (Hero.MainHero.Gold >= prostitutionCost) return true;
            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        private bool ConversationHasEnoughForDrinks(out TextObject text)
        {
            text = TextObject.Empty;

            if (Hero.MainHero.Gold >= drinkCost) return true;
            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        private bool PriceWithParty()
        {
            int numberOfMen = PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            int totalCost = numberOfMen * prostitutionCostPerParty;
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(totalCost.ToString()));

            return true;
        }

        private bool ConversationHasEnoughForPartyService(out TextObject text)
        {
            text = TextObject.Empty;
            int numberOfMen = PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            int totalCost = numberOfMen * prostitutionCostPerParty;

            if (Hero.MainHero.Gold >= totalCost) return true;
            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        private bool PriceWithProstitute()
        {
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(prostitutionCost.ToString()));
            return true;
        }

        private bool PriceWithMaid()
        {
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(drinkCost.ToString()));
            return true;
        }

        private bool ConditionalRandomName()
        {
            MBTextManager.SetTextVariable("NAME", Settlement.CurrentSettlement.Culture.FemaleNameList.GetRandomElement());
            return true;
        }

        // conversation_town_or_village_player_ask_location_of_hero_2_on_condition
        private bool ConditionalSendBrothelCaptive()
        {
            if (ConversationSentence.SelectedRepeatObject is CharacterObject characterObject)
            {
                StringHelpers.SetCharacterProperties("HERO", characterObject, null, ConversationSentence.SelectedRepeatLine, true);
                return true;
            }
            return false;
        }
        private void CheckInBrothelCaptives()
        {
            List<CharacterObject> brothelPrisoners = FetchBrothelPrisoners(Settlement.CurrentSettlement);
            ConversationSentence.ObjectsToRepeatOver = brothelPrisoners;
        }

        private void SendBrothelCaptive()
        {
            CharacterObject captive = ((CharacterObject)ConversationSentence.LastSelectedRepeatObject);

            if (captive.HeroObject.GetSkillValue(CESkills.Slavery) < 50 || captive.HeroObject.GetSkillValue(CESkills.Prostitution) < 50)
            {
                new Dynamics().RelationsModifier(captive.HeroObject, MBRandom.RandomInt(-10, -1), Hero.MainHero, false, true);
            }
        }

        private void ConversationBoughtDrink()
        {
            if (!DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
            {
                GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, drinkCost);
            }

            Hero.MainHero.HitPoints += 10;

        }

        private void ConversationBoughtParty()
        {
            int numberOfMen = PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });

            if (!DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
            {
                int totalCost = numberOfMen * prostitutionCostPerParty;
                GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, totalCost);
            }

            float ratio = numberOfMen / PartyBase.MainParty.NumberOfAllMembers;

            PartyBase.MainParty.MobileParty.RecentEventsMorale += ratio * 60f;

            TextObject textObject = GameTexts.FindText("str_CE_morale_level");
            textObject.SetTextVariable("PARTY", PartyBase.MainParty.Name);

            textObject.SetTextVariable("POSITIVE", 1);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));

            _hasBoughtProstituteToParty = true;
        }

        private void ConversationProstituteConsequenceSex()
        {
            try
            {
                if (!DoesOwnBrothelInSettlement(Settlement.CurrentSettlement)) GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, prostitutionCost);

                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");
                        break;
                    case CultureCode.Vlandia:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");
                        break;
                    case CultureCode.Aserai:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");
                        break;
                    case CultureCode.Empire:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");
                        break;
                    case CultureCode.Battania:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_wodden_straw_a");
                        break;
                    case CultureCode.Khuzait:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_f");
                        break;
                    case CultureCode.Invalid:
                    case CultureCode.Nord:
                    case CultureCode.Darshi:
                    case CultureCode.Vakken:
                    case CultureCode.AnyOtherCulture:
                    default:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");
                        break;
                }

                CEPersistence.agentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => agent.Character == CharacterObject.OneToOneConversationCharacter);
                CEPersistence.brothelState = CEPersistence.BrothelState.Start;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e);
            }
        }

        // Customer Conditions
        private static readonly string[] CustomerStrings = { "customer_confident", "customer_tired" };

        private static readonly string[] Responses = { "{=CEBROTHEL1019}That's too much, no thanks.", "{=CEBROTHEL1049}Alright, here you go." };

        private static readonly string[] RageResponses = { "{=CEBROTHEL1065}Well perhaps you should, you sure look like a {?PLAYER.GENDER}whore{?}prostitute{\\?}!", "{=CEBROTHEL1066}My apologies, {?PLAYER.GENDER}milady{?}my lord{\\?}!" };

        private bool ConversationWithCustomerNotMetRequirements() => CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("customer") && (!Hero.MainHero.IsFemale || Campaign.Current.IsMainHeroDisguised);

        private bool ConversationWithConfidentCustomer() => CharacterObject.OneToOneConversationCharacter.StringId == "customer_confident";

        private bool ConversationWithTiredCustomer() => CharacterObject.OneToOneConversationCharacter.StringId == "customer_tired";

        private void ConversationCustomerConsequenceSex()
        {
            try
            {
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, prostitutionCost);
                SkillObject prostitutionSkill = CESkills.Prostitution;
                if (Hero.MainHero.GetSkillValue(prostitutionSkill) < 100) Hero.MainHero.SetSkillValue(prostitutionSkill, 100);
                new Dynamics().VictimProstitutionModifier(MBRandom.RandomInt(1, 10), Hero.MainHero, false, true, true);

                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");
                        break;
                    case CultureCode.Vlandia:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");
                        break;
                    case CultureCode.Aserai:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");
                        break;
                    case CultureCode.Empire:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");
                        break;
                    case CultureCode.Battania:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");
                        break;
                    case CultureCode.Khuzait:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_b");
                        break;
                    case CultureCode.Invalid:
                    case CultureCode.Nord:
                    case CultureCode.Darshi:
                    case CultureCode.Vakken:
                    case CultureCode.AnyOtherCulture:
                    default:
                        CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");
                        break;
                }

                CEPersistence.agentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => { return agent.Character == CharacterObject.OneToOneConversationCharacter; });
                CEPersistence.brothelState = CEPersistence.BrothelState.Start;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e);
            }
        }


        private bool ConversationWithCustomerRandomResponse()
        {
            if (MBRandom.RandomInt(0, 100) > 20)
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(Responses[0]));
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(Responses[1]));
                ConversationCustomerConsequenceSex();
            }

            return true;
        }

        private bool ConversationWithCustomerRandomResponseRage()
        {
            MBTextManager.SetTextVariable("RESPONSE_STRING", MBRandom.RandomInt(0, 100) > 40
                                              ? new TextObject(RageResponses[0])
                                              : new TextObject(RageResponses[1]));

            return true;
        }
        #endregion

        #region Session
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter) => AddDialogs(campaignGameStarter);

        public void OnMissionEnded(IMission mission) => CleanUpBrothel();

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty) return;
            if (LocationComplex.Current == null || LocationComplex.Current.GetLocationWithId("brothel") == null) return;
            if (LocationComplex.Current.GetLocationWithId("brothel").Name == null)
            {
                try
                {
                    // Location Complex need to add to to prevent crashing // TODO do at campaign start on every location
                    FieldInfo fi = LocationComplex.Current.GetType().GetField("_locations", BindingFlags.Instance | BindingFlags.NonPublic);
                    Dictionary<string, Location> _locations = (Dictionary<string, Location>)fi.GetValue(LocationComplex.Current);

                    if (_locations.ContainsKey("brothel")) _locations.Remove("brothel");
                    //else LocationComplex.Current.AddPassage(LocationComplex.Current.GetLocationWithId("center"), _brothel);

                    _brothel.SetOwnerComplex(settlement.LocationComplex);

                    switch (settlement.Culture.GetCultureCode())
                    {
                        case CultureCode.Sturgia:
                            _brothel.SetSceneName(0, "sturgia_house_a_interior_tavern");
                            break;
                        case CultureCode.Vlandia:
                            _brothel.SetSceneName(0, "vlandia_tavern_interior_a");
                            break;
                        case CultureCode.Aserai:
                            _brothel.SetSceneName(0, "arabian_house_new_c_interior_b_tavern");
                            break;
                        case CultureCode.Empire:
                            _brothel.SetSceneName(0, "empire_house_c_tavern_a");
                            break;
                        case CultureCode.Battania:
                            _brothel.SetSceneName(0, "battania_tavern_interior_b");
                            break;
                        case CultureCode.Khuzait:
                            _brothel.SetSceneName(0, "khuzait_tavern_a");
                            break;
                        case CultureCode.Nord:
                        case CultureCode.Darshi:
                        case CultureCode.Vakken:
                        case CultureCode.AnyOtherCulture:
                        case CultureCode.Invalid:
                        default:
                            _brothel.SetSceneName(0, "empire_house_c_tavern_a");
                            break;
                    }
                    List<CharacterObject> brothelPrisoners = FetchBrothelPrisoners(Settlement.CurrentSettlement);
                    _brothel.RemoveAllCharacters();
                    foreach (CharacterObject brothelPrisoner in brothelPrisoners)
                    {
                        if (!brothelPrisoner.IsHero) continue;
                        _brothel.AddCharacter(CreateBrothelPrisoner(brothelPrisoner, Settlement.CurrentSettlement.Culture, LocationCharacter.CharacterRelations.Neutral));
                    }

                    _locations.Add("brothel", _brothel);
                    if (fi != null) fi.SetValue(LocationComplex.Current, _locations);

                    Campaign.Current.GameMenuManager.MenuLocations.Add(LocationComplex.Current.GetLocationWithId("brothel"));
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed to load LocationComplex Brothel Statue (Corrupt Save)" + e);
                }
            }
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty) return;
            CleanUpBrothel();
        }

        private void CleanUpBrothel()
        {
            if (_isBrothelInitialized)
            {
                LocationComplex.Current.GetLocationWithId("brothel").RemoveAllCharacters();
                _isBrothelInitialized = false;
            }
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newSettlementOwner, Hero oldSettlementOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            try
            {
                if (!settlement.IsTown) return;
                if (!DoesOwnBrothelInSettlement(settlement)) return;
                if (!Hero.MainHero.MapFaction.IsAtWarWith(newSettlementOwner.MapFaction)) return;

                if (Hero.MainHero.GetPerkValue(DefaultPerks.Trade.RapidDevelopment))
                {
                    GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, MathF.Round(DefaultPerks.Trade.RapidDevelopment.PrimaryBonus), false);
                }
                TextObject textObject3 = new TextObject("{CEBROTHEL0983}The brothel of {SETTLEMENT} has been captured by the enemy, and has been requisitioned.");
                textObject3.SetTextVariable("SETTLEMENT", settlement.Name);
                InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Yellow));
                BrothelInteraction(settlement, false, true, capturerHero);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("OnSettlementOwnerChanged : " + e);
            }
        }

        private void OnWarDeclared(IFaction faction1, IFaction faction2)
        {
            try
            {
                IFaction faction3 = (faction1 == Hero.MainHero.MapFaction) ? faction1 : ((faction2 == Hero.MainHero.MapFaction) ? faction2 : null);
                if (faction3 != null)
                {
                    IFaction faction4 = (faction3 != faction1) ? faction1 : faction2;
                    int count = _brothelList.Count;
                    for (int i = 0; i < count; i++)
                    {
                        CEBrothel brothel = _brothelList[i];
                        if (brothel != null && brothel.Settlement.MapFaction == faction4)
                        {
                            TextObject textObject3 = new TextObject("{CEBROTHEL0983}The brothel of {SETTLEMENT} has been captured by the enemy, and has been requisitioned.");
                            textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                            InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Yellow));
                            BrothelInteraction(brothel.Settlement, false, true, brothel.Settlement.OwnerClan.Leader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("OnWarDeclared : " + e);
            }
        }

        public void DailyTick()
        {
            _orderedDrinkThisDayInSettlement = null;

            try
            {
                foreach (CEBrothel brothel in from brothel in GetPlayerBrothels()
                                              let town = brothel.Settlement.Town
                                              where !town.InRebelliousState
                                              where brothel.IsRunning
                                              select brothel)
                {
                    int gold = MBRandom.RandomInt(-50, 200);
                    gold += brothel.CaptiveProstitutes.Count * 10;
                    gold += brothel.CaptiveProstitutes.FindAll((CharacterObject prisoner) => { return prisoner.IsHero; }).Count * 190;
                    brothel.ChangeGold(gold);

                    if (brothel.Capital >= 0 || Hero.MainHero.Gold >= Math.Abs(brothel.Capital)) continue;

                    if (brothel.Settlement.OwnerClan == Clan.PlayerClan)
                    {
                        TextObject textObject3 = new TextObject("{CEBROTHEL0998}The brothel of {SETTLEMENT} has gone bankrupted, and is no longer open.");
                        textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                        brothel.IsRunning = false;
                    }
                    else
                    {
                        TextObject textObject3 = new TextObject("{CEBROTHEL0999}The brothel of {SETTLEMENT} has gone bankrupted, and has been requisitioned.");
                        textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                        BrothelInteraction(brothel.Settlement, false);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed on BrothelDailyTick1: " + e);
            }

            try
            {
                // Escape
                for (int i = 0; i < _brothelList.Count; i++)
                {
                    for (int y = 0; y < _brothelList[i].CaptiveProstitutes.Count; y++)
                    {
                        if (_brothelList[i].CaptiveProstitutes[y].IsHero)
                        {
                            new Dynamics().RenownModifier(MBRandom.RandomInt(-3, -1), _brothelList[i].CaptiveProstitutes[y].HeroObject, false);

                            // Workaround for Missing Bug By Bannerlord Tweaks.
                            _brothelList[i].CaptiveProstitutes[y].HeroObject.CaptivityStartTime = CampaignTime.Now;

                            int numEscapeChance = CESettings.Instance.PrisonerHeroEscapeChanceSettlement;
                            if (numEscapeChance == -1) numEscapeChance = 25;

                            if (MBRandom.RandomInt(100) < numEscapeChance)
                            {
                                MobileParty.MainParty.PrisonRoster.AddToCounts(_brothelList[i].CaptiveProstitutes[y], 1, true);
                                EndCaptivityAction.ApplyByEscape(_brothelList[i].CaptiveProstitutes[y].HeroObject);
                                _brothelList[i].CaptiveProstitutes.RemoveAt(y);
                            }
                        }
                        else
                        {
                            int numEscapeChance = CESettings.Instance.PrisonerNonHeroEscapeChanceSettlement;
                            if (numEscapeChance == -1) numEscapeChance = 25;

                            if (MBRandom.RandomInt(100) < numEscapeChance)
                            {
                                _brothelList[i].CaptiveProstitutes.RemoveAt(y);
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed on BrothelDailyTick2: " + e);
            }
        }

        public void WeeklyTick()
        {
            SkillObject prostitutionSkill = CESkills.Prostitution;
            _hasBoughtTunToParty = false;
            _hasBoughtProstituteToParty = false;

            if (Hero.MainHero.GetSkillValue(prostitutionSkill) > 500) new Dynamics().VictimProstitutionModifier(MBRandom.RandomInt(-300, -200), Hero.MainHero, false, false);
            else if (Hero.MainHero.GetSkillValue(prostitutionSkill) > 100) new Dynamics().VictimProstitutionModifier(MBRandom.RandomInt(-40, -10), Hero.MainHero, false, false);

            try
            {
                // Renown Modifier
                for (int i = 0; i < _brothelList.Count; i++)
                {
                    for (int y = 0; y < _brothelList[i].CaptiveProstitutes.Count; y++)
                    {
                        if (_brothelList[i].CaptiveProstitutes[y].IsHero)
                        {
                            if (_brothelList[i].CaptiveProstitutes[y].HeroObject.GetSkillValue(prostitutionSkill) < 50)
                                new Dynamics().RenownModifier(MBRandom.RandomInt(-20, -5), _brothelList[i].CaptiveProstitutes[y].HeroObject, false);

                            new Dynamics().VictimProstitutionModifier(MBRandom.RandomInt(10, 20), _brothelList[i].CaptiveProstitutes[y].HeroObject);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed on BrothelWeeklyTick: " + e);
            }
        }

        private void OnHeroDeath(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            try
            {
                if (victim.IsPrisoner)
                {
                    for (int i = 0; i < _brothelList.Count; i++)
                    {
                        for (int y = 0; y < _brothelList[i].CaptiveProstitutes.Count; y++)
                        {
                            if (_brothelList[i].CaptiveProstitutes[y].IsHero)
                            {
                                if (victim == _brothelList[i].CaptiveProstitutes[y].HeroObject)
                                {
                                    _brothelList[i].CaptiveProstitutes[y].HeroObject.IsNoble = true;
                                    _brothelList[i].CaptiveProstitutes.RemoveAt(y);
                                    return;
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed at OnHeroDeath CEBrothel : " + e);
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(OnHeroDeath));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, WeeklyTick);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, OnMissionEnded);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
        }

        public static CEBrothel GetPlayerBrothel(Settlement settlement) => _brothelList.FirstOrDefault(brothelData => brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero);

        public static List<CEBrothel> GetPlayerBrothels()
        {
            try
            {
                return _brothelList.FindAll(brothelData => brothelData.Owner == Hero.MainHero);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to get player owned brothels.");

                return new List<CEBrothel>();
            }
        }

        public static bool DoesOwnBrothelInSettlement(Settlement settlement)
        {
            try
            {
                return _brothelList.Exists(brothelData => brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero);
            }
            catch (Exception)
            {
                _brothelList = new List<CEBrothel>();
                return false;
            }
        }

        public static void AddBrothelData(Settlement settlement) => _brothelList.Add(new CEBrothel(settlement));

        public static bool ContainsBrothelData(Settlement settlement)
        {
            try
            {
                int i = _brothelList.FindIndex(brothelData => brothelData.Settlement.StringId == settlement.StringId);
                if (i == -1) AddBrothelData(settlement);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void BrothelInteraction(Settlement settlement, bool flagToPurchase, bool releasePrisoners = false, Hero heroReleased = null)
        {
            try
            {
                if (ContainsBrothelData(settlement))
                {
                    int i = _brothelList.FindIndex(brothelData => brothelData.Settlement.StringId == settlement.StringId);
                    _brothelList[i].Owner = flagToPurchase ? Hero.MainHero : null;
                    _brothelList[i].Capital = _brothelList[i].InitialCapital;
                    foreach (CharacterObject captive in _brothelList[i].CaptiveProstitutes)
                    {
                        if (!releasePrisoners)
                        {
                            if (captive.IsHero)
                            {
                                captive.HeroObject.IsNoble = true;
                            }
                            MobileParty.MainParty.PrisonRoster.AddToCounts(captive, 1, captive.IsHero);
                        }
                        else
                        {
                            if (captive.IsHero)
                            {
                                if (heroReleased != null)
                                {
                                    if (captive.HeroObject.Clan.IsAtWarWith(heroReleased.Clan))
                                    {
                                        heroReleased.PartyBelongedTo.PrisonRoster.AddToCounts(captive, 1, true);
                                        continue;
                                    }
                                }
                                captive.HeroObject.IsNoble = true;
                                MobileParty.MainParty.PrisonRoster.AddToCounts(captive, 1, true);
                                EndCaptivityAction.ApplyByReleasing(captive.HeroObject, heroReleased);
                            }
                        }
                    }
                    _brothelList[i].CaptiveProstitutes = new List<CharacterObject>();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed on Brothel Interaction: " + e);
            }
        }

        public static List<CharacterObject> FetchBrothelPrisoners(Settlement settlement)
        {
            try
            {
                CEBrothel testLocation = _brothelList.FirstOrDefault(brothel => { return brothel.Settlement.StringId == settlement.StringId; });
                if (testLocation != null) return testLocation.CaptiveProstitutes;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed on FetchBrothelPrisoners: " + e);
            }

            return new List<CharacterObject>();
        }

        public static void SetBrothelPrisoners(Settlement settlement, TroopRoster prisoners)
        {
            try
            {
                if (settlement == null) return;

                if (!ContainsBrothelData(settlement)) return;

                int index = _brothelList.FindIndex(brothel => brothel.Settlement.StringId == settlement.StringId);

                List<string> captivesFreed = new List<string>();

                foreach (CharacterObject captive in _brothelList[index].CaptiveProstitutes)
                {
                    if (captive.IsHero)
                    {
                        captive.HeroObject.IsNoble = true;
                        captivesFreed.Add(captive.HeroObject.StringId);
                    }
                }

                _brothelList[index].CaptiveProstitutes.Clear();

                foreach (TroopRosterElement troopElement in prisoners.GetTroopRoster())
                {
                    if (troopElement.Character.IsHero)
                    {
                        troopElement.Character.HeroObject.IsNoble = false;
                        if (!captivesFreed.Contains(troopElement.Character.HeroObject.StringId))
                        {
                            if (troopElement.Character.HeroObject.GetSkillValue(CESkills.Slavery) < 50)
                            {
                                new Dynamics().RelationsModifier(troopElement.Character.HeroObject, MBRandom.RandomInt(-10, -1), Hero.MainHero, false, true);
                            }

                            if (CESettings.Instance.EventProstituteGear)
                            {
                                CharacterObject femaleDancer = HelperCreateFrom(settlement.Culture.FemaleDancer, true);

                                if (CESettings.Instance != null && CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(troopElement.Character.HeroObject, troopElement.Character.HeroObject.BattleEquipment, troopElement.Character.HeroObject.CivilianEquipment);

                                Equipment randomCivilian = femaleDancer.CivilianEquipments.GetRandomElementInefficiently();
                                Equipment randomBattle = new Equipment(false);
                                randomBattle.FillFrom(randomCivilian, false);

                                EquipmentHelper.AssignHeroEquipmentFromEquipment(troopElement.Character.HeroObject, randomCivilian);
                                EquipmentHelper.AssignHeroEquipmentFromEquipment(troopElement.Character.HeroObject, randomBattle);
                            }
                        }
                    }

                    for (int i = 0; i < troopElement.Number; i++)
                    {
                        _brothelList[index].CaptiveProstitutes.Add(troopElement.Character);
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to SetBrothelPrisoners");
            }
        }

        public static void RemoveBrothelPrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null) return;
                if (!ContainsBrothelData(settlement)) return;

                int index = _brothelList.FindIndex(brothel => brothel.Settlement.StringId == settlement.StringId);
                _brothelList[index].CaptiveProstitutes.Remove(prisoner);

                if (prisoner.IsHero)
                {
                    prisoner.HeroObject.IsNoble = true;
                }

                MobileParty.MainParty.PrisonRoster.AddToCounts(prisoner, 1, prisoner.IsHero);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to RemoveBrothelPrisoner");
            }
        }

        public static void AddBrothelPrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null) return;

                if (!ContainsBrothelData(settlement)) return;

                int index = _brothelList.FindIndex(brothel => brothel.Settlement.StringId == settlement.StringId);

                if (prisoner.IsHero)
                {
                    prisoner.HeroObject.IsNoble = false;
                    if (CESettings.Instance.EventProstituteGear)
                    {
                        CharacterObject femaleDancer = HelperCreateFrom(settlement.Culture.FemaleDancer, true);

                        if (CESettings.Instance != null && CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(prisoner.HeroObject, prisoner.HeroObject.BattleEquipment, prisoner.HeroObject.CivilianEquipment);

                        Equipment randomCivilian = femaleDancer.CivilianEquipments.GetRandomElementInefficiently();
                        Equipment randomBattle = new Equipment(false);
                        randomBattle.FillFrom(randomCivilian, false);

                        EquipmentHelper.AssignHeroEquipmentFromEquipment(prisoner.HeroObject, randomCivilian);
                        EquipmentHelper.AssignHeroEquipmentFromEquipment(prisoner.HeroObject, randomBattle);
                    }
                }

                _brothelList[index].CaptiveProstitutes.Add(prisoner);
                MobileParty.MainParty.PrisonRoster.AddToCounts(prisoner, -1);
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to AddBrothelPrisoner");
            }
        }

        public static bool ContainsPrisoner(CharacterObject prisoner)
        {

            if (prisoner == null) return false;
            if (Settlement.CurrentSettlement == null) return false;
            if (!ContainsBrothelData(Settlement.CurrentSettlement)) return false;

            int index = _brothelList.FindIndex(brothel => brothel.Settlement.StringId == Settlement.CurrentSettlement.StringId);
            return _brothelList[index].CaptiveProstitutes.Exists((captive) => { return captive.Name == prisoner.Name; });
        }

        public static bool IsInBrothel(CharacterObject prisoner)
        {
            if (prisoner == null) return false;
            return _brothelList.Exists(brothel => brothel.CaptiveProstitutes.Exists((captive) => { return captive.Name == prisoner.Name; }));
        }

        #endregion

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("SettlementsThatPlayerHasSpy", ref SettlementsThatPlayerHasSpy);
            dataStore.SyncData("_orderedDrinkThisDayInSettlement", ref _orderedDrinkThisDayInSettlement);
            dataStore.SyncData("_orderedDrinkThisVisit", ref _orderedDrinkThisVisit);
            dataStore.SyncData("_hasMetWithRansomBroker", ref _hasMetWithRansomBroker);
            dataStore.SyncData("_hasBoughtTunToParty", ref _hasBoughtTunToParty);
            dataStore.SyncData("_hasBoughtProstituteToParty", ref _hasBoughtProstituteToParty);
            dataStore.SyncData("_CEbrothelList", ref _brothelList);
        }

        public static void CleanList()
        {
            foreach (CEBrothel brothel in _brothelList)
            {
                foreach (CharacterObject captive in brothel.CaptiveProstitutes)
                {
                    MobileParty.MainParty.PrisonRoster.AddToCounts(captive, 1, captive.IsHero);
                }
                Hero.MainHero.ChangeHeroGold(brothelCost);
            }
            _brothelList = new List<CEBrothel>();
        }

        private static List<CEBrothel> _brothelList = new List<CEBrothel>();

        private List<Settlement> SettlementsThatPlayerHasSpy = new List<Settlement>();

        private const int prostitutionCost = 60;

        private const int drinkCost = 30;

        private const int prostitutionCostPerParty = 40;

        private const int brothelCost = 5000;

        private Settlement _orderedDrinkThisDayInSettlement;

        private bool _orderedDrinkThisVisit;

        private bool _hasMetWithRansomBroker;

        private bool _hasBoughtTunToParty;

        private bool _hasBoughtProstituteToParty;
    }
}