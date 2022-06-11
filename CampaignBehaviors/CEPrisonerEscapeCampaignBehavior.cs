#define V180

using CaptivityEvents.Config;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace CaptivityEvents.CampaignBehaviors
{
    // PrisonerReleaseCampaignBehavior
    public class CEPrisonerEscapeCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, DailyHeroTick);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyPartyTick);
        }

        public override void SyncData(IDataStore dataStore)
        { }

        // DailyHeroTick
        public void DailyHeroTick(Hero hero)
        {
            if (!hero.IsPrisoner || hero.PartyBelongedToAsPrisoner == null || hero == Hero.MainHero) return;

            if (CESettings.Instance.EscapeAutoRansom.SelectedIndex == 1 && hero.Clan != null && hero.PartyBelongedToAsPrisoner.MapFaction != null && MBRandom.RandomFloat < 0.1f)
            {
                // DiplomaticBartersBehavior
                IFaction mapFaction = hero.PartyBelongedToAsPrisoner.MapFaction;
                SetPrisonerFreeBarterable setPrisonerFreeBarterable = new(hero, mapFaction.Leader, hero.PartyBelongedToAsPrisoner, hero.Clan.Leader);
                if (setPrisonerFreeBarterable.GetValueForFaction(mapFaction) + setPrisonerFreeBarterable.GetValueForFaction(hero.Clan) > 0)
                {
                    IEnumerable<Barterable> baseBarterables = new Barterable[] { setPrisonerFreeBarterable };

                    BarterData barterData = new(mapFaction.Leader, hero.Clan.Leader, null, null, null, 0, true);
                    barterData.AddBarterGroup(new DefaultsBarterGroup());
                    foreach (Barterable barterable in baseBarterables)
                    {
                        barterable.SetIsOffered(true);
                        barterData.AddBarterable<DefaultsBarterGroup>(barterable, true);
                    }
                    Campaign.Current.BarterManager.ExecuteAIBarter(barterData, mapFaction, hero.Clan, mapFaction.Leader, hero.Clan.Leader);
                    return;
                }
            }

            if (CEApplyHeroChanceToEscape(hero)) return;

            float num = 0.075f;
            if (hero.PartyBelongedToAsPrisoner.IsMobile) num *= 6f - (float)Math.Pow(Math.Min(81, hero.PartyBelongedToAsPrisoner.NumberOfHealthyMembers), 0.25);

            if (hero.PartyBelongedToAsPrisoner == PartyBase.MainParty || hero.PartyBelongedToAsPrisoner.IsSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan)
            {
                num *= hero.PartyBelongedToAsPrisoner.IsSettlement
                    ? 0.5f
                    : 0.33f;
            }

            if (MBRandom.RandomFloat < num)
            {
                EndCaptivityAction.ApplyByEscape(hero);
                return;
            }
        }

        private bool CEApplyHeroChanceToEscape(Hero hero)
        {
            if (CESettings.Instance == null) return false;

            bool inSettlement = hero.PartyBelongedToAsPrisoner.IsSettlement;
            if (hero.PartyBelongedToAsPrisoner.LeaderHero == Hero.MainHero || inSettlement && hero.PartyBelongedToAsPrisoner.Settlement.OwnerClan == Clan.PlayerClan)
            {
                bool customCheck = inSettlement ? CESettings.Instance.PrisonerHeroEscapeSettlement : CESettings.Instance.PrisonerHeroEscapeParty;
                if (!customCheck) return false;
                int numEscapeChance = inSettlement ? CESettings.Instance.PrisonerHeroEscapeChanceSettlement : CESettings.Instance.PrisonerHeroEscapeChanceParty;
                if (MBRandom.RandomInt(100) < numEscapeChance) EndCaptivityAction.ApplyByEscape(hero);
            }
            else
            {
                bool customCheck = CESettings.Instance.PrisonerHeroEscapeOther;
                if (!customCheck) return false;
                int numEscapeChance = CESettings.Instance.PrisonerHeroEscapeChanceOther;
                if (MBRandom.RandomInt(100) < numEscapeChance) EndCaptivityAction.ApplyByEscape(hero);
            }
            return true;
        }

        // HourlyPartyTick
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

                foreach (TroopRosterElement troopRosterElement in mobileParty.PrisonRoster.GetTroopRoster())
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

        // ApplyEscapeChanceToExceededPrisoners
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

            bool isHeroParty = capturerParty.LeaderHero == Hero.MainHero;

            if (character.IsHero)
            {
                bool customCheck = isHeroParty ? CESettings.Instance.PrisonerHeroEscapeParty : CESettings.Instance.PrisonerHeroEscapeOther;
                if (!customCheck) return false;
                int numEscapeChance = isHeroParty ? CESettings.Instance.PrisonerHeroEscapeChanceParty : CESettings.Instance.PrisonerHeroEscapeChanceOther;
                if (MBRandom.RandomInt(100) < numEscapeChance) EndCaptivityAction.ApplyByEscape(character.HeroObject);
            }
            else
            {
                bool customCheck = isHeroParty ? CESettings.Instance.PrisonerNonHeroEscapeParty : CESettings.Instance.PrisonerNonHeroEscapeOther;
                if (!customCheck) return false;
                int numEscapeChance = isHeroParty ? CESettings.Instance.PrisonerNonHeroEscapeChanceParty : CESettings.Instance.PrisonerNonHeroEscapeChanceOther;
                if (MBRandom.RandomInt(100) < numEscapeChance) capturerParty.PrisonRoster.AddToCounts(character, -1);
            }

            return true;
        }
    }
}