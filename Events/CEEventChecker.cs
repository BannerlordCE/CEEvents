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
        private string _message;

        private string LatestMessage
        {
            get
            {
                var t = _message;
                _message = "";

                return t;
            }
            set => _message = value;
        }

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

            if (!ValidateEvent()) return LatestMessage;
            if (!SettingsCheck()) return LatestMessage;
            if (!GenderCheck(captive)) return LatestMessage;
            if (!SlaveryCheck(captive)) return LatestMessage;
            if (!ProstitutionCheck(captive)) return LatestMessage;
            if (!AgeCheck(captive)) return LatestMessage;
            if (!TraitCheck(captive)) return LatestMessage;
            if (!SkillCheck(captive)) return LatestMessage;
            if (!HealthCheck(captive)) return LatestMessage;
            if (!HeroCheck(captive, captorParty, nonRandomBehaviour)) return LatestMessage;
            if (!PlayerCheck()) return LatestMessage;
            if (!IsOwnedByNotableCheck()) return LatestMessage;
            if (!CaptorCheck(captorParty)) return LatestMessage;
            if (!CaptivesOutNumberCheck(captorParty)) return LatestMessage;
            if (!TroopsCheck(captorParty)) return LatestMessage;
            if (!MaleTroopsCheck(captorParty)) return LatestMessage;
            if (!FemaleTroopsCheck(captorParty)) return LatestMessage;
            if (!CaptiveCheck(captorParty)) return LatestMessage;
            if (!MaleCaptivesCheck(captorParty)) return LatestMessage;
            if (!FemaleCaptivesCheck(captorParty)) return LatestMessage;
            if (!MoraleCheck(captorParty)) return LatestMessage;

            if (nonRandomBehaviour)
            {
                if (!CaptorTraitCheck(captorParty)) return LatestMessage;
                if (!CaptorSkillCheck(captorParty)) return LatestMessage;
                if (!CaptorItemCheck(captorParty)) return LatestMessage;
                if (!CaptorPartyGenderCheck(captorParty)) return LatestMessage;
            }

            if (!LocationAndEventCheck(captorParty, out var eventMatchingCondition)) return LatestMessage;
            if (!TimeCheck(ref eventMatchingCondition)) return LatestMessage;
            if (!SeasonCheck(ref eventMatchingCondition)) return LatestMessage;


            _listEvent.Captive = captive;

            return null;
        }


#region private

        private bool SeasonCheck(ref bool eventMatchingCondition)
        {
            var hasWinterFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonWinter);
            var hasSummerFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSpring);
            var hasSpringFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSummer);
            var hasFallFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonFall);

            if (hasWinterFlag || hasSummerFlag) eventMatchingCondition = hasSummerFlag && CampaignTime.Now.GetSeasonOfYear == 1 || hasFallFlag && CampaignTime.Now.GetSeasonOfYear == 2 || hasWinterFlag && CampaignTime.Now.GetSeasonOfYear == 3 || hasSpringFlag && (CampaignTime.Now.GetSeasonOfYear == 4 || CampaignTime.Now.GetSeasonOfYear == 0);

            if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the seasons conditions.");

            return true;
        }

        private bool TimeCheck(ref bool eventMatchingCondition)
        {
            var hasNightFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeNight);
            var hasDayFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeDay);

            if (hasNightFlag || hasDayFlag) eventMatchingCondition = hasNightFlag && Campaign.Current.IsNight || hasDayFlag && Campaign.Current.IsDay;

            if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the time conditions.");

            return true;
        }

        private bool LocationAndEventCheck(PartyBase captorParty, out bool eventMatchingCondition)
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
                                return LogError("Failed to get Caravan");
                            }
                        else if (visitedByLordFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty) != null;
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Lord Party");
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
                                return LogError("Failed to get Lord Party");
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
                                return LogError("Failed to get Caravan");
                            }
                        else if (visitedByLordFlag)
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty) != null;
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Lord Party");
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
                                return LogError("Failed to get Lord Party");
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

            if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the location conditions.");

            return true;
        }

        private bool CaptorPartyGenderCheck(PartyBase captorParty)
        {
            if (captorParty?.Leader != null && captorParty.Leader.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorGenderIsMale)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorGenderIsMale.");
            if (captorParty?.Leader != null && !captorParty.Leader.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorGenderIsFemale)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorGenderIsFemale/Femdom.");

            return true;
        }

        private bool CaptorItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorPartyHaveItem.IsStringNoneOrEmpty()) return true;

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

                if (!flagHaveItem) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptorItem / Failed ");
            }

            return true;
        }

        private bool CaptorSkillCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorSkill.IsStringNoneOrEmpty()) return true;

                if (captorParty.LeaderHero == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkill.");

                var skillLevel = captorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqCaptorSkill));

                try
                {
                    if (!_listEvent.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelAbove.");
                }
                catch (Exception)
                {
                    return LogError("Missing ReqCaptorSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty()) return true;

                    if (skillLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelBelow.");
                }
                catch (Exception)
                {
                    return LogError("Missing ReqCaptorSkillLevelBelow");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptorTrait / Failed ");
            }

            return true;
        }

        private bool CaptorTraitCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorTrait.IsStringNoneOrEmpty()) return true;

                if (captorParty.LeaderHero == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTrait.");

                var traitLevel = captorParty.LeaderHero.GetTraitLevel(TraitObject.Find(_listEvent.ReqCaptorTrait));

                try
                {
                    if (!_listEvent.ReqCaptorTraitLevelAbove.IsStringNoneOrEmpty())
                        if (traitLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelAbove.");
                }
                catch (Exception)
                {
                    return LogError("Missing ReqCaptorTraitLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorTraitLevelBelow.IsStringNoneOrEmpty()) return true;

                    if (traitLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelBelow.");
                }
                catch (Exception)
                {
                    return LogError("Missing ReqCaptorTraitLevelBelow");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptorTrait / Failed ");
            }

            return true;
        }

        private bool MoraleCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMoraleAbove.IsStringNoneOrEmpty())
                    if (captorParty.IsMobile && captorParty.MobileParty.Morale < new VariablesLoader().GetIntFromXML(_listEvent.ReqMoraleAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMoraleAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMoraleBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.IsMobile && captorParty.MobileParty.Morale > new VariablesLoader().GetIntFromXML(_listEvent.ReqMoraleBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMoraleBelow / Failed ");
            }

            return true;
        }

        private bool FemaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            return true;
        }

        private bool MaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleCaptivesBelow / Failed ");
            }

            return true;
        }

        private bool CaptiveCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqCaptivesAbove.IsStringNoneOrEmpty())
                    if (captorParty.NumberOfPrisoners < new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.NumberOfPrisoners > new VariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptivesBelow / Failed ");
            }

            return true;
        }

        private bool FemaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqFemaleTroopsBelow / Failed ");
            }

            return true;
        }

        private bool MaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) < new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.MemberRoster.Count(troopRosterElement => !troopRosterElement.Character.IsFemale) > new VariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleTroopsBelow / Failed ");
            }

            return true;
        }

        private bool TroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqTroopsAbove.IsStringNoneOrEmpty())
                    if (captorParty.NumberOfRegularMembers < new VariablesLoader().GetIntFromXML(_listEvent.ReqTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqTroopsBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.NumberOfRegularMembers > new VariablesLoader().GetIntFromXML(_listEvent.ReqTroopsBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsBelow / Failed ");
            }

            return true;
        }

        private bool CaptivesOutNumberCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptivesOutNumber) && captorParty.NumberOfPrisoners < captorParty.NumberOfHealthyMembers) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptivesOutNumber.");

            return true;
        }

        private bool CaptorCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorIsHero) && captorParty.LeaderHero == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorIsHero.");

            return true;
        }

        private bool IsOwnedByNotableCheck()
        {
            var skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotOwnedByNotable);
            var isOwnedFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;
            var isNotOwnedFlag = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;

            if (!isOwnedFlag && !isNotOwnedFlag) return true;

            if (isOwnedFlag && CECampaignBehavior.ExtraProps.Owner == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. isOwnedFlag.");
            if (isNotOwnedFlag && CECampaignBehavior.ExtraProps.Owner != null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. isNotOwnedFlag.");

            return true;
        }

        private bool PlayerCheck()
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqGoldAbove))
                    if (Hero.MainHero.Gold < new VariablesLoader().GetIntFromXML(_listEvent.ReqGoldAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqGoldAbove / Failed ");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqGoldBelow)) return true;

                if (Hero.MainHero.Gold > new VariablesLoader().GetIntFromXML(_listEvent.ReqGoldBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqGoldBelow / Failed ");
            }

            return true;
        }

        private bool HeroCheck(CharacterObject captive, PartyBase captorParty, bool nonRandomBehaviour)
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
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsNonHero.");
            }
            else if (!captive.IsHero && captive.HeroObject == null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptiveIsHero))
            {
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsHero.");
            }

            return true;
        }

        private bool HeroHaveItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty()) return true;

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

                if (!flagHaveItem) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqCaptiveHaveItem");
            }

            return true;
        }

        private bool RelationCheck(PartyBase captorParty, Hero captiveHero)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationAbove) && captorParty.LeaderHero != null)
                    if (captiveHero.GetRelation(captorParty.LeaderHero) < new VariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationAbove.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroCaptorRelationAbove");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationBelow) || captorParty.LeaderHero == null) return true;

                if (captiveHero.GetRelation(captorParty.LeaderHero) > new VariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationBelow.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroCaptorRelationBelow");
            }

            return true;
        }

        private bool CaptiveHaveItemCheck(Hero captiveHero)
        {
            try
            {
                if (!_listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty())
                {
                    var foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqHeroPartyHaveItem);

                    if (foundItem == null) return LogError("ReqCaptiveHaveItem " + _listEvent.ReqHeroPartyHaveItem + " not found for " + _listEvent.Name);

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

                    if (!flagHaveItem) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqCaptiveHaveItem");
            }

            return true;
        }

        private bool HeroChecks(Hero captiveHero)
        {
            if (captiveHero.IsChild && _listEvent.SexualContent) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. SexualContent Child Detected.");
            if (captiveHero.Children.Count == 0 && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroHaveOffspring)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroHaveOffspring.");
            if (!captiveHero.IsPregnant && !CECampaignBehavior.CheckIfPregnancyExists(captiveHero) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsPregnant)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsPregnant.");
            if ((captiveHero.IsPregnant || CECampaignBehavior.CheckIfPregnancyExists(captiveHero)) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotPregnant)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsNotPregnant.");
            if (captiveHero.Spouse == null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroHaveSpouse)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroHaveSpouse.");
            if (captiveHero.Spouse != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotHaveSpouse)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroNotHaveSpouse.");
            if (captiveHero.OwnedCommonAreas.Count == 0 && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnsFief)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroOwnsFief.");
            if ((captiveHero.Clan == null || captiveHero != captiveHero.Clan.Leader) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsClanLeader)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsClanLeader.");
            if (!captiveHero.IsFactionLeader && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsFactionLeader)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsFactionLeader.");

            return true;
        }

        private bool HealthCheck(CharacterObject captive)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroHealthBelowPercentage))
                    if (captive.HitPoints > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthBelowPercentage))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthBelowPercentage.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroHealthBelowPercentage");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroHealthAbovePercentage)) return true;

                if (captive.HitPoints < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthAbovePercentage)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthAbovePercentage.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroHealthAbovePercentage");
            }

            return true;
        }

        private bool SkillCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroSkill.IsStringNoneOrEmpty()) return true;

                var skillLevel = captive.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqHeroSkill));

                try
                {
                    if (!_listEvent.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                        if (skillLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelAbove.");
                }
                catch (Exception)
                {
                    return LogError("Missing ReqHeroSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty()) return true;

                    if (skillLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelBelow.");
                }
                catch (Exception)
                {
                    return LogError("Missing ReqHeroSkillLevelBelow");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroSkill / Failed ");
            }

            return true;
        }

        private bool TraitCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroTrait.IsStringNoneOrEmpty()) return true;

                var traitLevel = captive.GetTraitLevel(TraitObject.Find(_listEvent.ReqHeroTrait));

                try
                {
                    if (!string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelAbove))
                        if (traitLevel < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelAbove.");
                }
                catch (Exception)
                {
                    return LogError("Invalid ReqHeroTraitLevelAbove");
                }

                try
                {
                    if (string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelBelow)) return true;

                    if (traitLevel > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelBelow.");
                }
                catch (Exception)
                {
                    return LogError("Invalid ReqHeroTraitLevelBelow");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqTrait");
            }

            return true;
        }

        private bool AgeCheck(CharacterObject captive)
        {
            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroMinAge))
                    if (captive.Age < new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroMinAge))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMinAge.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroMinAge");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroMaxAge)) return true;

                if (captive.Age > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaxAge)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaxAge.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroMaxAge");
            }

            return true;
        }

        private bool ProstitutionCheck(CharacterObject captive)
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
                                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelAbove.");
                        }
                        catch (Exception)
                        {
                            return LogError("Missing ReqHeroProstituteLevelAbove");
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroProstituteLevelBelow))
                                if (prostitute > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroProstituteLevelBelow))
                                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelBelow.");
                        }
                        catch (Exception)
                        {
                            return LogError("Missing ReqHeroProstituteLevelBelow");
                        }
                    }

                    if (prostituteSkillFlag == 0 && heroNotProstituteFlag) prostituteFlag = true;
                }

                if (!prostituteFlag) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions for ProstituteFlag.");
            }
            catch (Exception)
            {
                return LogError("Failed prostituteFlag");
            }

            return true;
        }

        private bool SlaveryCheck(CharacterObject captive)
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
                                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelAbove.");
                        }
                        catch (Exception)
                        {
                            return LogError("Missing ReqHeroSlaveLevelAbove");
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(_listEvent.ReqHeroSlaveLevelBelow))
                                if (slave > new VariablesLoader().GetIntFromXML(_listEvent.ReqHeroSlaveLevelBelow))
                                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelBelow.");
                        }
                        catch (Exception)
                        {
                            return LogError("Missing ReqHeroSlaveLevelBelow");
                        }
                    }

                    if (slaveSkillFlag == 0 && heroIsNotSlave) slaveCondition = true;
                }

                if (!slaveCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions for slave level flags.");
            }
            catch (Exception)
            {
                return LogError("Failed slaveFlag");
            }

            return true;
        }

        private bool GenderCheck(CharacterObject captive)
        {
            if (!captive.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroGenderIsFemale.");
            if (captive.IsFemale && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroGenderIsMale.");

            return true;
        }

        private bool SettingsCheck()
        {
            if (CESettings.Instance != null && !CESettings.Instance.SexualContent && _listEvent.SexualContent) return Error("Skipping event " + _listEvent.Name + " SexualContent events disabled.");
            if (!CESettings.Instance.NonSexualContent && !_listEvent.SexualContent) return Error("Skipping event " + _listEvent.Name + " NonSexualContent events disabled.");
            if (!CESettings.Instance.FemdomControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Femdom)) return Error("Skipping event " + _listEvent.Name + " Femdom events disabled.");
            if (!CESettings.Instance.CommonControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Common)) return Error("Skipping event " + _listEvent.Name + " Common events disabled.");
            if (!CESettings.Instance.BestialityControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Bestiality)) return Error("Skipping event " + _listEvent.Name + " Bestiality events disabled.");
            if (!CESettings.Instance.ProstitutionControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution)) return Error("Skipping event " + _listEvent.Name + " Prostitution events disabled.");
            if (!CESettings.Instance.RomanceControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Romance)) return Error("Skipping event " + _listEvent.Name + " Romance events disabled.");
            if (PlayerEncounter.Current != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.PlayerIsNotBusy)) return Error("Skipping event " + _listEvent.Name + " Player is busy.");

            return true;
        }

        private bool ValidateEvent()
        {
            return _listEvent != null || Error("Something is not right in FlagsDoMatchEventConditions.  Expected an event but got null.");
        }

        private bool LogError(string message)
        {
            CECustomHandler.LogToFile(message);

            return Error(message);
        }

        private bool Error(string message)
        {
            LatestMessage = message;

            return false;
        }

#endregion
    }
}