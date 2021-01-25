using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Helper;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    public class CEImpregnationSystem
    {
        public void ImpregnationChance(Hero targetHero, int modifier = 0, bool forcePreg = false, Hero senderHero = null)
        {
            ScoresCalculation score = new ScoresCalculation();

            if (targetHero != null && targetHero.IsFemale && !targetHero.IsPregnant)
            {
                if (CESettings.Instance != null && (IsHeroAgeSuitableForPregnancy(targetHero) && CESettings.Instance.PregnancyToggle))
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) modifier = 0;

                    if (MBRandom.Random.Next(100)
                        >= (CESettings.Instance.AttractivenessSkill
                            ? score.AttractivenessScore(targetHero) / 20 + modifier
                            : CESettings.Instance.PregnancyChance + modifier))
                    {
                        return;
                    }

                    Hero randomSoldier;

                    if (senderHero != null)
                    {
                        randomSoldier = senderHero;
                    }
                    else if (targetHero.CurrentSettlement?.MilitaParty != null && !targetHero.CurrentSettlement.MilitaParty.MemberRoster.IsEmpty())
                    {
                        Settlement settlementCurrent = targetHero.CurrentSettlement;
                        IEnumerable<TroopRosterElement> maleMembers = settlementCurrent.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);
                        List<TroopRosterElement> troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, settlementCurrent, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    }
                    else if (targetHero.PartyBelongedTo != null)
                    {
                        IEnumerable<TroopRosterElement> maleMembers = targetHero.PartyBelongedTo.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);
                        List<TroopRosterElement> troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedTo.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    }
                    else
                    {
                        CharacterObject m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                        randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    }

                    TextObject textObject3 = GameTexts.FindText("str_CE_impregnated");
                    textObject3.SetTextVariable("HERO", targetHero.Name);
                    textObject3.SetTextVariable("SPOUSE", randomSoldier.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                    CEHelper.spouseOne = randomSoldier;
                    CEHelper.spouseTwo = targetHero;
                    MakePregnantAction.Apply(targetHero);
                    CEHelper.spouseOne = CEHelper.spouseTwo = null;

                    //RelationsModifier(randomSoldier, 50, targetHero);
                }
                else if (forcePreg)
                {
                    CharacterObject m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false).GetRandomElement();
                    Hero randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.BornSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    CEHelper.spouseOne = randomSoldier;
                    CEHelper.spouseTwo = targetHero;
                    MakePregnantAction.Apply(targetHero);
                    CEHelper.spouseOne = CEHelper.spouseTwo = null;
                    TextObject textObject4 = new TextObject("{PLAYER_HERO} forced impregnated by {PLAYER_SPOUSE}.");
                    textObject4.SetTextVariable("PLAYER_HERO", targetHero.Name);
                    textObject4.SetTextVariable("PLAYER_SPOUSE", randomSoldier.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject4.ToString(), Colors.Magenta));
                }
            }
            else if (targetHero != null && !targetHero.IsFemale)
            {
                if (CESettings.Instance != null && !CESettings.Instance.PregnancyToggle) return;
                if (CESettings.Instance != null && !CESettings.Instance.UsePregnancyModifiers) modifier = 0;

                if (CESettings.Instance != null
                    && MBRandom.Random.Next(100)
                    >= (CESettings.Instance.AttractivenessSkill
                        ? score.AttractivenessScore(targetHero) / 20 + modifier
                        : CESettings.Instance.PregnancyChance + modifier))
                {
                    return;
                }

                Hero randomSoldier;

                if (senderHero != null)
                {
                    randomSoldier = senderHero;
                }
                else if (targetHero.CurrentSettlement?.MilitaParty != null && !targetHero.CurrentSettlement.MilitaParty.MemberRoster.IsEmpty())
                {
                    Settlement settlementCurrent = targetHero.CurrentSettlement;
                    IEnumerable<TroopRosterElement> femaleMembers = settlementCurrent.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);
                    List<TroopRosterElement> troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, settlementCurrent, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else if (targetHero.PartyBelongedTo != null)
                {
                    IEnumerable<TroopRosterElement> femaleMembers = targetHero.PartyBelongedTo.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);
                    List<TroopRosterElement> troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedTo.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else
                {
                    CharacterObject m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                    randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                }

                TextObject textObject3 = GameTexts.FindText("str_CE_impregnated");
                textObject3.SetTextVariable("HERO", randomSoldier.Name);
                textObject3.SetTextVariable("SPOUSE", targetHero.Name);
                InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                CEHelper.spouseOne = randomSoldier;
                CEHelper.spouseTwo = targetHero;
                MakePregnantAction.Apply(targetHero);
                CEHelper.spouseOne = CEHelper.spouseTwo = null;

                //RelationsModifier(randomSoldier, 50, targetHero);
            }
        }

        public void CaptivityImpregnationChance(Hero targetHero, int modifier = 0, bool forcePreg = false, bool lord = true, Hero captorHero = null)
        {
            ScoresCalculation scoresCalculation = new ScoresCalculation();


            if (targetHero != null && targetHero.IsFemale && !targetHero.IsPregnant)
            {
                if (CESettings.Instance != null && IsHeroAgeSuitableForPregnancy(targetHero) && CESettings.Instance.PregnancyToggle)
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) modifier = 0;

                    if (MBRandom.Random.Next(100)
                        >= (CESettings.Instance.AttractivenessSkill
                            ? scoresCalculation.AttractivenessScore(targetHero) / 20 + modifier
                            : CESettings.Instance.PregnancyChance + modifier))
                    {
                        return;
                    }

                    Hero randomSoldier;

                    if (captorHero != null)
                    {
                        randomSoldier = captorHero;
                    }
                    else if (lord && CECampaignBehavior.ExtraProps.Owner != null)
                    {
                        randomSoldier = CECampaignBehavior.ExtraProps.Owner;
                    }
                    else if (lord && targetHero.PartyBelongedToAsPrisoner != null && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty?.LeaderHero != null && !targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero.IsFemale)
                    {
                        randomSoldier = targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero;
                    }
                    else if (targetHero.PartyBelongedToAsPrisoner != null && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null)
                    {
                        IEnumerable<TroopRosterElement> maleMembers = targetHero.PartyBelongedToAsPrisoner.MobileParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);
                        List<TroopRosterElement> troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    }
                    else if (targetHero.PartyBelongedToAsPrisoner != null && targetHero.PartyBelongedToAsPrisoner.IsSettlement && targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty != null && !targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty.MemberRoster.IsEmpty())
                    {
                        Settlement playerCaptor = targetHero.PartyBelongedToAsPrisoner.Settlement;
                        IEnumerable<TroopRosterElement> maleMembers = playerCaptor.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);
                        List<TroopRosterElement> troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, playerCaptor, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    }
                    else
                    {
                        CharacterObject m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                        randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    }

                    TextObject textObject3 = GameTexts.FindText("str_CE_impregnated");
                    textObject3.SetTextVariable("HERO", targetHero.Name);
                    textObject3.SetTextVariable("SPOUSE", randomSoldier.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                    CEHelper.spouseOne = randomSoldier;
                    CEHelper.spouseTwo = targetHero;
                    MakePregnantAction.Apply(targetHero);
                    CEHelper.spouseOne = CEHelper.spouseTwo = null;

                    //RelationsModifier(randomSoldier, 50, targetHero);
                }
                else if (forcePreg)
                {
                    CharacterObject m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false).GetRandomElement();
                    Hero randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.BornSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    CEHelper.spouseOne = randomSoldier;
                    CEHelper.spouseTwo = targetHero;
                    MakePregnantAction.Apply(targetHero);
                    CEHelper.spouseOne = CEHelper.spouseTwo = null;
                    TextObject textObject4 = new TextObject("{PLAYER_HERO} forced impregnated by {PLAYER_SPOUSE}.");
                    textObject4.SetTextVariable("PLAYER_HERO", targetHero.Name);
                    textObject4.SetTextVariable("PLAYER_SPOUSE", randomSoldier.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject4.ToString(), Colors.Magenta));
                }
            }
            else if (targetHero != null && !targetHero.IsFemale)
            {
                if (CESettings.Instance != null && !CESettings.Instance.PregnancyToggle) return;
                if (CESettings.Instance != null && !CESettings.Instance.UsePregnancyModifiers) modifier = 0;

                if (CESettings.Instance != null
                    && MBRandom.Random.Next(100)
                    >= (CESettings.Instance.AttractivenessSkill
                        ? scoresCalculation.AttractivenessScore(targetHero) / 20 + modifier
                        : CESettings.Instance.PregnancyChance + modifier))
                {
                    return;
                }

                Hero randomSoldier = null;

                if (captorHero != null)
                {
                    randomSoldier = captorHero;
                    if (!(randomSoldier.IsFemale && !randomSoldier.IsPregnant)) return;
                }
                else if (lord && targetHero.PartyBelongedToAsPrisoner != null && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty?.LeaderHero != null)
                {
                    randomSoldier = targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero;
                    if (!(randomSoldier.IsFemale && !randomSoldier.IsPregnant)) return;
                }
                else if (targetHero.PartyBelongedToAsPrisoner != null && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null)
                {
                    IEnumerable<TroopRosterElement> femaleMembers = targetHero.PartyBelongedToAsPrisoner.MobileParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);
                    List<TroopRosterElement> troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else if (targetHero.PartyBelongedToAsPrisoner != null && targetHero.PartyBelongedToAsPrisoner.IsSettlement && targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty != null && !targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty.MemberRoster.IsEmpty())
                {
                    Settlement playerCaptor = targetHero.PartyBelongedToAsPrisoner.Settlement;
                    IEnumerable<TroopRosterElement> femaleMembers = playerCaptor.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);
                    List<TroopRosterElement> troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        CharacterObject m = troopRosterElements.GetRandomElement().Character;
                        if (targetHero.PartyBelongedToAsPrisoner.MobileParty != null) randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else
                {
                    CharacterObject m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale && characterObject.IsRegular).GetRandomElement();
                    randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                }

                TextObject textObject3 = GameTexts.FindText("str_CE_impregnated");

                if (randomSoldier != null)
                {
                    textObject3.SetTextVariable("HERO", randomSoldier.Name);
                    textObject3.SetTextVariable("SPOUSE", targetHero.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                    CEHelper.spouseOne = randomSoldier;
                }

                CEHelper.spouseTwo = targetHero;
                MakePregnantAction.Apply(randomSoldier);
                CEHelper.spouseOne = CEHelper.spouseTwo = null;

                //RelationsModifier(randomSoldier, 50, targetHero);
            }
        }

        private bool IsHeroAgeSuitableForPregnancy(Hero hero) => hero != null && hero.Age >= 18f && hero.Age <= 45f && !CECampaignBehavior.CheckIfPregnancyExists(hero);
    }
}