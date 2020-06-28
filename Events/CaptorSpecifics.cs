using System;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Events
{
    public class CaptorSpecifics
    {
        internal void CECaptorContinue(MenuCallbackArgs args)
        {
            if (CECampaignBehavior.ExtraProps.menuToSwitchBackTo != null)
            {
                GameMenu.SwitchToMenu(CECampaignBehavior.ExtraProps.menuToSwitchBackTo);
                CECampaignBehavior.ExtraProps.menuToSwitchBackTo = null;

                if (CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo != null)
                {
                    args.MenuContext.SetBackgroundMeshName(CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo);
                    CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = null;
                }
            }
            else
            {
                GameMenu.ExitToLast();
            }

            Campaign.Current.TimeControlMode = Campaign.Current.LastTimeControlMode;
            new CESubModule().LoadTexture("default");
        }

        internal void CEKillPrisoners(MenuCallbackArgs args, int amount = 10, bool killHeroes = false)
        {
            try
            {
                var prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                MobileParty.MainParty.PrisonRoster.KillNumberOfMenRandomly(amount, killHeroes);
                var textObject = GameTexts.FindText("str_CE_kill_prisoners");
                textObject.SetTextVariable("HERO", Hero.MainHero.Name);
                textObject.SetTextVariable("AMOUNT", amount);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't kill any prisoners.");
            }
        }

        internal void CEPrisonerRebel(MenuCallbackArgs args)
        {
            var releasedPrisoners = new TroopRoster();

            try
            {
                releasedPrisoners.Add(MobileParty.MainParty.PrisonRoster.ToFlattenedRoster());
                MobileParty.MainParty.PrisonRoster.Clear();
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't find anymore prisoners.");
            }

            if (!releasedPrisoners.IsEmpty())
                try
                {
                    var prisonerParty = MBObjectManager.Instance.CreateObject<MobileParty>("Escaped_Captives");

                    var leader = releasedPrisoners.FirstOrDefault(hasHero => hasHero.Character.IsHero);

                    if (leader.Character != null)
                    {
                        var clan = leader.Character.HeroObject.Clan;
                        var defaultPartyTemplate = clan.DefaultPartyTemplate;
                        var nearest = SettlementHelper.FindNearestSettlement(settlement => settlement.OwnerClan == clan) ?? SettlementHelper.FindNearestSettlement(settlement => true);
                        prisonerParty.InitializeMobileParty(new TextObject("{=CEEVENTS1107}Escaped Captives"), defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, MobileParty.PartyTypeEnum.Lord);
                        prisonerParty.MemberRoster.Clear();
                        prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());
                        prisonerParty.IsActive = true;
                        prisonerParty.Party.Owner = leader.Character.HeroObject;
                        prisonerParty.ChangePartyLeader(leader.Character, true);
                        prisonerParty.HomeSettlement = nearest;

                        prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown
                                                                   ? nearest.GatePosition
                                                                   : nearest.Position2D);
                    }
                    else
                    {
                        var clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                        var defaultPartyTemplate = clan.DefaultPartyTemplate;
                        var nearest = SettlementHelper.FindNearestSettlement(settlement => true);
                        prisonerParty.InitializeMobileParty(new TextObject("{=CEEVENTS1107}Escaped Captives"), defaultPartyTemplate, MobileParty.MainParty.Position2D, 0.5f, 0.1f, MobileParty.PartyTypeEnum.Bandit);
                        prisonerParty.MemberRoster.Clear();
                        prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());
                        prisonerParty.IsActive = true;
                        prisonerParty.Party.Owner = clan.Leader;
                        prisonerParty.HomeSettlement = nearest;

                        prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown
                                                                   ? nearest.GatePosition
                                                                   : nearest.Position2D);
                    }

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.Aggressiveness = 0.2f;
                    prisonerParty.InitializePartyTrade(0);
                    prisonerParty.EnableAi();

                    prisonerParty.Party.Visuals.SetMapIconAsDirty();

                    Hero.MainHero.HitPoints += 40;
                    Campaign.Current.Parties.AddItem(prisonerParty.Party);

                    CECustomHandler.LogToFile(prisonerParty.Leader.Name.ToString());
                    PlayerEncounter.RestartPlayerEncounter(MobileParty.MainParty.Party, prisonerParty.Party);
                    GameMenu.SwitchToMenu("encounter");
                }
                catch (Exception)
                {
                    CECaptorContinue(args);
                }
            else CECaptorContinue(args);
        }

        internal void CEHuntPrisoners(MenuCallbackArgs args, int amount = 20)
        {
            var releasedPrisoners = new TroopRoster();

            if (CESettings.Instance != null) amount = CESettings.Instance.AmountOfTroopsForHunt;

            try
            {
                for (var i = 0; i < amount; i++)
                {
                    var test = MobileParty.MainParty.PrisonRoster.Where(troop => !troop.Character.IsHero).GetRandomElement();

                    if (test.Character == null) continue;

                    MobileParty.MainParty.PrisonRoster.RemoveTroop(test.Character);
                    releasedPrisoners.AddToCounts(test.Character, 1, true);
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Couldn't find anymore prisoners.");
            }

            if (!releasedPrisoners.IsEmpty())
            {
                CECaptorContinue(args);

                try
                {
                    var prisonerParty = MBObjectManager.Instance.CreateObject<MobileParty>("Escaped_Captives");

                    var clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                    
                    var defaultPartyTemplate = clan.DefaultPartyTemplate;
                    var nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

                    prisonerParty.InitializeMobileParty(new TextObject("{=CEEVENTS1107}Escaped Captives"), defaultPartyTemplate, MobileParty.MainParty.Position2D, 0f, 0f, MobileParty.PartyTypeEnum.Bandit);
                    prisonerParty.MemberRoster.Clear();
                    prisonerParty.MemberRoster.Add(releasedPrisoners.ToFlattenedRoster());

                    prisonerParty.RecentEventsMorale = -100;
                    prisonerParty.IsActive = true;
                    prisonerParty.Party.Owner = clan.Leader;
                    prisonerParty.Aggressiveness = 0.2f;

                    prisonerParty.SetMovePatrolAroundPoint(nearest.IsTown
                                                               ? nearest.GatePosition
                                                               : nearest.Position2D);
                    prisonerParty.HomeSettlement = nearest;
                    prisonerParty.InitializePartyTrade(0);
                    prisonerParty.EnableAi();
                    prisonerParty.Party.Visuals.SetMapIconAsDirty();

                    Hero.MainHero.HitPoints += 40;
                    Campaign.Current.Parties.AddItem(prisonerParty.Party);

                    CECustomHandler.LogToFile(prisonerParty.Leader.Name.ToString());
                    StartBattleAction.Apply(MobileParty.MainParty.Party, prisonerParty.Party);
                    PlayerEncounter.RestartPlayerEncounter(prisonerParty.Party, MobileParty.MainParty.Party);
                    PlayerEncounter.Update();

                    CEPersistence.huntState = CEPersistence.HuntState.StartHunt;
                    CampaignMission.OpenBattleMission(PlayerEncounter.GetBattleSceneForMapPosition(MobileParty.MainParty.Position2D));
                }
                catch (Exception)
                {
                    CEKillPrisoners(args, amount);
                }
            }
            else
            {
                CECaptorContinue(args);
            }
        }

        public void CEStripVictim(Hero captive)
        {
            if (captive == null) return;
            var randomElement = new Equipment(false);

            var itemObjectBody = captive.IsFemale
                ? MBObjectManager.Instance.GetObject<ItemObject>("burlap_sack_dress")
                : MBObjectManager.Instance.GetObject<ItemObject>("tattered_rags");
            randomElement.AddEquipmentToSlotWithoutAgent(EquipmentIndex.Body, new EquipmentElement(itemObjectBody));
            var randomElement2 = new Equipment(true);
            randomElement2.FillFrom(randomElement, false);

            if (CESettings.Instance != null && CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(captive, captive.BattleEquipment, captive.CivilianEquipment);

            foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
            {
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