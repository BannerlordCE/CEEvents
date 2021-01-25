﻿using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using Path = System.IO.Path;

namespace CaptivityEvents.Helper
{
    public class CEHelper
    {
        public static Hero spouseOne = null;
        public static Hero spouseTwo = null;
        public static bool brothelFlagFemale = false;
        public static bool brothelFlagMale = false;
        public static bool settlementCheck = false;

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
                    ModuleInfo moduleInfo = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);

                    // 1.5.5
                    //if (moduleInfo != null && !moduleInfo.DependedModuleIds.Contains("zCaptivityEvents")) continue;

                    // 1.5.6
                    if (moduleInfo != null && !moduleInfo.DependedModules.Exists(item => item.ModuleId == "zCaptivityEvents")) continue;

                    try
                    {
                        if (moduleInfo == null) continue;
                        CECustomHandler.ForceLogToFile("Added to ModuleLoader: " + moduleInfo.Name);
                        modulePaths.Insert(0, Path.GetDirectoryName(ModuleInfo.GetPath(moduleInfo.Id)));
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
    }
}