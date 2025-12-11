using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;


namespace CaptivityEvents.Events
{
    public class CaptiveSpecifics
    {
        internal void CECaptivityContinue(ref MenuCallbackArgs args)
        {
            CEPersistence.animationPlayEvent = false;

            try
            {
                if (PlayerCaptivity.CaptorParty != null)
                {
                    string waitingList = WaitingList.CEWaitingList();

                    if (waitingList != null)
                    {
                        CEHelper.SafeActivateGameMenu(waitingList);
                    }
                    else
                    {
                        new CESubModule().LoadTexture("default");

                        CEHelper.SafeSwitchToMenu(PlayerCaptivity.CaptorParty.IsSettlement
                                                  ? "settlement_wait"
                                                  : "prisoner_wait");
                    }
                }
                else
                {
                    new CESubModule().LoadTexture("default");
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
            if (CEHelper.HelperMBRandom(100) > escapeChance + new ScoresCalculation().EscapeProwessScore(Hero.MainHero))
            {
                if (CESettings.Instance?.SexualContent ?? true)
                {
                    CEHelper.SafeSwitchToMenu(Hero.MainHero.IsFemale
                                               ? "CE_captivity_sexual_escape_failure"
                                               : "CE_captivity_sexual_escape_failure_male");
                }
                else
                {
                    CEHelper.SafeSwitchToMenu(Hero.MainHero.IsFemale
                                            ? "CE_captivity_escape_failure"
                                            : "CE_captivity_escape_failure_male");
                }

                return;
            }

            if (CESettings.Instance?.SexualContent ?? true)
            {
                CEHelper.SafeSwitchToMenu(Hero.MainHero.IsFemale
                                          ? "CE_captivity_sexual_escape_success"
                                          : "CE_captivity_sexual_escape_success_male");
            }
            else
            {
                CEHelper.SafeSwitchToMenu(Hero.MainHero.IsFemale
                                         ? "CE_captivity_escape_success"
                                         : "CE_captivity_escape_success_male");
            }
        }


        /// EndCaptivityInternal from PlayerCaptivity
        internal void CECaptivityLeave(ref MenuCallbackArgs args)
        {
            new CESubModule().LoadTexture("default");
            PartyBase captorParty = PlayerCaptivity.CaptorParty;
            CECampaignBehavior.ExtraProps.Owner = null;

            if (!captorParty.IsSettlement || !captorParty.Settlement.IsTown)
            {
                if (captorParty != null && captorParty.IsMobile)
                {
                    MobileParty.MainParty.IsCurrentlyAtSea = captorParty.MobileParty.IsCurrentlyAtSea;
                }
                PlayerCaptivity.EndCaptivity();
                return;
            }
            else
            {
                MobileParty.MainParty.IsCurrentlyAtSea = false;
            }

            // EndCaptivityInternal
            try
            {
                if (Hero.MainHero.IsAlive)
                {
                    if (Hero.MainHero.IsWounded) Hero.MainHero.HitPoints = 20;

                    PlayerEncounter.ProtectPlayerSide();
                    MobileParty.MainParty.SetDisorganized(false);
                    PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, 1, true);
                    MobileParty.MainParty.ChangePartyLeader(Hero.MainHero);
                }

                MobileParty.MainParty.CurrentSettlement = PlayerCaptivity.CaptorParty.Settlement;
                if (Campaign.Current.CurrentMenuContext != null) CEHelper.SafeSwitchToMenu("town");

                if (Hero.MainHero.IsAlive)
                {
                    Hero.MainHero.ChangeState(Hero.CharacterStates.Active);
                }


                if (captorParty.IsActive)
                {
                    captorParty.PrisonRoster.RemoveTroop(Hero.MainHero.CharacterObject, 1, default, 0);
                }

                if (Hero.MainHero.IsAlive)
                {
                    MobileParty.MainParty.IsActive = true;
                    PartyBase.MainParty.SetAsCameraFollowParty();
                    MobileParty.MainParty.SetMoveModeHold();
                    if (!MobileParty.MainParty.IsCurrentlyAtSea)
                    {
                        PartyBase.MainParty.UpdateVisibilityAndInspected(MobileParty.MainParty.Position, 0f);
                    }
                }

                PlayerCaptivity.CaptorParty = null;
            }
            catch (Exception)
            {
                PlayerCaptivity.EndCaptivity();
            }
        }

        private bool ShouldActivateRaftStateForMobileParty(MobileParty mobileParty)
        {
            return mobileParty.IsCurrentlyAtSea && !mobileParty.IsInRaftState && mobileParty.IsActive;
        }


        private void HandleRaftStateActivate(MobileParty mobileParty)
        {
            if (mobileParty.HasLandNavigationCapability)
            {
                RaftStateChangeAction.ActivateRaftStateForParty(mobileParty);
            }
        }

        private void ConsiderMemberAndArmyRaftStateStatus(MobileParty party, Army army)
        {
            if (ShouldActivateRaftStateForMobileParty(party))
            {
                HandleRaftStateActivate(party);
            }
            if (army != null && army.LeaderParty.IsCurrentlyAtSea && !army.LeaderParty.HasNavalNavigationCapability)
            {
                DisbandArmyAction.ApplyByNoShip(army);
            }
        }

        internal void CECaptivityRelease(ref MenuCallbackArgs args)
        {
            CECaptivityLeave(ref args);
        }

        internal void CECaptivityEscape(ref MenuCallbackArgs args)
        {
            CECampaignBehavior.ExtraProps.Owner = null;
            bool wasInSettlement = PlayerCaptivity.CaptorParty.IsSettlement;
            Settlement currentSettlement = PlayerCaptivity.CaptorParty.Settlement;

            TextObject textObject = GameTexts.FindText("str_CE_escape_success", wasInSettlement
                                                    ? "settlement"
                                                    : null);

            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);

            if (wasInSettlement)
            {
                string settlementName = currentSettlement != null
                    ? currentSettlement.Name.ToString()
                    : "ERROR";
                textObject.SetTextVariable("SETTLEMENT", settlementName);
            }

            if (PlayerCaptivity.CaptorParty != null && PlayerCaptivity.CaptorParty.IsMobile)
            {
                MobileParty.MainParty.IsCurrentlyAtSea = PlayerCaptivity.CaptorParty.MobileParty.IsCurrentlyAtSea;
            }
            else
            {
                MobileParty.MainParty.IsCurrentlyAtSea = false;
            }

            new CESubModule().LoadTexture("default");
            PlayerCaptivity.EndCaptivity();

            if (ShouldActivateRaftStateForMobileParty(MobileParty.MainParty))
            {
                if (MobileParty.MainParty.Army != null)
                {
                    ConsiderMemberAndArmyRaftStateStatus(MobileParty.MainParty, MobileParty.MainParty.Army);
                }
                else
                {
                    HandleRaftStateActivate(MobileParty.MainParty);
                }
            }
        }

        internal void CECaptivityChange(ref MenuCallbackArgs args, PartyBase party)
        {
            try
            {
                PlayerCaptivity.CaptorParty = party;
                PlayerCaptivity.StartCaptivity(party);
                CEHelper.delayedEvents.Clear();
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Failed to exception: " + e.Message + " stacktrace: " + e.StackTrace);
            }
        }
    }
}