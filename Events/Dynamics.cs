using System;
using CaptivityEvents.Custom;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Events
{
    public class Dynamics
    {
        internal void GainSkills(SkillObject skillObject, int amount, int chance, Hero hero = null)
        {
            if (MBRandom.Random.Next(30) >= chance) return;
            if (hero == null) hero = Hero.MainHero;

            try
            {
                hero.HeroDeveloper.AddSkillXp(skillObject, amount);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed to add to skill");
            }

            //TextObject textObject = new TextObject("{HERO} has learned {SKILL_AMOUNT} {SKILL} XP.", null);
            //textObject.SetTextVariable("HERO", Hero.MainHero.Name);
            //Hero.MainHero.AddSkillXp(skilltoget, 1f);
            //textObject.SetTextVariable("SKILL", skilltoget.Name);
            //textObject.SetTextVariable("SKILL_AMOUNT", amount);
            //InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
        }

        internal void ChangeSpouse(Hero hero, Hero spouseHero)
        {
            var heroSpouse = hero.Spouse;

            if (heroSpouse != null)
            {
                var textObject = GameTexts.FindText("str_CE_spouse_leave");
                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("SPOUSE", heroSpouse.Name);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));

                if (heroSpouse.Father != null) heroSpouse.Clan = heroSpouse.Father.Clan;
                else if (heroSpouse.Mother != null) heroSpouse.Clan = heroSpouse.Mother.Clan;
                hero.Spouse = null;
            }

            if (spouseHero == null) return;
            var spouseHeroSpouse = spouseHero.Spouse;

            if (spouseHeroSpouse != null)
            {
                var textObject3 = GameTexts.FindText("str_CE_spouse_leave");
                textObject3.SetTextVariable("HERO", hero.Name);
                textObject3.SetTextVariable("SPOUSE", spouseHeroSpouse.Name);
                InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                if (spouseHeroSpouse.Father != null) spouseHeroSpouse.Clan = spouseHeroSpouse.Father.Clan;
                else if (spouseHeroSpouse.Mother != null) spouseHeroSpouse.Clan = spouseHeroSpouse.Mother.Clan;
                spouseHero.Spouse = null;
            }

            MarriageAction.Apply(hero, spouseHero);
        }

        internal void TraitModifier(Hero hero, string trait, int amount = 0)
        {
            var found = false;

            foreach (var traitObject in DefaultTraits.Personality)
                if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                {
                    found = true;
                    var currentTraitLevel = hero.GetTraitLevel(traitObject);
                    var newNumber = currentTraitLevel + amount;
                    if (newNumber >= traitObject.MinValue && newNumber <= traitObject.MaxValue) hero.SetTraitLevel(traitObject, newNumber);

                    if (amount == 0) continue;
                    var textObject = GameTexts.FindText("str_CE_trait_level");
                    textObject.SetTextVariable("HERO", hero.Name);

                    textObject.SetTextVariable("POSITIVE", amount >= 0
                                                   ? 1
                                                   : 0);
                    textObject.SetTextVariable("TRAIT", CEStrings.FetchTraitString(trait));
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                }

            if (!found)
                foreach (var traitObject in DefaultTraits.SkillCategories)
                    if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                    {
                        found = true;
                        var currentTraitLevel = hero.GetTraitLevel(traitObject);
                        var newNumber = currentTraitLevel + amount;
                        if (newNumber >= traitObject.MinValue && newNumber <= traitObject.MaxValue) hero.SetTraitLevel(traitObject, newNumber);

                        if (amount == 0) continue;
                        var textObject = GameTexts.FindText("str_CE_trait_level");
                        textObject.SetTextVariable("HERO", hero.Name);

                        textObject.SetTextVariable("POSITIVE", amount >= 0
                                                       ? 1
                                                       : 0);
                        textObject.SetTextVariable("TRAIT", CEStrings.FetchTraitString(trait));
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
                    }

            if (found) return;

            {
                foreach (var traitObject in DefaultTraits.All)
                    if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                    {
                        found = true;
                        var currentTraitLevel = hero.GetTraitLevel(traitObject);
                        var newNumber = currentTraitLevel + amount;
                        if (newNumber >= traitObject.MinValue && newNumber <= traitObject.MaxValue) hero.SetTraitLevel(traitObject, newNumber);

                        if (amount == 0) continue;
                        if (CESettings.Instance != null && !CESettings.Instance.LogToggle) continue;

                        var textObject = GameTexts.FindText("str_CE_trait_level");
                        textObject.SetTextVariable("HERO", hero.Name);

                        textObject.SetTextVariable("POSITIVE", amount >= 0
                                                       ? 1
                                                       : 0);
                        textObject.SetTextVariable("TRAIT", trait);
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                    }

                if (!found) CECustomHandler.ForceLogToFile("Unable to find : " + trait);
            }
        }

        internal void SkillModifier(Hero hero, string skill, int amount = 0)
        {
            foreach (var skillObject in SkillObject.All)
                if (skillObject.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skill)
                {
                    var currentSkillLevel = hero.GetSkillValue(skillObject);
                    var newNumber = currentSkillLevel + amount;
                    if (newNumber < 0) newNumber = 0;

                    hero.SetSkillValue(skillObject, newNumber);

                    if (amount == 0) continue;
                    var textObject = GameTexts.FindText("str_CE_level_skill");
                    textObject.SetTextVariable("HERO", hero.Name);

                    textObject.SetTextVariable("NEGATIVE", amount > 0
                                                   ? 0
                                                   : 1);
                    textObject.SetTextVariable("SKILL_AMOUNT", Math.Abs(amount));

                    textObject.SetTextVariable("PLURAL", amount > 1 || amount < 1
                                                   ? 1
                                                   : 0);
                    textObject.SetTextVariable("SKILL", skill.ToLower());
                    textObject.SetTextVariable("TOTAL_AMOUNT", newNumber);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                }
        }

        internal void VictimSlaveryModifier(int amount, Hero hero, bool updateFlag = false, bool displayMessage = true, bool quickInformation = false)
        {
            if (hero == null) return;
            var slaverySkill = CESkills.Slavery;
            var slaveryFlag = CESkills.IsSlave;

            if (updateFlag)
            {
                var currentLevel = hero.GetSkillValue(slaveryFlag);

                if (amount == 0)
                {
                    if ((displayMessage || quickInformation) && currentLevel != 0)
                    {
                        var textObject = GameTexts.FindText("str_CE_level_leave");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", slaveryFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
                else
                {
                    if ((displayMessage || quickInformation) && currentLevel != 1)
                    {
                        var textObject = GameTexts.FindText("str_CE_level_enter");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", slaveryFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }

                hero.SetSkillValue(slaveryFlag, amount);
            }
            else
            {
                SetModifier(amount, hero, slaverySkill, slaveryFlag);
            }
        }

        private void SetModifier(int amount, Hero hero, SkillObject skill, SkillObject flag, bool displayMessage = true, bool quickInformation = false) //Warning: SkillObject flag never used.
        {
            if (amount == 0)
            {
                if ((displayMessage || quickInformation) && hero.GetSkillValue(skill) > 0)
                {
                    var textObject = GameTexts.FindText("str_CE_level_start");
                    textObject.SetTextVariable("SKILL", skill.Name);
                    textObject.SetTextVariable("HERO", hero.Name);

                    if (hero.GetSkillValue(skill) > 1)
                    {
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }

                hero.SetSkillValue(skill, 0);
            }
            else
            {
                var currentValue = hero.GetSkillValue(skill);
                var valueToSet = currentValue + amount;
                if (valueToSet < 1) valueToSet = 1;
                hero.SetSkillValue(skill, valueToSet);

                if (!displayMessage && !quickInformation) return;
                var textObject = GameTexts.FindText("str_CE_level_skill");
                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("SKILL", skill.Name);

                if (amount >= 0)
                {
                    textObject.SetTextVariable("NEGATIVE", 0);

                    textObject.SetTextVariable("PLURAL", amount >= 2
                                                   ? 1
                                                   : 0);
                }
                else
                {
                    textObject.SetTextVariable("NEGATIVE", 1);

                    textObject.SetTextVariable("PLURAL", amount <= -2
                                                   ? 1
                                                   : 0);
                }

                textObject.SetTextVariable("SKILL_AMOUNT", Math.Abs(amount));
                textObject.SetTextVariable("TOTAL_AMOUNT", currentValue + amount);
                if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                if (quickInformation) InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
            }
        }

        internal void CEKillPlayer(Hero killer)
        {
            GameMenu.ExitToLast();

            try
            {
                if (killer != null) KillCharacterAction.ApplyByMurder(Hero.MainHero, killer);
                else KillCharacterAction.ApplyByMurder(Hero.MainHero);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed CEKillPlayer " + e);
            }
        }

        internal void CEGainRandomPrisoners(PartyBase party)
        {
            var nearest = SettlementHelper.FindNearestSettlement(settlement => settlement.IsVillage);
            //var villagerPartyTemplate = nearest.Culture.VillagerPartyTemplate; //warning never used.
            MBRandom.RandomInt(1, 10);
            party.AddPrisoner(nearest.Culture.VillageWoman, 10, 7);
            party.AddPrisoner(nearest.Culture.Villager, 10, 7);
        }

        internal void VictimProstitutionModifier(int amount, Hero hero, bool updateFlag = false, bool displayMessage = true, bool quickInformation = false)
        {
            if (hero == null) return;
            var prostitutionSkill = CESkills.Prostitution;
            var prostitutionFlag = CESkills.IsProstitute;

            if (updateFlag)
            {
                var currentLevel = hero.GetSkillValue(prostitutionFlag);

                if (amount == 0)
                {
                    if ((displayMessage || quickInformation) && currentLevel != 0)
                    {
                        var textObject = GameTexts.FindText("str_CE_level_leave");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", prostitutionFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
                else
                {
                    if ((displayMessage || quickInformation) && currentLevel != 1)
                    {
                        var textObject = GameTexts.FindText("str_CE_level_enter");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", prostitutionFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) InformationManager.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }

                hero.SetSkillValue(prostitutionFlag, amount);
            }
            else
            {
                SetModifier(amount, hero, prostitutionSkill, prostitutionFlag, displayMessage, quickInformation);
            }
        }

        internal void MoralChange(int amount, PartyBase partyBase)
        {
            if (!partyBase.IsMobile || amount == 0) return;
            var textObject = GameTexts.FindText("str_CE_morale_level");
            textObject.SetTextVariable("PARTY", partyBase.Name);

            textObject.SetTextVariable("POSITIVE", amount >= 0
                                           ? 1
                                           : 0);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
            partyBase.MobileParty.RecentEventsMorale += amount;
        }

        internal void RenownModifier(int amount, Hero hero)
        {
            if (hero == null || amount == 0) return;

            hero.Clan.Renown += amount;
            if (CESettings.Instance != null && hero.Clan.Renown < CESettings.Instance.RenownMin) hero.Clan.Renown = CESettings.Instance.RenownMin;

            var textObject = GameTexts.FindText("str_CE_renown_level");
            textObject.SetTextVariable("HERO", hero.Name);

            textObject.SetTextVariable("POSITIVE", amount >= 0
                                           ? 1
                                           : 0);
            textObject.SetTextVariable("AMOUNT", Math.Abs(amount));
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
        }

        internal void RelationsModifier(Hero hero1, int relationChange, Hero hero2 = null)
        {
            if (hero1 == null || relationChange == 0) return;
            if (hero2 == null) hero2 = Hero.MainHero;

            Campaign.Current.Models.DiplomacyModel.GetHeroesForEffectiveRelation(hero1, hero2, out var hero3, out var hero4);
            var value = CharacterRelationManager.GetHeroRelation(hero3, hero4) + relationChange;
            value = MBMath.ClampInt(value, -100, 100);
            hero3.SetPersonalRelation(hero4, value);

            var textObject = GameTexts.FindText("str_CE_relationship_level");
            textObject.SetTextVariable("PLAYER_HERO", hero2.Name);
            textObject.SetTextVariable("HERO", hero1.Name);

            textObject.SetTextVariable("POSITIVE", relationChange >= 0
                                           ? 1
                                           : 0);
            textObject.SetTextVariable("AMOUNT", Math.Abs(relationChange));
            textObject.SetTextVariable("TOTAL", value);
            InformationManager.AddQuickInformation(textObject, 0, hero1.CharacterObject, "event:/ui/notification/relation");
        }

        internal void ChangeClan(Hero hero, Hero owner)
        {
            if (hero == null) return;

            if (owner != null) hero.Clan = owner.Clan;
        }
    }
}