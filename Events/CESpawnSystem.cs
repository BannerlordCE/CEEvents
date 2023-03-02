#define V102

using CaptivityEvents.Custom;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

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
                            if (troop.Ref != null && troop.Ref.ToLower() == "troop")
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
                    bool isFemale = heroVariables.Gender != null && heroVariables.Gender.ToLower() == "female";

                    string culture = null;
                    if (heroVariables.Culture != null)
                    {
                        culture = heroVariables.Culture.ToLower() switch
                        {
                            "player" => Hero.MainHero.Culture.StringId,
                            "captor" => party.Culture.StringId,
                            _ => heroVariables.Culture,
                        };
                    }
                    else
                    {
                        culture = heroVariables.Culture;
                    }

                    CultureObject cultureObject = MBObjectManager.Instance.GetObjectTypeList<CultureObject>().Where(x => (culture == null && x.IsMainCulture || x.StringId == culture.ToLower())).FirstOrDefault();
                    cultureObject ??= Hero.MainHero.Culture;
                    CharacterObject wanderer = cultureObject.NotableAndWandererTemplates.GetRandomElementWithPredicate((CharacterObject x) => x.Occupation == Occupation.Wanderer && (heroVariables.Gender == null || x.IsFemale == isFemale));
                    Settlement randomElement = Settlement.All.GetRandomElementWithPredicate((Settlement settlement) => settlement.Culture == wanderer.Culture && settlement.IsTown);

                    Hero hero = HeroCreator.CreateSpecialHero(wanderer, randomElement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, -1);

                    GiveGoldAction.ApplyBetweenCharacters(null, hero, 20000, true);
                    hero.SetHasMet();
                    hero.ChangeState(Hero.CharacterStates.Active);
                    if (heroVariables.Clan != null)
                    {
                        switch (heroVariables.Clan.ToLower())
                        {
                            case "captor":
                                AddCompanionAction.Apply(party.Owner.Clan, hero);
                                break;

                            case "player":
                                AddCompanionAction.Apply(Clan.PlayerClan, hero);
                                break;

                            default:
                                break;
                        }
                    }

                    try
                    {
                        if (heroVariables.SkillsToLevel != null)
                        {
                            foreach (SkillToLevel skillToLevel in heroVariables.SkillsToLevel)
                            {
                                int level = 0;
                                int xp = 0;

                                if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByLevel);
                                else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByXP);

                                new Dynamics().SkillModifier(hero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failed to level spawning Hero" + e);
                    }

                    if (heroVariables.Ref == "Prisoner" || heroVariables.Ref == "prisoner")
                    {
                        TakePrisonerAction.Apply(party, hero);
                    }
                    else
                    {
                        if (!party.IsMobile) AddHeroToPartyAction.Apply(hero, party.Settlement.Party.MobileParty, true);
                        else AddHeroToPartyAction.Apply(hero, party.MobileParty, true);
                    }

                    CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed to SpawnTheHero : " + e);
                }
            }
        }
    }
}