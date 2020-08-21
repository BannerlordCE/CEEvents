using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Models
{
    public class CEDefaultPregnancyModel : PregnancyModel
    {
        public override float CharacterFertilityProbability => 0.95f;

        public override float PregnancyDurationInDays => 3f;

        public override float MaternalMortalityProbabilityInLabor => 0.015f;

        public override float StillbirthProbability => 0.01f;

        public override float DeliveringFemaleOffspringProbability => 0.51f;

        public override float DeliveringTwinsProbability => 0.03f;

        private bool IsHeroAgeSuitableForPregnancy(Hero hero) => hero.Age >= 18f && hero.Age <= 45f;

        private bool IsHeroAgeSuitableForPregnancy(CEHero hero) //I created this overload for the unit test example.
=> hero.Age >= 18f && hero.Age <= 45f;


        private float GeneratePregnancyFactorNumber(float age, float explainedNumber) => (6.5f - (age - 18f) * 0.23f) * 0.02f * explainedNumber;


        public override float GetDailyChanceOfPregnancyForHero(Hero hero)
        {
            //Arrange decoupling of dependencies; unit tests can<t run on dependencies because the system isn<t running.
            CEHero h = new CEHero
            {
                Age = hero.Age,
                IsFertile = hero.IsFertile,
                Children = GetListOfHeroesFrom(hero.Children),
                Spouse = GetListOfHeroesFrom(hero.Spouse)
            };

            ExplainedNumber explainedNumber = new ExplainedNumber(1f);

            // 1.5.0
            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.PerfectHealth, hero.Clan.Leader.CharacterObject, true, ref explainedNumber);

            // 1.4.3
            // PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.PerfectHealth, hero.Clan.Leader.CharacterObject, ref explainedNumber);


            float perkBonus = explainedNumber.ResultNumber;

            float result = CEGetDailyChanceOfPregnancyForHero(h, perkBonus);

            return result;

        }

        public float CEGetDailyChanceOfPregnancyForHero(CEHero ceHero, float perkBonus)
        {
            float num = 0f;

            if (ceHero.Spouse != null && ceHero.IsFertile && IsHeroAgeSuitableForPregnancy(ceHero))
            {
                num = GeneratePregnancyFactorNumber(ceHero.Age, perkBonus);
            }

            switch (ceHero.Children.Count)
            {
                case 0:
                    num *= 3f;

                    break;
                case 1:
                    num *= 2f;

                    break;
            }

            return num;
        }


        private List<CEHero> GetListOfHeroesFrom(List<Hero> heroes) => heroes.Select(hero => new CEHero { Age = hero.Age, IsFertile = hero.IsFertile }).ToList();

        private CEHero GetListOfHeroesFrom(Hero hero) => new CEHero { Age = hero.Age, IsFertile = hero.IsFertile };

        private const int MinPregnancyAge = 18;

        private const int MaxPregnancyAge = 45;
    }


    public class CEHero
    {
        public CEHero Spouse;
        public bool IsFertile;
        public float Age;
        public List<CEHero> Children;
    }
}