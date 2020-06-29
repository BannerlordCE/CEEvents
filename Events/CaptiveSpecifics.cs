using System;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;

namespace CaptivityEvents.Events
{
    public class CaptiveSpecifics
    {
        internal void CECaptivityContinue(ref MenuCallbackArgs args)
        {
            CEPersistence.AnimationPlayEvent = false;
            var t = new CESubModule();

            try
            {
                if (PlayerCaptivity.CaptorParty != null)
                {
                    var waitingList = new WaitingList().CEWaitingList();

                    if (waitingList != null)
                    {
                        GameMenu.SwitchToMenu(waitingList);
                    }
                    else
                    {
                        t.LoadTexture("default");

                        GameMenu.SwitchToMenu(PlayerCaptivity.CaptorParty.IsSettlement
                                                  ? "settlement_wait"
                                                  : "prisoner_wait");
                    }
                }
                else
                {
                    t.LoadTexture("default");
                    GameMenu.ExitToLast();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Critical Error: CECaptivityContinue : " + e);
            }
        }

        internal void CECaptivityEscapeAttempt(ref MenuCallbackArgs args, int escapeChance = 10)
        {
            if (MBRandom.Random.Next(100) < escapeChance + new ScoresCalculation().EscapeProwessScore(Hero.MainHero))
            {
                if (CESettings.Instance != null && !CESettings.Instance.SexualContent)
                    GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                              ? "CE_captivity_escape_failure"
                                              : "CE_captivity_escape_failure_male");
                else
                    GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                              ? "CE_captivity_sexual_escape_failure"
                                              : "CE_captivity_sexual_escape_failure_male");

                return;
            }

            if (CESettings.Instance != null && !CESettings.Instance.SexualContent)
                GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                          ? "CE_captivity_escape_success"
                                          : "CE_captivity_escape_success_male");
            else
                GameMenu.SwitchToMenu(Hero.MainHero.IsFemale
                                          ? "CE_captivity_sexual_escape_success"
                                          : "CE_captivity_sexual_escape_success_male");
        }

        internal void CECaptivityLeave(ref MenuCallbackArgs args)
        {
            new CESubModule().LoadTexture("default");
            var captorParty = PlayerCaptivity.CaptorParty;
            CECampaignBehavior.ExtraProps.Owner = null;

            if (!captorParty.IsSettlement || !captorParty.Settlement.IsTown)
            {
                PlayerCaptivity.EndCaptivity();

                return;
            }

            try
            {
                if (Hero.MainHero.IsAlive)
                {
                    if (Hero.MainHero.IsWounded) Hero.MainHero.HitPoints = 20;

                    if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.IsMobile) PlayerCaptivity.CaptorParty.MobileParty.SetDoNotAttackMainParty(12);
                    PlayerEncounter.ProtectPlayerSide();
                    MobileParty.MainParty.IsDisorganized = false;
                    PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, 1, true);
                }

                if (Campaign.Current.CurrentMenuContext != null) GameMenu.SwitchToMenu("town");

                if (Hero.MainHero.IsAlive)
                {
                    Hero.MainHero.ChangeState(Hero.CharacterStates.Active);
                    Hero.MainHero.DaysLeftToRespawn = 0;
                }

                if (captorParty.IsActive) captorParty.PrisonRoster.RemoveTroop(Hero.MainHero.CharacterObject);

                if (Hero.MainHero.IsAlive)
                {
                    MobileParty.MainParty.IsActive = true;
                    PartyBase.MainParty.SetAsCameraFollowParty();
                    MobileParty.MainParty.SetMoveModeHold();
                    SkillLevelingManager.OnMainHeroReleasedFromCaptivity(PlayerCaptivity.CaptivityStartTime.ElapsedHoursUntilNow);
                    PartyBase.MainParty.UpdateVisibilityAndInspected(true);
                }

                PlayerCaptivity.CaptorParty = null;
            }
            catch (Exception)
            {
                PlayerCaptivity.EndCaptivity();
            }
        }

        internal void CECaptivityEscape(ref MenuCallbackArgs args)
        {
            CECampaignBehavior.ExtraProps.Owner = null;
            var wasInSettlement = PlayerCaptivity.CaptorParty.IsSettlement;
            var currentSettlement = PlayerCaptivity.CaptorParty.Settlement;

            var textObject = GameTexts.FindText("str_CE_escape_success", wasInSettlement
                                                    ? "settlement"
                                                    : null);
            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);

            if (wasInSettlement)
            {
                var settlementName = currentSettlement != null
                    ? currentSettlement.Name.ToString()
                    : "ERROR";
                textObject.SetTextVariable("SETTLEMENT", settlementName);
            }

            new CESubModule().LoadTexture("default");
            PlayerCaptivity.EndCaptivity();
        }

        internal void CECaptivityChange(ref MenuCallbackArgs args, PartyBase party)
        {
            try
            {
                PlayerCaptivity.CaptorParty = party;
                PlayerCaptivity.StartCaptivity(party);
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Failed to exception: " + e.Message + " stacktrace: " + e.StackTrace);
            }
        }
    }
}