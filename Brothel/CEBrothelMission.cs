using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelMission
    {
        internal Location Brothel { get; set; }
        internal bool IsBrothelInitialized;
        internal CEBrothelCustomerConditions Customer { get; set; }
        internal CEBrothelProstituteConditions Prostitute { get; set; }
        internal CEBrothelOwnerConditions Owner { get; set; }

        public CEBrothelMission(Location brothel, CEBrothelCustomerConditions customer, CEBrothelProstituteConditions prostitute, CEBrothelOwnerConditions owner)
        {
            Brothel = brothel;
            Customer = customer;
            Prostitute = prostitute;
            Owner = owner;
        }

        public void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            var settlement = PlayerEncounter.LocationEncounter.Settlement;

            if (CampaignMission.Current.Location != Brothel || IsBrothelInitialized) return;

            //LocationComplex.Current.AddPassage(settlement.LocationComplex.GetLocationWithId("center"), _Brothel);
            AddPeopleToTownTavern(settlement, unusedUsablePointCount);
            IsBrothelInitialized = true;
        }

        private LocationCharacter CreateTavernkeeper(CultureObject culture, LocationCharacter.CharacterRelations relations)
        {
            var owner = MBObjectManager.Instance.CreateObject<CharacterObject>();

            if (Owner.DoesOwnBrothelInSettlement(Settlement.CurrentSettlement)) //doesOwnBrothelInSettlement(Settlement.CurrentSettlement)
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


            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)).Equipment(culture.FemaleDancer.AllEquipments.GetRandomElement()), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "spawnpoint_tavernkeeper", true, relations, "as_human_tavern_keeper", true);
        }

        private LocationCharacter CreateRansomBroker(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            BasicCharacterObject owner = CharacterObject.CreateFrom(culture.RansomBroker);
            owner.Name = new TextObject("{=CEEVENTS1065}Slave Trader");
            owner.StringId = "brothel_slaver";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(owner)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), SandBoxManager.Instance.AgentBehaviorManager.AddOutdoorWandererBehaviors, "npc_common", true, relation, null, true);
        }

        private LocationCharacter CreateMusician(CultureObject culture, LocationCharacter.CharacterRelations relation)
        {
            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(culture.Musician)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "musician", true, relation, "as_human_musician", true, true);
        }

        private LocationCharacter CreateTownsManForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
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
            townsman.StringId = Customer.CustomerStrings.GetRandomElement();
            townsman.HairTags = templateToCopy.HairTags;
            townsman.BeardTags = templateToCopy.BeardTags;
            townsman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());

            string actionSetCode;

            if ((culture.StringId.ToLower() == "aserai" || culture.StringId.ToLower() == "khuzait")) actionSetCode = "as_human_villager_in_aserai_tavern";
            else actionSetCode = "as_human_villager_in_tavern";

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townsman)).Monster(Campaign.Current.HumanMonsterSettlementSlow).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.MaxAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_common", true, relation, actionSetCode, true);
        }

        private LocationCharacter CreateTavernWench(CultureObject culture, LocationCharacter.CharacterRelations relation)
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

        private LocationCharacter CreateDancer(CultureObject culture, LocationCharacter.CharacterRelations relation)
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
            townswoman.StringId = Prostitute.ProstituteStrings.GetRandomElement();
            townswoman.HairTags = templateToCopy.HairTags;
            townswoman.BeardTags = templateToCopy.BeardTags;
            townswoman.InitializeEquipmentsOnLoad(templateToCopy.AllEquipments.ToList());

            return new LocationCharacter(new AgentData(new SimpleAgentOrigin(townswoman)).Monster(Campaign.Current.HumanMonsterSettlement).Age(MBRandom.RandomInt(25, Campaign.Current.Models.AgeModel.BecomeOldAge)), SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors, "npc_dancer", true, relation, "as_human_female_dancer", true);
        }

        private LocationCharacter CreateTownsWomanForTavern(CultureObject culture, LocationCharacter.CharacterRelations relation)
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
            townswoman.StringId = Prostitute.ProstituteStrings.GetRandomElement();
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

            Brothel.AddLocationCharacters(CreateTavernkeeper, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            Brothel.AddLocationCharacters(CreateTavernWench, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            Brothel.AddLocationCharacters(CreateMusician, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);
            Brothel.AddLocationCharacters(CreateRansomBroker, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, 1);

            unusedUsablePointCount.TryGetValue("npc_dancer", out var dancers);
            if (dancers > 0) Brothel.AddLocationCharacters(CreateDancer, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, dancers);


            if (num <= 0) return;

            var num2 = (int) (num * 0.2f);
            if (num2 > 0) Brothel.AddLocationCharacters(CreateTownsManForTavern, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num2);
            var num3 = (int) (num * 0.3f);
            if (num3 > 0) Brothel.AddLocationCharacters(CreateTownsWomanForTavern, settlement.Culture, LocationCharacter.CharacterRelations.Neutral, num3);
        }

        protected internal void AddDialogs(CampaignGameStarter campaignGameStarter)
        {
            // Owner Dialogue Confident Prostitute
            campaignGameStarter.AddDialogLine("prostitute_requirements_owner", "start", "cprostitute_owner_00", "{=CEBROTHEL1008}Do you need something {?PLAYER.GENDER}milady{?}my lord{\\?}? [ib:confident][rb:very_positive]", () => Prostitute.ConversationWithProstituteIsOwner() && Prostitute.ConversationWithConfidentProstitute(), null);

            campaignGameStarter.AddPlayerLine("cprostitute_owner_00_yes", "cprostitute_owner_00", "prostitute_service_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null);

            campaignGameStarter.AddPlayerLine("cprostitute_owner_00_nevermind", "cprostitute_owner_00", "close_window", "{=CEBROTHEL1002}Continue as you were.", null, null);


            // Owner Dialogue Tired Prostitute
            campaignGameStarter.AddDialogLine("prostitute_requirements_owner", "start", "tprostitute_owner_00", "{=CEBROTHEL1006}Hello {?PLAYER.GENDER}milady{?}my lord{\\?}, I think I need a break.", () => Prostitute.ConversationWithProstituteIsOwner() && Prostitute.ConversationWithTiredProstitute(), null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_yes", "tprostitute_owner_00", "tprostitute_service_01_yes_response", "{=CEBROTHEL1007}I will like to have some fun.", null, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_no", "tprostitute_owner_00", "tprostitute_service_01_no_response", "{=CEBROTHEL1004}No, there is plenty of customers waiting, you must continue working.", null, null);

            campaignGameStarter.AddPlayerLine("tprostitute_owner_00_break", "tprostitute_owner_00", "tprostitute_owner_00_break_response", "{=CEBROTHEL1003}Go take a break.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_owner_00_break_r", "tprostitute_owner_00_break_response", "close_window", "{=CEBROTHEL1005}Thank you {?PLAYER.GENDER}milady{?}my lord{\\?}.", null, null);

            // Requirements not met
            campaignGameStarter.AddDialogLine("prostitute_requirements_not_met", "start", "close_window", "{=CEBROTHEL1009}Sorry {?PLAYER.GENDER}milady{?}my lord{\\?}, I am currently very busy.", Prostitute.ConversationWithProstituteNotMetRequirements, null);

            campaignGameStarter.AddDialogLine("customer_requirements_not_met", "start", "customer_00", "{=CEBROTHEL1010}What do you want? Cannot you see that I am trying to enjoy the whores here? [ib:normal][rb:unsure]", () => { return Customer.ConversationWithCustomerNotMetRequirements() && Customer.ConversationWithConfidentCustomer(); }, null);

            campaignGameStarter.AddPlayerLine("customer_00_nevermind", "customer_00", "prostitute_service_no_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            // Confident Customer 00
            campaignGameStarter.AddDialogLine("ccustomer_00_start", "start", "ccustomer_00", "{=CEBROTHEL1014}Well hello there you {?PLAYER.GENDER}fine whore{?}stud{\\?}, would you like {AMOUNT} denars for your services? [ib:confident][rb:very_positive]", () => { return RandomizeConversation(2) && Prostitute.PriceWithProstitute() && Customer.ConversationWithConfidentCustomer(); }, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_service", "ccustomer_00", "close_window", "{=CEBROTHEL1015}Yes, my lord I can do that.", null, Customer.ConversationCustomerConsequenceSex);

            campaignGameStarter.AddPlayerLine("ccustomer_00_rage", "ccustomer_00", "ccustomer_00_rage_reply", "{=CEBROTHEL1016}Excuse me, I don't work here!", null, null);

            campaignGameStarter.AddDialogLine("ccustomer_00_rage_reply_r", "ccustomer_00_rage_reply", "close_window", "{=!}{RESPONSE_STRING}", Customer.ConversationWithCustomerRandomResponseRage, null);

            campaignGameStarter.AddPlayerLine("ccustomer_00_nevermind", "ccustomer_00", "close_window", "{=CEBROTHEL1017}Sorry sir, I have to leave.", null, null);

            // Tried Customer 01
            campaignGameStarter.AddDialogLine("tcustomer_00_start", "start", "tcustomer_00", "{=CEBROTHEL1012}Yes? [ib:normal][rb:unsure]", () => Customer.ConversationWithTiredCustomer() || Customer.ConversationWithConfidentCustomer(), null);

            campaignGameStarter.AddPlayerLine("tcustomer_00_service", "tcustomer_00", "tcustomer_00_talk_service", "{=CEBROTHEL1013}Would you like my services for {AMOUNT} denars?", () => { return Prostitute.PriceWithProstitute() && !Customer.ConversationWithCustomerNotMetRequirements(); }, null);

            campaignGameStarter.AddPlayerLine("tcustomer_00_nevermind", "tcustomer_00", "close_window", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            campaignGameStarter.AddDialogLine("tcustomer_00_service_r", "tcustomer_00_talk_service", "close_window", "{=!}{RESPONSE_STRING}", Customer.ConversationWithCustomerRandomResponse, null);


            // Confident Prostitute Extra Replies
            campaignGameStarter.AddPlayerLine("prostitute_service_yes", "prostitute_service", "prostitute_service_yes_response", "{=CEBROTHEL1018}That's a fair price I'd say.", null, null, 100, Prostitute.ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("prostitute_service_no", "prostitute_service", "prostitute_service_no_response", "{=CEBROTHEL1019}That's too much, no thanks.", null, null);

            campaignGameStarter.AddDialogLine("prostitute_service_yes_response_id", "prostitute_service_yes_response", "close_window", "{=CEBROTHEL1020}Follow me sweetie. [ib:normal][rb:positive]", null, Prostitute.ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("prostitute_service_no_response_id", "prostitute_service_no_response", "close_window", "{=CEBROTHEL1021}Stop wasting my time then.[ib:aggressive][rb:unsure]", null, null);

            // Dialogue with Confidient Prostitute 00
            campaignGameStarter.AddDialogLine("cprostitute_talk_00", "start", "cprostitute_talk_00_response", "{=CEBROTHEL1022}Hey {?PLAYER.GENDER}beautiful{?}handsome{\\?} want to have some fun? [ib:confident][rb:very_positive]", () => { return RandomizeConversation(3) && Prostitute.ConversationWithConfidentProstitute(); }, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_00_service_r", "cprostitute_talk_00_response", "cprostitute_talk_00_service", "{=CEBROTHEL1023}Yeah, I could go for a bit of refreshment.", null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_00_nevermind_r", "cprostitute_talk_00_response", "prostitute_service_no_response", "{=CEBROTHEL1024}No, I am fine.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_00_service_ar", "cprostitute_talk_00_service", "prostitute_service", "{=CEBROTHEL1025}Sounds good, that'll be {AMOUNT} denars. [ib:confident][rb:very_positive]", Prostitute.PriceWithProstitute, null);

            // Dialogue with Confidient Prostitute 01
            campaignGameStarter.AddDialogLine("cprostitute_talk_01", "start", "cprostitute_talk_01_response", "{=CEBROTHEL1026}Hey {?PLAYER.GENDER}cutie{?}handsome{\\?} you look like you need some companionship.[ib:confident][rb:very_positive]", () => { return RandomizeConversation(3) && Prostitute.ConversationWithConfidentProstitute(); }, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_01_service_r", "cprostitute_talk_01_response", "cprostitute_talk_01_service", "{=CEBROTHEL1027}I have been awfully lonely...", null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_01_nevermind_r", "cprostitute_talk_01_response", "prostitute_service_no_response", "{=CEBROTHEL1028}No thanks.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_01_service_ar", "cprostitute_talk_01_service", "prostitute_service", "{=CEBROTHEL1029}I'm sorry to hear that, let's change that tonight for {AMOUNT} denars. [ib:confident][rb:very_positive]", Prostitute.PriceWithProstitute, null);

            // Dialogue with Confidient Prostitute 02
            campaignGameStarter.AddDialogLine("cprostitute_talk_02", "start", "cprostitute_talk_02_response", "{=CEBROTHEL1030}Is there something I can help you with this evening?[ib:confident][rb:very_positive]", Prostitute.ConversationWithConfidentProstitute, null);

            campaignGameStarter.AddPlayerLine("cprostitute_talk_02_service_r", "cprostitute_talk_02_response", "cprostitute_talk_02_service", "{=CEBROTHEL1031}Yeah, I think you can help me with the problem I'm having.", null, null);
            campaignGameStarter.AddPlayerLine("cprostitute_talk_02_nevermind_r", "cprostitute_talk_02_response", "cprostitute_service_02_no_response", "{=CEBROTHEL1032}Maybe later.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_talk_02_service_ar", "cprostitute_talk_02_service", "cprostitute_service_02", "{=CEBROTHEL1033}Perfect, my \"treatment\" costs {AMOUNT} denars for a full dose.[ib:confident][rb:very_positive]", Prostitute.PriceWithProstitute, null);

            campaignGameStarter.AddPlayerLine("cprostitute_service_02_yes", "cprostitute_service_02", "cprostitute_service_02_yes_response", "{=CEBROTHEL1034}Sounds like my kind of cure.", null, null, 100, Prostitute.ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("cprostitute_service_02_no", "cprostitute_service_02", "cprostitute_service_02_no_response", "{=CEBROTHEL1035}You know, my condition isn't that bad.", null, null);

            campaignGameStarter.AddDialogLine("cprostitute_service_02_yes_response_id", "cprostitute_service_02_yes_response", "close_window", "{=CEBROTHEL1036}Let's go to the doctor's office so you can be treated.[ib:confident][rb:very_positive]", null, Prostitute.ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("cprostitute_service_02_no_response_id", "cprostitute_service_02_no_response", "close_window", "{=CEBROTHEL1037}See ya around.[ib:confident][rb:very_positive]", null, null);

            // Dialogue with Tired Prostitute 00
            campaignGameStarter.AddDialogLine("tprostitute_talk_00", "start", "tprostitute_talk_response_00", "{=CEBROTHEL1038}What do you want?[ib:closed][rb:unsure]", () => { return RandomizeConversation(2) && Prostitute.ConversationWithTiredProstitute(); }, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_00", "tprostitute_talk_response_00", "tprostitute_service_accept_00", "{=CEBROTHEL1039}I'd like your services for the evening.", null, null);
            campaignGameStarter.AddPlayerLine("tprostitute_nevermind_00", "tprostitute_talk_response_00", "prostitute_service_no_response", "{=CEBROTHEL1011}Uh, nevermind.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_accept_00_r", "tprostitute_service_accept_00", "tprostitute_service_00", "{=CEBROTHEL1040}Fine, but it's going to be {AMOUNT} denars up front.[ib:closed][rb:unsure]", Prostitute.PriceWithProstitute, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_00_yes", "tprostitute_service_00", "tprostitute_service_00_yes_response", "{=CEBROTHEL1041}Very well.", null, null, 100, Prostitute.ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("tprostitute_service_00_no", "tprostitute_service_00", "prostitute_service_no_response", "{=CEBROTHEL1042}That's a bit too much.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_00_yes_response_id", "tprostitute_service_00_yes_response", "close_window", "{=CEBROTHEL1043}Right this way...[ib:closed][rb:unsure]", null, Prostitute.ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("tprostitute_service_00_no_response_id", "tprostitute_service_00_no_response", "close_window", "{=CEBROTHEL1044}Thank goodness...[ib:closed][rb:unsure]", null, null);

            //  Dialogue with Tired Prostitute 01
            campaignGameStarter.AddDialogLine("tprostitute_talk_01", "start", "tprostitute_talk_response_01", "{=CEBROTHEL1045}Is there something you want?[ib:closed][rb:unsure]", Prostitute.ConversationWithTiredProstitute, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_01", "tprostitute_talk_response_01", "tprostitute_service_accept_01", "{=CEBROTHEL1046}How much for your time?", null, null);
            campaignGameStarter.AddPlayerLine("tprostitute_nevermind_01", "tprostitute_talk_response_01", "tprostitute_service_01_no_response", "{=CEBROTHEL1047}Not as this moment.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_accept_01_r", "tprostitute_service_accept_01", "tprostitute_service_01", "{=CEBROTHEL1048}Ok well... {AMOUNT} denars sounds about right.[ib:closed][rb:unsure]", Prostitute.PriceWithProstitute, null);

            campaignGameStarter.AddPlayerLine("tprostitute_service_01_yes", "tprostitute_service_01", "tprostitute_service_01_yes_response", "{=CEBROTHEL1049}Alright, here you go.", null, null, 100, Prostitute.ConversationHasEnoughForService);
            campaignGameStarter.AddPlayerLine("tprostitute_service_01_no", "tprostitute_service_01", "tprostitute_service_01_no_response", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("tprostitute_service_01_yes_response_id", "tprostitute_service_01_yes_response", "close_window", "{=CEBROTHEL1051}Follow me to the back.[ib:closed][rb:unsure]", null, Prostitute.ConversationProstituteConsequenceSex);

            campaignGameStarter.AddDialogLine("tprostitute_service_01_no_response_id", "tprostitute_service_01_no_response", "close_window", "{=CEBROTHEL1052}Ugh...[ib:closed][rb:unsure]", null, null);

            // Dialogue With Owner 00
            campaignGameStarter.AddDialogLine("ce_owner_talk_00", "start", "ce_owner_response_00", "{=CEBROTHEL1053}Oh, a valued customer, how can I help you today?[ib:confident][rb:very_positive]", Owner.ConversationWithBrothelOwnerBeforeSelling, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_01", "ce_owner_response_00", "ce_owner_buy_00", "{=CEBROTHEL1054}I would like to buy your establishment.", Owner.ConversationWithBrothelOwnerShowBuy, null);

            campaignGameStarter.AddPlayerLine("ce_op_response_04", "ce_owner_response_00", "ce_owner_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_buy_00_r", "ce_owner_buy_00", "ce_owner_buy_response", "{=CEBROTHEL1056}I am selling this establishment for {AMOUNT} denars.", Owner.PriceWithBrothel, null);

            campaignGameStarter.AddPlayerLine("ce_owner_buy_yes", "ce_owner_buy_response", "ce_owner_business_complete", "{=CEBROTHEL1049}Alright, here you go.", null, Owner.ConversationBoughtBrothel, 100, Owner.ConversationHasEnoughMoneyForBrothel);
            campaignGameStarter.AddPlayerLine("ce_owner_buy_no", "ce_owner_buy_response", "ce_owner_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_business_complete_response", "ce_owner_business_complete", "close_window", "{=CEBROTHEL1057}A pleasure doing business. [ib:confident][rb:very_positive]", null, null);

            campaignGameStarter.AddDialogLine("ce_owner_exit_response", "ce_owner_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Brothel Owner 01
            campaignGameStarter.AddDialogLine("ce_owner_talk_01", "start", "close_window", "{=CEBROTHEL1059}Let me prepare the establishment, it will be ready for you soon.", Owner.ConversationWithBrothelOwnerAfterSelling, null);

            // Dialogue With Assistant 00
            campaignGameStarter.AddDialogLine("ce_assistant_talk_00", "start", "ce_assistant_response_00", "{=CEBROTHEL1060}Hello boss, how can I help you today?[ib:confident][rb:very_positive]", Owner.ConversationWithBrothelAssistantBeforeSelling, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_01", "ce_assistant_response_00", "ce_assistant_sell_00", "{=CEBROTHEL1061}I would like to sell our establishment.", null, null);

            campaignGameStarter.AddPlayerLine("ce_ap_response_04", "ce_assistant_response_00", "ce_assistant_exit_00", "{=CEBROTHEL1055}I don't need anything at the moment.", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_sell_00_r", "ce_assistant_sell_00", "ce_assistant_sell_response", "{=CEBROTHEL1062}We can sell this establishment for {AMOUNT} denars.", Owner.PriceWithBrothel, null);

            campaignGameStarter.AddPlayerLine("ce_assistant_sell_yes", "ce_assistant_sell_response", "ce_assistant_business_complete", "{=CEBROTHEL1018}That's a fair price I'd say.", null, Owner.ConversationSoldBrothel);
            campaignGameStarter.AddPlayerLine("ce_assistant_sell_no", "ce_assistant_sell_response", "ce_assistant_exit_00", "{=CEBROTHEL1050}Nevermind.", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_business_complete_r", "ce_assistant_business_complete", "close_window", "{=CEBROTHEL1063}It's been a pleasure working for you! [ib:confident][rb:very_positive]", null, null);

            campaignGameStarter.AddDialogLine("ce_assistant_exit_00_r", "ce_assistant_exit_00", "close_window", "{=CEBROTHEL1058}Very well, I'll be here if you need anything. [ib:confident][rb:very_positive]", null, null);

            // Dialogue With Assistance 01
            campaignGameStarter.AddDialogLine("ce_assistant_talk_01", "start", "close_window", "{=CEBROTHEL1064}The new owner will arrive here shortly, I will just clean up things for now.", Owner.ConversationWithBrothelAssistantAfterSelling, null);
        }


        private bool RandomizeConversation(int divider)
        {
            return MBRandom.RandomFloatRanged(0, 100) < 100.0f / divider;
        }
    }
}