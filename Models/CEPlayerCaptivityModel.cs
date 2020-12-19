using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

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
            float gameProcess = MiscHelper.GetGameProcess();
            float num = (1f + gameProcess * 1f) * (PlayerCaptivity.CaptorParty.IsSettlement ? CESettings.Instance.EventOccurrenceSettlement : PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.Leader != null && PlayerCaptivity.CaptorParty.Leader.IsHero ? CESettings.Instance.EventOccurrenceLord : CESettings.Instance.EventOccurrenceOther);

            return CheckTimeElapsedMoreThanHours(PlayerCaptivity.LastCheckTime, num);

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
                CECustomHandler.ForceLogToFile("Underaged Player Detected. Age: " + Hero.MainHero.Age );
                return "menu_captivity_end_by_party_removed";
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
        ///     Modified Default
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
                int prostituteSkillFlag = Hero.MainHero.GetSkillValue(CESkills.Prostitution);

                if (prostituteSkillFlag < 50)
                {
                    return "menu_captivity_end_by_ally_party_saved";
                }
            }

            if (CESettings.Instance != null && (!CESettings.Instance.SlaveryToggle && !FactionManager.IsAtWarAgainstFaction(PlayerCaptivity.CaptorParty.MapFaction, MobileParty.MainParty.MapFaction) && (PlayerCaptivity.CaptorParty.MapFaction == MobileParty.MainParty.MapFaction || !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(PlayerCaptivity.CaptorParty.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(PlayerCaptivity.CaptorParty.MapFaction)))) return "menu_captivity_end_no_more_enemies";

            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
            {
                // Default event transfer disabled if slavery is enabled override or if it is garrison
                if (PlayerCaptivity.CaptorParty.MapFaction != PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.MapFaction || (CESettings.Instance.SlaveryToggle && !PlayerCaptivity.CaptorParty.MobileParty.IsGarrison && !PlayerCaptivity.CaptorParty.MobileParty.IsMilitia)) return null;
                PlayerCaptivity.LastCheckTime = CampaignTime.Now;
                if (Game.Current.GameStateManager.ActiveState is MapState) Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                return "menu_captivity_transfer_to_town";
            }

            if (!CheckEvent()) return null;
            PlayerCaptivity.LastCheckTime = CampaignTime.Now;
            Hero.MainHero.HitPoints += MBRandom.Random.Next(10);

            if (MBRandom.Random.Next(100) >= (Hero.MainHero.GetSkillValue(DefaultSkills.Tactics) / 4 + Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) / 4) / 4) return null;

            if (!PlayerCaptivity.CaptorParty.IsMobile || PlayerCaptivity.CaptorParty.MapEvent == null) return null;

            return "menu_escape_captivity_during_battle";

            //return null;  //warning unreachable
        }
    }
}