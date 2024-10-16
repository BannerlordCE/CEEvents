#define V120

using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;

using static CaptivityEvents.Helper.CEHelper;

namespace CaptivityEvents.Events
{
    public class CaptorSpecifics
    {
        internal void CECaptorContinue(MenuCallbackArgs args)
        {
            CEPersistence.animationPlayEvent = false;

            try
            {
                if (PlayerCaptivity.CaptorParty != null)
                {
                    string waitingList = WaitingList.CEWaitingList();

                    if (waitingList != null)
                    {
                        GameMenu.ActivateGameMenu(waitingList);
                    }
                    else
                    {
                        new CESubModule().LoadTexture("default");

                        GameMenu.SwitchToMenu(PlayerCaptivity.CaptorParty.IsSettlement
                                                  ? "settlement_wait"
                                                  : "prisoner_wait");
                    }
                }
                else
                {
                    if (CECampaignBehavior.ExtraProps.menuToSwitchBackTo != null)
                    {
                        if (CECampaignBehavior.ExtraProps.menuToSwitchBackTo != "prisoner_wait")
                        {
                            GameMenu.SwitchToMenu(CECampaignBehavior.ExtraProps.menuToSwitchBackTo);
                        }
                        else
                        {
                            CECustomHandler.ForceLogToFile("General Error: CECaptorContinue : menuToSwitchBackTo : prisoner_wait");
                            if (Settlement.CurrentSettlement != null)
                            {
                                EncounterManager.StartSettlementEncounter(MobileParty.MainParty, Settlement.CurrentSettlement);
                            }
                            else
                            {
                                GameMenu.ExitToLast();
                            }
                            Campaign.Current.TimeControlMode = Campaign.Current.LastTimeControlMode;
                            new CESubModule().LoadTexture("default");
                            return;
                        }
                        CECampaignBehavior.ExtraProps.menuToSwitchBackTo = null;

                        if (CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo != null)
                        {
                            args.MenuContext.SetBackgroundMeshName(CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo);
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = null;
                        }
                    }
                    else
                    {
                        if (Settlement.CurrentSettlement != null)
                        {
                            EncounterManager.StartSettlementEncounter(MobileParty.MainParty, Settlement.CurrentSettlement);
                        }
                        else
                        {
                            GameMenu.ExitToLast();
                        }
                    }

                    Campaign.Current.TimeControlMode = Campaign.Current.LastTimeControlMode;
                    new CESubModule().LoadTexture("default");
                }
            }
            catch (Exception e)
            {
                CECampaignBehavior.ExtraProps.menuToSwitchBackTo = null;
                CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = null;
                CECustomHandler.ForceLogToFile("Critical Error: CECaptorContinue : " + e);
            }
        }

        internal void CECaptorReleasePrisoners(MenuCallbackArgs args, int amount = 10, bool releaseHeroes = false)
        {
            try
            {
                int prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                MobileParty.MainParty.PrisonRoster.KillNumberOfNonHeroTroopsRandomly(amount);
                if (releaseHeroes)
                {
                    foreach (TroopRosterElement element in MobileParty.MainParty.PrisonRoster.GetTroopRoster())
                    {
                        if (element.Character.IsHero) element.Character.HeroObject.ChangeState(Hero.CharacterStates.Active);
                    }
                    MobileParty.MainParty.PrisonRoster.Clear();
                }

                TextObject textObject = GameTexts.FindText("str_CE_release_prisoners");
                textObject.SetTextVariable("HERO", Hero.MainHero.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't release any prisoners.");
            }
        }

        internal void CECaptorWoundPrisoners(MenuCallbackArgs args, int amount = 10)
        {
            try
            {
                if (amount == 0) return;
                int prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                MobileParty.MainParty.PrisonRoster.WoundNumberOfTroopsRandomly(amount);
                TextObject textObject = GameTexts.FindText("str_CE_wound_prisoners");
                textObject.SetTextVariable("HERO", Hero.MainHero.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't wound any prisoners.");
            }
        }

        internal void CECaptorKillPrisoners(MenuCallbackArgs args, int amount = 10, bool killHeroes = false)
        {
            try
            {
                if (amount == 0) return;
                int prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                MobileParty.MainParty.PrisonRoster.KillNumberOfNonHeroTroopsRandomly(amount);
                TextObject textObject = GameTexts.FindText("str_CE_kill_prisoners");
                textObject.SetTextVariable("HERO", Hero.MainHero.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't kill any prisoners.");
            }
        }

        internal void CECaptorPrisonerRebel(MenuCallbackArgs args)
        {
            CEPersistence.animationPlayEvent = false;

            TroopRoster releasedPrisoners = TroopRoster.CreateDummyTroopRoster();

            try
            {
                foreach (TroopRosterElement element in MobileParty.MainParty.PrisonRoster.GetTroopRoster())
                {
                    if (element.Character.IsHero) element.Character.HeroObject.ChangeState(Hero.CharacterStates.Active);
                }
                releasedPrisoners.Add(MobileParty.MainParty.PrisonRoster.ToFlattenedRoster());
                MobileParty.MainParty.PrisonRoster.Clear();
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't find anymore prisoners.");
            }

            if (!releasedPrisoners.GetTroopRoster().IsEmpty())
            {
                try
                {
                    //SpawnAPartyInFaction
                    TroopRosterElement leader = releasedPrisoners.GetTroopRoster().FirstOrDefault(hasHero => hasHero.Character.IsHero);

                    Clan clan = null;
                    Settlement nearest = null;
                    MobileParty prisonerParty = null;

                    if (leader.Character != null)
                    {
                        clan = leader.Character.HeroObject.Clan;
                        nearest = SettlementHelper.FindNearestSettlement(settlement => settlement.OwnerClan == clan) ?? SettlementHelper.FindNearestSettlement(settlement => true);
                        prisonerParty = LordPartyComponent.CreateLordParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), leader.Character.HeroObject, MobileParty.MainParty.Position2D, 0.5f, nearest, leader.Character.HeroObject);
                    }
                    else
                    {
                        clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                        clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);
                        nearest = SettlementHelper.FindNearestSettlement(settlement => true);
                        prisonerParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false);
                    }

                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                    prisonerParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, -1);

                    prisonerParty.SetCustomName(new TextObject("{=CEEVENTS1107}Escaped Captives"));

                    prisonerParty.MemberRoster.Clear();
                    prisonerParty.ActualClan = clan;
                    prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());
                    prisonerParty.IsActive = true;

                    prisonerParty.Ai.SetMovePatrolAroundPoint(nearest.IsTown
                                       ? nearest.GatePosition
                                       : nearest.Position2D);

                    if (leader.Character != null)
                    {
                        prisonerParty.Party.SetCustomOwner(leader.Character.HeroObject);
                        prisonerParty.ChangePartyLeader(leader.Character.HeroObject);
                    }
                    else
                    {
                        prisonerParty.Party.SetCustomOwner(clan.Leader);
                    }

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.Aggressiveness = 0.2f;
                    prisonerParty.InitializePartyTrade(0);
                    prisonerParty.SetCustomHomeSettlement(nearest);

                    Hero.MainHero.HitPoints += 40;

                    CECustomHandler.LogToFile(prisonerParty.Name.ToString());

                    PlayerEncounter.RestartPlayerEncounter(MobileParty.MainParty.Party, prisonerParty.Party);
                    GameMenu.SwitchToMenu("encounter");
                }
                catch (Exception)
                {
                    CECaptorContinue(args);
                }
            }
            else
            {
                CECaptorContinue(args);
            }
        }

        internal void CECaptorHuntPrisoners(MenuCallbackArgs args, int amount = 20)
        {
            CEPersistence.animationPlayEvent = false;

            TroopRoster releasedPrisoners = TroopRoster.CreateDummyTroopRoster();

            amount = CESettings.Instance?.AmountOfTroopsForHunt ?? 15;

            try
            {
                for (int i = 0; i < amount; i++)
                {
                    TroopRosterElement test = MobileParty.MainParty.PrisonRoster.GetTroopRoster().Where(troop => !troop.Character.IsHero).GetRandomElementInefficiently();

                    if (test.Character == null) continue;

                    MobileParty.MainParty.PrisonRoster.RemoveTroop(test.Character);
                    releasedPrisoners.AddToCounts(test.Character, 1, true);
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't find anymore prisoners.");
            }

            if (!releasedPrisoners.GetTroopRoster().IsEmpty())
            {
                CECaptorContinue(args);

                try
                {
                    //SpawnAPartyInFaction CreateRaiderParty
                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                    clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                    Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

                    MobileParty prisonerParty = BanditPartyComponent.CreateLooterParty("CustomPartyCE_Hunt_" + MBRandom.RandomInt(int.MaxValue), clan, nearest, false);

                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                    prisonerParty.InitializeMobilePartyAroundPosition(defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, -1);

                    prisonerParty.SetCustomName(new TextObject("{=CEEVENTS1107}Escaped Captives"));

                    prisonerParty.MemberRoster.Clear();
                    prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.IsActive = true;
                    prisonerParty.ActualClan = clan;

                    prisonerParty.Party.SetCustomOwner(clan.Leader);
#if V120
                    prisonerParty.Party.SetVisualAsDirty();
#else
                    prisonerParty.Party.Visuals.SetMapIconAsDirty();
#endif
                    prisonerParty.InitializePartyTrade(0);
                    prisonerParty.SetCustomHomeSettlement(nearest);

                    Hero.MainHero.HitPoints += 40;

                    CECustomHandler.LogToFile(prisonerParty.Name.ToString());

                    PlayerEncounter.RestartPlayerEncounter(prisonerParty.Party, MobileParty.MainParty.Party, true);
                    StartBattleAction.Apply(MobileParty.MainParty.Party, prisonerParty.Party);
                    PlayerEncounter.Update();

                    CEPersistence.huntState = CEPersistence.HuntState.StartHunt;

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
#if V120
                        AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.GetLogicalPosition())
#else
                        AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(CampaignTime.Now, MobileParty.MainParty.GetLogicalPosition())
#endif
                    };
                    float timeOfDay = Campaign.CurrentTime % 24f;
                    if (Campaign.Current != null)
                    {
                        rec.TimeOfDay = timeOfDay;
                    }
                    CampaignMission.OpenBattleMission(rec);
                }
                catch (Exception)
                {
                    CECaptorKillPrisoners(args, amount);
                }
            }
            else
            {
                CECaptorContinue(args);
            }
        }

        internal void CECaptorMakeHeroCompanion(Hero captive)
        {
            if (captive == null) return;
            if (captive.IsFactionLeader)
            {
                if (captive.Clan != null && captive.Clan.Kingdom != null)
                {
                    Kingdom kingdom = captive.Clan.Kingdom;
                    Clan result = null;
                    float num = 0f;
                    IEnumerable<Clan> clans = kingdom.Clans;
                    foreach (Clan clan in clans.Where((Clan t) => t.Heroes.Any((Hero h) => h.IsAlive) && !t.IsMinorFaction && t != captive.Clan))
                    {
                        float clanStrength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(clan);
                        if (num <= clanStrength)
                        {
                            num = clanStrength;
                            result = clan;
                        }
                    }
                    kingdom.RulingClan = result;
                }
            }
            AddCompanionAction.Apply(Clan.PlayerClan, captive);
        }

        internal void CECaptorStripVictim(Hero captive, StripSettings stripSettings = null)
        {
            if (captive == null) return;

            try
            {
                string clothingLevel = "default";
                string mountLevel = "none";
                string meleeLevel = "none";
                string rangedLevel = "none";

                string customBody = "";
                string customCape = "";
                string customGloves = "";
                string customLegs = "";
                string customHead = "";

                if (stripSettings != null)
                {
                    clothingLevel = string.IsNullOrWhiteSpace(stripSettings.Clothing) ? "default" : stripSettings.Clothing.ToLower();
                    mountLevel = string.IsNullOrWhiteSpace(stripSettings.Mount) ? "none" : stripSettings.Mount.ToLower();
                    meleeLevel = string.IsNullOrWhiteSpace(stripSettings.Melee) ? "none" : stripSettings.Melee.ToLower();
                    rangedLevel = string.IsNullOrWhiteSpace(stripSettings.Ranged) ? "none" : stripSettings.Ranged.ToLower();

                    customBody = string.IsNullOrWhiteSpace(stripSettings.CustomBody) ? "" : stripSettings.CustomBody;
                    customCape = string.IsNullOrWhiteSpace(stripSettings.CustomCape) ? "" : stripSettings.CustomCape;
                    customGloves = string.IsNullOrWhiteSpace(stripSettings.CustomGloves) ? "" : stripSettings.CustomGloves;
                    customLegs = string.IsNullOrWhiteSpace(stripSettings.CustomLegs) ? "" : stripSettings.CustomLegs;
                    customHead = string.IsNullOrWhiteSpace(stripSettings.CustomHead) ? "" : stripSettings.CustomHead;
                }

                if (CESettingsIntegrations.Instance == null && clothingLevel == "slave" || !CESettingsIntegrations.Instance.ActivateKLBShackles && clothingLevel == "slave") return;

                Equipment randomElement = new(false);

                if (clothingLevel != "nude")
                {
                    if (clothingLevel == "advanced")
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
                        MBEquipmentRoster tryRoster = MBObjectManager.Instance.GetObject<MBEquipmentRoster>(clothingLevel);
                        if (tryRoster != null) {
                            Equipment tryEquipSet = tryRoster.AllEquipments.GetRandomElementWithPredicate(e => e.IsCivilian != true);
                                randomElement.FillFrom(tryEquipSet);
                        }
                        else
                        {
                            ItemObject itemObjectBody = Hero.MainHero.IsFemale
                                ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress")
                                : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                            randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                        }
                    }
                }

                if (meleeLevel != "none")
                {
                    string item;

                    if (meleeLevel == "Advanced")
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
                    if (rangedLevel == "advanced")
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

                if (mountLevel == "basic")
                {
                    ItemObject poorHorse = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                    EquipmentElement horseEquipment = new(poorHorse);

                    randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Horse, horseEquipment);
                }

                if (CESettings.Instance?.EventCaptorGearCaptives ?? true) CECampaignBehavior.AddReturnEquipment(captive, captive.BattleEquipment, captive.CivilianEquipment);

                foreach (EquipmentCustomIndex index in Enum.GetValues(typeof(EquipmentCustomIndex)))
                {
                    EquipmentIndex i = (EquipmentIndex)index;

                    try
                    {
                        if (!captive.BattleEquipment.GetEquipmentFromSlot(i).IsEmpty) PartyBase.MainParty.ItemRoster.AddToCounts(captive.BattleEquipment.GetEquipmentFromSlot(i).Item, 1);
                    }
                    catch (Exception) { }

                    try
                    {
                        if (!captive.CivilianEquipment.GetEquipmentFromSlot(i).IsEmpty) PartyBase.MainParty.ItemRoster.AddToCounts(captive.CivilianEquipment.GetEquipmentFromSlot(i).Item, 1);
                    }
                    catch (Exception) { }
                }

                EquipmentHelper.AssignHeroEquipmentFromEquipment(captive, randomElement);
                EquipmentHelper.AssignHeroEquipmentFromEquipment(captive, randomElement2);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("CECaptorStripVictim: " + e.ToString());
            }
        }
    }
}
