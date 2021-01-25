using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using Helpers;
using MountAndBlade.CampaignBehaviors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        #region Events

        private void LaunchCaptorEvent()
        {
            if (CEHelper.notificationCaptorExists || CEHelper.progressEventExists) return;
            CharacterObject captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
            CEEvent returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);

            if (returnedEvent == null) return;
            returnedEvent.Captive = captive;
            CEHelper.notificationCaptorExists = true;

            try
            {
                if (!returnedEvent.NotificationName.IsStringNoneOrEmpty()) new CESubModule().LoadCampaignNotificationTexture(returnedEvent.NotificationName);
                else if (returnedEvent.SexualContent) new CESubModule().LoadCampaignNotificationTexture("CE_sexual_notification");
                else new CESubModule().LoadCampaignNotificationTexture("CE_castle_notification");
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));

                CECustomHandler.ForceLogToFile("LoadCampaignNotificationTexture");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }

            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new CECaptorMapNotification(returnedEvent, new TextObject("{=CEEVENTS1090}Captor event is ready")));

        }

        private void LaunchRandomEvent()
        {
            if (CEHelper.notificationEventExists || CEHelper.progressEventExists) return;
            CEEvent returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();
            if (returnedEvent == null) return;
            CEHelper.notificationEventExists = true;

            try
            {
                if (!returnedEvent.NotificationName.IsStringNoneOrEmpty()) new CESubModule().LoadCampaignNotificationTexture(returnedEvent.NotificationName, 1);
                else if (returnedEvent.SexualContent) new CESubModule().LoadCampaignNotificationTexture("CE_random_sexual_notification", 1);
                else new CESubModule().LoadCampaignNotificationTexture("CE_random_notification", 1);
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));

                CECustomHandler.ForceLogToFile("LoadCampaignNotificationTexture");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }

            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new CEEventMapNotification(returnedEvent, new TextObject("{=CEEVENTS1059}Random event is ready")));
        }

        private bool CheckEventHourly()
        {
            if (CEHelper.progressEventExists) return false;
            _hoursPassed++;

            if (CESettings.Instance == null) return false;
            if (!(_hoursPassed > CESettings.Instance.EventOccurrenceCaptor)) return false;
            CEHelper.notificationEventCheck = true;
            CEHelper.notificationCaptorCheck = true;
            _hoursPassed = 0;
            return true;
        }


        #endregion

        #region Pregnancy

        private void OnChildConceived(Hero hero)
        {
            try
            {
                if (CEHelper.spouseOne == null && CEHelper.spouseTwo == null) return;

                Hero father = CEHelper.spouseOne == hero
                    ? CEHelper.spouseTwo
                    : CEHelper.spouseOne;
                CECustomHandler.LogToFile("Added " + hero.Name + "'s Pregenancy");
                if (CESettings.Instance != null) _heroPregnancies.Add(new Pregnancy(hero, father, CampaignTime.DaysFromNow(CESettings.Instance.PregnancyDurationInDays)));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to handle OnChildConceivedEvent.");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }
        }

        /// <summary>
        /// Change Weight
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="hero"></param>
        private void ChangeWeight(Hero hero, int stage, float weight = 0.3f)
        {

            if (stage != 0) weight = hero.Weight;

            switch (stage)
            {
                case 1:
                    weight = MBMath.ClampFloat(weight + 0.01f, 0.3f, 0.7f);
                    break;
                case 2:
                    weight = MBMath.ClampFloat(weight + 0.01f, 0.3f, 0.95f);
                    break;
                case 3:
                    weight = 1f;
                    break;
                default:
                    break;
            }

            hero.Weight = weight;
        }

        private void CalculatePregnancyWeight(Pregnancy pregnancy)
        {
            try
            {
                if (pregnancy == null || pregnancy.AlreadyOccured) return;

                if (pregnancy.DueDate.RemainingDaysFromNow < 1f)
                {
                    ChangeWeight(pregnancy.Mother, 3);
                }
                else if (pregnancy.DueDate.RemainingDaysFromNow < 10f)
                {
                    ChangeWeight(pregnancy.Mother, 2);
                }
                else if (pregnancy.DueDate.RemainingDaysFromNow < 20f)
                {
                    ChangeWeight(pregnancy.Mother, 1);
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to handle alerts. CalculatePregnancyWeight");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }
        }

        public void OnDailyTick()
        {
            try
            {
                if (!Hero.MainHero.IsPregnant) return;
                Pregnancy pregnancydue = _heroPregnancies.Find(pregnancy => pregnancy.Mother == Hero.MainHero);

                if (pregnancydue == null || pregnancydue.AlreadyOccured) return;
                TextObject textObject40;

                if (pregnancydue.DueDate.RemainingDaysFromNow < 1f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1061}You are about to give birth...");
                }
                else if (pregnancydue.DueDate.RemainingDaysFromNow < 10f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1062}Your baby begins kicking, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancydue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
                }
                else if (pregnancydue.DueDate.RemainingDaysFromNow < 20f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1063}Your pregnant belly continues to swell, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancydue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
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
                CECustomHandler.ForceLogToFile("Failed to handle alerts. pregenancy");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }
        }

        /// <summary>
        /// Behaviour Duplicate found In PregnancyCampaignBehavior 
        /// </summary>
        /// <param name="mother"></param>
        /// <param name="father"></param>
        /// <param name="isOffspringFemale"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        private Hero DeliverOffSpring(Hero mother, Hero father, bool isOffspringFemale, int age = 1)
        {
            CharacterObject characterObject = isOffspringFemale ? mother.CharacterObject : father.CharacterObject;
            characterObject.Culture = Hero.MainHero.Culture;

            // Reflection One
            MethodInfo mi = typeof(HeroCreator).GetMethod("CreateNewHero", BindingFlags.NonPublic | BindingFlags.Static);
            if (mi == null) return HeroCreator.DeliverOffSpring(mother, father, isOffspringFemale, null, 0);
            Hero hero = (Hero)mi.Invoke(null, new object[] { characterObject, age });

            int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            CharacterObject characterObject2 = CharacterObject.ChildTemplates.FirstOrDefault((CharacterObject t) => t.Culture == mother.Culture && t.Age <= becomeChildAge && t.IsFemale == isOffspringFemale && t.Occupation == Occupation.Lord);

            if (characterObject2 != null)
            {
                Equipment equipment = characterObject2.FirstCivilianEquipment.Clone(false);
                Equipment equipment2 = new Equipment(false);
                // TaleWorld's Bug
                if (hero.BattleEquipment == null)
                {
                    PropertyInfo fi = hero.GetType().GetProperty("BattleEquipment", BindingFlags.Instance | BindingFlags.Public);
                    if (fi != null) fi.SetValue(hero, new Equipment(false));
                }
                if (hero.CivilianEquipment == null)
                {
                    PropertyInfo fi = hero.GetType().GetProperty("CivilianEquipment", BindingFlags.Instance | BindingFlags.Public);
                    if (fi != null) fi.SetValue(hero, new Equipment(true));
                }
                equipment2.FillFrom(equipment, false);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment2);
            }

            hero.FirstName = NameGenerator.Current.GenerateHeroFirstName(hero, true);
            hero.CharacterObject.Name = hero.FirstName;

            // Reflection Two
            mi = hero.HeroDeveloper.GetType().GetMethod("CheckInitialLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null) mi.Invoke(hero.HeroDeveloper, new object[] { });

            Campaign.Current.GetCampaignBehavior<IHeroCreationCampaignBehavior>().DeriveSkillsFromTraits(hero, characterObject);
            hero.CharacterObject.IsFemale = isOffspringFemale;

            if (hero.CharacterObject.Occupation != Occupation.Lord)
            {
                PropertyInfo fi = hero.CharacterObject.GetType().GetProperty("Occupation", BindingFlags.Instance | BindingFlags.Public);
                if (fi != null) fi.SetValue(hero.CharacterObject, Occupation.Lord);
            }

            BodyProperties bodyPropertiesMin = mother.CharacterObject.GetBodyPropertiesMin(false);
            BodyProperties bodyPropertiesMin2 = father.CharacterObject.GetBodyPropertiesMin(false);
            int seed = isOffspringFemale ? mother.CharacterObject.GetDefaultFaceSeed(1) : father.CharacterObject.GetDefaultFaceSeed(1);
            string hairTags = isOffspringFemale ? mother.CharacterObject.HairTags : father.CharacterObject.HairTags;
            string tattooTags = isOffspringFemale ? mother.CharacterObject.TattooTags : father.CharacterObject.TattooTags;

            // 1.5.6
            // hero.CharacterObject.StaticBodyPropertiesMin = BodyProperties.GetRandomBodyProperties(isOffspringFemale, bodyPropertiesMin, bodyPropertiesMin2, 1, seed, hairTags, father.CharacterObject.BeardTags, tattooTags).StaticProperties;

            // 1.5.7
            
            PropertyInfo pi = hero.GetType().GetProperty("StaticBodyProperties", BindingFlags.Instance | BindingFlags.NonPublic);
            StaticBodyProperties staticBody = BodyProperties.GetRandomBodyProperties(isOffspringFemale, bodyPropertiesMin, bodyPropertiesMin2, 1, seed, hairTags, father.CharacterObject.BeardTags, tattooTags).StaticProperties;
            if (pi != null) pi.SetValue(hero, staticBody);
            

            hero.Mother = mother;
            hero.Father = father;

            // Reflection Two
            MethodInfo mi2 = typeof(HeroCreator).GetMethod("DecideBornSettlement", BindingFlags.NonPublic | BindingFlags.Static);
            if (mi == null) return HeroCreator.DeliverOffSpring(mother, father, isOffspringFemale, null, 0);
            hero.BornSettlement = (Settlement)mi2.Invoke(null, new object[] { hero });

            hero.IsNoble = true;

            if (mother == Hero.MainHero || father == Hero.MainHero)
            {
                hero.HasMet = true;
                hero.Clan = Clan.PlayerClan;
            }
            else
            {
                hero.Clan = father.Clan;
            }
            CampaignEventDispatcher.Instance.OnHeroCreated(hero, true);
            return hero;
        }

        /// <summary>
        /// Behaviour Duplicate found In PregnancyCampaignBehavior 
        /// </summary>
        /// <param name="pregnancy"></param>
        private void CheckOffspringsToDeliver(Pregnancy pregnancy)
        {
            try
            {
                if (!pregnancy.Mother.IsAlive)
                {
                    pregnancy.AlreadyOccured = true;
                    return;
                }

                CalculatePregnancyWeight(pregnancy);

                if (pregnancy.DueDate.IsFuture) return;
                PregnancyModel pregnancyModel = Campaign.Current.Models.PregnancyModel;

                Hero mother = pregnancy.Mother;
                bool flag = MBRandom.RandomFloat <= pregnancyModel.DeliveringTwinsProbability;
                List<Hero> aliveOffsprings = new List<Hero>();

                int num = flag ? 2 : 1;
                int stillbornCount = 0;

                for (int i = 0; i < num; i++)
                {
                    if (MBRandom.RandomFloat > pregnancyModel.StillbirthProbability)
                    {
                        bool isOffspringFemale = MBRandom.RandomFloat <= pregnancyModel.DeliveringFemaleOffspringProbability;

                        try
                        {
                            Hero item = DeliverOffSpring(pregnancy.Mother, pregnancy.Father, isOffspringFemale, 0);
                            aliveOffsprings.Add(item);
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Bad pregnancy " + (isOffspringFemale ? "Female" : "Male"));
                            CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                            Hero item = HeroCreator.DeliverOffSpring(pregnancy.Mother, pregnancy.Father, !isOffspringFemale, null, 0);
                            aliveOffsprings.Add(item);
                        }
                    }
                    else
                    {
                        if (mother == Hero.MainHero)
                        {
                            TextObject textObject = new TextObject("{=pw4cUPEn}{MOTHER.LINK} has delivered stillborn.");
                            StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, null, textObject);
                            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
                        }

                        stillbornCount++;
                    }
                }

                if (mother == Hero.MainHero || pregnancy.Father == Hero.MainHero)
                {
                    TextObject textObject = mother == Hero.MainHero
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

                if (mother.IsHumanPlayerCharacter || pregnancy.Father == Hero.MainHero)
                {
                    for (int i = 0; i < stillbornCount; i++)
                    {
                        ChildbirthLogEntry childbirthLogEntry = new ChildbirthLogEntry(mother, null);
                        LogEntry.AddLogEntry(childbirthLogEntry);
                        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(null, childbirthLogEntry.GetEncyclopediaText()));
                    }
                    foreach (Hero newbornHero in aliveOffsprings)
                    {
                        ChildbirthLogEntry childbirthLogEntry2 = new ChildbirthLogEntry(mother, newbornHero);
                        LogEntry.AddLogEntry(childbirthLogEntry2);
                        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(newbornHero, childbirthLogEntry2.GetEncyclopediaText()));
                    }
                }

                mother.IsPregnant = false;
                pregnancy.AlreadyOccured = true;

                ChangeWeight(pregnancy.Mother, 0, MBRandom.RandomFloatRanged(0.4025f, 0.6025f));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Bad pregnancy");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                TextObject textObject = new TextObject("{=CEEVENTS1008}Error: bad pregnancy in CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                pregnancy.AlreadyOccured = true;
            }
        }

        public static bool CheckIfPregnancyExists(Hero pregnantHero) => _heroPregnancies.Any(pregnancy => pregnancy.Mother == pregnantHero);

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

        #endregion

        #region Equipment
        private void CheckEquipmentToReturn(ReturnEquipment returnEquipment)
        {
            try
            {
                if (returnEquipment.Captive.IsPrisoner || returnEquipment.Captive.PartyBelongedToAsPrisoner != null) return;

                foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                {
                    try
                    {
                        EquipmentElement fetchedEquipment = returnEquipment.BattleEquipment.GetEquipmentFromSlot(i);
                        returnEquipment.Captive.BattleEquipment.AddEquipmentToSlotWithoutAgent(i, fetchedEquipment);
                    }
                    catch (Exception) { }

                    try
                    {
                        EquipmentElement fetchedEquipment2 = returnEquipment.CivilianEquipment.GetEquipmentFromSlot(i);
                        returnEquipment.Captive.CivilianEquipment.AddEquipmentToSlotWithoutAgent(i, fetchedEquipment2);
                    }
                    catch (Exception) { }
                }

                returnEquipment.AlreadyOccured = true;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Bad Equipment");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                TextObject textObject = new TextObject("{=CEEVENTS1009}Error: bad equipment in return equipment list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                returnEquipment.AlreadyOccured = true;
            }
        }

        public static void AddReturnEquipment(Hero captive, Equipment battleEquipment, Equipment civilianEquipment)
        {
            if (!_returnEquipment.Exists(item => item.Captive == captive)) _returnEquipment.Add(new ReturnEquipment(captive, battleEquipment, civilianEquipment));
        }

        #endregion

        public void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim.IsFemale && _heroPregnancies.Any(pregnancy => pregnancy.Mother == victim)) _heroPregnancies.RemoveAll(pregnancy => pregnancy.Mother == victim);
        }

        private void OnHourlyTick()
        {
            if (CESettings.Instance.EventCaptorOn && Hero.MainHero.IsPartyLeader && CheckEventHourly())
            {
                CECustomHandler.LogToFile("Checking Campaign Events");

                try
                {
                    if (MobileParty.MainParty.Party.PrisonRoster.Count > 0)
                    {
                        if (CampaignOptions.IsMapNotificationsEnabled && CESettings.Instance.EventCaptorNotifications)
                        {
                            if (CESettings.Instance.EventRandomEnabled && (!CEHelper.notificationEventExists || !CEHelper.notificationCaptorExists))
                            {
                                int randomNumber = MBRandom.RandomInt(100);

                                if (!CEHelper.notificationEventExists && randomNumber < CESettings.Instance.EventRandomFireChance) LaunchRandomEvent();
                                else if (!CEHelper.notificationCaptorExists && randomNumber > CESettings.Instance.EventRandomFireChance) LaunchCaptorEvent();
                            }
                            else
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
                                            CharacterObject captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                                            returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);
                                        }
                                    }
                                    else
                                    {
                                        CharacterObject captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                                        returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);
                                        if (returnedEvent != null) returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();
                                    }
                                }
                                else
                                {
                                    CharacterObject captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                                    returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);
                                }

                                if (returnedEvent != null)
                                {
                                    Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                                    if (!mapState.AtMenu)
                                    {
                                        _extraVariables.menuToSwitchBackTo = null;
                                        _extraVariables.currentBackgroundMeshNameToSwitchBackTo = null;
                                        GameMenu.ActivateGameMenu("prisoner_wait");
                                    }
                                    else
                                    {
                                        _extraVariables.menuToSwitchBackTo = mapState.GameMenuId;
                                        _extraVariables.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                                    }

                                    GameMenu.SwitchToMenu(returnedEvent.Name);
                                }
                            }
                        }
                    }
                    else if (CESettings.Instance.EventRandomEnabled)
                    {
                        if (CampaignOptions.IsMapNotificationsEnabled && CESettings.Instance.EventCaptorNotifications)
                        {
                            LaunchRandomEvent();
                        }
                        else
                        {
                            if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                            {
                                CEEvent returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();

                                if (returnedEvent != null)
                                {
                                    Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                                    if (!mapState.AtMenu)
                                    {
                                        _extraVariables.menuToSwitchBackTo = null;
                                        _extraVariables.currentBackgroundMeshNameToSwitchBackTo = null;
                                        GameMenu.ActivateGameMenu("prisoner_wait");
                                    }
                                    else
                                    {
                                        _extraVariables.menuToSwitchBackTo = mapState.GameMenuId;
                                        _extraVariables.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                                    }

                                    GameMenu.SwitchToMenu(returnedEvent.Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("CheckEventHourly Failure");
                    CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                }
            }

            try
            {
                _heroPregnancies.ForEach(item =>
                {
                    if (item.Mother != null)
                    {
                        item.Mother.IsPregnant = true;
                        CheckOffspringsToDeliver(item);
                    }
                    else
                    {
                        item.AlreadyOccured = true;
                    }
                });

                _heroPregnancies.RemoveAll(item => item.AlreadyOccured);
            }
            catch (Exception e)
            {
                TextObject textObject = new TextObject("{=CEEVENTS1007}Error: resetting the CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                _heroPregnancies = new List<Pregnancy>();
                CECustomHandler.ForceLogToFile("Failed _heroPregnancies ForEach");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }

            if (CESettings.Instance.EventCaptorGearCaptives)
            {
                try
                {
                    _returnEquipment.ForEach(CheckEquipmentToReturn);

                    _returnEquipment.RemoveAll(item => item.AlreadyOccured);
                }
                catch (Exception e)
                {
                    TextObject textObject = new TextObject("{=CEEVENTS1006}Error: resetting the return equipment list");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                    _returnEquipment = new List<ReturnEquipment>();
                    CECustomHandler.ForceLogToFile("Failed _returnEquipment ForEach");
                    CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                }
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnChildConceivedEvent.AddNonSerializedListener(this, OnChildConceived);

            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);

            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_CEheroPregnancies", ref _heroPregnancies);
            dataStore.SyncData("_CEreturnEquipment", ref _returnEquipment);
            dataStore.SyncData("_CEextraVariables", ref _extraVariables);
        }

        public static void ResetFullData()
        {
            _extraVariables = new ExtraVariables();
            _extraVariables.ResetVariables();
            _returnEquipment = new List<ReturnEquipment>();
            _heroPregnancies = new List<Pregnancy>();
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

            [SaveableField(1)]
            public readonly Hero Mother;

            [SaveableField(2)]
            public readonly Hero Father;

            [SaveableField(3)]
            public readonly CampaignTime DueDate;

            [SaveableField(4)]
            public bool AlreadyOccured;
        }

        internal class ReturnEquipment
        {
            public ReturnEquipment(Hero captive, Equipment battleEquipment, Equipment civilianEquipment)
            {
                Captive = captive;
                Equipment randomElement = new Equipment(false);
                randomElement.FillFrom(battleEquipment, false);
                BattleEquipment = randomElement;
                Equipment randomElement2 = new Equipment(true);
                randomElement2.FillFrom(civilianEquipment, false);
                CivilianEquipment = randomElement2;
                AlreadyOccured = false;
            }

            [SaveableField(1)]
            public readonly Hero Captive;

            [SaveableField(2)]
            public readonly Equipment CivilianEquipment;

            [SaveableField(3)]
            public readonly Equipment BattleEquipment;

            [SaveableField(4)]
            public bool AlreadyOccured;
        }

        internal class ExtraVariables
        {
            public void ResetVariables()
            {
                Owner = null;
                menuToSwitchBackTo = null;
                currentBackgroundMeshNameToSwitchBackTo = null;

                CEHelper.notificationCaptorCheck = false;
                CEHelper.notificationEventCheck = false;
                CEHelper.notificationCaptorExists = false;
                CEHelper.notificationEventExists = false;

                CEHelper.progressEventExists = false;
            }

            [SaveableField(1)]
            public Hero Owner;

            [SaveableField(2)]
            public string menuToSwitchBackTo;

            [SaveableField(3)]
            public string currentBackgroundMeshNameToSwitchBackTo;
        }
    }
}