using CaptivityEvents.Events;
using System;
using System.Linq;
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
            campaignGameStarter.AddPlayerLine("LordDefeatedCaptureCEMod", "defeated_lord_answer", "LordDefeatedCaptureCEModAnswer", "{=CEEVENTS1107}Time to strip you of your belongings.", null, new ConversationSentence.OnConsequenceDelegate(ConversationCEEventLordCaptureOnConsequence), 100, null, null);
            campaignGameStarter.AddDialogLine("LordDefeatedReturn", "LordDefeatedCaptureCEModAnswer", "close_window", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationCEEventResponseInPartyOnCondition), null, 100, null);

            campaignGameStarter.AddDialogLine("start_wanderer_unmet_party", "start", "prisoner_recruit_start_player", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationConditionTalkToPrisonerInParty), null, 120, null);
            campaignGameStarter.AddDialogLine("start_wanderer_unmet_party", "lord_introduction", "prisoner_recruit_start_player", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationConditionTalkToPrisonerInParty), null, 120, null);
            campaignGameStarter.AddDialogLine("start_wanderer_unmet_cell", "start", "CEPrisonerInCell", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationConditionTalkToPrisonerInCell), null, 120, null);
            campaignGameStarter.AddDialogLine("start_wanderer_unmet_cell", "lord_introduction", "CEPrisonerInCell", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationConditionTalkToPrisonerInCell), null, 120, null);

            campaignGameStarter.AddPlayerLine("PrisonerPrisonerInParty_01", "CEPrisonerInParty", "PrisonerPrisonerInPartyResponse", "{=CEEVENTS1108}Time to have some fun.", null, new ConversationSentence.OnConsequenceDelegate(ConversationCEEventInPartyOnConsequence), 100, null, null);
            campaignGameStarter.AddPlayerLine("PrisonerPrisonerInParty_02", "CEPrisonerInParty", "close_window", "{=CEEVENTS1051}Nevermind.", null, null, 100, null, null);

            campaignGameStarter.AddPlayerLine("PrisonerPrisonerInParty", "prisoner_recruit_start_player", "PrisonerPrisonerInPartyResponse", "{=CEEVENTS1108}Time to have some fun.", null, new ConversationSentence.OnConsequenceDelegate(ConversationCEEventInPartyOnConsequence), 100, null, null);
            campaignGameStarter.AddDialogLine("PrisonerPrisonerInPartyResponseChat", "PrisonerPrisonerInPartyResponse", "close_window", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationCEEventResponseInPartyOnCondition), null, 100, null);

            campaignGameStarter.AddPlayerLine("CEPrisonerInCell_01", "CEPrisonerInCell", "CEPrisonerInCell_01_response", "{=CEEVENTS1052}You are coming with me.", null, null, 100, null, null);

            campaignGameStarter.AddDialogLine("CEPrisonerInCell_01_r", "CEPrisonerInCell_01_response", "close_window", "{=!}{RESPONSE_STRING}", new ConversationSentence.OnConditionDelegate(ConversationCEEventResponseInPartyOnCondition), new ConversationSentence.OnConsequenceDelegate(ConversationCEEventInCellOnConsequence), 100, null);

            campaignGameStarter.AddPlayerLine("CEPrisonerInCell_02", "CEPrisonerInCell", "close_window", "{=CEEVENTS1051}Nevermind.", null, null, 100, null, null);
        }

        private static void ConversationCEEventLordCaptureOnConsequence()
        {
            Campaign.Current.CurrentConversationContext = ConversationContext.Default;
            CEEventLoader.CEStripVictim(CharacterObject.OneToOneConversationCharacter.HeroObject);
            if (CharacterObject.OneToOneConversationCharacter.HeroObject.GetSkillValue(CESkills.Slavery) < 50)
            {
                CEEventLoader.RelationsModifier(CharacterObject.OneToOneConversationCharacter.HeroObject, -10);
            }
            TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
        }

        private static bool ConversationConditionTalkToPrisonerInCell()
        {
            CharacterObject captive = CharacterObject.OneToOneConversationCharacter;
            if (captive != null && captive.IsHero && captive.HeroObject.GetSkillValue(CESkills.IsSlave) == 1)
            {
                if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 250)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1053}Yes you came {?PLAYER.GENDER}mistress{?}master{\\?}! [ib:confident][rb:very_positive]", false);
                }
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 100)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1054}Yes {?PLAYER.GENDER}mistress{?}master{\\?} [ib:weary][rb:positive]", false);
                }
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 50)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1055}Yes? [ib:weary][rb:unsure]", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1072}I am not your slave! [ib:aggressive][rb:very_negative]", false);
                }
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1057}What do you want? [ib:nervous][rb:very_negative]", false);
            }
            return (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.IsSettlement && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner);
        }

        private static bool ConversationConditionTalkToPrisonerInParty()
        {
            CharacterObject captive = CharacterObject.OneToOneConversationCharacter;
            if (captive != null && captive.IsHero && captive.HeroObject.GetSkillValue(CESkills.IsSlave) == 1)
            {
                if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 250)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1053}Yes you came {?PLAYER.GENDER}mistress{?}master{\\?}! [ib:confident][rb:very_positive]", false);
                }
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 100)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1054}Yes {?PLAYER.GENDER}mistress{?}master{\\?} [ib:weary][rb:positive]", false);
                }
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 50)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1055}Yes? [ib:weary][rb:unsure]", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1072}I am not your slave! [ib:aggressive][rb:very_negative]", false);
                }
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1057}What do you want? [ib:nervous][rb:very_negative]", false);
            }

            return (Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner != null && Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.IsMobile && Hero.OneToOneConversationHero.HeroState == Hero.CharacterStates.Prisoner);
        }

        private static bool ConversationCEEventResponseInPartyOnCondition()
        {
            CharacterObject captive = CharacterObject.OneToOneConversationCharacter;
            if (captive != null && captive.IsHero && captive.HeroObject.GetSkillValue(CESkills.IsSlave) == 1)
            {
                if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 250)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1073}Finally![ib:confident][rb:very_positive]", false);
                }
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 100)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1071}Yes {?PLAYER.GENDER}mistress{?}master{\\?} [ib:confident2][rb:positive]", false);
                }
                else if (captive.HeroObject.GetSkillValue(CESkills.Slavery) > 50)
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1070}Alright.[ib:weary][rb:unsure]", false);
                }
                else
                {
                    MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1072}What?! [ib:aggressive][rb:very_negative]", false);
                }
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", "{=CEEVENTS1109}Wait what?[ib:nervous][rb:very_negative]", false);
            }
            return true;
        }

        private static void ConversationCEEventInPartyOnConsequence()
        {
            CESubModule.captivePlayEvent = true;
            CESubModule.captiveToPlay = CharacterObject.OneToOneConversationCharacter;
        }

        private static void ConversationCEEventInCellOnConsequence()
        {
            try
            {
                CESubModule.captivePlayEvent = true;
                CESubModule.captiveToPlay = CharacterObject.OneToOneConversationCharacter;

                CESubModule.gameEntity = Mission.Current.Scene.GetFirstEntityWithName("_barrier_passage_center");
                CESubModule.agentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => { return agent.Character == CharacterObject.OneToOneConversationCharacter; });
                CESubModule.dungeonState = CESubModule.DungeonState.StartWalking;
            }
            catch (Exception e)
            {
                Custom.CECustomHandler.ForceLogToFile("Failed to launch ConversationCEEventConsequenceGoToChambers : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e.ToString());
            }
        }
    }
}