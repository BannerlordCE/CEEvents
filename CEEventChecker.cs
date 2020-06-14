using CaptivityEvents.CampaignBehaviours;
using CaptivityEvents.Custom;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Events
{
    internal class CEEventChecker
    {
        public static string CheckFlags(CharacterObject captive, PartyBase captorParty = null)
        {
            string returnString = "";

            if (captorParty == null)
            {
                captorParty = PartyBase.MainParty;
            }

            returnString = returnString + "Captive Gender: " + (captive.IsFemale ? "Female" : "Male") + "\n";

            int slaveSkillFlag = captive.GetSkillValue(CESkills.IsSlave);
            returnString = returnString + "Captive is Slave: " + (slaveSkillFlag != 0 ? "True" : "False") + "\n";

            int prostituteSkillFlag = captive.GetSkillValue(CESkills.IsProstitute);
            returnString = returnString + "Captive is Prostitute: " + (prostituteSkillFlag != 0 ? "True" : "False") + "\n";

            returnString += "Location : ";

            if (captorParty != null && captorParty.IsSettlement)
            {
                if (captorParty.Settlement.IsTown)
                {
                    returnString += "(hasDungeonFlag || hasCityFlag)";
                    try
                    {
                        bool hasCaravan = (captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null);
                        if (hasCaravan)
                        {
                            returnString += "(visitedByCaravanFlag)";
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Caravan");
                    }

                    try
                    {
                        bool hasLord = (captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                        if (hasLord)
                        {
                            returnString += "(VisitedByLordFlag)";
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.Settlement.IsVillage)
                {
                    returnString += "(hasVillageFlag)";
                }

                if (captorParty.Settlement.IsHideout())
                {
                    returnString += "(hasHideoutFlag)";
                }

                if (captorParty.Settlement.IsCastle)
                {
                    returnString += "(hasCastleFlag)";

                    try
                    {
                        bool hasLord = (captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                        if (hasLord)
                        {
                            returnString += "(VisitedByLordFlag)";
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.Settlement.IsUnderSiege)
                {
                    returnString += "(duringSiegeFlag)";
                }

                if (captorParty.Settlement.IsUnderRaid)
                {
                    returnString += "(duringRaidFlag)";
                }
            }
            else if (captorParty != null && captorParty.IsMobile && captorParty.MobileParty.CurrentSettlement != null)
            {
                if (captorParty.MobileParty.CurrentSettlement.IsTown)
                {
                    returnString += "(hasPartyInTownFlag)";

                    try
                    {
                        bool hasCaravan = (captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null);
                        if (hasCaravan)
                        {
                            returnString += "(visitedByCaravanFlag)";
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Caravan");
                    }

                    try
                    {
                        bool hasLord = (captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                        if (hasLord)
                        {
                            returnString += "(VisitedByLordFlag)";
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.MobileParty.CurrentSettlement.IsVillage)
                {
                    returnString += "(hasVillageFlag)";
                }

                if (captorParty.MobileParty.CurrentSettlement.IsCastle)
                {
                    returnString += "(hasCastleFlag)";

                    try
                    {
                        bool hasLord = (captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                        if (hasLord)
                        {
                            returnString += "(VisitedByLordFlag)";
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Failed to get Lord Party");
                    }
                }

                if (captorParty.MobileParty.CurrentSettlement.IsHideout())
                {
                    returnString += "(hasHideoutFlag)";
                }

                if (captorParty.MobileParty.CurrentSettlement.IsUnderSiege)
                {
                    returnString += "(duringSiegeFlag)";
                }

                if (captorParty.MobileParty.CurrentSettlement.IsUnderRaid)
                {
                    returnString += "(duringRaidFlag)";
                }
            }
            else if (captorParty.IsMobile)
            {
                returnString += "(hasTravellingFlag)";
                if (captorParty.MobileParty.BesiegerCamp != null)
                {
                    returnString += "(duringSiegeFlag)";
                }

                if (captorParty.MapEvent != null && captorParty.MapEvent.IsRaid && captorParty.MapFaction.IsAtWarWith(captorParty.MapEvent.MapEventSettlement.MapFaction) && captorParty.MapEvent.DefenderSide.TroopCount == 0)
                {
                    returnString += "(duringRaidFlag)";
                }
            }

            returnString += "\nWork in progress\n";

            return returnString;
        }

        public static string FlagsDoMatchEventConditions(CEEvent listEvent, CharacterObject captive, PartyBase captorParty = null)
        {
            if (listEvent == null)
            {
                return "Something is not right in FlagsDoMatchEventConditions";
            }

            RestrictedListOfFlags[] restrictedList = listEvent.MultipleRestrictedListOfFlags;

            // Settings checking
            if (!CESettings.Instance.SexualContent && listEvent.SexualContent)
            {
                return "Skipping event " + listEvent.Name + " SexualContent events disabled.";
            }
            if (!CESettings.Instance.NonSexualContent && !listEvent.SexualContent)
            {
                return "Skipping event " + listEvent.Name + " NonSexualContent events disabled.";
            }
            if (!CESettings.Instance.FemdomControl && restrictedList.Contains(RestrictedListOfFlags.Femdom))
            {
                return "Skipping event " + listEvent.Name + " Femdom events disabled.";
            }
            if (!CESettings.Instance.CommonControl && restrictedList.Contains(RestrictedListOfFlags.Common))
            {
                return "Skipping event " + listEvent.Name + " Common events disabled.";
            }
            if (!CESettings.Instance.BestialityControl && restrictedList.Contains(RestrictedListOfFlags.Bestiality))
            {
                return "Skipping event " + listEvent.Name + " Bestiality events disabled.";
            }
            if (!CESettings.Instance.ProstitutionControl && restrictedList.Contains(RestrictedListOfFlags.Prostitution))
            {
                return "Skipping event " + listEvent.Name + " Prostitution events disabled.";
            }
            if (!CESettings.Instance.RomanceControl && restrictedList.Contains(RestrictedListOfFlags.Romance))
            {
                return "Skipping event " + listEvent.Name + " Romance events disabled.";
            }

            if (PlayerEncounter.Current != null && restrictedList.Contains(RestrictedListOfFlags.PlayerIsNotBusy))
            {
                return "Skipping event " + listEvent.Name + " Player is busy.";
            }

            bool nonRandomBehaviour = true;
            if (captorParty == null)
            {
                nonRandomBehaviour = false;
                captorParty = PartyBase.MainParty;
            }

            bool CEFlag = true;

            // Gender Checks
            if (!captive.IsFemale && restrictedList.Contains(RestrictedListOfFlags.HeroGenderIsFemale))
            {
                return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroGenderIsFemale.";
            }
            if (captive.IsFemale && restrictedList.Contains(RestrictedListOfFlags.HeroGenderIsMale))
            {
                return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroGenderIsMale.";
            }

            // Slavery Flags

            bool skipFlags = restrictedList.Contains(RestrictedListOfFlags.HeroIsSlave) && restrictedList.Contains(RestrictedListOfFlags.HeroIsNotSlave);
            bool heroSlaveFlag = restrictedList.Contains(RestrictedListOfFlags.HeroIsSlave) && !skipFlags;
            bool heroNotSlaveFlag = !restrictedList.Contains(RestrictedListOfFlags.HeroIsSlave) && !skipFlags;

            bool slaveFlag = true;
            try
            {
                if (heroSlaveFlag || heroNotSlaveFlag)
                {
                    slaveFlag = false;
                    int slaveSkillFlag = captive.GetSkillValue(CESkills.IsSlave);

                    if (slaveSkillFlag != 0 && heroSlaveFlag)
                    {
                        slaveFlag = true;
                        int slave = captive.GetSkillValue(CESkills.Slavery);

                        try
                        {
                            if (listEvent.ReqHeroSlaveLevelAbove != null && listEvent.ReqHeroSlaveLevelAbove != "")
                            {
                                if (slave < CEEventLoader.GetIntFromXML(listEvent.ReqHeroSlaveLevelAbove))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelAbove.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroSlaveLevelAbove");
                        }
                        try
                        {
                            if (listEvent.ReqHeroSlaveLevelBelow != null && listEvent.ReqHeroSlaveLevelBelow != "")
                            {
                                if (slave > CEEventLoader.GetIntFromXML(listEvent.ReqHeroSlaveLevelBelow))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroSlaveLevelBelow.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroSlaveLevelBelow");
                        }
                    }
                    if (slaveSkillFlag == 0 && heroNotSlaveFlag)
                    {
                        slaveFlag = true;
                    }
                }
                if (!slaveFlag)
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions for slave level flags.";
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed slaveFlag");
            }
            // End Slavery Flags

            // Prostitution Flags
            skipFlags = restrictedList.Contains(RestrictedListOfFlags.HeroIsProstitute) && restrictedList.Contains(RestrictedListOfFlags.HeroIsNotProstitute);
            bool heroProstituteFlag = restrictedList.Contains(RestrictedListOfFlags.HeroIsProstitute) && !skipFlags;
            bool heroNotProstituteFlag = !restrictedList.Contains(RestrictedListOfFlags.HeroIsProstitute) && !skipFlags;

            bool prostituteFlag = true;
            try
            {
                if (heroProstituteFlag || heroNotProstituteFlag)
                {
                    prostituteFlag = false;
                    int prostituteSkillFlag = captive.GetSkillValue(CESkills.IsProstitute);

                    if (prostituteSkillFlag != 0 && heroProstituteFlag)
                    {
                        prostituteFlag = true;
                        int prostitute = captive.GetSkillValue(CESkills.Prostitution);

                        try
                        {
                            if (listEvent.ReqHeroProstituteLevelAbove != null && listEvent.ReqHeroProstituteLevelAbove != "")
                            {
                                if (prostitute < CEEventLoader.GetIntFromXML(listEvent.ReqHeroProstituteLevelAbove))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelAbove.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroProstituteLevelAbove");
                        }
                        try
                        {
                            if (listEvent.ReqHeroProstituteLevelBelow != null && listEvent.ReqHeroProstituteLevelBelow != "")
                            {
                                if (prostitute > CEEventLoader.GetIntFromXML(listEvent.ReqHeroProstituteLevelBelow))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroProstituteLevelBelow.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqHeroProstituteLevelBelow");
                        }
                    }
                    if (prostituteSkillFlag == 0 && heroNotProstituteFlag)
                    {
                        prostituteFlag = true;
                    }
                }
                if (!prostituteFlag)
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions for ProstituteFlag.";
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed prostituteFlag");
            }
            // End Prostitution Flags

            // Age
            try
            {
                if (listEvent.ReqHeroMinAge != null && listEvent.ReqHeroMinAge != "")
                {
                    if (captive.Age < CEEventLoader.GetIntFromXML(listEvent.ReqHeroMinAge))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroMinAge.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroMinAge");
            }
            try
            {
                if (listEvent.ReqHeroMaxAge != null && listEvent.ReqHeroMaxAge != "")
                {
                    if (captive.Age > CEEventLoader.GetIntFromXML(listEvent.ReqHeroMaxAge))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroMaxAge.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroMaxAge");
            }

            // ReqTrait
            try
            {
                if (!listEvent.ReqHeroTrait.IsStringNoneOrEmpty())
                {
                    int TraitLevel = captive.GetTraitLevel(TraitObject.Find(listEvent.ReqHeroTrait));

                    try
                    {
                        if (listEvent.ReqHeroTraitLevelAbove != null && listEvent.ReqHeroTraitLevelAbove != "")
                        {
                            if (TraitLevel < CEEventLoader.GetIntFromXML(listEvent.ReqHeroTraitLevelAbove))
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelAbove.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove");
                    }
                    try
                    {
                        if (listEvent.ReqHeroTraitLevelBelow != null && listEvent.ReqHeroTraitLevelBelow != "")
                        {
                            if (TraitLevel > CEEventLoader.GetIntFromXML(listEvent.ReqHeroTraitLevelBelow))
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroTraitLevelBelow.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow");
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqTrait");
            }

            // ReqSkill
            try
            {
                if (!listEvent.ReqHeroSkill.IsStringNoneOrEmpty())
                {
                    int SkillLevel = captive.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == listEvent.ReqHeroSkill; }));

                    try
                    {
                        if (!listEvent.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty())
                        {
                            if (SkillLevel < CEEventLoader.GetIntFromXML(listEvent.ReqHeroSkillLevelAbove))
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelAbove.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ReqHeroSkillLevelAbove");
                    }
                    try
                    {
                        if (!listEvent.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty())
                        {
                            if (SkillLevel > CEEventLoader.GetIntFromXML(listEvent.ReqHeroSkillLevelBelow))
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroSkillLevelBelow.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ReqHeroSkillLevelBelow");
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqHeroSkill / Failed ");
            }

            // Health
            try
            {
                if (listEvent.ReqHeroHealthBelowPercentage != null && listEvent.ReqHeroHealthBelowPercentage != "")
                {
                    if (captive.HitPoints > CEEventLoader.GetIntFromXML(listEvent.ReqHeroHealthBelowPercentage))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroHealthBelowPercentage.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroHealthBelowPercentage");
            }
            try
            {
                if (listEvent.ReqHeroHealthAbovePercentage != null && listEvent.ReqHeroHealthAbovePercentage != "")
                {
                    if (captive.HitPoints < CEEventLoader.GetIntFromXML(listEvent.ReqHeroHealthAbovePercentage))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroHealthAbovePercentage.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ReqHeroHealthAbovePercentage");
            }

            // Hero Checks
            if (captive.IsHero && captive.HeroObject != null && (restrictedList.Contains(RestrictedListOfFlags.CaptiveIsHero) || captive.IsPlayerCharacter))
            {
                Hero captiveHero = captive.HeroObject;

                // Hero Checks
                if (captiveHero.IsChild && listEvent.SexualContent)
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. SexualContent Child Detected.";
                }
                if (captiveHero.Children.Count == 0 && restrictedList.Contains(RestrictedListOfFlags.HeroHaveOffspring))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroHaveOffspring.";
                }
                if ((!captiveHero.IsPregnant && !CECampaignBehavior.CheckIfPregnancyExists(captiveHero)) && restrictedList.Contains(RestrictedListOfFlags.HeroIsPregnant))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroIsPregnant.";
                }
                if ((captiveHero.IsPregnant || CECampaignBehavior.CheckIfPregnancyExists(captiveHero)) && restrictedList.Contains(RestrictedListOfFlags.HeroIsNotPregnant))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroIsNotPregnant.";
                }

                if (captiveHero.Spouse == null && restrictedList.Contains(RestrictedListOfFlags.HeroHaveSpouse))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroHaveSpouse.";
                }
                if (captiveHero.Spouse != null && restrictedList.Contains(RestrictedListOfFlags.HeroNotHaveSpouse))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroNotHaveSpouse.";
                }

                if (captiveHero.OwnedCommonAreas.Count == 0 && restrictedList.Contains(RestrictedListOfFlags.HeroOwnsFief))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroOwnsFief.";
                }

                if ((captiveHero.Clan == null || captiveHero != captiveHero.Clan.Leader) && restrictedList.Contains(RestrictedListOfFlags.HeroIsClanLeader))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroIsClanLeader.";
                }

                if (!captiveHero.IsFactionLeader && restrictedList.Contains(RestrictedListOfFlags.HeroIsFactionLeader))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. HeroIsFactionLeader.";
                }

                if (nonRandomBehaviour)
                {
                    // ReqCaptiveHaveItem
                    try
                    {
                        if (!listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty())
                        {
                            ItemObject foundItem = ItemObject.All.FirstOrDefault((ItemObject item) => { return item.StringId == listEvent.ReqHeroPartyHaveItem; });
                            if (foundItem == null)
                            {
                                CECustomHandler.LogToFile("ReqCaptiveHaveItem " + listEvent.ReqHeroPartyHaveItem + " not found for " + listEvent.Name);
                            }
                            else
                            {
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

                                        if (civilianItem != null && civilianItem == foundItem)
                                        {
                                            flagHaveItem = true;
                                            break;
                                        }
                                    }
                                    catch (Exception) { }
                                }

                                if (!flagHaveItem)
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.";
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ReqCaptiveHaveItem");
                    }
                    // Relations
                    try
                    {
                        if (listEvent.ReqHeroCaptorRelationAbove != null && listEvent.ReqHeroCaptorRelationAbove != "" && captorParty.LeaderHero != null)
                        {
                            if (captiveHero.GetRelation(captorParty.LeaderHero) < CEEventLoader.GetFloatFromXML(listEvent.ReqHeroCaptorRelationAbove))
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationAbove.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ReqHeroCaptorRelationAbove");
                    }
                    try
                    {
                        if (listEvent.ReqHeroCaptorRelationBelow != null && listEvent.ReqHeroCaptorRelationBelow != "" && captorParty.LeaderHero != null)
                        {
                            if (captiveHero.GetRelation(captorParty.LeaderHero) > CEEventLoader.GetFloatFromXML(listEvent.ReqHeroCaptorRelationBelow))
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroCaptorRelationBelow.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ReqHeroCaptorRelationBelow");
                    }
                }
                else
                {
                    // ReqHeroHaveItem
                    try
                    {
                        if (!listEvent.ReqHeroPartyHaveItem.IsStringNoneOrEmpty())
                        {
                            bool flagHaveItem = false;
                            ItemObject foundItem = ItemObject.All.FirstOrDefault((ItemObject item) => { return item.StringId == listEvent.ReqHeroPartyHaveItem; });

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

                                        if (civilianItem != null && civilianItem == foundItem)
                                        {
                                            flagHaveItem = true;
                                            break;
                                        }
                                    }
                                    catch (Exception) { }
                                }
                            }

                            if (captorParty.ItemRoster.FindIndexOfItem(foundItem) != -1)
                            {
                                flagHaveItem = true;
                            }

                            if (!flagHaveItem)
                            {
                                return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        CECustomHandler.LogToFile("Missing ReqCaptiveHaveItem");
                    }
                }
            }
            else if (captive.IsHero && restrictedList.Contains(RestrictedListOfFlags.CaptiveIsNonHero) && captive.HeroObject != null)
            {
                return "Skipping event " + listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsNonHero.";
            }
            else if (!captive.IsHero && captive.HeroObject == null && restrictedList.Contains(RestrictedListOfFlags.CaptiveIsHero))
            {
                return "Skipping event " + listEvent.Name + " it does not match the conditions. " + captive.Name + " CaptiveIsHero.";
            }

            // Player Checks
            try
            {
                if (listEvent.ReqGoldAbove != null && listEvent.ReqGoldAbove != "")
                {
                    if (Hero.MainHero.Gold < CEEventLoader.GetIntFromXML(listEvent.ReqGoldAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqGoldAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed ");
            }
            try
            {
                if (listEvent.ReqGoldBelow != null && listEvent.ReqGoldBelow != "")
                {
                    if (Hero.MainHero.Gold > CEEventLoader.GetIntFromXML(listEvent.ReqGoldBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqGoldBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed ");
            }

            // IsOwnedByNotable
            skipFlags = restrictedList.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && restrictedList.Contains(RestrictedListOfFlags.HeroNotOwnedByNotable);
            bool isOwnedFlag = restrictedList.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;
            bool isNotOwnedFlag = !restrictedList.Contains(RestrictedListOfFlags.HeroOwnedByNotable) && !skipFlags;

            if (isOwnedFlag || isNotOwnedFlag)
            {
                if (isOwnedFlag && CECampaignBehavior.extraVariables.Owner == null)
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. isOwnedFlag.";
                }
                else if (isNotOwnedFlag && CECampaignBehavior.extraVariables.Owner != null)
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. isNotOwnedFlag.";
                }
            }

            // Captor Checks
            if (restrictedList.Contains(RestrictedListOfFlags.CaptorIsHero) && captorParty.LeaderHero == null)
            {
                return "Skipping event " + listEvent.Name + " it does not match the conditions. CaptorIsHero.";
            }

            // CaptivesOutNumber
            if (restrictedList.Contains(RestrictedListOfFlags.CaptivesOutNumber) && captorParty.NumberOfPrisoners < captorParty.NumberOfHealthyMembers)
            {
                return "Skipping event " + listEvent.Name + " it does not match the conditions. CaptivesOutNumber.";
            }

            // ReqTroops
            try
            {
                if (!listEvent.ReqTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.NumberOfRegularMembers < CEEventLoader.GetIntFromXML(listEvent.ReqTroopsAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqTroopsAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqTroopsBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.NumberOfRegularMembers > CEEventLoader.GetIntFromXML(listEvent.ReqTroopsBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqTroopsBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed ");
            }

            // ReqMaleTroops
            try
            {
                if (!listEvent.ReqMaleTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < CEEventLoader.GetIntFromXML(listEvent.ReqMaleTroopsAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqMaleTroopsAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqMaleTroopsBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) > CEEventLoader.GetIntFromXML(listEvent.ReqMaleTroopsBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqMaleTroopsBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed ");
            }

            // ReqFemaleTroops
            try
            {
                if (!listEvent.ReqFemaleTroopsAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < CEEventLoader.GetIntFromXML(listEvent.ReqFemaleTroopsAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqFemaleTroopsAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqFemaleTroopsBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.MemberRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > CEEventLoader.GetIntFromXML(listEvent.ReqFemaleTroopsBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqFemaleTroopsBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed ");
            }

            // ReqCaptives
            try
            {
                if (!listEvent.ReqCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.NumberOfPrisoners < CEEventLoader.GetIntFromXML(listEvent.ReqCaptivesAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptivesAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqCaptivesBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.NumberOfPrisoners > CEEventLoader.GetIntFromXML(listEvent.ReqCaptivesBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptivesBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed ");
            }

            // ReqMaleCaptives
            try
            {
                if (!listEvent.ReqMaleCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) < CEEventLoader.GetIntFromXML(listEvent.ReqMaleCaptivesAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqMaleCaptivesAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqMaleCaptivesBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Count(troopRosterElement => { return !troopRosterElement.Character.IsFemale; }) > CEEventLoader.GetIntFromXML(listEvent.ReqMaleCaptivesBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqMaleCaptivesBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed ");
            }

            // ReqFemaleCaptives
            try
            {
                if (!listEvent.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) < CEEventLoader.GetIntFromXML(listEvent.ReqFemaleCaptivesAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.PrisonRoster.Count(troopRosterElement => { return troopRosterElement.Character.IsFemale; }) > CEEventLoader.GetIntFromXML(listEvent.ReqFemaleCaptivesBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqFemaleCaptivesAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed ");
            }

            // ReqMorale
            try
            {
                if (!listEvent.ReqMoraleAbove.IsStringNoneOrEmpty())
                {
                    if (captorParty.IsMobile && captorParty.MobileParty.Morale < CEEventLoader.GetIntFromXML(listEvent.ReqMoraleAbove))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqMoraleAbove.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed ");
            }
            try
            {
                if (!listEvent.ReqMoraleBelow.IsStringNoneOrEmpty())
                {
                    if (captorParty.IsMobile && captorParty.MobileParty.Morale > CEEventLoader.GetIntFromXML(listEvent.ReqMoraleBelow))
                    {
                        return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqMoraleBelow.";
                    }
                }
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed ");
            }

            if (nonRandomBehaviour)
            {
                // ReqCaptorTrait
                try
                {
                    if (!listEvent.ReqCaptorTrait.IsStringNoneOrEmpty())
                    {
                        if (captorParty.LeaderHero == null)
                        {
                            return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptorTrait.";
                        }

                        int TraitLevel = captorParty.LeaderHero.GetTraitLevel(TraitObject.Find(listEvent.ReqCaptorTrait));

                        try
                        {
                            if (!listEvent.ReqCaptorTraitLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (TraitLevel < CEEventLoader.GetIntFromXML(listEvent.ReqCaptorTraitLevelAbove))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelAbove.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove");
                        }
                        try
                        {
                            if (!listEvent.ReqCaptorTraitLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (TraitLevel > CEEventLoader.GetIntFromXML(listEvent.ReqCaptorTraitLevelBelow))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptorTraitLevelBelow.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow");
                        }
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Incorrect ReqCaptorTrait / Failed ");
                }

                // ReqCaptorSkill
                try
                {
                    if (!listEvent.ReqCaptorSkill.IsStringNoneOrEmpty())
                    {
                        if (captorParty.LeaderHero == null)
                        {
                            return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptorSkill.";
                        }

                        int SkillLevel = captorParty.LeaderHero.GetSkillValue(SkillObject.FindFirst((SkillObject skill) => { return skill.StringId == listEvent.ReqCaptorSkill; }));

                        try
                        {
                            if (!listEvent.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel < CEEventLoader.GetIntFromXML(listEvent.ReqCaptorSkillLevelAbove))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelAbove.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove");
                        }
                        try
                        {
                            if (!listEvent.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty())
                            {
                                if (SkillLevel > CEEventLoader.GetIntFromXML(listEvent.ReqCaptorSkillLevelBelow))
                                {
                                    return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqCaptorSkillLevelBelow.";
                                }
                            }
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow");
                        }
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Incorrect ReqCaptorTrait / Failed ");
                }

                // ReqCaptorItem
                try
                {
                    if (!listEvent.ReqCaptorPartyHaveItem.IsStringNoneOrEmpty())
                    {
                        bool flagHaveItem = false;
                        ItemObject foundItem = ItemObject.All.FirstOrDefault((ItemObject item) => { return item.StringId == listEvent.ReqCaptorPartyHaveItem; });

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

                                    if (civilianItem != null && civilianItem == foundItem)
                                    {
                                        flagHaveItem = true;
                                        break;
                                    }
                                }
                                catch (Exception) { }
                            }
                        }

                        if (captorParty.ItemRoster.FindIndexOfItem(foundItem) != -1)
                        {
                            flagHaveItem = true;
                        }

                        if (!flagHaveItem)
                        {
                            return "Skipping event " + listEvent.Name + " it does not match the conditions. ReqHeroPartyHaveItem.";
                        }
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Incorrect ReqCaptorItem / Failed ");
                }

                // Captor Party Gender Checks
                if (captorParty != null && captorParty.Leader != null && captorParty.Leader.IsFemale && restrictedList.Contains(RestrictedListOfFlags.CaptorGenderIsMale))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. CaptorGenderIsMale.";
                }
                if (captorParty != null && captorParty.Leader != null && !captorParty.Leader.IsFemale && restrictedList.Contains(RestrictedListOfFlags.CaptorGenderIsFemale))
                {
                    return "Skipping event " + listEvent.Name + " it does not match the conditions. CaptorGenderIsFemale/Femdom.";
                }
            }

            // Locations and Event Checks
            bool hasCityFlag = restrictedList.Contains(RestrictedListOfFlags.LocationCity);
            bool hasDungeonFlag = restrictedList.Contains(RestrictedListOfFlags.LocationDungeon);
            bool hasVillageFlag = restrictedList.Contains(RestrictedListOfFlags.LocationVillage);
            bool hasHideoutFlag = restrictedList.Contains(RestrictedListOfFlags.LocationHideout);
            bool hasCastleFlag = restrictedList.Contains(RestrictedListOfFlags.LocationCastle);
            bool hasPartyInTownFlag = restrictedList.Contains(RestrictedListOfFlags.LocationPartyInTown);
            bool hasTravellingFlag = restrictedList.Contains(RestrictedListOfFlags.LocationTravellingParty);

            bool visitedByCaravanFlag = restrictedList.Contains(RestrictedListOfFlags.VisitedByCaravan);
            bool visitedByLordFlag = restrictedList.Contains(RestrictedListOfFlags.VisitedByLord);
            bool duringSiegeFlag = restrictedList.Contains(RestrictedListOfFlags.DuringSiege);
            bool duringRaidFlag = restrictedList.Contains(RestrictedListOfFlags.DuringRaid);

            if (hasCityFlag || hasDungeonFlag || hasVillageFlag || hasHideoutFlag || hasTravellingFlag || hasCastleFlag || hasPartyInTownFlag || visitedByCaravanFlag || duringSiegeFlag || duringRaidFlag)
            {
                CEFlag = false;
                if (captorParty != null && captorParty.IsSettlement)
                {
                    if (captorParty.Settlement.IsTown && (hasDungeonFlag || hasCityFlag))
                    {
                        if (visitedByCaravanFlag)
                        {
                            try
                            {
                                CEFlag = (captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Caravan");
                            }
                        }
                        else if (visitedByLordFlag)
                        {
                            try
                            {
                                CEFlag = (captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            CEFlag = true;
                        }
                    }

                    if (hasVillageFlag && captorParty.Settlement.IsVillage)
                    {
                        CEFlag = true;
                    }

                    if (hasHideoutFlag && captorParty.Settlement.IsHideout())
                    {
                        CEFlag = true;
                    }

                    if (hasCastleFlag && captorParty.Settlement.IsCastle)
                    {
                        if (visitedByLordFlag)
                        {
                            try
                            {
                                CEFlag = (captorParty.Settlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            CEFlag = true;
                        }
                    }

                    if (duringSiegeFlag != captorParty.Settlement.IsUnderSiege)
                    {
                        CEFlag = false;
                    }

                    if (duringRaidFlag != captorParty.Settlement.IsUnderRaid)
                    {
                        CEFlag = false;
                    }
                }
                else if (captorParty != null && captorParty.IsMobile && captorParty.MobileParty.CurrentSettlement != null)
                {
                    if (hasPartyInTownFlag && captorParty.MobileParty.CurrentSettlement.IsTown)
                    {
                        if (visitedByCaravanFlag)
                        {
                            try
                            {
                                CEFlag = (captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsCaravan; }) != null);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Caravan");
                            }
                        }
                        else if (visitedByLordFlag)
                        {
                            try
                            {
                                CEFlag = (captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            CEFlag = true;
                        }
                    }

                    if (hasVillageFlag && captorParty.MobileParty.CurrentSettlement.IsVillage)
                    {
                        CEFlag = true;
                    }

                    if (hasCastleFlag && captorParty.MobileParty.CurrentSettlement.IsCastle)
                    {
                        if (visitedByLordFlag)
                        {
                            try
                            {
                                CEFlag = (captorParty.MobileParty.CurrentSettlement.Parties.FirstOrDefault(mobileParty => { return mobileParty.IsLordParty; }) != null);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.LogToFile("Failed to get Lord Party");
                            }
                        }
                        else
                        {
                            CEFlag = true;
                        }
                    }

                    if (hasHideoutFlag && captorParty.MobileParty.CurrentSettlement.IsHideout())
                    {
                        CEFlag = true;
                    }

                    if (duringSiegeFlag != captorParty.MobileParty.CurrentSettlement.IsUnderSiege)
                    {
                        CEFlag = false;
                    }

                    if (duringRaidFlag != captorParty.MobileParty.CurrentSettlement.IsUnderRaid)
                    {
                        CEFlag = false;
                    }
                }
                else if (hasTravellingFlag)
                {
                    if (captorParty.IsMobile)
                    {
                        CEFlag = true;

                        if (duringSiegeFlag != (captorParty.MobileParty.BesiegerCamp != null))
                        {
                            CEFlag = false;
                        }

                        bool raidingEvent = captorParty.MapEvent != null && captorParty.MapEvent.IsRaid && captorParty.MapFaction.IsAtWarWith(captorParty.MapEvent.MapEventSettlement.MapFaction) && captorParty.MapEvent.DefenderSide.TroopCount == 0;
                        if (duringRaidFlag != raidingEvent)
                        {
                            CEFlag = false;
                        }
                    }
                }
            }

            if (!CEFlag)
            {
                return "Skipping event " + listEvent.Name + " it does not match the location conditions.";
            }

            // Time Checks
            bool hasNightFlag = restrictedList.Contains(RestrictedListOfFlags.TimeNight);
            bool hasDayFlag = restrictedList.Contains(RestrictedListOfFlags.TimeDay);

            if (hasNightFlag || hasDayFlag)
            {
                CEFlag = false;
                if (hasNightFlag && Campaign.Current.IsNight)
                {
                    CEFlag = true;
                }
                if (hasDayFlag && Campaign.Current.IsDay)
                {
                    CEFlag = true;
                }
            }

            if (!CEFlag)
            {
                return "Skipping event " + listEvent.Name + " it does not match the time conditions.";
            }

            // Seasons Checks
            bool hasWinterFlag = restrictedList.Contains(RestrictedListOfFlags.SeasonWinter);
            bool hasSummerFlag = restrictedList.Contains(RestrictedListOfFlags.SeasonSpring);
            bool hasSpringFlag = restrictedList.Contains(RestrictedListOfFlags.SeasonSummer);
            bool hasFallFlag = restrictedList.Contains(RestrictedListOfFlags.SeasonFall);

            if (hasWinterFlag || hasSummerFlag)
            {
                CEFlag = false;
                if (hasSummerFlag && CampaignTime.Now.GetSeasonOfYear == 1)
                {
                    CEFlag = true;
                }
                if (hasFallFlag && CampaignTime.Now.GetSeasonOfYear == 2)
                {
                    CEFlag = true;
                }
                if (hasWinterFlag && CampaignTime.Now.GetSeasonOfYear == 3)
                {
                    CEFlag = true;
                }
                if (hasSpringFlag && (CampaignTime.Now.GetSeasonOfYear == 4 || CampaignTime.Now.GetSeasonOfYear == 0))
                {
                    CEFlag = true;
                }
            }

            if (!CEFlag)
            {
                return "Skipping event " + listEvent.Name + " it does not match the seasons conditions.";
            }

            listEvent.Captive = captive;

            return null;
        }
    }
}