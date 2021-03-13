using CaptivityEvents.Brothel;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using Helpers;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace CaptivityEvents
{
    internal class CEPrisonerDialogue
    {
        private readonly Dynamics _dynamics = new Dynamics();

        public void AddPrisonerLines(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddDialogLine("CELordDefeatedLord", "start", "CELordDefeatedLordAnswer", "{=pURE9lFV}{SURRENDER_OFFER}", ConversationCEEventLordCaptureOnCondition, null, 200, null);

            campaignGameStarter.AddPlayerLine("CELordDefeatedLordAnswerCapture", "CELordDefeatedLordAnswer", "defeat_lord_answer_1", "{=g5G8AJ5n}You are my prisoner now.", null, null, 100, null, null);
            campaignGameStarter.AddPlayerLine("CELordDefeatedLordAnswerRelease", "CELordDefeatedLordAnswer", "defeat_lord_answer_2", "{=vHKkVkAF}You have fought well. You are free to go.", null, new ConversationSentence.OnConsequenceDelegate(LCELordDefeatedLordAnswerReleaseOnConsequence), 100, null, null);
            campaignGameStarter.AddPlayerLine("CELordDefeatedLordAnswerStrip", "CELordDefeatedLordAnswer", "LordDefeatedCaptureCEModAnswer", "{=CEEVENTS1107}Time to strip you of your belongings.", null, ConversationCEEventLordCaptureOnConsequence);


            campaignGameStarter.AddPlayerLine("LordDefeatedCaptureCEMod", "defeated_lord_answer", "LordDefeatedCaptureCEModAnswer", "{=CEEVENTS1107}Time to strip you of your belongings.", null, ConversationCEEventLordCaptureOnConsequence);
            campaignGameStarter.AddDialogLine("LordDefeatedReturn", "LordDefeatedCaptureCEModAnswer", "close_window", "{=!}{RESPONSE_STRING}", ConversationCEEventResponseInPartyOnCondition, null);

            campaignGameStarter.AddDialogLine("start_wanderer_unmet_party", "start", "prisoner_recruit_start_player", "{=!}{RESPONSE_STRING}", ConversationConditionTalkToPrisonerInParty, null, 120);
            campaignGameStarter.AddDialogLine("start_wanderer_unmet_party", "lord_introduction", "prisoner_recruit_start_player", "{=!}{RESPONSE_STRING}", ConversationConditionTalkToPrisonerInParty, null, 120);
            campaignGameStarter.AddDialogLine("start_wanderer_unmet_cell", "start", "CEPrisonerInCell", "{=!}{RESPONSE_STRING}", ConversationConditionTalkToPrisonerInCell, null, 120);
            campaignGameStarter.AddDialogLine("start_wanderer_unmet_cell", "lord_introduction", "CEPrisonerInCell", "{=!}{RESPONSE_STRING}", ConversationConditionTalkToPrisonerInCell, null, 120);

            campaignGameStarter.AddPlayerLine("PrisonerPrisonerInParty_01", "CEPrisonerInParty", "PrisonerPrisonerInPartyResponse", "{=CEEVENTS1108}Time to have some fun.", null, ConversationCEEventInPartyOnConsequence);
            campaignGameStarter.AddPlayerLine("PrisonerPrisonerInParty_02", "CEPrisonerInParty", "close_window", "{=CEEVENTS1051}Nevermind.", null, null);

            campaignGameStarter.AddPlayerLine("PrisonerPrisonerInParty", "prisoner_recruit_start_player", "PrisonerPrisonerInPartyResponse", "{=CEEVENTS1108}Time to have some fun.", null, ConversationCEEventInPartyOnConsequence);
            campaignGameStarter.AddDialogLine("PrisonerPrisonerInPartyResponseChat", "PrisonerPrisonerInPartyResponse", "close_window", "{=!}{RESPONSE_STRING}", ConversationCEEventResponseInPartyOnCondition, null);

            campaignGameStarter.AddPlayerLine("CEPrisonerInCell_01", "CEPrisonerInCell", "CEPrisonerInCell_01_response", "{=CEEVENTS1052}You are coming with me.", null, null);

            if (CESettings.Instance.ProstitutionControl) campaignGameStarter.AddPlayerLine("CEPrisonerInCell_02", "CEPrisonerInCell", "CEPrisonerInCell_02_response", "{=CEBROTHEL0979}Time to make you work at the brothel.", null , null, 100, ConversationCEEventBrothelOnCondition);

            campaignGameStarter.AddDialogLine("CEPrisonerInCell_01_r", "CEPrisonerInCell_01_response", "close_window", "{=!}{RESPONSE_STRING}", ConversationCEEventResponseInPartyOnCondition, ConversationCEEventInCellOnConsequence);

            campaignGameStarter.AddDialogLine("CEPrisonerInCell_02_r", "CEPrisonerInCell_02_response", "close_window", "{=!}{RESPONSE_STRING}", ConversationCEEventResponseInPartyOnCondition, ConversationCEEventBrothelOnConsequence);

            campaignGameStarter.AddPlayerLine("CEPrisonerInCell_02", "CEPrisonerInCell", "close_window", "{=CEEVENTS1051}Nevermind.", null, null);
        }

        private bool ConversationCEEventBrothelOnCondition(out TextObject text)
        {
            text = TextObject.Empty;
            if (Settlement.CurrentSettlement != null && CEBrothelBehavior.DoesOwnBrothelInSettlement(Settlement.CurrentSettlement))
            {
                return true;
            }
            text = new TextObject("{=}You do not own the brothel in this settlement.");
            return false;
        }

        private void LCELordDefeatedLordAnswerReleaseOnConsequence()
        {
            EndCaptivityAction.ApplyByReleasedByPlayerAfterBattle(Hero.OneToOneConversationHero, Hero.MainHero, null);
            _dynamics.RelationsModifier(CharacterObject.OneToOneConversationCharacter.HeroObject, 4, null, true, true);
            DialogHelper.SetDialogString("DEFEAT_LORD_ANSWER", "str_prisoner_released");
        }

        private bool ConversationCEEventLordCaptureOnCondition()
        {
            if (Campaign.Current.CurrentConversationContext == ConversationContext.CapturedLord && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.MapFaction != null && Hero.OneToOneConversationHero.MapFaction.IsBanditFaction)
            {
                GameState currentState = Game.Current.GameStateManager.ActiveState;
                DialogHelper.SetDialogString("SURRENDER_OFFER", "str_surrender_offer");
                return true;
            }
            return false;
        }

        private void ConversationCEEventBrothelOnConsequence()
        {

        }

        private void ConversationCEEventLordCaptureOnConsequence()
        {
            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            new CaptorSpecifics().CEStripVictim(CharacterObject.OneToOneConversationCharacter.HeroObject);

            if (CharacterObject.OneToOneConversationCharacter.HeroObject.GetSkillValue(CESkills.Slavery) < 50) new Dynamics().RelationsModifier(CharacterObject.OneToOneConversationCharacter.HeroObject, -10);

            TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
        }

        private bool ConversationConditionTalkToPrisonerInCell()
        {
            CharacterObject captive = CharacterObject.OneToOneConversationCharacter;

            if (captive != null && captive.IsHero && captive.HeroObject.GetSkillValue(CESkills.IsSlave) == 1)
            {
                if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 250) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1053}Yes you came {?PLAYER.GENDER}mistress{?}master{\\?}! [ib:confident][rb:very_positive]");
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 100) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1054}Yes {?PLAYER.GENDER}mistress{?}master{\\?} [ib:weary][rb:positive]");
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 50) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1055}Yes? [ib:weary][rb:unsure]");
                else MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1072}I am not your slave! [ib:aggressive][rb:very_negative]");
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1057}What do you want? [ib:nervous][rb:very_negative]");
            }

            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.IsSettlement && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner;
        }

        private bool ConversationConditionTalkToPrisonerInParty()
        {
            CharacterObject captive = CharacterObject.OneToOneConversationCharacter;

            if (captive != null && captive.IsHero && captive.HeroObject.GetSkillValue(CESkills.IsSlave) == 1)
            {
                if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 250) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1053}Yes you came {?PLAYER.GENDER}mistress{?}master{\\?}! [ib:confident][rb:very_positive]");
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 100) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1054}Yes {?PLAYER.GENDER}mistress{?}master{\\?} [ib:weary][rb:positive]");
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 50) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1055}Yes? [ib:weary][rb:unsure]");
                else MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1072}I am not your slave! [ib:aggressive][rb:very_negative]");
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1057}What do you want? [ib:nervous][rb:very_negative]");
            }

            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.IsMobile && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner;
        }

        private bool ConversationCEEventResponseInPartyOnCondition()
        {
            try
            {
                CharacterObject captive = CharacterObject.OneToOneConversationCharacter;

                if (captive != null && captive.IsHero && captive.HeroObject.GetSkillValue(CESkills.IsSlave) == 1)
                {
                    if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 250) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1073}Finally![ib:confident][rb:very_positive]");
                    else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 100) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1071}Yes {?PLAYER.GENDER}mistress{?}master{\\?} [ib:confident2][rb:positive]");
                    else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 50) MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1070}Alright.[ib:weary][rb:unsure]");
                    else MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1072}What?! [ib:aggressive][rb:very_negative]");
                }
                else
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1109}Wait what?[ib:nervous][rb:very_negative]");
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to launch ConversationCEEventResponseInPartyOnCondition : " + e);
            }

            return true;
        }

        private void ConversationCEEventInPartyOnConsequence()
        {
            CEPersistence.captivePlayEvent = true;
            CEPersistence.captiveToPlay = CharacterObject.OneToOneConversationCharacter;
        }

        private void ConversationCEEventInCellOnConsequence()
        {
            try
            {
                CEPersistence.captivePlayEvent = true;
                CEPersistence.captiveToPlay = CharacterObject.OneToOneConversationCharacter;

                CEPersistence.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("_barrier_passage_center");
                CEPersistence.agentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => agent.Character == CharacterObject.OneToOneConversationCharacter);
                CEPersistence.dungeonState = CEPersistence.DungeonState.StartWalking;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to launch ConversationCEEventInCellOnConsequence : " + e);
            }
        }
    }
}