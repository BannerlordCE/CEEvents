using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace CaptivityEvents.Issues
{
    public class CEWhereAreMyThingsIssueBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public static IssueBase OnStartIssue(PotentialIssueData pid, Hero issueOwner)
        {
            return new CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue(issueOwner);
        }

        internal class CEWhereAreMyThingsIssue : IssueBase
        {
            protected override int RewardGold => (int)(350f + 1500f * base.IssueDifficultyMultiplier);
            protected override bool IsThereAlternativeSolution => false;
            protected override bool IsThereLordSolution => false;
            public override TextObject Title => new TextObject("{=CEEVENTS1089}Missing Equipment", null);
            public override TextObject Description => new TextObject("{=CEEVENTS1088}Someone found some equipment that looks like yours.", null);
            protected override TextObject IssueBriefByIssueGiver => new TextObject("{=CEEVENTS1087}Been looking at this equipment someone left here.", null);
            protected override TextObject IssueAcceptByPlayer => new TextObject("{=CEEVENTS1086}Hey, that looks like my equipment!", null);
            protected override TextObject IssueQuestSolutionExplanationByIssueGiver => new TextObject("{=CEEVENTS1085}Well you can pay for it.", null);
            protected override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=CEEVENTS1084}Are you serious?", null);

            public CEWhereAreMyThingsIssue(Hero issueOwner) : base(issueOwner, new Dictionary<IssueEffect, float> { }, CampaignTime.DaysFromNow(25f))
            {
            }

            protected override void AfterIssueCreation()
            {
            }

            protected override void OnGameLoad()
            {
            }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                return new CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssueQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(CESettings.Instance.StolenGearDuration), RewardGold, new Equipment(Hero.MainHero.BattleEquipment), new Equipment(Hero.MainHero.CivilianEquipment));
            }

            protected override float GetFrequency()
            {
                return 0.01f;
            }

            public override bool IssueStayAliveConditions()
            {
                return true;
            }

            protected override void CompleteIssueWithTimedOutConsequences()
            {
            }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                bool flag2 = issueGiver.GetRelationWithPlayer() >= -10f;
                flag = (flag2 ? IssueBase.PreconditionFlags.None : IssueBase.PreconditionFlags.Relation);
                relationHero = issueGiver;
                skill = null;
                return flag2;
            }
        }

        internal class CEWhereAreMyThingsIssueQuest : QuestBase
        {
            public override TextObject Title
            {
                get
                {
                    TextObject textObject;
                    if (QuestGiver.CurrentSettlement.IsVillage)
                    {
                        textObject = GameTexts.FindText("str_CE_quest_found", "village");
                    }
                    else if (QuestGiver.CurrentSettlement.IsTown)
                    {
                        textObject = GameTexts.FindText("str_CE_quest_found", "town");
                    }
                    else
                    {
                        textObject = GameTexts.FindText("str_CE_quest_found", null);
                    }
                    textObject.SetTextVariable("ISSUE_SETTLEMENT", QuestGiver.CurrentSettlement.Name);
                    return textObject;
                }
            }

            private TextObject JournalTaskName
            {
                get
                {
                    TextObject textObject = PlayerAcceptedQuestLogText;
                    return textObject;
                }
            }

            public override bool IsRemainingTimeHidden => false;

            public CEWhereAreMyThingsIssueQuest(string questId, Hero giverHero, CampaignTime duration, int rewardGold, Equipment stolenBattleEquipment, Equipment stolenCivilianEquipment) : base(questId, giverHero, duration, rewardGold)
            {
                this.stolenBattleEquipment = stolenBattleEquipment;
                this.stolenCivilianEquipment = stolenCivilianEquipment;
                base.AddTrackedObject(QuestGiver);
                SetDialogs();
                base.InitializeQuestOnCreation();
                OnQuestAccepted();
            }

            protected override void InitializeQuestOnGameLoad()
            {
                base.AddTrackedObject(QuestGiver);
                SetDialogs();
            }

            protected override void RegisterEvents()
            {
            }

            protected override void SetDialogs()
            {
                OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("{=CEEVENTS1080}I am serious.", null, null).Condition(() => Hero.OneToOneConversationHero == QuestGiver).Consequence(new ConversationSentence.OnConsequenceDelegate(OnQuestAccepted)).CloseDialog();

                DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=CEEVENTS1079}Have you come here to claim your equipment?", null), null, null).Condition(delegate
                {
                    return CharacterObject.OneToOneConversationCharacter == QuestGiver.CharacterObject;
                }).BeginPlayerOptions().PlayerOption(new TextObject("{=CEEVENTS1077}Yes, I came for my things.", null), null).NpcLine(new TextObject("{=CEEVENTS1078}Here you go {?PLAYER.GENDER}milady{?}sir{\\?}.", null), null, null).Consequence(new ConversationSentence.OnConsequenceDelegate(base.CompleteQuestWithSuccess)).CloseDialog();
            }

            private void OnQuestAccepted()
            {
                base.StartQuest();
                base.AddLog(PlayerAcceptedQuestLogText);
            }

            // Quest Conditions
            protected override void OnCompleteWithSuccess()
            {
                base.AddLog(OnQuestSucceededLogText, false);

                foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                {
                    try
                    {
                        if (!Hero.MainHero.BattleEquipment.GetEquipmentFromSlot(i).IsEmpty)
                        {
                            PartyBase.MainParty.ItemRoster.AddToCounts(Hero.MainHero.BattleEquipment.GetEquipmentFromSlot(i).Item, 1, true);
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        if (!Hero.MainHero.CivilianEquipment.GetEquipmentFromSlot(i).IsEmpty)
                        {
                            PartyBase.MainParty.ItemRoster.AddToCounts(Hero.MainHero.CivilianEquipment.GetEquipmentFromSlot(i).Item, 1, true);
                        }
                    }
                    catch (Exception) { }
                }

                EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, stolenBattleEquipment);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, stolenCivilianEquipment);

                base.RemoveTrackedObject(QuestGiver);
            }

            public override void OnFailed()
            {
                base.AddLog(OnQuestFailedLogText, false);
                base.RemoveTrackedObject(QuestGiver);
            }

            protected override void OnTimedOut()
            {
                base.AddLog(OnQuestTimedOutLogText, false);
                base.RemoveTrackedObject(QuestGiver);
            }

            // Quest Logs
            private TextObject PlayerAcceptedQuestLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=CEEVENTS1077}{QUEST_GIVER.LINK} of {QUEST_SETTLEMENT.LINK} has found your equipment you must find {?QUEST_GIVER.GENDER}her{?}him{\\?}. Otherwise they will sell it.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", QuestGiver.CharacterObject, null, textObject, false);
                    StringHelpers.SetSettlementProperties("QUEST_SETTLEMENT", QuestGiver.CurrentSettlement, textObject, false);
                    return textObject;
                }
            }

            private TextObject OnQuestSucceededLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=CEEVENTS1076}You have recovered your equipment back from {QUEST_GIVER.LINK}.", null);
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", QuestGiver.CharacterObject, null, textObject, false);
                    return textObject;
                }
            }

            private TextObject OnQuestFailedLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=CEEVENTS1075}You have failed to recover your equipment.", null);
                    return textObject;
                }
            }

            private TextObject OnQuestTimedOutLogText
            {
                get
                {
                    TextObject textObject = new TextObject("{=CEEVENTS1074}You have failed to recover your equipment in time.", null);
                    return textObject;
                }
            }

            [SaveableField(10)]
            private readonly Equipment stolenBattleEquipment;

            [SaveableField(11)]
            private readonly Equipment stolenCivilianEquipment;
        }
    }
}