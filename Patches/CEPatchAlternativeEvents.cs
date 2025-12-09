#define V127

using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static CaptivityEvents.Helper.CEHelper;

namespace CaptivityEvents.Patches
{
    /// <summary>
    /// Patches for DeathAlternative, MarriageAlternative, and DesertionAlternative events
    /// These allow custom events to fire instead of or alongside vanilla death, marriage, and desertion
    /// </summary>
    [HarmonyPatch]
    internal static class CEPatchAlternativeEvents
    {
        // Static storage for pending alternative events
        private static Hero _pendingDeathVictim;
        private static Hero _pendingDeathKiller;
        private static KillCharacterAction.KillCharacterActionDetail _pendingDeathDetail;
        private static bool _deathEventPending;

        private static Hero _pendingMarriageHero1;
        private static Hero _pendingMarriageHero2;
        private static bool _marriageEventPending;

        private static Hero _pendingDesertionHero;
        private static bool _desertionEventPending;

        #region Death Alternative

        /// <summary>
        /// Prefix patch for KillCharacterAction.ApplyInternal to intercept deaths
        /// </summary>
        [HarmonyPatch(typeof(KillCharacterAction), "ApplyInternal")]
        [HarmonyPrefix]
        private static bool KillCharacterActionPrefix(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail actionDetail, bool showNotification)
        {
            // Don't intercept if alternative events are disabled or already processing
            if (_deathEventPending) return true;

            // Only intercept deaths involving heroes we care about
            if (victim == null) return true;

            if (victim != Hero.MainHero) return true;

            // Check if there's a death alternative event available
            CEEvent deathEvent = ReturnWeightedChoiceOfEventsAlternative(RestrictedListOfFlags.DeathAlternative, victim, killer);

            if (deathEvent == null) return true; // No alternative event, proceed with normal death

            // Store pending death info
            _pendingDeathVictim = victim;
            _pendingDeathKiller = killer;
            _pendingDeathDetail = actionDetail;
            _deathEventPending = true;

            // Launch the alternative event
            try
            {
                LaunchAlternativeEvent(deathEvent, "{=CEEVENTS1101}A death alternative event is ready");
                CECustomHandler.LogToFile($"Death Alternative Event triggered for {victim.Name}");
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("DeathAlternative Failure: " + e.Message);
                _deathEventPending = false;
                return true; // On error, proceed with normal death
            }

            return false; // Block the original death action
        }

        /// <summary>
        /// Proceeds with the original death after alternative event completes
        /// </summary>
        public static void ProceedWithDeath()
        {
            if (!_deathEventPending || _pendingDeathVictim == null) return;

            _deathEventPending = false;
            Hero victim = _pendingDeathVictim;
            Hero killer = _pendingDeathKiller;

            _pendingDeathVictim = null;
            _pendingDeathKiller = null;

            // Execute the original death
            try
            {
                _deathEventPending = true; // Set flag to prevent recursion
                KillCharacterAction.ApplyByMurder(victim, killer);
            }
            finally
            {
                _deathEventPending = false;
            }
        }

        /// <summary>
        /// Cancels the pending death (hero survives)
        /// </summary>
        public static void CancelDeath()
        {
            _deathEventPending = false;
            _pendingDeathVictim = null;
            _pendingDeathKiller = null;
            CECustomHandler.LogToFile("Death was cancelled by alternative event");
        }

        public static Hero GetPendingDeathVictim() => _pendingDeathVictim;
        public static Hero GetPendingDeathKiller() => _pendingDeathKiller;
        public static bool IsDeathPending() => _deathEventPending;

        #endregion

        #region Marriage Alternative

        /// <summary>
        /// Prefix patch for MarriageAction.Apply to intercept marriages
        /// </summary>
        [HarmonyPatch(typeof(MarriageAction), "Apply")]
        [HarmonyPrefix]
        private static bool MarriageActionPrefix(Hero firstHero, Hero secondHero, bool showNotification)
        {
            // Don't intercept if already processing
            if (_marriageEventPending) return true;

            if (firstHero == null || secondHero == null) return true;

            // Only intercept if player is one of the people getting married
            if (firstHero != Hero.MainHero && secondHero != Hero.MainHero) return true;

            // Verify neither hero is already married (this would indicate some other action)
            if (firstHero.Spouse != null || secondHero.Spouse != null) return true;

            // Check if there's a marriage alternative event available
            Hero otherHero = firstHero == Hero.MainHero ? secondHero : firstHero;
            CEEvent marriageEvent = ReturnWeightedChoiceOfEventsAlternative(RestrictedListOfFlags.MarriageAlternative, otherHero, null);

            if (marriageEvent == null) return true; // No alternative event, proceed with normal marriage

            // Store pending marriage info
            _pendingMarriageHero1 = firstHero;
            _pendingMarriageHero2 = secondHero;
            _marriageEventPending = true;

            // Launch the alternative event
            try
            {
                LaunchAlternativeEvent(marriageEvent, "{=CEEVENTS1102}A marriage alternative event is ready");
                CECustomHandler.LogToFile($"Marriage Alternative Event triggered for {firstHero.Name} and {secondHero.Name}");
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("MarriageAlternative Failure: " + e.Message);
                _marriageEventPending = false;
                return true; // On error, proceed with normal marriage
            }

            return false; // Block the original marriage action
        }

        /// <summary>
        /// Proceeds with the original marriage after alternative event completes
        /// </summary>
        public static void ProceedWithMarriage()
        {
            if (!_marriageEventPending || _pendingMarriageHero1 == null || _pendingMarriageHero2 == null) return;

            _marriageEventPending = false;
            Hero hero1 = _pendingMarriageHero1;
            Hero hero2 = _pendingMarriageHero2;

            _pendingMarriageHero1 = null;
            _pendingMarriageHero2 = null;

            // Execute the original marriage
            try
            {
                _marriageEventPending = true; // Set flag to prevent recursion
                MarriageAction.Apply(hero1, hero2);
            }
            finally
            {
                _marriageEventPending = false;
            }
        }

        /// <summary>
        /// Cancels the pending marriage
        /// </summary>
        public static void CancelMarriage()
        {
            _marriageEventPending = false;
            _pendingMarriageHero1 = null;
            _pendingMarriageHero2 = null;
            CECustomHandler.LogToFile("Marriage was cancelled by alternative event");
        }

        public static Hero GetPendingMarriageHero1() => _pendingMarriageHero1;
        public static Hero GetPendingMarriageHero2() => _pendingMarriageHero2;
        public static bool IsMarriagePending() => _marriageEventPending;

        #endregion

        #region Desertion Alternative

        /// <summary>
        /// Prefix patch for RemoveCompanionAction.ApplyInternal to intercept companion leaving
        /// </summary>
        [HarmonyPatch(typeof(RemoveCompanionAction), "ApplyInternal")]
        [HarmonyPrefix]
        private static bool RemoveCompanionActionPrefix(Clan clan, Hero companion)
        {
            // Don't intercept if already processing
            if (_desertionEventPending) return true;

            if (companion == null || clan == null) return true;

            // Only intercept if it's the player's clan
            if (clan != Clan.PlayerClan) return true;

            // Verify companion is actually a hero (not a troop)
            if (!companion.IsAlive) return true;

            // Verify companion is actually a companion in the player's clan
            if (!Clan.PlayerClan.Companions.Contains(companion)) return true;

            // Check if there's a desertion alternative event available
            CEEvent desertionEvent = ReturnWeightedChoiceOfEventsAlternative(RestrictedListOfFlags.DesertionAlternative, companion, null);

            if (desertionEvent == null) return true; // No alternative event, proceed with normal desertion

            // Store pending desertion info
            _pendingDesertionHero = companion;
            _desertionEventPending = true;

            // Launch the alternative event
            try
            {
                LaunchAlternativeEvent(desertionEvent, "{=CEEVENTS1103}A companion is considering leaving");
                CECustomHandler.LogToFile($"Desertion Alternative Event triggered for {companion.Name}");
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("DesertionAlternative Failure: " + e.Message);
                _desertionEventPending = false;
                return true; // On error, proceed with normal desertion
            }

            return false; // Block the original desertion action
        }

        /// <summary>
        /// Proceeds with the original desertion after alternative event completes
        /// </summary>
        public static void ProceedWithDesertion()
        {
            if (!_desertionEventPending || _pendingDesertionHero == null) return;

            _desertionEventPending = false;
            Hero companion = _pendingDesertionHero;

            _pendingDesertionHero = null;

            // Execute the original desertion
            try
            {
                _desertionEventPending = true; // Set flag to prevent recursion
                RemoveCompanionAction.ApplyByFire(Clan.PlayerClan, companion);
            }
            finally
            {
                _desertionEventPending = false;
            }
        }

        /// <summary>
        /// Cancels the pending desertion (companion stays)
        /// </summary>
        public static void CancelDesertion()
        {
            _desertionEventPending = false;
            _pendingDesertionHero = null;
            CECustomHandler.LogToFile("Desertion was cancelled by alternative event");
        }

        public static Hero GetPendingDesertionHero() => _pendingDesertionHero;
        public static bool IsDesertionPending() => _desertionEventPending;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Returns a weighted random choice of alternative events matching the specified flag
        /// </summary>
        private static CEEvent  ReturnWeightedChoiceOfEventsAlternative(RestrictedListOfFlags alternativeFlag, Hero targetHero, Hero secondaryHero)
        {
            List<CEEvent> events = [];
            int CurrentOrder = 0;

            // Get the appropriate event list based on the alternative flag
            List<CEEvent> sourceEvents = alternativeFlag switch
            {
                RestrictedListOfFlags.DeathAlternative => CEPersistence.CEAlternativeDeathEvents,
                RestrictedListOfFlags.MarriageAlternative => CEPersistence.CEAlternativeMarriageEvents,
                RestrictedListOfFlags.DesertionAlternative => CEPersistence.CEAlternativeDesertionEvents,
                _ => []
            };

            if (sourceEvents == null || sourceEvents.Count <= 0) return null;

            CECustomHandler.LogToFile($"Checking for {alternativeFlag} events. Total events: {sourceEvents.Count}");

            foreach (CEEvent listEvent in sourceEvents)
            {
                // Basic validation
                string result = new CEEventChecker(listEvent).FlagsDoMatchEventConditionsAlternative(targetHero, secondaryHero, alternativeFlag);

                if (result == null)
                {
                    int weightedChance = 10;

                    if (listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.IgnoreAllOther))
                    {
                        CECustomHandler.LogToFile("IgnoreAllOther detected - auto fire " + listEvent.Name);
                        listEvent.Captive = targetHero?.CharacterObject;
                        return listEvent;
                    }

                    int OrderToCall = 0;
                    if (!string.IsNullOrEmpty(listEvent.OrderToCall))
                    {
                        OrderToCall = new CEVariablesLoader().GetIntFromXML(listEvent.OrderToCall);
                    }

                    if (OrderToCall < CurrentOrder)
                    {
                        CECustomHandler.LogToFile("OrderToCall - " + OrderToCall + " was less than CurrentOrder - " + CurrentOrder + " for " + listEvent.Name);
                        continue;
                    }
                    else if (OrderToCall > CurrentOrder)
                    {
                        events.Clear();
                        CurrentOrder = OrderToCall;
                    }

                    if (!string.IsNullOrEmpty(listEvent.WeightedChanceOfOccurring))
                    {
                        weightedChance = new CEVariablesLoader().GetIntFromXML(listEvent.WeightedChanceOfOccurring);
                    }

                    for (int a = weightedChance; a > 0; a--) events.Add(listEvent);
                }
                else
                {
                    CECustomHandler.LogToFile(result);
                }
            }

            CECustomHandler.LogToFile($"Number of filtered {alternativeFlag} events: {events.Count}");

            try
            {
                if (events.Count > 0)
                {
                    CEEvent selectedEvent = events.GetRandomElement();
                    selectedEvent.Captive = targetHero?.CharacterObject;
                    return selectedEvent;
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Alternative event selection error: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Launches an alternative event with notification
        /// </summary>
        private static void LaunchAlternativeEvent(CEEvent ceEvent, string notificationText)
        {
            if (ceEvent == null) return;

            notificationEventExists = true;

            if (CESettings.Instance?.EventCaptorNotifications ?? true)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(ceEvent.NotificationName))
                        new CESubModule().LoadCampaignNotificationTexture(ceEvent.NotificationName, 1);
                    else if (ceEvent.SexualContent)
                        new CESubModule().LoadCampaignNotificationTexture("CE_random_sexual_notification", 1);
                    else
                        new CESubModule().LoadCampaignNotificationTexture("CE_random_notification", 1);
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));
                    CECustomHandler.ForceLogToFile("LoadCampaignNotificationTexture: " + e.Message);
                }

                Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(
                    new CEEventMapNotification(ceEvent, new TextObject(notificationText)));
            }
            else
            {
                // Directly open the event menu
                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                {
                    Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                    if (!mapState.AtMenu)
                    {
                        TaleWorlds.CampaignSystem.GameMenus.GameMenu.ActivateGameMenu("prisoner_wait");
                    }

                    TaleWorlds.CampaignSystem.GameMenus.GameMenu.SwitchToMenu(ceEvent.Name);
                }
            }
        }

        #endregion
    }
}
