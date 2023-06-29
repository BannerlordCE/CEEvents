#define V120

using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;

using static CaptivityEvents.Helper.CEHelper;


namespace CaptivityEvents.CampaignBehaviors
{
    public class CECampaignBehavior : CampaignBehaviorBase
    {
        private readonly Dynamics _dynamics = new();

        #region Events

        private bool LaunchCaptorEvent(CEEvent OverrideEvent = null)
        {
            if (CESettings.Instance?.EventCaptorNotifications ?? true)
            {
                if (notificationCaptorExists || progressEventExists) return false;
            }

            CEEvent returnedEvent;
            if (OverrideEvent == null)
            {
                CharacterObject captive = MobileParty.MainParty.Party.PrisonRoster.GetTroopRoster().GetRandomElement().Character;
                returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);
            }
            else
            {
                returnedEvent = OverrideEvent;
            }

            if (returnedEvent == null) return false;
            notificationCaptorExists = true;

            if (CESettings.Instance?.EventCaptorNotifications ?? true)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(returnedEvent.NotificationName)) new CESubModule().LoadCampaignNotificationTexture(returnedEvent.NotificationName);
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
            else
            {
                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
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

            return true;
        }

        private bool LaunchRandomEvent(CEEvent OverrideEvent = null)
        {
            if (CESettings.Instance?.EventCaptorNotifications ?? true)
            {
                if (notificationEventExists || progressEventExists) return false;
            }

            CEEvent returnedEvent;
            if (OverrideEvent == null)
            {
                returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsRandom();
            }
            else
            {
                returnedEvent = OverrideEvent;
            }

            if (returnedEvent == null) return false;
            notificationEventExists = true;

            if (CESettings.Instance?.EventCaptorNotifications ?? true)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(returnedEvent.NotificationName)) new CESubModule().LoadCampaignNotificationTexture(returnedEvent.NotificationName, 1);
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
            else
            {
                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
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

            return true;
        }

        private CEEvent CheckDelayedCaptorEvent()
        {
            CEEvent eventToFire = null;
            bool shouldFireEvent = delayedEvents.Any(item =>
            {
                if (item.eventName != null && item.eventTime < Campaign.Current.CampaignStartTime.ElapsedHoursUntilNow)
                {
                    CECustomHandler.LogToFile("Firing " + item.eventName);
                    if (item.conditions == true)
                    {
                        string result = CEEventManager.FireSpecificEventPartyLeader(item.eventName, out CEEvent ceEvent, true, item.heroName);
                        switch (result)
                        {
                            case "$FAILEDTOFIND":
                                CECustomHandler.LogToFile("Failed to load event list.");
                                break;

                            case "$EVENTNOTFOUND":
                                CECustomHandler.LogToFile("Event not found.");
                                break;

                            case "$EVENTCONDITIONSNOTMET":
                                CECustomHandler.LogToFile("Event conditions are not met.");
                                break;

                            default:
                                if (result.StartsWith("$"))
                                {
                                    CECustomHandler.LogToFile(result.Substring(1));
                                }
                                else
                                {
                                    eventToFire = ceEvent;
                                    item.hasBeenFired = true;
                                    return true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        eventToFire = CEPersistence.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == item.eventName.ToLower());
                        if (!eventToFire.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                        {
                            eventToFire = null;
                            return false;
                        }
                        item.hasBeenFired = true;
                        return true;
                    }
                }
                return false;
            });

            if (shouldFireEvent)
            {
                delayedEvents.RemoveAll(item => item.hasBeenFired);
                return eventToFire;
            }
            return null;
        }

        private CEEvent CheckDelayedRandomEvent()
        {
            CEEvent eventToFire = null;
            bool shouldFireEvent = delayedEvents.Any(item =>
            {
                if (item.eventName != null && item.eventTime < Campaign.Current.CampaignStartTime.ElapsedHoursUntilNow)
                {
                    CECustomHandler.LogToFile("Firing " + item.eventName);
                    if (item.conditions == true)
                    {
                        string result = CEEventManager.FireSpecificEventRandom(item.eventName, out CEEvent ceEvent, true);
                        switch (result)
                        {
                            case "$FAILEDTOFIND":
                                CECustomHandler.LogToFile("Failed to load event list.");
                                break;

                            case "$EVENTNOTFOUND":
                                CECustomHandler.LogToFile("Event not found.");
                                break;

                            case "$EVENTCONDITIONSNOTMET":
                                CECustomHandler.LogToFile("Event conditions are not met.");
                                break;

                            default:
                                if (result.StartsWith("$"))
                                {
                                    CECustomHandler.LogToFile(result.Substring(1));
                                }
                                else
                                {
                                    eventToFire = ceEvent;
                                    item.hasBeenFired = true;
                                    return true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        eventToFire = CEPersistence.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == item.eventName.ToLower());
                        if (!eventToFire.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                        {
                            eventToFire = null;
                            return false;
                        }
                        item.hasBeenFired = true;
                        return true;
                    }
                }
                return false;
            });

            if (shouldFireEvent)
            {
                delayedEvents.RemoveAll(item => item.hasBeenFired);
                return eventToFire;
            }

            return null;
        }

        private bool CheckEventHourly()
        {
            if (progressEventExists) return false;
            _hoursPassed++;

            float value = CESettings.Instance?.EventOccurrenceRandom ?? 12f;

            try
            {
                if ((CESettings.Instance?.EventCaptorOn ?? true) && PartyBase.MainParty.NumberOfPrisoners > 0)
                {
                    value = CESettings.Instance?.EventOccurrenceCaptor ?? 12f;
                }
            }
            catch (Exception)
            {
                value = CESettings.Instance?.EventOccurrenceRandom ?? 12f;
            }

            if (!(_hoursPassed > value)) return false;
            notificationEventCheck = true;
            notificationCaptorCheck = true;
            _hoursPassed = 0;
            return true;
        }

        #endregion Events

        #region Pregnancy

        private void OnChildConceived(Hero hero)
        {
            try
            {
                if (spouseOne == null && spouseTwo == null) return;
                Hero father = spouseOne == hero
                    ? spouseTwo
                    : spouseOne;
                CECustomHandler.ForceLogToFile("Added " + hero.Name + "'s Pregnancy");
                _heroPregnancies.Add(new Pregnancy(hero, father, CampaignTime.DaysFromNow(CESettings.Instance?.PregnancyDurationInDays ?? 14f)));
                ChangeMenu(0);
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
        public static void ChangeWeight(Hero hero, int stage, float weight = 0.3f)
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

        private static void CalculatePregnancyWeight(Pregnancy pregnancy)
        {
            try
            {
                if (pregnancy == null || pregnancy.AlreadyOccurred) return;

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
                Pregnancy pregnancyDue = _heroPregnancies.Find(pregnancy => pregnancy.Mother == Hero.MainHero);

                if (pregnancyDue == null || pregnancyDue.AlreadyOccurred) return;
                TextObject textObject40;

                if (pregnancyDue.DueDate.RemainingDaysFromNow < 1f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1061}You are about to give birth...");
                }
                else if (pregnancyDue.DueDate.RemainingDaysFromNow < 10f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1062}Your baby begins kicking, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancyDue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
                }
                else if (pregnancyDue.DueDate.RemainingDaysFromNow < 20f)
                {
                    textObject40 = new TextObject("{=CEEVENTS1063}Your pregnant belly continues to swell, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancyDue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    textObject40 = new TextObject("{=CEEVENTS1064}You're pregnant, you have {DAYS_REMAINING} days remaining.");
                    textObject40.SetTextVariable("DAYS_REMAINING", Math.Floor(pregnancyDue.DueDate.RemainingDaysFromNow).ToString(CultureInfo.InvariantCulture));
                }

                if (CESettings.Instance?.PregnancyMessages ?? true) InformationManager.DisplayMessage(new InformationMessage(textObject40.ToString(), Colors.Gray));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to handle alerts. Pregnancy.");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }
        }

        /// <summary>
        /// Behavior Duplicate found In PregnancyCampaignBehavior
        /// </summary>
        /// <param name="pregnancy"></param>
        public static void CheckOffspringsToDeliver(Pregnancy pregnancy)
        {
            try
            {

                PregnancyModel pregnancyModel = Campaign.Current.Models.PregnancyModel;

                Hero mother = pregnancy.Mother;
                bool flag = MBRandom.RandomFloat <= pregnancyModel.DeliveringTwinsProbability;
                List<Hero> aliveOffsprings = new();

                int num = flag ? 2 : 1;
                int stillbornCount = 0;

                for (int i = 0; i < 1; i++)
                {
                    if (MBRandom.RandomFloat > pregnancyModel.StillbirthProbability)
                    {
                        bool isOffspringFemale = MBRandom.RandomFloat <= pregnancyModel.DeliveringFemaleOffspringProbability;

                        try
                        {
                            Hero item = HeroCreator.DeliverOffSpring(pregnancy.Mother, pregnancy.Father, isOffspringFemale);
                            aliveOffsprings.Add(item);
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Bad pregnancy " + (isOffspringFemale ? "Female" : "Male"));
                            CECustomHandler.ForceLogToFile(e.Message + " : " + e);

                            Hero item = HeroCreator.DeliverOffSpring(pregnancy.Mother, pregnancy.Father, !isOffspringFemale);
                            aliveOffsprings.Add(item);
                        }
                    }
                    else
                    {
                        if (mother == Hero.MainHero)
                        {
                            TextObject textObject = new("{=pw4cUPEn}{MOTHER.LINK} has delivered stillborn.");
                            StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, textObject);
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
                    StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, textObject);
                    AddQuickInformation(textObject);
                }

                if (mother.IsHumanPlayerCharacter || pregnancy.Father == Hero.MainHero)
                {
                    for (int i = 0; i < stillbornCount; i++)
                    {
                        ChildbirthLogEntry childbirthLogEntry = new(mother, null);
                        LogEntry.AddLogEntry(childbirthLogEntry);
#if V120
                        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(null, childbirthLogEntry.GetEncyclopediaText(), CampaignTime.Now));
#else
                        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(null, childbirthLogEntry.GetEncyclopediaText()));
#endif
                    }
                    foreach (Hero newbornHero in aliveOffsprings)
                    {
                        ChildbirthLogEntry childbirthLogEntry2 = new(mother, newbornHero);
                        LogEntry.AddLogEntry(childbirthLogEntry2);
#if V120
                        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(newbornHero, childbirthLogEntry2.GetEncyclopediaText(), CampaignTime.Now));
#else
                        Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new ChildBornMapNotification(newbornHero, childbirthLogEntry2.GetEncyclopediaText()));
#endif
                    }
                }

                mother.IsPregnant = false;
                pregnancy.AlreadyOccurred = true;

                ChangeWeight(pregnancy.Mother, 0, MBRandom.RandomFloatRanged(0.4025f, 0.6025f));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Bad pregnancy");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                TextObject textObject = new("{=CEEVENTS1008}Error: bad pregnancy in CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                pregnancy.AlreadyOccurred = true;
            }
        }

        private static bool ShouldFireAlternativeEvent(Pregnancy pregnancy)
        {
            return pregnancy.Mother == Hero.MainHero || pregnancy.Father == Hero.MainHero;
        }

        private static void ConsequencePregnancyEvent(Pregnancy pregnancy)
        {
            List<CEEvent> eventNames = new();

            try
            {
                foreach (CEEvent triggeredEvent in CEPersistence.CEAlternativePregnancyEvents)
                {

                    string conditionMatched = "InvalidEvent";
                    if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                    {
                        if (PlayerCaptivity.IsCaptive)
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                        } 
                        else
                        {
                            conditionMatched = "PlayerNotCaptive";
                        }
                    }
                    else if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                    {
                        if (!PlayerCaptivity.IsCaptive)
                        {
                            conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
                        } 
                        else
                        {
                            conditionMatched = "PlayerCaptive";
                        }
                    }

                    if (conditionMatched != null)
                    {
                        CECustomHandler.LogToFile(conditionMatched);
                        continue;
                    }
                    int weightedChance = 0;

                    try
                    {
                        weightedChance = new CEVariablesLoader().GetIntFromXML(triggeredEvent.WeightedChanceOfOccurring);
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }

                    if (weightedChance == 0) weightedChance = 1;

                    for (int a = weightedChance; a > 0; a--) eventNames.Add(triggeredEvent);
                }

                if (eventNames.Count > 0)
                {
                    int number = HelperMBRandom(0, eventNames.Count);

                    try
                    {
                        CEEvent triggeredEvent = eventNames[number];
                        triggeredEvent.Captive = CharacterObject.PlayerCharacter;
                        triggeredEvent.Pregnancy = pregnancy;
                        GameMenu.ActivateGameMenu(triggeredEvent.Name);
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                    }
                }
                else
                {
                    CheckOffspringsToDeliver(pregnancy);
                }
            }
            catch (Exception)
            {
                CheckOffspringsToDeliver(pregnancy);
                CECustomHandler.LogToFile("ConsequencePregnancyEvent in events Failed.");
            }
        }

        private static void CheckPregnancy(Pregnancy pregnancy)
        {
            try
            {
                if (!pregnancy.Mother.IsAlive)
                {
                    pregnancy.AlreadyOccurred = true;
                }

                if (pregnancy.AlreadyOccurred) return;

                CalculatePregnancyWeight(pregnancy);

                if (pregnancy.DueDate.IsFuture) return;

                if (ShouldFireAlternativeEvent(pregnancy) && CEPersistence.CEAlternativePregnancyEvents.Count > 0)
                {
                    ConsequencePregnancyEvent(pregnancy);
                }
                else
                {
                    CheckOffspringsToDeliver(pregnancy);
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Bad pregnancy");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                TextObject textObject = new("{=CEEVENTS1008}Error: bad pregnancy in CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                pregnancy.AlreadyOccurred = true;
            }
        }

        public static bool CheckIfPregnancyExists(Hero pregnantHero) => _heroPregnancies.Any(pregnancy => pregnancy.Mother == pregnantHero && !pregnancy.AlreadyOccurred);

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

#endregion Pregnancy

        #region Equipment

        private void CheckEquipmentToReturn(ReturnEquipment returnEquipment)
        {
            try
            {
                if (returnEquipment.Captive.IsPrisoner || returnEquipment.Captive.PartyBelongedToAsPrisoner != null) return;

                foreach (EquipmentCustomIndex index in Enum.GetValues(typeof(EquipmentCustomIndex)))
                {
                    EquipmentIndex i = (EquipmentIndex)index;

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

                returnEquipment.AlreadyOccurred = true;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Bad Equipment");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                TextObject textObject = new("{=CEEVENTS1009}Error: bad equipment in return equipment list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                returnEquipment.AlreadyOccurred = true;
            }
        }

        public static void AddReturnEquipment(Hero captive, Equipment battleEquipment, Equipment civilianEquipment)
        {
            if (!_returnEquipment.Exists(item => item.Captive == captive)) _returnEquipment.Add(new ReturnEquipment(captive, battleEquipment, civilianEquipment));
        }

        #endregion Equipment

        public void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim.IsFemale && _heroPregnancies.Any(pregnancy => pregnancy.Mother == victim)) _heroPregnancies.RemoveAll(pregnancy => pregnancy.Mother == victim);
        }

        private void OnHourlyTick()
        {
            if ((CESettings.Instance?.EventCaptorOn ?? true) && Hero.MainHero.IsPartyLeader && CheckEventHourly())
            {
                CECustomHandler.LogToFile("Checking Campaign Events");

                try
                {
                    bool shouldEventsFire = !progressEventExists;

                    if (delayedEvents.Count > 0 && shouldEventsFire)
                    {
                        CEEvent captorEventCheck = CheckDelayedCaptorEvent();
                        CEEvent randomEventCheck = CheckDelayedRandomEvent();

                        if (captorEventCheck != null)
                        {
                            notificationCaptorExists = false;
                            LaunchCaptorEvent(captorEventCheck);
                        }
                        else if (randomEventCheck != null)
                        {
                            notificationEventExists = false;
                            LaunchCaptorEvent(randomEventCheck);
                        }
                        else
                        {
                            shouldEventsFire = true;
                        }
                    }

                    if (shouldEventsFire)
                    {
                        if (MobileParty.MainParty.Party.PrisonRoster.Count > 0)
                        {
                            if (CESettings.Instance?.EventRandomEnabled ?? true)
                            {
                                int randomNumber = MBRandom.RandomInt(100);

                                if (randomNumber < (CESettings.Instance?.EventRandomFireChance ?? 20f) && !LaunchRandomEvent()) LaunchCaptorEvent();
                                if (randomNumber > (CESettings.Instance?.EventRandomFireChance ?? 20f) && !LaunchCaptorEvent()) LaunchRandomEvent();
                            }
                            else
                            {
                                LaunchCaptorEvent();
                            }
                        }
                        else if (CESettings.Instance?.EventRandomEnabled ?? true)
                        {
                            LaunchRandomEvent();
                        }
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("CheckEventHourly Failure");
                    CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                }
            }

            // Pregnancies
            try
            {
                _heroPregnancies.ForEach(item =>
                {
                    if (item.Mother != null && !item.AlreadyOccurred)
                    {
                        item.Mother.IsPregnant = true;
                        CheckPregnancy(item);
                    }
                    else
                    {
                        item.AlreadyOccurred = true;
                        ChangeMenu(0);
                    }
                });
                _heroPregnancies.RemoveAll(item => item.AlreadyOccurred);
            }
            catch (Exception e)
            {
                TextObject textObject = new("{=CEEVENTS1007}Error: resetting the CE pregnancy list");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                _heroPregnancies = new List<Pregnancy>();
                CECustomHandler.ForceLogToFile("Failed _heroPregnancies ForEach");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }

            // Gear
            if (CESettings.Instance?.EventCaptorGearCaptives ?? true)
            {
                try
                {
                    _returnEquipment.ForEach(CheckEquipmentToReturn);

                    _returnEquipment.RemoveAll(item => item.AlreadyOccurred);
                }
                catch (Exception e)
                {
                    TextObject textObject = new("{=CEEVENTS1006}Error: resetting the return equipment list");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Black));
                    _returnEquipment = new List<ReturnEquipment>();
                    CECustomHandler.ForceLogToFile("Failed _returnEquipment ForEach");
                    CECustomHandler.ForceLogToFile(e.Message + " : " + e);
                }
            }
        }

        private void OnHeroPrisonerReleased(Hero prisoner, PartyBase party, IFaction capturerFaction, EndCaptivityDetail detail)
        {
            CESkills.NodeSkills.ForEach((CESkillNode skillNode) =>
            {
                if (skillNode.SetZeroOnEscape)
                {
                    _dynamics.ResetCustomSkill(prisoner, skillNode.Id, false);
                }
            });
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnChildConceivedEvent.AddNonSerializedListener(this, OnChildConceived);

            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
            CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener(this, OnHeroPrisonerReleased);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);

            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_CEHeroPregnancies", ref _heroPregnancies);
            dataStore.SyncData("_CEReturnEquipment", ref _returnEquipment);
            dataStore.SyncData("_CEExtraVariables", ref _extraVariables);
        }

        public static void ResetFullData()
        {
            _extraVariables = new ExtraVariables();
            _extraVariables.ResetVariables();
            _returnEquipment = new List<ReturnEquipment>();
            _heroPregnancies = new List<Pregnancy>();
        }

        public static List<Pregnancy> HeroPregnancies => _heroPregnancies;

        public static List<ReturnEquipment> HeroReturnEquipment => _returnEquipment;

        private int _hoursPassed;

        private static List<Pregnancy> _heroPregnancies = new();

        private static List<ReturnEquipment> _returnEquipment = new();

        public static ExtraVariables ExtraProps => _extraVariables;

        private static ExtraVariables _extraVariables = new();

        public class Pregnancy
        {
            public Pregnancy(Hero pregnantHero, Hero father, CampaignTime dueDate)
            {
                Mother = pregnantHero;
                Father = father;
                DueDate = dueDate;
                AlreadyOccurred = false;
            }

            [SaveableField(1)]
            public readonly Hero Mother;

            [SaveableField(2)]
            public readonly Hero Father;

            [SaveableField(3)]
            public readonly CampaignTime DueDate;

            [SaveableField(4)]
            public bool AlreadyOccurred;
        }

        public class ReturnEquipment
        {
            public ReturnEquipment(Hero captive, Equipment battleEquipment, Equipment civilianEquipment)
            {
                Captive = captive;
                Equipment randomElement = new(false);
                randomElement.FillFrom(battleEquipment, false);
                BattleEquipment = randomElement;
                Equipment randomElement2 = new(true);
                randomElement2.FillFrom(civilianEquipment, false);
                CivilianEquipment = randomElement2;
                AlreadyOccurred = false;
            }

            [SaveableField(1)]
            public readonly Hero Captive;

            [SaveableField(2)]
            public readonly Equipment CivilianEquipment;

            [SaveableField(3)]
            public readonly Equipment BattleEquipment;

            [SaveableField(4)]
            public bool AlreadyOccurred;
        }

        public class ExtraVariables
        {
            public void ResetVariables()
            {
                Owner = null;
                menuToSwitchBackTo = null;
                currentBackgroundMeshNameToSwitchBackTo = null;

                notificationCaptorCheck = false;
                notificationEventCheck = false;
                notificationCaptorExists = false;
                notificationEventExists = false;

                progressEventExists = false;
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