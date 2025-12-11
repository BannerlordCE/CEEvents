using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using Path = System.IO.Path;

namespace CaptivityEvents.Helper
{
    public class CEHelper
    {

        private static string CurrentLocation(Settlement settlement)
        {
            return settlement.IsCastle ? "castle" : settlement.IsTown ? "town" : "village";
        }

        private static readonly string[] _validCultures = ["aserai", "battania", "empire", "khuzait", "sturgia", "vlandia"];

        public static string CustomSceneToPlay(string sceneToPlay, PartyBase partyBase)
        {
            if (sceneToPlay.Contains("$location_culture"))
            {
                string locationCulture = "empire";
                try
                {
                    locationCulture = partyBase.Settlement?.Culture?.StringId ?? SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition()).Culture.StringId;

                    if (!_validCultures.Contains(locationCulture)) locationCulture = "empire";
                }
                catch (Exception)
                {
                    locationCulture = "empire";
                }
                sceneToPlay = sceneToPlay.Replace("$location_culture", locationCulture);
            }

            if (sceneToPlay.Contains("$party_culture"))
            {
                string partyCulture = partyBase.Culture?.StringId ?? "empire";
                if (!_validCultures.Contains(partyCulture)) partyCulture = "empire";
                sceneToPlay = sceneToPlay.Replace("$party_culture", partyCulture);
            }

            if (sceneToPlay.Contains("$location"))
            {
                sceneToPlay = sceneToPlay.Replace("$location", CurrentLocation(partyBase.Settlement ?? SettlementHelper.FindNearestSettlementToPoint(Hero.MainHero.GetCampaignPosition(), (Settlement x) => true)));
            }

            string[] e =
            [
                "01",
                "02",
                "03"
            ];

            if (sceneToPlay.Contains("$randomize"))
            {
                sceneToPlay = sceneToPlay.Replace("$randomize", e.GetRandomElement());
            }

            return sceneToPlay.ToLower();

        }

        public static string CustomSceneToPlay(string sceneToPlay, Settlement settlement)
        {
            if (sceneToPlay.Contains("$location_culture"))
            {
                string locationCulture = settlement.Culture?.StringId ?? "empire";
                if (!_validCultures.Contains(locationCulture)) locationCulture = "empire";
                sceneToPlay = sceneToPlay.Replace("$location_culture", locationCulture);
            }

            if (sceneToPlay.Contains("$location"))
            {
                sceneToPlay = sceneToPlay.Replace("$location", CurrentLocation(settlement));
            }

            string[] e =
            [
                "01",
                "02",
                "03"
            ];

            if (sceneToPlay.Contains("$randomize"))
            {
                sceneToPlay = sceneToPlay.Replace("$randomize", e.GetRandomElement());
            }

            return sceneToPlay.ToLower();

        }

        public static void AddQuickInformation(TextObject message, int priorty = 0, BasicCharacterObject announcerCharacter = null, string soundEventPath = "", Equipment equipment = null)
        {
            MBInformationManager.AddQuickInformation(message, priorty, announcerCharacter, equipment, soundEventPath);
        }

        public static int HelperMBRandom()
        {
            return MBRandom.RandomInt();
        }

        public static int HelperMBRandom(int maxValue)
        {
            return MBRandom.RandomInt(maxValue);
        }

        public static int HelperMBRandom(int minValue, int maxValue)
        {
            return MBRandom.RandomInt(minValue, maxValue);
        }

        public enum EquipmentCustomIndex
        {
            Weapon0 = 0,
            Weapon1 = 1,
            Weapon2 = 2,
            Weapon3 = 3,
            Weapon4 = 4,
            Head = 5,
            Body = 6,
            Leg = 7,
            Gloves = 8,
            Cape = 9,
            Horse = 10,
            HorseHarness = 11,
        }

        public static Hero spouseOne = null;
        public static Hero spouseTwo = null;
        public static bool brothelFlagFemale = false;
        public static bool brothelFlagMale = false;
        public static int waitMenuCheck = -1;

        public static bool notificationCaptorExists = false;
        public static bool notificationCaptorCheck = false;
        public static bool notificationEventExists = false;
        public static bool notificationEventCheck = false;

        public static bool progressEventExists = false;
        public static bool progressEventCheck = false;

        public static List<CEDelayedEvent> delayedEvents = [];

        internal static void SetSkillValue(Hero hero, SkillObject skillObject, int value)
        {
            MethodInfo mi = typeof(Hero).GetMethod("SetSkillValueInternal", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (mi == null)
            {
                hero.HeroDeveloper.SetInitialSkillLevel(skillObject, value);
            }
            else
            {
                mi.Invoke(hero, [skillObject, value]);
            }
        }

        internal static void AddDelayedEvent(CEDelayedEvent delayedEvent)
        {
            if (!delayedEvents.Any(item => item.eventName == delayedEvent.eventName))
            {
                delayedEvents.Add(delayedEvent);
            }
        }

        internal static List<string> GetModulePaths(string[] modulesFound, out List<ModuleInfo> modules)
        {
            List<string> modulePaths = [];

            List<ModuleInfo> findingModules = [];

            foreach (string moduleID in modulesFound)
            {
                try
                {
                    ModuleInfo moduleInfo = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);

                    if (moduleInfo != null && !moduleInfo.DependedModules.Exists(item => item.ModuleId == "zCaptivityEvents")) continue;

                    try
                    {
                        if (moduleInfo == null) continue;
                        CECustomHandler.ForceLogToFile("Added to ModuleLoader: " + moduleInfo.Name);
                        modulePaths.Insert(0, Path.GetDirectoryName(ModuleHelper.GetPath(moduleInfo.Id)));

                        findingModules.Add(moduleInfo);
                    }
                    catch (Exception)
                    {
                        if (moduleInfo != null) CECustomHandler.ForceLogToFile("Failed to Load " + moduleInfo.Name + " Events");
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile("Failed to fetch DependedModuleIds from " + moduleID);
                }
            }

            modules = findingModules;

            return modulePaths;
        }

        internal static void ChangeMenu(int number)
        {
            string waitingList = WaitingList.CEWaitingList();
            if (waitingList != null) SafeSwitchToMenu(waitingList);
            waitMenuCheck = number;
        }

        /// <summary>
        /// Safely switches to a menu after validating it exists in the GameMenuManager.
        /// </summary>
        /// <param name="menuName">The name of the menu to switch to</param>
        /// <returns>True if the menu switch was successful, false otherwise</returns>
        public static bool SafeSwitchToMenu(string menuName)
        {
            try
            {
                if (Campaign.Current?.GameMenuManager?.GetGameMenu(menuName) != null)
                {
                    GameMenu.SwitchToMenu(menuName);
                    return true;
                }
                else
                {
                    CECustomHandler.ForceLogToFile($"SafeSwitchToMenu failed: Menu '{menuName}' does not exist in GameMenuManager. This event may not have been properly registered.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile($"SafeSwitchToMenu exception for menu '{menuName}': {ex}");
                return false;
            }
        }

        /// <summary>
        /// Safely activates a menu after validating it exists in the GameMenuManager.
        /// </summary>
        /// <param name="menuName">The name of the menu to activate</param>
        /// <returns>True if the menu activation was successful, false otherwise</returns>
        public static bool SafeActivateGameMenu(string menuName)
        {
            try
            {
                if (Campaign.Current?.GameMenuManager?.GetGameMenu(menuName) != null)
                {
                    GameMenu.ActivateGameMenu(menuName);
                    return true;
                }
                else
                {
                    CECustomHandler.ForceLogToFile($"SafeActivateGameMenu failed: Menu '{menuName}' does not exist in GameMenuManager. This event may not have been properly registered.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile($"SafeActivateGameMenu exception for menu '{menuName}': {ex}");
                return false;
            }
        }


        internal static TextObject ShouldChangeMenu(TextObject text)
        {
            if (PlayerCaptivity.CaptorParty.IsMobile && PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement != null)
            {
                Settlement current = PlayerCaptivity.CaptorParty.MobileParty.CurrentSettlement;
                switch (waitMenuCheck)
                {
                    case 2:
                        if (!(current.IsUnderSiege || current.IsUnderRaid))
                        {
                            ChangeMenu(3);
                        }
                        break;

                    case 3:
                        if (current.IsUnderSiege || current.IsUnderRaid)
                        {
                            ChangeMenu(2);
                        }
                        break;

                    default:
                        ChangeMenu(3);
                        break;
                }
                text.SetTextVariable("SETTLEMENT_NAME", current.Name);
            }
            else if (PlayerCaptivity.CaptorParty.IsSettlement)
            {
                Settlement current = PlayerCaptivity.CaptorParty.Settlement;
                switch (waitMenuCheck)
                {
                    case 2:
                        if (!(current.IsUnderSiege || current.IsUnderRaid))
                        {
                            ChangeMenu(3);
                        }
                        break;

                    case 3:
                        if (current.IsUnderSiege || current.IsUnderRaid)
                        {
                            ChangeMenu(2);
                        }
                        break;

                    default:
                        ChangeMenu(3);
                        break;
                }

                text.SetTextVariable("SETTLEMENT_NAME", PlayerCaptivity.CaptorParty.Settlement.Name);
            }
            else
            {
                if (waitMenuCheck != 1) ChangeMenu(1);
                text.SetTextVariable("PARTY_NAME", PlayerCaptivity.CaptorParty.Name);
            }

            return text;
        }


        static Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .Where(alz =>
                    {
                        return !alz.GetName().ToString().StartsWith("TaleWorlds")
                            && !alz.GetName().ToString().StartsWith("System")
                            && !alz.GetName().ToString().StartsWith("Microsoft")
                            && !alz.GetName().ToString().StartsWith("mscorlib")
                            && !alz.GetName().ToString().StartsWith("SandBox")
                            && !alz.GetName().ToString().StartsWith("Native")
                            && !alz.GetName().ToString().StartsWith("CustomBattle")
                            && !alz.GetName().ToString().StartsWith("Bannerlord");
                    })
                    .ToArray();
        }

        public static bool CheckAssemblies(string v)
        {
            Assembly[] captivityAssemblies = GetAssemblies().Where(azc =>
            {
                return azc.GetName().ToString().ToLower().StartsWith(v.ToLower())
                ;
            }).ToArray();
            if (captivityAssemblies.Length > 0)
            {
                return true;
            }
            return false;
        }

        public static bool CheckHotButter()
        {

            var HotButter = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id.ToLower().StartsWith("hotbutterscenes"); });
            if (HotButter != null)
            {
                return true; // CheckAssemblies("captivityevents");
            }
            return CheckAssemblies("hotbutter");

        }

        public static CampaignVec2 GetPlayerPositionClean()
        {
            return PlayerCaptivity.CaptorParty != null ? PlayerCaptivity.CaptorParty.Position : Campaign.Current.CameraFollowParty.Position;
        }

        public static CampaignVec2 GetSpawnPositionAroundSettlement(Settlement settlement)
        {
            CampaignVec2 campaignVec = NavigationHelper.FindPointAroundPosition(settlement.GatePosition, MobileParty.NavigationType.Default, 5f, 0f, true, false);
            float num = MobileParty.MainParty.SeeingRange * MobileParty.MainParty.SeeingRange;
            if (campaignVec.DistanceSquared(MobileParty.MainParty.Position) < num)
            {
                for (int i = 0; i < 15; i++)
                {
                    CampaignVec2 campaignVec2 = NavigationHelper.FindReachablePointAroundPosition(campaignVec, MobileParty.NavigationType.Default, 5f, 0f, false);
                    if (NavigationHelper.IsPositionValidForNavigationType(campaignVec2, MobileParty.NavigationType.Default))
                    {
                        float num2 = DistanceHelper.FindClosestDistanceFromMobilePartyToPoint(MobileParty.MainParty, campaignVec2, MobileParty.NavigationType.Default, out _);
                        if (num2 * num2 > num)
                        {
                            campaignVec = campaignVec2;
                            break;
                        }
                    }
                }
            }
            return campaignVec;
        }
    }
}