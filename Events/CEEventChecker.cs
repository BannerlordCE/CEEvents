using System;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Events
{
    [Serializable]
    public class CESkippingEventException : Exception
    {
        public CESkippingEventException() { }

        public CESkippingEventException(string message) : base(message) { }

        public CESkippingEventException(string message, Exception inner) : base(message, inner) { }
    }


    internal class CEEventChecker
    {
        private readonly CEEvent _listEvent;

        public CEEventChecker(CEEvent listEvent)
        {
            _listEvent = listEvent;
        }

        public static string CheckFlags(CharacterObject captive, PartyBase captorParty = null)
        {
            var returnString = "";

            if (captorParty == null) captorParty = PartyBase.MainParty;

            returnString = returnString
                           + "Captive Gender: "
                           + (captive.IsFemale
                               ? "Female"
                               : "Male")
                           + "\n";

            var slaveSkillFlag = captive.GetSkillValue(CESkills.IsSlave);

            returnString = returnString
                           + "Captive is Slave: "
                           + (slaveSkillFlag != 0
                               ? "True"
                               : "False")
                           + "\n";

            var prostituteSkillFlag = captive.GetSkillValue(CESkills.IsProstitute);

            returnString = returnString
                           + "Captive is Prostitute: "
                           + (prostituteSkillFlag != 0
                               ? "True"
                               : "False")
                           + "\n";

            returnString += "Location : ";

            if (captorParty != null && captorParty.IsSettlement)
            {
                if (captorParty.Settlement.IsTown)
                {
                    returnString += "(hasDungeonFlag || hasCityFlag)";

                    try
                    {
                        var hasCaravan = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null;
                        if (hasCaravan) returnString += "(visitedByCaravanFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Caravan");
                    }

                    try
                    {
                        var hasLord = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null;
                        if (hasLord) returnString += "(VisitedByLordFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.Settlement.IsVillage) returnString += "(hasVillageFlag)";

                if (captorParty.Settlement.IsHideout()) returnString += "(hasHideoutFlag)";

                if (captorParty.Settlement.IsCastle)
                {
                    returnString += "(hasCastleFlag)";

                    try
                    {
                        var hasLord = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null;
                        if (hasLord) returnString += "(VisitedByLordFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.Settlement.IsUnderSiege) returnString += "(duringSiegeFlag)";

                if (captorParty.Settlement.IsUnderRaid) returnString += "(duringRaidFlag)";
            }
            else if (captorParty != null && captorParty.IsMobile && captorParty.MobileParty.CurrentSettlement != null)
            {
                if (captorParty.MobileParty.CurrentSettlement.IsTown)
                {
                    returnString += "(hasPartyInTownFlag)";

                    try
                    {
                        var hasCaravan = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null;
                        if (hasCaravan) returnString += "(visitedByCaravanFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Caravan");
                    }

                    try
                    {
                        var hasLord = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null;
                        if (hasLord) returnString += "(VisitedByLordFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.MobileParty.CurrentSettlement.IsVillage) returnString += "(hasVillageFlag)";

                if (captorParty.MobileParty.CurrentSettlement.IsCastle)
                {
                    returnString += "(hasCastleFlag)";

                    try
                    {
                        var hasLord = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null;
                        if (hasLord) returnString += "(VisitedByLordFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.MobileParty.CurrentSettlement.IsHideout()) returnString += "(hasHideoutFlag)";

                if (captorParty.MobileParty.CurrentSettlement.IsUnderSiege) returnString += "(duringSiegeFlag)";

                if (captorParty.MobileParty.CurrentSettlement.IsUnderRaid) returnString += "(duringRaidFlag)";
            }
            else if (captorParty != null && captorParty.IsMobile)
            {
                returnString += "(hasTravellingFlag)";
                if (captorParty.MobileParty.BesiegerCamp != null) returnString += "(duringSiegeFlag)";

                if (captorParty.MapEvent != null && captorParty.MapEvent.IsRaid && captorParty.MapFaction.IsAtWarWith(captorParty.MapEvent.MapEventSettlement.MapFaction) && captorParty.MapEvent.DefenderSide.TroopCount == 0) returnString += "(duringRaidFlag)";
            }

            returnString += "\nWork in progress\n";

            return returnString;
        }

        public string FlagsDoMatchEventConditions(CharacterObject captive, PartyBase captorParty = null)
        {
            var nonRandomBehaviour = true;

            if (captorParty == null)
            {
                nonRandomBehaviour = false;
                captorParty = PartyBase.MainParty;
            }

            try
            {
                ValidateEvent();
                SettingsCheck();
                GenderCheck(captive);
                SlaveryCheck(captive);
                ProstitutionCheck(captive);
                AgeCheck(captive);
                TraitCheck(captive);
                SkillCheck(captive);
                HealthCheck(captive);
                HeroCheck(captive, captorParty, nonRandomBehaviour);
                PlayerCheck();
                IsOwnedByNotableCheck();
                CaptorCheck(captorParty);
                CaptivesOutNumberCheck(captorParty);
                TroopsCheck(captorParty);
                MaleTroopsCheck(captorParty);
                FemaleTroopsCheck(captorParty);
                CaptiveCheck(captorParty);
                MaleCaptivesCheck(captorParty);
                FemaleCaptivesCheck(captorParty);
                MoraleCheck(captorParty);

                if (nonRandomBehaviour)
                {
                    CaptorTraitCheck(captorParty);
                    CaptorSkillCheck(captorParty);
                    CaptorItemCheck(captorParty);
                    CaptorPartyGenderCheck(captorParty);
                }

                LocationAndEventCheck(captorParty, out var eventMatchingCondition);
                TimeCheck(ref eventMatchingCondition);
                SeasonCheck(eventMatchingCondition);
            }
            catch (CESkippingEventException e)
            {
                return e.Message;
            }

            _listEvent.Captive = captive;

            return null;
        }


        #region private

        private void SeasonCheck(bool eventMatchingCondition)
        {
            var hasWinterFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonWinter);
            var hasSummerFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSpring);
            var hasSpringFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSummer);
            var hasFallFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonFall);

            if (hasWinterFlag || hasSummerFlag) eventMatchingCondition = hasSummerFlag && CampaignTime.Now.GetSeasonOfYear == 1 || hasFallFlag && CampaignTime.Now.GetSeasonOfYear == 2 || hasWinterFlag && CampaignTime.Now.GetSeasonOfYear == 3 || hasSpringFlag && (CampaignTime.Now.GetSeasonOfYear == 4 || CampaignTime.Now.GetSeasonOfYear == 0);

            if (!eventMatchingCondition) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the seasons conditions.");
        }

        private void TimeCheck(ref bool eventMatchingCondition)
        {
            var hasNightFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeNight);
            var hasDayFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeDay);

            if (hasNightFlag || hasDayFlag) eventMatchingCondition = hasNightFlag && Campaign.Current.IsNight || hasDayFlag && Campaign.Current.IsDay;

            if (!eventMatchingCondition) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the time conditions.");
        }

        private void LocationAndEventCheck(PartyBase captorParty, out bool eventMatchingCondition)
        {
            var hasCityFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity);
            var hasDungeonFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationDungeon);
            var hasVillageFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationVillage);
            var hasHideoutFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationHideout);
            var hasCastleFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCastle);
            var hasPartyInTownFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationPartyInTown);
            var hasTravelingFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationTravellingParty);
            var visitedByCaravanFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.VisitedByCaravan);
            var visitedByLordFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.VisitedByLord);
            var duringSiegeFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.DuringSiege);
            var duringRaidFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.DuringRaid);

            eventMatchingCondition = true;

            if (hasCityFlag || hasDungeonFlag || hasVillageFlag || hasHideoutFlag || hasTravelingFlag || hasCastleFlag || hasPartyInTownFlag || visitedByCaravanFlag || duringSiegeFlag || duringRaidFlag)
            {
                eventMatchingCondition = false;

                if (captorParty != null && captorParty.IsSettlement)
                {
                    if (captorParty.Settlement.IsTown && (hasDungeonFlag || hasCityFlag))
                    {
                        if (visitedByCaravanFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan) != null;
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Caravan");
                            }
                        else if (visitedByLordFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty) != null;
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        else eventMatchingCondition = true;
                    }

                    if (hasVillageFlag && captorParty.Settlement.IsVillage) eventMatchingCondition = true;

                    if (hasHideoutFlag && captorParty.Settlement.IsHideout()) eventMatchingCondition = true;

                    if (hasCastleFlag && captorParty.Settlement.IsCastle)
                    {
                        if (visitedByLordFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty) != null;
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        else eventMatchingCondition = true;
                    }

                    if (duringSiegeFlag != captorParty.Settlement.IsUnderSiege) eventMatchingCondition = false;

                    if (duringRaidFlag != captorParty.Settlement.IsUnderRaid) eventMatchingCondition = false;
                }
                else if (captorParty != null && captorParty.IsMobile && captorParty.MobileParty.CurrentSettlement != null)
                {
                    if (hasPartyInTownFlag && captorParty.MobileParty.CurrentSettlement.IsTown)
                    {
                        if (visitedByCaravanFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan) != null;
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Caravan");
                            }
                        else if (visitedByLordFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty) != null;
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        else eventMatchingCondition = true;
                    }

                    if (hasVillageFlag && captorParty.MobileParty.CurrentSettlement.IsVillage) eventMatchingCondition = true;

                    if (hasCastleFlag && captorParty.MobileParty.CurrentSettlement.IsCastle)
                    {
                        if (visitedByLordFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty) != null;
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        else eventMatchingCondition = true;
                    }

                    if (duringSiegeFlag != captorParty.MobileParty.CurrentSettlement.IsUnderSiege) eventMatchingCondition = false;
                    if (duringRaidFlag != captorParty.MobileParty.CurrentSettlement.IsUnderRaid) eventMatchingCondition = false;
                    if (hasHideoutFlag && captorParty.MobileParty.CurrentSettlement.IsHideout()) eventMatchingCondition = true;
                }
                else if (hasTravelingFlag)
                {
                    if (captorParty.IsMobile)
                    {
                        eventMatchingCondition = true;

                        if (duringSiegeFlag != (captorParty.MobileParty.BesiegerCamp != null)) eventMatchingCondition = false;

                        var raidingEvent = captorParty.MapEvent != null && captorParty.MapEvent.IsRaid && captorParty.MapFaction.IsAtWarWith(captorParty.MapEvent.MapEventSettlement.MapFaction) && captorParty.MapEvent.DefenderSide.TroopCount == 0;
                        if (duringRaidFlag != raidingEvent) eventMatchingCondition = false;
                    }
                }
            }

            if (!eventMatchingCondition) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the location conditions.");
        }

        private void CaptorPartyGenderCheck(PartyBase captorParty)
        {
            if (captorParty?.Leader != null && captorParty.Leader.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorGenderIsMale)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorGenderIsMale.");
            if (captorParty?.Leader != null && !captorParty.Leader.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorGenderIsFemale)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorGenderIsFemale/Femdom.");
        }

        private void CaptorItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorPartyHaveItem.IsStringNoneOrEmpty()) return;

                var flagHaveItem = false;
                var foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqCaptorPartyHaveItem);

                if (captorParty.LeaderHero != null)
                    foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                    {
                        try
                        {
                            var battleItem = captorParty.LeaderHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                            if (battleItem != null && battleItem == foundItem)
                            {
                                flagHaveItem = true;

                                break;
                            }
                        }
                        catch (Exception) { }

                        try
                        {
                            var civilianItem = captorParty.LeaderHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

                            if (civilianItem == null || civilianItem != foundItem) continue;
                            flagHaveItem = true;

                            break;
                        }
                        catch (Exception) { }
                    }

                if (captorParty.ItemRoster.FindIndexOfItem(foundItem) != -1) flagHaveItem = true;

                if (!flagHaveItem) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptorItem / Failed ");
            }
        }

        private void CaptorSkillCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorSkill.IsStringNoneOrEmpty()) return;

                if (captorParty.LeaderHero == null) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkill.");

                var skillLevel = captorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqCaptorSkill));

                try
                {
                    if (!_listEvent.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelAbove))
                            throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelAbove.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty()) return;

                    if (skillLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelBelow.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow");
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptorTrait / Failed ");
            }
        }

        private void CaptorTraitCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorTrait.IsStringNoneOrEmpty()) return;

                if (captorParty.LeaderHero == null) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTrait.");

                var traitLevel = captorParty.LeaderHero.GetTraitLevel(TraitObject.Find(_listEvent.ReqCaptorTrait));

                try
                {
                    if (!_listEvent.ReqCaptorTraitLevelAbove.IsStringNoneOrEmpty())
                        if (traitLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelAbove))
                            throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelAbove.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorTraitLevelBelow.IsStringNoneOrEmpty()) return;

                    if (traitLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelBelow.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow");
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptorTrait / Failed ");
            }
        }

        private void MoraleCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMoraleAbove.IsStringNoneOrEmpty())
                    if (captorParty.IsMobile && captorParty.MobileParty.Morale < new VariablesLoader().GetIntFromXML(_listEvent.ReqMoraleAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMoraleBelow.IsStringNoneOrEmpty()) return;

                if (captorParty.IsMobile && captorParty.MobileParty.Morale > new VariablesLoader().GetIntFromXML(_listEvent.ReqMoraleBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed ");
            }
        }

        private void FemaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) return;

                if (captorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }
        }

        private void MaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) return;

                if (captorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed ");
            }
        }

        private void CaptiveCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.NumberOfPrisoners < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqCaptivesBelow.IsStringNoneOrEmpty()) return;

                if (captorParty.NumberOfPrisoners > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed ");
            }
        }

        private void FemaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsBelow))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed ");
            }
        }

        private void MaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) return;

                if (captorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) > new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed ");
            }
        }

        private void TroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.NumberOfRegularMembers < new VariablesLoader().GetIntFromXML(_listEvent.ReqTroopsAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqTroopsBelow.IsStringNoneOrEmpty()) return;

                if (captorParty.NumberOfRegularMembers > new VariablesLoader().GetIntFromXML(_listEvent.ReqTroopsBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed ");
            }
        }

        private void CaptivesOutNumberCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptivesOutNumber) && captorParty.NumberOfPrisoners < captorParty.NumberOfHealthyMembers) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptivesOutNumber.");
        }

        private void CaptorCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorIsHero) && captorParty.LeaderHero == null) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorIsHero.");
        }

        private void IsOwnedByNotableCheck()
        {
            var skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotOwnedByNotable);
            var isOwnedFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;
            var isNotOwnedFlag = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;

            if (!isOwnedFlag && !isNotOwnedFlag) return;

            if (isOwnedFlag && CECampaignBehavior.ExtraProps.Owner == null) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. isOwnedFlag.");
            if (isNotOwnedFlag && CECampaignBehavior.ExtraProps.Owner != null) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. isNotOwnedFlag.");
        }

        private void PlayerCheck()
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqGoldAbove))
                    if (Hero.MainHero.Gold < new VariablesLoader().GetIntFromXML(_listEvent.ReqGoldAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed ");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqGoldBelow)) return;

                if (Hero.MainHero.Gold > new VariablesLoader().GetIntFromXML(_listEvent.ReqGoldBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed ");
            }
        }

        private void HeroCheck(CharacterObject captive, PartyBase captorParty, bool nonRandomBehaviour)
        {
            if (captive.IsHero && captive.HeroObject != null && (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptiveIsHero) || captive.IsPlayerCharacter))
            {
                var captiveHero = captive.HeroObject;
                //HeroChecks(captiveHero); // Please fix the lag and the wrong flags, in this file.

                if (nonRandomBehaviour)
                {
                    CaptiveHaveItemCheck(captiveHero);

                    RelationCheck(captorParty, captiveHero);
                }
                else
                {
                    HeroHaveItemCheck(captorParty);
                }
            }
            else if (captive.IsHero && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptiveIsNonHero) && captive.HeroObject != null)
            {
                throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsNonHero.");
            }
            else if (!captive.IsHero && captive.HeroObject == null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptiveIsHero))
            {
                throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsHero.");
            }
        }

        private void HeroHaveItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty()) return;

                var flagHaveItem = false;
                var foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqHeroPartyHaveItem);

                if (captorParty.LeaderHero != null)
                    foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                    {
                        try
                        {
                            var battleItem = captorParty.LeaderHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                            if (battleItem != null && battleItem == foundItem)
                            {
                                flagHaveItem = true;

                                break;
                            }
                        }
                        catch (Exception) { }

                        try
                        {
                            var civilianItem = captorParty.LeaderHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

                            if (civilianItem == null || civilianItem != foundItem) continue;
                            flagHaveItem = true;

                            break;
                        }
                        catch (Exception) { }
                    }

                if (captorParty.ItemRoster.FindIndexOfItem(foundItem) != -1) flagHaveItem = true;

                if (!flagHaveItem) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqCaptiveHaveItem");
            }
        }

        private void RelationCheck(PartyBase captorParty, Hero captiveHero)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationAbove) && captorParty.LeaderHero != null)
                    if (captiveHero.GetRelation(captorParty.LeaderHero) < new VariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationAbove))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationAbove.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroCaptorRelationAbove");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationBelow) || captorParty.LeaderHero == null) return;

                if (captiveHero.GetRelation(captorParty.LeaderHero) > new VariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationBelow))
                    throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationBelow.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroCaptorRelationBelow");
            }
        }

        private void CaptiveHaveItemCheck(Hero captiveHero)
        {
            try
            {
                if (!_listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty())
                {
                    var foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqHeroPartyHaveItem);

                    if (foundItem == null)
                    {
                        CECustomHandler.LogToFile("ReqCaptiveHaveItem " + _listEvent.ReqHeroPartyHaveItem + " not found for " + _listEvent.Name);
                    }
                    else
                    {
                        var flagHaveItem = false;

                        foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                        {
                            try
                            {
                                var battleItem = captiveHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                                if (battleItem != null && battleItem == foundItem)
                                {
                                    flagHaveItem = true;

                                    break;
                                }
                            }
                            catch (Exception) { }

                            try
                            {
                                var civilianItem = captiveHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

                                if (civilianItem == null || civilianItem != foundItem) continue;
                                flagHaveItem = true;

                                break;
                            }
                            catch (Exception) { }
                        }

                        if (!flagHaveItem) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqCaptiveHaveItem");
            }
        }

        private void HeroChecks(Hero captiveHero)
        {
            if (captiveHero.IsChild && _listEvent.SexualContent) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. SexualContent Child Detected.");
            if (captiveHero.Children.Count == 0 && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroHaveOffspring)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroHaveOffspring.");
            if (!captiveHero.IsPregnant && !CECampaignBehavior.CheckIfPregnancyExists(captiveHero) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsPregnant)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsPregnant.");
            if ((captiveHero.IsPregnant || CECampaignBehavior.CheckIfPregnancyExists(captiveHero)) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotPregnant)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsNotPregnant.");
            if (captiveHero.Spouse == null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroHaveSpouse)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroHaveSpouse.");
            if (captiveHero.Spouse != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotHaveSpouse)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroNotHaveSpouse.");
            if (captiveHero.OwnedCommonAreas.Count == 0 && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnsFief)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroOwnsFief.");
            if ((captiveHero.Clan == null || captiveHero != captiveHero.Clan.Leader) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsClanLeader)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsClanLeader.");
            if (!captiveHero.IsFactionLeader && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsFactionLeader)) throw new CESkippingEventException( "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsFactionLeader.");
        }

        private void HealthCheck(CharacterObject captive)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroHealthBelowPercentage))
                    if (captive.HitPoints > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthBelowPercentage))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthBelowPercentage.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroHealthBelowPercentage");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroHealthAbovePercentage)) return;

                if (captive.HitPoints < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthAbovePercentage)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthAbovePercentage.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroHealthAbovePercentage");
            }
        }

        private void SkillCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroSkill.IsStringNoneOrEmpty()) return;

                var skillLevel = captive.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqHeroSkill));

                try
                {
                    if (!_listEvent.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelAbove))
                            throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelAbove.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqHeroSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty()) return;

                    if (skillLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelBelow.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqHeroSkillLevelBelow");
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqHeroSkill / Failed ");
            }
        }

        private void TraitCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroTrait.IsStringNoneOrEmpty()) return;

                var traitLevel = captive.GetTraitLevel(TraitObject.Find(_listEvent.ReqHeroTrait));

                try
                {
                    if (!string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelAbove))
                        if (traitLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelAbove))
                            throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelAbove.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove");
                }

                try
                {
                    if (string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelBelow)) return;

                    if (traitLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelBelow)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelBelow.");
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow");
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqTrait");
            }
        }

        private void AgeCheck(CharacterObject captive)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroMinAge))
                    if (captive.Age < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroMinAge))
                        throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMinAge.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroMinAge");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroMaxAge)) return;

                if (captive.Age > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaxAge)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaxAge.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroMaxAge");
            }
        }

        private void ProstitutionCheck(CharacterObject captive)
        {
            var skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotProstitute);
            var heroProstituteFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && !skipFlags;
            var heroNotProstituteFlag = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && !skipFlags;

            var prostituteFlag = true;

            try
            {
                if (heroProstituteFlag || heroNotProstituteFlag)
                {
                    prostituteFlag = false;
                    var prostituteSkillFlag = captive.GetSkillValue(CESkills.IsProstitute);

                    if (prostituteSkillFlag != 0 && heroProstituteFlag)
                    {
                        prostituteFlag = true;
                        var prostitute = captive.GetSkillValue(CESkills.Prostitution);

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroProstituteLevelAbove))
                                if (prostitute < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroProstituteLevelAbove))
                                    throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelAbove.");
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroProstituteLevelAbove");
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroProstituteLevelBelow))
                                if (prostitute > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroProstituteLevelBelow))
                                    throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelBelow.");
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroProstituteLevelBelow");
                        }
                    }

                    if (prostituteSkillFlag == 0 && heroNotProstituteFlag) prostituteFlag = true;
                }

                if (!prostituteFlag) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions for ProstituteFlag.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed prostituteFlag");
            }
        }

        private void SlaveryCheck(CharacterObject captive)
        {
            var skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsSlave) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotSlave);
            var heroIsSlave = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsSlave) && !skipFlags;
            var heroIsNotSlave = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsSlave) && !skipFlags;

            var slaveCondition = true;

            try
            {
                if (heroIsSlave || heroIsNotSlave)
                {
                    slaveCondition = false;
                    var slaveSkillFlag = captive.GetSkillValue(CESkills.IsSlave);

                    if (slaveSkillFlag != 0 && heroIsSlave)
                    {
                        slaveCondition = true;
                        var slave = captive.GetSkillValue(CESkills.Slavery);

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroSlaveLevelAbove))
                                if (slave < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSlaveLevelAbove))
                                    throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelAbove.");
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroSlaveLevelAbove");
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroSlaveLevelBelow))
                                if (slave > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSlaveLevelBelow))
                                    throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelBelow.");
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroSlaveLevelBelow");
                        }
                    }

                    if (slaveSkillFlag == 0 && heroIsNotSlave) slaveCondition = true;
                }

                if (!slaveCondition) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions for slave level flags.");
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed slaveFlag");
            }
        }

        private void GenderCheck(CharacterObject captive)
        {
            if (!captive.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroGenderIsFemale.");
            if (captive.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroGenderIsMale.");
        }

        private void SettingsCheck()
        {
            if (CESettings.Instance != null && !CESettings.Instance.SexualContent && _listEvent.SexualContent) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " SexualContent events disabled.");
            if (!CESettings.Instance.NonSexualContent && !_listEvent.SexualContent) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " NonSexualContent events disabled.");
            if (!CESettings.Instance.FemdomControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Femdom)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " Femdom events disabled.");
            if (!CESettings.Instance.CommonControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Common)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " Common events disabled.");
            if (!CESettings.Instance.BestialityControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Bestiality)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " Bestiality events disabled.");
            if (!CESettings.Instance.ProstitutionControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " Prostitution events disabled.");
            if (!CESettings.Instance.RomanceControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Romance)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " Romance events disabled.");
            if (PlayerEncounter.Current != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.PlayerIsNotBusy)) throw new CESkippingEventException("Skipping event " + _listEvent.Name + " Player is busy.");
        }

        private void ValidateEvent()
        {
            if (_listEvent == null) throw new CESkippingEventException("Something is not right in FlagsDoMatchEventConditions.  Expected an event but got null.");
        }

        #endregion
    }
}