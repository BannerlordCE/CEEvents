#define BETA
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using Path = System.IO.Path;

namespace CaptivityEvents.Helper
{
    public class CEHelper
    {

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

        public static List<CEDelayedEvent> delayedEvents = new List<CEDelayedEvent>();

        internal static void SetSkillValue(Hero hero, SkillObject skillObject, int value)
        {
#if BETA
            MethodInfo mi = typeof(Hero).GetMethod("SetSkillValueInternal", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (mi == null)
            {
                hero.HeroDeveloper.SetInitialSkillLevel(skillObject, value);
            }
            else
            {
                mi.Invoke(hero, new object[] { skillObject, value });
            }
#else
            hero.SetSkillValue(skillObject, value);
#endif
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
            List<string> modulePaths = new List<string>();

            List<ModuleInfo> findingModules = new List<ModuleInfo>();

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
            string waitingList = new WaitingList().CEWaitingList();
            if (waitingList != null) GameMenu.SwitchToMenu(waitingList);
            waitMenuCheck = number;
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
    }
}