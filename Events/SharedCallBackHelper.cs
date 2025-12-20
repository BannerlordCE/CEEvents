using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using CaptivityEvents.Issues;
using CaptivityEvents.Notifications;
using Helpers;
using SandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static CaptivityEvents.CampaignBehaviors.CECampaignBehavior;

namespace CaptivityEvents.Events
{
    public class SharedCallBackHelper(CEEvent listedEvent, Option option, List<CEEvent> eventList)
    {
        private readonly Dynamics _dynamics = new();
        private readonly ScoresCalculation _score = new();
        private readonly CEVariablesLoader _variableLoader = new();

#region Consequences

        internal void ConsequenceGiveItem()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveItem)) return;

            try
            {
                string[] items = _variableLoader.GetStringFromXML(option.ItemToGive);

                foreach (string item in items)
                {
                    try
                    {
                        ItemObject itemObjectBody = null;

                        if (!string.IsNullOrWhiteSpace(item))
                            itemObjectBody = MBObjectManager.Instance.GetObject<ItemObject>(item);
                        else
                            CECustomHandler.LogToFile("Missing ConsequenceGiveItem");

                        if (itemObjectBody != null) PartyBase.MainParty.ItemRoster.AddToCounts(itemObjectBody, 1);

                        TextObject textObject = GameTexts.FindText("str_CE_item_received");
                        textObject.SetTextVariable("ITEM", itemObjectBody?.Name.ToString());
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid ConsequenceGiveItem - " + item); }
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ConsequenceGiveItem"); }
        }

        internal void ConsequencePlayScene()
        {
            if (listedEvent.SceneToPlay == null && option.SceneToPlay == null) return;

            bool isCaptive = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive);
            bool isRandom = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random);
            //bool isCaptor = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor);

            PartyBase party = isCaptive
                ? PlayerCaptivity.CaptorParty //captive
                : PartyBase.MainParty; //random, captor

            CharacterObject character1 = (isCaptive || isRandom) ? Hero.MainHero.IsFemale ? Hero.MainHero.CharacterObject : null : listedEvent.Captive?.IsFemale ?? false ? listedEvent.Captive : null;
            CharacterObject character2 = (isCaptive || isRandom) ? !Hero.MainHero.IsFemale ? Hero.MainHero.CharacterObject : null : !listedEvent.Captive?.IsFemale ?? false ? listedEvent.Captive : null;

            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationByPlayer))
            {
                character1 ??= Hero.MainHero.CharacterObject;
                character2 = character1 != Hero.MainHero.CharacterObject ? Hero.MainHero.CharacterObject : character2;
            }

            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
            {
                character1 ??= party.LeaderHero.CharacterObject;
                character2 = character1 != party.LeaderHero.CharacterObject ? party.LeaderHero.CharacterObject : character2;
            }

            try
            {
                string sceneToPlay = CEHelper.CustomSceneToPlay(option.SceneToPlay ?? listedEvent.SceneToPlay, party);
                CESceneNotification data = new(character2, character1, sceneToPlay);

                MBInformationManager.ShowSceneNotification(data);
            }
            catch (System.Reflection.TargetInvocationException)
            {
                CECustomHandler.LogToFile("Invalid ConsequencePlayScene");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid ConsequencePlayScene");
            }
        }

        internal void ConsequenceLeaveSpouse()
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) _dynamics.ChangeSpouse(Hero.MainHero, null);
        }

        internal void ConsequenceGold()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;

            int content = _score.AttractivenessScore(Hero.MainHero);
            int currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
        }

        internal void ConsequenceChangeGold()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(option.GoldTotal))
                    level = _variableLoader.GetIntFromXML(option.GoldTotal);
                else if (!string.IsNullOrEmpty(listedEvent.GoldTotal))
                    level = _variableLoader.GetIntFromXML(listedEvent.GoldTotal);
                else
                    CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        internal void ConsequenceGiveBirth()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveBirth)) return;

            try
            {
                CheckOffspringToDeliver(listedEvent.Pregnancy);
            }
            catch (Exception e) { CECustomHandler.LogToFile("Invalid ConsequenceGiveBirth : " + e); }
        }

        internal void ConsequenceAbort()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Abort)) return;

            try
            {
                listedEvent.Pregnancy.Mother.IsPregnant = false;
                listedEvent.Pregnancy.AlreadyOccurred = true;

                ChangeWeight(listedEvent.Pregnancy.Mother, 0, MBRandom.RandomFloatRanged(0.4025f, 0.6025f));
            }
            catch (Exception e) { CECustomHandler.LogToFile("Invalid ConsequenceAbort : " + e); }
        }

        internal void ConsequenceChangeTrait()
        {
            try
            {
                bool isRandom = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random);
                bool isCaptor = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor);

                Hero captiveHero = isCaptor ? listedEvent.Captive?.HeroObject : Hero.MainHero;
                Hero captorHero = isCaptor ? Hero.MainHero : PlayerCaptivity.CaptorParty?.LeaderHero;

                if (option.TraitsToLevel != null)
                {
                    foreach (TraitToLevel traitToLevel in option.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;
                        string refName = traitToLevel.Ref?.ToLower() ?? "hero";
                        Hero targetHero = null;

                        switch (refName)
                        {
                            case "hero":
                                targetHero = captiveHero;

                                break;
                            case "captor":
                                targetHero = captorHero;

                                break;
                            case "captive":
                                if (!isRandom) targetHero = captiveHero;

                                break;
                        }

                        if (targetHero == null) continue;

                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel))
                            level = _variableLoader.GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(targetHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
                else if (listedEvent.TraitsToLevel != null)
                {
                    foreach (TraitToLevel traitToLevel in listedEvent.TraitsToLevel)
                    {
                        int level = 0;
                        int xp = 0;
                        string refName = traitToLevel.Ref?.ToLower() ?? "hero";
                        Hero targetHero = null;

                        switch (refName)
                        {
                            case "hero":
                                targetHero = captiveHero;

                                break;
                            case "captor":
                                targetHero = captorHero;

                                break;
                            case "captive":
                                if (!isRandom) targetHero = captiveHero;

                                break;
                        }

                        if (targetHero == null) continue;

                        if (!string.IsNullOrWhiteSpace(traitToLevel.ByLevel))
                            level = _variableLoader.GetIntFromXML(traitToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(traitToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(traitToLevel.ByXP);

                        _dynamics.TraitModifier(targetHero, traitToLevel.Id, level, xp, !traitToLevel.HideNotification, traitToLevel.Color);
                    }
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }

        internal void ConsequenceChangeSkill()
        {
            try
            {
                bool isRandom = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random);
                bool isCaptor = listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor);

                Hero captiveHero = isCaptor ? listedEvent.Captive?.HeroObject : Hero.MainHero;
                Hero captorHero = isCaptor ? Hero.MainHero : PlayerCaptivity.CaptorParty?.LeaderHero;

                if (option.SkillsToLevel != null)
                {
                    foreach (SkillToLevel skillToLevel in option.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;
                        string refName = skillToLevel.Ref?.ToLower() ?? "hero";
                        Hero targetHero = null;

                        switch (refName)
                        {
                            case "hero":
                                targetHero = captiveHero;

                                break;
                            case "captor":
                                targetHero = captorHero;

                                break;
                            case "captive":
                                if (!isRandom) targetHero = captiveHero;

                                break;
                        }

                        if (targetHero == null) continue;

                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel))
                            level = _variableLoader.GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(targetHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
                else if (listedEvent.SkillsToLevel != null)
                {
                    foreach (SkillToLevel skillToLevel in listedEvent.SkillsToLevel)
                    {
                        int level = 0;
                        int xp = 0;
                        string refName = skillToLevel.Ref?.ToLower() ?? "hero";
                        Hero targetHero = null;

                        switch (refName)
                        {
                            case "hero":
                                targetHero = captiveHero;

                                break;
                            case "captor":
                                targetHero = captorHero;

                                break;
                            case "captive":
                                if (!isRandom) targetHero = captiveHero;

                                break;
                        }

                        if (targetHero == null) continue;

                        if (!string.IsNullOrWhiteSpace(skillToLevel.ByLevel))
                            level = _variableLoader.GetIntFromXML(skillToLevel.ByLevel);
                        else if (!string.IsNullOrWhiteSpace(skillToLevel.ByXP)) xp = _variableLoader.GetIntFromXML(skillToLevel.ByXP);

                        new Dynamics().SkillModifier(targetHero, skillToLevel.Id, level, xp, !skillToLevel.HideNotification, skillToLevel.Color);
                    }
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        internal void ConsequenceSlaveryLevel()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel)) return;

            try
            {
                if (!string.IsNullOrEmpty(option.SlaveryTotal)) { _dynamics.VictimSlaveryModifier(_variableLoader.GetIntFromXML(option.SlaveryTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(listedEvent.SlaveryTotal)) { _dynamics.VictimSlaveryModifier(_variableLoader.GetIntFromXML(listedEvent.SlaveryTotal), Hero.MainHero); }
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
            bool informationMessage = !option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool noMessages = option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag))
                _dynamics.VictimSlaveryModifier(1, Hero.MainHero, true, !informationMessage && !noMessages, informationMessage && !noMessages);
            else if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) _dynamics.VictimSlaveryModifier(0, Hero.MainHero, true, !informationMessage && !noMessages, informationMessage && !noMessages);
        }

        internal void ConsequenceProstitutionLevel()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel)) return;

            try
            {
                if (!string.IsNullOrEmpty(option.ProstitutionTotal)) { _dynamics.VictimProstitutionModifier(_variableLoader.GetIntFromXML(option.ProstitutionTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(listedEvent.ProstitutionTotal)) { _dynamics.VictimProstitutionModifier(_variableLoader.GetIntFromXML(listedEvent.ProstitutionTotal), Hero.MainHero); }
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
            bool informationMessage = !option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoInformationMessage);
            bool noMessages = option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.NoMessages);

            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag))
                _dynamics.VictimProstitutionModifier(1, Hero.MainHero, true, !informationMessage && !noMessages, informationMessage && !noMessages);
            else if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) _dynamics.VictimProstitutionModifier(0, Hero.MainHero, true, !informationMessage && !noMessages, informationMessage && !noMessages);
        }

        internal void ConsequenceSpawnTroop()
        {
            if (option.SpawnTroops != null)
            {
                new CESpawnSystem().SpawnTheTroops(option.SpawnTroops, PartyBase.MainParty);
            }
        }

        internal void ConsequenceSpawnHero()
        {
            if (option.SpawnHeroes != null)
            {
                new CESpawnSystem().SpawnTheHero(option.SpawnHeroes, PartyBase.MainParty);
            }
        }

        internal void ConsequenceRenown()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown)) return;

            try
            {
                if (!string.IsNullOrEmpty(option.RenownTotal)) { _dynamics.RenownModifier(_variableLoader.GetIntFromXML(option.RenownTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(listedEvent.RenownTotal)) { _dynamics.RenownModifier(_variableLoader.GetIntFromXML(listedEvent.RenownTotal), Hero.MainHero); }
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
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth)) return;

            try
            {
                if (!string.IsNullOrEmpty(option.HealthTotal)) { Hero.MainHero.HitPoints += _variableLoader.GetIntFromXML(option.HealthTotal); }
                else if (!string.IsNullOrEmpty(listedEvent.HealthTotal)) { Hero.MainHero.HitPoints += _variableLoader.GetIntFromXML(listedEvent.HealthTotal); }
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
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            PartyBase party = PlayerCaptivity.IsCaptive
                ? PlayerCaptivity.CaptorParty //captive
                : PartyBase.MainParty; //random, captor

            try
            {
                if (!string.IsNullOrEmpty(option.MoraleTotal)) { _dynamics.MoraleChange(_variableLoader.GetIntFromXML(option.MoraleTotal), party); }
                else if (!string.IsNullOrEmpty(listedEvent.MoraleTotal)) { _dynamics.MoraleChange(_variableLoader.GetIntFromXML(listedEvent.MoraleTotal), party); }
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
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripPlayer)) return;

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

                if (option.StripSettings != null)
                {
                    forced = option.StripSettings.Forced;
                    questEnabled = option.StripSettings.QuestEnabled;
                    clothingLevel = string.IsNullOrWhiteSpace(option.StripSettings.Clothing) ? "default" : option.StripSettings.Clothing.ToLower();
                    mountLevel = string.IsNullOrWhiteSpace(option.StripSettings.Mount) ? "default" : option.StripSettings.Mount.ToLower();
                    meleeLevel = string.IsNullOrWhiteSpace(option.StripSettings.Melee) ? "default" : option.StripSettings.Melee.ToLower();
                    rangedLevel = string.IsNullOrWhiteSpace(option.StripSettings.Ranged) ? "default" : option.StripSettings.Ranged.ToLower();

                    customBody = string.IsNullOrWhiteSpace(option.StripSettings.CustomBody) ? "" : option.StripSettings.CustomBody;
                    customCape = string.IsNullOrWhiteSpace(option.StripSettings.CustomCape) ? "" : option.StripSettings.CustomCape;
                    customGloves = string.IsNullOrWhiteSpace(option.StripSettings.CustomGloves) ? "" : option.StripSettings.CustomGloves;
                    customLegs = string.IsNullOrWhiteSpace(option.StripSettings.CustomLegs) ? "" : option.StripSettings.CustomLegs;
                    customHead = string.IsNullOrWhiteSpace(option.StripSettings.CustomHead) ? "" : option.StripSettings.CustomHead;
                }

                if (CESettingsIntegrations.Instance == null && clothingLevel == "slave" || !CESettingsIntegrations.Instance.ActivateKLBShackles && clothingLevel == "slave") return;

                if (!(CESettings.Instance?.StolenGear ?? true) && !forced) return;
                Equipment randomElement = new();

                if (clothingLevel != "nude")
                {
                    if (CEHelper.HelperMBRandom(100) < (CESettings.Instance?.BetterOutFitChance ?? 25) && clothingLevel == "default" || clothingLevel == "advanced")
                    {
                        string bodyString;
                        string legString;
                        string headString;
                        string capeString;
                        string glovesString;

                        switch (PlayerCaptivity.CaptorParty?.Culture != null ? PlayerCaptivity.CaptorParty?.Culture.Name.ToString().ToLower() : null)
                        {
                            case CampaignData.CultureSturgia:
                                headString = "nordic_fur_cap";
                                capeString = Hero.MainHero.IsFemale ? "female_hood" : "";
                                bodyString = Hero.MainHero.IsFemale ? "cut_dress" : "heavy_nordic_tunic";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                glovesString = "armwraps";

                                break;

                            case CampaignData.CultureNord:
                                headString = "nordic_fur_cap";
                                capeString = Hero.MainHero.IsFemale ? "female_hood" : "";
                                bodyString = Hero.MainHero.IsFemale ? "cut_dress" : "heavy_nordic_tunic";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                glovesString = "armwraps";

                                break;

                            case CampaignData.CultureAserai:
                                headString = Hero.MainHero.IsFemale ? "" : "turban";
                                bodyString = Hero.MainHero.IsFemale ? "aserai_villager_female_dress" : "aserai_tunic_waistcoat";
                                legString = Hero.MainHero.IsFemale ? "southern_moccasins" : "wrapped_shoes";
                                capeString = "wrapped_scarf";
                                glovesString = "armwraps";

                                break;

                            case CampaignData.CultureKhuzait:
                                headString = "fur_hat";
                                capeString = "wrapped_scarf";
                                bodyString = Hero.MainHero.IsFemale ? "khuzait_dress" : "steppe_armor";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                glovesString = "armwraps";

                                break;

                            case CampaignData.CultureEmpire:
                                headString = Hero.MainHero.IsFemale ? "female_head_wrap" : "arming_cap";
                                bodyString = Hero.MainHero.IsFemale ? "vlandian_corset_dress" : "padded_leather_shirt";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                capeString = "wrapped_scarf";
                                glovesString = "armwraps";

                                break;

                            case CampaignData.CultureBattania:
                                headString = Hero.MainHero.IsFemale ? "female_head_wrap" : "wrapped_headcloth";
                                capeString = Hero.MainHero.IsFemale ? "wrapped_scarf" : "battania_shoulder_strap";
                                glovesString = "armwraps";
                                bodyString = Hero.MainHero.IsFemale ? "battania_dress_c" : "burlap_waistcoat";
                                legString = "ragged_boots";

                                break;

                            case CampaignData.CultureVlandia:
                                headString = Hero.MainHero.IsFemale ? "female_head_wrap" : "arming_cap";
                                bodyString = Hero.MainHero.IsFemale ? "vlandian_corset_dress" : "padded_leather_shirt";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "ragged_boots";
                                capeString = "wrapped_scarf";
                                glovesString = "armwraps";

                                break;

                            default:
                                headString = Hero.MainHero.IsFemale ? "female_head_wrap" : "wrapped_headcloth";
                                capeString = Hero.MainHero.IsFemale ? "female_scarf" : "battania_shoulder_strap";
                                bodyString = Hero.MainHero.IsFemale ? "plain_dress" : "padded_leather_shirt";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "ragged_boots";
                                glovesString = "";

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
                        ItemObject itemObjectBody = customBody != "" ? MBObjectManager.Instance.GetObject<ItemObject>(customBody) : null;
                        ItemObject itemObjectCape = customCape != "" ? MBObjectManager.Instance.GetObject<ItemObject>(customCape) : null;
                        ItemObject itemObjectGloves = customGloves != "" ? MBObjectManager.Instance.GetObject<ItemObject>(customGloves) : null;
                        ItemObject itemObjectLeg = customLegs != "" ? MBObjectManager.Instance.GetObject<ItemObject>(customLegs) : null;
                        ItemObject itemObjectHead = customHead != "" ? MBObjectManager.Instance.GetObject<ItemObject>(customHead) : null;

                        if (itemObjectBody != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                        if (itemObjectCape != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Cape, new EquipmentElement(itemObjectCape));
                        if (itemObjectGloves != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Gloves, new EquipmentElement(itemObjectGloves));
                        if (itemObjectLeg != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Leg, new EquipmentElement(itemObjectLeg));
                        if (itemObjectHead != null) randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Head, new EquipmentElement(itemObjectHead));
                    }
                    else
                    {
                        ItemObject itemObjectBody = Hero.MainHero.IsFemale ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress") : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                    }
                }

                if (meleeLevel != "none" || meleeLevel == "default" && CEHelper.HelperMBRandom(100) < (CESettings.Instance?.WeaponChance ?? 75))
                {
                    string item;

                    if (CEHelper.HelperMBRandom(100) < ((CESettings.Instance?.WeaponSkill ?? true) ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.TwoHanded) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) / 275 * 100)) : (CESettings.Instance?.WeaponChance ?? 75)) && meleeLevel == "Default" || meleeLevel == "Advanced")
                    {
                        item = (PlayerCaptivity.CaptorParty?.Culture != null ? PlayerCaptivity.CaptorParty?.Culture.Name.ToString().ToLower() : null) switch
                               {
                                   CampaignData.CultureSturgia => "sturgia_axe_3_t3",
                                   CampaignData.CultureAserai => "eastern_spear_1_t2",
                                   CampaignData.CultureEmpire => "northern_spear_1_t2",
                                   CampaignData.CultureBattania => "aserai_sword_1_t2",
                                   _ => "vlandia_sword_1_t2",
                               };
                    }
                    else
                    {
                        item = (PlayerCaptivity.CaptorParty?.Culture != null ? PlayerCaptivity.CaptorParty?.Culture.Name.ToString().ToLower() : null) switch
                               {
                                   CampaignData.CultureSturgia => "seax",
                                   CampaignData.CultureAserai => "celtic_dagger",
                                   CampaignData.CultureEmpire => "gladius_b",
                                   CampaignData.CultureBattania => "hooked_cleaver",
                                   _ => "seax",
                               };
                    }

                    ItemObject itemObjectWeapon0 = MBObjectManager.Instance.GetObject<ItemObject>(item);
                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon0, new EquipmentElement(itemObjectWeapon0));
                }

                if (rangedLevel != "none")
                {
                    if (CEHelper.HelperMBRandom(100) < (CESettings.Instance?.WeaponChance ?? 75) && CEHelper.HelperMBRandom(100) < ((CESettings.Instance?.RangedSkill ?? true) ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Bow) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Throwing) / 275 * 100)) : (CESettings.Instance?.RangedBetterChance ?? 5)) && rangedLevel == "default" || rangedLevel == "advanced")
                    {
                        string rangedItem;
                        string rangedAmmo = null;

                        switch (PlayerCaptivity.CaptorParty?.Culture != null ? PlayerCaptivity.CaptorParty?.Culture.Name.ToString().ToLower() : null)
                        {
                            case CampaignData.CultureSturgia:
                                rangedItem = "nordic_shortbow";
                                rangedAmmo = "default_arrows";

                                break;

                            case CampaignData.CultureNord:
                                rangedItem = "nordic_shortbow";
                                rangedAmmo = "default_arrows";

                                break;

                            case CampaignData.CultureVlandia:
                                rangedItem = "crossbow_a";
                                rangedAmmo = "tournament_bolts";

                                break;

                            case CampaignData.CultureAserai:
                                rangedItem = "tribal_bow";
                                rangedAmmo = "default_arrows";

                                break;

                            case CampaignData.CultureEmpire:
                                rangedItem = "hunting_bow";
                                rangedAmmo = "default_arrows";

                                break;

                            case CampaignData.CultureBattania:
                                rangedItem = "northern_javelin_2_t3";

                                break;

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

                Equipment randomElement2 = new();
                randomElement2.FillFrom(randomElement, false);

                if (CEHelper.HelperMBRandom(100) < ((CESettings.Instance?.HorseSkill ?? true) ? Hero.MainHero.GetSkillValue(DefaultSkills.Riding) / 275 * 100 : (CESettings.Instance?.HorseChance ?? 10)) && mountLevel == "default" || mountLevel == "basic")
                {
                    ItemObject poorHorse = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                    EquipmentElement horseEquipment = new(poorHorse);

                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Horse, horseEquipment);
                }

                bool isEquipmentTheSame = new Equipment(Hero.MainHero.BattleEquipment).IsEquipmentEqualTo(randomElement) && new Equipment(Hero.MainHero.CivilianEquipment).IsEquipmentEqualTo(randomElement2);

                if ((CESettings.Instance?.StolenGearQuest ?? true) && CEHelper.HelperMBRandom(100) < (CESettings.Instance?.StolenGearChance ?? 99) && questEnabled && !isEquipmentTheSame)
                {
                    Hero issueOwner = null;
                    List<TextObject> listOfSettlements = [];

                    while (issueOwner == null)
                    {
                        Settlement nearestSettlement = SettlementHelper.FindNearestSettlementToPoint(CEHelper.GetPlayerPositionClean(), settlement => !listOfSettlements.Contains(settlement.Name));
                        listOfSettlements.Add(nearestSettlement.Name);

                        if (nearestSettlement.IsUnderRaid || nearestSettlement.IsRaided) continue;

                        issueOwner = nearestSettlement.Notables.FirstOrDefault((y) => y.CanHaveCampaignIssues() && y.GetTraitLevel(DefaultTraits.Mercy) <= 0);

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
                CECustomHandler.ForceLogToFile("ConsequenceStripPlayer : " + e);
            }
        }

        internal void ConsequenceStartBattle(Action callback, int type)
        {
            try
            {
                if (option.BattleSettings != null)
                {
                    CEPersistence.AnimationPlayEvent = false;

                    try
                    {
                        CEEvent victoryEvent = eventList.Find(item => item.Name == option.BattleSettings.Victory);
                        victoryEvent.Captive = listedEvent.Captive;
                        victoryEvent.SavedCompanions = listedEvent.SavedCompanions;

                        CEPersistence.VictoryEvent = victoryEvent.Name;
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle VictoryEvent Missing");
                        callback();

                        return;
                    }

                    try
                    {
                        CEEvent defeatEvent = eventList.Find(item => item.Name == option.BattleSettings.Defeat);
                        defeatEvent.Captive = listedEvent.Captive;
                        defeatEvent.SavedCompanions = listedEvent.SavedCompanions;

                        CEPersistence.DefeatEvent = defeatEvent.Name;
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
                        if (option.BattleSettings.SpawnTroops != null)
                        {
                            foreach (SpawnTroop troop in option.BattleSettings.SpawnTroops)
                            {
                                try
                                {
                                    int num = _variableLoader.GetIntFromXML(troop.Number);
                                    int numWounded = _variableLoader.GetIntFromXML(troop.WoundedNumber);
                                    CharacterObject characterObject;

                                    if (troop.Id != null && troop.Id.ToLower() == "random")
                                    {
                                        characterObject = CharacterObject.All.GetRandomElementWithPredicate((t) => !t.IsHero && t.Occupation == Occupation.Soldier);
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
                                            if (troop.Ref != null && troop.Ref.ToLower() == "friend")
                                            {
                                                friendlyTroops.AddToCounts(characterObject, num, false, numWounded);
                                            }
                                            else if (troop.Ref != null && troop.Ref.ToLower() == "temporary")
                                            {
                                                temporaryTroops.AddToCounts(characterObject, num, false, numWounded);
                                            }
                                            else
                                            {
                                                enemyTroops.AddToCounts(characterObject, num, false, numWounded);
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
                                CharacterObject characterObject = CharacterObject.All.GetRandomElementWithPredicate((t) => !t.IsHero && t.Occupation == Occupation.Soldier);
                                enemyTroops.AddToCounts(characterObject, 1, true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle SpawnTroops Failed");
                    }

                    if (!enemyTroops.GetTroopRoster().IsEmpty() && option.BattleSettings.Ref != null)
                    {
                        callback();
                        Hero.MainHero.HitPoints += 40;
                        CEPersistence.PlayerTroops.Clear();

                        try
                        {
                            // Player Party Setup
                            foreach (TroopRosterElement troopRosterElement in PartyBase.MainParty.MemberRoster.GetTroopRoster())
                            {
                                if (!troopRosterElement.Character.IsPlayerCharacter) CEPersistence.PlayerTroops.Add(troopRosterElement);
                            }

                            PartyBase.MainParty.MemberRoster.RemoveIf((t) => !t.Character.IsPlayerCharacter);

                            if (!PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter))
                            {
                                CEPersistence.RemovePlayer = true;
                                PartyBase.MainParty.MemberRoster.AddToCounts(CharacterObject.PlayerCharacter, 1);
                            }
                            else
                            {
                                CEPersistence.RemovePlayer = false;
                            }

                            if (!CEPersistence.PlayerTroops.IsEmpty())
                            {
                                List<CharacterObject> list = [];
                                int num = _variableLoader.GetIntFromXML(option.BattleSettings.PlayerTroops);

                                foreach (TroopRosterElement troopRosterElement in from t in CEPersistence.PlayerTroops
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
                                    PartyBase.MainParty.MemberRoster.AddToCounts(character, 1);
                                }
                            }

                            foreach (TroopRosterElement troopRosterElement in temporaryTroops.GetTroopRoster())
                            {
                                PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, troopRosterElement.WoundedNumber);
                                CEPersistence.TemporaryTroops.Add(troopRosterElement);
                            }

                            foreach (TroopRosterElement troopRosterElement in friendlyTroops.GetTroopRoster())
                            {
                                PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, troopRosterElement.WoundedNumber);
                                CEPersistence.PlayerTroops.Add(troopRosterElement);
                            }
                            // Player Party Setup Ends Here

                            switch (option.BattleSettings.Ref.ToLower())
                            {
                                case "city":
                                {
                                    if (Settlement.CurrentSettlement == null)
                                    {
                                        CECustomHandler.ForceLogToFile("ConsequenceStartBattle : city required. ");
                                        CEPersistence.VictoryEvent = null;
                                        CEPersistence.DefeatEvent = null;

                                        return;
                                    }

                                    CEPersistence.CurrentBattleState = CEPersistence.BattleState.StartBattle;
                                    CEPersistence.DestroyParty = false;
                                    CEPersistence.SurrenderParty = false;

                                    //PlayerEncounter StartVillageBattleMission StartAlleyFightWithOtherAlley
                                    int wallLevel = Settlement.CurrentSettlement.Town.GetWallLevel();
                                    string scene = Settlement.CurrentSettlement.LocationComplex.GetScene("center", wallLevel);
                                    Location locationWithId = LocationComplex.Current.GetLocationWithId("center");

                                    CampaignMission.OpenAlleyFightMission(scene, wallLevel, locationWithId, PartyBase.MainParty.MemberRoster, enemyTroops);

                                    break;
                                }
                                case "regularspawn":
                                {
                                    //SpawnAPartyInFaction
                                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                                    clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                                    Settlement nearest = SettlementHelper.FindNearestSettlementToPoint(CEHelper.GetPlayerPositionClean(), _ => true);

                                    MobileParty customParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false, null, CEHelper.GetSpawnPositionAroundSettlement(nearest));

                                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                                    customParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position, 0.5f, 0.1f);

                                    customParty.MemberRoster.Clear();
                                    customParty.MemberRoster.Add(enemyTroops);

                                    TextObject textObject = new(option.BattleSettings.EnemyName ?? "Bandits");
                                    customParty.Party.SetCustomName(textObject);

                                    // InitBanditParty
                                    customParty.Party.SetVisualAsDirty();
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
                                    customParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position, customParty.NavigationCapability);

                                    PlayerEncounter.RestartPlayerEncounter(customParty.Party, PartyBase.MainParty);
                                    CEPersistence.CurrentBattleState = CEPersistence.BattleState.StartBattle;
                                    CEPersistence.DestroyParty = false;
                                    CEPersistence.SurrenderParty = true;
                                    PlayerEncounter.StartBattle();
                                    PlayerEncounter.Update();
                                    //EncounterAttackConsequence

                                    bool flag = PlayerEncounter.IsNavalEncounter();
                                    MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position);
                                    string battleSceneForMapPatch = Campaign.Current.Models.SceneModel.GetBattleSceneForMapPatch(mapPatchAtPosition, flag);
                                    CampaignVec2 campaignVec = CampaignVec2.Normalized(PlayerEncounter.Battle.AttackerSide.LeaderParty.Position - PlayerEncounter.Battle.DefenderSide.LeaderParty.Position);

                                    MissionInitializerRecord rec = new(battleSceneForMapPatch)
                                                                   {
                                                                       TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace),
                                                                       DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier(),
                                                                       DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier(),
                                                                       NeedsRandomTerrain = false,
                                                                       PlayingInCampaignMode = true,
                                                                       RandomTerrainSeed = MBRandom.RandomInt(10000),
                                                                       AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position),
                                                                       SceneHasMapPatch = true,
                                                                       DecalAtlasGroup = 2,
                                                                       PatchCoordinates = mapPatchAtPosition.normalizedCoordinates,
                                                                       PatchEncounterDir = campaignVec.ToVec2(),
                                                                   };


                                    bool flag2 = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((involvedParty) => involvedParty.Party.IsMobile && (involvedParty.Party.MobileParty.IsCaravan || involvedParty.Party.Owner is { IsMerchant: true }));

                                    if (flag)
                                    {
                                        CampaignMission.OpenNavalBattleMission(rec);
                                    }
                                    else if (flag2)
                                    {
                                        CampaignMission.OpenCaravanBattleMission(rec, flag2);
                                    }
                                    else
                                    {
                                        CampaignMission.OpenBattleMission(rec);
                                    }

                                    break;
                                }
                                case "regular":
                                {
                                    //SpawnAPartyInFaction
                                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                                    clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                                    Settlement nearest = SettlementHelper.FindNearestSettlementToPoint(CEHelper.GetPlayerPositionClean(), _ => true);

                                    MobileParty customParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false, null, CEHelper.GetSpawnPositionAroundSettlement(nearest));
                                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                                    customParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position, 0.5f, 0.1f);

                                    customParty.MemberRoster.Clear();
                                    customParty.MemberRoster.Add(enemyTroops);

                                    TextObject textObject = new(option.BattleSettings.EnemyName ?? "Bandits");
                                    customParty.Party.SetCustomName(textObject);

                                    // InitBanditParty
                                    customParty.Party.SetVisualAsDirty();
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
                                    customParty.SetMovePatrolAroundPoint(nearest.IsTown ? nearest.GatePosition : nearest.Position, customParty.NavigationCapability);

                                    PlayerEncounter.RestartPlayerEncounter(customParty.Party, PartyBase.MainParty);
                                    CEPersistence.CurrentBattleState = CEPersistence.BattleState.StartBattle;
                                    CEPersistence.DestroyParty = true;
                                    CEPersistence.SurrenderParty = false;
                                    PlayerEncounter.StartBattle();
                                    PlayerEncounter.Update();
                                    //EncounterAttackConsequence

                                    bool flag = PlayerEncounter.IsNavalEncounter();
                                    MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position);
                                    string battleSceneForMapPatch = Campaign.Current.Models.SceneModel.GetBattleSceneForMapPatch(mapPatchAtPosition, flag);
                                    CampaignVec2 campaignVec = CampaignVec2.Normalized(PlayerEncounter.Battle.AttackerSide.LeaderParty.Position - PlayerEncounter.Battle.DefenderSide.LeaderParty.Position);

                                    MissionInitializerRecord rec = new(battleSceneForMapPatch)
                                                                   {
                                                                       TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace),
                                                                       DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier(),
                                                                       DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier(),
                                                                       NeedsRandomTerrain = false,
                                                                       PlayingInCampaignMode = true,
                                                                       RandomTerrainSeed = MBRandom.RandomInt(10000),
                                                                       AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position),
                                                                       SceneHasMapPatch = true,
                                                                       DecalAtlasGroup = 2,
                                                                       PatchCoordinates = mapPatchAtPosition.normalizedCoordinates,
                                                                       PatchEncounterDir = campaignVec.ToVec2(),
                                                                   };


                                    bool flag2 = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((involvedParty) => involvedParty.Party.IsMobile && (involvedParty.Party.MobileParty.IsCaravan || involvedParty.Party.Owner is { IsMerchant: true }));

                                    if (flag)
                                    {
                                        CampaignMission.OpenNavalBattleMission(rec);
                                    }
                                    else if (flag2)
                                    {
                                        CampaignMission.OpenCaravanBattleMission(rec, flag2);
                                    }
                                    else
                                    {
                                        CampaignMission.OpenBattleMission(rec);
                                    }

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
                if (CEPersistence.SoundEvent != null)
                {
                    CEPersistence.SoundEvent.Stop();
                    CEPersistence.SoundLoop = false;
                }

                string soundToPlay = isListedEvent ? listedEvent.SoundName : option.SoundName;

                if (soundToPlay == null) return;
                int soundIndex = SoundEvent.GetEventIdFromString(soundToPlay);

                if (soundIndex == -1) return;

                Campaign campaign = Campaign.Current;
                Scene mapScene = null;

                if ((campaign?.MapSceneWrapper) != null)
                {
                    mapScene = ((MapScene)Campaign.Current.MapSceneWrapper).Scene;
                }

                CEPersistence.SoundEvent = SoundEvent.CreateEvent(soundIndex, mapScene);
                CEPersistence.SoundEvent.Play();
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

        internal void ConsequenceDamageParty(PartyBase party)
        {
            if (option.DamageParty == null) return;

            try
            {
                DamageParty damageParty = option.DamageParty;

                if (damageParty.Ref == "Troop")
                {
                    _dynamics.CEWoundTroops(party, _variableLoader.GetIntFromXML(damageParty.WoundedNumber));
                    _dynamics.CEKillTroops(party, _variableLoader.GetIntFromXML(damageParty.Number), damageParty.IncludeHeroes.ToLower() == "true");
                }
                else if (damageParty.Ref == "Prisoner")
                {
                    _dynamics.CEWoundPrisoners(party, _variableLoader.GetIntFromXML(damageParty.WoundedNumber));
                    _dynamics.CEKillPrisoners(party, _variableLoader.GetIntFromXML(damageParty.Number), damageParty.IncludeHeroes.ToLower() == "true");
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsquenceDamageParty Failed: " + e);
            }
        }

        internal void ConsequenceTeleportPlayer()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.TeleportPlayer)) return;

            try
            {
                TeleportSettings teleportSettings = new();

                if (option.TeleportSettings != null)
                {
                    teleportSettings = option.TeleportSettings;
                }
                else
                {
                    CECustomHandler.ForceLogToFile("ConsequenceTeleportPlayer Failed: Missing TeleportSettings");
                }

                Settlement nearest;

                if (teleportSettings.LocationName != null)
                {
                    nearest = SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), settlement => settlement.IsTown && settlement.MapFaction != Hero.MainHero.MapFaction);

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
                                          "random" => SettlementHelper.FindRandomSettlement(settlement => TeleportChecker(settlement.IsVillage, settlement, faction)),
                                          _ => SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), settlement => TeleportChecker(settlement.IsVillage, settlement, faction)),
                                      };

                            break;

                        case "castle":
                            nearest = distance switch
                                      {
                                          "random" => SettlementHelper.FindRandomSettlement(settlement => TeleportChecker(settlement.IsCastle, settlement, faction)),
                                          _ => SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), settlement => TeleportChecker(settlement.IsCastle, settlement, faction)),
                                      };

                            break;

                        case "hideout":
                            nearest = distance switch
                                      {
                                          "random" => SettlementHelper.FindRandomSettlement(settlement => TeleportChecker(settlement.IsHideout, settlement, faction)),
                                          _ => SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), settlement => TeleportChecker(settlement.IsHideout, settlement, faction)),
                                      };
                            nearest.Hideout.IsSpotted = true;

                            break;

                        default:
                            nearest = distance switch
                                      {
                                          "random" => SettlementHelper.FindRandomSettlement(settlement => TeleportChecker(settlement.IsTown, settlement, faction)),
                                          _ => SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), settlement => TeleportChecker(settlement.IsTown, settlement, faction)),
                                      };

                            break;
                    }

                    if (Hero.MainHero.IsPrisoner)
                    {
                        try
                        {
                            Hero prisonerCharacter = Hero.MainHero;
                            PartyBase party = nearest.Party;

                            prisonerCharacter.PartyBelongedToAsPrisoner?.PrisonRoster.RemoveTroop(prisonerCharacter.CharacterObject);
                            prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
                            prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
                            party.AddPrisoner(prisonerCharacter.CharacterObject, 1);

                            PlayerCaptivity.StartCaptivity(party);
                            CEHelper.DelayedEvents.Clear();
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.LogToFile("Failed to ConsequenceTeleportPlayer: " + e.Message + " stacktrace: " + e.StackTrace);
                        }
                    }
                    else
                    {
                        MobileParty.MainParty.Position = nearest.GatePosition;
                        EncounterManager.StartSettlementEncounter(MobileParty.MainParty, nearest);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceTeleportPlayer Failed: " + e);
            }
        }

        internal void ConsequenceDelayedEvent()
        {
            try
            {
                if (option.DelayEvent != null)
                {
                    if (option.DelayEvent.TriggerEvents != null)
                    {
                        foreach (TriggerEvent trigger in option.DelayEvent.TriggerEvents)
                        {
                            CEDelayedEvent delayedEvent = new(trigger.EventName, -1, trigger.EventUseConditions?.ToLower() != "true");
                            CEHelper.AddDelayedEvent(delayedEvent);
                        }
                    }
                    else
                    {
                        CEDelayedEvent delayedEvent = new(option.DelayEvent.TriggerEventName, option.DelayEvent.TimeToTake != null ? float.Parse(option.DelayEvent.TimeToTake) : -1, option.DelayEvent.UseConditions?.ToLower() != "true");
                        CEHelper.AddDelayedEvent(delayedEvent);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("ConsequenceDelayedEvent Failed: " + e);
            }
        }

        internal void ConsequenceMission()
        {
            try
            {
                if (option.SceneSettings != null)
                {
                    ConversationCharacterData data1 = new(Hero.MainHero.CharacterObject);

                    CharacterObject character2 = null;

                    switch (option.SceneSettings.TalkTo?.ToLower())
                    {
                        case "none":
                            break;

                        default:
                            character2 = Hero.MainHero.IsPrisoner ? Hero.MainHero.PartyBelongedToAsPrisoner.LeaderHero?.CharacterObject ?? Hero.MainHero.PartyBelongedToAsPrisoner.MemberRoster.GetCharacterAtIndex(0) : listedEvent.Captive;

                            break;
                    }

                    character2?.StringId = "CECustomStringId_" + option.SceneSettings.SceneName;
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

#region Icons

        internal void InitIcons(ref MenuCallbackArgs args)
        {
            Escaping(ref args);
            Leave(ref args);
            Wait(ref args);
            Trade(ref args);
            RansomAndBribe(ref args);
            BribeAndEscape(ref args);
            SubMenu(ref args);
            Continue(ref args);
            EmptyIcon(ref args);
        }

        private void EmptyIcon(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;
        }

        private void Continue(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;
        }

        private void SubMenu(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
        }

        private void BribeAndEscape(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.BribeAndEscape)) args.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
        }

        private void RansomAndBribe(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe)) args.optionLeaveType = GameMenuOption.LeaveType.Ransom;
        }

        private void Trade(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade)) args.optionLeaveType = GameMenuOption.LeaveType.Trade;
        }

        private void Wait(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait)) args.optionLeaveType = GameMenuOption.LeaveType.Wait;
        }

        private void Leave(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Leave)) args.optionLeaveType = GameMenuOption.LeaveType.Leave;
        }

        private void Escaping(ref MenuCallbackArgs args)
        {
            if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AttemptEscape) || option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape) || option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EscapeIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Escape;
        }

#endregion Icons

        internal void InitGiveItem()
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveItem)) return;

            try
            {
                string[] items = _variableLoader.GetStringFromXML(option.ItemToGive);

                for (int i = 0; i < items.Length; i++)
                {
                    try
                    {
                        ItemObject itemObjectBody = null;

                        if (!string.IsNullOrWhiteSpace(items[i]))
                            itemObjectBody = MBObjectManager.Instance.GetObject<ItemObject>(items[i]);
                        else
                            CECustomHandler.LogToFile("Missing GiveItem");
                        if (i == 0) MBTextManager.SetTextVariable("ITEM_TO_GIVE", itemObjectBody?.Name?.ToString() ?? "");
                        MBTextManager.SetTextVariable("ITEM_TO_GIVE_" + i, itemObjectBody?.Name?.ToString() ?? "");
                    }
                    catch (Exception) { CECustomHandler.LogToFile("Invalid GiveItem - " + items[i]); }
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GiveItem"); }
        }


        internal bool ShouldHide(ref MenuCallbackArgs args)
        {
            if (!option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.UnavailableIsInvisible)) return true;

            return args.IsEnabled;
        }

        internal bool CheckUseConditions(ref MenuCallbackArgs args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(option?.UseConditions)) return true;

                CEEvent conditionEvent = eventList.Find(item => item.Name == option.UseConditions);

                if (conditionEvent == null)
                {
                    CECustomHandler.LogToFile("UseConditions event not found: " + option.UseConditions);

                    return true;
                }

                string conditionMatched = null;

                if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                {
                    conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter, PlayerCaptivity.CaptorParty);
                }
                else if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                {
                    conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
                }
                else if (conditionEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                {
                    conditionMatched = new CEEventChecker(conditionEvent).FlagsDoMatchEventConditions(listedEvent.Captive, PartyBase.MainParty);
                }

                if (conditionMatched != null)
                {
                    args.IsEnabled = false;
                    args.Tooltip = GameTexts.FindText("str_CE_conditions_not_met");
                    CECustomHandler.LogToFile("MenuOption disabled: " + conditionMatched);

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("CheckUseConditions failed: " + e.Message);

                return true;
            }
        }

        internal void LoadBackgroundImage(string textureFlag = "", CharacterObject specificCaptive = null)
        {
            try
            {
                if (!(CESettings.Instance?.CustomBackgrounds ?? true))
                {
                    CEPersistence.AnimationPlayEvent = false;
                    new CESubModule().LoadTexture(textureFlag);

                    return;
                }

                if (listedEvent.Backgrounds != null)
                {
                    List<string> backgroundNames = [];

                    foreach (Background background in listedEvent.Backgrounds)
                    {
                        try
                        {
                            int weightedChance = 0;

                            if (background.UseConditions != null && background.UseConditions.ToLower() != "false")
                            {
                                CEEvent triggeredEvent = eventList.Find(item => item.Name == background.UseConditions);

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
                                    CECustomHandler.LogToFile("IgnoreAllOther detected - auto fire " + triggeredEvent.Name);
                                    backgroundNames.Add(background.Name);

                                    break;
                                }

                                try
                                {
                                    weightedChance = _variableLoader.GetIntFromXML(!string.IsNullOrWhiteSpace(background.Weight) ? background.Weight : triggeredEvent.WeightedChanceOfOccurring);
                                }
                                catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }
                            }
                            else
                            {
                                try
                                {
                                    weightedChance = _variableLoader.GetIntFromXML(background.Weight);
                                }
                                catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }
                            }

                            if (weightedChance == 0) weightedChance = 1;

                            for (int a = weightedChance; a > 0; a--) backgroundNames.Add(background.Name);
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Failed to generate a background for " + listedEvent.Name + " " + e);
                        }
                    }

                    CEPersistence.AnimationPlayEvent = false;

                    if (backgroundNames.Count > 0)
                    {
                        int number = CEHelper.HelperMBRandom(0, backgroundNames.Count);

                        try
                        {
                            new CESubModule().LoadTexture(backgroundNames[number]);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Failed to load background for " + listedEvent.Name);
                            new CESubModule().LoadTexture(textureFlag);
                        }
                    }
                    else
                    {
                        CECustomHandler.ForceLogToFile("Failed to find valid events for " + listedEvent.Name);
                        new CESubModule().LoadTexture(textureFlag);
                    }
                }
                else
                {
                    string backgroundName = listedEvent.BackgroundName;

                    if (!string.IsNullOrWhiteSpace(backgroundName))
                    {
                        CEPersistence.AnimationPlayEvent = false;
                        new CESubModule().LoadTexture(backgroundName);
                    }
                    else if (listedEvent.BackgroundAnimation is { Count: > 0 })
                    {
                        CEPersistence.AnimationImageList = listedEvent.BackgroundAnimation;
                        CEPersistence.AnimationIndex = 0;
                        CEPersistence.AnimationPlayEvent = true;
                        float speed = 0.03f;

                        try
                        {
                            if (!string.IsNullOrWhiteSpace(listedEvent.BackgroundAnimationSpeed)) speed = _variableLoader.GetFloatFromXML(listedEvent.BackgroundAnimationSpeed);
                        }
                        catch (Exception e)
                        {
                            // Will force log if cannot load animation speed
                            CECustomHandler.ForceLogToFile("Failed to load BackgroundAnimationSpeed for " + listedEvent.Name + " : Exception: " + e);
                        }

                        CEPersistence.AnimationSpeed = speed;
                    }
                    else
                    {
                        CEPersistence.AnimationPlayEvent = false;
                        new CESubModule().LoadTexture(textureFlag);
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to load background for " + listedEvent.Name);
                new CESubModule().LoadTexture(textureFlag);
            }
        }
    }
}
