using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Events
{
    internal class CECompanionSystem
    {

        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;

        private readonly Dynamics _dynamics = new Dynamics();
        private readonly ScoresCalculation _score = new ScoresCalculation();
        private readonly CEImpregnationSystem _impregnation = new CEImpregnationSystem();
        private readonly CEVariablesLoader _variableLoader = new CEVariablesLoader();

        public CECompanionSystem(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
        }

        internal void ConsequenceCompanions(CharacterObject hero, PartyBase party)
        {
            if (_option.Companions != null)
            {
                try
                {
                    foreach (Companion companion in _option.Companions)
                    {
                        Hero referenceHero;
                        List<Hero> heroes = new List<Hero>();

                        if (companion.Ref != null)
                        {
                            switch (companion.Ref.ToLower())
                            {
                                case "hero":
                                    if (!hero.IsHero) { continue; }
                                    referenceHero = hero.HeroObject;
                                    break;
                                case "captor":
                                    if (!party.Leader.IsHero) { continue; }
                                    referenceHero = party.Leader.HeroObject;
                                    break;
                                default:
                                    referenceHero = Hero.MainHero;
                                    break;
                            }

                            if (companion.Type != null)
                            {

                                switch (companion.Type.ToLower())
                                {
                                    case "spouse":
                                        if (referenceHero.Spouse == null) continue;
                                        heroes.Add(referenceHero.Spouse);
                                        break;
                                    case "companion":
                                        if (referenceHero.Clan == null) continue;
                                        foreach (Hero companionHero in referenceHero.Clan.Companions)
                                        {
                                            heroes.Add(companionHero);
                                        }
                                        break;
                                    default:
                                        if (referenceHero.Spouse != null)
                                        {
                                            heroes.Add(referenceHero.Spouse);
                                        }
                                        if (referenceHero.Clan != null)
                                        {
                                            foreach (Hero companionHero in referenceHero.Clan.Companions)
                                            {
                                                heroes.Add(companionHero);
                                            }
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                if (referenceHero.Spouse != null)
                                {
                                    heroes.Add(referenceHero.Spouse);
                                }
                                if (referenceHero.Clan != null)
                                {
                                    foreach (Hero companionHero in referenceHero.Clan.Companions)
                                    {
                                        heroes.Add(companionHero);
                                    }
                                }
                            }
                        }
                        else if (companion.Id != null)
                        {
                            Hero heroCompanion = _listedEvent.SavedCompanions.FirstOrDefault((item) => item.Key == companion.Id).Value;
                            if (hero != null) heroes.Add(heroCompanion);
                        }

                        if (heroes.Count == 0) continue;

                        if (companion.Location != null)
                        {
                            switch (companion.Location.ToLower())
                            {
                                case "prisoner":
                                    heroes = heroes.FindAll((companionHero) => { return companionHero?.PartyBelongedToAsPrisoner != party && companionHero.IsPrisoner; });
                                    break;
                                case "party":
                                    heroes = heroes.FindAll((companionHero) => { return companionHero?.PartyBelongedTo?.Party != null && companionHero.PartyBelongedTo.Party != party && !companionHero.PartyBelongedTo.IsGarrison; });
                                    break;
                                case "settlement":
                                    heroes = heroes.FindAll((companionHero) => { return companionHero?.CurrentSettlement != null; });
                                    break;
                                case "current prisoner":
                                    heroes = heroes.FindAll((companionHero) => { return companionHero?.PartyBelongedToAsPrisoner == party; });
                                    break;
                                case "current":
                                    heroes = heroes.FindAll((companionHero) => { return companionHero?.PartyBelongedTo?.Party == party; });
                                    break;
                                default:
                                    break;
                            }
                            if (heroes.Count == 0) continue;
                        }

                        if (companion.UseOtherConditions != null && companion.UseOtherConditions.ToLower() != "false")
                        {
                            CEEvent triggeredEvent = CEPersistence.CEEventList.Find(item => item.Name == companion.UseOtherConditions);

                            if (triggeredEvent == null) continue;

                            heroes = heroes.FindAll((companionHero) =>
                            {
                                string conditionals = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(companionHero.CharacterObject, party);
                                if (conditionals != null)
                                {
                                    CECustomHandler.LogToFile(conditionals);
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            });
                        }

                        if (heroes.Count == 0) continue;

                        Hero heroSelected = heroes.GetRandomElement();

                        try
                        {
                            ConsequenceForceMarry(companion, heroSelected);
                            ConsequenceLeaveSpouse(companion, heroSelected);
                            ConsequenceGold(companion, heroSelected);
                            ConsequenceChangeGold(companion, heroSelected);
                            ConsequenceChangeCaptorGold(companion, heroSelected);
                            ConsequenceRenown(companion, heroSelected);
                            ConsequenceChangeCaptorRenown(companion, heroSelected);
                            ConsequenceChangeHealth(companion, heroSelected);
                            ConsequenceChangeTrait(companion, heroSelected);
                            ConsequenceChangeSkill(companion, heroSelected);
                            ConsequenceSlaveryFlags(companion, heroSelected);
                            ConsequenceProstitutionFlags(companion, heroSelected);
                            ConsequenceChangeMorale(companion, heroSelected);
                            ConsequenceSpecificCaptorRelations(companion, heroSelected);
                            ConsequenceImpregnation(companion, heroSelected);
                            ConsequenceImpregnationByLeader(companion, heroSelected);
                            ConsequenceImpregnationByPlayer(companion, heroSelected);
                            ConsequenceChangeClan(companion, heroSelected);
                            ConsequenceChangeKingdom(companion, heroSelected);
                            ConsequenceEscape(companion, heroSelected);
                            ConsequenceRelease(companion, heroSelected);
                            ConsequenceWoundPrisoner(companion, heroSelected);
                            ConsequenceKillPrisoner(companion, heroSelected);
                            ConsequenceStrip(companion, heroSelected);
                            ConsequenceGainRandomPrisoners(companion, heroSelected);

                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Incorrect ConsequenceCompanions heroSelected: " + e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Incorrect ConsequenceCompanions: " + e.ToString());
                }
            }
        }

        private void ConsequenceForceMarry(Companion companion, Hero hero)
        {
            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor)) _dynamics.ChangeSpouse(hero, hero?.PartyBelongedToAsPrisoner?.LeaderHero);
        }

        internal void ConsequenceLeaveSpouse(Companion companion, Hero hero)
        {
            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) _dynamics.ChangeSpouse(hero, null);
        }

        internal void ConsequenceGold(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;

            int content = _score.AttractivenessScore(hero);
            int currentValue = hero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= companion.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            GiveGoldAction.ApplyBetweenCharacters(null, hero, content);
        }

        internal void ConsequenceChangeGold(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(companion.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(companion.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, hero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        internal void ConsequenceChangeCaptorGold(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) return;

            if (hero.PartyBelongedToAsPrisoner.LeaderHero == null) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(companion.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(companion.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, hero.PartyBelongedToAsPrisoner.LeaderHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        internal void ConsequenceRenown(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown)) return;

            try
            {
                if (!string.IsNullOrEmpty(companion.RenownTotal)) { _dynamics.RenownModifier(new CEVariablesLoader().GetIntFromXML(companion.RenownTotal), hero); }
                else
                {
                    CECustomHandler.LogToFile("Missing RenownTotal");
                    _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), hero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid RenownTotal"); }
        }

        internal void ConsequenceChangeCaptorRenown(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown)) return;

            if (hero.PartyBelongedToAsPrisoner.LeaderHero == null) return;

            try
            {
                if (!string.IsNullOrEmpty(companion.RenownTotal)) { _dynamics.RenownModifier(new CEVariablesLoader().GetIntFromXML(companion.RenownTotal), hero.PartyBelongedToAsPrisoner.LeaderHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing RenownTotal");
                    _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), hero.PartyBelongedToAsPrisoner.LeaderHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid RenownTotal"); }
        }

        internal void ConsequenceChangeHealth(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth)) return;

            try
            {
                if (!string.IsNullOrEmpty(companion.HealthTotal)) { hero.HitPoints += new CEVariablesLoader().GetIntFromXML(companion.HealthTotal); }
                else
                {
                    CECustomHandler.LogToFile("Invalid HealthTotal");
                    hero.HitPoints += MBRandom.RandomInt(-20, 20);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing HealthTotal"); }
        }

        internal void ConsequenceChangeTrait(Companion companion, Hero hero)
        {
            try
            {
                int level = 0;
                int xp = 0;

                if (companion.TraitsToLevel != null && companion.TraitsToLevel.Count(TraitToLevel => TraitToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (TraitToLevel traitToLevel in companion.TraitsToLevel)
                    {
                        if (traitToLevel.Ref.ToLower() == "captor" && hero.PartyBelongedToAsPrisoner.LeaderHero == null) continue;
                        if (!traitToLevel.ByLevel.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!traitToLevel.ByXP.IsStringNoneOrEmpty()) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(traitToLevel.Ref.ToLower() != "hero" ? hero.PartyBelongedToAsPrisoner.LeaderHero : hero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }

        internal void ConsequenceChangeSkill(Companion companion, Hero hero)
        {
            try
            {
                int level = 0;
                int xp = 0;

                if (companion.SkillsToLevel != null && companion.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in companion.SkillsToLevel)
                    {
                        if (skillToLevel.Ref.ToLower() == "captor" && hero.PartyBelongedToAsPrisoner.LeaderHero == null) continue;
                        if (!skillToLevel.ByLevel.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByLevel);
                        else if (!skillToLevel.ByXP.IsStringNoneOrEmpty()) xp = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(skillToLevel.Ref.ToLower() != "hero" ? hero.PartyBelongedToAsPrisoner.LeaderHero : hero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }

            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        internal void ConsequenceSlaveryFlags(Companion companion, Hero hero)
        {
            bool InformationMessage = !companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) _dynamics.VictimSlaveryModifier(1, hero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
            else if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) _dynamics.VictimSlaveryModifier(0, hero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
        }

        internal void ConsequenceProstitutionFlags(Companion companion, Hero hero)
        {
            bool InformationMessage = !companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) _dynamics.VictimProstitutionModifier(1, hero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
            else if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) _dynamics.VictimProstitutionModifier(0, hero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
        }

        internal void ConsequenceChangeMorale(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            PartyBase party = hero.IsPrisoner
                ? hero.PartyBelongedToAsPrisoner //captive         
                : hero.PartyBelongedTo?.Party; //random, captor

            try
            {
                if (!string.IsNullOrEmpty(companion.MoraleTotal)) { _dynamics.MoraleChange(new CEVariablesLoader().GetIntFromXML(companion.MoraleTotal), party); }
                else
                {
                    CECustomHandler.LogToFile("Missing MoralTotal");
                    _dynamics.MoraleChange(MBRandom.RandomInt(-5, 5), party);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }
        }

        private void ConsequenceSpecificCaptorRelations(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation)) return;
            bool InformationMessage = !companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);


            try
            {
                _dynamics.RelationsModifier(hero, new CEVariablesLoader().GetIntFromXML(companion.RelationTotal), null, InformationMessage && !NoMessages, !InformationMessage && !NoMessages);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RelationTotal");
                _dynamics.RelationsModifier(hero, MBRandom.RandomInt(-5, 5), null, InformationMessage && !NoMessages, !InformationMessage && !NoMessages);
            }
        }

        private void ConsequenceImpregnation(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk)) return;

            try
            {
                if (!string.IsNullOrEmpty(companion.PregnancyRiskModifier))
                {
                    _impregnation.CaptivityImpregnationChance(hero, new CEVariablesLoader().GetIntFromXML(companion.PregnancyRiskModifier), false, false);
                }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(hero, 30, false, false);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
        }

        private void ConsequenceImpregnationByLeader(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero)) return;

            try
            {
                if (!string.IsNullOrEmpty(companion.PregnancyRiskModifier)) { _impregnation.CaptivityImpregnationChance(hero, new CEVariablesLoader().GetIntFromXML(companion.PregnancyRiskModifier)); }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(hero, 30);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
        }

        private void ConsequenceImpregnationByPlayer(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationByPlayer)) return;

            try
            {
                if (!string.IsNullOrEmpty(companion.PregnancyRiskModifier)) { _impregnation.CaptivityImpregnationChance(hero, new CEVariablesLoader().GetIntFromXML(companion.PregnancyRiskModifier), false, false, Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    _impregnation.CaptivityImpregnationChance(hero, 30, false, false, Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid PregnancyRiskModifier"); }
        }

        private void ConsequenceChangeClan(Companion companion, Hero hero)
        {
            if (companion.ClanOptions == null) return;

            if (hero.PartyBelongedToAsPrisoner != null && hero.PartyBelongedToAsPrisoner.LeaderHero != null) _dynamics.ClanChange(companion.ClanOptions, hero, hero.PartyBelongedToAsPrisoner.LeaderHero);
            else _dynamics.ClanChange(companion.ClanOptions, hero, null);
        }

        private void ConsequenceChangeKingdom(Companion companion, Hero hero)
        {
            if (companion.KingdomOptions == null) return;

            if (hero.PartyBelongedToAsPrisoner != null && hero.PartyBelongedToAsPrisoner.LeaderHero != null) _dynamics.KingdomChange(companion.KingdomOptions, hero, hero.PartyBelongedToAsPrisoner.LeaderHero);
            else _dynamics.KingdomChange(companion.KingdomOptions, hero, null);
        }

        private void ConsequenceEscape(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) return;

            try
            {
                EndCaptivityAction.ApplyByEscape(hero);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure of companion escape: " + e.ToString());
            }
        }

        private void ConsequenceWoundPrisoner(Companion companion, Hero hero)
        {
            if (hero.IsPrisoner && companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundPrisoner))
            {
                hero.MakeWounded(null);
            }

            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.WoundCaptor))
            {
                if (!hero.IsPrisoner) hero.MakeWounded();
                else if (hero?.PartyBelongedToAsPrisoner?.LeaderHero != null) hero.PartyBelongedToAsPrisoner.LeaderHero.MakeWounded();
            }
        }

        private void ConsequenceKillPrisoner(Companion companion, Hero hero)
        {
            if (hero.IsPrisoner && companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner))
            {
                if (hero?.PartyBelongedToAsPrisoner?.LeaderHero != null) KillCharacterAction.ApplyByExecution(hero, hero?.PartyBelongedToAsPrisoner?.LeaderHero);
                else KillCharacterAction.ApplyByMurder(hero);
            }

            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor))
            {
                if (!hero.IsPrisoner) KillCharacterAction.ApplyByMurder(hero);
                else if (hero?.PartyBelongedToAsPrisoner?.LeaderHero != null) KillCharacterAction.ApplyByMurder(hero?.PartyBelongedToAsPrisoner?.LeaderHero);
            }
        }

        private void ConsequenceGainRandomPrisoners(Companion companion, Hero hero)
        {
            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) _dynamics.CEGainRandomPrisoners(hero.IsPrisoner ? hero.PartyBelongedToAsPrisoner : hero.PartyBelongedTo.Party);
        }

        private void ConsequenceRelease(Companion companion, Hero hero)
        {
            if (!companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) return;

            try
            {
                EndCaptivityAction.ApplyByReleasing(hero);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure of ConsequenceRelease: " + e.ToString());
            }
        }

        private void ConsequenceStrip(Companion companion, Hero hero)
        {
            if (companion.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Strip))
            {
                if (hero == null) return;
                Equipment randomElement = new Equipment(false);

                ItemObject itemObjectBody = hero.IsFemale
                    ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress")
                    : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                Equipment randomElement2 = new Equipment(true);
                randomElement2.FillFrom(randomElement, false);

                if (CESettings.Instance != null && CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(hero, hero.BattleEquipment, hero.CivilianEquipment);

                foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                {
                    try
                    {
                        if (!hero.BattleEquipment.GetEquipmentFromSlot(i).IsEmpty) PartyBase.MainParty.ItemRoster.AddToCounts(hero.BattleEquipment.GetEquipmentFromSlot(i).Item, 1);
                    }
                    catch (Exception) { }

                    try
                    {
                        if (!hero.CivilianEquipment.GetEquipmentFromSlot(i).IsEmpty) PartyBase.MainParty.ItemRoster.AddToCounts(hero.CivilianEquipment.GetEquipmentFromSlot(i).Item, 1);
                    }
                    catch (Exception) { }
                }

                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElement);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, randomElement2);
            }
        }

    }
}
