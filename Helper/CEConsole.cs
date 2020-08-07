using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using Path = System.IO.Path;
using Texture = TaleWorlds.TwoDimension.Texture;

namespace CaptivityEvents.Helper
{
    internal class CEConsole
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("force_fire_event", "captivity")]
        public static string ForceFireEvent(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckParameters(strings, 0) && CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.force_fire_event [EventName] [CaptiveName]\".";
                bool flag = false;

                string eventName = "";
                string heroName = null;

                if (CampaignCheats.CheckParameters(strings, 1))
                {
                    eventName = strings[0];

                    if (eventName.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }
                else if (CampaignCheats.CheckParameters(strings, 2))
                {
                    eventName = strings[0];
                    heroName = strings[1];

                    if (eventName.IsStringNoneOrEmpty() || heroName.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }

                if (!flag) return "Wrong input.\nFormat is \"captivity.force_fire_event [EventName] [CaptiveName]\".";

                string result;

                if (PlayerCaptivity.IsCaptive)
                {
                    result = CEEventManager.FireSpecificEvent(eventName, true);

                    switch (result)
                    {
                        case "$FAILEDTOFIND":
                            return "Failed to load event list.";

                        case "$EVENTNOTFOUND":
                            return "Event not found.";

                        case "$EVENTCONDITIONSNOTMET":
                            return "Event conditions are not met.";

                        default:
                            if (result.StartsWith("$")) return result.Substring(1);

                            if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptive)
                            {
                                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                if (!mapStateCaptive.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                GameMenu.SwitchToMenu(result);

                                return "Successfully force launched event.";
                            }
                            else
                            {
                                return "Failed to launch event, incorrect game state.";
                            }
                    }
                }

                CEEvent ceEvent = null;
                result = CEEventManager.FireSpecificEventRandom(eventName, out ceEvent, true);

                switch (result)
                {
                    case "$FAILEDTOFIND":
                        return "Failed to load event list.";

                    case "$EVENTNOTFOUND":
                    case "$EVENTCONDITIONSNOTMET":
                        if (PartyBase.MainParty.NumberOfPrisoners > 0)
                        {
                            result = CEEventManager.FireSpecificEventPartyLeader(eventName, out ceEvent, true, heroName);

                            switch (result)
                            {
                                case "$FAILEDTOFIND":
                                    return "Failed to load event list.";

                                case "$FAILTOFINDHERO":
                                    return "Failed to find specified captive in party : " + heroName;

                                case "$EVENTNOTFOUND":
                                    return "Event not found.";

                                case "$EVENTCONDITIONSNOTMET":
                                    return "No captives meet the event conditions.";

                                default:
                                    if (result.StartsWith("$")) return result.Substring(1);

                                    if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptor)
                                    {
                                        Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                        if (!mapStateCaptor.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                        GameMenu.SwitchToMenu(result);

                                        return "Successfully force launched event.";
                                    }
                                    else
                                    {
                                        return "Failed to launch event, incorrect game state.";
                                    }
                            }
                        }
                        else
                        {
                            return "Please add more prisoners to your party.\n\"Format is \"campaign.add_prisoner [PositiveNumber] [TroopName]\".";
                        }
                    default:
                        if (result.StartsWith("$")) return result.Substring(1);

                        if (Game.Current.GameStateManager.ActiveState is MapState mapStateRandom)
                        {
                            Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                            if (!mapStateRandom.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                            GameMenu.SwitchToMenu(result);

                            return "Successfully force launched event.";
                        }
                        else
                        {
                            return "Failed to launch event, incorrect game state.";
                        }
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        private static void LaunchCaptorEvent(CEEvent returnedEvent)
        {
            if (CEHelper.notificationCaptorExists) return;

            if (returnedEvent == null) return;
            CEHelper.notificationCaptorExists = true;

            try
            {
                if (!returnedEvent.NotificationName.IsStringNoneOrEmpty()) new CESubModule().LoadCampaignNotificationTexture(returnedEvent.NotificationName);
                else if (returnedEvent.SexualContent) new CESubModule().LoadCampaignNotificationTexture("CE_sexual_notification");
                else new CESubModule().LoadCampaignNotificationTexture("CE_castle_notification");
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));

                CECustomHandler.ForceLogToFile("LoadCampaignNotificationTexture");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }

            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new CECaptorMapNotification(returnedEvent, new TextObject("{=CEEVENTS1090}Captor event is ready")));
        }

        private static void LaunchRandomEvent(CEEvent returnedEvent)
        {
            if (CEHelper.notificationEventExists) return;

            if (returnedEvent == null) return;
            CEHelper.notificationEventExists = true;

            try
            {
                if (!returnedEvent.NotificationName.IsStringNoneOrEmpty()) new CESubModule().LoadCampaignNotificationTexture(returnedEvent.NotificationName, 1);
                else if (returnedEvent.SexualContent) new CESubModule().LoadCampaignNotificationTexture("CE_random_sexual_notification", 1);
                else new CESubModule().LoadCampaignNotificationTexture("CE_random_notification", 1);
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("LoadCampaignNotificationTextureFailure", Colors.Red));

                CECustomHandler.ForceLogToFile("LoadCampaignNotificationTexture");
                CECustomHandler.ForceLogToFile(e.Message + " : " + e);
            }

            Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new CEEventMapNotification(returnedEvent, new TextObject("{=CEEVENTS1059}Random event is ready")));
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("fire_event", "captivity")]
        public static string FireEvent(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckParameters(strings, 0) && CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.fire_ceevent [EventName] [CaptiveName]\".";

                bool flag = false;

                string eventName = "";
                string heroName = null;

                if (CampaignCheats.CheckParameters(strings, 1))
                {
                    eventName = strings[0];

                    if (eventName.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }
                else if (CampaignCheats.CheckParameters(strings, 2))
                {
                    eventName = strings[0];
                    heroName = strings[1];

                    if (eventName.IsStringNoneOrEmpty() || heroName.IsStringNoneOrEmpty()) return "Wrong input.\nFormat is \"captivity.fire_event [EventName] [CaptiveName]\".";

                    flag = true;
                }

                if (!flag) return "Wrong input.\nFormat is \"captivity.fire_ceevent [EventName] [CaptiveName]\".";
                string result;

                if (PlayerCaptivity.IsCaptive)
                {
                    result = CEEventManager.FireSpecificEvent(eventName);

                    switch (result)
                    {
                        case "$FAILEDTOFIND":
                            return "Failed to load event list.";

                        case "$EVENTNOTFOUND":
                            return "Event not found.";

                        case "$EVENTCONDITIONSNOTMET":
                            return "Event conditions are not met.";

                        default:
                            if (result.StartsWith("$")) return result.Substring(1);

                            if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptive)
                            {
                                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                                if (!mapStateCaptive.AtMenu)
                                {
                                    GameMenu.ActivateGameMenu("prisoner_wait");
                                }
                                else
                                {
                                    CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapStateCaptive.GameMenuId;
                                    CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapStateCaptive.MenuContext.CurrentBackgroundMeshName;
                                }

                                GameMenu.SwitchToMenu(result);

                                return "Successfully launched event.";
                            }
                            else
                            {
                                return "Failed to launch event, incorrect game state.";
                            }
                    }
                }

                CEEvent returnedEvent = null;

                result = CEEventManager.FireSpecificEventRandom(eventName, out returnedEvent);

                switch (result)
                {
                    case "$FAILEDTOFIND":
                        return "Failed to load event list.";

                    case "$EVENTNOTFOUND":
                    case "$EVENTCONDITIONSNOTMET":
                        if (PartyBase.MainParty.NumberOfPrisoners > 0)
                        {
                            result = CEEventManager.FireSpecificEventPartyLeader(eventName, out returnedEvent, false, heroName);

                            switch (result)
                            {
                                case "$FAILEDTOFIND":
                                    return "Failed to load event list.";

                                case "$FAILTOFINDHERO":
                                    return "Failed to find specified captive in party : " + heroName;

                                case "$EVENTNOTFOUND":
                                    return "Event not found.";

                                case "$EVENTCONDITIONSNOTMET":
                                    return "No captives meet the event conditions.";

                                default:
                                    if (result.StartsWith("$")) return result.Substring(1);

                                    if (Game.Current.GameStateManager.ActiveState is MapState mapStateCaptor)
                                    {
                                        if (CESettings.Instance.EventCaptorNotifications)
                                        {
                                            LaunchCaptorEvent(returnedEvent);
                                        }
                                        else
                                        {
                                            Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                            if (!mapStateCaptor.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                            GameMenu.SwitchToMenu(result);
                                        }

                                        return "Successfully launched event.";
                                    }
                                    else
                                    {
                                        return "Failed to launch event, incorrect game state.";
                                    }
                            }
                        }
                        else
                        {
                            return "Please add more prisoners to your party.\n\"Format is \"campaign.add_prisoner [PositiveNumber] [TroopName]\".";
                        }
                    default:
                        if (result.StartsWith("$")) return result.Substring(1);

                        if (Game.Current.GameStateManager.ActiveState is MapState mapStateRandom)
                        {
                            if (CESettings.Instance.EventCaptorNotifications)
                            {
                                LaunchRandomEvent(returnedEvent);
                            }
                            else
                            {
                                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                                if (!mapStateRandom.AtMenu) GameMenu.ActivateGameMenu("prisoner_wait");

                                GameMenu.SwitchToMenu(result);
                            }

                            return "Successfully launched event.";
                        }
                        else
                        {
                            return "Failed to launch event, incorrect game state.";
                        }
                }

                //return "Wrong input.\nFormat is \"captivity.fire_ceevent [EventName] [CaptiveName]\".";  //Warning: unreachable
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("list_events", "captivity")]
        public static string ListEvents(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.list_events [SEARCH_TERM]\".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                if (CEPersistence.CEEvents == null || CEPersistence.CEEvents.Count <= 0) return "Failed to load event list.";
                string text = "";
                bool searchActive = !searchTerm.IsStringNoneOrEmpty();

                if (searchActive) searchTerm = searchTerm.ToLower();

                foreach (CEEvent ceEvent in CEPersistence.CEEvents)
                {
                    if (searchActive)
                    {
                        if (ceEvent.Name.ToLower().IndexOf(searchTerm, StringComparison.Ordinal) != -1) text = text + ceEvent.Name + "\n";
                    }
                    else
                    {
                        text = text + ceEvent.Name + "\n";
                    }
                }

                return text;

            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("current_status", "captivity")]
        public static string CurrentStatus(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.current_status [SEARCH_HERO]\".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                Hero hero = searchTerm.IsStringNoneOrEmpty()
                    ? Hero.MainHero
                    : Campaign.Current.Heroes.FirstOrDefault(heroToFind => heroToFind.Name.ToString() == searchTerm);

                return hero == null
                    ? "Hero not found."
                    : CEEventChecker.CheckFlags(hero.CharacterObject, PlayerCaptivity.CaptorParty);
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("debug_status", "captivity")]
        public static string Debug(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.debug_status\".";

                string debug = "";

                debug += "Notification Status:\nCaptor Exists: " + CEHelper.notificationCaptorExists + "\nRandom Exists: " + CEHelper.notificationEventExists;


                return debug;
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reset_status", "captivity")]
        public static string ResetStatus(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.reset_status [SEARCH_HERO]\".";

                string searchTerm = null;

                if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0];

                Hero hero = searchTerm.IsStringNoneOrEmpty()
                    ? Hero.MainHero
                    : Campaign.Current.Heroes.FirstOrDefault(heroToFind => { return heroToFind.Name.ToString() == searchTerm; });

                if (hero == null) return "Hero not found.";

                try
                {
                    Dynamics d = new Dynamics();
                    d.VictimProstitutionModifier(0, hero, true);
                    d.VictimProstitutionModifier(0, hero, false, false);
                    d.VictimSlaveryModifier(0, hero, true);
                    d.VictimSlaveryModifier(0, hero, false, false);
                    CECampaignBehavior.ExtraProps.ResetVariables();

                    return "Successfully reset status";
                }
                catch (Exception e)
                {
                    return "Failed : " + e;
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("clear_pregnancies", "captivity")]
        public static string ClearPregnancies(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.clear_pregnancies \".";

                //string searchTerm = null;
                //if (CampaignCheats.CheckParameters(strings, 1)) searchTerm = strings[0]; //Warning, not accessed

                try
                {
                    bool successful = CECampaignBehavior.ClearPregnancyList();

                    return successful
                        ? "Successfully cleared pregnancies of Captivity Events"
                        : "Failed to Clear";
                }
                catch (Exception e)
                {
                    return "Failed : " + e;
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("clean_save", "captivity")]
        public static string CleanSave(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.clean_save \".";

                try
                {
                    bool successful = CECampaignBehavior.ClearPregnancyList();
                    CEBrothelBehavior.CleanList();
                    ResetStatus(new List<string>());

                    return successful
                        ? "Successfully cleaned save of captivity events data. Save the game now."
                        : "Failed to Clean";
                }
                catch (Exception e)
                {
                    return "Failed : " + e;
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("fire_fix", "captivity")]
        public static string FireFix(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.fire_fix \".";

                try
                {
                    Hero.MainHero.Children.ForEach(child =>
                    {
                        child.Clan = Hero.MainHero.Clan;
                        if (child.CharacterObject.Occupation != Occupation.Lord)
                        {
                            PropertyInfo fi = child.CharacterObject.GetType().GetProperty("Occupation", BindingFlags.Instance | BindingFlags.Public);
                            if (fi != null) fi.SetValue(child.CharacterObject, Occupation.Lord);
                        }
                    });

                    return "Successfully fixed";
                }
                catch (Exception e)
                {
                    return "Failed : " + e;
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reload_images", "captivity")]
        public static string ReloadImages(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.reload_images \".";

                try
                {
                    string[] modulesFound = Utilities.GetModulesNames();
                    List<string> modulePaths = new List<string>();

                    CECustomHandler.ForceLogToFile("\n -- Loaded Modules -- \n" + string.Join("\n", modulesFound));

                    foreach (string moduleID in modulesFound)
                    {
                        try
                        {
                            ModuleInfo moduleInfo = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);

                            if (moduleInfo != null && !moduleInfo.DependedModuleIds.Contains("zCaptivityEvents")) continue;

                            try
                            {
                                if (moduleInfo == null) continue;
                                modulePaths.Insert(0, Path.GetDirectoryName(ModuleInfo.GetPath(moduleInfo.Id)));
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

                    // Load Images
                    string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/";
                    string requiredPath = fullPath + "CaptivityRequired";

                    // Get Required
                    string[] requiredImages = Directory.EnumerateFiles(requiredPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

                    // Get All in ModuleLoader
                    string[] files = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();


                    CEPersistence.CEEventImageList.Clear();

                    // Module Image Load
                    if (modulePaths.Count != 0)
                    {
                        foreach (string filepath in modulePaths)
                        {
                            try
                            {
                                string[] moduleFiles = Directory.EnumerateFiles(filepath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

                                foreach (string file in moduleFiles)
                                {
                                    if (!CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                                    {
                                        try
                                        {
                                            TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                            texture.PreloadTexture();
                                            Texture texture2D = new Texture(new EngineTexture(texture));
                                            CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                                        }
                                        catch (Exception e)
                                        {
                                            CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                                        }
                                    }
                                    else CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                                }
                            }
                            catch (Exception) { }
                        }
                    }

                    // Captivity Location Image Load
                    try
                    {

                        foreach (string file in files)
                        {
                            if (requiredImages.Contains(file)) continue;

                            if (!CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                            {
                                try
                                {
                                    TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                    texture.PreloadTexture();
                                    Texture texture2D = new Texture(new EngineTexture(texture));
                                    CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                                }
                                catch (Exception e)
                                {
                                    CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                                }
                            }
                            else CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                        }

                        foreach (string file in requiredImages)
                        {
                            if (CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file))) continue;

                            try
                            {
                                TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                texture.PreloadTexture();
                                Texture texture2D = new Texture(new EngineTexture(texture));
                                CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                            }
                            catch (Exception e)
                            {
                                CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                            }
                        }

                        new CESubModule().LoadTexture("default", false, true);
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failure to load textures, Critical failure. " + e);
                    }

                    CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CEEventImageList.Count + " images.");

                    return "Loaded " + CEPersistence.CEEventImageList.Count + " images.";
                }
                catch (Exception e)
                {
                    return "Failed : " + e;
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reload_events", "captivity")]
        public static string ReloadEvents(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (CampaignCheats.CheckHelp(strings)) return "Format is \"captivity.reload_events \".";

                try
                {
                    string[] modulesFound = Utilities.GetModulesNames();
                    List<string> modulePaths = new List<string>();

                    CECustomHandler.ForceLogToFile("\n -- Loaded Modules -- \n" + string.Join("\n", modulesFound));

                    foreach (string moduleID in modulesFound)
                    {
                        try
                        {
                            ModuleInfo moduleInfo = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);

                            if (moduleInfo != null && !moduleInfo.DependedModuleIds.Contains("zCaptivityEvents")) continue;

                            try
                            {
                                if (moduleInfo == null) continue;
                                modulePaths.Insert(0, Path.GetDirectoryName(ModuleInfo.GetPath(moduleInfo.Id)));
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

                    // Events Removing
                    MethodInfo mi = Campaign.Current.GameMenuManager.GetType().GetMethod("RemoveRelatedGameMenus", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (mi != null) mi.Invoke(Campaign.Current.GameMenuManager, new object[] { "CEEVENTS" });

                    // Unload 
                    CEPersistence.CEEvents.Clear();
                    CEPersistence.CEEventList.Clear();
                    CEPersistence.CEWaitingList.Clear();
                    CEPersistence.CECallableEvents.Clear();

                    // Load Events
                    CEPersistence.CEEvents = CECustomHandler.GetAllVerifiedXSEFSEvents(modulePaths);

                    CEHelper.brothelFlagFemale = false;
                    CEHelper.brothelFlagMale = false;

                    // Go Through Events
                    foreach (CEEvent _listedEvent in CEPersistence.CEEvents.Where(_listedEvent => !_listedEvent.Name.IsStringNoneOrEmpty()))
                    {
                        if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Overwriteable) && CEPersistence.CEEvents.FindAll(matchEvent => matchEvent.Name == _listedEvent.Name).Count > 1) continue;

                        if (!CEHelper.brothelFlagFemale)
                        {
                            if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale))
                                CEHelper.brothelFlagFemale = true;
                        }

                        if (!CEHelper.brothelFlagMale)
                        {
                            if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale))
                                CEHelper.brothelFlagMale = true;
                        }

                        if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu))
                        {
                            CEPersistence.CEWaitingList.Add(_listedEvent);
                        }
                        else
                        {
                            if (!_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CanOnlyBeTriggeredByOtherEvent)) CEPersistence.CECallableEvents.Add(_listedEvent);

                            CEPersistence.CEEventList.Add(_listedEvent);
                        }
                    }

                    new CESubModule().AddCustomEvents(new CampaignGameStarter(Campaign.Current.GameMenuManager, Campaign.Current.ConversationManager, Campaign.Current.CurrentGame.GameTextManager, Campaign.Current.CampaignGameLoadingType == Campaign.GameLoadingType.Tutorial));

                    // Load Images
                    string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/";
                    string requiredPath = fullPath + "CaptivityRequired";

                    // Get Required
                    string[] requiredImages = Directory.EnumerateFiles(requiredPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

                    // Get All in ModuleLoader
                    string[] files = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

                    CEPersistence.CEEventImageList.Clear();

                    // Module Image Load
                    if (modulePaths.Count != 0)
                    {
                        foreach (string filepath in modulePaths)
                        {
                            try
                            {
                                string[] moduleFiles = Directory.EnumerateFiles(filepath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

                                foreach (string file in moduleFiles)
                                {
                                    if (!CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                                    {
                                        try
                                        {
                                            TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                            texture.PreloadTexture();
                                            Texture texture2D = new Texture(new EngineTexture(texture));
                                            CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                                        }
                                        catch (Exception e)
                                        {
                                            CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                                        }
                                    }
                                    else CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                                }
                            }
                            catch (Exception) { }
                        }
                    }

                    // Captivity Location Image Load
                    try
                    {

                        foreach (string file in files)
                        {
                            if (requiredImages.Contains(file)) continue;

                            if (!CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                            {
                                try
                                {
                                    TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                    texture.PreloadTexture();
                                    Texture texture2D = new Texture(new EngineTexture(texture));
                                    CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                                }
                                catch (Exception e)
                                {
                                    CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                                }
                            }
                            else CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                        }

                        foreach (string file in requiredImages)
                        {
                            if (CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file))) continue;

                            try
                            {
                                TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                texture.PreloadTexture();
                                Texture texture2D = new Texture(new EngineTexture(texture));
                                CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                            }
                            catch (Exception e)
                            {
                                CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                            }
                        }

                        new CESubModule().LoadTexture("default", false, true);
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failure to load textures, Critical failure. " + e);
                    }

                    CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CEEventImageList.Count + " images and " + CEPersistence.CEEvents.Count + " events.");

                    return "Loaded " + CEPersistence.CEEventImageList.Count + " images and " + CEPersistence.CEEvents.Count + " events.";
                }
                catch (Exception e)
                {
                    return "Failed : " + e;
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("play_sound", "captivity")]
        public static string PlaySound(List<string> strings)
        {
            try
            {
                Thread.Sleep(500);

                if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType)) return CampaignCheats.ErrorType;

                if (CampaignCheats.CheckHelp(strings) && CampaignCheats.CheckParameters(strings, 1)) return "Format is \"captivity.play_sound [SOUND_ID]\".";

                string searchTerm = strings[0];
                int id = 0;

                try
                {
                    if (!searchTerm.IsStringNoneOrEmpty()) id = SoundEvent.GetEventIdFromString(searchTerm);

                    if (id == 0) return "Sound not found.";
                }
                catch
                {
                    return "Sound not found.";
                }

                try
                {
                    if (Game.Current.GameStateManager.ActiveState is MissionState)
                    {
                        Mission.Current.MakeSound(id, Agent.Main.Frame.origin, true, false, -1, -1);

                        string text = "";
                        List<GameEntity> entities = new List<GameEntity>();
                        Mission.Current.Scene.GetEntities(ref entities);
                        foreach (GameEntity test in entities)
                        {
                            text += test.Name + " : " + test.ToString() + "\n";
                        }
                        CECustomHandler.ForceLogToFile(text);


                        return string.Empty;
                    }

                    SoundEvent.PlaySound2D(id);

                    return string.Empty;
                }
                catch (Exception)
                {
                    return "Failed to play sound";
                }
            }
            catch (Exception e)
            {
                return "Sosig\n" + e;
            }
        }


    }
}