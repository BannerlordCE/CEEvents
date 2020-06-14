using CaptivityEvents.CampaignBehaviours;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Issues;
using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Models
{
    public class CEPlayerCaptivityModel : DefaultPlayerCaptivityModel
    {
        private bool capturedEvent = false;
        public static bool captureOverride = false;

        private bool CheckTimeElapsedMoreThanHours(CampaignTime eventBeginTime, float hoursToWait)
        {
            float elapsedHoursUntilNow = eventBeginTime.ElapsedHoursUntilNow;
            float randomNumber = PlayerCaptivity.RandomNumber;
            return hoursToWait * (0.5 + randomNumber) < elapsedHoursUntilNow;
        }

        private bool CheckEvent()
        {
            if (PlayerCaptivity.CaptorParty != null)
            {
                float gameProcess = MiscHelper.GetGameProcess();
                float num = (1f + gameProcess * 1f) * (PlayerCaptivity.CaptorParty.IsSettlement ? CESettings.Instance.EventOccuranceSettlement : (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.Leader != null && PlayerCaptivity.CaptorParty.Leader.IsHero) ? CESettings.Instance.EventOccuranceLord : CESettings.Instance.EventOccuranceOther);
                return CheckTimeElapsedMoreThanHours(PlayerCaptivity.LastCheckTime, num);
            }
            else
            {
                return false;
            }
        }

        private bool StripEvent()
        {
            if (Hero.MainHero.CaptivityStartTime.IsNow && !capturedEvent)
            {
                capturedEvent = true;
            }

            if (capturedEvent && Hero.MainHero.CaptivityStartTime.IsPast)
            {
                capturedEvent = false;
                if (MobileParty.MainParty.IsActive)
                {
                    MobileParty.MainParty.IsActive = false;
                    PartyBase.MainParty.UpdateVisibilityAndInspected(true);
                    PlayerCaptivity.CaptorParty.SetAsCameraFollowParty();
                }
                if (captureOverride == true)
                {
                    captureOverride = false;
                    return false;
                }

                if (CESettings.Instance.StolenGear)
                {
                    Equipment randomElement = new Equipment(false);
                    if ((MBRandom.Random.Next(100) < CESettings.Instance.BetterOutFitChance))
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
                                capeString = Hero.MainHero.IsFemale ? "female_hood" : "";
                                bodyString = Hero.MainHero.IsFemale ? "cut_dress" : "heavy_nordic_tunic";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                glovesString = "armwraps";
                                break;

                            case CultureCode.Aserai:
                                headString = Hero.MainHero.IsFemale ? "" : "turban";
                                bodyString = Hero.MainHero.IsFemale ? "aserai_villager_female_dress" : "aserai_tunic_waistcoat";
                                legString = Hero.MainHero.IsFemale ? "southern_moccasins" : "wrapped_shoes";
                                capeString = "wrapped_scarf";
                                glovesString = "armwraps";
                                break;

                            case CultureCode.Khuzait:
                                headString = "fur_hat";
                                capeString = "wrapped_scarf";
                                bodyString = Hero.MainHero.IsFemale ? "khuzait_dress" : "steppe_armor";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                glovesString = "armwraps";
                                break;

                            case CultureCode.Empire:
                                headString = Hero.MainHero.IsFemale ? "female_head_wrap" : "arming_cap";
                                bodyString = Hero.MainHero.IsFemale ? "vlandian_corset_dress" : "padded_leather_shirt";
                                legString = Hero.MainHero.IsFemale ? "ladys_shoe" : "rough_tied_boots";
                                capeString = "wrapped_scarf";
                                glovesString = "armwraps";
                                break;

                            case CultureCode.Battania:
                                headString = Hero.MainHero.IsFemale ? "female_head_wrap" : "wrapped_headcloth";
                                capeString = Hero.MainHero.IsFemale ? "wrapped_scarf" : "battania_shoulder_strap";
                                glovesString = "armwraps";
                                bodyString = Hero.MainHero.IsFemale ? "battania_dress_c" : "burlap_waistcoat";
                                legString = "ragged_boots";
                                break;

                            case CultureCode.Vlandia:
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
                        ItemObject itemObjectBody = Hero.MainHero.IsFemale ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress") : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
                    }
                    if (MBRandom.Random.Next(100) < CESettings.Instance.WeaponChance)
                    {
                        string Item;
                        if (MBRandom.Random.Next(100) < (CESettings.Instance.WeaponSkill ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.TwoHanded) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) / 275 * 100)) : CESettings.Instance.WeaponChance))
                        {
                            switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                            {
                                case CultureCode.Sturgia:
                                    Item = "sturgia_axe_3_t3";
                                    break;

                                case CultureCode.Aserai:
                                    Item = "eastern_spear_1_t2";
                                    break;

                                case CultureCode.Empire:
                                    Item = "northern_spear_1_t2";
                                    break;

                                case CultureCode.Battania:
                                    Item = "aserai_sword_1_t2";
                                    break;

                                default:
                                    Item = "vlandia_sword_1_t2";
                                    break;
                            }
                        }
                        else
                        {
                            switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                            {
                                case CultureCode.Sturgia:
                                    Item = "seax";
                                    break;

                                case CultureCode.Aserai:
                                    Item = "celtic_dagger";
                                    break;

                                case CultureCode.Empire:
                                    Item = "gladius_b";
                                    break;

                                case CultureCode.Battania:
                                    Item = "hooked_cleaver";
                                    break;

                                default:
                                    Item = "seax";
                                    break;
                            }
                        }
                        ItemObject itemObjectWeapon0 = MBObjectManager.Instance.GetObject<ItemObject>(Item);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon0, new EquipmentElement(itemObjectWeapon0));
                    }

                    if (MBRandom.Random.Next(100) < CESettings.Instance.WeaponChance && MBRandom.Random.Next(100) < (CESettings.Instance.RangedSkill ? Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Bow) / 275 * 100, Math.Max(Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow) / 275 * 100, Hero.MainHero.GetSkillValue(DefaultSkills.Throwing) / 275 * 100)) : CESettings.Instance.RangedBetterChance))
                    {
                        string RangedItem;
                        string RangedAmmo = null;

                        switch (PlayerCaptivity.CaptorParty.Culture.GetCultureCode())
                        {
                            case CultureCode.Sturgia:
                                RangedItem = "nordic_shortbow";
                                RangedAmmo = "default_arrows";
                                break;

                            case CultureCode.Vlandia:
                                RangedItem = "crossbow_a";
                                RangedAmmo = "tournament_bolts";
                                break;

                            case CultureCode.Aserai:
                                RangedItem = "tribal_bow";
                                RangedAmmo = "default_arrows";
                                break;

                            case CultureCode.Empire:
                                RangedItem = "hunting_bow";
                                RangedAmmo = "default_arrows";
                                break;

                            case CultureCode.Battania:
                                RangedItem = "northern_javelin_2_t3";
                                break;

                            default:
                                RangedItem = "hunting_bow";
                                RangedAmmo = "default_arrows";
                                break;
                        }

                        ItemObject itemObjectWeapon2 = MBObjectManager.Instance.GetObject<ItemObject>(RangedItem);
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon2, new EquipmentElement(itemObjectWeapon2));

                        if (RangedAmmo != null)
                        {
                            ItemObject itemObjectWeapon3 = MBObjectManager.Instance.GetObject<ItemObject>(RangedAmmo);
                            randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon3, new EquipmentElement(itemObjectWeapon3));
                        }
                    }
                    else
                    {
                        ItemObject itemObjectWeapon2 = MBObjectManager.Instance.GetObject<ItemObject>("throwing_stone");
                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Weapon2, new EquipmentElement(itemObjectWeapon2));
                    }

                    Equipment randomElement2 = new Equipment(true);
                    randomElement2.FillFrom(randomElement, false);

                    if (MBRandom.Random.Next(100) < ((CESettings.Instance.HorseSkill) ? Hero.MainHero.GetSkillValue(DefaultSkills.Riding) / 275 * 100 : CESettings.Instance.HorseChance))
                    {
                        ItemObject poorhorse = MBObjectManager.Instance.GetObject<ItemObject>("sumpter_horse");
                        EquipmentElement horseEquipment = new EquipmentElement(poorhorse);

                        randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Horse, horseEquipment);
                    }

                    if (CESettings.Instance.StolenGearQuest && MBRandom.Random.Next(100) < CESettings.Instance.StolenGearChance)
                    {
                        Hero issueOwner = null;
                        List<TextObject> listOfSettlements = new List<TextObject>();
                        while (issueOwner == null)
                        {
                            Settlement nearestSettlement = SettlementHelper.FindNearestSettlement((Settlement settlement) => { return !listOfSettlements.Contains(settlement.Name); });
                            listOfSettlements.Add(nearestSettlement.Name);

                            if (!nearestSettlement.IsUnderRaid && !nearestSettlement.IsRaided)
                            {
                                foreach (Hero hero in nearestSettlement.Notables)
                                {
                                    if (hero.Issue == null && !hero.IsOccupiedByAnEvent())
                                    {
                                        issueOwner = hero;
                                        break;
                                    }
                                }
                                if (issueOwner != null)
                                {
                                    PotentialIssueData potentialIssueData = new PotentialIssueData(new Func<PotentialIssueData, Hero, IssueBase>(CEWhereAreMyThingsIssueBehavior.OnStartIssue), typeof(CEWhereAreMyThingsIssueBehavior.CEWhereAreMyThingsIssue), 0.25f);

                                    Campaign.Current.IssueManager.CreateNewIssue(potentialIssueData, issueOwner);
                                    Campaign.Current.IssueManager.StartIssueQuest(issueOwner);
                                }
                            }
                        }
                    }

                    EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, randomElement);
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(Hero.MainHero, randomElement2);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Custom CheckCaptivityChange Function
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>EventName</returns>
        public override string CheckCaptivityChange(float dt)
        {
            if (PlayerCaptivity.IsCaptive)
            {
                if (Hero.MainHero.Age < 18f)
                {
                    EndCaptivityAction.ApplyByReleasing(Hero.MainHero, null);
                    return "menu_captivity_end_by_party_removed";
                }
                else if (PlayerCaptivity.CaptorParty != null && !PlayerCaptivity.CaptorParty.IsSettlement)
                {
                    if (StripEvent())
                    {
                        CECustomHandler.LogToFile(Hero.MainHero.Name.ToString() + " is captive.");
                        if (CESettings.Instance.SexualContent)
                        {
                            return Hero.MainHero.IsFemale ? "CE_advanced_sexual_capture" : "CE_advanced_sexual_capture_male";
                        }
                        else
                        {
                            return Hero.MainHero.IsFemale ? "CE_advanced_capture" : "CE_advanced_capture_male";
                        }
                    }

                    if (CheckEvent())
                    {
                        PlayerCaptivity.LastCheckTime = CampaignTime.Now;

                        CECustomHandler.LogToFile("About to choose a event!");
                        CEEvent captiveEvent = CEEventManager.ReturnWeightedChoiceOfEvents();
                        if (captiveEvent != null)
                        {
                            return captiveEvent.Name;
                        }
                    }
                }
                else
                {
                    if (StripEvent())
                    {
                        CECustomHandler.LogToFile(Hero.MainHero.Name.ToString() + " is captive in Settlement.");
                        if (CESettings.Instance.SexualContent)
                        {
                            return Hero.MainHero.IsFemale ? "CE_advanced_sexual_capture_settlement" : "CE_advanced_sexual_capture_settlement_male";
                        }
                        else
                        {
                            return Hero.MainHero.IsFemale ? "CE_advanced_capture_settlement" : "CE_advanced_capture_settlement_male";
                        }
                    }

                    if (CheckEvent())
                    {
                        PlayerCaptivity.LastCheckTime = CampaignTime.Now;

                        CECustomHandler.LogToFile("About to choose a settlement event!");
                        //PrintDebugInGameTextMessage("About to choose a settlement event!");
                        CEEvent captiveEvent = CEEventManager.ReturnWeightedChoiceOfEvents();
                        if (captiveEvent != null)
                        {
                            return captiveEvent.Name;
                        }
                    }
                }
            }

            return DefaultOverridenCheckCaptivityChange(dt);
        }

        /// <summary>
        /// Modified Default
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private string DefaultOverridenCheckCaptivityChange(float dt)
        {
            if (PlayerCaptivity.CaptorParty.IsMobile && !PlayerCaptivity.CaptorParty.MobileParty.IsActive)
            {
                CECampaignBehavior.extraVariables.Owner = null;
                CEEventLoader.VictimSlaveryModifier(0, Hero.MainHero, true, true);
                return "menu_captivity_end_by_party_removed";
            }
            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MapFaction == Hero.MainHero.Clan)
            {
                CECampaignBehavior.extraVariables.Owner = null;
                CEEventLoader.VictimSlaveryModifier(0, Hero.MainHero, true, true);
                return "menu_captivity_end_by_ally_party_saved";
            }
            if (!CESettings.Instance.SlaveryToggle && !FactionManager.IsAtWarAgainstFaction(PlayerCaptivity.CaptorParty.MapFaction, MobileParty.MainParty.MapFaction) && (PlayerCaptivity.CaptorParty.MapFaction == MobileParty.MainParty.MapFaction || (!Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingModerate(PlayerCaptivity.CaptorParty.MapFaction) && !Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(PlayerCaptivity.CaptorParty.MapFaction))))
            {
                return "menu_captivity_end_no_more_enemies";
            }
            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
            {
                // Default event transfer disabled if slavery is enabled override or if it is garrison
                if (PlayerCaptivity.CaptorParty.MapFaction == PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement.MapFaction &&
                    (!CESettings.Instance.SlaveryToggle || (PlayerCaptivity.CaptorParty.MobileParty.IsGarrison || PlayerCaptivity.CaptorParty.MobileParty.IsMilitia)))
                {
                    PlayerCaptivity.LastCheckTime = CampaignTime.Now;
                    if (Game.Current.GameStateManager.ActiveState is MapState)
                    {
                        Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                    }
                    return "menu_captivity_transfer_to_town";
                }
            }
            else
            {
                if (CheckEvent())
                {
                    PlayerCaptivity.LastCheckTime = CampaignTime.Now;
                    Hero.MainHero.HitPoints += MBRandom.Random.Next(10);
                    if (MBRandom.Random.Next(100) < (Hero.MainHero.GetSkillValue(DefaultSkills.Tactics) / 4 + Hero.MainHero.GetSkillValue(DefaultSkills.Roguery) / 4) / 4)
                    {
                        if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MapEvent != null)
                        {
                            CECampaignBehavior.extraVariables.Owner = null;
                            CEEventLoader.VictimSlaveryModifier(0, Hero.MainHero, true, true);
                            return "menu_escape_captivity_during_battle";
                        }
                    }
                }
            }
            return null;
        }
    }
}