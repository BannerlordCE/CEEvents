#define V100

using CaptivityEvents.Config;
using Helpers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static CaptivityEvents.Helper.CEHelper;

using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;

namespace CaptivityEvents.Issues
{
    // Quest Reference TaleWorlds.CampaignSystem.SandBox.Issues
    public class CEWhereAreMyThingsIssueBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        { }

        public override void SyncData(IDataStore dataStore)
        { }

        public static IssueBase OnStartIssue(in PotentialIssueData potentialIssueData, Hero issueOwner) => new CEWhereAreMyThingsIssue(issueOwner);

        internal class CEWhereAreMyThingsIssue : IssueBase
        {
            protected override int RewardGold => 500 + MathF.Round(1200f * base.IssueDifficultyMultiplier);

            public override bool IsThereAlternativeSolution => false;
            public override bool IsThereLordSolution => false;
            public override TextObject IssueBriefByIssueGiver => new("{=CEEVENTS1087}Been looking at this equipment someone left here.");
            public override TextObject IssueAcceptByPlayer => new("{=CEEVENTS1086}Hey, that looks like my equipment!");
            public override TextObject IssueQuestSolutionExplanationByIssueGiver => new("{=CEEVENTS1085}Well you can pay for it.");
            public override TextObject IssueQuestSolutionAcceptByPlayer => new("{=CEEVENTS1084}Are you serious?");

            public override TextObject Title => new("{=CEEVENTS1089}Missing Equipment");
            public override TextObject Description => new("{=CEEVENTS1088}Someone found some equipment that looks like yours.");

            public CEWhereAreMyThingsIssue(Hero issueOwner) : base(issueOwner, CampaignTime.DaysFromNow(25f))
            {
            }

            protected override void AfterIssueCreation()
            { }

            protected override void OnGameLoad()
            { }

            protected override QuestBase GenerateIssueQuest(string questId)
            {
                float stolenGearDuration = CESettings.Instance?.StolenGearDuration ?? 10f;

                return new CEWhereAreMyThingsIssueQuest(questId, IssueOwner, CampaignTime.DaysFromNow(stolenGearDuration), RewardGold, new Equipment(Hero.MainHero.BattleEquipment), new Equipment(Hero.MainHero.CivilianEquipment));
            }

            public override IssueBase.IssueFrequency GetFrequency() => IssueBase.IssueFrequency.Rare;

            public override bool IssueStayAliveConditions() => true;

            protected override void CompleteIssueWithTimedOutConsequences()
            { }

            protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
            {
                bool flag2 = issueGiver.GetRelationWithPlayer() >= -10f;

                flag = flag2
                    ? PreconditionFlags.None
                    : PreconditionFlags.Relation;
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

                    if (QuestGiver.CurrentSettlement.IsVillage) textObject = GameTexts.FindText("str_CE_quest_found", "village");
                    else if (QuestGiver.CurrentSettlement.IsTown) textObject = GameTexts.FindText("str_CE_quest_found", "town");
                    else textObject = GameTexts.FindText("str_CE_quest_found");
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
                _stolenBattleEquipment = stolenBattleEquipment;
                _stolenCivilianEquipment = stolenCivilianEquipment;
                AddTrackedObject(QuestGiver);
                SetDialogs();
                InitializeQuestOnCreation();
                OnQuestAccepted();
            }

            protected override void InitializeQuestOnGameLoad()
            {
                AddTrackedObject(QuestGiver);
                SetDialogs();
            }

            protected override void RegisterEvents()
            { }

            protected override void SetDialogs()
            {
                OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start").NpcLine("{=CEEVENTS1080}I am serious.").Condition(() => Hero.OneToOneConversationHero == QuestGiver).Consequence(OnQuestAccepted).CloseDialog();

                DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss").NpcLine(new TextObject("{=CEEVENTS1079}Have you come here to claim your equipment?")).Condition(() => CharacterObject.OneToOneConversationCharacter == QuestGiver.CharacterObject).BeginPlayerOptions().PlayerOption(new TextObject("{=CEEVENTS1077}Yes, I came for my things.")).NpcLine(new TextObject("{=CEEVENTS1078}Here you go {?PLAYER.GENDER}milady{?}sir{\\?}.")).Consequence(CompleteQuestWithSuccess).CloseDialog();
            }

            private void OnQuestAccepted()
            {
                StartQuest();
                AddLog(PlayerAcceptedQuestLogText);
            }

            // Quest Conditions
            protected override void OnCompleteWithSuccess()
            {
                AddLog(OnQuestSucceededLogText);

                foreach (EquipmentCustomIndex index in Enum.GetValues(typeof(EquipmentCustomIndex)))
                {
                    EquipmentIndex i = (EquipmentIndex)index;

                    try
                    {
                        if (!Hero.MainHero.BattleEquipment.GetEquipmentFromSlot(i).IsEmpty) PartyBase.MainParty.ItemRoster.AddToCounts(Hero.MainHero.BattleEquipment.GetEquipmentFromSlot(i).Item, 1);
                    }
                    catch (Exception) { }

                    try
                    {
                        if (!Hero.MainHero.CivilianEquipment.GetEquipmentFromSlot(i).IsEmpty) PartyBase.MainParty.ItemRoster.AddToCounts(Hero.MainHero.CivilianEquipment.GetEquipmentFromSlot(i).Item, 1);
                    }
                    catch (Exception) { }
                }

                EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, _stolenBattleEquipment);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, _stolenCivilianEquipment);

                RemoveTrackedObject(QuestGiver);
            }

            public override void OnFailed()
            {
                AddLog(OnQuestFailedLogText);
                RemoveTrackedObject(QuestGiver);
            }

            protected override void OnTimedOut()
            {
                AddLog(OnQuestTimedOutLogText);
                RemoveTrackedObject(QuestGiver);
            }

            // Quest Logs
            private TextObject PlayerAcceptedQuestLogText
            {
                get
                {
                    TextObject textObject = new("{=CEEVENTS1077}{QUEST_GIVER.LINK} of {QUEST_SETTLEMENT.LINK} has found your equipment you must find {?QUEST_GIVER.GENDER}her{?}him{\\?}. Otherwise they will sell it.");
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", QuestGiver.CharacterObject, textObject);
                    StringHelpers.SetSettlementProperties("QUEST_SETTLEMENT", QuestGiver.CurrentSettlement, textObject);

                    return textObject;
                }
            }

            private TextObject OnQuestSucceededLogText
            {
                get
                {
                    TextObject textObject = new("{=CEEVENTS1076}You have recovered your equipment back from {QUEST_GIVER.LINK}.");
                    StringHelpers.SetCharacterProperties("QUEST_GIVER", QuestGiver.CharacterObject, textObject);

                    return textObject;
                }
            }

            private TextObject OnQuestFailedLogText
            {
                get
                {
                    TextObject textObject = new("{=CEEVENTS1075}You have failed to recover your equipment.");

                    return textObject;
                }
            }

            private TextObject OnQuestTimedOutLogText
            {
                get
                {
                    TextObject textObject = new("{=CEEVENTS1074}You have failed to recover your equipment in time.");

                    return textObject;
                }
            }

            [SaveableField(10)]
            private readonly Equipment _stolenBattleEquipment;

            [SaveableField(11)]
            private readonly Equipment _stolenCivilianEquipment;
        }
    }
}