#define V172

using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using CaptivityEvents.Issues;
using Helpers;
using SandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

#if V171
#else

using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;

#endif

namespace CaptivityEvents.Events
{
    public class SharedCallBackHelper
    {
        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;

        private readonly Dynamics _dynamics = new();
        private readonly ScoresCalculation _score = new();

        public SharedCallBackHelper(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
        }

        #region Consequences

        internal void ConsequenceXP()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP))
            {
                try
                {
                    string skillToLevel = "";

                    if (!string.IsNullOrEmpty(_option.SkillToLevel)) skillToLevel = _option.SkillToLevel;
                    else if (!string.IsNullOrEmpty(_listedEvent.SkillToLevel)) skillToLevel = _listedEvent.SkillToLevel;
                    else CECustomHandler.LogToFile("Missing SkillToLevel");

                    foreach (SkillObject skillObject in Skills.All.Where(skillObject => skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)) _dynamics.GainSkills(skillObject, 50, 100);
                }
                catch (Exception) { CECustomHandler.LogToFile("GiveXP Failed"); }
            }
        }

        internal void ConsequenceLeaveSpouse()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) _dynamics.ChangeSpouse(Hero.MainHero, null);
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
            try
            {
                if (_option.TraitsToLevel != null && _option.TraitsToLevel.Count(TraitToLevel => TraitToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (TraitToLevel traitToLevel in _option.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (traitToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(Hero.MainHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else if (_listedEvent.TraitsToLevel != null && _listedEvent.TraitsToLevel.Count(TraitsToLevel => TraitsToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (TraitToLevel traitToLevel in _listedEvent.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (traitToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(Hero.MainHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait)) return;

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
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }

        internal void ConsequenceChangeSkill()
        {
            try
            {
                if (_option.SkillsToLevel != null && _option.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _option.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (skillToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(Hero.MainHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else if (_listedEvent.SkillsToLevel != null && _listedEvent.SkillsToLevel.Count(SkillToLevel => SkillToLevel.Ref.ToLower() == "hero") != 0)
                {
                    foreach (SkillToLevel skillToLevel in _listedEvent.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;

                        if (skillToLevel.Ref.ToLower() != "hero") continue;
                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel)) level = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = new CEVariablesLoader().GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(Hero.MainHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else
                {
                    if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill)) return;

                    int level = 0;
                    int xp = 0;

                    if (!string.IsNullOrWhiteSpace(_option.SkillTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.SkillTotal);
                    else if (!string.IsNullOrWhiteSpace(_option.SkillXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_option.SkillXPTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillTotal);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillXPTotal)) xp = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillXPTotal);
                    else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                    if (!string.IsNullOrWhiteSpace(_option.SkillToLevel)) new Dynamics().SkillModifier(Hero.MainHero, _option.SkillToLevel, level, xp);
                    else if (!string.IsNullOrWhiteSpace(_listedEvent.SkillToLevel)) new Dynamics().SkillModifier(Hero.MainHero, _listedEvent.SkillToLevel, level, xp);
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

        internal void ConsequenceStripPlayer()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripPlayer)) return;

            try
            {
                bool forced = false, questEnabled = true;
                string clothingLevel = "default";
                string mountLevel = "default";
                string meleeLevel = "default";
                string rangedLevel = "default";

                string customBody = "";
                string customCape = "";
                string customGloves = "";
                string customLegs = "";
                string customHead = "";

                if (_option.StripSettings != null)
                {
                    forced = _option.StripSettings.Forced;
                    questEnabled = _option.StripSettings.QuestEnabled || true;
                    clothingLevel = string.IsNullOrWhiteSpace(_option.StripSettings.Clothing) ? "default" : _option.StripSettings.Clothing.ToLower();
                    mountLevel = string.IsNullOrWhiteSpace(_option.StripSettings.Mount) ? "default" : _option.StripSettings.Mount.ToLower();
                    meleeLevel = string.IsNullOrWhiteSpace(_option.StripSettings.Melee) ? "default" : _option.StripSettings.Melee.ToLower();
                    rangedLevel = string.IsNullOrWhiteSpace(_option.StripSettings.Ranged) ? "default" : _option.StripSettings.Ranged.ToLower();

                    customBody = string.IsNullOrWhiteSpace(_option.StripSettings.CustomBody) ? "default" : _option.StripSettings.CustomBody;
                    customCape = string.IsNullOrWhiteSpace(_option.StripSettings.CustomCape) ? "default" : _option.StripSettings.CustomCape;
                    customGloves = string.IsNullOrWhiteSpace(_option.StripSettings.CustomGloves) ? "default" : _option.StripSettings.CustomGloves;
                    customLegs = string.IsNullOrWhiteSpace(_option.StripSettings.CustomLegs) ? "default" : _option.StripSettings.CustomLegs;
                    customHead = string.IsNullOrWhiteSpace(_option.StripSettings.CustomHead) ? "default" : _option.StripSettings.CustomHead;
                }

                if (CESettingsIntegrations.Instance == null && clothingLevel == "slave" || !CESettingsIntegrations.Instance.ActivateKLBShackles && clothingLevel == "slave") return;

                if (CESettings.Instance != null && !CESettings.Instance.StolenGear && !forced) return;
                Equipment randomElement = new(false);

                if (clothingLevel != "nude")
                {
                    if (MBRandom.Random.Next(100) < CESettings.Instance.BetterOutFitChance && clothingLevel == "default" || clothingLevel == "advanced")
                    {
                        string bodyString = "";
                        string legString = "";
                        string headString = "";
                        string capeString = "";
                        string glovesString = "";

                        switch (PlayerCaptivity.CaptorParty?.Culture?.GetCultureCode())
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
                    else if (clothingLevel == "slave")
                    {
                        ItemObject itemObjectLeg = MBObjectManager.Instance.GetObject<ItemObject>("klbcloth2a");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Leg, new EquipmentElement(itemObjectLeg));

                        ItemObject itemObjectCape = MBObjectManager.Instance.GetObject<ItemObject>("klbcloth3a");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Cape, new EquipmentElement(itemObjectCape));

                        ItemObject itemObjectGloves = MBObjectManager.Instance.GetObject<ItemObject>("klbcloth1a");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Gloves, new EquipmentElement(itemObjectGloves));
                    }
                    else if (clothingLevel == "custom")
                    {
                        ItemObject itemObjectBody = MBObjectManager.Instance.GetObject<ItemObject>(customBody);
                        ItemObject itemObjectCape = MBObjectManager.Instance.GetObject<ItemObject>(customCape);
                        ItemObject itemObjectGloves = MBObjectManager.Instance.GetObject<ItemObject>(customGloves);
                        ItemObject itemObjectLeg = MBObjectManager.Instance.GetObject<ItemObject>(customLegs);
                        ItemObject itemObjectHead = MBObjectManager.Instance.GetObject<ItemObject>(customHead);

                        if (itemObjectBody != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                        if (itemObjectCape != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Cape, new EquipmentElement(itemObjectCape));
                        if (itemObjectGloves != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Gloves, new EquipmentElement(itemObjectGloves));
                        if (itemObjectLeg != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Leg, new EquipmentElement(itemObjectLeg));
                        if (itemObjectHead != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Head, new EquipmentElement(itemObjectHead));
                    }
                    else
                    {
                        ItemObject itemObjectBody = Hero.MainHero.IsFemale
                            ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress")
                            : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                    }
                }

                if (meleeLevel != "none" || meleeLevel == "default" && MBRandom.Random.Next(100) < CESettings.Instance.WeaponChance)
                {
                    string item;

                    if (MBRandom.Random.Next(100)
                        < (CESettings.Instance.WeaponSkill
                            ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.TwoHanded) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) / 275 * 100))
                            : CESettings.Instance.WeaponChance) && meleeLevel == "Default" || meleeLevel == "Advanced")
                    {
                        item = PlayerCaptivity.CaptorParty.Culture.GetCultureCode() switch
                        {
                            CultureCode.Sturgia => "sturgia_axe_3_t3",
                            CultureCode.Aserai => "eastern_spear_1_t2",
                            CultureCode.Empire => "northern_spear_1_t2",
                            CultureCode.Battania => "aserai_sword_1_t2",
                            _ => "vlandia_sword_1_t2",
                        };
                    }
                    else
                    {
                        item = (PlayerCaptivity.CaptorParty?.Culture?.GetCultureCode()) switch
                        {
                            CultureCode.Sturgia => "seax",
                            CultureCode.Aserai => "celtic_dagger",
                            CultureCode.Empire => "gladius_b",
                            CultureCode.Battania => "hooked_cleaver",
                            _ => "seax",
                        };
                    }

                    ItemObject itemObjectWeapon0 = MBObjectManager.Instance.GetObject<ItemObject>(item);
                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon0, new EquipmentElement(itemObjectWeapon0));
                }

                if (rangedLevel != "none")
                {
                    if (CESettings.Instance != null && MBRandom.Random.Next(100) < CESettings.Instance.WeaponChance
                                                        && MBRandom.Random.Next(100)
                                                        < (CESettings.Instance.RangedSkill
                                                            ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Bow) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Throwing) / 275 * 100))
                                                            : CESettings.Instance.RangedBetterChance) && rangedLevel == "default" || rangedLevel == "advanced")
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

                Equipment randomElement2 = new(true);
                randomElement2.FillFrom(randomElement, false);

                if (CESettings.Instance != null && MBRandom.Random.Next(100)
                    < (CESettings.Instance.HorseSkill
                        ? Hero.MainHero.GetSkillValue(DefaultSkills.Riding) / 275 * 100
                        : CESettings.Instance.HorseChance) && mountLevel == "default" || mountLevel == "basic")
                {
                    ItemObject poorHorse = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                    EquipmentElement horseEquipment = new(poorHorse);

                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Horse, horseEquipment);
                }

                if (CESettings.Instance != null && (CESettings.Instance.StolenGearQuest && MBRandom.Random.Next(100) < CESettings.Instance.StolenGearChance) && questEnabled)
                {
                    Hero issueOwner = null;
                    List<TextObject> listOfSettlements = new();

                    while (issueOwner == null)
                    {
                        Settlement nearestSettlement = SettlementHelper.FindNearestSettlement(settlement => !listOfSettlements.Contains(settlement.Name));
                        listOfSettlements.Add(nearestSettlement.Name);

                        if (nearestSettlement.IsUnderRaid || nearestSettlement.IsRaided) continue;

                        issueOwner = nearestSettlement.Notables.FirstOrDefault((Hero y) => y.CanHaveQuestsOrIssues() && y.GetTraitLevel(DefaultTraits.Mercy) <= 0);

                        if (issueOwner == null) continue;

                        PotentialIssueData potentialIssueData = new(CEWhereAreMyThingsIssueBehavior.OnStartIssue, typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue), IssueBase.IssueFrequency.Rare);

                        Campaign.Current.IssueManager.CreateNewIssue(potentialIssueData, issueOwner);
                        Campaign.Current.IssueManager.StartIssueQuest(issueOwner);
                    }
                }

                EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, randomElement);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, randomElement2);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceStripPlayer : " + e.ToString());
            }
        }

        internal void ConsequenceStartBattle(Action callback, int type)
        {
            try
            {
                if (_option.BattleSettings != null)
                {
                    CEPersistence.animationPlayEvent = false;
                    CEEvent VictoryEvent, DefeatEvent;

                    try
                    {
                        VictoryEvent = _eventList.Find(item => item.Name == _option.BattleSettings.Victory);
                        VictoryEvent.Captive = _listedEvent.Captive;
                        VictoryEvent.SavedCompanions = _listedEvent.SavedCompanions;

                        CEPersistence.victoryEvent = VictoryEvent.Name;
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle VictoryEvent Missing");
                        callback();
                        return;
                    }

                    try
                    {
                        DefeatEvent = _eventList.Find(item => item.Name == _option.BattleSettings.Defeat);
                        DefeatEvent.Captive = _listedEvent.Captive;
                        DefeatEvent.SavedCompanions = _listedEvent.SavedCompanions;

                        CEPersistence.defeatEvent = DefeatEvent.Name;
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle DefeatEvent Missing");
                        callback();
                        return;
                    }

                    TroopRoster enemyTroops = TroopRoster.CreateDummyTroopRoster();
                    TroopRoster friendlyTroops = TroopRoster.CreateDummyTroopRoster();
                    TroopRoster temporaryTroops = TroopRoster.CreateDummyTroopRoster();

                    try
                    {
                        if (_option.BattleSettings.SpawnTroops != null)
                        {
                            foreach (SpawnTroop troop in _option.BattleSettings.SpawnTroops)
                            {
                                try
                                {
                                    int num = new CEVariablesLoader().GetIntFromXML(troop.Number);
                                    int numWounded = new CEVariablesLoader().GetIntFromXML(troop.WoundedNumber);
                                    CharacterObject characterObject = null;

                                    if (troop.Id != null && troop.Id.ToLower() == "random")
                                    {
                                        characterObject = CharacterObject.All.GetRandomElementWithPredicate((CharacterObject t) => !t.IsHero && t.Occupation == Occupation.Soldier);
                                    }
                                    else
                                    {
                                        characterObject = MBObjectManager.Instance.GetObject<CharacterObject>(troop.Id);
                                    }

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
                                            if (troop.Ref != null && troop.Ref.ToLower() == "friendly")
                                            {
                                                friendlyTroops.AddToCounts(characterObject, num, false, numWounded, 0, true, -1);
                                            }
                                            else if (troop.Ref != null && troop.Ref.ToLower() == "temporary")
                                            {
                                                temporaryTroops.AddToCounts(characterObject, num, false, numWounded, 0, true, -1);
                                            }
                                            else
                                            {
                                                enemyTroops.AddToCounts(characterObject, num, false, numWounded, 0, true, -1);
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
                        else
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                CharacterObject characterObject = CharacterObject.All.GetRandomElementWithPredicate((CharacterObject t) => !t.IsHero && t.Occupation == Occupation.Soldier);
                                enemyTroops.AddToCounts(characterObject, 1, true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle SpawnTroops Failed");
                    }

                    if (!enemyTroops.GetTroopRoster().IsEmpty() && _option.BattleSettings.Ref != null)
                    {
                        callback();
                        Hero.MainHero.HitPoints += 40;
                        CEPersistence.playerTroops.Clear();
                        try
                        {
                            // Player Party Setup
                            foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.MemberRoster.GetTroopRoster())
                            {
                                if (!troopRosterElement.Character.IsPlayerCharacter) CEPersistence.playerTroops.Add(troopRosterElement);
                            }

                            PartyBase.MainParty.MemberRoster.RemoveIf((TroopRosterElement t) => !t.Character.IsPlayerCharacter);

                            if (!PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter))
                            {
                                CEPersistence.removePlayer = true;
                                PartyBase.MainParty.MemberRoster.AddToCounts(CharacterObject.PlayerCharacter, 1);
                            }
                            else
                            {
                                CEPersistence.removePlayer = false;
                            }

                            if (!CEPersistence.playerTroops.IsEmpty())
                            {
                                List<CharacterObject> list = new();
                                int num = new CEVariablesLoader().GetIntFromXML(_option.BattleSettings.PlayerTroops);
                                foreach (TroopRosterElement troopRosterElement in from t in CEPersistence.playerTroops
                                                                                  orderby t.Character.Level descending
                                                                                  select t)
                                {
                                    if (num <= 0) break;
                                    int num2 = 0;
                                    while (num2 < troopRosterElement.Number - troopRosterElement.WoundedNumber && num > 0)
                                    {
                                        list.Add(troopRosterElement.Character);
                                        num--;
                                        num2++;
                                    }
                                }

                                foreach (CharacterObject character in list)
                                {
                                    PartyBase.MainParty.MemberRoster.AddToCounts(character, 1, false, 0, 0, true, -1);
                                }
                            }

                            foreach (TroopRosterElement troopRosterElement in temporaryTroops.GetTroopRoster())
                            {
                                PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, troopRosterElement.WoundedNumber, 0, true, -1);
                            }

                            foreach (TroopRosterElement troopRosterElement in friendlyTroops.GetTroopRoster())
                            {
                                PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, troopRosterElement.WoundedNumber, 0, true, -1);
                                CEPersistence.playerTroops.Add(troopRosterElement);
                            }
                            // Player Party Setup Ends Here

                            switch (_option.BattleSettings.Ref.ToLower())
                            {
                                case "city":
                                    {
                                        if (Settlement.CurrentSettlement == null)
                                        {
                                            CECustomHandler.ForceLogToFile("ConsequenceStartBattle : city required. ");
                                        }
                                        // StartCommonAreaBattle RivalGangMovingInIssueBehavior
                                        MobileParty customParty = MobileParty.CreateParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), null, null);

                                        Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                                        clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                                        customParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, Settlement.CurrentSettlement.GatePosition, 1f, 0.5f);
                                        customParty.MemberRoster.Clear();
                                        customParty.MemberRoster.Add(enemyTroops.ToFlattenedRoster());

                                        TextObject textObject = new(_option.BattleSettings.EnemyName ?? "Bandits", null);
                                        customParty.SetCustomName(textObject);

                                        EnterSettlementAction.ApplyForParty(customParty, Settlement.CurrentSettlement);

                                        PlayerEncounter.RestartPlayerEncounter(customParty.Party, PartyBase.MainParty, false);

                                        CEPersistence.battleState = CEPersistence.BattleState.StartBattle;
                                        CEPersistence.destroyParty = true;
                                        CEPersistence.surrenderParty = false;

                                        PlayerEncounter.Current.ForceAlleyFight = true;
                                        PlayerEncounter.StartBattle();
                                        PlayerEncounter.StartAlleyFightMission();
                                        break;
                                    }
                                case "regularspawn":
                                    {
                                        //SpawnAPartyInFaction
                                        Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                                        clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                                        Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

                                        MobileParty customParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false);

                                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                                        customParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, -1);

                                        customParty.MemberRoster.Clear();
                                        customParty.MemberRoster.Add(enemyTroops.ToFlattenedRoster());

                                        TextObject textObject = new(_option.BattleSettings.EnemyName ?? "Bandits", null);
                                        customParty.SetCustomName(textObject);

                                        // InitBanditParty
                                        customParty.Party.Visuals.SetMapIconAsDirty();
                                        customParty.ActualClan = clan;

                                        // CreatePartyTrade
                                        int initialGold = (int)(10f * customParty.Party.MemberRoster.TotalManCount * (0.5f + 1f * MBRandom.RandomFloat));
                                        customParty.InitializePartyTrade(initialGold);

                                        foreach (ItemObject itemObject in Items.All)
                                        {
                                            if (itemObject.IsFood)
                                            {
                                                int num2 = MBRandom.RoundRandomized(customParty.MemberRoster.TotalManCount * (1f / itemObject.Value) * 8f * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
                                                if (num2 > 0)
                                                {
                                                    customParty.ItemRoster.AddToCounts(itemObject, num2);
                                                }
                                            }
                                        }

                                        customParty.Aggressiveness = 1f - 0.2f * MBRandom.RandomFloat;
                                        customParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position2D);

                                        PlayerEncounter.RestartPlayerEncounter(customParty.Party, PartyBase.MainParty, true);
                                        CEPersistence.battleState = CEPersistence.BattleState.StartBattle;
                                        CEPersistence.destroyParty = false;
                                        CEPersistence.surrenderParty = true;
                                        PlayerEncounter.StartBattle();
                                        PlayerEncounter.Update();
                                        //EncounterAttackConsequence

                                        MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
                                        string battleSceneForMapPatch = PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition);
                                        MissionInitializerRecord rec = new(battleSceneForMapPatch)
                                        {
                                            TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace),
                                            DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier(),
                                            DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier(),
                                            NeedsRandomTerrain = false,
                                            PlayingInCampaignMode = true,
                                            RandomTerrainSeed = MBRandom.RandomInt(10000),
                                            AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(CampaignTime.Now, MobileParty.MainParty.GetLogicalPosition())
                                        };
                                        float timeOfDay = Campaign.CurrentTime % 24f;
                                        if (Campaign.Current != null)
                                        {
                                            rec.TimeOfDay = timeOfDay;
                                        }
                                        CampaignMission.OpenBattleMission(rec);
                                        break;
                                    }
                                case "regular":
                                    {
                                        //SpawnAPartyInFaction
                                        Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                                        clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                                        Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

                                        MobileParty customParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false);
                                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                                        customParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, -1);

                                        customParty.MemberRoster.Clear();
                                        customParty.MemberRoster.Add(enemyTroops.ToFlattenedRoster());

                                        TextObject textObject = new(_option.BattleSettings.EnemyName ?? "Bandits", null);
                                        customParty.SetCustomName(textObject);

                                        // InitBanditParty
                                        customParty.Party.Visuals.SetMapIconAsDirty();
                                        customParty.ActualClan = clan;

                                        // CreatePartyTrade
                                        int initialGold = (int)(10f * customParty.Party.MemberRoster.TotalManCount * (0.5f + 1f * MBRandom.RandomFloat));
                                        customParty.InitializePartyTrade(initialGold);

                                        foreach (ItemObject itemObject in Items.All)
                                        {
                                            if (itemObject.IsFood)
                                            {
                                                int num2 = MBRandom.RoundRandomized(customParty.MemberRoster.TotalManCount * (1f / itemObject.Value) * 8f * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
                                                if (num2 > 0)
                                                {
                                                    customParty.ItemRoster.AddToCounts(itemObject, num2);
                                                }
                                            }
                                        }

                                        customParty.Aggressiveness = 1f - 0.2f * MBRandom.RandomFloat;
                                        customParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position2D);

                                        PlayerEncounter.RestartPlayerEncounter(customParty.Party, PartyBase.MainParty, true);
                                        CEPersistence.battleState = CEPersistence.BattleState.StartBattle;
                                        CEPersistence.destroyParty = true;
                                        CEPersistence.surrenderParty = false;
                                        PlayerEncounter.StartBattle();
                                        PlayerEncounter.Update();
                                        //EncounterAttackConsequence
                                        MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
                                        string battleSceneForMapPatch = PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition);
                                        MissionInitializerRecord rec = new(battleSceneForMapPatch)
                                        {
                                            TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace),
                                            DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier(),
                                            DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier(),
                                            NeedsRandomTerrain = false,
                                            PlayingInCampaignMode = true,
                                            RandomTerrainSeed = MBRandom.RandomInt(10000),
                                            AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(CampaignTime.Now, MobileParty.MainParty.GetLogicalPosition())
                                        };
                                        float timeOfDay = Campaign.CurrentTime % 24f;
                                        if (Campaign.Current != null)
                                        {
                                            rec.TimeOfDay = timeOfDay;
                                        }
                                        CampaignMission.OpenBattleMission(rec);
                                        break;
                                    }
                                default:
                                    CECustomHandler.ForceLogToFile("ConsequenceStartBattle : no battle type set");
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("ConsequenceStartBattle : " + e);
                        }
                    }
                    else
                    {
                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle generatedTrooper is Empty");
                        callback();
                    }
                }
                else
                {
                    CECustomHandler.ForceLogToFile("ConsequenceStartBattle BattleSettings Missing");
                    callback();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceStartBattle Failed: " + e);
                callback();
            }
        }

        internal void ConsequencePlaySound(bool isListedEvent = false)
        {
            try
            {
                if (CEPersistence.soundEvent != null)
                {
                    CEPersistence.soundEvent.Stop();
                    CEPersistence.soundLoop = false;
                }

                string soundToPlay = isListedEvent ? _listedEvent.SoundName : _option.SoundName;

                if (soundToPlay == null) return;
                int soundIndex = SoundEvent.GetEventIdFromString(soundToPlay);
                if (soundIndex != -1)
                {
                    Campaign campaign = Campaign.Current;
                    Scene _mapScene = null;
                    if ((campaign?.MapSceneWrapper) != null)
                    {
                        _mapScene = ((MapScene)Campaign.Current.MapSceneWrapper).Scene;
                    }

                    CEPersistence.soundEvent = SoundEvent.CreateEvent(soundIndex, _mapScene);
                    CEPersistence.soundEvent.Play();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("ConsequencePlaySound " + isListedEvent + " : " + e);
            }
        }

        internal bool TeleportChecker(bool firstStatement, Settlement settlement, string faction)
        {
            return faction switch
            {
                "enemy" => firstStatement && settlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction),
                "otherenemy" => firstStatement && settlement.MapFaction != Hero.MainHero.MapFaction,
                "netural" => firstStatement && !settlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) && settlement.MapFaction != Hero.MainHero.MapFaction,
                "otherfriendly" => firstStatement && !settlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction),
                "friendly" => firstStatement && settlement.MapFaction == Hero.MainHero.MapFaction,
                _ => firstStatement,
            };
        }

        internal void ConsequenceTeleportPlayer()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.TeleportPlayer)) return;
            try
            {
                TeleportSettings teleportSettings = new();
                if (_option.TeleportSettings != null)
                {
                    teleportSettings = _option.TeleportSettings;
                }

                Settlement nearest;
                if (teleportSettings.LocationName != null)
                {
                    nearest = SettlementHelper.FindNearestSettlement(settlement => { return settlement.IsTown && settlement.MapFaction != Hero.MainHero.MapFaction; });
                    if (nearest != null)
                    {
                        CECustomHandler.ForceLogToFile("LocationName Failed to Find: " + teleportSettings.LocationName);
                    }
                }
                else
                {
                    string location = teleportSettings?.Location ?? "";
                    string distance = teleportSettings?.Distance ?? "";
                    string faction = teleportSettings?.Faction ?? "";

                    location = location.ToLower();
                    distance = distance.ToLower();
                    faction = faction.ToLower();

                    switch (location)
                    {
                        case "village":
                            nearest = distance switch
                            {
                                "random" => SettlementHelper.FindRandomSettlement(settlement =>
                                                                     {
                                                                         return TeleportChecker(settlement.IsVillage, settlement, faction);
                                                                     }),
                                _ => SettlementHelper.FindNearestSettlement(settlement =>
                               {
                                   return TeleportChecker(settlement.IsVillage, settlement, faction);
                               }),
                            };
                            break;

                        case "castle":
                            nearest = distance switch
                            {
                                "random" => SettlementHelper.FindRandomSettlement(settlement =>
                                                                     {
                                                                         return TeleportChecker(settlement.IsCastle, settlement, faction);
                                                                     }),
                                _ => SettlementHelper.FindNearestSettlement(settlement =>
                               {
                                   return TeleportChecker(settlement.IsCastle, settlement, faction);
                               }),
                            };
                            break;

                        case "hideout":
                            nearest = distance switch
                            {
                                "random" => SettlementHelper.FindRandomSettlement(settlement =>
                                                                     {
                                                                         return TeleportChecker(settlement.IsHideout, settlement, faction);
                                                                     }),
                                _ => SettlementHelper.FindNearestSettlement(settlement =>
                               {
                                   return TeleportChecker(settlement.IsHideout, settlement, faction);
                               }),
                            };
                            nearest.Hideout.IsSpotted = true;
                            break;

                        default:
                            nearest = distance switch
                            {
                                "random" => SettlementHelper.FindRandomSettlement(settlement =>
                                                                     {
                                                                         return TeleportChecker(settlement.IsTown, settlement, faction);
                                                                     }),
                                _ => SettlementHelper.FindNearestSettlement(settlement =>
                               {
                                   return TeleportChecker(settlement.IsTown, settlement, faction);
                               }),
                            };
                            break;
                    }

                    if (Hero.MainHero.IsPrisoner)
                    {
                        try
                        {
                            Hero prisonerCharacter = Hero.MainHero;
                            PartyBase party = nearest.Party;

                            if (prisonerCharacter.PartyBelongedToAsPrisoner != null)
                            {
                                prisonerCharacter.PartyBelongedToAsPrisoner.PrisonRoster.RemoveTroop(prisonerCharacter.CharacterObject, 1, default, 0);
                            }
                            prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
                            prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
                            party.AddPrisoner(prisonerCharacter.CharacterObject, 1);

                            PlayerCaptivity.StartCaptivity(party);
                            CEHelper.delayedEvents.Clear();
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.LogToFile("Failed to ConsequenceTeleportPlayer: " + e.Message + " stacktrace: " + e.StackTrace);
                        }
                    }
                    else
                    {
                        MobileParty.MainParty.Position2D = nearest.GatePosition;
                        EncounterManager.StartSettlementEncounter(MobileParty.MainParty, nearest);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceTeleportPlayer Failed: " + e);
            }
        }

        internal void ConsequenceMission()
        {
            try
            {
                if (_option.SceneSettings != null)
                {
                    ConversationCharacterData data1 = new(Hero.MainHero.CharacterObject);

                    CharacterObject character2 = null;

                    switch (_option.SceneSettings.TalkTo?.ToLower())
                    {
                        case "none":
                            break;

                        default:
                            character2 = Hero.MainHero.IsPrisoner ? Hero.MainHero.PartyBelongedToAsPrisoner.LeaderHero?.CharacterObject ?? Hero.MainHero.PartyBelongedToAsPrisoner.MemberRoster.GetCharacterAtIndex(0) : _listedEvent.Captive;
                            break;
                    }

                    character2.StringId = "CECustomStringId_" + _option.SceneSettings.SceneName;
                    ConversationCharacterData data2 = new(character2);

                    CampaignMission.OpenConversationMission(data1, data2);
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceMission Failed: " + e);
            }
        }

        #endregion Consequences

        internal void LoadBackgroundImage(string textureFlag = "", CharacterObject specificCaptive = null)
        {
            try
            {
                if (!CESettings.Instance.CustomBackgrounds)
                {
                    CEPersistence.animationPlayEvent = false;
                    new CESubModule().LoadTexture(textureFlag);
                    return;
                }

                if (_listedEvent.Backgrounds != null)
                {
                    List<string> backgroundNames = new();
                    foreach (Background background in _listedEvent.Backgrounds)
                    {
                        try
                        {
                            int weightedChance = 0;

                            if (background.UseConditions != null && background.UseConditions.ToLower() != "false")
                            {
                                CEEvent triggeredEvent = _eventList.Find(item => item.Name == background.UseConditions);

                                if (triggeredEvent == null)
                                {
                                    CECustomHandler.ForceLogToFile("Couldn't find " + background.UseConditions + " in events.");
                                    continue;
                                }

                                string conditionMatched = null;
                                if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                                {
                                    conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(specificCaptive, PartyBase.MainParty);
                                }
                                else if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                                {
                                    conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                                }
                                else if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                                {
                                    conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
                                }

                                if (conditionMatched != null)
                                {
                                    CECustomHandler.LogToFile(conditionMatched);
                                    continue;
                                }

                                if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.IgnoreAllOther))
                                {
                                    CECustomHandler.LogToFile("IgnoreAllOther detected - autofire " + triggeredEvent.Name);
                                    backgroundNames.Add(background.Name);
                                    break;
                                }

                                try
                                {
                                    weightedChance = new CEVariablesLoader().GetIntFromXML(!string.IsNullOrWhiteSpace(background.Weight)
                                                                                  ? background.Weight
                                                                                  : triggeredEvent.WeightedChanceOfOccuring);
                                }
                                catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }
                            }
                            else
                            {
                                try
                                {
                                    weightedChance = new CEVariablesLoader().GetIntFromXML(background.Weight);
                                }
                                catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }
                            }

                            if (weightedChance == 0) weightedChance = 1;

                            for (int a = weightedChance; a > 0; a--) backgroundNames.Add(background.Name);
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Failed to generate a background for " + _listedEvent.Name + " " + e);
                            continue;
                        }
                    }

                    CEPersistence.animationPlayEvent = false;
                    if (backgroundNames.Count > 0)
                    {
                        int number = MBRandom.Random.Next(0, backgroundNames.Count);

                        try
                        {
                            new CESubModule().LoadTexture(backgroundNames[number]);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name);
                            new CESubModule().LoadTexture(textureFlag);
                        }
                    }
                    else
                    {
                        CECustomHandler.ForceLogToFile("Failed to find valid events for " + _listedEvent.Name);
                        new CESubModule().LoadTexture(textureFlag);
                    }
                }
                else
                {
                    string backgroundName = _listedEvent.BackgroundName;

                    if (!string.IsNullOrWhiteSpace(backgroundName))
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
                            if (!string.IsNullOrWhiteSpace(_listedEvent.BackgroundAnimationSpeed)) speed = new CEVariablesLoader().GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
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
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name);
                new CESubModule().LoadTexture(textureFlag);
            }
        }
    }
}