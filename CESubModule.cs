#define V180

using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;
using Path = System.IO.Path;
using Texture = TaleWorlds.TwoDimension.Texture;
using TaleWorlds.ScreenSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors.BarterBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using System.Xml;
using CaptivityEvents.Notifications;


#if V172
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.MountAndBlade.View.Missions;
#else
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.Core.ViewModelCollection.Information;
#endif

namespace CaptivityEvents
{
    public static class CEPersistence
    {
        public enum DungeonState
        {
            Normal,
            StartWalking,
            FadeIn
        }

        public enum BrothelState
        {
            Normal,
            Start,
            FadeIn,
            Black,
            FadeOut
        }

        public enum HuntState
        {
            Normal,
            StartHunt,
            HeadStart,
            Hunting,
            AfterBattle
        }

        public enum BattleState
        {
            Normal,
            StartBattle,
            AfterBattle,
            UpdateBattle,
        }

        // Events
        public static List<CEEvent> CEEvents = new();

        public static List<CEEvent> CEEventList = new();
        public static List<CEEvent> CEAlternativePregnancyEvents = new();
        public static List<CEEvent> CEWaitingList = new();
        public static List<CEEvent> CECallableEvents = new();

        // Captive Variables
        public static bool captivePlayEvent;

        public static CharacterObject captiveToPlay;

        public static int captiveInventoryStage = 0;
        public static Hero removeHero = null;

        public static string victoryEvent;
        public static string defeatEvent;
        public static List<TroopRosterElement> playerTroops = new();
        public static bool removePlayer = false;
        public static bool destroyParty = false;
        public static bool surrenderParty = false;
        public static bool playerWon = false;

        // Animation Variables
        public static bool animationPlayEvent;

        public static List<string> animationImageList = new();
        public static int animationIndex;
        public static float animationSpeed = 0.03f;

        public static bool notificationExists;

        public static Agent agentTalkingTo;
        public static GameEntity gameEntity = null;

        // Unknown
        public static float playerSpeed = 0f;

        public static HuntState huntState = HuntState.Normal;
        public static DungeonState dungeonState = DungeonState.Normal;
        public static BrothelState brothelState = BrothelState.Normal;
        public static BattleState battleState = BattleState.Normal;

        // Fade out for Brothel
        public static float brothelFadeIn = 2f;

        public static float brothelBlack = 10f;
        public static float brothelFadeOut = 2f;

        public static List<CECustom> CECustomFlags = new();
        public static List<CEScene> CECustomScenes = new();

        public static List<CECustomModule> CECustomModules = new();

        // Images
        public static Dictionary<string, Texture> CEEventImageList = new();

        // Sound
        public static SoundEvent soundEvent = null;

        public static bool soundLoop = false;
    }

    public class CESubModule : MBSubModuleBase
    {
        // Loaded Variables
        private static bool _isLoaded;

        private static bool _isLoadedInGame;

        // Harmony
        private Harmony _harmony;

        public const string HarmonyId = "com.CE.captivityEvents";

        // Last Check on Animation Loop
        private static float lastCheck;

        // Timer for Hunting
        private static float huntingTimerOne;

        // Fade out for Dungeon
        private static float dungeonFadeOut = 2f;

        // Timer for Brothel
        private static float brothelTimerOne;
        private static float brothelTimerTwo;
        private static float brothelTimerThree;

        // Max Brothel Sound
        private static readonly float brothelSoundMin = 1f;

        private static readonly float brothelSoundMax = 3f;

        // Mount & Blade II Bannerlord\GUI\GauntletUI\spriteData.xml
        // Mount & Blade II Bannerlord\Modules\Native\GUI\NativeSpriteData.xml
        private static readonly int[] sprite_index = new int[] { 2, 3, 4, 5 };

        // Sounds for Brothel
        private static readonly Dictionary<string, int> brothelSounds = new();

        public void LoadTexture(string name, bool swap = false, bool forcelog = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                if (!swap)
                {
                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[3]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_male_prison"] : CEPersistence.CEEventImageList["default_male_prison_sfw"]
                        : CEPersistence.CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[2]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_female_prison"] : CEPersistence.CEEventImageList["default_female_prison_sfw"]
                        : CEPersistence.CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[1]] = name == "default"
                          ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_male"] : CEPersistence.CEEventImageList["default_male_sfw"]
                        : CEPersistence.CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[0]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_female"] : CEPersistence.CEEventImageList["default_female_sfw"]
                        : CEPersistence.CEEventImageList[name];
                }
                else
                {
                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[3]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_female_prison"] : CEPersistence.CEEventImageList["default_female_prison_sfw"]
                        : CEPersistence.CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[2]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_male_prison"] : CEPersistence.CEEventImageList["default_male_prison_sfw"]
                        : CEPersistence.CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[1]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_female"] : CEPersistence.CEEventImageList["default_female_sfw"]
                        : CEPersistence.CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[sprite_index[0]] = name == "default"
                        ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? CEPersistence.CEEventImageList["default_male"] : CEPersistence.CEEventImageList["default_male_sfw"]
                        : CEPersistence.CEEventImageList[name];
                }
            }
            catch (Exception e)
            {
                if (forcelog)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Failure to load " + name + ". Refer to LogFileFC.txt in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs", Colors.Red));
                    CECustomHandler.ForceLogToFile("Failure to load " + name + " - exception : " + e.Message);
                }
                else
                {
                    CECustomHandler.LogToFile("Failed to load the texture of " + name);
                }
            }
        }

        public void LoadCampaignNotificationTexture(string name, int sheet = 0, bool forcelog = false)
        {
            try
            {
                UIResourceManager.SpriteData.SpriteCategories["ce_notification_icons"].SpriteSheets[sheet] = name == "default"
                    ? CEPersistence.CEEventImageList["CE_default_notification"]
                    : CEPersistence.CEEventImageList[name];
            }
            catch (Exception e)
            {
                if (forcelog)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Failure to load " + name + ". Refer to LogFileFC.txt in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs", Colors.Red));
                    CECustomHandler.ForceLogToFile("Failure to load " + name + " - exception : " + e.Message);
                }
                else
                {
                    CECustomHandler.LogToFile("Failed to load the texture of " + name);
                }
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            ModuleInfo ceModule = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "zCaptivityEvents"; });
            ModuleInfo nativeModule = ModuleHelper.GetModules().FirstOrDefault(searchInfo => { return searchInfo.IsNative(); });

            ApplicationVersion modversion = ceModule.Version;
            ApplicationVersion gameversion = nativeModule.Version;

            if (gameversion.Major != modversion.Major || gameversion.Minor != modversion.Minor || modversion.Revision != gameversion.Revision)
            {
                CECustomHandler.ForceLogToFile("Captivity Events " + modversion + " has the detected the wrong version " + gameversion);
                DialogResult a = MessageBox.Show("Warning:\n Captivity Events " + modversion + " has the detected the wrong game version. Please download the correct version for " + gameversion + ". Or continue at your own risk.", "Captivity Events has the detected the wrong version");
            }
            else
            {
                CECustomHandler.ForceLogToFile("Captivity Events " + modversion + " has the detected the version " + gameversion);
            }

            try
            {
                _harmony = new Harmony(HarmonyId);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to initialize Harmony: " + e);
            }

            string[] modulesFound = Utilities.GetModulesNames();

            CECustomHandler.ForceLogToFile("\n -- Loaded Modules -- \n" + string.Join("\n", modulesFound));

            List<string> modulePaths = CEHelper.GetModulePaths(modulesFound, out List<ModuleInfo> modules);

            // Load Events
            CEPersistence.CEEvents = CECustomHandler.GetAllVerifiedXSEFSEvents(modulePaths);
            CEPersistence.CECustomFlags = CECustomHandler.GetCustom();
            CEPersistence.CECustomScenes = CECustomHandler.GetScenes();
            CEPersistence.CECustomModules = CECustomHandler.GetModules();

            try
            {
                CEPersistence.CECustomModules.ForEach(item =>
                {
                    item.CEModuleName = modules.FirstOrDefault(moduleInfo => { return moduleInfo.Id == item.CEModuleName; })?.Name ?? item.CEModuleName;
                });
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to name CECustomModules");
            }

            // Load Images
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/";
            string requiredPath = fullPath + "CaptivityRequired";

            // Get Required
            string[] requiredImages = Directory.EnumerateFiles(requiredPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

            // Get All in ModuleLoader
            string[] files = Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif")).ToArray();

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
                                    texture.PreloadTexture(false);
                                    Texture texture2D = new(new EngineTexture(texture));
                                    CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                                }
                                catch (Exception e)
                                {
                                    CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                                }
                            }
                            else
                            {
                                CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                            }
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
                            texture.PreloadTexture(false);
                            Texture texture2D = new(new EngineTexture(texture));
                            CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                        }
                    }
                    else
                    {
                        CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                    }
                }

                foreach (string file in requiredImages)
                {
                    if (CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file))) continue;

                    try
                    {
                        TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                        texture.PreloadTexture(false);
                        Texture texture2D = new(new EngineTexture(texture));
                        CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                    }
                }

                SpriteCategory spriteCategory = UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"];
                spriteCategory.SpriteSheets.AddRange(new Texture[] { CEPersistence.CEEventImageList["default_female_prison"], CEPersistence.CEEventImageList["default_male_prison"], CEPersistence.CEEventImageList["default_female"], CEPersistence.CEEventImageList["default_male"] });
                spriteCategory.SheetSizes = spriteCategory.SheetSizes.AddRangeToArray(new Vec2i[] { new Vec2i(445, 805), new Vec2i(445, 805), new Vec2i(445, 805), new Vec2i(445, 805) });
                spriteCategory.SpriteSheetCount = 6;
                CECustomHandler.ForceLogToFile("Loading Textures 1.7.2");

                PropertyInfo propertyWidth = typeof(SpritePart).GetProperty("Width");
                PropertyInfo propertyHeight = typeof(SpritePart).GetProperty("Height");
                foreach (SpritePart spritePart in UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteParts)
                {
                    switch (spritePart.Name)
                    {
                        case "wait_prisoner_female":
                            spritePart.SheetID = 6;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth.GetSetMethod(true).Invoke(spritePart, new object[] { 445 });
                            propertyHeight.GetSetMethod(true).Invoke(spritePart, new object[] { 805 });
                            spritePart.UpdateInitValues();
                            break;

                        case "wait_prisoner_male":
                            spritePart.SheetID = 5;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth.GetSetMethod(true).Invoke(spritePart, new object[] { 445 });
                            propertyHeight.GetSetMethod(true).Invoke(spritePart, new object[] { 805 });
                            spritePart.UpdateInitValues();
                            break;

                        case "wait_captive_female":
                            spritePart.SheetID = 4;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth.GetSetMethod(true).Invoke(spritePart, new object[] { 445 });
                            propertyHeight.GetSetMethod(true).Invoke(spritePart, new object[] { 805 });
                            spritePart.UpdateInitValues();
                            break;

                        case "wait_captive_male":
                            spritePart.SheetID = 3;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth.GetSetMethod(true).Invoke(spritePart, new object[] { 445 });
                            propertyHeight.GetSetMethod(true).Invoke(spritePart, new object[] { 805 });
                            spritePart.UpdateInitValues();
                            break;

                        default:
                            break;
                    }
                }

                LoadTexture("default", false, true);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure to load textures, Critical failure. " + e);
            }

            CECustomHandler.ForceLogToFile("Loading Notification Sprites");

            try
            {
                // Load theMount & Blade II Bannerlord\Modules\SandBox\GUI\Brushes
                // MapNotification Sprite (REMEMBER TO DOUBLE CHECK FOR NEXT VERSION 1.5.7)
                SpriteData loadedData = new("CESpriteData");
                loadedData.Load(UIResourceManager.UIResourceDepot);

                string categoryName = "ce_notification_icons";
                SpriteData spriteData = UIResourceManager.SpriteData;

                SpriteCategory spriteCategory = spriteData.SpriteCategories[categoryName];
                spriteCategory.SpriteSheets.Add(CEPersistence.CEEventImageList["CE_default_notification"]);
                spriteCategory.SpriteSheets.Add(CEPersistence.CEEventImageList["CE_default_notification"]);
                spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

                UIResourceManager.BrushFactory.Initialize();
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure to load Notification Sprites, Critical failure. " + e);
            }

            CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CEEventImageList.Count + " images and " + CEPersistence.CEEvents.Count + " events.");
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            try
            {
                Dictionary<string, Version> dict = Harmony.VersionInfo(out Version myVersion);
                CECustomHandler.ForceLogToFile("My version: " + myVersion);

                foreach (KeyValuePair<string, Version> entry in dict)
                {
                    string id = entry.Key;
                    Version version = entry.Value;
                    CECustomHandler.ForceLogToFile("Mod " + id + " uses Harmony version " + version);
                }

                CECustomHandler.ForceLogToFile(CESettings.Instance?.EventCaptorNotifications ?? true
                                                   ? "Patching Map Notifications: No Conflicts Detected : Enabled."
                                                                   : "EventCaptorNotifications: Disabled.");

                _harmony.PatchAll();
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile("Failed to load: " + ex);
                MessageBox.Show($"Error Initializing Captivity Events:\n\n{ex}");
            }

            if (_isLoaded) return;

            try
            {
                if (CESettings.Instance?.IsHardCoded ?? false)
                {
                    TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(
                        new InitialStateOption(
                            "CaptivityEventsSettings",
                            new TextObject("Captivity Events Settings", null),
                            9990,
                            () => { ScreenManager.PushScreen(new CESettingsScreen()); },
                             () => new ValueTuple<bool, TextObject>(false, TextObject.Empty)
                        )
                      );
                }

                if (CESettingsIntegrations.Instance == null)
                {
                    CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettingsIntegrations missing MCMv4");
                }
                else
                {
                    CESettingsIntegrations.Instance.InitializeSettings();
                }

                if (CESettingsFlags.Instance == null)
                {
                    CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettingsFlags missing MCMv4");
                }
                else
                {
                    CESettingsFlags.Instance.InitializeSettings(CEPersistence.CECustomFlags);
                }
                CECustomHandler.ForceLogToFile("Loaded CESettings: "
                                               + (CESettings.Instance?.LogToggle ?? false
                                                   ? "Logs are enabled."
                                                   : "Extra Event Logs are disabled enable them through settings."));
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettings is being accessed improperly.");
            }

            foreach (CEEvent _listedEvent in CEPersistence.CEEvents.Where(_listedEvent => !string.IsNullOrWhiteSpace(_listedEvent.Name)))
            {
                if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Overwriteable) && (CEPersistence.CEEventList.FindAll(matchEvent => matchEvent.Name == _listedEvent.Name).Count > 0 || CEPersistence.CEWaitingList.FindAll(matchEvent => matchEvent.Name == _listedEvent.Name).Count > 0)) continue;

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

                if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.BirthAlternative))
                {
                    CEPersistence.CEAlternativePregnancyEvents.Add(_listedEvent);
                }
                else if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu))
                {
                    CEPersistence.CEWaitingList.Add(_listedEvent);
                }
                else
                {
                    if (!_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CanOnlyBeTriggeredByOtherEvent))
                    {
                        int weightedChance = 1;
                        try
                        {
                            if (_listedEvent.WeightedChanceOfOccuring != null) weightedChance = new CEVariablesLoader().GetIntFromXML(_listedEvent.WeightedChanceOfOccuring);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccuring on " + _listedEvent.Name);
                        }
                        if (weightedChance > 0)
                        {
                            CEPersistence.CECallableEvents.Add(_listedEvent);
                        }
                    }

                    CEPersistence.CEEventList.Add(_listedEvent);
                }
            }

            CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CEWaitingList.Count + " waiting menus ");
            CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CECallableEvents.Count + " callable events ");

            if (CEPersistence.CEEvents.Count > 0)
            {
                try
                {
                    if (CESettingsEvents.Instance == null)
                    {
                        CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettingsEvents missing MCMv4");
                    }
                    else
                    {
                        CESettingsEvents.Instance.InitializeSettings(CEPersistence.CECustomModules, CEPersistence.CECallableEvents);
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettings is being accessed improperly.");
                }

                try
                {
                    TextObject textObject = new("{=CEEVENTS1000}Captivity Events loaded with {EVENT_COUNT} Events and {IMAGE_COUNT} Images.\n^o^ Enjoy your events. Remember to endorse!");
                    textObject.SetTextVariable("EVENT_COUNT", CEPersistence.CEEvents.Count);
                    textObject.SetTextVariable("IMAGE_COUNT", CEPersistence.CEEventImageList.Count);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                    _isLoaded = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error Initialising Captivity Events:\n\n{e.GetType()}");
                    CECustomHandler.ForceLogToFile("Failed to load: " + e);
                    _isLoaded = false;
                }
            }
            else
            {
                _isLoaded = false;
            }

            if (!_isLoaded)
            {
                TextObject textObject = new("{=CEEVENTS1005}Error: Captivity Events failed to load events. Please refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs. Mod is disabled.");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
            }
        }

        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            CEConsole.CleanSave(new List<string>());
            base.OnNewGameCreated(game, initializerObject);
        }


        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is not Campaign) return;
            CleanBugs();
            ResetHelper();
            if (!_isLoaded) return;
#if V172
            game.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/zCaptivityEvents/ModuleData/module_strings.xml");
#else
#endif
            InitalizeAttributes(game);
            CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
            AddBehaviours(campaignStarter);
        }

        private void CleanBugs()
        {
            CheckEncounterIssue();
        }

        private void CheckEncounterIssue()
        {
            try
            {
                if (PlayerEncounter.Current == null) return;
                if (PlayerEncounter.EncounteredMobileParty == null) return;
                if (!PlayerEncounter.EncounteredMobileParty.StringId.StartsWith("CustomPartyCE_Hunt_")) return;
                CEPersistence.huntState = CEPersistence.HuntState.AfterBattle;
            }
            catch (Exception)
            {
                return;
            }
        }

        private void ResetHelper()
        {
            CEHelper.spouseOne = null;
            CEHelper.spouseTwo = null;
            CEHelper.waitMenuCheck = -1;

            CEHelper.notificationCaptorExists = false;
            CEHelper.notificationCaptorCheck = false;
            CEHelper.notificationEventExists = false;
            CEHelper.notificationEventCheck = false;
        }

        // Do Loading Investigate
        public override bool DoLoading(Game game)
        {
            if (Campaign.Current == null) return true;

            if (!(CESettings.Instance?.PrisonerEscapeBehavior ?? true)) return base.DoLoading(game);
            IMbEvent<Hero> dailyTickHeroEvent = CampaignEvents.DailyTickHeroEvent;

            if (dailyTickHeroEvent != null)
            {
                dailyTickHeroEvent.ClearListeners(Campaign.Current.GetCampaignBehavior<PrisonerReleaseCampaignBehavior>());
                if ((CESettings.Instance?.EscapeAutoRansom?.SelectedIndex ?? 0) != 2) dailyTickHeroEvent.ClearListeners(Campaign.Current.GetCampaignBehavior<DiplomaticBartersBehavior>());
            }

            IMbEvent<MobileParty> hourlyPartyTick = CampaignEvents.HourlyTickPartyEvent;
            hourlyPartyTick?.ClearListeners(Campaign.Current.GetCampaignBehavior<PrisonerReleaseCampaignBehavior>());

            IMbEvent<BarterData> barterablesRequested = CampaignEvents.BarterablesRequested;
            barterablesRequested?.ClearListeners(Campaign.Current.GetCampaignBehavior<SetPrisonerFreeBarterBehavior>());

            return base.DoLoading(game);
        }

        private void InitalizeAttributes(Game game) => CESkills.RegisterAll(game);

        private void AddBehaviours(CampaignGameStarter campaignStarter)
        {
            LoadTexture("default", false, true);

            campaignStarter.AddBehavior(new CECampaignBehavior());
            if (CESettings.Instance?.ProstitutionControl ?? true)
            {
                CEBrothelBehavior brothelBehavior = new();
                brothelBehavior.OnSessionLaunched(campaignStarter);
                campaignStarter.AddBehavior(brothelBehavior);
            }
            if (CESettings.Instance?.PrisonerEscapeBehavior ?? true)
            {
                campaignStarter.AddBehavior(new CEPrisonerEscapeCampaignBehavior());
                campaignStarter.AddBehavior(new CESetPrisonerFreeBarterBehavior());
            }
            if (CESettings.Instance?.EventCaptiveOn ?? true) ReplaceModel<PlayerCaptivityModel, CEPlayerCaptivityModel>(campaignStarter);
            CEPrisonerDialogue prisonerDialogue = new();
            if ((CESettings.Instance?.EventCaptorOn ?? true) && (CESettings.Instance?.EventCaptorDialogue ?? true)) prisonerDialogue.AddPrisonerLines(campaignStarter);
            if (CEPersistence.CECustomScenes.Count > 0) prisonerDialogue.AddCustomLines(campaignStarter, CEPersistence.CECustomScenes);
            //if (CESettings.Instance?.PregnancyToggle) ReplaceModel<PregnancyModel, CEDefaultPregnancyModel>(campaignStarter);

            if (_isLoadedInGame) CEConsole.ReloadEvents(new List<string>());
            else AddCustomEvents(campaignStarter);

            if (_isLoadedInGame) return;
#if V172
            TooltipVM.AddTooltipType(typeof(CEBrothel), CEBrothelToolTip.BrothelTypeTooltipAction);
#else
            PropertyBasedTooltipVM.AddTooltipType(typeof(CEBrothel), CEBrothelToolTip.BrothelTypeTooltipAction);
#endif
            LoadBrothelSounds();
            _isLoadedInGame = true;
        }

        protected void ReplaceModel<TBaseType, TChildType>(IGameStarter gameStarter) where TBaseType : GameModel where TChildType : GameModel
        {
            if (gameStarter.Models is not IList<GameModel> list) return;
            bool flag = false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is TBaseType)
                {
                    flag = true;
                    if (list[i] is not TChildType) list[i] = Activator.CreateInstance<TChildType>();
                }
            }

            if (!flag) gameStarter.AddModel(Activator.CreateInstance<TChildType>());
        }

        protected void ReplaceBehaviour<TBaseType, TChildType>(CampaignGameStarter gameStarter) where TBaseType : CampaignBehaviorBase where TChildType : CampaignBehaviorBase
        {
            if (gameStarter.CampaignBehaviors is not IList<CampaignBehaviorBase> list) return;
            bool flag = false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] is TBaseType)
                {
                    flag = true;
                    if (list[i] is not TChildType) list[i] = Activator.CreateInstance<TChildType>();
                }
            }

            if (!flag) gameStarter.AddBehavior(Activator.CreateInstance<TChildType>());
        }

        public void AddCustomEvents(CampaignGameStarter gameStarter)
        {
            // Waiting Menu Load
            foreach (CEEvent waitingEvent in CEPersistence.CEWaitingList) AddEvent(gameStarter, waitingEvent, CEPersistence.CEEvents);

            // Alternative Event Load 
            foreach (CEEvent alternativeEvent in CEPersistence.CEAlternativePregnancyEvents) AddEvent(gameStarter, alternativeEvent, CEPersistence.CEEvents);

            // Listed Event Load
            foreach (CEEvent listedEvent in CEPersistence.CEEventList) AddEvent(gameStarter, listedEvent, CEPersistence.CEEvents);
        }

        private void AddEvent(CampaignGameStarter gameStarter, CEEvent _listedEvent, List<CEEvent> eventList)
        {
            CECustomHandler.LogToFile("Loading Event: " + _listedEvent.Name);

            try
            {
                if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                {
                    CEEventLoader.CELoadCaptorEvent(gameStarter, _listedEvent, eventList);
                }
                else if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                {
                    CEEventLoader.CELoadCaptiveEvent(gameStarter, _listedEvent, eventList);
                }
                else if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                {
                    CEEventLoader.CELoadRandomEvent(gameStarter, _listedEvent, eventList);
                }
                else
                {
                    CECustomHandler.ForceLogToFile("Failed to load " + _listedEvent.Name + " contains no category flag (Captor, Captive, Random)");
                    TextObject textObject = new("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
                    textObject.SetTextVariable("NAME", _listedEvent.Name);
                    textObject.SetTextVariable("TEST", "TEST");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to load " + _listedEvent.Name + " exception: " + e.Message + " stacktrace: " + e.StackTrace);

                if (!_isLoadedInGame)
                {
                    TextObject textObject = new("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
                    textObject.SetTextVariable("NAME", _listedEvent.Name);
                    textObject.SetTextVariable("ERROR", e.Message);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                }
            }
        }

        private void LoadBrothelSounds()
        {
            brothelSounds.Add("female_01_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/01/stun"));
            brothelSounds.Add("female_02_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/02/stun"));
            brothelSounds.Add("female_03_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/03/stun"));
            brothelSounds.Add("female_04_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/04/stun"));
            brothelSounds.Add("female_05_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/05/stun"));

            brothelSounds.Add("male_01_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/01/stun"));
            brothelSounds.Add("male_02_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/02/stun"));
            brothelSounds.Add("male_03_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/03/stun"));
            brothelSounds.Add("male_04_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/04/stun"));
            brothelSounds.Add("male_05_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/05/stun"));
            brothelSounds.Add("male_06_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/06/stun"));
            brothelSounds.Add("male_07_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/07/stun"));
        }

        protected override void OnApplicationTick(float dt)
        {
            if (Game.Current == null || Game.Current.GameStateManager == null) return;

            // CaptiveState
            CaptiveStateCheck();

            // RemoveCaptiveState
            RemoveCaptiveStateCheck();

            // SoundState
            SoundStateCheck();

            // Animated Background Menus
            AnimationStateCheck();

            // Brothel Event To Play
            BrothelStateCheck();

            // Hunt Event To Play
            HuntStateCheck();

            // Battle Event To Play
            BattleStateCheck();
        }

        private void SoundStateCheck()
        {
            if (CEPersistence.soundLoop && CEPersistence.soundEvent != null && Game.Current.GameStateManager.ActiveState is MapState)
            {
                try
                {
                    if (!CEPersistence.soundEvent.IsPlaying())
                    {
                        CEPersistence.soundEvent.Play();
                    }
                }
                catch (Exception)
                {
                    CEPersistence.soundEvent = null;
                }
            }
        }

        // TODO MOVE TO PROPER LISTENERS AND AWAY FROM ONAPPLICATIONTICK
        private void AnimationStateCheck()
        {
            if (CEPersistence.animationPlayEvent && Game.Current.GameStateManager.ActiveState is MapState)
            {
                try
                {
                    if (Game.Current.ApplicationTime > lastCheck)
                    {
                        if (CEPersistence.animationIndex > CEPersistence.animationImageList.Count() - 1) CEPersistence.animationIndex = 0;

                        LoadTexture(CEPersistence.animationImageList[CEPersistence.animationIndex]);
                        CEPersistence.animationIndex++;

                        lastCheck = Game.Current.ApplicationTime + CEPersistence.animationSpeed;
                    }
                }
                catch (Exception)
                {
                    CEPersistence.animationPlayEvent = false;
                }
            }
        }

        private void RemoveCaptiveStateCheck()
        {
            switch (CEPersistence.captiveInventoryStage)
            {
                case 0:
                    break;

                case 1:
                    if (Game.Current.GameStateManager.ActiveState is InventoryState inventoryState)
                    {
                        CEPersistence.captiveInventoryStage = 2;
                    }
                    break;

                case 2:
                    if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                    {
                        if (CEPersistence.removeHero != null)
                        {
                            while (MobileParty.MainParty.MemberRoster.Contains(CEPersistence.removeHero.CharacterObject))
                            {
                                MobileParty.MainParty.MemberRoster.RemoveTroop(CEPersistence.removeHero.CharacterObject, 1);
                            }

                            CEPersistence.removeHero = null;
                            PartyBase.MainParty.Visuals.SetMapIconAsDirty();
                        }
                        CEPersistence.captiveInventoryStage = 0;
                    }
                    break;
            }
        }

        private void CaptiveStateCheck()
        {
            // CaptiveState
            if (!CEPersistence.captivePlayEvent) return;

            // Dungeon
            DungeonStateCheck();

            // Party Menu -> Map State
            if (Game.Current.GameStateManager.ActiveState is PartyState) Game.Current.GameStateManager.PopState();

            // Map State -> Play Menu
            if (Game.Current.GameStateManager.ActiveState is MapState mapState)
            {
                CEPersistence.captivePlayEvent = false;

                try
                {
                    if (Hero.MainHero.IsFemale)
                    {
                        CEEvent triggeredEvent = CEPersistence.captiveToPlay.IsFemale
                            ? CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_female_sexual_menu")
                            : CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_female_sexual_menu_m");
                        triggeredEvent.Captive = CEPersistence.captiveToPlay;

                        if (mapState.AtMenu)
                        {
                            CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }
                        else
                        {
                            CECampaignBehavior.ExtraProps.menuToSwitchBackTo = null;
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = null;
                        }

                        GameMenu.ActivateGameMenu(triggeredEvent.Name);

                        if (mapState.MenuContext != null)
                        {
                            mapState.MenuContext.SetBackgroundMeshName("wait_prisoner_female");
                        }
                    }
                    else
                    {
                        CEEvent triggeredEvent = CEPersistence.captiveToPlay.IsFemale
                            ? CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_male_sexual_menu")
                            : CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_male_sexual_menu_m");
                        triggeredEvent.Captive = CEPersistence.captiveToPlay;

                        if (mapState.AtMenu)
                        {
                            CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }
                        else
                        {
                            CECampaignBehavior.ExtraProps.menuToSwitchBackTo = null;
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = null;
                        }

                        GameMenu.ActivateGameMenu(triggeredEvent.Name);
                        if (mapState.MenuContext != null)
                        {
                            mapState.MenuContext.SetBackgroundMeshName("wait_prisoner_male");
                        }
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile(
                        Hero.MainHero.IsFemale
                        ? "Missing : CE_captor_female_sexual_menu/CE_captor_female_sexual_menu_m"
                        : "Missing : CE_captor_male_sexual_menu/CE_captor_male_sexual_menu_m");
                }

                CEPersistence.captiveToPlay = null;
            }
        }

        private void DungeonStateCheck()
        {
            if (CEPersistence.dungeonState == CEPersistence.DungeonState.Normal) return;

            // Dungeon
            if (Game.Current.GameStateManager.ActiveState is MissionState missionStateDungeon && missionStateDungeon.CurrentMission.IsLoadingFinished)
            {
                switch (CEPersistence.dungeonState)
                {
                    case CEPersistence.DungeonState.StartWalking:
                        if (CharacterObject.OneToOneConversationCharacter == null)
                        {
                            try
                            {
                                MissionCameraFadeView behavior = Mission.Current.GetMissionBehavior<MissionCameraFadeView>();

                                Mission.Current.MainAgentServer.Controller = Agent.ControllerType.AI;

                                WorldPosition worldPosition = new(Mission.Current.Scene, UIntPtr.Zero, CEPersistence.gameEntity.GlobalPosition, false);

                                if (CEPersistence.agentTalkingTo.CanBeAssignedForScriptedMovement())
                                {
                                    CEPersistence.agentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                    dungeonFadeOut = 2f;
                                }
                                else
                                {
                                    CEPersistence.agentTalkingTo.DisableScriptedMovement();
                                    CEPersistence.agentTalkingTo.HandleStopUsingAction();
                                    CEPersistence.agentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                    dungeonFadeOut = 2f;
                                }

                                behavior.BeginFadeOut(dungeonFadeOut);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                            }
                            brothelTimerOne = missionStateDungeon.CurrentMission.CurrentTime + dungeonFadeOut;
                            CEPersistence.dungeonState = CEPersistence.DungeonState.FadeIn;
                        }

                        break;

                    case CEPersistence.DungeonState.FadeIn:
                        if (brothelTimerOne < missionStateDungeon.CurrentMission.CurrentTime)
                        {
                            CEPersistence.agentTalkingTo.ResetLookAgent();
                            CEPersistence.agentTalkingTo.ResetAgentProperties();
                            CEPersistence.dungeonState = CEPersistence.DungeonState.Normal;
                            Mission.Current.EndMission();
                        }

                        break;

                    case CEPersistence.DungeonState.Normal:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void BrothelStateCheck()
        {
            if (CEPersistence.brothelState == CEPersistence.BrothelState.Normal) return;

            if (Game.Current.GameStateManager.ActiveState is MissionState missionStateBrothel && missionStateBrothel.CurrentMission.IsLoadingFinished)
            {
                switch (CEPersistence.brothelState)
                {
                    case CEPersistence.BrothelState.Start:
                        if (CharacterObject.OneToOneConversationCharacter == null)
                        {
                            try
                            {
                                MissionCameraFadeView behavior = Mission.Current.GetMissionBehavior<MissionCameraFadeView>();

                                Mission.Current.MainAgentServer.Controller = Agent.ControllerType.AI;

                                if (CEPersistence.gameEntity != null)
                                {
                                    WorldPosition worldPosition = new(Mission.Current.Scene, UIntPtr.Zero, CEPersistence.gameEntity.GlobalPosition, false);

                                    if (CEPersistence.agentTalkingTo.CanBeAssignedForScriptedMovement())
                                    {
                                        CEPersistence.agentTalkingTo.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                        CEPersistence.brothelFadeIn = 3f;
                                    }
                                    else
                                    {
                                        CEPersistence.agentTalkingTo.DisableScriptedMovement();
                                        CEPersistence.agentTalkingTo.HandleStopUsingAction();
                                        CEPersistence.agentTalkingTo.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                        CEPersistence.brothelFadeIn = 3f;
                                    }
                                }

                                behavior.BeginFadeOutAndIn(CEPersistence.brothelFadeIn, CEPersistence.brothelBlack, CEPersistence.brothelFadeOut);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                            }
                            brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.brothelFadeIn;
                            CEPersistence.brothelState = CEPersistence.BrothelState.FadeIn;
                        }

                        break;

                    case CEPersistence.BrothelState.FadeIn:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.CurrentTime)
                        {
                            brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.brothelBlack;
                            brothelTimerTwo = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);
                            brothelTimerThree = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);
                            Hero.MainHero.HitPoints += 10;

                            CEPersistence.agentTalkingTo.ResetLookAgent();
                            CEPersistence.agentTalkingTo.ResetAgentProperties();

                            if (CEPersistence.gameEntity != null)
                            {
                                Mission.Current.MainAgent.TeleportToPosition(CEPersistence.gameEntity.GlobalPosition);
                            }
                            CEPersistence.brothelState = CEPersistence.BrothelState.Black;

                            if (CESettingsIntegrations.Instance != null && CESettingsIntegrations.Instance.ActivateHotButter)
                            {
                                brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.brothelFadeOut;
                                Mission.Current.MainAgentServer.Controller = Agent.ControllerType.Player;
                                CEPersistence.brothelState = CEPersistence.BrothelState.FadeOut;
                                try
                                {
                                    string sceneToPlay = CEHelper.CustomSceneToPlay("scn_pompa_$location_culture_$location_$randomize", PartyBase.MainParty);
                                    CESceneNotification data = new(Hero.MainHero.IsFemale ? CharacterObject.Find(CEPersistence.agentTalkingTo.Character.StringId) : Hero.MainHero.CharacterObject, !Hero.MainHero.IsFemale ? CharacterObject.Find(CEPersistence.agentTalkingTo.Character.StringId) : Hero.MainHero.CharacterObject, sceneToPlay);
                                    MBInformationManager.ShowSceneNotification(data);
                                }
                                catch (Exception e)
                                {
                                    CECustomHandler.ForceLogToFile("FadeIn: " + e);
                                }
                            }
                        }

                        break;

                    case CEPersistence.BrothelState.Black:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.CurrentTime)
                        {
                            brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.brothelFadeOut;
                            Mission.Current.MainAgentServer.Controller = Agent.ControllerType.Player;
                            CEPersistence.brothelState = CEPersistence.BrothelState.FadeOut;
                        }
                        else if (brothelTimerTwo < missionStateBrothel.CurrentMission.CurrentTime && (CESettingsIntegrations.Instance == null || !CESettingsIntegrations.Instance.ActivateHotButter))
                        {
                            brothelTimerTwo = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            try
                            {
                                int soundnum = brothelSounds.Where(sound => { return sound.Key.StartsWith(Agent.Main.GetAgentVoiceDefinition()); }).GetRandomElementInefficiently().Value;
                                Mission.Current.MakeSound(soundnum, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception) { }
                        }
                        else if (brothelTimerThree < missionStateBrothel.CurrentMission.CurrentTime && (CESettingsIntegrations.Instance == null || !CESettingsIntegrations.Instance.ActivateHotButter))
                        {
                            brothelTimerThree = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            try
                            {
                                int soundnum = brothelSounds.Where(sound => { return sound.Key.StartsWith(CEPersistence.agentTalkingTo.GetAgentVoiceDefinition()); }).GetRandomElementInefficiently().Value;
                                Mission.Current.MakeSound(soundnum, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception) { }
                        }

                        break;

                    case CEPersistence.BrothelState.FadeOut:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.CurrentTime)
                        {
                            CEPersistence.agentTalkingTo = null;
                            CEPersistence.brothelState = CEPersistence.BrothelState.Normal;
                        }

                        break;

                    case CEPersistence.BrothelState.Normal:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void HuntStateCheck()
        {
            // Hunt Event To Play
            if (CEPersistence.huntState == CEPersistence.HuntState.Normal) return;

            // Hunt Event States
            if ((CEPersistence.huntState == CEPersistence.HuntState.StartHunt || CEPersistence.huntState == CEPersistence.HuntState.HeadStart) && Game.Current.GameStateManager.ActiveState is MissionState missionState && missionState.CurrentMission.IsLoadingFinished)
            {
                try
                {
                    switch (CEPersistence.huntState)
                    {
                        case CEPersistence.HuntState.StartHunt:
                            if (Mission.Current != null && Mission.Current.IsLoadingFinished && Mission.Current.CurrentTime > 2f && Mission.Current.Agents != null && !Mission.Current.Agents.Any((item) => item.IsPaused))
                            {
                                foreach (Agent agent2 in from agent in Mission.Current.Agents.ToList()
                                                         where agent.IsHuman && agent.IsEnemyOf(Agent.Main)
                                                         select agent)
                                {
                                    ForceAgentDropEquipment(agent2);
                                }
                               

                                // 1.5.1
                                missionState.CurrentMission.ClearCorpses(false);

                                CEHelper.AddQuickInformation(new TextObject("{=CEEVENTS1069}Let's give them a headstart."), 100, CharacterObject.PlayerCharacter);

                                CEPersistence.huntState = CEPersistence.HuntState.HeadStart;
                                huntingTimerOne = Mission.Current.CurrentTime + (CESettings.Instance?.HuntBegins ?? 7f);
                            }

                            break;

                        case CEPersistence.HuntState.HeadStart:
                            if (Mission.Current != null && Mission.Current.Agents != null && Mission.Current.CurrentTime > huntingTimerOne)
                            {
                                foreach (Agent agent2 in from agent in Mission.Current.Agents.ToList()
                                                         where agent.IsHuman && agent.IsEnemyOf(Agent.Main)
                                                         select agent)
                                {
                                    CommonAIComponent component = agent2.GetComponent<CommonAIComponent>();
                                    component?.Panic();
                                    agent2.DestinationSpeed = 0.5f;
                                }
                                CEHelper.AddQuickInformation(new TextObject("{=CEEVENTS1068}Hunt them down!"), 100, CharacterObject.PlayerCharacter, CharacterObject.PlayerCharacter.IsFemale
                                                                           ? "event:/voice/combat/female/01/victory"
                                                                           : "event:/voice/combat/male/01/victory");
                                CEPersistence.huntState = CEPersistence.HuntState.Hunting;
                            }

                            break;

                        case CEPersistence.HuntState.Normal:
                        case CEPersistence.HuntState.Hunting:
                        case CEPersistence.HuntState.AfterBattle:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed on hunting mission: " + e);
                    CEPersistence.huntState = CEPersistence.HuntState.Hunting;
                }
            }
            else if ((CEPersistence.huntState == CEPersistence.HuntState.HeadStart || CEPersistence.huntState == CEPersistence.HuntState.Hunting) && Game.Current.GameStateManager.ActiveState is MapState mapstate && mapstate.IsActive)
            {
                CEPersistence.huntState = CEPersistence.HuntState.AfterBattle;
                PlayerEncounter.SetPlayerVictorious();
                if (CESettings.Instance?.HuntLetPrisonersEscape ?? false) PlayerEncounter.EnemySurrender = true;
                PlayerEncounter.Update();
            }
            else if (CEPersistence.huntState == CEPersistence.HuntState.AfterBattle && Game.Current.GameStateManager.ActiveState is MapState mapstate2 && !mapstate2.IsMenuState)
            //TODO: move all of these to their proper listeners and out of the OnApplicationTick
            {
                if (PlayerEncounter.Current == null)
                {
                    CEPersistence.huntState = CEPersistence.HuntState.Normal;
                }
                else
                {
                    //PlayerEncounter.Update();
                }
            }
        }

        private void HandleFinishBattle(MapState mapstate)
        {
            CEPersistence.battleState = CEPersistence.BattleState.Normal;
            if (CEPersistence.playerWon)
            {
                if (CEPersistence.victoryEvent != null)
                {
                    GameMenu.ActivateGameMenu(CEPersistence.victoryEvent);
                    if (mapstate.MenuContext != null)
                    {
                        mapstate.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                               ? "wait_prisoner_female"
                                                               : "wait_prisoner_male");
                    }
                }
            }
            else
            {
                if (CEPersistence.defeatEvent != null)
                {
                    GameMenu.ActivateGameMenu(CEPersistence.defeatEvent);
                    if (mapstate.MenuContext != null)
                    {
                        mapstate.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                               ? "wait_prisoner_female"
                                                               : "wait_prisoner_male");
                    }
                }
            }
        }

        private void BattleStateCheck()
        {
            if (CEPersistence.battleState == CEPersistence.BattleState.Normal) return;

            if (CEPersistence.battleState == CEPersistence.BattleState.StartBattle && Game.Current.GameStateManager.ActiveState is MissionState missionState && missionState.CurrentMission.IsLoadingFinished)
            {
                CEPersistence.battleState = CEPersistence.BattleState.AfterBattle;
            }
            else if (CEPersistence.battleState == CEPersistence.BattleState.AfterBattle && Game.Current.GameStateManager.ActiveState is MapState mapstate2 && !mapstate2.IsMenuState)
            //TODO: move all of these to their proper listeners and out of the OnApplicationTick
            {
                if (PlayerEncounter.Current == null)
                {
                    HandleFinishBattle(mapstate2);
                }
                else
                {
                    try
                    {
                        if (PlayerEncounter.Battle == null) return;

                        CEPersistence.playerWon = PlayerEncounter.Battle.WinningSide == PlayerEncounter.Battle.PlayerSide;

                        PartyBase.MainParty.MemberRoster.RemoveIf((TroopRosterElement t) => !t.Character.IsPlayerCharacter || CEPersistence.removePlayer);

                        foreach (TroopRosterElement troopRosterElement in CEPersistence.playerTroops)
                        {
                            PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, 0, troopRosterElement.Xp, true, -1);
                        }

                        if (PlayerEncounter.EncounteredMobileParty != null && CEPersistence.surrenderParty)
                        {
                            try
                            {
                                if (CEPersistence.playerWon)
                                {
                                    PlayerEncounter.EnemySurrender = true;
                                }
                                else
                                {
                                    PlayerEncounter.PlayerSurrender = true;
                                }
                                PlayerEncounter.Update();
                                CEPersistence.battleState = CEPersistence.BattleState.UpdateBattle;
                            }
                            catch (Exception)
                            {
                                PlayerEncounter.Finish(true);
                            }
                        }
                        else if (PlayerEncounter.EncounteredMobileParty != null && CEPersistence.destroyParty)
                        {
                            PlayerEncounter.Current.FinalizeBattle();
                            try
                            {
                                DestroyPartyAction.Apply(PartyBase.MainParty, PlayerEncounter.EncounteredMobileParty);
                            }
                            catch (Exception e)
                            {
                                CECustomHandler.ForceLogToFile("FinalizeBattle: " + e);
                            }

                            PlayerEncounter.Finish(false);
                            if (Settlement.CurrentSettlement != null)
                            {
                                EncounterManager.StartSettlementEncounter(MobileParty.MainParty, Settlement.CurrentSettlement);
                                HandleFinishBattle(mapstate2);
                            }
                        }
                        else
                        {
                            PlayerEncounter.Current.FinalizeBattle();
                            PlayerEncounter.Finish(false);
                            if (Settlement.CurrentSettlement != null)
                            {
                                EncounterManager.StartSettlementEncounter(MobileParty.MainParty, Settlement.CurrentSettlement);
                                HandleFinishBattle(mapstate2);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("BattleStateCheck: " + e);
                        LoadingWindow.DisableGlobalLoadingWindow();
                        CEPersistence.battleState = CEPersistence.BattleState.Normal;
                    }
                }
            }
            else if (CEPersistence.battleState == CEPersistence.BattleState.UpdateBattle && Game.Current.GameStateManager.ActiveState is MapState mapstate3 && !mapstate3.IsMenuState)
            //TODO: move all of these to their proper listeners and out of the OnApplicationTick
            {
                try
                {
                    if (PlayerEncounter.Current == null)
                    {
                        HandleFinishBattle(mapstate3);
                    }
                    else
                    {
                        PlayerEncounter.Update();
                    }
                }
                catch (Exception)
                {
                    CEPersistence.battleState = CEPersistence.BattleState.Normal;
                }
            }
        }

        private void ForceAgentDropEquipment(Agent agent)
        {
            try
            {
                agent.RemoveEquippedWeapon(EquipmentIndex.Weapon0);
                agent.RemoveEquippedWeapon(EquipmentIndex.Weapon1);
                agent.RemoveEquippedWeapon(EquipmentIndex.Weapon2);
                agent.RemoveEquippedWeapon(EquipmentIndex.Weapon3);
                agent.RemoveEquippedWeapon(EquipmentIndex.Weapon4);
                if (agent.HasMount) agent.MountAgent.Die(new Blow(), Agent.KillInfo.Musket);
            }
            catch (Exception) { }
        }
    }
}