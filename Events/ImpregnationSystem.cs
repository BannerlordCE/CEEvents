using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Helper;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    public class ImpregnationSystem
    {
        public void ImpregnationChance(Hero targetHero, int modifier = 0, bool forcePreg = false, Hero senderHero = null)
        {
            var score = new ScoresCalculation();

            if (targetHero != null && targetHero.IsFemale && !targetHero.IsPregnant)
            {
                if (CESettings.Instance != null && (IsHeroAgeSuitableForPregnancy(targetHero) && CESettings.Instance.PregnancyToggle))
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) modifier = 0;

                    if (MBRandom.Random.Next(100)
                        >= (CESettings.Instance.AttractivenessSkill
                            ? score.AttractivenessScore(targetHero) / 20 + modifier
                            : CESettings.Instance.PregnancyChance + modifier)) return;
                    Hero randomSoldier;

                    if (senderHero != null)
                    {
                        randomSoldier = senderHero;
                    }
                    else if (targetHero.CurrentSettlement?.MilitaParty != null && !targetHero.CurrentSettlement.MilitaParty.MemberRoster.IsEmpty())
                    {
                        var settlementCurrent = targetHero.CurrentSettlement;
                        var maleMembers = settlementCurrent.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);

                        var troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, settlementCurrent, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                        randomSoldier.IsNoble = true;
                    }
                    else if (targetHero.PartyBelongedTo != null)
                    {
                        var maleMembers = targetHero.PartyBelongedTo.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);

                        var troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedTo.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                        randomSoldier.IsNoble = true;
                    }
                    else
                    {
                        var m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                        randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                        randomSoldier.IsNoble = true;
                    }

                    var textObject3 = GameTexts.FindText("str_CE_impregnated");
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
                    var m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false).GetRandomElement();
                    var randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.BornSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    CEHelper.spouseOne = randomSoldier;
                    CEHelper.spouseTwo = targetHero;
                    MakePregnantAction.Apply(targetHero);
                    CEHelper.spouseOne = CEHelper.spouseTwo = null;
                    var textObject4 = new TextObject("{PLAYER_HERO} forced impregnated by {PLAYER_SPOUSE}.");
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
                        : CESettings.Instance.PregnancyChance + modifier)) return;
                
                Hero randomSoldier;

                if (senderHero != null)
                {
                    randomSoldier = senderHero;
                }
                else if (targetHero.CurrentSettlement?.MilitaParty != null && !targetHero.CurrentSettlement.MilitaParty.MemberRoster.IsEmpty())
                {
                    var settlementCurrent = targetHero.CurrentSettlement;
                    var femaleMembers = settlementCurrent.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);

                    var troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, settlementCurrent, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);

                        randomSoldier.IsNoble = true;
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else if (targetHero.PartyBelongedTo != null)
                {
                    var femaleMembers = targetHero.PartyBelongedTo.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);

                    var troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedTo.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                        randomSoldier.IsNoble = true;
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else
                {
                    var m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                    randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                    randomSoldier.IsNoble = true;
                }

                var textObject3 = GameTexts.FindText("str_CE_impregnated");
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
            var score = new ScoresCalculation();


            if (targetHero != null && targetHero.IsFemale && !targetHero.IsPregnant)
            {
                if (CESettings.Instance != null && IsHeroAgeSuitableForPregnancy(targetHero) && CESettings.Instance.PregnancyToggle)
                {
                    if (!CESettings.Instance.UsePregnancyModifiers) modifier = 0;

                    if (MBRandom.Random.Next(100)
                        >= (CESettings.Instance.AttractivenessSkill
                            ? score.AttractivenessScore(targetHero) / 20 + modifier
                            : CESettings.Instance.PregnancyChance + modifier)) return;
                    Hero randomSoldier;

                    if (captorHero != null)
                    {
                        randomSoldier = captorHero;
                    }
                    else if (lord && CECampaignBehavior.ExtraProps.Owner != null)
                    {
                        randomSoldier = CECampaignBehavior.ExtraProps.Owner;
                    }
                    else if (lord && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty?.LeaderHero != null && !targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero.IsFemale)
                    {
                        randomSoldier = targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero;
                    }
                    else if (targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null)
                    {
                        var maleMembers = targetHero.PartyBelongedToAsPrisoner.MobileParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);

                        var troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                        randomSoldier.IsNoble = true;
                    }
                    else if (targetHero.PartyBelongedToAsPrisoner.IsSettlement && targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty != null && !targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty.MemberRoster.IsEmpty())
                    {
                        var playerCaptor = targetHero.PartyBelongedToAsPrisoner.Settlement;
                        var maleMembers = playerCaptor.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale == false);

                        var troopRosterElements = maleMembers.ToList();

                        if (!troopRosterElements.Any()) return;

                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, playerCaptor, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                        randomSoldier.IsNoble = true;
                    }
                    else
                    {
                        var m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                        randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                        randomSoldier.IsNoble = true;
                    }

                    var textObject3 = GameTexts.FindText("str_CE_impregnated");
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
                    var m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale == false).GetRandomElement();
                    var randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.BornSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(20) + 20);
                    CEHelper.spouseOne = randomSoldier;
                    CEHelper.spouseTwo = targetHero;
                    MakePregnantAction.Apply(targetHero);
                    CEHelper.spouseOne = CEHelper.spouseTwo = null;
                    var textObject4 = new TextObject("{PLAYER_HERO} forced impregnated by {PLAYER_SPOUSE}.");
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
                        : CESettings.Instance.PregnancyChance + modifier)) return;
                Hero randomSoldier = null;

                if (captorHero != null)
                {
                    randomSoldier = captorHero;
                }
                else if (lord && targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty?.LeaderHero != null && targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero.IsFemale)
                {
                    randomSoldier = targetHero.PartyBelongedToAsPrisoner.MobileParty.LeaderHero;
                }
                else if (targetHero.PartyBelongedToAsPrisoner.IsMobile && targetHero.PartyBelongedToAsPrisoner.MobileParty != null)
                {
                    var femaleMembers = targetHero.PartyBelongedToAsPrisoner.MobileParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);

                    var troopRosterElements = femaleMembers as TroopRosterElement[] ?? femaleMembers.ToArray();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        var m = troopRosterElements.GetRandomElement().Character;
                        randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                        randomSoldier.IsNoble = true;
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else if (targetHero.PartyBelongedToAsPrisoner.IsSettlement && targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty != null && !targetHero.PartyBelongedToAsPrisoner.Settlement.MilitaParty.MemberRoster.IsEmpty())
                {
                    var playerCaptor = targetHero.PartyBelongedToAsPrisoner.Settlement;
                    var femaleMembers = playerCaptor.MilitaParty.MemberRoster.Where(characterObject => characterObject.Character.IsFemale);

                    var troopRosterElements = femaleMembers.ToList();

                    if (!troopRosterElements.Any()) return;

                    do
                    {
                        var m = troopRosterElements.GetRandomElement().Character;
                        if (targetHero.PartyBelongedToAsPrisoner.MobileParty != null) randomSoldier = HeroCreator.CreateSpecialHero(m, targetHero.PartyBelongedToAsPrisoner.MobileParty.HomeSettlement, CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                        if (randomSoldier != null) randomSoldier.IsNoble = true;
                    } while (!IsHeroAgeSuitableForPregnancy(randomSoldier));
                }
                else
                {
                    var m = CharacterObject.Templates.Where(characterObject => characterObject.IsFemale && characterObject.Occupation == Occupation.Outlaw).GetRandomElement();
                    randomSoldier = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, MBRandom.Random.Next(15) + 18);
                    randomSoldier.IsNoble = true;
                }

                var textObject3 = GameTexts.FindText("str_CE_impregnated");

                if (randomSoldier != null)
                {
                    textObject3.SetTextVariable("HERO", randomSoldier.Name);
                    textObject3.SetTextVariable("SPOUSE", targetHero.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                    CEHelper.spouseOne = randomSoldier;
                }

                CEHelper.spouseTwo = targetHero;
                MakePregnantAction.Apply(targetHero);
                CEHelper.spouseOne = CEHelper.spouseTwo = null;

                //RelationsModifier(randomSoldier, 50, targetHero);
            }
        }

        private bool IsHeroAgeSuitableForPregnancy(Hero hero)
        {
            return hero != null && hero.Age >= 18f && hero.Age <= 45f && !CECampaignBehavior.CheckIfPregnancyExists(hero);
        }
    }
}