using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace CaptivityEvents.CampaignBehaviors
{
    public class CEPrisonerEscapeCampaignBehavior : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyHeroTick);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyPartyTick);
        }

        public override void SyncData(IDataStore dataStore) { }

        public void DailyHeroTick(Hero hero)
        {
            if (!hero.IsPrisoner || hero.PartyBelongedToAsPrisoner == null || hero == Hero.MainHero) return;

            if (CEApplyHeroChanceToEscape(hero)) return;

            float num = 0.075f;
            if (hero.PartyBelongedToAsPrisoner.IsMobile) num *= 6f - (float)Math.Pow(Math.Min(81, hero.PartyBelongedToAsPrisoner.NumberOfHealthyMembers), 0.25);

            if (hero.PartyBelongedToAsPrisoner == PartyBase.MainParty || hero.PartyBelongedToAsPrisoner.IsSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan)
            {
                num *= hero.PartyBelongedToAsPrisoner.IsSettlement
                    ? 0.5f
                    : 0.33f;
            }

            if (MBRandom.RandomFloat < num) EndCaptivityAction.ApplyByEscape(hero);
        }

        private bool CEApplyHeroChanceToEscape(Hero hero)
        {
            if (CESettings.Instance == null) return false;

            bool inSettlement = hero.PartyBelongedToAsPrisoner.IsSettlement;
            if (hero.PartyBelongedToAsPrisoner.LeaderHero == Hero.MainHero || inSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan)
            {
                int numEscapeChance = inSettlement ? CESettings.Instance.PrisonerHeroEscapeChanceSettlement : CESettings.Instance.PrisonerHeroEscapeChanceParty;
                if (numEscapeChance == -1) return false;
                if (MBRandom.RandomInt(100) < numEscapeChance) EndCaptivityAction.ApplyByEscape(hero);
            }
            else
            {
                int numEscapeChance = CESettings.Instance.PrisonerHeroEscapeChanceOther;
                if (numEscapeChance == -1) return false;
                if (MBRandom.RandomInt(100) < numEscapeChance) EndCaptivityAction.ApplyByEscape(hero);
            }
            return true;
        }

        public void HourlyPartyTick(MobileParty mobileParty)
        {
            int prisonerSizeLimit = mobileParty.Party.PrisonerSizeLimit;

            if (mobileParty.PrisonRoster.TotalManCount <= prisonerSizeLimit) return;
            int num = mobileParty.PrisonRoster.TotalManCount - prisonerSizeLimit;

            for (int i = 0; i < num; i++)
            {
                int totalManCount = mobileParty.PrisonRoster.TotalManCount;
                bool flag = mobileParty.PrisonRoster.TotalRegulars > 0;
                float randomFloat = MBRandom.RandomFloat;

                int num2 = flag
                    ? (int)(mobileParty.PrisonRoster.TotalRegulars * randomFloat)
                    : (int)(mobileParty.PrisonRoster.TotalManCount * randomFloat);
                CharacterObject character = null;

                foreach (TroopRosterElement troopRosterElement in mobileParty.PrisonRoster)
                {
                    if (!troopRosterElement.Character.IsHero || !flag)
                    {
                        num2 -= troopRosterElement.Number;

                        if (num2 > 0) continue;
                        character = troopRosterElement.Character;

                        break;
                    }
                }

                ApplyEscapeChanceToExceededPrisoners(character, mobileParty);
            }
        }

        private void ApplyEscapeChanceToExceededPrisoners(CharacterObject character, MobileParty capturerParty)
        {
            const float num = 0.1f;

            if (capturerParty.IsGarrison || capturerParty.IsMilitia || character.IsPlayerCharacter) return;

            if (CEApplyExceedChanceToEscape(character, capturerParty)) return;

            if (!(MBRandom.RandomFloat < num)) return;
            if (character.IsHero)
            {
                EndCaptivityAction.ApplyByEscape(character.HeroObject);
                return;
            }
            capturerParty.PrisonRoster.AddToCounts(character, -1);
        }

        private bool CEApplyExceedChanceToEscape(CharacterObject character, MobileParty capturerParty)
        {
            if (CESettings.Instance == null) return false;

            if (!CESettings.Instance.PrisonerExceeded) return true;

            if (character.IsHero)
            {
                int numEscapeChance = capturerParty.LeaderHero == Hero.MainHero ? CESettings.Instance.PrisonerHeroEscapeChanceParty : CESettings.Instance.PrisonerHeroEscapeChanceOther;
                if (numEscapeChance == -1) return false;
                if (MBRandom.RandomInt(100) < numEscapeChance) EndCaptivityAction.ApplyByEscape(character.HeroObject);
            }
            else
            {
                int numEscapeChance = capturerParty.LeaderHero == Hero.MainHero ? CESettings.Instance.PrisonerNonHeroEscapeChanceParty : CESettings.Instance.PrisonerNonHeroEscapeChanceOther;
                if (numEscapeChance == -1) return false;
                if (MBRandom.RandomInt(100) < numEscapeChance) capturerParty.PrisonRoster.AddToCounts(character, -1);

            }

            return true;
        }
    }
}