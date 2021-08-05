#define BETA
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
                    string waitingList = new WaitingList().CEWaitingList();

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
                MobileParty.MainParty.PrisonRoster.KillNumberOfMenRandomly(amount, false);
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
                int prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                MobileParty.MainParty.PrisonRoster.KillNumberOfMenRandomly(amount, killHeroes);
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

                    TroopRosterElement leader = releasedPrisoners.GetTroopRoster().FirstOrDefault(hasHero => hasHero.Character.IsHero);

                    Clan clan = null;
                    Settlement nearest = null;
                    MobileParty prisonerParty = null;

                    if (leader.Character != null)
                    {
                        clan = leader.Character.HeroObject.Clan;
                        nearest = SettlementHelper.FindNearestSettlement(settlement => settlement.OwnerClan == clan) ?? SettlementHelper.FindNearestSettlement(settlement => true);
                        prisonerParty = LordPartyComponent.CreateLordParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), leader.Character.HeroObject, MobileParty.MainParty.Position2D, 0.5f, nearest);
                    }
                    else
                    {
                        clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                        clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);
                        nearest = SettlementHelper.FindNearestSettlement(settlement => true);
                        prisonerParty = BanditPartyComponent.CreateBanditParty("CustomPartyCE_" + MBRandom.RandomInt(int.MaxValue), clan, nearest.Hideout, false);
                    }

                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                    prisonerParty.InitializeMobileParty(defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, -1);
                    prisonerParty.SetCustomName(new TextObject("{=CEEVENTS1107}Escaped Captives"));

                    prisonerParty.MemberRoster.Clear();
                    prisonerParty.ActualClan = clan;
                    prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());
                    prisonerParty.IsActive = true;

#if BETA
                    prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown
                                       ? nearest.GatePosition
                                       : nearest.Position2D);

                    if (leader.Character != null)
                    {
                        prisonerParty.Party.SetCustomOwner(leader.Character.HeroObject);
                        prisonerParty.ChangePartyLeader(leader.Character);
                    } else
                    {
                        prisonerParty.Party.SetCustomOwner(clan.Leader);
                    }
#else
                    prisonerParty.HomeSettlement = nearest;
                    prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown
                                       ? nearest.GatePosition
                                       : nearest.Position2D);

                    if (leader.Character != null)
                    {
                        prisonerParty.Party.Owner = leader.Character.HeroObject;
                        prisonerParty.ChangePartyLeader(leader.Character);
                    }
                    else
                    {
                        prisonerParty.Party.Owner = clan.Leader;
                    }
#endif

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.Aggressiveness = 0.2f;
                    prisonerParty.InitializePartyTrade(0);

                    Hero.MainHero.HitPoints += 40;

                    CECustomHandler.LogToFile(prisonerParty.Leader.Name.ToString());
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

            if (CESettings.Instance != null) amount = CESettings.Instance.AmountOfTroopsForHunt;

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
                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                    clan.Banner.SetBannerVisual(Banner.CreateRandomBanner().BannerVisual);

                    Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

                    MobileParty prisonerParty = BanditPartyComponent.CreateBanditParty("CustomPartyCE_Hunt_" + MBRandom.RandomInt(int.MaxValue), clan, nearest.Hideout, false);

                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;

                    prisonerParty.InitializeMobileParty(defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, -1);
                    prisonerParty.SetCustomName(new TextObject("{=CEEVENTS1107}Escaped Captives"));

                    prisonerParty.MemberRoster.Clear();
                    prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.IsActive = true;
                    prisonerParty.ActualClan = clan;

#if BETA
                    prisonerParty.Party.SetCustomOwner(clan.Leader);
                    prisonerParty.Party.Visuals.SetMapIconAsDirty();
                    prisonerParty.InitializePartyTrade(0);
#else
                    prisonerParty.Party.Owner = clan.Leader;
                    prisonerParty.Party.Visuals.SetMapIconAsDirty();

                    prisonerParty.HomeSettlement = nearest;
                    prisonerParty.InitializePartyTrade(0);
#endif

                    Hero.MainHero.HitPoints += 40;


                    CECustomHandler.LogToFile(prisonerParty.Leader.Name.ToString());
                    PlayerEncounter.RestartPlayerEncounter(prisonerParty.Party, MobileParty.MainParty.Party, true);
                    StartBattleAction.Apply(MobileParty.MainParty.Party, prisonerParty.Party);
                    PlayerEncounter.Update();

                    CEPersistence.huntState = CEPersistence.HuntState.StartHunt;
                    CampaignMission.OpenBattleMission(PlayerEncounter.GetBattleSceneForMapPosition(MobileParty.MainParty.Position2D));
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
                if (captive.Clan != null && captive.Clan.IsKingdomFaction)
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
        internal void CECaptorStripVictim(Hero captive)
        {
            if (captive == null) return;
            Equipment randomElement = new Equipment(false);

            ItemObject itemObjectBody = captive.IsFemale
                ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress")
                : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
            randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
            Equipment randomElement2 = new Equipment(true);
            randomElement2.FillFrom(randomElement, false);

            if (CESettings.Instance != null && CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(captive, captive.BattleEquipment, captive.CivilianEquipment);



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
    }
}