using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Models;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelBehaviour : CampaignBehaviorBase
    {
        private static readonly Location brothel = new Location("brothel", new TextObject("{=CEEVENTS1099}Brothel"), new TextObject("{=CEEVENTS1099}Brothel"), 30, true, false, "CanIfSettlementAccessModelLetsPlayer", "CanAlways", "CanAlways", "CanIfGrownUpMaleOrHero", new string[] { "empire_house_c_tavern_a", "", "", "" }, null);

        private bool _isBrothelInitialized = false;

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            if (CESettings.Instance.ProstitutionControl)
            {
                // Option Added To Town
                campaignGameStarter.AddGameMenuOption("town", "town_brothel", "{=CEEVENTS1100}Go to the brothel district", new GameMenuOption.OnConditionDelegate(CanGoToBrothelDistrictOnCondition), delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("town_brothel");
                }, false, 1, false);

                campaignGameStarter.AddGameMenu("town_brothel", "{=CEEVENTS1098}You are in the brothel district", new OnInitDelegate(BrothelDistrictOnInit), GameOverlays.MenuOverlayType.SettlementWithCharacters, GameMenu.MenuFlags.none, null);

                campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_visit", "{=CEEVENTS1101}Visit the brothel",
                   new GameMenuOption.OnConditionDelegate(VisitBrothelOnCondition),
                   new GameMenuOption.OnConsequenceDelegate(VisitBrothelOnConsequence), false, 0, false);

                campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_prostitution", "{=CEEVENTS1102}Become a prostitute at the brothel",
                    new GameMenuOption.OnConditionDelegate(ProstitutionMenuJoinOnCondition),
                    new GameMenuOption.OnConsequenceDelegate(ProstitutionMenuJoinOnConsequence), false, 1, false);

                campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_some_captives", "{=CEEVENTS1097}Sell some captives to the brothel", new GameMenuOption.OnConditionDelegate(SellPrisonerOneStackOnCondition), delegate (MenuCallbackArgs x)
                {
                    ChooseRansomPrisoners();
                }, false, 2, false);

                campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_all_prisoners", "{=CEEVENTS1096}Sell all captives to the brothel ({RANSOM_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(SellPrisonersCondition), delegate (MenuCallbackArgs x)
                {
                    SellAllPrisoners();
                }, false, 3, false);

                campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_back", "{=qWAmxyYz}Back to town center", new GameMenuOption.OnConditionDelegate(BackOnCondition), delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("town");
                }, true, 4, false);
            }
        }

        // Ransom Functions
        private static int GetRansomValueOfAllPrisoners()
        {
            int num = 0;
            foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.PrisonRoster)
            {
                num += troopRosterElement.Character.PrisonerRansomValue(Hero.MainHero) * troopRosterElement.Number;
            }
            return num;
        }
        private static bool SellPrisonersCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count > 0)
            {
                int ransomValueOfAllPrisoners = GetRansomValueOfAllPrisoners();
                MBTextManager.SetTextVariable("RANSOM_AMOUNT", ransomValueOfAllPrisoners, false);
                args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                return true;
            }
            return false;
        }
        private static void SellAllPrisoners()
        {
            SellPrisonersAction.ApplyForAllPrisoners(MobileParty.MainParty, MobileParty.MainParty.PrisonRoster, Settlement.CurrentSettlement, true);
            GameMenu.SwitchToMenu("town_brothel");
        }
        private static void ChooseRansomPrisoners()
        {
            GameMenu.SwitchToMenu("town_brothel");
            PartyScreenManager.OpenScreenAsRansom();
        }
        private static bool SellPrisonerOneStackOnCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count > 0)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
                return true;
            }
            return false;
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
            if (Campaign.Current.GameMenuManager.NextLocation != null && GameStateManager.Current.ActiveState is MapState)
            {
                PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, Campaign.Current.GameMenuManager.PreviousLocation, null, null);
                Campaign.Current.GameMenuManager.SetNextMenu("town_brothel");
                Campaign.Current.GameMenuManager.NextLocation = null;
                Campaign.Current.GameMenuManager.PreviousLocation = null;
                return true;
            }
            return false;
        }

        // Brothel Menu
        private static void BrothelDistrictOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            Settlement settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty.CurrentSettlement;
            brothel.SetOwnerComplex(settlement.LocationComplex);
            switch (settlement.Culture.GetCultureCode())
            {
                case CultureCode.Sturgia:
                    brothel.SetSceneName(0, "sturgia_house_a_interior_tavern");
                    break;

                case CultureCode.Vlandia:
                    brothel.SetSceneName(0, "vlandia_tavern_interior_a");
                    break;

                case CultureCode.Aserai:
                    brothel.SetSceneName(0, "arabian_house_new_c_interior_b_tavern");
                    break;

                case CultureCode.Empire:
                    brothel.SetSceneName(0, "empire_house_c_tavern_a");
                    break;

                case CultureCode.Battania:
                    brothel.SetSceneName(0, "battania_tavern_interior_a");
                    break;

                case CultureCode.Khuzait:
                    brothel.SetSceneName(0, "khuzait_tavern_a");
                    break;

                default:
                    brothel.SetSceneName(0, "empire_house_c_tavern_a");
                    break;
            }
            Campaign.Current.GameMenuManager.MenuLocations.Add(brothel);
            if (CheckAndOpenNextLocation(args))
            {
                return;
            }
            args.MenuTitle = new TextObject("{=CEEVENTS1099}Brothel", null);
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
            if ((PlayerEncounter.LocationEncounter as TownEncounter).IsAmbush)
            {
                GameMenu.ActivateGameMenu("menu_town_thugs_start");
                return;
            }

            Campaign.Current.GameMenuManager.NextLocation = brothel;
            Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, null, null, null);
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;
        }
        public static bool ProstitutionMenuJoinOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            if (!CEHelper.brothelFlagFemale && Hero.MainHero.IsFemale || !CEHelper.brothelFlagMale && !Hero.MainHero.IsFemale)
            {
                return false;
            }
            if (Campaign.Current.IsMainHeroDisguised)
            {
                return false;
            }
            return true;
        }
        public static void ProstitutionMenuJoinOnConsequence(MenuCallbackArgs args)
        {
            SkillObject ProstitueFlag = CESkills.IsProstitute;
            Hero.MainHero.SetSkillValue(ProstitueFlag, 1);
            SkillObject ProstitutionSkill = CESkills.Prostitution;

            if (Hero.MainHero.GetSkillValue(ProstitutionSkill) < 100)
            {
                Hero.MainHero.SetSkillValue(ProstitutionSkill, 100);
            }
            TextObject textObject = GameTexts.FindText("str_CE_join_prostitution", null);
            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            InformationManager.AddQuickInformation(textObject, 0, CharacterObject.PlayerCharacter, "event:/ui/notification/relation");

            CEPlayerCaptivityModel.captureOverride = true;
            PartyBase capturerParty = SettlementHelper.FindNearestSettlement((settlement) => { return settlement.IsTown; }).Party;
            Hero prisonerCharacter = Hero.MainHero;
            prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
            prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
            while (PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter))
            {
                PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, -1, true);
            }
            capturerParty.AddPrisoner(prisonerCharacter.CharacterObject, 1, 0);
            if (prisonerCharacter == Hero.MainHero)
            {
                PlayerCaptivity.StartCaptivity(capturerParty);
            }
            string test = CEEventLoader.CEWaitingList();
            GameMenu.ExitToLast();
            if (test != null)
            {
                GameMenu.ActivateGameMenu(test);
            }
        }


        // Brothel Mission
        public void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            if (CampaignMission.Current.Location == brothel && !_isBrothelInitialized)
            {
                LocationComplex.Current.AddPassage(settlement.LocationComplex.GetLocationWithId("center"), brothel);
                AddPeopleToTownTavern(settlement, unusedUsablePointCount);
                _isBrothelInitialized = true;
            }
        }

        private static LocationCharacter CreateTavernkeeper(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {

            CharacterObject owner = MBObjectManager.Instance.CreateObject<CharacterObject>();
            if (DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
            {
                CharacterObject templateToCopy = CharacterObject.CreateFrom(culture.TavernWench);
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
                owner.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList<Equipment>());
                owner.Name = new TextObject("{=CEEVENTS1050}Brothel Assistant");
                owner.StringId = "brothel_assistant";
            }
            else
            {
                CharacterObject templateToCopy2 = CharacterObject.CreateFrom(culture.Tavernkeeper);
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
                owner.InitializeEquipmentsOnLoad(templateToCopy2.AllEquipments.ToList<Equipment>());
                owner.Name = new TextObject("{=CEEVENTS1066}Brothel Owner");
                owner.StringId = "brothel_owner";
            }


            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner, -1, null, default)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)).Equipment(culture.FemaleDancer.AllEquipments.GetRandomElement()), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "spawnpoint_tavernkeeper", true, relation, "as_human_tavern_keeper", true, false, null, false, false, true);
        }
        private static LocationCharacter CreateRansomBroker(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            BasicCharacterObject owner = CharacterObject.CreateFrom(culture.RansomBroker);
            owner.Name = new TextObject("{=CEEVENTS1065}Slave Trader");
            owner.StringId = "brothel_slaver";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner, -1, null, default)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors), "npc_common", true, relation, null, true, false, null, false, false, true);
        }
        private static LocationCharacter CreateMusician(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(culture.Musician, -1, null, default)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "musician", true, relation, "as_human_musician", true, true, null, false, false, true);
        }
        private static LocationCharacter CreateTownsManForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject templateToCopy = CharacterObject.CreateFrom(culture.Townsman);
            CharacterObject townsman = MBObjectManager.Instance.CreateObject<CharacterObject>();
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
            townsman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList<Equipment>());

            string actionSetCode;
            if (culture != null && (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait"))
            {
                actionSetCode = "as_human_villager_in_aserai_tavern";
            }
            else
            {
                actionSetCode = "as_human_villager_in_tavern";
            }
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townsman, -1, null, default)).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }
        private static LocationCharacter CreateTavernWench(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {

            CharacterObject templateToCopy = CharacterObject.CreateFrom(culture.TavernWench);
            CharacterObject townswoman = MBObjectManager.Instance.CreateObject<CharacterObject>();
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
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList<Equipment>());


            AgentData agentData = new AgentData(new SimpleAgentOrigin(townswoman, -1, null, default)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge));

            return new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "sp_tavern_wench", true, relation, "as_human_barmaid", true, false, null, false, false, true)
            {
                PrefabNamesForBones =
                {
                    {
                        agentData.AgentMonster.OffHandItemBoneIndex,
                        "kitchen_pitcher_b_tavern"
                    }
                }
            };
        }
        private static LocationCharacter CreateDancer(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject templateToCopy = CharacterObject.CreateFrom(culture.FemaleDancer);
            CharacterObject townswoman = MBObjectManager.Instance.CreateObject<CharacterObject>();
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
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList<Equipment>());

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman, -1, null, default)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_dancer", true, relation, "as_human_female_dancer", true, false, null, false, false, true);
        }
        private static LocationCharacter CreateTownsWomanForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            CharacterObject templateToCopy = CharacterObject.CreateFrom(culture.FemaleDancer);
            CharacterObject townswoman = MBObjectManager.Instance.CreateObject<CharacterObject>();
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
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList<Equipment>());

            string actionSetCode;
            if (culture != null && (culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait"))
            {
                actionSetCode = "as_human_villager_in_aserai_tavern";
            }
            else
            {
                actionSetCode = "as_human_villager_in_tavern";
            }

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman, -1, Banner.CreateRandomBanner(), default)).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors), "npc_common", true, relation, actionSetCode, true, false, null, false, false, true);
        }

        private void AddPeopleToTownTavern(Settlement settlement, Dictionary<string, int> unusedUsablePointCount)
        {
            unusedUsablePointCount.TryGetValue("npc_common", out int num);
            num -= 3;

            brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernkeeper), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMusician), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateRansomBroker), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            unusedUsablePointCount.TryGetValue("npc_dancer", out int dancers);
            if (dancers > 0)
            {
                brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDancer), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, dancers);
            }

            if (num > 0)
            {
                int num5 = (int)(num * 0.05f);
                if (num5 > 0)
                {
                    brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTavernWench), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num5);
                }
                int num2 = (int)(num * 0.1f);
                if (num2 > 0)
                {
                    brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsManForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num2);
                }
                int num3 = (int)(num * 0.3f);
                if (num3 > 0)
                {
                    brothel.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTownsWomanForTavern), settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num3);
                }
            }
        }

        protected void AddDialogs(CampaignGameStarter campaignGameStarter)
        {

            // Owner Dialogue Confident Prostitute
            campaignGameStarter.AddDialogLine("prostitute_requirements_owner", "start", "cprostitute_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(() => ConversationWithProstituteIsOwner() && ConversationWithConfidentProstitute()), null, 100, null);

            campaignGameStarter.AddPlayerLine("cprostitute_owner_00_yes", "cprostitute_owner_00", "prostitute_service_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null, 100, null, null);

            campaignGameStarter.AddPlayerLine("cprostitute_owner_00_nevermind", "cprostitute_owner_00", "close_window", "{=CEBROTHEL1002}Continue as you were.", null, null, 100, null, null);


            // Owner Dialogue Tired Prostitute
            campaignGameStarter.AddDialogLine("prostitute_requirements_owner", "start", "tprostitute_owner_00", "{=CEBROTHEL1006}Hello {?PLAYER.GENDER}milady{?}my lord{\\?}, I think I need a break.", new ConversationSentence.OnConditionDelegate(() => ConversationWithProstituteIsOwner() && ConversationWithTiredProstitute()), null, 100, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_yes", "tprostitute_owner_00", "tprostitute_service_01_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null, 100, null, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_no", "tprostitute_owner_00", "tprostitute_service_01_no_response", "{=CEBROTHEL1004}No, there is plenty of customers waiting, you must continue working.", null, null, 100, null, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_break", "tprostitute_owner_00", "tprostitute_owner_00_break_response", "{=CEBROTHEL1003}Go take a break.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("tprostitute_owner_00_break_r", "tprostitute_owner_00_break_response", "close_window", "{=CEBROTHEL1005}Thank you {?PLAYER.GENDER}milady{?}my lord{\\?}.", null, null, 100, null);

            // Requirements not met
            campaignGameStarter.AddDialogLine("prostitute_requirements_not_met", "start", "close_window", "{=CEBROTHEL1009}Sorry {?PLAYER.GENDER}milady{?}my lord{\\?}, I am currently very busy.", new ConversationSentence.OnConditionDelegate(ConversationWithProstituteNotMetRequirements), null, 100, null);

            campaignGameStarter.AddDialogLine("customer_requirements_not_met", "start", "customer_00", "{=CEBROTHEL1010}What do you want? Cannot you see that I am trying to enjoy the whores here? [ib:normal][rb:unsure]", new ConversationSentence.OnConditionDelegate(() => { return ConversationWithCustomerNotMetRequirements() && ConversationWithConfidentCustomer(); }), null, 100, null);

            campaignGameStarter.AddPlayerLine("customer_00_nevermind", "customer_00", "prostitute_service_no_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null, 100, null, null);

            // Confident Customer 00
            campaignGameStarter.AddDialogLine("ccustomer_00_start", "start", "ccustomer_00", "{=CEBROTHEL1014}Well hello there you {?PLAYER.GENDER}fine whore{?}stud{\\?}, would you like {AMOUNT} denars for your services? [ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(() => { return RandomizeConversation(2) && PriceWithProstitute() && ConversationWithConfidentCustomer(); }), null, 100, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_service", "ccustomer_00", "close_window", "{=CEBROTHEL1015}Yes, my lord I can do that.", null, new ConversationSentence.OnConsequenceDelegate(ConversationCustomerConsequenceSex), 100, null, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_rage", "ccustomer_00", "ccustomer_00_rage_reply", "{=CEBROTHEL1016}Excuse me, I don't work here!", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("ccustomer_00_rage_reply_r", "ccustomer_00_rage_reply", "close_window", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationWithCustomerRandomResponseRage), null, 100, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_nevermind", "ccustomer_00", "close_window", "{=CEBROTHEL1017}Sorry sir, I have to leave.", null, null, 100, null, null);

            // Tried Customer 01
            campaignGameStarter.AddDialogLine("tcustomer_00_start", "start", "tcustomer_00", "{=CEBROTHEL1012}Yes? [ib:normal][rb:unsure]", new ConversationSentence.OnConditionDelegate(() => ConversationWithTiredCustomer() || ConversationWithConfidentCustomer()), null, 100, null);

            campaignGameStarter.AddPlayerLine("tcustomer_00_service", "tcustomer_00", "tcustomer_00_talk_service", "{=CEBROTHEL1013}Would you like my services for {AMOUNT} denars?", new ConversationSentence.OnConditionDelegate(() => { return PriceWithProstitute() && !ConversationWithCustomerNotMetRequirements(); }), null, 100, null, null);

            campaignGameStarter.AddPlayerLine("tcustomer_00_nevermind", "tcustomer_00", "close_window", "{=CEBROTHEL1011}Uh, nevermind.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("tcustomer_00_service_r", "tcustomer_00_talk_service", "close_window", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationWithCustomerRandomResponse), null, 100, null);



            // Confident Prostitute Extra Replies
            campaignGameStarter.AddPlayerLine("prostitute_service_yes", "prostitute_service", "prostitute_service_yes_response", "{=CEBROTHEL1018}That's a fair price I'd say.", null, null, 100, new ConversationSentence.OnClickableConditionDelegate(ConversationHasEnoughForService), null);
            campaignGameStarter.AddPlayerLine("prostitute_service_no", "prostitute_service", "prostitute_service_no_response", "{=CEBROTHEL1019}That's too much, no thanks.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("prostitute_service_yes_response_id", "prostitute_service_yes_response", "close_window", "{=CEBROTHEL1020}Follow me sweetie. [ib:normal][rb:positive]", null, new ConversationSentence.OnConsequenceDelegate(ConversationProstituteConsequenceSex), 100, null);

            campaignGameStarter.AddDialogLine("prostitute_service_no_response_id", "prostitute_service_no_response", "close_window", "{=CEBROTHEL1021}Stop wasting my time then.[ib:aggressive][rb:unsure]", null, null, 100, null);

            // Dialogue with Confidient Prostitute 00
            campaignGameStarter.AddDialogLine("cprostitute_talk_00", "start", "cprostitute_talk_00_response", "{=CEBROTHEL1022}Hey {?PLAYER.GENDER}beautiful{?}handsome{\\?} want to have some fun? [ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(() => { return RandomizeConversation(3) && ConversationWithConfidentProstitute(); }), null, 100, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_00_service_r", "cprostitute_talk_00_response", "cprostitute_talk_00_service", "{=CEBROTHEL1023}Yeah, I could go for a bit of refreshment.", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_00_nevermind_r", "cprostitute_talk_00_response", "prostitute_service_no_response", "{=CEBROTHEL1024}No, I am fine.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_00_service_ar", "cprostitute_talk_00_service", "prostitute_service", "{=CEBROTHEL1025}Sounds good, that'll be {AMOUNT} denars. [ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(PriceWithProstitute), null, 100, null);

            // Dialogue with Confidient Prostitute 01
            campaignGameStarter.AddDialogLine("cprostitute_talk_01", "start", "cprostitute_talk_01_response", "{=CEBROTHEL1026}Hey {?PLAYER.GENDER}cutie{?}handsome{\\?} you look like you need some companionship.[ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(() => { return RandomizeConversation(3) && ConversationWithConfidentProstitute(); }), null, 100, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_01_service_r", "cprostitute_talk_01_response", "cprostitute_talk_01_service", "{=CEBROTHEL1027}I have been awfully lonely...", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_01_nevermind_r", "cprostitute_talk_01_response", "prostitute_service_no_response", "{=CEBROTHEL1028}No thanks.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_01_service_ar", "cprostitute_talk_01_service", "prostitute_service", "{=CEBROTHEL1029}I'm sorry to hear that, let's change that tonight for {AMOUNT} denars. [ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(PriceWithProstitute), null, 100, null);

            // Dialogue with Confidient Prostitute 02
            campaignGameStarter.AddDialogLine("cprostitute_talk_02", "start", "cprostitute_talk_02_response", "{=CEBROTHEL1030}Is there something I can help you with this evening?[ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(ConversationWithConfidentProstitute), null, 100, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_02_service_r", "cprostitute_talk_02_response", "cprostitute_talk_02_service", "{=CEBROTHEL1031}Yeah, I think you can help me with the problem I'm having.", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_02_nevermind_r", "cprostitute_talk_02_response", "cprostitute_service_02_no_response", "{=CEBROTHEL1032}Maybe later.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_02_service_ar", "cprostitute_talk_02_service", "cprostitute_service_02", "{=CEBROTHEL1033}Perfect, my \"treatment\" costs {AMOUNT} denars for a full dose.[ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(PriceWithProstitute), null, 100, null);

            campaignGameStarter.AddPlayerLine("cprostitute_service_02_yes", "cprostitute_service_02", "cprostitute_service_02_yes_response", "{=CEBROTHEL1034}Sounds like my kind of cure.", null, null, 100, new ConversationSentence.OnClickableConditionDelegate(ConversationHasEnoughForService), null);
            campaignGameStarter.AddPlayerLine("cprostitute_service_02_no", "cprostitute_service_02", "cprostitute_service_02_no_response", "{=CEBROTHEL1035}You know, my condition isn't that bad.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("cprostitute_service_02_yes_response_id", "cprostitute_service_02_yes_response", "close_window", "{=CEBROTHEL1036}Let's go to the doctor's office so you can be treated.[ib:confident][rb:very_positive]", null, new ConversationSentence.OnConsequenceDelegate(ConversationProstituteConsequenceSex), 100, null);

            campaignGameStarter.AddDialogLine("cprostitute_service_02_no_response_id", "cprostitute_service_02_no_response", "close_window", "{=CEBROTHEL1037}See ya around.[ib:confident][rb:very_positive]", null, null, 100, null);

            // Dialogue with Tired Prostitute 00
            campaignGameStarter.AddDialogLine("tprostitute_talk_00", "start", "tprostitute_talk_response_00", "{=CEBROTHEL1038}What do you want?[ib:closed][rb:unsure]", new ConversationSentence.OnConditionDelegate(() => { return RandomizeConversation(2) && ConversationWithTiredProstitute(); }), null, 100, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_00", "tprostitute_talk_response_00", "tprostitute_service_accept_00", "{=CEBROTHEL1039}I'd like your services for the evening.", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("tprostitute_nevermind_00", "tprostitute_talk_response_00", "prostitute_service_no_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_accept_00_r", "tprostitute_service_accept_00", "tprostitute_service_00", "{=CEBROTHEL1040}Fine, but it's going to be {AMOUNT} denars up front.[ib:closed][rb:unsure]", new ConversationSentence.OnConditionDelegate(PriceWithProstitute), null, 100, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_00_yes", "tprostitute_service_00", "tprostitute_service_00_yes_response", "{=CEBROTHEL1041}Very well.", null, null, 100, new ConversationSentence.OnClickableConditionDelegate(ConversationHasEnoughForService), null);
            campaignGameStarter.AddPlayerLine("tprostitute_service_00_no", "tprostitute_service_00", "prostitute_service_no_response", "{=CEBROTHEL1042}That's a bit too much.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_00_yes_response_id", "tprostitute_service_00_yes_response", "close_window", "{=CEBROTHEL1043}Right this way...[ib:closed][rb:unsure]", null, new ConversationSentence.OnConsequenceDelegate(ConversationProstituteConsequenceSex), 100, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_00_no_response_id", "tprostitute_service_00_no_response", "close_window", "{=CEBROTHEL1044}Thank goodness...[ib:closed][rb:unsure]", null, null, 100, null);

            //  Dialogue with Tired Prostitute 01
            campaignGameStarter.AddDialogLine("tprostitute_talk_01", "start", "tprostitute_talk_response_01", "{=CEBROTHEL1045}Is there something you want?[ib:closed][rb:unsure]", new ConversationSentence.OnConditionDelegate(ConversationWithTiredProstitute), null, 100, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_01", "tprostitute_talk_response_01", "tprostitute_service_accept_01", "{=CEBROTHEL1046}How much for your time?", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("tprostitute_nevermind_01", "tprostitute_talk_response_01", "tprostitute_service_01_no_response", "{=CEBROTHEL1047}Not as this moment.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_accept_01_r", "tprostitute_service_accept_01", "tprostitute_service_01", "{=CEBROTHEL1048}Ok well... {AMOUNT} denars sounds about right.[ib:closed][rb:unsure]", new ConversationSentence.OnConditionDelegate(PriceWithProstitute), null, 100, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_01_yes", "tprostitute_service_01", "tprostitute_service_01_yes_response", "{=CEBROTHEL1049}Alright, here you go.", null, null, 100, new ConversationSentence.OnClickableConditionDelegate(ConversationHasEnoughForService), null);
            campaignGameStarter.AddPlayerLine("tprostitute_service_01_no", "tprostitute_service_01", "tprostitute_service_01_no_response", "{=CEBROTHEL1050}Nevermind.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_01_yes_response_id", "tprostitute_service_01_yes_response", "close_window", "{=CEBROTHEL1051}Follow me to the back.[ib:closed][rb:unsure]", null, new ConversationSentence.OnConsequenceDelegate(ConversationProstituteConsequenceSex), 100, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_01_no_response_id", "tprostitute_service_01_no_response", "close_window", "{=CEBROTHEL1052}Ugh...[ib:closed][rb:unsure]", null, null, 100, null);

            // Dialogue With Owner 00
            campaignGameStarter.AddDialogLine("ce_owner_talk_00", "start", "ce_owner_response_00", "{=CEBROTHEL1053}Oh, a valued customer, how can I help you today?[ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(ConversationWithBrothelOwnerBeforeSelling), null, 100, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_01", "ce_owner_response_00", "ce_owner_buy_00", "{=CEBROTHEL1054}I would like to buy your establishment.", new ConversationSentence.OnConditionDelegate(ConversationWithBrothelOwnerShowBuy), null, 100, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_04", "ce_owner_response_00", "ce_owner_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null, 100, null);

            campaignGameStarter.AddDialogLine("ce_owner_buy_00_r", "ce_owner_buy_00", "ce_owner_buy_response", "{=CEBROTHEL1056}I am selling this establishment for {AMOUNT} denars.", new ConversationSentence.OnConditionDelegate(PriceWithBrothel), null, 100, null);

            campaignGameStarter.AddPlayerLine("ce_owner_buy_yes", "ce_owner_buy_response", "ce_owner_business_complete", "{=CEBROTHEL1049}Alright, here you go.", null,
                new ConversationSentence.OnConsequenceDelegate(ConversationBoughtBrothel), 100, new ConversationSentence.OnClickableConditionDelegate(ConversationHasEnoughMoneyForBrothel), null);
            campaignGameStarter.AddPlayerLine("ce_owner_buy_no", "ce_owner_buy_response", "ce_owner_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("ce_owner_business_complete_response", "ce_owner_business_complete", "close_window", "{=CEBROTHEL1057}A pleasure doing business. [ib:confident][rb:very_positive]", null, null, 100, null);

            campaignGameStarter.AddDialogLine("ce_owner_exit_response", "ce_owner_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null, 100, null);

            // Dialogue With Brothel Owner 01
            campaignGameStarter.AddDialogLine("ce_owner_talk_01", "start", "close_window", "{=CEBROTHEL1059}Let me prepare the establishment, it will be ready for you soon.", new ConversationSentence.OnConditionDelegate(ConversationWithBrothelOwnerAfterSelling), null, 100, null);

            // Dialogue With Assistant 00
            campaignGameStarter.AddDialogLine("ce_assistant_talk_00", "start", "ce_assistant_response_00", "{=CEBROTHEL1060}Hello boss, how can I help you today?[ib:confident][rb:very_positive]", new ConversationSentence.OnConditionDelegate(ConversationWithBrothelAssistantBeforeSelling), null, 100, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_01", "ce_assistant_response_00", "ce_assistant_sell_00", "{=CEBROTHEL1061}I would like to sell our establishment.", null, null, 100, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_04", "ce_assistant_response_00", "ce_assistant_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null, 100, null);

            campaignGameStarter.AddDialogLine("ce_assistant_sell_00_r", "ce_assistant_sell_00", "ce_assistant_sell_response", "{=CEBROTHEL1062}We can sell this establishment for {AMOUNT} denars.", new ConversationSentence.OnConditionDelegate(PriceWithBrothel), null, 100, null);

            campaignGameStarter.AddPlayerLine("ce_assistant_sell_yes", "ce_assistant_sell_response", "ce_assistant_business_complete", "{=CEBROTHEL1018}That's a fair price I'd say.", null,
                new ConversationSentence.OnConsequenceDelegate(ConversationSoldBrothel), 100, null, null);
            campaignGameStarter.AddPlayerLine("ce_assistant_sell_no", "ce_assistant_sell_response", "ce_assistant_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_business_complete_r", "ce_assistant_business_complete", "close_window", "{=CEBROTHEL1063}It's been a pleasure working for you! [ib:confident][rb:very_positive]", null, null, 100, null);

            campaignGameStarter.AddDialogLine("ce_assistant_exit_00_r", "ce_assistant_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null, 100, null);

            // Dialogue With Assistance 01
            campaignGameStarter.AddDialogLine("ce_assistant_talk_01", "start", "close_window", "{=CEBROTHEL1064}The new owner will arrive here shortly, I will just clean up things for now.", new ConversationSentence.OnConditionDelegate(ConversationWithBrothelAssistantAfterSelling), null, 100, null);
        }


        private bool RandomizeConversation(int divider)
        {
            return MBRandom.RandomFloatRanged(0, 100) < (100 / divider);
        }

        // Owner Conditions
        private bool ConversationWithBrothelAssistantAfterSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && _hasSoldEstablishment;
        }
        private bool ConversationWithBrothelOwnerAfterSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && _hasSoldEstablishment;
        }
        private bool ConversationWithBrothelAssistantBeforeSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && !_hasSoldEstablishment;
        }
        private bool ConversationWithBrothelOwnerBeforeSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !_hasSoldEstablishment;
        }
        private bool ConversationWithBrothelOwnerShowBuy()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !Campaign.Current.IsMainHeroDisguised;
        }

        private void ConversationBoughtBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, brothelCost, false);
            BrothelInteraction(Settlement.CurrentSettlement, true);
            _hasSoldEstablishment = true;
        }
        private void ConversationSoldBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, brothelCost, false);
            BrothelInteraction(Settlement.CurrentSettlement, false);
            _hasSoldEstablishment = true;
        }
        private bool ConversationHasEnoughMoneyForBrothel(out TextObject text)
        {
            text = TextObject.Empty;
            if (Hero.MainHero.Gold < brothelCost)
            {
                text = new TextObject("{=CEEVENTS1138}You don't have enough gold", null);
                return false;
            }
            return true;
        }
        private bool PriceWithBrothel()
        {
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(brothelCost.ToString(), null), false);
            return true;
        }

        // Prostitute Conditions
        private static readonly string[] prostituteStrings = { "prostitute_confident", "prostitute_confident", "prostitute_tired" };

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
            if (Hero.MainHero.Gold < prostitutionCost)
            {
                text = new TextObject("{=CEEVENTS1138}You don't have enough gold", null);
                return false;
            }
            return true;
        }
        private bool PriceWithProstitute()
        {
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(prostitutionCost.ToString(), null), false);
            return true;
        }
        private void ConversationProstituteConsequenceSex()
        {
            try
            {
                if (!DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
                {
                    GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, prostitutionCost, false);
                }
                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");
                        break;

                    case CultureCode.Vlandia:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");
                        break;

                    case CultureCode.Aserai:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");
                        break;

                    case CultureCode.Empire:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");
                        break;

                    case CultureCode.Battania:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");
                        break;
                    case CultureCode.Khuzait:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_b");
                        break;
                    default:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");
                        break;
                }

                CESubModule.agentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => { return agent.Character == CharacterObject.OneToOneConversationCharacter; });
                CESubModule.brothelState = CESubModule.BrothelState.Start;
            }
            catch (Exception e)
            {
                Custom.CECustomHandler.ForceLogToFile("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e.ToString());
            }
        }

        // Customer Conditions
        private static readonly string[] customerStrings = { "customer_confident", "customer_tired" };

        private static readonly string[] responses = { "{=CEBROTHEL1019}That's too much, no thanks.", "{=CEBROTHEL1049}Alright, here you go." };

        private static readonly string[] rageResponses = { "{=CEBROTHEL1065}Well perhaps you should, you sure look like a {?PLAYER.GENDER}whore{?}prostitute{\\?}!", "{=CEBROTHEL1066}My apologies, {?PLAYER.GENDER}milady{?}my lord{\\?}!" };

        private bool ConversationWithCustomerNotMetRequirements()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("customer") && (!(Hero.MainHero.IsFemale) || Campaign.Current.IsMainHeroDisguised);
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
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, prostitutionCost, false);
                SkillObject ProstitueFlag = CESkills.IsProstitute;
                Hero.MainHero.SetSkillValue(ProstitueFlag, 1);
                SkillObject ProstitutionSkill = CESkills.Prostitution;

                if (Hero.MainHero.GetSkillValue(ProstitutionSkill) < 100)
                {
                    Hero.MainHero.SetSkillValue(ProstitutionSkill, 100);
                }
                CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(1, 10), Hero.MainHero, false, true, true);
                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");
                        break;

                    case CultureCode.Vlandia:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");
                        break;

                    case CultureCode.Aserai:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");
                        break;

                    case CultureCode.Empire:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");
                        break;

                    case CultureCode.Battania:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");
                        break;
                    case CultureCode.Khuzait:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_b");
                        break;
                    default:
                        CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");
                        break;
                }

                CESubModule.agentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => { return agent.Character == CharacterObject.OneToOneConversationCharacter; });
                CESubModule.brothelState = CESubModule.BrothelState.Start;
            }
            catch (Exception e)
            {
                Custom.CECustomHandler.ForceLogToFile("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e.ToString());
            }
        }


        private bool ConversationWithCustomerRandomResponse()
        {
            if (MBRandom.RandomInt(0, 100) > 20)
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(responses[0], null), false);
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(responses[1], null), false);
                ConversationCustomerConsequenceSex();
            }
            return true;
        }

        private bool ConversationWithCustomerRandomResponseRage()
        {
            if (MBRandom.RandomInt(0, 100) > 40)
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(rageResponses[0], null), false);
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(rageResponses[1], null), false);
            }
            return true;
        }
        // Session
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party == MobileParty.MainParty)
            {
                brothel.RemoveAllCharacters();
                _isBrothelInitialized = false;
                _hasSoldEstablishment = false;
            }
        }

        public void DailyTick()
        {
            _orderedDrinkThisDayInSettlement = null;
            foreach (CEBrothel brothel in CEBrothelBehaviour.GetPlayerBrothels())
            {
                Town town = brothel.Settlement.Town;
                if (!town.IsRebeling)
                {
                    if (brothel.IsRunning)
                    {
                        brothel.ChangeGold(MBRandom.RandomInt(50, 400));
                        brothel.ChangeGold(brothel.Expense);
                        if (brothel.Capital < 0 && Hero.MainHero.Gold < Math.Abs(brothel.Capital))
                        {
                            if (brothel.Settlement.OwnerClan == Clan.PlayerClan)
                            {
                                TextObject textObject3 = new TextObject("{CEBROTHEL0998}The brothel of {SETTLEMENT} has gone bankrupted, and is no longer active.");
                                textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                                InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                                brothel.IsRunning = false;
                            }
                            else
                            {
                                TextObject textObject3 = new TextObject("{CEBROTHEL0999}The brothel of {SETTLEMENT} has gone bankrupted, and has been requisitioned.");
                                textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                                InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                            }

                        }
                    }
                }
            }
        }
        public void WeeklyTick()
        {
            SkillObject ProstitutionSkill = CESkills.Prostitution;
            _hasBoughtTunToParty = false;
            if (Hero.MainHero.GetSkillValue(ProstitutionSkill) > 500)
            {
                CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(-300, -200), Hero.MainHero, false, false, false);
            }
            else if (Hero.MainHero.GetSkillValue(ProstitutionSkill) > 100)
            {
                CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(-40, -10), Hero.MainHero, false, false, false);
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(DailyTick));
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(WeeklyTick));
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(AddGameMenus));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(AddGameMenus));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement>(OnSettlementLeft));
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(LocationCharactersAreReadyToSpawn));
        }

        public static List<CEBrothel> GetPlayerBrothels()
        {
            try
            {
                return _brothelList.FindAll(brothelData => { return brothelData.Owner == Hero.MainHero; });
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to get player owned brothels.");
                return new List<CEBrothel>();
            }
        }
        public static bool DoesOwnBrothelInSettlement(Settlement settlement)
        {
            return _brothelList.Exists(brothelData => { return brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero; });
        }
        public static void AddBrothelData(Settlement settlement)
        {
            _brothelList.Add(new CEBrothel(settlement));
        }
        public static bool ContainsBrothelData(Settlement settlement)
        {
            try
            {
                CEBrothel data = _brothelList.FirstOrDefault(brothelData => { return brothelData.Settlement.StringId == settlement.StringId; });
                if (data == null)
                {
                    AddBrothelData(settlement);
                }
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
                {
                    _brothelList.Where(brothel => { return brothel.Settlement.StringId == settlement.StringId; }).Select(brothel => { brothel.Owner = flagToPurchase ? Hero.MainHero : null; return brothel; }).ToList();
                }
            }
            catch (Exception)
            {

            }
        }
        public static List<CharacterObject> FetchBrothelPrisoners(Settlement settlement)
        {
            try
            {
                CEBrothel testLocation = _brothelList.FirstOrDefault(brothel => { return brothel.Settlement.StringId == settlement.StringId; });
                if (testLocation != null)
                {
                    return testLocation.CaptiveProstitutes;
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
        public static void RemovePrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null)
                {
                    return;
                }

                if (ContainsBrothelData(settlement))
                {
                    _brothelList.Where(brothel => { return brothel.Settlement.StringId == settlement.StringId; }).Select(brothel => { brothel.CaptiveProstitutes.Remove(prisoner); return brothel; }).ToList();
                    MobileParty.MainParty.PrisonRoster.AddToCounts(prisoner, 1, prisoner.IsHero);
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to RemovePrisoner");
            }
        }
        public static void AddPrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null)
                {
                    return;
                }
                if (ContainsBrothelData(settlement))
                {
                    _brothelList.Where(brothel => { return brothel.Settlement.StringId == settlement.StringId; }).Select(brothel => { brothel.CaptiveProstitutes.Add(prisoner); return brothel; }).ToList();
                    MobileParty.MainParty.PrisonRoster.RemoveTroop(prisoner, 1);
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to AddPrisoner");
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<Settlement>>("SettlementsThatPlayerHasSpy", ref SettlementsThatPlayerHasSpy);
            dataStore.SyncData<Settlement>("_orderedDrinkThisDayInSettlement", ref _orderedDrinkThisDayInSettlement);
            dataStore.SyncData<bool>("_orderedDrinkThisVisit", ref _orderedDrinkThisVisit);
            dataStore.SyncData<bool>("_hasMetWithRansomBroker", ref _hasMetWithRansomBroker);
            dataStore.SyncData<bool>("_hasBoughtTunToParty", ref _hasBoughtTunToParty);
            dataStore.SyncData<List<CEBrothel>>("_CEbrothelList", ref _brothelList);
        }

        private static List<CEBrothel> _brothelList = new List<CEBrothel>();

        private List<Settlement> SettlementsThatPlayerHasSpy = new List<Settlement>();

        private const int prostitutionCost = 60;

        private const int brothelCost = 5000;

        private Settlement _orderedDrinkThisDayInSettlement;

        private bool _orderedDrinkThisVisit;

        private bool _hasSoldEstablishment;

        private bool _hasMetWithRansomBroker;

        private bool _hasBoughtTunToParty;
    }
}