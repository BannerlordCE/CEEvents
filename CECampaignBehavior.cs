using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace CaptivityEvents.CampaignBehaviors
{
    internal class CECampaignBehavior : CampaignBehaviorBase
    {
        private void RunOnChildConceived(Hero hero)
        {
            try
            {
                if (CEContext.spouseOne == null && CEContext.spouseTwo == null) return;

                var father = CEContext.spouseOne == hero
                    ? CEContext.spouseTwo
                    : CEContext.spouseOne;
                CECustomHandler.LogToFile("Added " + hero.Name + "'s Pregenancy");
                _heroPregnancies.Add(new Pregnancy(hero, father, CampaignTime.DaysFromNow(CESettings.Instance.PregnancyDurationInDays)));
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Failed to handle OnChildConceivedEvent.");
                CECustomHandler.LogMessage(e.Message + " : " + e);
            }
        }

        private void LaunchCaptorEvent()
        {
            if (CEContext.notificationCaptorExists) return;

            var captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
            var returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);

            if (returnedEvent == null) return;
            CEContext.notificationCaptorExists = true;

            try
            {
                if (!returnedEvent.NotificationName.IsStringNoneOrEmpty())
                    CESubModule.LoadCampaignNotificationTexture(returnedEvent.NotificationName);
                else if (returnedEvent.SexualContent)
                    CESubModule.LoadCampaignNotificationTexture("CE_sexual_notification");
                else
                    CESubModule.LoadCampaignNotificationTexture("CE_castle_notification");
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));

                CECustomHandler.LogMessage("LoadCampaignNotificationTexture");
                CECustomHandler.LogMessage(e.Message + " : " + e);
            }

            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new CECaptorMapNotification(returnedEvent, new TextObject("{=CEEVENTS1090}Captor event is ready")));
        }

        private void LaunchRandomEvent()
        {
            if (CEContext.notificationEventExists) return;
            var returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();

            if (returnedEvent == null) return;
            CEContext.notificationEventExists = true;

            try
            {
                if (!returnedEvent.NotificationName.IsStringNoneOrEmpty())
                    CESubModule.LoadCampaignNotificationTexture(returnedEvent.NotificationName, 1);
                else if (returnedEvent.SexualContent)
                    CESubModule.LoadCampaignNotificationTexture("CE_random_sexual_notification", 1);
                else
                    CESubModule.LoadCampaignNotificationTexture("CE_random_notification", 1);
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));

                CECustomHandler.LogMessage("LoadCampaignNotificationTexture");
                CECustomHandler.LogMessage(e.Message + " : " + e);
            }

            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new CEEventMapNotification(returnedEvent, new TextObject("{=CEEVENTS1059}Random event is ready")));
        }

        private void RunHourlyTick()
        {
            if (CESettings.Instance != null && CESettings.Instance.EventCaptorOn && Hero.MainHero.IsPartyLeader)
                if (CheckEventHourly())
                {
                    CECustomHandler.LogToFile("Checking Campaign Events");

                    try
                    {
                        if (MobileParty.MainParty.Party.PrisonRoster.Count > 0)
                        {
                            if (CESettings.Instance.EventCaptorNotifications)
                            {
                                if (CESettings.Instance.EventRandomEnabled && (!CEContext.notificationEventExists || !CEContext.notificationCaptorExists))
                                {
                                    var randomNumber = MBRandom.RandomInt(100);

                                    if (!CEContext.notificationEventExists && randomNumber < CESettings.Instance.EventRandomFireChance)
                                        LaunchRandomEvent();
                                    else if (!CEContext.notificationCaptorExists && randomNumber > CESettings.Instance.EventRandomFireChance) LaunchCaptorEvent();
                                }

                                {
                                    LaunchCaptorEvent();
                                }
                            }
                            else
                            {
                                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                                {
                                    CEEvent returnedEvent;

                                    if (CESettings.Instance.EventRandomEnabled)
                                    {
                                        if (MBRandom.RandomInt(100) < CESettings.Instance.EventRandomFireChance)
                                        {
                                            returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();

                                            if (returnedEvent != null)
                                            {
                                                var Captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                                                returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(Captive);
                                            }
                                        }
                                        else
                                        {
                                            var captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                                            returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);
                                            if (returnedEvent != null) returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();
                                        }
                                    }
                                    else
                                    {
                                        var captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                                        returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);
                                    }

                                    if (returnedEvent != null)
                                    {
                                        Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                                        if (!mapState.AtMenu)
                                        {
                                            GameMenu.ActivateGameMenu("prisoner_wait");
                                        }
                                        else
                                        {
                                            _extraVariables.MenuToSwitchBackTo = mapState.GameMenuId;
                                            _extraVariables.CurrentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                                        }

                                        GameMenu.SwitchToMenu(returnedEvent.Name);
                                    }
                                }
                            }
                        }
                        else if (CESettings.Instance.EventRandomEnabled)
                        {
                            if (CESettings.Instance.EventCaptorNotifications)
                            {
                                LaunchRandomEvent();
                            }
                            else
                            {
                                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                                {
                                    var returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();

                                    if (returnedEvent != null)
                                    {
                                        Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                                        if (!mapState.AtMenu)
                                        {
                                            GameMenu.ActivateGameMenu("prisoner_wait");
                                        }
                                        else
                                        {
                                            _extraVariables.MenuToSwitchBackTo = mapState.GameMenuId;
                                            _extraVariables.CurrentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                                        }

                                        GameMenu.SwitchToMenu(returnedEvent.Name);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.LogMessage("CheckEventHourly Failure");
                        CECustomHandler.LogMessage(e.Message + " : " + e);
                    }
                }

            ;

            try
            {
                _heroPregnancies.ForEach(item =>
                                         {
                                             item.Mother.IsPregnant = true;
                                             CheckOffspringsToDeliver(item);
                                         });

                _heroPregnancies.RemoveAll(item => item.AlreadyOccured);
            }
            catch (Exception e)
            {
                var textObject = new TextObject("{=CEEVENTS1007}Error: resetting the CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                _heroPregnancies = new List<Pregnancy>();
                CECustomHandler.LogMessage("Failed _heroPregnancies ForEach");
                CECustomHandler.LogMessage(e.Message + " : " + e);
            }

            if (CESettings.Instance != null && !CESettings.Instance.EventCaptorGearCaptives) return;

            {
                try
                {
                    _returnEquipment.ForEach(CheckEquipmentToReturn);

                    _returnEquipment.RemoveAll(item => item.AlreadyOccured);
                }
                catch (Exception e)
                {
                    var textObject = new TextObject("{=CEEVENTS1006}Error: resetting the return equipment list");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                    _returnEquipment = new List<ReturnEquipment>();
                    CECustomHandler.LogMessage("Failed _returnEquipment ForEach");
                    CECustomHandler.LogMessage(e.Message + " : " + e);
                }
            }
        }

        private void RunDailyTick()
        {
            try
            {
                if (!Hero.MainHero.IsPregnant) return;
                var pregnancydue = _heroPregnancies.Find(pregnancy => pregnancy.Mother == Hero.MainHero);

                if (pregnancydue == null || pregnancydue.AlreadyOccured) return;
                TextObject textObject40;

                if (pregnancydue.DueDate.RemainingDaysFromNow < 1f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1061}You are about to give birth...");
                    Hero.MainHero.DynamicBodyProperties = new DynamicBodyProperties(Hero.MainHero.DynamicBodyProperties.Age, 1f, Hero.MainHero.DynamicBodyProperties.Build);
                }
                else if (pregnancydue.DueDate.RemainingDaysFromNow < 10f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1062}Your baby begins kicking, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancydue.DueDate.RemainingDaysFromNow).ToString());
                    var weight = Hero.MainHero.DynamicBodyProperties.Weight;
                    Hero.MainHero.DynamicBodyProperties = new DynamicBodyProperties(Hero.MainHero.DynamicBodyProperties.Age, MBMath.ClampFloat(weight + 0.3f, 0.3f, 1f), Hero.MainHero.DynamicBodyProperties.Build);
                }
                else if (pregnancydue.DueDate.RemainingDaysFromNow < 20f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1063}Your pregnant belly continues to swell, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancydue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
                    var weight = Hero.MainHero.DynamicBodyProperties.Weight;
                    Hero.MainHero.DynamicBodyProperties = new DynamicBodyProperties(Hero.MainHero.DynamicBodyProperties.Age, MBMath.ClampFloat(weight + 0.15f, 0.3f, 1f), Hero.MainHero.DynamicBodyProperties.Build);
                }
                else
                {
                    textObject40 = new TextObject("{=CEEVENTS1064}You're pregnant, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancydue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
                }

                if (CESettings.Instance != null && CESettings.Instance.PregnancyMessages) InformationManager.DisplayMessage(new InformationMessage(textObject40.ToString(), Colors.Gray));
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Failed to handle alerts. pregnancy");
                CECustomHandler.LogMessage(e.Message + " : " + e);
            }
        }

        // Checks
        private void CheckOffspringsToDeliver(Pregnancy pregnancy)
        {
            try
            {
                if (!pregnancy.Mother.IsAlive)
                {
                    pregnancy.AlreadyOccured = true;

                    return;
                }

                if (pregnancy.DueDate.IsFuture) return;
                var pregnancyModel = Campaign.Current.Models.PregnancyModel;

                var mother = pregnancy.Mother;
                var flag = MBRandom.RandomFloat <= pregnancyModel.DeliveringTwinsProbability;
                var aliveOffsprings = new List<Hero>();

                var num = flag
                    ? 2
                    : 1;
                var stillbornCount = 0;

                for (var i = 0; i < num; i++)
                    if (MBRandom.RandomFloat > pregnancyModel.StillbirthProbability)
                    {
                        var isOffspringFemale = MBRandom.RandomFloat <= pregnancyModel.DeliveringFemaleOffspringProbability;

                        try
                        {
                            var item = HeroCreator.DeliverOffSpring(pregnancy.Mother, pregnancy.Father, isOffspringFemale, 0);
                            aliveOffsprings.Add(item);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                CECustomHandler.LogMessage("Bad pregnancy Unknown");
                                CECustomHandler.LogMessage(e.Message + " : " + e);
                                var item = HeroCreator.DeliverOffSpring(pregnancy.Mother, pregnancy.Father, false, 0);
                                aliveOffsprings.Add(item);
                            }
                            catch (Exception e2)
                            {
                                CECustomHandler.LogMessage("Bad pregnancy Male");
                                CECustomHandler.LogMessage(e2.Message + " : " + e2);
                                var item = HeroCreator.DeliverOffSpring(pregnancy.Mother, pregnancy.Father, true, 0);
                                aliveOffsprings.Add(item);
                            }
                        }
                    }
                    else
                    {
                        if (mother == Hero.MainHero)
                        {
                            var textObject = new TextObject("{=pw4cUPEn}{MOTHER.LINK} has delivered stillborn.");
                            StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, null, textObject);
                            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
                        }

                        stillbornCount++;
                    }

                if (mother == Hero.MainHero || pregnancy.Father == Hero.MainHero)
                {
                    var textObject = mother == Hero.MainHero
                        ? new TextObject("{=oIA9lkpc}You have given birth to {DELIVERED_CHILDREN}.")
                        : new TextObject("{=CEEVENTS1092}Your captive {MOTHER.NAME} has given birth to {DELIVERED_CHILDREN}.");

                    switch (stillbornCount)
                    {
                        case 2:
                            textObject.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=Sn9a1Aba}two stillborn babies"));

                            break;
                        case 1 when aliveOffsprings.Count == 0:
                            textObject.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=qWLq2y84}a stillborn baby"));

                            break;
                        case 1 when aliveOffsprings.Count == 1:
                            textObject.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=CEEVENTS1168}one healthy and one stillborn baby"));

                            break;
                        case 0 when aliveOffsprings.Count == 1:
                            textObject.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=CEEVENTS1169}a healthy baby"));

                            break;
                        case 0 when aliveOffsprings.Count == 2:
                            textObject.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=CEEVENTS1170}two healthy babies"));

                            break;
                    }

                    StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, null, textObject);
                    InformationManager.AddQuickInformation(textObject);
                }

                // 1.4.2 version
                //if (mother.IsHumanPlayerCharacter || pregnancy.Father == Hero.MainHero)
                //{
                //    for (int i = 0; i < stillbornCount; i++)
                //    {
                //        ChildbirthLogEntry childbirthLogEntry = new ChildbirthLogEntry(mother, null);
                //        LogEntry.AddLogEntry(childbirthLogEntry);
                //        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(null, childbirthLogEntry.GetEncyclopediaText()));
                //    }
                //    foreach (Hero newbornHero in aliveOffsprings)
                //    {
                //        ChildbirthLogEntry childbirthLogEntry2 = new ChildbirthLogEntry(mother, newbornHero);
                //        LogEntry.AddLogEntry(childbirthLogEntry2);
                //        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(newbornHero, childbirthLogEntry2.GetEncyclopediaText()));
                //    }
                //}

                // 1.4.1 Version
                var childbirthLogEntry = new ChildbirthLogEntry(pregnancy.Mother, aliveOffsprings, stillbornCount);
                LogEntry.AddLogEntry(childbirthLogEntry);
                if (mother == Hero.MainHero || pregnancy.Father == Hero.MainHero) Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(aliveOffsprings, childbirthLogEntry.GetEncyclopediaText()));

                mother.IsPregnant = false;
                pregnancy.AlreadyOccured = true;

                pregnancy.Mother.DynamicBodyProperties = new DynamicBodyProperties(pregnancy.Mother.DynamicBodyProperties.Age, MBRandom.RandomFloatRanged(0.4025f, 0.6025f), pregnancy.Mother.DynamicBodyProperties.Build);
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Bad pregnancy");
                CECustomHandler.LogMessage(e.Message + " : " + e);
                var textObject = new TextObject("{=CEEVENTS1008}Error: bad pregnancy in CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                pregnancy.AlreadyOccured = true;
            }
        }

        private void CheckEquipmentToReturn(ReturnEquipment returnEquipment)
        {
            try
            {
                if (returnEquipment.Captive.PartyBelongedToAsPrisoner != null) return;

                foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                {
                    try
                    {
                        var fetchedEquipment = returnEquipment.BattleEquipment.GetEquipmentFromSlot(i);
                        returnEquipment.Captive.BattleEquipment.AddEquipmentToSlotWithoutAgent(i, fetchedEquipment);
                    }
                    catch (Exception) { }

                    try
                    {
                        var fetchedEquipment2 = returnEquipment.CivilianEquipment.GetEquipmentFromSlot(i);
                        returnEquipment.Captive.CivilianEquipment.AddEquipmentToSlotWithoutAgent(i, fetchedEquipment2);
                    }
                    catch (Exception) { }
                }

                returnEquipment.AlreadyOccured = true;
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Bad Equipment");
                CECustomHandler.LogMessage(e.Message + " : " + e);
                var textObject = new TextObject("{=CEEVENTS1009}Error: bad equipment in return equipment list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                returnEquipment.AlreadyOccured = true;
            }
        }

        private bool CheckEventHourly()
        {
            _hoursPassed++;

            if (CESettings.Instance != null && !(_hoursPassed > CESettings.Instance.EventOccuranceCaptor)) return false;
            
            CEContext.notificationEventCheck = true;
            CEContext.notificationCaptorCheck = true;
            _hoursPassed = 0;

            return true;

        }

        public static void AddReturnEquipment(Hero captive, Equipment battleEquipment, Equipment civilianEquipment)
        {
            if (!_returnEquipment.Exists(item => item.Captive == captive)) _returnEquipment.Add(new ReturnEquipment(captive, battleEquipment, civilianEquipment));
        }

        public static bool CheckIfPregnancyExists(Hero pregnantHero)
        {
            return _heroPregnancies.Any(pregnancy => pregnancy.Mother == pregnantHero);
        }

        public static bool ClearPregnancyList()
        {
            try
            {
                _heroPregnancies.ForEach(item =>
                                         {
                                             item.Mother.IsPregnant = false;
                                         });
                _heroPregnancies = new List<Pregnancy>();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim.IsFemale && _heroPregnancies.Any(pregnancy => pregnancy.Mother == victim)) _heroPregnancies.RemoveAll(pregnancy => pregnancy.Mother == victim);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnChildConceivedEvent.AddNonSerializedListener(this, RunOnChildConceived);

            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, RunHourlyTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, RunDailyTick);

            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
        }

        // Data
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_CEheroPregnancies", ref _heroPregnancies);
            dataStore.SyncData("_CEreturnEquipment", ref _returnEquipment);
            dataStore.SyncData("_CEextraVariables", ref _extraVariables);
        }

        private int _hoursPassed;

        private static List<Pregnancy> _heroPregnancies = new List<Pregnancy>();

        private static List<ReturnEquipment> _returnEquipment = new List<ReturnEquipment>();


        public static ExtraVariables ExtraProps => _extraVariables;

        private static ExtraVariables _extraVariables = new ExtraVariables();

        internal class Pregnancy
        {
            public Pregnancy(Hero pregnantHero, Hero father, CampaignTime dueDate)
            {
                Mother = pregnantHero;
                Father = father;
                DueDate = dueDate;
                AlreadyOccured = false;
            }

            [SaveableField(1)] public readonly Hero Mother;

            [SaveableField(2)] public readonly Hero Father;

            [SaveableField(3)] public readonly CampaignTime DueDate;

            [SaveableField(4)] public bool AlreadyOccured;
        }

        internal class ReturnEquipment
        {
            public ReturnEquipment(Hero captive, Equipment battleEquipment, Equipment civilianEquipment)
            {
                Captive = captive;
                var randomElement = new Equipment(false);
                randomElement.FillFrom(battleEquipment, false);
                BattleEquipment = randomElement;
                var randomElement2 = new Equipment(true);
                randomElement2.FillFrom(civilianEquipment, false);
                CivilianEquipment = randomElement2;
                AlreadyOccured = false;
            }

            [SaveableField(1)] public readonly Hero Captive;

            [SaveableField(2)] public readonly Equipment CivilianEquipment;

            [SaveableField(3)] public readonly Equipment BattleEquipment;

            [SaveableField(4)] public bool AlreadyOccured;
        }

        internal class ExtraVariables
        {
            public void ResetVariables()
            {
                Owner = null;
                MenuToSwitchBackTo = null;
                CurrentBackgroundMeshNameToSwitchBackTo = null;

                CEContext.notificationCaptorCheck = false;
                CEContext.notificationEventCheck = false;
                CEContext.notificationCaptorExists = false;
                CEContext.notificationEventExists = false;
            }

            [SaveableField(1)] public Hero Owner;

            [SaveableField(2)] public string MenuToSwitchBackTo;

            [SaveableField(3)] public string CurrentBackgroundMeshNameToSwitchBackTo;
        }
    }
}