using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelBehavior : CampaignBehaviorBase
    {

        internal CEBrothelMission Mission { get; set; }

        internal CEBrothelMenu Menu { get; set; }

        internal CEBrothelSession Session { get; set; }

        internal Location BrothelLocation { get; set; }

        internal CEBrothelProstituteConditions Prostitute { get; set; }

        internal CEBrothelOwnerConditions Owner { get; set; }

        internal CEBrothelCustomerConditions Customer { get; set; }

        public CEBrothelBehavior() 
        {
            BrothelLocation = new Location("_Brothel", new TextObject("{=CEEVENTS1099}Brothel"), new TextObject("{=CEEVENTS1099}Brothel"), 30, true, false, "CanIfSettlementAccessModelLetsPlayer", "CanAlways", "CanAlways", "CanIfGrownUpMaleOrHero", new[] {"empire_house_c_tavern_a", "", "", ""}, null);
            
            Menu = new CEBrothelMenu(BrothelLocation);
            Owner = new CEBrothelOwnerConditions();
            Customer = new CEBrothelCustomerConditions(Owner);
            Prostitute = new CEBrothelProstituteConditions(Owner);
            Mission = new CEBrothelMission(BrothelLocation, Customer, Prostitute,Owner);
            Session = new CEBrothelSession(Mission);
            //Ransom = new CEBrothelRansom();
        }


        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("SettlementsThatPlayerHasSpy", ref Session.SettlementsThatPlayerHasSpy);
            dataStore.SyncData("_orderedDrinkThisDayInSettlement", ref Session.OrderedDrinkThisDayInSettlement);
            dataStore.SyncData("_orderedDrinkThisVisit", ref Session.OrderedDrinkThisVisit);
            dataStore.SyncData("_hasMetWithRansomBroker", ref Session.HasMetWithRansomBroker);
            dataStore.SyncData("_hasBoughtTunToParty", ref Session.HasBoughtTunToParty);
            dataStore.SyncData("_CEbrothelList", ref Session.BrothelList);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, Session.DailyTick);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, Session.WeeklyTick);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, AddGameMenus);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, Session.OnSessionLaunched);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, Session.OnSettlementLeft);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, Mission.LocationCharactersAreReadyToSpawn);
        }


        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {
            if (CESettings.Instance != null && !CESettings.Instance.ProstitutionControl) return;


            campaignGameStarter.AddGameMenuOption("town", "town_brothel", "{=CEEVENTS1100}Go to the _Brothel district", Menu.CanGoToBrothelDistrictOnCondition, delegate
                                                                                                                                                                {
                                                                                                                                                                    GameMenu.SwitchToMenu("town_brothel");
                                                                                                                                                                }, false, 1);

            campaignGameStarter.AddGameMenu("town_brothel", "{=CEEVENTS1098}You are in the _Brothel district", Menu.BrothelDistrictOnInit, GameOverlays.MenuOverlayType.SettlementWithCharacters);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_visit", "{=CEEVENTS1101}Visit the _Brothel", Menu.VisitBrothelOnCondition, Menu.VisitBrothelOnConsequence, false, 0);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_prostitution", "{=CEEVENTS1102}Become a prostitute at the _Brothel", Menu.ProstitutionMenuJoinOnCondition, Menu.ProstitutionMenuJoinOnConsequence, false, 1);


            var brothelRansom = new CEBrothelRansom();

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_some_captives", "{=CEEVENTS1097}Sell some captives to the _Brothel", brothelRansom.SellPrisonerOneStackOnCondition, delegate
                                                                                                                                                                                                         {
                                                                                                                                                                                                             brothelRansom.ChooseRansomPrisoners();
                                                                                                                                                                                                         }, false, 2);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_sell_all_prisoners", "{=CEEVENTS1096}Sell all captives to the _Brothel ({RANSOM_AMOUNT}{GOLD_ICON})", brothelRansom.SellPrisonersCondition, delegate
                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                brothelRansom.SellAllPrisoners();
                                                                                                                                                                                                                            }, false, 3);

            campaignGameStarter.AddGameMenuOption("town_brothel", "town_brothel_back", "{=qWAmxyYz}Back to town center", Menu.BackOnCondition, delegate
                                                                                                                                               {
                                                                                                                                                   GameMenu.SwitchToMenu("town");
                                                                                                                                               }, true, 4);
        }
    }
}