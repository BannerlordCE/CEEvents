using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
                string t = _message;
                _message = "";

                return t;
            }
            set => _message = value;
        }

        public CEEventChecker(CEEvent listEvent) => _listEvent = listEvent;

        public static string CheckFlags(CharacterObject captive, PartyBase captorParty = null)
        {
            string returnString = "";
            if (captorParty == null) captorParty = PartyBase.MainParty;

            returnString += "\n------- " + captive.Name + "'s Status -------\n";

            returnString += "Gender: " +
                (captive.IsFemale ?
                    "Female" :
                    "Male") +
                "\n";

            foreach (SkillObject skill in CESkills.CustomSkills)
            {
                int value = captive.GetSkillValue(skill);
                CESkillNode skillNode = CESkills.FindSkillNode(skill.StringId);
                bool isTrueFalse = (skillNode.MaxLevel == "1" && skillNode.MinLevel == "0");
                returnString += skill.StringId + " : " +
                    (isTrueFalse ? (value != 0 ? "True" : "False") : value.ToString()) +
                    "\n";
            }

            returnString += "Owner: " +
                (CECampaignBehavior.ExtraProps.Owner == null ?
                    "None" :
                    CECampaignBehavior.ExtraProps.Owner.Name.ToString()) +
                "\n";

            returnString += "Location : ";
            if (captorParty != null && captorParty.IsSettlement)
            {
                if (captorParty.Settlement.IsTown)
                {
                    returnString += "(hasDungeonFlag || hasCityFlag)";

                    try
                    {
                        bool hasCaravan = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null;
                        if (hasCaravan) returnString += "(visitedByCaravanFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Caravan");
                    }

                    try
                    {
                        bool hasLord = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty && !mobileParty.IsMainParty; }) != null;
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
                        bool hasLord = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty && !mobileParty.IsMainParty; }) != null;
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
                        bool hasCaravan = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null;
                        if (hasCaravan) returnString += "(visitedByCaravanFlag)";
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Caravan");
                    }

                    try
                    {
                        bool hasLord = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty && !mobileParty.IsMainParty; }) != null;
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
                        bool hasLord = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty && !mobileParty.IsMainParty; }) != null;
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

            Vec3? position3D = (captorParty != null && captorParty.IsMobile) ? captorParty?.MobileParty?.GetPosition() : captorParty?.Settlement?.GetPosition();
            List<TerrainType> faceTerrainType = Campaign.Current.MapSceneWrapper.GetEnvironmentTerrainTypes(captorParty.Position2D);
            AtmosphereInfo atmosphere = new DefaultMapWeatherModel().GetAtmosphereModel(CampaignTime.Now, (Vec3)position3D);

            string environmentTerrainTypes = "";
            faceTerrainType.ForEach((type) => { environmentTerrainTypes += type.ToString() + " "; });
            if (atmosphere.SnowInfo.Density > 0) environmentTerrainTypes += "(Snow)";
            returnString += "\nEnvironment Terrain Types : " + environmentTerrainTypes;

            returnString += "\n\n\n------- Party Status -------";
            if (captorParty.IsMobile) returnString += "\nMoral Total : " + captorParty.MobileParty.Morale;
            if (captorParty != PartyBase.MainParty && captorParty?.Leader != null)
            {
                returnString += "\nParty Leader Name : " + captorParty.Leader.Name.ToString();
                returnString += "\nParty Leader Hero : " + (captorParty.Leader.IsHero ? "True" : "False");
                returnString += "\nParty Leader Gender : " + (captorParty.Leader.IsFemale ? "Female" : "Male");
            }

            returnString += "\n\n--- Party Members ---";

            returnString += "\nTotal Females : " + captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            returnString += "\nTotal Males : " + captorParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            returnString += "\nTotal : " + captorParty.MemberRoster.Count();

            returnString += "\n\n--- Captive Members ---";

            returnString += "\nTotal Females : " + captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            returnString += "\nTotal Males : " + captorParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; });
            returnString += "\nTotal : " + captorParty.PrisonRoster.Count();

            returnString += "\n\n--- Other Settings ---";
            returnString += "\nToo Many Companions : " + (Clan.PlayerClan.Companions.Count<Hero>() >= Clan.PlayerClan.CompanionLimit);

            returnString += "\nWork in progress\n";

            return returnString;
        }

        public string FlagsDoMatchEventConditions(CharacterObject captive, PartyBase captorParty = null)
        {
            bool nonRandomBehaviour = true;

            if (captorParty == null)
            {
                nonRandomBehaviour = false;
                captorParty = PartyBase.MainParty;
            }

            if (!ValidateEvent()) return LatestMessage;
            if (!SettingsCheck()) return LatestMessage;
            if (!CustomFlagCheck()) return LatestMessage;
            if (!GenderCheck(captive)) return LatestMessage;
            if (!SlaveryCheck(captive)) return LatestMessage;
            if (!SlaveryLevelCheck(captive)) return LatestMessage;
            if (!ProstitutionCheck(captive)) return LatestMessage;
            if (!ProstitutionLevelCheck(captive)) return LatestMessage;
            if (!AgeCheck(captive)) return LatestMessage;
            if (!HeroTraitCheck(captive)) return LatestMessage;
            if (!HeroSkillCheck(captive)) return LatestMessage;
            if (!TraitsCheck(captive)) return LatestMessage;
            if (!SkillsCheck(captive)) return LatestMessage;
            if (!HealthCheck(captive)) return LatestMessage;
            if (!HeroCheck(captive, captorParty, nonRandomBehaviour)) return LatestMessage;
            if (!PlayerCheck()) return LatestMessage;
            if (!PlayerItemCheck()) return LatestMessage;
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
                if (!CaptorTraitsCheck(captorParty)) return LatestMessage;
                if (!CaptorSkillCheck(captorParty)) return LatestMessage;
                if (!CaptorSkillsCheck(captorParty)) return LatestMessage;
                if (!CaptorItemCheck(captorParty)) return LatestMessage;
                if (!CaptorPartyGenderCheck(captorParty)) return LatestMessage;
            }

            if (!LocationAndEventCheck(captorParty, out bool eventMatchingCondition)) return LatestMessage;
            if (!WorldMapCheck(captorParty, ref eventMatchingCondition)) return LatestMessage;
            if (!TimeCheck(ref eventMatchingCondition)) return LatestMessage;
            if (!SeasonCheck(ref eventMatchingCondition)) return LatestMessage;

            _listEvent.Captive = captive;

            return null;
        }

        #region private

        private bool WorldMapCheck(PartyBase party, ref bool eventMatchingCondition)
        {

            if (_listEvent.TerrianTypesRequirements == null) return true;

            foreach (TerrianTypes terrianTypes in _listEvent.TerrianTypesRequirements)
            {
                bool hasWorldMapWater = terrianTypes.TerrianType.Contains("Water");
                bool hasWorldMapMountain = terrianTypes.TerrianType.Contains("Mountain");
                bool hasWorldMapSnow = terrianTypes.TerrianType.Contains("Snow");
                bool hasWorldMapSteppe = terrianTypes.TerrianType.Contains("Steppe");
                bool hasWorldMapPlain = terrianTypes.TerrianType.Contains("Plain");
                bool hasWorldMapDesert = terrianTypes.TerrianType.Contains("Desert");
                bool hasWorldMapSwamp = terrianTypes.TerrianType.Contains("Swamp");
                bool hasWorldMapDune = terrianTypes.TerrianType.Contains("Dune");
                bool hasWorldMapBridge = terrianTypes.TerrianType.Contains("Bridge");
                bool hasWorldMapRiver = terrianTypes.TerrianType.Contains("River");
                bool hasWorldMapForest = terrianTypes.TerrianType.Contains("Forest");
                bool hasWorldMapShallowRiver = terrianTypes.TerrianType.Contains("ShallowRiver");
                bool hasWorldMapLake = terrianTypes.TerrianType.Contains("Lake");
                bool hasWorldMapCanyon = terrianTypes.TerrianType.Contains("Canyon");
                bool hasWorldMapRuralArea = terrianTypes.TerrianType.Contains("RuralArea");

                if (
                    hasWorldMapWater || hasWorldMapMountain || hasWorldMapSnow || hasWorldMapSteppe || hasWorldMapPlain || hasWorldMapDesert || hasWorldMapSwamp || hasWorldMapDune || hasWorldMapBridge || hasWorldMapRiver || hasWorldMapForest || hasWorldMapShallowRiver || hasWorldMapLake || hasWorldMapCanyon || hasWorldMapRuralArea
                )
                {
                    Vec3? position3D = (party != null && party.IsMobile) ? party?.MobileParty?.GetPosition() : party?.Settlement?.GetPosition();
                    List<TerrainType> faceTerrainType = Campaign.Current.MapSceneWrapper.GetEnvironmentTerrainTypes(party.Position2D);
                    AtmosphereInfo atmosphere = new DefaultMapWeatherModel().GetAtmosphereModel(CampaignTime.Now, (Vec3)position3D);

                    string environmentTerrainTypes = "";
                    faceTerrainType.ForEach((type) => { environmentTerrainTypes += type.ToString() + " "; });
                    if (atmosphere.SnowInfo.Density > 0) environmentTerrainTypes += "(Snow)";

                    eventMatchingCondition = false;
                    if (hasWorldMapWater) eventMatchingCondition = environmentTerrainTypes.Contains("Water");
                    if (hasWorldMapMountain) eventMatchingCondition = environmentTerrainTypes.Contains("Mountain");
                    if (hasWorldMapSnow) eventMatchingCondition = environmentTerrainTypes.Contains("Snow");
                    if (hasWorldMapSteppe) eventMatchingCondition = environmentTerrainTypes.Contains("Steppe");
                    if (hasWorldMapPlain) eventMatchingCondition = environmentTerrainTypes.Contains("Plain");
                    if (hasWorldMapDesert) eventMatchingCondition = environmentTerrainTypes.Contains("Desert");
                    if (hasWorldMapSwamp) eventMatchingCondition = environmentTerrainTypes.Contains("Swamp");
                    if (hasWorldMapDune) eventMatchingCondition = environmentTerrainTypes.Contains("Dune");
                    if (hasWorldMapBridge) eventMatchingCondition = environmentTerrainTypes.Contains("Bridge");
                    if (hasWorldMapRiver) eventMatchingCondition = environmentTerrainTypes.Contains("River");
                    if (hasWorldMapForest) eventMatchingCondition = environmentTerrainTypes.Contains("Forest");
                    if (hasWorldMapShallowRiver) eventMatchingCondition = environmentTerrainTypes.Contains("ShallowRiver");
                    if (hasWorldMapLake) eventMatchingCondition = environmentTerrainTypes.Contains("Lake");
                    if (hasWorldMapCanyon) eventMatchingCondition = environmentTerrainTypes.Contains("Canyon");
                    if (hasWorldMapRuralArea) eventMatchingCondition = environmentTerrainTypes.Contains("RuralArea");

                    if (eventMatchingCondition) break;
                }
            }

            if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the TerrianType conditions.");

            return true;
        }

        private bool SeasonCheck(ref bool eventMatchingCondition)
        {
            bool hasWinterFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonWinter);
            bool hasSummerFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSpring);
            bool hasSpringFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonSummer);
            bool hasFallFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.SeasonFall);

            if (hasWinterFlag || hasSummerFlag || hasSpringFlag || hasFallFlag)
            {
                eventMatchingCondition =
                    hasSummerFlag && CampaignTime.Now.GetSeasonOfYear == 1 ||
                    hasFallFlag && CampaignTime.Now.GetSeasonOfYear == 2 ||
                    hasWinterFlag && CampaignTime.Now.GetSeasonOfYear == 3 ||
                    hasSpringFlag && (CampaignTime.Now.GetSeasonOfYear == 4 || CampaignTime.Now.GetSeasonOfYear == 0);
            }

            if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the seasons conditions.");

            return true;
        }

        private bool TimeCheck(ref bool eventMatchingCondition)
        {
            bool hasNightFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeNight);
            bool hasDayFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.TimeDay);

            if (hasNightFlag || hasDayFlag) eventMatchingCondition = hasNightFlag && Campaign.Current.IsNight || hasDayFlag && Campaign.Current.IsDay;

            if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the time conditions.");

            return true;
        }

        private bool LocationAndEventCheck(PartyBase captorParty, out bool eventMatchingCondition)
        {
            bool hasCityFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity);
            bool hasDungeonFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationDungeon);
            bool hasVillageFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationVillage);
            bool hasHideoutFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationHideout);
            bool hasCastleFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCastle);
            bool hasPartyInTownFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationPartyInTown);
            bool hasPartyInVillageFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationPartyInVillage);
            bool hasPartyInCastleFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationPartyInCastle);
            bool hasTravelingFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationTravellingParty);
            bool visitedByCaravanFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.VisitedByCaravan);
            bool visitedByLordFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.VisitedByLord);
            bool duringSiegeFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.DuringSiege);
            bool duringRaidFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.DuringRaid);

            eventMatchingCondition = true;

            if (hasCityFlag || hasDungeonFlag || hasVillageFlag || hasHideoutFlag || hasTravelingFlag || hasCastleFlag || hasPartyInTownFlag || visitedByCaravanFlag || duringSiegeFlag || duringRaidFlag)
            {
                eventMatchingCondition = false;

                if (captorParty != null && captorParty.IsSettlement)
                {
                    if (captorParty.Settlement.IsTown && (hasDungeonFlag || hasCityFlag))
                    {
                        if (visitedByCaravanFlag)
                        {
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan) != null;
                                if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the visitedByCaravanFlag conditions.");
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Caravan");
                            }
                        }
                        else if (visitedByLordFlag)
                        {
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty && !mobileParty.IsMainParty) != null;
                                if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the visitedByLordFlag conditions.");
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            eventMatchingCondition = true;
                        }
                    }

                    if (hasVillageFlag && captorParty.Settlement.IsVillage) eventMatchingCondition = true;

                    if (hasHideoutFlag && captorParty.Settlement.IsHideout()) eventMatchingCondition = true;

                    if ((hasCastleFlag || hasDungeonFlag) && captorParty.Settlement.IsCastle)
                    {
                        if (visitedByLordFlag)
                        {
                            try
                            {
                                eventMatchingCondition = captorParty.Settlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty && !mobileParty.IsMainParty) != null;
                                if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the visitedByLordFlag conditions.");
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            eventMatchingCondition = true;
                        }
                    }

                    if (duringSiegeFlag != captorParty.Settlement.IsUnderSiege) eventMatchingCondition = false;

                    if (duringRaidFlag != captorParty.Settlement.IsUnderRaid) eventMatchingCondition = false;
                }
                else if (captorParty != null && captorParty.IsMobile && captorParty.MobileParty.CurrentSettlement != null)
                {
                    if (hasPartyInTownFlag && captorParty.MobileParty.CurrentSettlement.IsTown)
                    {
                        if (visitedByCaravanFlag)
                        {
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsCaravan) != null;
                                if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the visitedByCaravanFlag conditions.");
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Caravan");
                            }
                        }
                        else if (visitedByLordFlag)
                        {
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty && !mobileParty.IsMainParty) != null;
                                if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the visitedByLordFlag conditions.");
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            eventMatchingCondition = true;
                        }
                    }

                    if (hasPartyInVillageFlag && captorParty.MobileParty.CurrentSettlement.IsVillage) eventMatchingCondition = true;

                    if (hasPartyInCastleFlag && captorParty.MobileParty.CurrentSettlement.IsCastle)
                    {
                        if (visitedByLordFlag)
                        {
                            try
                            {
                                eventMatchingCondition = captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => mobileParty.IsLordParty && !mobileParty.IsMainParty) != null;
                                if (!eventMatchingCondition) return Error("Skipping event " + _listEvent.Name + " it does not match the visitedByLordFlag conditions.");
                            }
                            catch (Exception)
                            {
                                return LogError("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            eventMatchingCondition = true;
                        }
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

                        bool raidingEvent = captorParty.MapEvent != null && captorParty.MapEvent.IsRaid && captorParty.MapFaction.IsAtWarWith(captorParty.MapEvent.MapEventSettlement.MapFaction) && captorParty.MapEvent.DefenderSide.TroopCount == 0;
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

        private bool PlayerItemCheck()
        {
            try
            {
                if (_listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty()) return true;

                bool flagHaveItem = false;
                ItemObject foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqHeroPartyHaveItem);

                foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                {
                    try
                    {
                        ItemObject battleItem = Hero.MainHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                        if (battleItem != null && battleItem == foundItem)
                        {
                            flagHaveItem = true;

                            break;
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        ItemObject civilianItem = Hero.MainHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

                        if (civilianItem == null || civilianItem != foundItem) continue;
                        flagHaveItem = true;

                        break;
                    }
                    catch (Exception) { }
                }

                if (PartyBase.MainParty.ItemRoster.FindIndexOfItem(foundItem) != -1) flagHaveItem = true;

                if (!flagHaveItem) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroPartyHaveItem / Failed ");
            }

            return true;
        }

        private bool CaptorItemCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorPartyHaveItem.IsStringNoneOrEmpty()) return true;

                bool flagHaveItem = false;
                ItemObject foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqCaptorPartyHaveItem);

                if (captorParty.LeaderHero != null)
                {
                    foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                    {
                        try
                        {
                            ItemObject battleItem = captorParty.LeaderHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                            if (battleItem != null && battleItem == foundItem)
                            {
                                flagHaveItem = true;

                                break;
                            }
                        }
                        catch (Exception) { }

                        try
                        {
                            ItemObject civilianItem = captorParty.LeaderHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

                            if (civilianItem == null || civilianItem != foundItem) continue;
                            flagHaveItem = true;

                            break;
                        }
                        catch (Exception) { }
                    }
                }

                if (captorParty.ItemRoster.FindIndexOfItem(foundItem) != -1) flagHaveItem = true;

                if (!flagHaveItem) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorPartyHaveItem.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptorPartyHaveItem / Failed ");
            }

            return true;
        }

        private bool CaptorSkillCheck(PartyBase captorParty)
        {
            try
            {
                if (_listEvent.ReqCaptorSkill.IsStringNoneOrEmpty()) return true;

                if (captorParty.LeaderHero == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkill.");

                int skillLevel = captorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _listEvent.ReqCaptorSkill));

                try
                {
                    if (!_listEvent.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                    {
                        if (skillLevel < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelAbove.");
                    }
                }
                catch (Exception)
                {
                    return LogError("Missing ReqCaptorSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty()) return true;

                    if (skillLevel > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqCaptorSkillLevelBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelBelow.");
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

                int traitLevel = captorParty.LeaderHero.GetTraitLevel(TraitObject.Find(_listEvent.ReqCaptorTrait));

                try
                {
                    if (!_listEvent.ReqCaptorTraitLevelAbove.IsStringNoneOrEmpty())
                    {
                        if (traitLevel < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelAbove.");
                    }
                }
                catch (Exception)
                {
                    return LogError("Missing ReqCaptorTraitLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqCaptorTraitLevelBelow.IsStringNoneOrEmpty()) return true;

                    if (traitLevel > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqCaptorTraitLevelBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelBelow.");
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
                {
                    if (captorParty.IsMobile && captorParty.MobileParty.Morale < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqMoraleAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMoraleAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMoraleBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.IsMobile && captorParty.MobileParty.Morale > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqMoraleBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMoraleBelow.");
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
                {
                    if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqHeroFemaleCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroFemaleCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroFemaleCaptivesAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroFemaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqFemaleCaptivesBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqHeroFemaleCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroFemaleCaptivesBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroFemaleCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroFemaleCaptivesBelow / Failed ");
            }

            return true;
        }

        private bool MaleCaptivesCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleCaptivesAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqHeroMaleCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaleCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaleCaptivesAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroMaleCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqMaleCaptivesBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleCaptivesBelow / Failed ");
            }

            try
            {
                if (_listEvent.ReqHeroMaleCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaleCaptivesBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaleCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroMaleCaptivesBelow / Failed ");
            }

            return true;
        }

        private bool CaptiveCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.NumberOfPrisoners < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqHeroCaptivesAbove.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroCaptivesAbove))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptivesAbove.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroCaptivesAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.NumberOfPrisoners > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqCaptivesBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqCaptivesBelow / Failed ");
            }

            try
            {
                if (_listEvent.ReqHeroCaptivesBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroCaptivesBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptivesBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroCaptivesBelow / Failed ");
            }

            return true;
        }

        private bool FemaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqHeroFemaleTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroFemaleTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroFemaleTroopsAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroFemaleTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqFemaleTroopsBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqFemaleTroopsBelow.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqFemaleTroopsBelow / Failed ");
            }

            try
            {
                if (!_listEvent.ReqHeroFemaleTroopsBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroFemaleTroopsBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroFemaleTroopsBelow.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroFemaleTroopsBelow / Failed ");
            }

            return true;
        }

        private bool MaleTroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqHeroMaleTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaleTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaleTroopsAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroMaleTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqMaleTroopsBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqMaleTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqMaleTroopsBelow / Failed ");
            }

            try
            {
                if (_listEvent.ReqHeroMaleTroopsBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale && troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaleTroopsBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaleTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroMaleTroopsBelow / Failed ");
            }

            return true;
        }

        private bool TroopsCheck(PartyBase captorParty)
        {
            try
            {
                if (!_listEvent.ReqTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsAbove / Failed ");
            }

            try
            {
                if (!_listEvent.ReqHeroTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroTroopsAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTroopsAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroTroopsAbove / Failed ");
            }

            try
            {
                if (_listEvent.ReqTroopsBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.MemberRoster.Sum(troopRosterElement => { return troopRosterElement.Number; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqTroopsBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqTroopsBelow / Failed ");
            }

            try
            {
                if (_listEvent.ReqHeroTroopsBelow.IsStringNoneOrEmpty()) return true;

                if (captorParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsHero) ? troopRosterElement.Number : 0; }) > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroTroopsBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTroopsBelow.");
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqHeroTroopsBelow / Failed ");
            }

            return true;
        }

        private bool CaptivesOutNumberCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptivesOutNumber) && captorParty.NumberOfPrisoners < captorParty.NumberOfHealthyMembers)
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptivesOutNumber.");

            return true;
        }

        private bool CaptorCheck(PartyBase captorParty)
        {
            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorIsHero) && captorParty.LeaderHero == null)
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorIsHero.");

            if (_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CaptorIsNonHero) && captorParty.LeaderHero != null)
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. CaptorIsNonHero.");

            return true;
        }

        private bool IsOwnedByNotableCheck()
        {
            bool skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroNotOwnedByNotable);
            bool isOwnedFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;
            bool isNotOwnedFlag = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;

            if (!isOwnedFlag && !isNotOwnedFlag) return true;

            if (CECampaignBehavior.ExtraProps == null) CECampaignBehavior.ResetFullData();

            if (isOwnedFlag && CECampaignBehavior.ExtraProps.Owner == null)
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. isOwnedFlag.");

            if (isNotOwnedFlag && CECampaignBehavior.ExtraProps.Owner != null)
                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. isNotOwnedFlag.");

            return true;
        }

        private bool PlayerCheck()
        {

            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqGoldAbove))
                {
                    if (Hero.MainHero.Gold < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqGoldAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect ReqGoldAbove / Failed ");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqGoldBelow)) return true;

                if (Hero.MainHero.Gold > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqGoldBelow))
                    return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqGoldBelow.");
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
                Hero captiveHero = captive.HeroObject;
                return HeroChecks(captiveHero) && (nonRandomBehaviour && CaptiveHaveItemCheck(captiveHero) && RelationCheck(captorParty, captiveHero) || HeroHaveItemCheck(captorParty));

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

                bool flagHaveItem = false;
                ItemObject foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqHeroPartyHaveItem);

                if (captorParty.LeaderHero != null)
                {
                    foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                    {
                        try
                        {
                            ItemObject battleItem = captorParty.LeaderHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                            if (battleItem != null && battleItem == foundItem)
                            {
                                flagHaveItem = true;

                                break;
                            }
                        }
                        catch (Exception) { }

                        try
                        {
                            ItemObject civilianItem = captorParty.LeaderHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

                            if (civilianItem == null || civilianItem != foundItem) continue;
                            flagHaveItem = true;

                            break;
                        }
                        catch (Exception) { }
                    }
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
                {
                    if (captiveHero.GetRelation(captorParty.LeaderHero) < new CEVariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroCaptorRelationAbove");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroCaptorRelationBelow) || captorParty.LeaderHero == null) return true;

                if (captiveHero.GetRelation(captorParty.LeaderHero) > new CEVariablesLoader().GetFloatFromXML(_listEvent.ReqHeroCaptorRelationBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationBelow.");
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
                    ItemObject foundItem = ItemObject.All.FirstOrDefault(item => item.StringId == _listEvent.ReqHeroPartyHaveItem);

                    if (foundItem == null) return LogError("ReqCaptiveHaveItem " + _listEvent.ReqHeroPartyHaveItem + " not found for " + _listEvent.Name);

                    bool flagHaveItem = false;

                    foreach (EquipmentIndex i in Enum.GetValues(typeof(EquipmentIndex)))
                    {
                        try
                        {
                            ItemObject battleItem = captiveHero.BattleEquipment.GetEquipmentFromSlot(i).Item;

                            if (battleItem != null && battleItem == foundItem)
                            {
                                flagHaveItem = true;

                                break;
                            }
                        }
                        catch (Exception) { }

                        try
                        {
                            ItemObject civilianItem = captiveHero.CivilianEquipment.GetEquipmentFromSlot(i).Item;

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
                {
                    if (captive.HitPoints > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthBelowPercentage))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthBelowPercentage.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroHealthBelowPercentage");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroHealthAbovePercentage)) return true;

                if (captive.HitPoints < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroHealthAbovePercentage)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroHealthAbovePercentage.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroHealthAbovePercentage");
            }

            return true;
        }

        private bool CaptorSkillsCheck(PartyBase captorParty)
        {
            if (_listEvent.SkillsRequired == null) return true;
            if (captorParty.Leader == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorSkill.");
            return SkillsCheck(captorParty.Leader, true);
        }

        private bool SkillsCheck(CharacterObject character, bool captor = false)
        {
            try
            {
                if (_listEvent.SkillsRequired == null) return true;

                foreach (SkillRequired skillRequired in _listEvent.SkillsRequired)
                {
                    if (captor && skillRequired.Ref == "Hero") continue;

                    SkillObject foundSkill = CESkills.FindSkill(skillRequired.Id);
                    if (foundSkill == null) return LogError("Couldn't find " + skillRequired.Id);
                    int skillLevel = character.GetSkillValue(foundSkill);

                    try
                    {
                        if (!skillRequired.Min.IsStringNoneOrEmpty())
                        {
                            if (skillLevel < new CEVariablesLoader().GetIntFromXML(skillRequired.Min))
                                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. " + (captor ? "ReqCaptorSkillLevelAbove" : "ReqHeroSkillLevelAbove") + ".");
                        }
                    }
                    catch (Exception)
                    {
                        return LogError("Invalid Skill Required Min");
                    }

                    try
                    {
                        if (!skillRequired.Max.IsStringNoneOrEmpty())
                        {

                            if (skillLevel > new CEVariablesLoader().GetIntFromXML(skillRequired.Max))
                                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. " + (captor ? "ReqCaptorSkillLevelBelow" : "ReqHeroSkillLevelBelow") + ".");
                        }
                    }
                    catch (Exception)
                    {
                        return LogError("Invalid Skill Required Max");
                    }
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect SkillsRequired / Failed ");
            }

            return true;
        }

        private bool HeroSkillCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroSkill.IsStringNoneOrEmpty()) return true;

                SkillObject foundSkill = CESkills.FindSkill(_listEvent.ReqHeroSkill);
                if (foundSkill == null) return LogError("Couldn't find " + _listEvent.ReqHeroSkill);

                int skillLevel = captive.GetSkillValue(foundSkill);

                try
                {
                    if (!_listEvent.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                    {
                        if (skillLevel < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelAbove.");
                    }
                }
                catch (Exception)
                {
                    return LogError("Missing ReqHeroSkillLevelAbove");
                }

                try
                {
                    if (_listEvent.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty()) return true;

                    if (skillLevel > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroSkillLevelBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelBelow.");
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

        private bool CaptorTraitsCheck(PartyBase captorParty)
        {
            if (_listEvent.TraitsRequired == null) return true;
            if (captorParty.Leader == null) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqCaptorTrait.");
            return TraitsCheck(captorParty.Leader, true);
        }

        private bool TraitsCheck(CharacterObject character, bool captor = false)
        {
            try
            {
                if (_listEvent.TraitsRequired == null) return true;

                foreach (TraitRequired traitRequired in _listEvent.TraitsRequired)
                {
                    if (captor && traitRequired.Ref == "Hero") continue;

                    TraitObject foundTrait = TraitObject.Find(traitRequired.Id);
                    if (foundTrait == null) return LogError("Couldn't find " + traitRequired.Id);
                    int traitLevel = character.GetTraitLevel(foundTrait);

                    try
                    {
                        if (!traitRequired.Min.IsStringNoneOrEmpty())
                        {
                            if (traitLevel < new CEVariablesLoader().GetIntFromXML(traitRequired.Min))
                                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. " + (captor ? "ReqCaptorTraitLevelAbove" : "ReqHeroTraitLevelAbove") + ".");
                        }
                    }
                    catch (Exception)
                    {
                        return LogError("Invalid Trait Required Min");
                    }

                    try
                    {
                        if (!traitRequired.Max.IsStringNoneOrEmpty())
                        {

                            if (traitLevel > new CEVariablesLoader().GetIntFromXML(traitRequired.Max))
                                return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. " + (captor ? "ReqCaptorTraitLevelBelow" : "ReqHeroTraitLevelBelow") + ".");
                        }
                    }
                    catch (Exception)
                    {
                        return LogError("Invalid Trait Required Max");
                    }
                }
            }
            catch (Exception)
            {
                return LogError("Incorrect TraitsRequired / Failed ");
            }

            return true;
        }

        private bool HeroTraitCheck(CharacterObject captive)
        {
            try
            {
                if (_listEvent.ReqHeroTrait.IsStringNoneOrEmpty()) return true;

                int traitLevel = captive.GetTraitLevel(TraitObject.Find(_listEvent.ReqHeroTrait));

                try
                {
                    if (!string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelAbove))
                    {
                        if (traitLevel < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelAbove))
                            return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelAbove.");
                    }
                }
                catch (Exception)
                {
                    return LogError("Invalid ReqHeroTraitLevelAbove");
                }

                try
                {
                    if (string.IsNullOrEmpty(_listEvent.ReqHeroTraitLevelBelow)) return true;

                    if (traitLevel > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroTraitLevelBelow)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelBelow.");
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
                {
                    if (captive.Age < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroMinAge))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMinAge.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroMinAge");
            }

            try
            {
                if (string.IsNullOrEmpty(_listEvent.ReqHeroMaxAge)) return true;

                if (captive.Age > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroMaxAge)) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroMaxAge.");
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroMaxAge");
            }

            return true;
        }

        private bool ProstitutionLevelCheck(CharacterObject captive)
        {
            int prostitute = captive.GetSkillValue(CESkills.Prostitution);

            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroProstituteLevelAbove))
                {
                    if (prostitute < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroProstituteLevelAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroProstituteLevelAbove");
            }

            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroProstituteLevelBelow))
                {
                    if (prostitute > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroProstituteLevelBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelBelow.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroProstituteLevelBelow");
            }

            return true;
        }

        private bool ProstitutionCheck(CharacterObject captive)
        {
            bool skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotProstitute);
            bool heroProstituteFlag = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && !skipFlags;
            bool heroNotProstituteFlag = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && !skipFlags;

            try
            {
                if (heroProstituteFlag || heroNotProstituteFlag)
                {
                    int prostituteSkillFlag = captive.GetSkillValue(CESkills.IsProstitute);

                    if (prostituteSkillFlag == 0 && heroProstituteFlag) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsProstitute.");
                    if (prostituteSkillFlag != 0 && heroNotProstituteFlag) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsNotProstitute.");
                }
            }
            catch (Exception)
            {
                return LogError("Failed HeroIsProstitute HeroIsNotProstitute");
            }

            return true;
        }

        private bool SlaveryLevelCheck(CharacterObject captive)
        {
            int slave = captive.GetSkillValue(CESkills.Slavery);

            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroSlaveLevelAbove))
                {
                    if (slave < new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroSlaveLevelAbove))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelAbove.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroSlaveLevelAbove");
            }

            try
            {
                if (!string.IsNullOrEmpty(_listEvent.ReqHeroSlaveLevelBelow))
                {
                    if (slave > new CEVariablesLoader().GetIntFromXML(_listEvent.ReqHeroSlaveLevelBelow))
                        return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelBelow.");
                }
            }
            catch (Exception)
            {
                return LogError("Missing ReqHeroSlaveLevelBelow");
            }

            return true;
        }

        private bool SlaveryCheck(CharacterObject captive)
        {
            bool skipFlags = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsSlave) && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsNotSlave);
            bool heroIsSlave = _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsSlave) && !skipFlags;
            bool heroIsNotSlave = !_listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsSlave) && !skipFlags;

            try
            {
                if (heroIsSlave || heroIsNotSlave)
                {
                    int slaveSkillFlag = captive.GetSkillValue(CESkills.IsSlave);
                    if (slaveSkillFlag == 0 && heroIsSlave) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsSlave.");
                    if (slaveSkillFlag != 0 && heroIsNotSlave) return Error("Skipping event " + _listEvent.Name + " it does not match the conditions. HeroIsNotSlave.");
                }
            }
            catch (Exception)
            {
                return LogError("Failed HeroIsSlave HeroIsNotSlave");
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
            // Settings
            if (!CESettings.Instance.SexualContent && _listEvent.SexualContent) return Error("Skipping event " + _listEvent.Name + " SexualContent events disabled.");
            if (!CESettings.Instance.NonSexualContent && !_listEvent.SexualContent) return Error("Skipping event " + _listEvent.Name + " NonSexualContent events disabled.");

            // Default Flags
            if (!CESettings.Instance.FemdomControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Femdom)) return Error("Skipping event " + _listEvent.Name + " Femdom events disabled.");
            if (!CESettings.Instance.CommonControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Common)) return Error("Skipping event " + _listEvent.Name + " Common events disabled.");
            if (!CESettings.Instance.BestialityControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Bestiality)) return Error("Skipping event " + _listEvent.Name + " Bestiality events disabled.");
            if (!CESettings.Instance.ProstitutionControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution)) return Error("Skipping event " + _listEvent.Name + " Prostitution events disabled.");
            if (!CESettings.Instance.RomanceControl && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Romance)) return Error("Skipping event " + _listEvent.Name + " Romance events disabled.");

            if (!CESettings.Instance.StolenGear && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.StripEnabled)) return Error("Skipping event " + _listEvent.Name + " StolenGear disabled.");
            if (CESettings.Instance.StolenGear && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.StripDisabled)) return Error("Skipping event " + _listEvent.Name + " StolenGear enabled.");

            // Custom Flags
            if (PlayerEncounter.Current != null && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.PlayerIsNotBusy)) return Error("Skipping event " + _listEvent.Name + " Player is busy.");
            if (Clan.PlayerClan.Companions.Count<Hero>() >= Clan.PlayerClan.CompanionLimit && _listEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.PlayerAllowedCompanion)) return Error("Skipping event " + _listEvent.Name + " Player has too many companions.");

            return true;
        }

        private bool CustomFlagCheck()
        {
            if (_listEvent.MultipleListOfCustomFlags != null && _listEvent.MultipleListOfCustomFlags.Count > 0)
            {
                try
                {
                    int size = _listEvent.MultipleListOfCustomFlags.Count;
                    for (int i = 0; i < size; i++)
                    {
                        KeyValuePair<string, bool> flagFound = CESettingsFlags.Instance.CustomFlags.First((flag) => { return flag.Key == _listEvent.MultipleListOfCustomFlags[i]; });

                        if (!flagFound.Value)
                        {
                            return Error("Skipping event " + _listEvent.Name + " " + _listEvent.MultipleListOfCustomFlags[i] + " events disabled.");
                        }
                    }
                }
                catch (Exception)
                {
                    return ForceLogError("Failure in CustomFlags: Missing flag for " + _listEvent.Name);
                }
            }

            return true;
        }

        private bool ValidateEvent() => _listEvent != null || Error("Something is not right in FlagsDoMatchEventConditions.  Expected an event but got null.");

        private bool ForceLogError(string message)
        {

            CECustomHandler.ForceLogToFile(message);

            return Error(message);
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