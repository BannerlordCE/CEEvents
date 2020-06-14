using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace CaptivityEvents.CampaignBehaviours
{
    public class CEPrisonerEscapeCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(DailyHeroTick));
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, new Action<MobileParty>(HourlyPartyTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public void DailyHeroTick(Hero hero)
        {
            if (hero.IsPrisoner && hero.PartyBelongedToAsPrisoner != null && hero != Hero.MainHero)
            {
                if (!CESettings.Instance.PrisonerHeroEscapeAllowed && (hero.PartyBelongedToAsPrisoner.LeaderHero == Hero.MainHero || hero.PartyBelongedToAsPrisoner.IsSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan))
                {
                    return;
                }

                float num = 0.075f;
                if (hero.PartyBelongedToAsPrisoner.IsMobile)
                {
                    num *= 6f - (float)Math.Pow(Math.Min(81, hero.PartyBelongedToAsPrisoner.NumberOfHealthyMembers), 0.25);
                }
                if (hero.PartyBelongedToAsPrisoner == PartyBase.MainParty || (hero.PartyBelongedToAsPrisoner.IsSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan))
                {
                    num *= (hero.PartyBelongedToAsPrisoner.IsSettlement ? 0.5f : 0.33f);
                }
                if (MBRandom.RandomFloat < num)
                {
                    EndCaptivityAction.ApplyByEscape(hero, null);
                }
            }
        }

        public void HourlyPartyTick(MobileParty mobileParty)
        {
            int prisonerSizeLimit = mobileParty.Party.PrisonerSizeLimit;
            if (mobileParty.PrisonRoster.TotalManCount > prisonerSizeLimit)
            {
                int num = mobileParty.PrisonRoster.TotalManCount - prisonerSizeLimit;
                for (int i = 0; i < num; i++)
                {
                    int totalManCount = mobileParty.PrisonRoster.TotalManCount;
                    bool flag = mobileParty.PrisonRoster.TotalRegulars > 0;
                    float randomFloat = MBRandom.RandomFloat;
                    int num2 = flag ? ((int)(mobileParty.PrisonRoster.TotalRegulars * randomFloat)) : ((int)(mobileParty.PrisonRoster.TotalManCount * randomFloat));
                    CharacterObject character = null;
                    foreach (TroopRosterElement troopRosterElement in mobileParty.PrisonRoster)
                    {
                        if (!troopRosterElement.Character.IsHero || !flag)
                        {
                            num2 -= troopRosterElement.Number;
                            if (num2 <= 0)
                            {
                                character = troopRosterElement.Character;
                                break;
                            }
                        }
                    }
                    ApplyEscapeChanceToExceededPrisoners(character, mobileParty);
                }
            }
        }

        private void ApplyEscapeChanceToExceededPrisoners(CharacterObject character, MobileParty capturerParty)
        {
            float num = 0.1f;
            if (capturerParty.IsGarrison || capturerParty.IsMilitia || character.IsPlayerCharacter || character.IsHero && !CESettings.Instance.PrisonerHeroEscapeAllowed || !character.IsHero && !CESettings.Instance.PrisonerNonHeroEscapeAllowed)
            {
                return;
            }
            if (MBRandom.RandomFloat < num)
            {
                if (character.IsHero)
                {
                    EndCaptivityAction.ApplyByEscape(character.HeroObject, null);
                    return;
                }
                capturerParty.PrisonRoster.AddToCounts(character, -1, false, 0, 0, true, -1);
            }
        }
    }
}