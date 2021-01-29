#define BETA
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;
#if BETA
using TaleWorlds.ModuleManager;
#endif 
using Path = System.IO.Path;

namespace CaptivityEvents.Helper
{
    public class CEHelper
    {
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

        internal static List<string> GetModulePaths(string[] modulesFound, out List<ModuleInfo> modules)
        {
            List<string> modulePaths = new List<string>();

            List<ModuleInfo> findingModules = new List<ModuleInfo>();

            foreach (string moduleID in modulesFound)
            {
                try
                {

#if BETA
                    ModuleInfo moduleInfo = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);
#else
                    ModuleInfo moduleInfo = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);
#endif

                    if (moduleInfo != null && !moduleInfo.DependedModules.Exists(item => item.ModuleId == "zCaptivityEvents")) continue;

                    try
                    {
                        if (moduleInfo == null) continue;
                        CECustomHandler.ForceLogToFile("Added to ModuleLoader: " + moduleInfo.Name);
#if BETA
                        modulePaths.Insert(0, Path.GetDirectoryName(ModuleHelper.GetPath(moduleInfo.Id)));
#else
                        modulePaths.Insert(0, Path.GetDirectoryName(ModuleInfo.GetPath(moduleInfo.Id)));
#endif

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