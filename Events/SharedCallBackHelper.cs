using CaptivityEvents.Custom;
using CaptivityEvents.Issues;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Events
{
    public class SharedCallBackHelper
    {
        private readonly CEEvent _listedEvent;
        private readonly Option _option;

        private readonly Dynamics _dynamics = new Dynamics();
        private readonly ScoresCalculation _score = new ScoresCalculation();

        public SharedCallBackHelper(CEEvent listedEvent, Option option)
        {
            _listedEvent = listedEvent;
            _option = option;
        }


        #region private

        internal void ConsequenceXP()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP)) GiveXP();
        }

        internal void ConsequenceLeaveSpouse()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) _dynamics.ChangeSpouse(Hero.MainHero, null);
        }

        internal void GiveXP()
        {
            try
            {
                string skillToLevel = "";

                if (!string.IsNullOrEmpty(_option.SkillToLevel)) skillToLevel = _option.SkillToLevel;
                else if (!string.IsNullOrEmpty(_listedEvent.SkillToLevel)) skillToLevel = _listedEvent.SkillToLevel;
                else CECustomHandler.LogToFile("Missing SkillToLevel");

                foreach (SkillObject skillObject in SkillObject.All.Where(skillObject => skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)) _dynamics.GainSkills(skillObject, 50, 100);
            }
            catch (Exception) { CECustomHandler.LogToFile("GiveXP Failed"); }
        }

        internal void ConsequenceGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;

            int content = _score.AttractivenessScore(Hero.MainHero);
            int currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
        }

        internal void ConsequenceChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        internal void ConsequenceChangeTrait()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait)) return;

            try
            {
                int level = 0;
                int xp = 0;

                if (!string.IsNullOrEmpty(_option.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.TraitTotal);
                else if (!string.IsNullOrEmpty(_option.TraitXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_option.TraitXPTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.TraitXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitXPTotal);
                else CECustomHandler.LogToFile("Missing Trait TraitTotal");

                if (!string.IsNullOrEmpty(_option.TraitToLevel)) _dynamics.TraitModifier(Hero.MainHero, _option.TraitToLevel, level, xp);
                else if (!string.IsNullOrEmpty(_listedEvent.TraitToLevel)) _dynamics.TraitModifier(Hero.MainHero, _listedEvent.TraitToLevel, level, xp);
                else CECustomHandler.LogToFile("Missing TraitToLevel");
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }


        internal void ConsequenceChangeSkill()
        {
            try
            {
                int level = 0;
                int xp = 0;

                if (_option.SkillsToLevel != null && _option.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _option.SkillsToLevel)
                    {
                        if (skillToLevel.Ref.ToLower() != "hero") continue;
                        if (!skillToLevel.ByLevel.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByLevel);
                        else if (!skillToLevel.ByXP.IsStringNoneOrEmpty()) xp = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(Hero.MainHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else if (_listedEvent.SkillsToLevel != null && _listedEvent.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _listedEvent.SkillsToLevel)
                    {
                        if (skillToLevel.Ref.ToLower() != "hero") continue;
                        if (!skillToLevel.ByLevel.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByLevel);
                        else if (!skillToLevel.ByXP.IsStringNoneOrEmpty()) xp = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(Hero.MainHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill)) return;

                    if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_option.SkillTotal);
                    else if (!_option.SkillXPTotal.IsStringNoneOrEmpty()) xp = new CEVariablesLoader().GetIntFromXML(_option.SkillXPTotal);
                    else if (!_listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillTotal);
                    else if (!_listedEvent.SkillXPTotal.IsStringNoneOrEmpty()) xp = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillXPTotal);
                    else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                    if (!_option.SkillToLevel.IsStringNoneOrEmpty()) new Dynamics().SkillModifier(Hero.MainHero, _option.SkillToLevel, level, xp);
                    else if (!_listedEvent.SkillToLevel.IsStringNoneOrEmpty()) new Dynamics().SkillModifier(Hero.MainHero, _listedEvent.SkillToLevel, level, xp);
                    else CECustomHandler.LogToFile("Missing SkillToLevel");
                }

            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        internal void ConsequenceSlaveryLevel()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.SlaveryTotal)) { _dynamics.VictimSlaveryModifier(new CEVariablesLoader().GetIntFromXML(_option.SlaveryTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(_listedEvent.SlaveryTotal)) { _dynamics.VictimSlaveryModifier(new CEVariablesLoader().GetIntFromXML(_listedEvent.SlaveryTotal), Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing SlaveryTotal");
                    _dynamics.VictimSlaveryModifier(1, Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid SlaveryTotal"); }
        }


        internal void ConsequenceSlaveryFlags()
        {
            bool InformationMessage = !_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) _dynamics.VictimSlaveryModifier(1, Hero.MainHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) _dynamics.VictimSlaveryModifier(0, Hero.MainHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
        }

        internal void ConsequenceProstitutionLevel()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.ProstitutionTotal)) { _dynamics.VictimProstitutionModifier(new CEVariablesLoader().GetIntFromXML(_option.ProstitutionTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(_listedEvent.ProstitutionTotal)) { _dynamics.VictimProstitutionModifier(new CEVariablesLoader().GetIntFromXML(_listedEvent.ProstitutionTotal), Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing ProstitutionTotal");
                    _dynamics.VictimProstitutionModifier(1, Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ProstitutionTotal"); }
        }

        internal void ConsequenceProstitutionFlags()
        {
            bool InformationMessage = !_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool NoMessages = _option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) _dynamics.VictimProstitutionModifier(1, Hero.MainHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) _dynamics.VictimProstitutionModifier(0, Hero.MainHero, true, !InformationMessage && !NoMessages, InformationMessage && !NoMessages);
        }

        internal void ConsequenceSpawnTroop()
        {
            if (_option.SpawnTroops != null)
            {
                new CESpawnSystem().SpawnTheTroops(_option.SpawnTroops, PartyBase.MainParty);
            }
        }

        internal void ConsequenceSpawnHero()
        {
            if (_option.SpawnHeroes != null)
            {
                new CESpawnSystem().SpawnTheHero(_option.SpawnHeroes, PartyBase.MainParty);
            }
        }

        internal void ConsequenceRenown()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.RenownTotal)) { _dynamics.RenownModifier(new CEVariablesLoader().GetIntFromXML(_option.RenownTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(_listedEvent.RenownTotal)) { _dynamics.RenownModifier(new CEVariablesLoader().GetIntFromXML(_listedEvent.RenownTotal), Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing RenownTotal");
                    _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid RenownTotal"); }
        }

        internal void ConsequenceChangeHealth()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.HealthTotal)) { Hero.MainHero.HitPoints += new CEVariablesLoader().GetIntFromXML(_option.HealthTotal); }
                else if (!string.IsNullOrEmpty(_listedEvent.HealthTotal)) { Hero.MainHero.HitPoints += new CEVariablesLoader().GetIntFromXML(_listedEvent.HealthTotal); }
                else
                {
                    CECustomHandler.LogToFile("Invalid HealthTotal");
                    Hero.MainHero.HitPoints += MBRandom.RandomInt(-20, 20);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing HealthTotal"); }
        }


        internal void ConsequenceChangeMorale()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            PartyBase party = PlayerCaptivity.IsCaptive
                ? PlayerCaptivity.CaptorParty //captive         
                : PartyBase.MainParty; //random, captor

            try
            {
                if (!string.IsNullOrEmpty(_option.MoraleTotal)) { _dynamics.MoraleChange(new CEVariablesLoader().GetIntFromXML(_option.MoraleTotal), party); }
                else if (!string.IsNullOrEmpty(_listedEvent.MoraleTotal)) { _dynamics.MoraleChange(new CEVariablesLoader().GetIntFromXML(_listedEvent.MoraleTotal), party); }
                else
                {
                    CECustomHandler.LogToFile("Missing MoralTotal");
                    _dynamics.MoraleChange(MBRandom.RandomInt(-5, 5), party);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }
        }

        // TODO: Not being used anywhere
        internal void ConsequenceChangeMorale(PartyBase party)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.MoraleTotal)) { _dynamics.MoraleChange(new CEVariablesLoader().GetIntFromXML(_option.MoraleTotal), party); }
                else if (!string.IsNullOrEmpty(_listedEvent.MoraleTotal)) { _dynamics.MoraleChange(new CEVariablesLoader().GetIntFromXML(_listedEvent.MoraleTotal), party); }
                else
                {
                    CECustomHandler.LogToFile("Missing MoralTotal");
                    _dynamics.MoraleChange(MBRandom.RandomInt(-5, 5), party);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }
        }

        internal void ConsequenceStripPlayer()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripPlayer)) return;

            bool forced = false, questEnabled = true;
            string clothingLevel = "Default";
            string mountLevel = "Default";

            if (_option.StripSettings != null)
            {
                forced = _option.StripSettings.Forced;
                questEnabled = _option.StripSettings.QuestEnabled || true;
                clothingLevel = _option.StripSettings.Clothing.IsStringNoneOrEmpty() ? "Default" : _option.StripSettings.Clothing;
                mountLevel = _option.StripSettings.Mount.IsStringNoneOrEmpty() ? "Default" : _option.StripSettings.Clothing;
            }

            if (CESettings.InstanceToCheck != null && !CESettings.InstanceToCheck.StolenGear && !forced) return;
            Equipment randomElement = new Equipment(false);

            if (clothingLevel != "Nude")
            {

                if (CESettings.InstanceToCheck != null && MBRandom.Random.Next(100) < CESettings.InstanceToCheck.BetterOutFitChance && clothingLevel != "Basic" || clothingLevel == "Advanced")
                {
                    string bodyString = "";
                    string legString = "";
                    string headString = "";
                    string capeString = "";
                    string glovesString = "";

                    switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                    {
                        case CultureCode.Sturgia:
                            headString = "nordic_fur_cap";
                            capeString = Hero.MainHero.IsFemale
                                ? "female_hood"
                                : "";
                            bodyString = Hero.MainHero.IsFemale
                                ? "cut_dress"
                                : "heavy_nordic_tunic";
                            legString = Hero.MainHero.IsFemale
                                ? "ladys_shoe"
                                : "rough_tied_boots";
                            glovesString = "armwraps";
                            break;
                        case CultureCode.Aserai:
                            headString = Hero.MainHero.IsFemale
                                ? ""
                                : "turban";
                            bodyString = Hero.MainHero.IsFemale
                                ? "aserai_villager_female_dress"
                                : "aserai_tunic_waistcoat";

                            legString = Hero.MainHero.IsFemale
                                ? "southern_moccasins"
                                : "wrapped_shoes";
                            capeString = "wrapped_scarf";
                            glovesString = "armwraps";
                            break;
                        case CultureCode.Khuzait:
                            headString = "fur_hat";
                            capeString = "wrapped_scarf";
                            bodyString = Hero.MainHero.IsFemale
                                ? "khuzait_dress"
                                : "steppe_armor";
                            legString = Hero.MainHero.IsFemale
                                ? "ladys_shoe"
                                : "rough_tied_boots";
                            glovesString = "armwraps";
                            break;
                        case CultureCode.Empire:
                            headString = Hero.MainHero.IsFemale
                                ? "female_head_wrap"
                                : "arming_cap";
                            bodyString = Hero.MainHero.IsFemale
                                ? "vlandian_corset_dress"
                                : "padded_leather_shirt";
                            legString = Hero.MainHero.IsFemale
                                ? "ladys_shoe"
                                : "rough_tied_boots";
                            capeString = "wrapped_scarf";
                            glovesString = "armwraps";
                            break;
                        case CultureCode.Battania:
                            headString = Hero.MainHero.IsFemale
                                ? "female_head_wrap"
                                : "wrapped_headcloth";
                            capeString = Hero.MainHero.IsFemale
                                ? "wrapped_scarf"
                                : "battania_shoulder_strap";
                            glovesString = "armwraps";
                            bodyString = Hero.MainHero.IsFemale
                                ? "battania_dress_c"
                                : "burlap_waistcoat";
                            legString = "ragged_boots";
                            break;
                        case CultureCode.Vlandia:
                            headString = Hero.MainHero.IsFemale
                                ? "female_head_wrap"
                                : "arming_cap";
                            bodyString = Hero.MainHero.IsFemale
                                ? "vlandian_corset_dress"
                                : "padded_leather_shirt";
                            legString = Hero.MainHero.IsFemale
                                ? "ladys_shoe"
                                : "ragged_boots";
                            capeString = "wrapped_scarf";
                            glovesString = "armwraps";
                            break;
                        case CultureCode.Invalid:
                        case CultureCode.Nord:
                        case CultureCode.Darshi:
                        case CultureCode.Vakken:
                        case CultureCode.AnyOtherCulture:
                        default:
                            headString = Hero.MainHero.IsFemale
                                ? "female_head_wrap"
                                : "wrapped_headcloth";
                            capeString = Hero.MainHero.IsFemale
                                ? "female_scarf"
                                : "battania_shoulder_strap";
                            bodyString = Hero.MainHero.IsFemale
                                ? "plain_dress"
                                : "padded_leather_shirt";
                            legString = Hero.MainHero.IsFemale
                                ? "ladys_shoe"
                                : "ragged_boots";
                            break;
                    }

                    if (bodyString != "")
                    {
                        ItemObject itemObjectBody = MBObjectManager.Instance.GetObject<ItemObject>(bodyString);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                    }

                    if (legString != "")
                    {
                        ItemObject itemObjectLeg = MBObjectManager.Instance.GetObject<ItemObject>(legString);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Leg, new EquipmentElement(itemObjectLeg));
                    }

                    if (capeString != "")
                    {
                        ItemObject itemObjectCape = MBObjectManager.Instance.GetObject<ItemObject>(capeString);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Cape, new EquipmentElement(itemObjectCape));
                    }

                    if (headString != "")
                    {
                        ItemObject itemObjectHead = MBObjectManager.Instance.GetObject<ItemObject>(headString);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Head, new EquipmentElement(itemObjectHead));
                    }

                    if (glovesString != "")
                    {
                        ItemObject itemObjectGloves = MBObjectManager.Instance.GetObject<ItemObject>(glovesString);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Gloves, new EquipmentElement(itemObjectGloves));
                    }
                }
                else
                {
                    ItemObject itemObjectBody = Hero.MainHero.IsFemale
                        ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress")
                        : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                }

                if (CESettings.InstanceToCheck != null && MBRandom.Random.Next(100) < CESettings.InstanceToCheck.WeaponChance)
                {
                    string item;

                    if (MBRandom.Random.Next(100)
                        < (CESettings.InstanceToCheck.WeaponSkill
                            ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.TwoHanded) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) / 275 * 100))
                            : CESettings.InstanceToCheck.WeaponChance))
                    {
                        switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                        {
                            case CultureCode.Sturgia:
                                item = "sturgia_axe_3_t3";
                                break;
                            case CultureCode.Aserai:
                                item = "eastern_spear_1_t2";
                                break;
                            case CultureCode.Empire:
                                item = "northern_spear_1_t2";
                                break;
                            case CultureCode.Battania:
                                item = "aserai_sword_1_t2";
                                break;
                            case CultureCode.Invalid:
                            case CultureCode.Vlandia:
                            case CultureCode.Khuzait:
                            case CultureCode.Nord:
                            case CultureCode.Darshi:
                            case CultureCode.Vakken:
                            case CultureCode.AnyOtherCulture:
                            default:
                                item = "vlandia_sword_1_t2";
                                break;
                        }
                    }
                    else
                    {
                        switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                        {
                            case CultureCode.Sturgia:
                                item = "seax";
                                break;
                            case CultureCode.Aserai:
                                item = "celtic_dagger";
                                break;
                            case CultureCode.Empire:
                                item = "gladius_b";
                                break;
                            case CultureCode.Battania:
                                item = "hooked_cleaver";
                                break;
                            case CultureCode.Invalid:
                            case CultureCode.Vlandia:
                            case CultureCode.Khuzait:
                            case CultureCode.Nord:
                            case CultureCode.Darshi:
                            case CultureCode.Vakken:
                            case CultureCode.AnyOtherCulture:
                            default:
                                item = "seax";
                                break;
                        }
                    }

                    ItemObject itemObjectWeapon0 = MBObjectManager.Instance.GetObject<ItemObject>(item);
                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon0, new EquipmentElement(itemObjectWeapon0));
                }

                if (CESettings.InstanceToCheck != null && (MBRandom.Random.Next(100) < CESettings.InstanceToCheck.WeaponChance
                                                    && MBRandom.Random.Next(100)
                                                    < (CESettings.InstanceToCheck.RangedSkill
                                                        ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Bow) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Throwing) / 275 * 100))
                                                        : CESettings.InstanceToCheck.RangedBetterChance)))
                {
                    string rangedItem;
                    string rangedAmmo = null;

                    switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                    {
                        case CultureCode.Sturgia:
                            rangedItem = "nordic_shortbow";
                            rangedAmmo = "default_arrows";
                            break;
                        case CultureCode.Vlandia:
                            rangedItem = "crossbow_a";
                            rangedAmmo = "tournament_bolts";
                            break;
                        case CultureCode.Aserai:
                            rangedItem = "tribal_bow";
                            rangedAmmo = "default_arrows";
                            break;
                        case CultureCode.Empire:
                            rangedItem = "hunting_bow";
                            rangedAmmo = "default_arrows";
                            break;
                        case CultureCode.Battania:
                            rangedItem = "northern_javelin_2_t3";
                            break;
                        case CultureCode.Invalid:
                        case CultureCode.Khuzait:
                        case CultureCode.Nord:
                        case CultureCode.Darshi:
                        case CultureCode.Vakken:
                        case CultureCode.AnyOtherCulture:
                        default:
                            rangedItem = "hunting_bow";
                            rangedAmmo = "default_arrows";
                            break;
                    }

                    ItemObject itemObjectWeapon2 = MBObjectManager.Instance.GetObject<ItemObject>(rangedItem);
                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon2, new EquipmentElement(itemObjectWeapon2));

                    if (rangedAmmo != null)
                    {
                        ItemObject itemObjectWeapon3 = MBObjectManager.Instance.GetObject<ItemObject>(rangedAmmo);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon3, new EquipmentElement(itemObjectWeapon3));
                    }
                }
                else
                {
                    ItemObject itemObjectWeapon2 = MBObjectManager.Instance.GetObject<ItemObject>("throwing_stone");
                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon2, new EquipmentElement(itemObjectWeapon2));
                }
            }

            Equipment randomElement2 = new Equipment(true);
            randomElement2.FillFrom(randomElement, false);


            if (CESettings.InstanceToCheck != null && MBRandom.Random.Next(100)
                < (CESettings.InstanceToCheck.HorseSkill
                    ? Hero.MainHero.GetSkillValue(DefaultSkills.Riding) / 275 * 100
                    : CESettings.InstanceToCheck.HorseChance) && mountLevel != "None" || mountLevel == "Basic")
            {
                ItemObject poorHorse = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                EquipmentElement horseEquipment = new EquipmentElement(poorHorse);

                randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Horse, horseEquipment);
            }


            if (CESettings.InstanceToCheck != null && (CESettings.InstanceToCheck.StolenGearQuest && MBRandom.Random.Next(100) < CESettings.InstanceToCheck.StolenGearChance) && questEnabled)
            {
                Hero issueOwner = null;
                List<TextObject> listOfSettlements = new List<TextObject>();

                while (issueOwner == null)
                {
                    Settlement nearestSettlement = SettlementHelper.FindNearestSettlement(settlement => !listOfSettlements.Contains(settlement.Name));
                    listOfSettlements.Add(nearestSettlement.Name);

                    if (nearestSettlement.IsUnderRaid || nearestSettlement.IsRaided) continue;

                    foreach (Hero hero in nearestSettlement.Notables.Where(hero => hero.Issue == null && !hero.IsOccupiedByAnEvent()))
                    {
                        issueOwner = hero;
                        break;
                    }

                    if (issueOwner == null) continue;

                    PotentialIssueData potentialIssueData = new PotentialIssueData(CEWhereAreMyThingsIssueBehavior.OnStartIssue, typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue), IssueBase.IssueFrequency.Rare);

                    Campaign.Current.IssueManager.CreateNewIssue(potentialIssueData, issueOwner);
                    Campaign.Current.IssueManager.StartIssueQuest(issueOwner);
                }
            }

            EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, randomElement);
            EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, randomElement2);

        }


        internal void LoadBackgroundImage(string textureFlag = "")
        {
            try
            {
                string backgroundName = _listedEvent.BackgroundName;

                if (!backgroundName.IsStringNoneOrEmpty())
                {
                    CEPersistence.animationPlayEvent = false;
                    new CESubModule().LoadTexture(backgroundName);
                }
                else if (_listedEvent.BackgroundAnimation != null && _listedEvent.BackgroundAnimation.Count > 0)
                {
                    CEPersistence.animationImageList = _listedEvent.BackgroundAnimation;
                    CEPersistence.animationIndex = 0;
                    CEPersistence.animationPlayEvent = true;
                    float speed = 0.03f;

                    try
                    {
                        if (!_listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = new CEVariablesLoader().GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e)
                    {
                        // Will force log if cannot load animation speed
                        CECustomHandler.ForceLogToFile("Failed to load BackgroundAnimationSpeed for " + _listedEvent.Name + " : Exception: " + e);
                    }

                    CEPersistence.animationSpeed = speed;
                }
                else
                {
                    CEPersistence.animationPlayEvent = false;
                    new CESubModule().LoadTexture(textureFlag);
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name);
                new CESubModule().LoadTexture(textureFlag);
            }
        }

        #endregion
    }
}