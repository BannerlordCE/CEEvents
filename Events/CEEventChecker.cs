using System;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Events
{
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


            var response = ValidateEvent();

            if (!string.IsNullOrEmpty(response)) return response;

            response = SettingsCheck();

            if (!string.IsNullOrEmpty(response)) return response;

            response = GenderCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = SlaveryCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = ProstitutionCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = AgeCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = TraitCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = SkillCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = HealthCheck(captive);

            if (!string.IsNullOrEmpty(response)) return response;

            response = HeroCheck(captive, captorParty, nonRandomBehaviour);

            if (!string.IsNullOrEmpty(response)) return response;

            response = PlayerCheck();

            if (!string.IsNullOrEmpty(response)) return response;

            response = IsOwnedByNotableCheck();

            if (!string.IsNullOrEmpty(response)) return response;

            response = CaptorCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = CaptivesOutNumberCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = TroopsCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = MaleTroopsCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = FemaleTroopsCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = CaptiveCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = MaleCaptivesCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = FemaleCaptivesCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;

            response = MoraleCheck(captorParty);

            if (!string.IsNullOrEmpty(response)) return response;


            if (nonRandomBehaviour)
            {
                response = CaptorTraitCheck(captorParty);

                if (!string.IsNullOrEmpty(response)) return response;

                response = CaptorSkillCheck(captorParty);

                if (!string.IsNullOrEmpty(response)) return response;

                response = CaptorItemCheck(captorParty);

                if (!string.IsNullOrEmpty(response)) return response;

                response = CaptorPartyGenderCheck(captorParty);

                if (!string.IsNullOrEmpty(response)) return response;
            }

            response = LocationAndEventCheck(captorParty, out var eventMatchingCondition);

            if (!string.IsNullOrEmpty(response)) return response;

            response = TimeCheck(ref eventMatchingCondition);

            if (!string.IsNullOrEmpty(response)) return response;

            response = SeasonCheck(ref eventMatchingCondition);

            if (!string.IsNullOrEmpty(response)) return response;


            _listEvent.Captive = captive;

            return null;
        }


#region private

        private string SeasonCheck(ref bool eventMatchingCondition)
        {
            var hasWinterFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonWinter);
            var hasSummerFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSpring);
            var hasSpringFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSummer);
            var hasFallFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonFall);

            if (hasWinterFlag || hasSummerFlag) eventMatchingCondition = hasSummerFlag && CampaignTime.Now.GetSeasonOfYear == 1 || hasFallFlag && CampaignTime.Now.GetSeasonOfYear == 2 || hasWinterFlag && CampaignTime.Now.GetSeasonOfYear == 3 || hasSpringFlag && (CampaignTime.Now.GetSeasonOfYear == 4 || CampaignTime.Now.GetSeasonOfYear == 0);

            if (!eventMatchingCondition) return "Skipping event " + _listEvent.Name + " it does not match the seasons conditions.";

            return "";
        }

        private string TimeCheck(ref bool eventMatchingCondition)
        {
            var hasNightFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeNight);
            var hasDayFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeDay);

            if (hasNightFlag || hasDayFlag) eventMatchingCondition = hasNightFlag && Campaign.Current.IsNight || hasDayFlag && Campaign.Current.IsDay;

            if (!eventMatchingCondition) return "Skipping event " + _listEvent.Name + " it does not match the time conditions.";

            return "";
        }

        private string LocationAndEventCheck(PartyBase captorParty, out bool eventMatchingCondition)
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

            if (!eventMatchingCondition) return "Skipping event " + _listEvent.Name + " it does not match the location conditions.";

            return "";
        }

        private string CaptorPartyGenderCheck(PartyBase captorParty)
        {
            if (captorParty?.Leader != null && captorParty.Leader.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorGenderIsMale)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorGenderIsMale.";
            if (captorParty?.Leader != null && !captorParty.Leader.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorGenderIsFemale)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorGenderIsFemale/Femdom.";

            return "";
        }

        private string CaptorItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorPartyHaveItem.IsStringNoneOrEmpty()) return "";

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

                if (!flagHaveItem) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptorItem / Failed ");
            }

            return "";
        }

        private string CaptorSkillCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorSkill.IsStringNoneOrEmpty()) return "";

                if (captorParty.LeaderHero == null) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkill.";

                var skillLevel = captorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqCaptorSkill));

                try
                {
                    if (!_listEvent.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelAbove))
                            return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelAbove.";
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty()) return "";

                    if (skillLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelBelow.";
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

            return "";
        }

        private string CaptorTraitCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorTrait.IsStringNoneOrEmpty()) return "";

                if (captorParty.LeaderHero == null) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTrait.";

                var traitLevel = captorParty.LeaderHero.GetTraitLevel(TraitObject.Find(_listEvent.ReqCaptorTrait));

                try
                {
                    if (!_listEvent.ReqCaptorTraitLevelAbove.IsStringNoneOrEmpty())
                        if (traitLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelAbove))
                            return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelAbove.";
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorTraitLevelBelow.IsStringNoneOrEmpty()) return "";

                    if (traitLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelBelow.";
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

            return "";
        }

        private string MoraleCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMoraleAbove.IsStringNoneOrEmpty())
                    if (captorParty.IsMobile && captorParty.MobileParty.Morale < new VariablesLoader().GetIntFromXML(_listEvent.ReqMoraleAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMoraleBelow.IsStringNoneOrEmpty()) return "";

                if (captorParty.IsMobile && captorParty.MobileParty.Morale > new VariablesLoader().GetIntFromXML(_listEvent.ReqMoraleBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed ");
            }

            return "";
        }

        private string FemaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) return "";

                if (captorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            return "";
        }

        private string MaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) return "";

                if (captorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed ");
            }

            return "";
        }

        private string CaptiveCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.NumberOfPrisoners < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqCaptivesBelow.IsStringNoneOrEmpty()) return "";

                if (captorParty.NumberOfPrisoners > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed ");
            }

            return "";
        }

        private string FemaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsBelow))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed ");
            }

            return "";
        }

        private string MaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) return "";

                if (captorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed ");
            }

            return "";
        }

        private string TroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.NumberOfRegularMembers < new VariablesLoader().GetIntFromXML(_listEvent.ReqTroopsAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqTroopsBelow.IsStringNoneOrEmpty()) return "";

                if (captorParty.NumberOfRegularMembers > new VariablesLoader().GetIntFromXML(_listEvent.ReqTroopsBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed ");
            }

            return "";
        }

        private string CaptivesOutNumberCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptivesOutNumber) && captorParty.NumberOfPrisoners < captorParty.NumberOfHealthyMembers) return "Skipping event " + _listEvent.Name + " it does not match the conditions. CaptivesOutNumber.";
            return "";
        }

        private string CaptorCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorIsHero) && captorParty.LeaderHero == null) return "Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorIsHero.";
            return "";
        }

        private string IsOwnedByNotableCheck()
        {
            var skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotOwnedByNotable);
            var isOwnedFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;
            var isNotOwnedFlag = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;

            if (!isOwnedFlag && !isNotOwnedFlag) return "";

            if (isOwnedFlag && CECampaignBehavior.ExtraProps.Owner == null) return "Skipping event " + _listEvent.Name + " it does not match the conditions. isOwnedFlag.";
            if (isNotOwnedFlag && CECampaignBehavior.ExtraProps.Owner != null) return "Skipping event " + _listEvent.Name + " it does not match the conditions. isNotOwnedFlag.";
            return "";
        }

        private string PlayerCheck()
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqGoldAbove))
                    if (Hero.MainHero.Gold < new VariablesLoader().GetIntFromXML(_listEvent.ReqGoldAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed ");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqGoldBelow)) return "";

                if (Hero.MainHero.Gold > new VariablesLoader().GetIntFromXML(_listEvent.ReqGoldBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed ");
            }
            return "";
        }

        private string HeroCheck(CharacterObject captive, PartyBase captorParty, bool nonRandomBehaviour)
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
                return "Skipping event " + _listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsNonHero.";
            }
            else if (!captive.IsHero && captive.HeroObject == null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptiveIsHero))
            {
                return "Skipping event " + _listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsHero.";
            }
            return "";
        }

        private string HeroHaveItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty()) return "";

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

                if (!flagHaveItem) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqCaptiveHaveItem");
            }
            return "";
        }

        private string RelationCheck(PartyBase captorParty, Hero captiveHero)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationAbove) && captorParty.LeaderHero != null)
                    if (captiveHero.GetRelation(captorParty.LeaderHero) < new VariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationAbove))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationAbove.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroCaptorRelationAbove");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationBelow) || captorParty.LeaderHero == null) return "";

                if (captiveHero.GetRelation(captorParty.LeaderHero) > new VariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationBelow.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroCaptorRelationBelow");
            }
            return "";
        }

        private string CaptiveHaveItemCheck(Hero captiveHero)
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

                        if (!flagHaveItem) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqCaptiveHaveItem");
            }
            return "";
        }

        private string HeroChecks(Hero captiveHero)
        {
            if (captiveHero.IsChild && _listEvent.SexualContent) return "Skipping event " + _listEvent.Name + " it does not match the conditions. SexualContent Child Detected.";
            if (captiveHero.Children.Count == 0 && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroHaveOffspring)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroHaveOffspring.";
            if (!captiveHero.IsPregnant && !CECampaignBehavior.CheckIfPregnancyExists(captiveHero) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsPregnant)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsPregnant.";
            if ((captiveHero.IsPregnant || CECampaignBehavior.CheckIfPregnancyExists(captiveHero)) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotPregnant)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsNotPregnant.";
            if (captiveHero.Spouse == null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroHaveSpouse)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroHaveSpouse.";
            if (captiveHero.Spouse != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotHaveSpouse)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroNotHaveSpouse.";
            if (captiveHero.OwnedCommonAreas.Count == 0 && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnsFief)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroOwnsFief.";
            if ((captiveHero.Clan == null || captiveHero != captiveHero.Clan.Leader) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsClanLeader)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsClanLeader.";
            if (!captiveHero.IsFactionLeader && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsFactionLeader)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsFactionLeader.";
            return "";
        }

        private string HealthCheck(CharacterObject captive)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroHealthBelowPercentage))
                    if (captive.HitPoints > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthBelowPercentage))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthBelowPercentage.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroHealthBelowPercentage");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroHealthAbovePercentage)) return "";

                if (captive.HitPoints < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthAbovePercentage)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthAbovePercentage.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroHealthAbovePercentage");
            }
            return "";
        }

        private string SkillCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroSkill.IsStringNoneOrEmpty()) return "";

                var skillLevel = captive.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqHeroSkill));

                try
                {
                    if (!_listEvent.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelAbove))
                            return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelAbove.";
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing ReqHeroSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty()) return "";

                    if (skillLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelBelow.";
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
            return "";
        }

        private string TraitCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroTrait.IsStringNoneOrEmpty()) return "";

                var traitLevel = captive.GetTraitLevel(TraitObject.Find(_listEvent.ReqHeroTrait));

                try
                {
                    if (!string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelAbove))
                        if (traitLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelAbove))
                            return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelAbove.";
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove");
                }

                try
                {
                    if (string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelBelow)) return "";

                    if (traitLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelBelow)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelBelow.";
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
            return "";
        }

        private string AgeCheck(CharacterObject captive)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroMinAge))
                    if (captive.Age < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroMinAge))
                        return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMinAge.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroMinAge");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroMaxAge)) return "";

                if (captive.Age > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaxAge)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaxAge.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroMaxAge");
            }
            return "";
        }

        private string ProstitutionCheck(CharacterObject captive)
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
                                    return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelAbove.";
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroProstituteLevelAbove");
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroProstituteLevelBelow))
                                if (prostitute > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroProstituteLevelBelow))
                                    return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelBelow.";
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroProstituteLevelBelow");
                        }
                    }

                    if (prostituteSkillFlag == 0 && heroNotProstituteFlag) prostituteFlag = true;
                }

                if (!prostituteFlag) return "Skipping event " + _listEvent.Name + " it does not match the conditions for ProstituteFlag.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed prostituteFlag");
            }
            return "";
        }

        private string SlaveryCheck(CharacterObject captive)
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
                                    return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelAbove.";
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroSlaveLevelAbove");
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroSlaveLevelBelow))
                                if (slave > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSlaveLevelBelow))
                                    return "Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelBelow.";
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroSlaveLevelBelow");
                        }
                    }

                    if (slaveSkillFlag == 0 && heroIsNotSlave) slaveCondition = true;
                }

                if (!slaveCondition) return "Skipping event " + _listEvent.Name + " it does not match the conditions for slave level flags.";
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed slaveFlag");
            }
            return "";
        }

        private string GenderCheck(CharacterObject captive)
        {
            if (!captive.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroGenderIsFemale.";
            if (captive.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale)) return "Skipping event " + _listEvent.Name + " it does not match the conditions. HeroGenderIsMale.";
            return "";
        }

        private string SettingsCheck()
        {
            if (CESettings.Instance != null && !CESettings.Instance.SexualContent && _listEvent.SexualContent) return "Skipping event " + _listEvent.Name + " SexualContent events disabled.";
            if (!CESettings.Instance.NonSexualContent && !_listEvent.SexualContent) return "Skipping event " + _listEvent.Name + " NonSexualContent events disabled.";
            if (!CESettings.Instance.FemdomControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Femdom)) return "Skipping event " + _listEvent.Name + " Femdom events disabled.";
            if (!CESettings.Instance.CommonControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Common)) return "Skipping event " + _listEvent.Name + " Common events disabled.";
            if (!CESettings.Instance.BestialityControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Bestiality)) return "Skipping event " + _listEvent.Name + " Bestiality events disabled.";
            if (!CESettings.Instance.ProstitutionControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution)) return "Skipping event " + _listEvent.Name + " Prostitution events disabled.";
            if (!CESettings.Instance.RomanceControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Romance)) return "Skipping event " + _listEvent.Name + " Romance events disabled.";
            if (PlayerEncounter.Current != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.PlayerIsNotBusy)) return "Skipping event " + _listEvent.Name + " Player is busy.";

            return "";
        }

        private string ValidateEvent()
        {
            return _listEvent == null
                ? "Something is not right in FlagsDoMatchEventConditions.  Expected an event but got null."
                : "";
        }

#endregion
    }
}