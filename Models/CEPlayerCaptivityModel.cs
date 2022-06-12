#define V180

using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;

namespace CaptivityEvents.Models
{
    public class CEPlayerCaptivityModel : DefaultPlayerCaptivityModel
    {
        private bool CheckTimeElapsedMoreThanHours(CampaignTime eventBeginTime, float hoursToWait)
        {
            float elapsedHoursUntilNow = eventBeginTime.ElapsedHoursUntilNow;
            float randomNumber = PlayerCaptivity.RandomNumber;

            return hoursToWait * (0.5 + randomNumber) < elapsedHoursUntilNow;
        }

        private bool CheckEvent()
        {
            if (PlayerCaptivity.CaptorParty == null) return false;
            bool isInSettlement = PlayerCaptivity.CaptorParty.IsSettlement;
            bool isInLordParty = !isInSettlement && PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.LeaderHero != null;

            float eventOccurence = CESettings.Instance != null ?
                isInSettlement ?
                CESettings.Instance.EventOccurrenceSettlement :
                isInLordParty ?
                CESettings.Instance.EventOccurrenceLord :
                CESettings.Instance.EventOccurrenceOther :
                6f;
            return CheckTimeElapsedMoreThanHours(PlayerCaptivity.LastCheckTime, eventOccurence);
        }

        /// <summary>
        /// Custom CheckCaptivityChange Function
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>EventName</returns>
        public override string CheckCaptivityChange(float dt)
        {
            if (!PlayerCaptivity.IsCaptive) return DefaultOverridenCheckCaptivityChange(dt);

            if (Hero.MainHero.Age < 18f)
            {
                EndCaptivityAction.ApplyByReleasing(Hero.MainHero);
                InformationManager.DisplayMessage(new InformationMessage(("Invalid Age: " + Hero.MainHero.Age), Colors.Gray));
                CECustomHandler.ForceLogToFile("Underaged Player Detected. Age: " + Hero.MainHero.Age);
                return "menu_captivity_end_by_party_removed";
            }

            if (CEHelper.delayedEvents.Count > 0)
            {
                string eventToFire = null;

                bool shouldFireEvent = CEHelper.delayedEvents.Any(item =>
                {
                    if (item.eventName != null && item.eventTime < Campaign.Current.CampaignStartTime.ElapsedHoursUntilNow)
                    {
                        CECustomHandler.LogToFile("Firing " + item.eventName);
                        if (item.conditions == true)
                        {
                            string result = CEEventManager.FireSpecificEvent(item.eventName);
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
                                        eventToFire = item.eventName;
                                        item.hasBeenFired = true;
                                        return true;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            eventToFire = item.eventName.ToLower();
                            CEEvent foundevent = CEPersistence.CEEventList.FirstOrDefault(ceevent => ceevent.Name.ToLower() == eventToFire);
                            if (foundevent != null && !foundevent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
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
                    CEHelper.delayedEvents.RemoveAll(item => item.hasBeenFired);
                    PlayerCaptivity.LastCheckTime = CampaignTime.Now;
                    return eventToFire;
                }
            }

            if (PlayerCaptivity.CaptorParty != null && !PlayerCaptivity.CaptorParty.IsSettlement)
            {
                if (!CheckEvent()) return DefaultOverridenCheckCaptivityChange(dt);
                PlayerCaptivity.LastCheckTime = CampaignTime.Now;

                CECustomHandler.LogToFile("About to choose a event!");
                CEEvent captiveEvent = CEEventManager.ReturnWeightedChoiceOfEvents();

                if (captiveEvent != null) return captiveEvent.Name;
            }
            else
            {
                if (!CheckEvent()) return DefaultOverridenCheckCaptivityChange(dt);
                PlayerCaptivity.LastCheckTime = CampaignTime.Now;

                CECustomHandler.LogToFile("About to choose a settlement event!");
                CEEvent captiveEvent = CEEventManager.ReturnWeightedChoiceOfEvents();

                if (captiveEvent != null) return captiveEvent.Name;
            }

            return DefaultOverridenCheckCaptivityChange(dt);
        }

        /// <summary>
        /// Modified Default
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string DefaultOverridenCheckCaptivityChange(float dt)
        {
            if (PlayerCaptivity.CaptorParty.IsMobile && !PlayerCaptivity.CaptorParty.MobileParty.IsActive)
            {
                return "menu_captivity_end_by_party_removed";
            }

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MapFaction == Hero.MainHero.Clan)
            {
                return "menu_captivity_end_by_ally_party_saved";
            }

            if (PlayerCaptivity.CaptorParty.IsSettlement && PlayerCaptivity.CaptorParty.MapFaction == Hero.MainHero.Clan)
            {
                int IsSlave = Hero.MainHero.GetSkillValue(CESkills.IsSlave);

                if (IsSlave == 1)
                {
                    return "menu_captivity_end_by_ally_party_saved";
                }
            }

            if (CESettings.Instance != null && (!(CESettings.Instance?.SlaveryToggle ?? true) && !FactionManager.IsAtWarAgainstFaction(PlayerCaptivity.CaptorParty.MapFaction, MobileParty.MainParty.MapFaction) && (PlayerCaptivity.CaptorParty.MapFaction == MobileParty.MainParty.MapFaction || !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(PlayerCaptivity.CaptorParty.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(PlayerCaptivity.CaptorParty.MapFaction)))) return "menu_captivity_end_no_more_enemies";

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
            {
                // Default event transfer disabled if slavery is enabled override or if it is garrison
                if (PlayerCaptivity.CaptorParty.MapFaction != PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.MapFaction || ((CESettings.Instance?.SlaveryToggle ?? true) && !PlayerCaptivity.CaptorParty.MobileParty.IsGarrison && !PlayerCaptivity.CaptorParty.MobileParty.IsMilitia)) return null;
                PlayerCaptivity.LastCheckTime = CampaignTime.Now;
                if (Game.Current.GameStateManager.ActiveState is MapState) Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                return "menu_captivity_transfer_to_town";
            }

            if (!CheckEvent()) return null;
            PlayerCaptivity.LastCheckTime = CampaignTime.Now;
            Hero.MainHero.HitPoints += CEHelper.HelperMBRandom(10);

            if (CEHelper.HelperMBRandom(100) >= (Hero.MainHero.GetSkillValue(DefaultSkills.Tactics) / 4 + Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) / 4) / 4) return null;

            if (!PlayerCaptivity.CaptorParty.IsMobile || PlayerCaptivity.CaptorParty.MapEvent == null) return null;

            return "menu_escape_captivity_during_battle";

            //return null;  //warning unreachable
        }
    }
}