using CaptivityEvents.Custom;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Events
{
    internal class CESpawnSystem
    {
        public void SpawnTheTroops(SpawnTroop[] variables, PartyBase party)
        {
            foreach (SpawnTroop troop in variables)
            {
                try
                {
                    int num = new CEVariablesLoader().GetIntFromXML(troop.Number);
                    int numWounded = new CEVariablesLoader().GetIntFromXML(troop.WoundedNumber);
                    CharacterObject characterObject = MBObjectManager.Instance.GetObject<CharacterObject>(troop.Id);

                    if (characterObject == null)
                    {
                        foreach (CharacterObject characterObject2 in MBObjectManager.Instance.GetObjectTypeList<CharacterObject>())
                        {
                            if (characterObject2.Occupation == Occupation.Soldier && string.Equals(characterObject2.Name.ToString(), troop.Id, StringComparison.OrdinalIgnoreCase))
                            {
                                characterObject = characterObject2;
                                break;
                            }
                        }
                    }

                    if (characterObject != null)
                    {
                        if (num > 0)
                        {
                            if (troop.Ref == "Troop")
                            {
                                party.MemberRoster.AddToCounts(characterObject, num, false, numWounded, 0, true, -1);
                            }
                            else
                            {
                                party.PrisonRoster.AddToCounts(characterObject, num, false, numWounded, 0, true, -1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed to SpawnTheTroops : " + e);
                }
            }
        }

        public void SpawnTheHero(SpawnHero[] variables, PartyBase party)
        {

            foreach (SpawnHero heroVariables in variables)
            {
                try
                {
                    bool isFemale = heroVariables.Gender == "Female";

                    string culture = null;
                    switch (heroVariables.Culture)
                    {
                        case "Player":
                            culture = Hero.MainHero.Culture.StringId;
                            break;
                        case "Captor":
                            culture = party.Culture.StringId;
                            break;
                        default:
                            culture = heroVariables.Culture;
                            break;
                    }

                    CharacterObject wanderer = (from x in CharacterObject.Templates
                                                where x.Occupation == Occupation.Wanderer && (culture == null || x.Culture != null && x.Culture.StringId == culture.ToLower()) && (heroVariables.Gender == null || x.IsFemale == isFemale)
                                                select x).GetRandomElement();
                    Settlement randomElement = (from settlement in Settlement.All
                                                where settlement.Culture == wanderer.Culture && settlement.IsTown
                                                select settlement).GetRandomElement();

                    Hero hero = HeroCreator.CreateSpecialHero(wanderer, randomElement, null, null, -1);
                    GiveGoldAction.ApplyBetweenCharacters(null, hero, 20000, true);
                    hero.HasMet = true;
                    hero.Clan = randomElement.OwnerClan;
                    hero.ChangeState(Hero.CharacterStates.Active);
                    switch (heroVariables.Clan)
                    {
                        case "Captor":
                            AddCompanionAction.Apply(party.Owner.Clan, hero);
                            break;
                        case "Player":
                            AddCompanionAction.Apply(Clan.PlayerClan, hero);
                            break;
                        default:
                            break;
                    }
                    CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);

                    if (heroVariables.Ref == "Prisoner")
                    {
                        TakePrisonerAction.Apply(party, hero);
                    }
                    else
                    {
                        if (!party.IsMobile) AddHeroToPartyAction.Apply(hero, party.Settlement.MilitaParty, true);
                        else AddHeroToPartyAction.Apply(hero, party.MobileParty, true);
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed to SpawnTheHero : " + e);
                }
            }
        }
    }
}
