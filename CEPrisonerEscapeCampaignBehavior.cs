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
            if (!CESettings.Instance.PrisonerHeroEscapeAllowed && (hero.PartyBelongedToAsPrisoner.LeaderHero == Hero.MainHero || hero.PartyBelongedToAsPrisoner.IsSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan)) return;

            var num = 0.075f;
            if (hero.PartyBelongedToAsPrisoner.IsMobile) num *= 6f - (float) Math.Pow(Math.Min(81, hero.PartyBelongedToAsPrisoner.NumberOfHealthyMembers), 0.25);

            if (hero.PartyBelongedToAsPrisoner == PartyBase.MainParty || hero.PartyBelongedToAsPrisoner.IsSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan)
                num *= hero.PartyBelongedToAsPrisoner.IsSettlement
                    ? 0.5f
                    : 0.33f;

            if (MBRandom.RandomFloat < num) EndCaptivityAction.ApplyByEscape(hero);
        }

        public void HourlyPartyTick(MobileParty mobileParty)
        {
            var prisonerSizeLimit = mobileParty.Party.PrisonerSizeLimit;

            if (mobileParty.PrisonRoster.TotalManCount <= prisonerSizeLimit) return;
            
            var num = mobileParty.PrisonRoster.TotalManCount - prisonerSizeLimit;

            for (var i = 0; i < num; i++)
            {
                var totalManCount = mobileParty.PrisonRoster.TotalManCount;
                var flag = mobileParty.PrisonRoster.TotalRegulars > 0;
                var randomFloat = MBRandom.RandomFloat;

                var num2 = flag
                    ? (int) (mobileParty.PrisonRoster.TotalRegulars * randomFloat)
                    : (int) (mobileParty.PrisonRoster.TotalManCount * randomFloat);
                CharacterObject character = null;

                foreach (var troopRosterElement in mobileParty.PrisonRoster)
                    if (!troopRosterElement.Character.IsHero || !flag)
                    {
                        num2 -= troopRosterElement.Number;

                        if (num2 > 0) continue;
                            
                        character = troopRosterElement.Character;
                        break;
                    }

                ApplyEscapeChanceToExceededPrisoners(character, mobileParty);
            }
        }

        private void ApplyEscapeChanceToExceededPrisoners(CharacterObject character, MobileParty capturerParty)
        {
            const float num = 0.1f;

            if (capturerParty.IsGarrison || capturerParty.IsMilitia || character.IsPlayerCharacter || character.IsHero && !CESettings.Instance.PrisonerHeroEscapeAllowed || !character.IsHero && !CESettings.Instance.PrisonerNonHeroEscapeAllowed) return;

            if (!(MBRandom.RandomFloat < num)) return;

            if (character.IsHero)
            {
                EndCaptivityAction.ApplyByEscape(character.HeroObject);

                return;
            }

            capturerParty.PrisonRoster.AddToCounts(character, -1);
        }
    }
}