using CaptivityEvents.Models;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace CaptivityEventsTests
{
    [TestFixture]
    public class CEDefaultPregnancyModelTests
    {
        /// <summary>
        ///     GIVEN I need to know the pregnancy chance of a hero
        ///     WHEN the hero is 18 of age
        ///     AND is fertile
        ///     AND have 0 children
        ///     AND have a spouse
        ///     AND perk bonus is 1
        ///     THEN there is 39% chances to be pregnant
        /// </summary>
        [Test]
        public void GIVEN_Hero_WHEN_Age18_IsFertile_Children0_HaveSpouse_PerkBonus1_Then_PregnancyChance39()
        {
            //Arrange
            CEHero hero = new CEHero { Age = 18, IsFertile = true, Children = new List<CEHero>(), Spouse = new CEHero() };
            float perkBonus = 1.0f;
            CEDefaultPregnancyModel sut = new CEDefaultPregnancyModel();
            float expectedResult = 0.39f;

            //Act
            float actualResult = sut.CEGetDailyChanceOfPregnancyForHero(hero, perkBonus);

            //Assert
            actualResult.Should().Be(expectedResult);
        }

        /// <summary>
        ///     GIVEN I need to know the pregnancy chance of a hero
        ///     WHEN the hero is 18 of age
        ///     AND is fertile
        ///     AND have 1 children
        ///     AND have no spouse
        ///     AND perk bonus is 1
        ///     THEN there is a chances to be pregnant
        /// </summary>
        [Test]
        public void GIVEN_Hero_WHEN_Age18_IsFertile_Children1_HaveNoSpouse_PerkBonus1_Then_PregnancyChance0()
        {
            //Arrange
            List<CEHero> child = new List<CEHero> { new CEHero() };
            CEHero hero = new CEHero { Age = 18, IsFertile = true, Children = child, Spouse = null };
            float perkBonus = 1.0f;
            CEDefaultPregnancyModel sut = new CEDefaultPregnancyModel();

            //Act
            float actualResult = sut.CEGetDailyChanceOfPregnancyForHero(hero, perkBonus);

            //Assert
            actualResult.Should().BeGreaterThan(0);
        }

        /// <summary>
        ///     GIVEN I need to know the pregnancy chance of a hero
        ///     WHEN the hero is 18 of age
        ///     AND is not fertile
        ///     THEN there is 0% chances to be pregnant
        /// </summary>
        [Test]
        public void GIVEN_Hero_WHEN_Age18_IsNotFertile_Then_PregnancyChance0()
        {
            //Arrange
            CEHero hero = new CEHero { Age = 18, IsFertile = true, Children = new List<CEHero>(), Spouse = new CEHero() };
            float perkBonus = 1.0f;
            CEDefaultPregnancyModel sut = new CEDefaultPregnancyModel();
            float expectedResult = 0.39f;

            //Act
            float actualResult = sut.CEGetDailyChanceOfPregnancyForHero(hero, perkBonus);

            //Assert
            actualResult.Should().Be(expectedResult);
        }


        /// <summary>
        ///     GIVEN I need to know the pregnancy chance of a hero
        ///     WHEN the hero is 17 of age
        ///     THEN there is 0% chances to be pregnant
        /// </summary>
        [Test]
        public void GIVEN_Hero_WHEN_Age17_Then_PregnancyChance0()
        {
            //Arrange
            CEHero hero = new CEHero { Age = 17, IsFertile = true, Children = new List<CEHero>(), Spouse = new CEHero() };
            float perkBonus = 1.0f;
            CEDefaultPregnancyModel sut = new CEDefaultPregnancyModel();
            float expectedResult = 0.0f;

            //Act
            float actualResult = sut.CEGetDailyChanceOfPregnancyForHero(hero, perkBonus);

            //Assert
            actualResult.Should().Be(expectedResult);
        }


        /// <summary>
        ///     GIVEN I need to know the pregnancy chance of a hero
        ///     WHEN the hero is 45 of age
        ///     AND is fertile
        ///     AND have 0 children
        ///     AND have a spouse
        ///     AND perk bonus is 1
        ///     THEN there is 0.00009% chances to be pregnant
        /// </summary>
        [Test]
        public void GIVEN_Hero_WHEN_Age45_IsFertile_Children0_HaveSpouse_PerkBonus1_Then_PregnancyChanceIsBelow1()
        {
            //Arrange
            CEHero hero = new CEHero { Age = 44, IsFertile = true, Children = new List<CEHero>(), Spouse = new CEHero() };
            float perkBonus = 0.0029f;
            CEDefaultPregnancyModel sut = new CEDefaultPregnancyModel();
            float expectedResult = 9.04799963E-05F;

            //Act
            float actualResult = sut.CEGetDailyChanceOfPregnancyForHero(hero, perkBonus);

            //Assert
            actualResult.Should().Be(expectedResult);
        }


        /// <summary>
        ///     GIVEN I need to know the pregnancy chance of a hero
        ///     WHEN the hero is 46 of age
        ///     THEN there is 0% chances to be pregnant
        /// </summary>
        [Test]
        public void GIVEN_Hero_WHEN_Age46_Then_PregnancyChance0()
        {
            //Arrange
            CEHero hero = new CEHero { Age = 46, IsFertile = true, Children = new List<CEHero>(), Spouse = new CEHero() };
            float perkBonus = 1.0f;
            CEDefaultPregnancyModel sut = new CEDefaultPregnancyModel();
            float expectedResult = 0.0f;

            //Act
            float actualResult = sut.CEGetDailyChanceOfPregnancyForHero(hero, perkBonus);

            //Assert
            actualResult.Should().Be(expectedResult);
        }
    }
}