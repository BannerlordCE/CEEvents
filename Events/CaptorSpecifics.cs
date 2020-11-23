using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using HarmonyLib;
using Helpers;
using System;
using System.Linq;
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
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Critical Error: CECaptorContinue : " + e);
            }
        }

        internal void CEReleasePrisoners(MenuCallbackArgs args, int amount = 10, bool releaseHeroes = false)
        {
            try
            {
                int prisonerCount = MobileParty.MainParty.PrisonRoster.Count;
                if (prisonerCount < amount) amount = prisonerCount;
                MobileParty.MainParty.PrisonRoster.KillNumberOfMenRandomly(amount, false);
                if (releaseHeroes)
                {
                    foreach (TroopRosterElement element in MobileParty.MainParty.PrisonRoster)
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

        internal void CEWoundPrisoners(MenuCallbackArgs args, int amount = 10)
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

        internal void CEKillPrisoners(MenuCallbackArgs args, int amount = 10, bool killHeroes = false)
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

        internal void CEPrisonerRebel(MenuCallbackArgs args)
        {
            CEPersistence.animationPlayEvent = false;

            TroopRoster releasedPrisoners = TroopRoster.CreateDummyTroopRoster();

            try
            {
                foreach (TroopRosterElement element in MobileParty.MainParty.PrisonRoster)
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

            if (!releasedPrisoners.IsEmpty())
            {
                try
                {
                    MobileParty prisonerParty = MBObjectManager.Instance.CreateObject<MobileParty>("Escaped_Captives");

                    TroopRosterElement leader = releasedPrisoners.FirstOrDefault(hasHero => hasHero.Character.IsHero);

                    if (leader.Character != null)
                    {
                        Clan clan = leader.Character.HeroObject.Clan;
                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;
                        Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => settlement.OwnerClan == clan) ?? SettlementHelper.FindNearestSettlement(settlement => true);
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
                        Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");
                        PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;
                        Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => true);
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
            }
            else
            {
                CECaptorContinue(args);
            }
        }

        internal void CEHuntPrisoners(MenuCallbackArgs args, int amount = 20)
        {
            CEPersistence.animationPlayEvent = false;

            TroopRoster releasedPrisoners = TroopRoster.CreateDummyTroopRoster();

            if (CESettings.Instance != null) amount = CESettings.Instance.AmountOfTroopsForHunt;

            try
            {
                for (int i = 0; i < amount; i++)
                {
                    TroopRosterElement test = MobileParty.MainParty.PrisonRoster.Where(troop => !troop.Character.IsHero).GetRandomElement();

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
                    MobileParty prisonerParty = MBObjectManager.Instance.CreateObject<MobileParty>("Escaped_Captives");

                    Clan clan = Clan.BanditFactions.First(clanLooters => clanLooters.StringId == "looters");

                    PartyTemplateObject defaultPartyTemplate = clan.DefaultPartyTemplate;
                    Settlement nearest = SettlementHelper.FindNearestSettlement(settlement => { return true; });

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

        internal void CEMakeHeroCompanion(Hero captive)
        {
            if (captive == null) return;
            AddCompanionAction.Apply(Clan.PlayerClan, captive);
        }


        public void CEStripVictim(Hero captive)
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