using Helpers;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Models
{
    internal class CEDefaultPregnancyModel : PregnancyModel
    {
        public override float CharacterFertilityProbability => 0.95f;

        public override float PregnancyDurationInDays => 3f;

        public override float MaternalMortalityProbabilityInLabor => 0.015f;

        public override float StillbirthProbability => 0.01f;

        public override float DeliveringFemaleOffspringProbability => 0.51f;

        public override float DeliveringTwinsProbability => 0.03f;

        private bool IsHeroAgeSuitableForPregnancy(Hero hero)
        {
            return hero.Age >= 18f && hero.Age <= 45f;
        }

        public override float GetDailyChanceOfPregnancyForHero(Hero hero)
        {
            float num = 0f;
            if (hero.Spouse != null && hero.IsFertile && IsHeroAgeSuitableForPregnancy(hero))
            {
                ExplainedNumber explainedNumber = new ExplainedNumber(1f, null);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.PerfectHealth, hero.Clan.Leader.CharacterObject, ref explainedNumber);
                num = (6.5f - (hero.Age - 18f) * 0.23f) * 0.02f * explainedNumber.ResultNumber;
            }
            if (hero.Children.Count == 0)
            {
                num *= 3f;
            }
            else if (hero.Children.Count == 1)
            {
                num *= 2f;
            }
            return num;
        }

        private const int MinPregnancyAge = 18;

        private const int MaxPregnancyAge = 45;
    }
}