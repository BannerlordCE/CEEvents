using System;
using System.Linq;
using CaptivityEvents.Custom;
using CaptivityEvents.Enums;
using CaptivityEvents.Events;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace CaptivityEvents
{
    internal class CEPrisonerDialogue
    {
        public static void AddPrisonerLines(CampaignGameStarter campaignGameStarter)
        {
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

            campaignGameStarter.AddDialogLine("CEPrisonerInCell_01_r", "CEPrisonerInCell_01_response", "close_window", "{=!}{RESPONSE_STRING}", ConversationCEEventResponseInPartyOnCondition, ConversationCEEventInCellOnConsequence);

            campaignGameStarter.AddPlayerLine("CEPrisonerInCell_02", "CEPrisonerInCell", "close_window", "{=CEEVENTS1051}Nevermind.", null, null);
        }

        private static void ConversationCEEventLordCaptureOnConsequence()
        {
            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            CEEventLoader.CEStripVictim(CharacterObject.OneToOneConversationCharacter.HeroObject);
            if (CharacterObject.OneToOneConversationCharacter.HeroObject.GetSkillValue(CESkills.Slavery) < 50) CEEventLoader.RelationsModifier(CharacterObject.OneToOneConversationCharacter.HeroObject, -10);
            TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
        }

        private static bool ConversationConditionTalkToPrisonerInCell()
        {
            var captive = CharacterObject.OneToOneConversationCharacter;

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

            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.IsSettlement && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner;
        }

        private static bool ConversationConditionTalkToPrisonerInParty()
        {
            var captive = CharacterObject.OneToOneConversationCharacter;

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

        private static bool ConversationCEEventResponseInPartyOnCondition()
        {
            var captive = CharacterObject.OneToOneConversationCharacter;

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

            return true;
        }

        private static void ConversationCEEventInPartyOnConsequence()
        {
            CESubModule.CaptivePlayEvent = true;
            CESubModule.CaptiveToPlay = CharacterObject.OneToOneConversationCharacter;
        }

        private static void ConversationCEEventInCellOnConsequence()
        {
            try
            {
                CESubModule.CaptivePlayEvent = true;
                CESubModule.CaptiveToPlay = CharacterObject.OneToOneConversationCharacter;

                CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("_barrier_passage_center");
                CESubModule.AgentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => agent.Character == CharacterObject.OneToOneConversationCharacter);
                CESubModule.dungeonState = DungeonState.StartWalking;
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Failed to launch ConversationCEEventConsequenceGoToChambers : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e);
            }
        }
    }
}