using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelSession
    {
        internal List<CEBrothel> BrothelList;
        internal List<Settlement> SettlementsThatPlayerHasSpy = new List<Settlement>();
        
        internal Settlement OrderedDrinkThisDayInSettlement;
        internal bool OrderedDrinkThisVisit;
        internal bool HasMetWithRansomBroker;
        internal bool HasBoughtTunToParty;
        internal CEBrothelMission Mission { get; set; }

        public CEBrothelSession(CEBrothelMission mission)
        {
            Mission = mission;
            BrothelList = new List<CEBrothel>();
        }

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            Mission.AddDialogs(campaignGameStarter);
        }

        internal void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty) return;

            Mission.Brothel.RemoveAllCharacters();
            Mission.IsBrothelInitialized = false;
        }

        public void DailyTick()
        {
            OrderedDrinkThisDayInSettlement = null;

            try
            {
                foreach (var brothel in from brothel in GetPlayerBrothels()
                                        let town = brothel.Settlement.Town
                                        where !town.IsRebeling
                                        where brothel.IsRunning
                                        select brothel)
                {
                    brothel.ChangeGold(MBRandom.RandomInt(50, 400));

                    if (brothel.Capital >= 0 || Hero.MainHero.Gold >= Math.Abs(brothel.Capital)) continue;

                    if (brothel.Settlement.OwnerClan == Clan.PlayerClan)
                    {
                        var textObject3 = new TextObject("{CEBROTHEL0998}The _Brothel of {SETTLEMENT} has gone bankrupted, and is no longer active.");
                        textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                        brothel.IsRunning = false;
                    }
                    else
                    {
                        var textObject3 = new TextObject("{CEBROTHEL0999}The _Brothel of {SETTLEMENT} has gone bankrupted, and has been requisitioned.");
                        textObject3.SetTextVariable("SETTLEMENT", brothel.Settlement.Name);
                        InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                        Mission.Owner.BrothelInteraction(brothel.Settlement, false);
                    }
                }
            }
            catch (Exception) { }
        }

        public void WeeklyTick()
        {
            var prostitutionSkill = CESkills.Prostitution;
            HasBoughtTunToParty = false;

            if (Hero.MainHero.GetSkillValue(prostitutionSkill) > 500) CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(-300, -200), Hero.MainHero, false, false);
            else if (Hero.MainHero.GetSkillValue(prostitutionSkill) > 100) CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(-40, -10), Hero.MainHero, false, false);
        }

        public List<CEBrothel> GetPlayerBrothels()
        {
            try
            {
                return BrothelList.FindAll(brothelData => brothelData.Owner == Hero.MainHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogMessage("Failed to get player owned brothels.");

                return new List<CEBrothel>();
            }
        }

        public void AddBrothelData(Settlement settlement)
        {
            BrothelList.Add(new CEBrothel(settlement));
        }

        public bool ContainsBrothelData(Settlement settlement)
        {
            try
            {
                var data = BrothelList.FirstOrDefault(brothelData => brothelData.Settlement.StringId == settlement.StringId);
                if (data == null) AddBrothelData(settlement);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<CharacterObject> FetchBrothelPrisoners(Settlement settlement)
        {
            var testLocation = BrothelList.FirstOrDefault(brothel => brothel.Settlement.StringId == settlement.StringId);

            return testLocation?.CaptiveProstitutes;
        }

        public void RemovePrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null) return;

                if (!ContainsBrothelData(settlement)) return;

                BrothelList.Where(brothel => brothel.Settlement.StringId == settlement.StringId).Select(brothel =>
                                                                                                        {
                                                                                                            brothel.CaptiveProstitutes.Remove(prisoner);

                                                                                                            return brothel;
                                                                                                        }).ToList();
                MobileParty.MainParty.PrisonRoster.AddToCounts(prisoner, 1, prisoner.IsHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogMessage("Failed to RemovePrisoner");
            }
        }

        public void AddPrisoner(Settlement settlement, CharacterObject prisoner)
        {
            try
            {
                if (settlement == null) return;

                if (!ContainsBrothelData(settlement)) return;

                BrothelList.Where(brothel => brothel.Settlement.StringId == settlement.StringId).Select(brothel =>
                                                                                                        {
                                                                                                            brothel.CaptiveProstitutes.Add(prisoner);

                                                                                                            return brothel;
                                                                                                        }).ToList();
                MobileParty.MainParty.PrisonRoster.RemoveTroop(prisoner);
            }
            catch (Exception)
            {
                CECustomHandler.LogMessage("Failed to AddPrisoner");
            }
        }

        public void CleanList()
        {
            BrothelList = new List<CEBrothel>();
        }
    }
}