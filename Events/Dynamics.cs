#define V120

using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;


namespace CaptivityEvents.Events
{
    public class Dynamics
    {
        internal void GainSkills(SkillObject skillObject, int amount, int chance, Hero hero = null)
        {
            if (CEHelper.HelperMBRandom(30) >= chance) return;
            hero ??= Hero.MainHero;

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
            //Hero.MainHero.AddSkillXp(skillObject, 1f);
            //textObject.SetTextVariable("SKILL", skillObject.Name);
            //textObject.SetTextVariable("SKILL_AMOUNT", amount);
            //InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
        }

        /// <summary>
        /// Duplicate Code from TaleWorlds.CampaignSystem.Actions KillCharacterAction
        /// </summary>
        /// <param name="hero"></param>
        internal void RemoveFactionLeader(Hero hero)
        {
            if (hero.Clan == null) return;

            if ((hero.Clan?.Leader) == hero)
            {
                if (hero != Hero.MainHero && hero.Clan.Heroes.Any((Hero x) => !x.IsChild && x != hero && x.IsAlive && x.IsLord))
                {
                    ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(hero.Clan);
                }
                if (hero.Clan.Kingdom != null && hero.Clan.Kingdom.RulingClan == hero.Clan)
                {
                    List<Clan> list = (from t in hero.Clan.Kingdom.Clans
                                       where !t.IsEliminated && t.Leader != hero && !t.IsUnderMercenaryService
                                       select t).ToList();
                    if (list.IsEmpty())
                    {
                        if (!hero.Clan.Kingdom.IsEliminated)
                        {
                            DestroyKingdomAction.ApplyByKingdomLeaderDeath(hero.Clan.Kingdom);
                        }
                    }
                    else if (list.Count > 1)
                    {
                        Clan clanToExclude = (hero.Clan.Leader == hero || hero.Clan.Leader == null) ? hero.Clan : null;
                        hero.Clan.Kingdom.AddDecision(new KingSelectionKingdomDecision(hero.Clan, clanToExclude), true);
                    }
                    else
                    {
                        hero.Clan.Kingdom.RulingClan = list.First();
                        CampaignEventDispatcher.Instance.OnRulingClanChanged(hero.Clan.Kingdom, hero.Clan.Kingdom.RulingClan);
                    }
                }
            }
        }

        internal void ChangeSpouse(Hero hero, Hero spouseHero)
        {
            try
            {
                if (hero == null) return;
                Hero heroSpouse = hero.Spouse;

                if (!hero.IsHumanPlayerCharacter && hero.IsFactionLeader) RemoveFactionLeader(hero);
                else if (spouseHero != null && !spouseHero.IsHumanPlayerCharacter && spouseHero.IsFactionLeader) RemoveFactionLeader(spouseHero);

                if (heroSpouse != null)
                {
                    TextObject textObject = GameTexts.FindText("str_CE_spouse_leave");
                    textObject.SetTextVariable("HERO", hero.Name);
                    textObject.SetTextVariable("SPOUSE", heroSpouse.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));

                    if (heroSpouse.Father != null) heroSpouse.Clan = heroSpouse.Father.Clan;
                    else if (heroSpouse.Mother != null) heroSpouse.Clan = heroSpouse.Mother.Clan;
                    hero.Spouse = null;
                }

                if (spouseHero == null) return;

                if (hero?.Clan == spouseHero?.Clan) return;

                Hero spouseHeroSpouse = spouseHero.Spouse;

                if (spouseHeroSpouse != null)
                {
                    TextObject textObject3 = GameTexts.FindText("str_CE_spouse_leave");
                    textObject3.SetTextVariable("HERO", hero.Name);
                    textObject3.SetTextVariable("SPOUSE", spouseHeroSpouse.Name);
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));

                    if (spouseHeroSpouse.Father != null) spouseHeroSpouse.Clan = spouseHeroSpouse.Father.Clan;
                    else if (spouseHeroSpouse.Mother != null) spouseHeroSpouse.Clan = spouseHeroSpouse.Mother.Clan;
                    spouseHero.Spouse = null;
                }

                MarriageAction.Apply(hero, spouseHero);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed ChangeSpouse " + e + " HERO1: " + hero + " HERO2: " + spouseHero);
            }
        }

        private void TraitObjectModifier(TraitObject traitObject, Color color, Hero hero, string trait, int amount, int xp, bool display)
        {
            if (xp == 0)
            {
                int currentTraitLevel = hero.GetTraitLevel(traitObject);
                int newNumber = currentTraitLevel + amount;
                if (newNumber < (traitObject?.MinValue ?? 0)) newNumber = traitObject?.MinValue ?? 0;

                hero.SetTraitLevel(traitObject, newNumber);

                if (!display) return;
                TextObject textObject = GameTexts.FindText("str_CE_trait_level");
                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("POSITIVE", newNumber >= 0 ? 1 : 0);
                textObject.SetTextVariable("TRAIT", CEStrings.FetchTraitString(trait));
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), color));
            }
            else if (hero == Hero.MainHero)
            {
                try
                {
                    int traitLevel = Hero.MainHero.GetTraitLevel(traitObject);
                    Campaign.Current.PlayerTraitDeveloper.AddTraitXp(traitObject, xp);
                    if (traitLevel != Hero.MainHero.GetTraitLevel(traitObject))
                    {
                        // Reflection One
                        MethodInfo mi = typeof(CampaignEventDispatcher).GetMethod("OnPlayerTraitChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                        mi?.Invoke(CampaignEventDispatcher.Instance, new object[] { traitObject, traitLevel });
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed TraitObjectModifier " + e);
                }
            }
        }

        internal void TraitModifier(Hero hero, string trait, int amount, int xp, bool display = true, string color = "gray")
        {
            bool found = false;

            foreach (TraitObject traitObject in DefaultTraits.Personality)
            {
                if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                {
                    found = true;
                    TraitObjectModifier(traitObject, PickColor(color), hero, trait, amount, xp, display);
                }
            }

            if (!found)
            {
                foreach (TraitObject traitObject in DefaultTraits.SkillCategories)
                {
                    if (traitObject.Name.ToString().Equals(trait, StringComparison.InvariantCultureIgnoreCase) || traitObject.StringId == trait)
                    {
                        found = true;
                        TraitObjectModifier(traitObject, PickColor(color), hero, trait, amount, xp, display);
                    }
                }
            }

            if (!found) CECustomHandler.ForceLogToFile("Unable to find : " + trait);
        }

        private void SkillObjectModifier(SkillObject skillObject, Color color, Hero hero, string skill, int amount, int xp, bool display = true, bool resetSkill = false)
        {
            if (xp == 0)
            {
                int currentSkillLevel = hero.GetSkillValue(skillObject);
                int newNumber = resetSkill ? 0 : currentSkillLevel + amount;
                bool isToggle = false;
                bool wasPositive = false;

                CESkillNode skillNode = CESkills.FindSkillNode(skill);
                if (skillNode != null)
                {
                    int maxLevel = new CEVariablesLoader().GetIntFromXML(skillNode.MaxLevel);
                    int minLevel = new CEVariablesLoader().GetIntFromXML(skillNode.MinLevel);

                    if (maxLevel == 1 && minLevel == 0)
                    {
                        isToggle = true;
                    }

                    wasPositive = amount > 0 || amount == 0 && !isToggle;

                    if (maxLevel != 0 && newNumber > maxLevel)
                    {
                        newNumber = maxLevel;
                        amount = maxLevel - currentSkillLevel;
                    }
                    else if (newNumber < minLevel)
                    {
                        newNumber = minLevel;
                        amount = minLevel - currentSkillLevel;
                    }
                }
                else if (newNumber < 0)
                {
                    newNumber = 0;
                    amount = newNumber - currentSkillLevel;
                }

                float xpToSet = Campaign.Current.Models.CharacterDevelopmentModel.GetXpRequiredForSkillLevel(newNumber);

                int levels = Campaign.Current.Models.CharacterDevelopmentModel.GetSkillLevelChange(hero, skillObject, xpToSet);

                hero.HeroDeveloper.SetInitialSkillLevel(skillObject, newNumber);

                if (levels > 0)
                {
                    //MethodInfo mi = hero.HeroDeveloper.GetType().GetMethod("ChangeSkillLevelFromXpChange", BindingFlags.Instance | BindingFlags.NonPublic);
                    //if (mi != null) mi.Invoke(hero.HeroDeveloper, new object[] { skillObject, levels, false });
                }
                else
                {
                    CEHelper.SetSkillValue(hero, skillObject, newNumber);
                }

                if (!display) return;

                TextObject textObject;

                if (isToggle)
                {
                    textObject = wasPositive ? GameTexts.FindText("str_CE_level_enter") : GameTexts.FindText("str_CE_level_leave");
                    textObject.SetTextVariable("HERO", hero.Name);
                    textObject.SetTextVariable("OCCUPATION", skillObject.Name.ToString());
                }
                else
                {
                    textObject = GameTexts.FindText("str_CE_level_skill");
                    textObject.SetTextVariable("HERO", hero.Name);

                    if (xp == 0)
                        textObject.SetTextVariable("NEGATIVE", wasPositive ? 0 : 1);
                    else
                        textObject.SetTextVariable("NEGATIVE", xp >= 0 ? 0 : 1);

                    textObject.SetTextVariable("SKILL_AMOUNT", Math.Abs(amount));
                    textObject.SetTextVariable("PLURAL", amount > 1 || amount < 1 ? 1 : 0);
                    textObject.SetTextVariable("SKILL", skillObject.Name.ToString().ToLower());
                    textObject.SetTextVariable("TOTAL_AMOUNT", newNumber);
                }

                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), color));
            }
            else
            {
                hero.HeroDeveloper.AddSkillXp(skillObject, xp, true, display);
            }
        }

        internal Color PickColor(string color)
        {
            return color switch
            {
                "Black" or "black" => Colors.Black,
                "White" or "white" => Colors.White,
                "Yellow" or "yellow" => Colors.Yellow,
                "Red" or "red" => Colors.Red,
                "Magenta" or "magenta" => Colors.Magenta,
                "Green" or "green" => Colors.Green,
                "Cyan" or "cyan" => Colors.Cyan,
                _ => Colors.Gray,
            };
        }

        internal void ResetCustomSkill(Hero hero, string skill, bool display = true, string color = "gray")
        {
            bool found = false;

            foreach (SkillObject skillObjectCustom in CESkills.CustomSkills)
            {
                if (skillObjectCustom.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObjectCustom.StringId == skill)
                {
                    found = true;
                    SkillObjectModifier(skillObjectCustom, PickColor(color), hero, skill, 0, 0, display, true);
                    break;
                }
            }

            if (found) return;

            foreach (SkillObject skillObject in Skills.All)
            {
                if (skillObject.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skill)
                {
                    found = true;
                    SkillObjectModifier(skillObject, PickColor(color), hero, skill, 0, 0, display, true);
                    break;
                }
            }

            if (!found) CECustomHandler.ForceLogToFile("Unable to find : " + skill);
        }

        internal void ResetCustomSkills(Hero hero)
        {
            foreach (SkillObject skillObjectCustom in CESkills.CustomSkills)
            {
                SkillObjectModifier(skillObjectCustom, PickColor("gray"), hero, skillObjectCustom.StringId, 0, 0, false, true);
            }
        }

        internal void SkillModifier(Hero hero, string skill, int amount, int xp, bool display = true, string color = "gray")
        {
            bool found = false;

            foreach (SkillObject skillObjectCustom in CESkills.CustomSkills)
            {
                if (skillObjectCustom.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObjectCustom.StringId == skill)
                {
                    found = true;
                    SkillObjectModifier(skillObjectCustom, PickColor(color), hero, skill, amount, xp, display);
                    break;
                }
            }

            if (found) return;

            foreach (SkillObject skillObject in Skills.All)
            {
                if (skillObject.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skill)
                {
                    found = true;
                    SkillObjectModifier(skillObject, PickColor(color), hero, skill, amount, xp, display);
                    break;
                }
            }

            if (!found) CECustomHandler.ForceLogToFile("Unable to find : " + skill);
        }

        private void SetModifier(int amount, Hero hero, SkillObject skill, bool displayMessage = true, bool quickInformation = false)
        {
            if (amount == 0)
            {
                if ((displayMessage || quickInformation) && hero.GetSkillValue(skill) > 0)
                {
                    TextObject textObject = GameTexts.FindText("str_CE_level_start");
                    textObject.SetTextVariable("SKILL", skill.Name);
                    textObject.SetTextVariable("HERO", hero.Name);

                    if (hero.GetSkillValue(skill) > 1)
                    {
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                        if (quickInformation) CEHelper.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
                CEHelper.SetSkillValue(hero, skill, 0);
            }
            else
            {
                int currentValue = hero.GetSkillValue(skill);
                int valueToSet = currentValue + amount;
                if (valueToSet < 1) valueToSet = 1;
                if (valueToSet > 999) valueToSet = 999;

                CEHelper.SetSkillValue(hero, skill, valueToSet);

                if (!displayMessage && !quickInformation) return;
                TextObject textObject = GameTexts.FindText("str_CE_level_skill");
                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("SKILL", skill.Name);

                textObject.SetTextVariable("NEGATIVE", amount >= 0 ? 0 : 1);
                textObject.SetTextVariable("PLURAL", amount >= 2 ? 1 : 0);

                textObject.SetTextVariable("SKILL_AMOUNT", Math.Abs(amount));
                textObject.SetTextVariable("TOTAL_AMOUNT", valueToSet);

                if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
                if (quickInformation) CEHelper.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
            }
        }

        internal void VictimSlaveryModifier(int amount, Hero hero, bool updateFlag = false, bool displayMessage = true, bool quickInformation = false)
        {
            if (hero == null) return;
            SkillObject slaverySkill = CESkills.Slavery;
            SkillObject slaveryFlag = CESkills.IsSlave;

            if (updateFlag)
            {
                int currentLevel = hero.GetSkillValue(slaveryFlag);

                if (amount == 0)
                {
                    if ((displayMessage || quickInformation) && currentLevel != 0)
                    {
                        TextObject textObject = GameTexts.FindText("str_CE_level_leave");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", slaveryFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) CEHelper.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
                else
                {
                    if ((displayMessage || quickInformation) && currentLevel != 1)
                    {
                        TextObject textObject = GameTexts.FindText("str_CE_level_enter");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", slaveryFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) CEHelper.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
                CEHelper.SetSkillValue(hero, slaveryFlag, amount);
            }
            else
            {
                SetModifier(amount, hero, slaverySkill);
            }
        }

        internal void VictimProstitutionModifier(int amount, Hero hero, bool updateFlag = false, bool displayMessage = true, bool quickInformation = false)
        {
            if (hero == null) return;
            SkillObject prostitutionSkill = CESkills.Prostitution;
            SkillObject prostitutionFlag = CESkills.IsProstitute;

            if (updateFlag)
            {
                int currentLevel = hero.GetSkillValue(prostitutionFlag);

                if (amount == 0)
                {
                    if ((displayMessage || quickInformation) && currentLevel != 0)
                    {
                        TextObject textObject = GameTexts.FindText("str_CE_level_leave");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", prostitutionFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) CEHelper.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }
                else
                {
                    if ((displayMessage || quickInformation) && currentLevel != 1)
                    {
                        TextObject textObject = GameTexts.FindText("str_CE_level_enter");
                        textObject.SetTextVariable("HERO", hero.Name);
                        textObject.SetTextVariable("OCCUPATION", prostitutionFlag.Name);
                        if (displayMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));

                        if (quickInformation) CEHelper.AddQuickInformation(textObject, 0, hero.CharacterObject, "event:/ui/notification/relation");
                    }
                }

                CEHelper.SetSkillValue(hero, prostitutionFlag, amount);
            }
            else
            {
                SetModifier(amount, hero, prostitutionSkill, displayMessage, quickInformation);
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

        // KillCharacterAction && MarriageAction
        internal void ClanOption(Hero firstHero, Clan newClan, bool setLeader = false, bool adopt = false)
        {
            if (firstHero.Clan != newClan)
            {
                Clan clan = firstHero.Clan;
                PropertyInfo pi = Campaign.Current.GetType().GetProperty("PlayerDefaultFaction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                try
                {
                    RemoveFactionLeader(firstHero);

                    if (firstHero.PartyBelongedTo != null && clan != null && !firstHero.IsHumanPlayerCharacter)
                    {
                        if (clan.Kingdom != newClan.Kingdom)
                        {
                            if (firstHero.PartyBelongedTo.Army != null)
                            {
                                if (firstHero.PartyBelongedTo.Army.LeaderParty == firstHero.PartyBelongedTo)
                                {
                                    DisbandArmyAction.ApplyByArmyLeaderIsDead(firstHero.PartyBelongedTo.Army);
                                }
                                else
                                {
                                    firstHero.PartyBelongedTo.Army = null;
                                }
                            }
                            IFaction kingdom = newClan.Kingdom;
                            FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(firstHero, kingdom ?? newClan);
                        }

                        if (firstHero.GovernorOf != null)
                        {
                            ChangeGovernorAction.RemoveGovernorOf(firstHero);
                        }

                        if (firstHero.PartyBelongedTo != null)
                        {
                            MobileParty partyBelongedTo = firstHero.PartyBelongedTo;
                            if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == firstHero)
                            {
                                DisbandPartyAction.StartDisband(partyBelongedTo);
                                partyBelongedTo.Party.SetCustomOwner(null);
                            }

                            firstHero.ChangeState(Hero.CharacterStates.Fugitive);
                            MobileParty partyBelongedTo2 = firstHero.PartyBelongedTo;
                            partyBelongedTo2?.MemberRoster.RemoveTroop(firstHero.CharacterObject, 1, default, 0);
                        }
                    }

                    firstHero.Clan = newClan;
                    if (pi != null && firstHero.IsHumanPlayerCharacter) pi.SetValue(Campaign.Current, newClan);
                    if (clan != null)
                    {
                        foreach (Hero hero3 in clan.Heroes)
                        {
                            hero3.UpdateHomeSettlement();
                        }
                    }
                    foreach (Hero hero4 in newClan.Heroes)
                    {
                        hero4.UpdateHomeSettlement();
                    }
                    if (adopt)
                    {
                        if (newClan.Leader.IsFemale)
                        {
                            firstHero.Mother = newClan.Leader;
                            firstHero.Father = newClan.Leader.Spouse;
                        }
                        else
                        {
                            firstHero.Father = newClan.Leader;
                            firstHero.Mother = newClan.Leader.Spouse;
                        }
                    }
                    if (setLeader)
                    {
                        if (firstHero.IsHumanPlayerCharacter)
                        {
                            ChangeClanLeaderAction.ApplyWithSelectedNewLeader(newClan, firstHero);
                        }
                        else
                        {
                            newClan.SetLeader(firstHero);
                        }
                    }
                }
                catch (Exception)
                {
                    firstHero.Clan = clan;
                    if (pi != null && firstHero.IsHumanPlayerCharacter) pi.SetValue(Campaign.Current, clan);
                }
            }
        }

        private Clan ChangeClanName(Clan clan, TextObject clanName, TextObject informalName)
        {
            clan.ChangeClanName(clanName, informalName);
            return clan;
        }

        internal void ClanChange(ClanOption[] clanOptions, Hero hero = null, Hero captor = null)
        {
            foreach (ClanOption clanOption in clanOptions)
            {
                try
                {
                    Clan clan = null;
                    TextObject clanName = null;
                    Banner banner = null;
                    Hero leader = null;

                    if (clanOption.Clan != null)
                    {
                        switch (clanOption.Clan.ToLower())
                        {
                            case "new":
                                clanName = clanOption.Ref.ToLower() == "captor" ? captor.Culture.ClanNameList.GetRandomElement() : hero.Culture.ClanNameList.GetRandomElement();
                                banner = Banner.CreateRandomClanBanner();
                                leader = clanOption.Ref.ToLower() == "captor" ? captor : hero;
                                break;

                            case "random":
                                clan = Clan.All.GetRandomElement();
                                break;

                            case "hero":
                                clan = hero?.Clan;
                                if (clan == null)
                                {
                                    clanName = new TextObject(hero.Name + "'s Slaves");
                                    banner = Banner.CreateRandomClanBanner();
                                    leader = hero;
                                }
                                break;

                            case "captor":
                                clan = captor?.Clan;
                                if (clan == null)
                                {
                                    clanName = new TextObject(captor.Name + "'s Slaves");
                                    banner = Banner.CreateRandomClanBanner();
                                    leader = captor;
                                }
                                break;

                            case "settlement":
                                clan = clanOption.Ref.ToLower() == "captor" ? captor.CurrentSettlement.OwnerClan : hero.CurrentSettlement.OwnerClan;
                                break;
                        }
                    }

                    if (clan == null)
                    {
                        if (clanOption.Ref.ToLower() == "captor")
                        {
                            if (captor?.Clan != null)
                            {
                                PropertyInfo pi = captor.Clan.GetType().GetProperty("Banner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                ChangeClanName(captor.Clan, clanName, clanName);
                                pi?.SetValue(captor.Clan, banner);
                                captor.Clan.SetLeader(leader);
                            }
                        }
                        else if (hero?.Clan != null)
                        {
                            PropertyInfo pi = hero.Clan.GetType().GetProperty("Banner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            ChangeClanName(hero.Clan, clanName, clanName);
                            pi?.SetValue(hero.Clan, banner);
                            hero.Clan.SetLeader(leader);
                        }

                        TextObject text = GameTexts.FindText("str_CE_clan", clanOption.Action.ToLower());
                        text.SetTextVariable("HERO", clanOption.Ref.ToLower() == "captor" ? captor?.Name : hero?.Name);
                        text.SetTextVariable("CLAN", clanName);
                        InformationManager.DisplayMessage(new InformationMessage(text.ToString(), Colors.Magenta));

                        CECustomHandler.ForceLogToFile("Failed ClanChange : clan is null ");
                        return;
                    }

                    if (!clanOption.HideNotification)
                    {
                        TextObject text = GameTexts.FindText("str_CE_clan", clanOption.Action.ToLower());
                        text.SetTextVariable("HERO", clanOption.Ref.ToLower() == "captor" ? captor?.Name : hero?.Name);
                        text.SetTextVariable("CLAN", clan?.Name);
                        InformationManager.DisplayMessage(new InformationMessage(text.ToString(), Colors.Magenta));
                    }

                    switch (clanOption.Action.ToLower())
                    {
                        case "join":
                            ClanOption(clanOption.Ref.ToLower() == "captor" ? captor : hero, clan);
                            break;

                        case "joinasleader":
                            ClanOption(clanOption.Ref.ToLower() == "captor" ? captor : hero, clan, true);
                            break;

                        case "adopted":
                            ClanOption(clanOption.Ref.ToLower() == "captor" ? captor : hero, clan, false, true);
                            break;

                        case "adoptedasleader":
                            ClanOption(clanOption.Ref.ToLower() == "captor" ? captor : hero, clan, false, true);
                            break;
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed ClanChange " + e);
                }
            }
        }

        internal void KingdomChange(KingdomOption[] kingdomOptions, Hero hero = null, Hero captor = null)
        {
            foreach (KingdomOption kingdomOption in kingdomOptions)
            {
                try
                {
                    Kingdom kingdom = null;

                    if (kingdomOption.Kingdom != null)
                    {
                        switch (kingdomOption.Kingdom.ToLower())
                        {
                            case "random":
                                kingdom = Kingdom.All.GetRandomElement();
                                break;

                            case "hero":
                                kingdom = hero.Clan.Kingdom;
                                break;

                            case "captor":
                                kingdom = captor.Clan.Kingdom;
                                break;

                            case "settlement":
                                kingdom = kingdomOption.Ref.ToLower() == "captor" ? captor.CurrentSettlement.OwnerClan.Kingdom : hero.CurrentSettlement.OwnerClan.Kingdom;
                                break;
                        }
                    }

                    switch (kingdomOption.Action.ToLower())
                    {
                        case "leave":
                            ChangeKingdomAction.ApplyByLeaveKingdom(kingdomOption.Ref.ToLower() == "captor" ? captor.Clan : hero.Clan, !kingdomOption.HideNotification);
                            break;

                        case "join":
                            ChangeKingdomAction.ApplyByJoinToKingdom(kingdomOption.Ref.ToLower() == "captor" ? captor.Clan : hero.Clan, kingdom, !kingdomOption.HideNotification);
                            break;

                        case "joinasmercenary":
                            ChangeKingdomAction.ApplyByJoinFactionAsMercenary(kingdomOption.Ref.ToLower() == "captor" ? captor.Clan : hero.Clan, kingdom, 50, !kingdomOption.HideNotification);
                            break;
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed KingdomChange " + e);
                }
            }
        }

        internal void CEWoundPrisoners(PartyBase party, int amount = 10)
        {
            try
            {
                if (amount == 0) return;
                int prisonerCount = party.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                party.PrisonRoster.WoundNumberOfTroopsRandomly(amount);
                TextObject textObject = GameTexts.FindText("str_CE_wound_prisoners");
                textObject.SetTextVariable("HERO", party.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't wound any prisoners.");
            }
        }

        internal void CEKillPrisoners(PartyBase party, int amount = 10, bool killHeroes = false)
        {
            try
            {
                if (amount == 0) return;
                int prisonerCount = party.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                party.PrisonRoster.KillNumberOfMenRandomly(amount, killHeroes);
                TextObject textObject = GameTexts.FindText("str_CE_kill_prisoners");
                textObject.SetTextVariable("HERO", party.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't kill any prisoners.");
            }
        }

        internal void CEWoundTroops(PartyBase party, int amount = 10)
        {
            try
            {
                if (amount == 0) return;
                int prisonerCount = party.MemberRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                party.MemberRoster.WoundNumberOfTroopsRandomly(amount);
                TextObject textObject = GameTexts.FindText("str_CE_wound_troops");
                textObject.SetTextVariable("PARTY", party.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("CEWoundTroops Couldn't wound any troops.");
            }
        }

        internal void CEKillTroops(PartyBase party, int amount = 10, bool killHeroes = false)
        {
            try
            {
                if (amount == 0) return;
                int prisonerCount = party.MemberRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                party.MemberRoster.KillNumberOfMenRandomly(amount, killHeroes);
                TextObject textObject = GameTexts.FindText("str_CE_kill_troops");
                textObject.SetTextVariable("PARTY", party.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("CEKillTroops Couldn't kill any troops.");
            }
        }

        internal void CEGainRandomPrisoners(PartyBase party)
        {
            Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => settlement.IsVillage);
            //PartyTemplateObject villagerPartyTemplate = nearest.Culture.VillagerPartyTemplate; Will be used in figuring out on what to give
            MBRandom.RandomInt(1, 10);

            party.PrisonRoster.AddToCounts(nearest.Culture.VillageWoman, 10, false, 7);
            party.PrisonRoster.AddToCounts(nearest.Culture.Villager, 10, false, 7);
        }

        internal void MoraleChange(int amount, PartyBase partyBase)
        {
            if (!partyBase.IsMobile || amount == 0) return;
            TextObject textObject = GameTexts.FindText("str_CE_morale_level");
            textObject.SetTextVariable("PARTY", partyBase.Name);

            textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
            partyBase.MobileParty.RecentEventsMorale += amount;
        }

        internal void RenownModifier(int amount, Hero hero, bool displayMessage = true)
        {
            try
            {
                if (hero == null || amount == 0) return;

                if ((CESettings.Instance?.RenownChoice?.SelectedIndex ?? 1) == 0) return;

                float renown = hero.Clan.Renown + amount;
                float min = (CESettings.Instance?.RenownChoice?.SelectedIndex) switch
                {
                    0 => 0,
                    2 => Campaign.Current.Models.ClanTierModel.GetRequiredRenownForTier(hero.Clan.Tier),
                    _ => CESettings.Instance?.RenownMin ?? -150f,
                };
                if (renown < min) renown = min;


                if (renown < 0)
                {
                    hero.Clan.Renown = renown;
                }
                else
                {
                    hero.Clan.ResetClanRenown();
                    hero.Clan.AddRenown(renown, false);
                }

                if (!displayMessage) return;

                TextObject textObject = GameTexts.FindText("str_CE_renown_level");

                textObject.SetTextVariable("HERO", hero.Name);
                textObject.SetTextVariable("POSITIVE", amount >= 0 ? 1 : 0);
                textObject.SetTextVariable("AMOUNT", Math.Abs(amount));

                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed RenownModifier " + e);
            }
        }

        internal void RelationsModifier(Hero hero1, int relationChange, Hero hero2 = null, bool quickInformationMessage = true, bool regularMessage = false)
        {
            if (hero1 == null || relationChange == 0) return;
            hero2 ??= Hero.MainHero;

            Campaign.Current.Models.DiplomacyModel.GetHeroesForEffectiveRelation(hero1, hero2, out Hero hero3, out Hero hero4);
            if (hero3 == null || hero4 == null)
            {
                hero3 = hero1;
                hero4 = hero2;
            }
            int value = CharacterRelationManager.GetHeroRelation(hero3, hero4) + relationChange;
            value = MBMath.ClampInt(value, -100, 100);
            hero3.SetPersonalRelation(hero4, value);

            TextObject textObject = GameTexts.FindText("str_CE_relationship_level");
            textObject.SetTextVariable("PLAYER_HERO", hero2.Name);
            textObject.SetTextVariable("HERO", hero1.Name);

            textObject.SetTextVariable("POSITIVE", relationChange >= 0 ? 1 : 0);
            textObject.SetTextVariable("AMOUNT", Math.Abs(relationChange));
            textObject.SetTextVariable("TOTAL", value);
            if (quickInformationMessage) CEHelper.AddQuickInformation(textObject, 0, hero1.CharacterObject, "event:/ui/notification/relation");
            if (regularMessage) InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
        }
    }
}