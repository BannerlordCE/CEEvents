using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Incidents;
using CaptivityEvents.Notifications;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors.BarterBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;
using Path = System.IO.Path;
using Texture = TaleWorlds.TwoDimension.Texture;

namespace CaptivityEvents
{
    public static class CEPersistence
    {
        public static bool animationPlayEvent;

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
        public static List<CEEvent> CEEvents = [];

        public static List<CEEvent> CEEventList = [];
        public static List<CEEvent> CEMenuOptionEvents = [];
        public static List<CEEvent> CEAlternativePregnancyEvents = [];
        public static List<CEEvent> CEAlternativeDeathEvents = [];
        public static List<CEEvent> CEAlternativeMarriageEvents = [];
        public static List<CEEvent> CEAlternativeDesertionEvents = [];
        public static List<CEEvent> CEPartyEnteredSettlementEvents = [];
        public static List<CEEvent> CEWaitingList = [];

        public static List<CEEvent> CECaptorEvents = [];
        public static List<CEEvent> CERandomEvents = [];
        public static List<CEEvent> CECaptiveEvents = [];

        public static List<CEEvent> CECallableEvents = [];

        public static List<Texture> CETextures = [];
        public static List<Texture> CENavalTextures = [];

        public static int CETexturesCount;
        public static int CENavalTexturesCount;

        // Mount & Blade II Bannerlord\GUI\GauntletUI\spriteData.xml
        // Mount & Blade II Bannerlord\Modules\Native\GUI\NativeSpriteData.xml
        public static int[] SpriteIndex = [0, 0, 0, 0];

        // Mount & Blade II Bannerlord\GUI\GauntletUI\spriteData.xml
        // Mount & Blade II Bannerlord\Modules\Native\GUI\NativeSpriteData.xml
        public static int[] NavalSpriteIndex = [0, 0, 0];

        // Captive Variables
        public static bool CaptivePlayEvent;

        public static CharacterObject CaptiveToPlay;

        public static int CaptiveInventoryStage = 0;
        public static Hero RemoveHero = null;

        public static string VictoryEvent;
        public static string DefeatEvent;
        public static List<TroopRosterElement> PlayerTroops = [];
        public static List<TroopRosterElement> TemporaryTroops = [];
        public static bool RemovePlayer = false;
        public static bool DestroyParty = false;
        public static bool SurrenderParty = false;
        public static bool PlayerWon = false;
        public static bool PlayerDied = false;
        public static bool PlayerSurrendered;

        // Animation Variables
        public static bool AnimationPlayEvent;

        public static List<string> AnimationImageList = [];
        public static int AnimationIndex;
        public static float AnimationSpeed = 0.03f;

        public static bool NotificationExists;

        public static Agent AgentTalkingTo;
        public static GameEntity GameEntity = null;

        // Unknown
        public static float PlayerSpeed = 0f;

        public static HuntState CurrentHuntState = HuntState.Normal;
        public static DungeonState CurrentDungeonState = DungeonState.Normal;
        public static BrothelState CurrentBrothelState = BrothelState.Normal;
        public static BattleState CurrentBattleState = BattleState.Normal;

        // Dialog line tracking to prevent duplicates
        public static bool PrisonerLinesAdded = false;
        public static bool CustomLinesAdded = false;

        // Fade out for Brothel
        public static float BrothelFadeIn = 2f;
        public static bool HotButterAvailable;
        public static float BrothelBlack = 10f;
        public static float BrothelFadeOut = 2f;

        public static List<CECustom> CECustomFlags = [];
        public static List<CEScene> CECustomScenes = [];

        public static List<CECustomModule> CECustomModules = [];

        // Images
        public static Dictionary<string, string> CEEventImageList = [];

        public static Dictionary<string, Texture> CELoadedTextures = [];


        // Sound
        public static SoundEvent SoundEvent;

        public static bool SoundLoop = false;
    }

    public class CESubModule : MBSubModuleBase
    {
        // Static instance for patches to access
        public static CESubModule Instance { get; private set; }

        // Loaded Variables
        private static bool _isLoaded;

        private static bool _isLoadedInGame;

        private static bool _isNavalLoaded;

        // Harmony
        private Harmony _harmony;

        public const string HarmonyId = "com.CE.captivityEvents";

        // Track if we've already played the custom intro movie
        private static bool _customIntroPlayed;

        // Dramalord integration
        public static bool IsDramalordLoaded { get; private set; }

        // Last Check on Animation Loop
        private static float _lastCheck;

        // Timer for Hunting
        private static float _huntingTimerOne;

        // Fade out for Dungeon
        private static float _dungeonFadeOut = 2f;

        // Timer for Brothel
        private static float _brothelTimerOne;
        private static float _brothelTimerTwo;
        private static float _brothelTimerThree;
        public static float SfIn = CEPersistence.BrothelFadeIn;
        public static float SfBlack = CEPersistence.BrothelBlack;
        public static float SfOut = CEPersistence.BrothelFadeOut;

        // Max Brothel Sound
        private static readonly float BrothelSoundMin = 1f;

        private static readonly float BrothelSoundMax = 3f;

        // Sounds for Brothel
        private static readonly Dictionary<string, int> BrothelSounds = [];

        public Texture QuickLoadCampaignTexture(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    CECustomHandler.ForceLogToFile("QuickLoadCampaignTexture called with null or empty path");

                    return null;
                }

                string name = Path.GetFileName(path);

                // Check if already loaded in cache
                if (CEPersistence.CELoadedTextures.ContainsKey(name))
                {
                    Texture cachedTexture = CEPersistence.CELoadedTextures[name];

                    // Verify the cached texture is still valid (note: IsValid is false until added to sprite sheet)
                    if (cachedTexture?.PlatformTexture != null)
                    {
                        // Additional check: verify the underlying texture hasn't been invalidated (material_error)
                        try
                        {
                            FieldInfo textureField = typeof(EngineTexture).GetField("Texture", BindingFlags.NonPublic | BindingFlags.Instance);

                            if (textureField != null)
                            {
                                object engineTexture = textureField.GetValue(cachedTexture.PlatformTexture);

                                if (engineTexture != null)
                                {
                                    PropertyInfo nameProperty = engineTexture.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);

                                    if (nameProperty != null)
                                    {
                                        string textureName = nameProperty.GetValue(engineTexture) as string;

                                        if (textureName == "material_error")
                                        {
                                            CECustomHandler.ForceLogToFile($"Cached texture {name} has been invalidated (material_error), reloading from disk");
                                            CEPersistence.CELoadedTextures.Remove(name);
                                            // Continue to load fresh texture below
                                        }
                                        else
                                        {
                                            // Texture is valid, return it
                                            return cachedTexture;
                                        }
                                    }
                                    else
                                    {
                                        // Can't verify, return cached texture
                                        return cachedTexture;
                                    }
                                }
                                else
                                {
                                    // engineTexture is null, texture has been invalidated
                                    CECustomHandler.ForceLogToFile($"Cached texture {name} has null underlying texture, reloading from disk");
                                    CEPersistence.CELoadedTextures.Remove(name);
                                }
                            }
                            else
                            {
                                // Can't verify, return cached texture
                                return cachedTexture;
                            }
                        }
                        catch (Exception ex)
                        {
                            CECustomHandler.LogToFile($"Error validating cached texture {name}: {ex.Message}");

                            // If validation fails, assume it's valid and return it
                            return cachedTexture;
                        }
                    }
                    else
                    {
                        // Remove invalid cached texture
                        CEPersistence.CELoadedTextures.Remove(name);
                        CECustomHandler.ForceLogToFile($"Removed invalid cached texture: {name} (PlatformTexture is null)");
                    }
                }

                // Load new texture
                string directory = Path.GetDirectoryName(path);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    CECustomHandler.ForceLogToFile($"Invalid directory for texture: {path}");

                    return null;
                }

                TaleWorlds.Engine.Texture texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{name}", $"{directory}");

                if (texture == null)
                {
                    CECustomHandler.ForceLogToFile($"LoadTextureFromPath returned null for: {name} in {directory}");

                    return null;
                }

                texture.PreloadTexture(true);
                Texture texture2D = new(new EngineTexture(texture));

                // Note: IsValid will be false until the texture is added to a sprite category's SpriteSheets
                // This is expected behavior - the texture becomes valid once it's part of a sprite sheet
                CECustomHandler.LogToFile($"Loaded texture {name}, IsValid={texture2D.IsValid} (will become true after adding to sprite sheet)");

                // If texture is already in cache, remove it so we can re-add it at the end (LRU behavior)
                if (CEPersistence.CELoadedTextures.Remove(name))
                {
                    CECustomHandler.LogToFile($"Moved texture {name} to end of cache (LRU)");
                }

                // Manage cache size before adding
                if (CEPersistence.CELoadedTextures.Count >= (CESettings.Instance?.EventAmountOfImagesToPreload ?? 40))
                {
                    KeyValuePair<string, Texture> textureToRemove = CEPersistence.CELoadedTextures.First();
                    CEPersistence.CELoadedTextures.Remove(textureToRemove.Key);
                    CECustomHandler.LogToFile($"Cache full, removing oldest texture: {textureToRemove.Key}");

                    try
                    {
                        textureToRemove.Value.PlatformTexture?.Release();
                    }
                    catch (Exception ex)
                    {
                        // Texture may have already been released by the engine
                        CECustomHandler.LogToFile($"Error releasing texture {textureToRemove.Key}: {ex.Message}");
                    }
                }

                // Add to end of cache (most recently used)
                CEPersistence.CELoadedTextures.Add(name, texture2D);

                if (texture2D.IsValid) return texture2D;
                CECustomHandler.ForceLogToFile("QuickLoadCampaignTexture failed to create Texture2D for path: " + path);

                return null;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure to load " + path + " - exception : " + e);

                return null;
            }
        }


        private void ValidateRequiredImages()
        {
            try
            {
                string[] requiredImages =
                [
                    "default_male_prison", "default_male_prison_sfw",
                    "default_female_prison", "default_female_prison_sfw",
                    "default_male", "default_male_sfw",
                    "default_female", "default_female_sfw",
                    "default_female_sea", "default_female_sea_sfw",
                    "default_male_sea", "default_male_sea_sfw",
                    "default_raft", "default_raft_sfw",
                    "CE_default_notification"
                ];

                int missingCount = 0;

                foreach (string imageName in requiredImages)
                {
                    if (!CEPersistence.CEEventImageList.ContainsKey(imageName))
                    {
                        CECustomHandler.ForceLogToFile($"WARNING: Required image '{imageName}' is missing from CEEventImageList!");
                        missingCount++;
                    }
                }

                if (missingCount > 0)
                {
                    CECustomHandler.ForceLogToFile($"Total required images missing: {missingCount} out of {requiredImages.Length}");
                }
                else
                {
                    CECustomHandler.ForceLogToFile("All required default images are present.");
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile($"Error validating required images: {e.Message}");
            }
        }

        public void ValidateAndRestoreTextures()
        {
            try
            {
                // Check if sprite sheets are still valid
                SpriteCategory spriteCategory = UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"];

                bool needsRestore = false;

                // Check if our custom sprite indices still point to valid textures
                foreach (int index in CEPersistence.SpriteIndex)
                {
                    if (index >= 0 && index < spriteCategory.SpriteSheets.Count)
                    {
                        Texture texture = spriteCategory.SpriteSheets[index];

                        if (texture == null || texture.PlatformTexture == null)
                        {
                            CECustomHandler.ForceLogToFile($"Texture at sprite_index {index} is invalid after battle, needs restore");
                            needsRestore = true;

                            break;
                        }
                    }
                    else
                    {
                        CECustomHandler.ForceLogToFile($"Sprite index {index} is out of range (count: {spriteCategory.SpriteSheets.Count}), needs restore");
                        needsRestore = true;

                        break;
                    }
                }

                // Clear invalid cached textures
                var invalidKeys = CEPersistence.CELoadedTextures.Where(kvp => kvp.Value == null || kvp.Value.PlatformTexture == null).Select(kvp => kvp.Key).ToList();

                if (invalidKeys.Count > 0)
                {
                    CECustomHandler.ForceLogToFile($"Removing {invalidKeys.Count} invalid cached textures after battle");

                    foreach (var key in invalidKeys)
                    {
                        CEPersistence.CELoadedTextures.Remove(key);
                    }

                    needsRestore = true;
                }

                // Restore if needed
                if (needsRestore)
                {
                    CECustomHandler.ForceLogToFile("Restoring textures after battle...");

                    // Clear the CETextures list to force reload
                    CEPersistence.CETextures.Clear();

                    // Reload default textures
                    LoadTexture("default");

                    CECustomHandler.ForceLogToFile("Textures restored after battle");
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile($"ValidateAndRestoreTextures failed: {e.Message}\n{e.StackTrace}");
            }
        }

        private Texture SafeLoadTexture(string name, string defaultKey)
        {
            try
            {
                if (CEPersistence.CEEventImageList.ContainsKey(name))
                {
                    Texture texture = QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    if (texture != null) return texture;
                }

                // Fallback to default if specific texture fails
                if (CEPersistence.CEEventImageList.ContainsKey(defaultKey))
                {
                    return QuickLoadCampaignTexture(CEPersistence.CEEventImageList[defaultKey]);
                }

                return null;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile($"SafeLoadTexture failed for '{name}', defaultKey '{defaultKey}': {e.Message}");

                return null;
            }
        }

        public void LoadTexture(string name, bool swap = false, bool forcelog = false)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                // Check if the image exists in the dictionary first
                if (name != "default" && !CEPersistence.CEEventImageList.ContainsKey(name))
                {
                    CECustomHandler.ForceLogToFile($"Image '{name}' not found in CEEventImageList. Available images: {CEPersistence.CEEventImageList.Count}");

                    if (forcelog)
                    {
                        InformationManager.DisplayMessage(new InformationMessage($"Image '{name}' not found. Using default. Refer to LogFileFC.txt", Colors.Red));
                    }

                    // Fall back to default
                    name = "default";
                }

                if (!swap)
                {
                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[2]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_prison"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_prison_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[3]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_prison"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_prison_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[0]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[1]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    if (_isNavalLoaded)
                    {
                        UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteSheets[CEPersistence.NavalSpriteIndex[0]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sea"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sea_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                        UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteSheets[CEPersistence.NavalSpriteIndex[1]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sea"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sea_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                        UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteSheets[CEPersistence.NavalSpriteIndex[2]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_raft"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_raft_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);
                    }
                }
                else
                {
                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[2]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_prison"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_prison_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[3]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_prison"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_prison_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[0]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[CEPersistence.SpriteIndex[1]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                    if (_isNavalLoaded)
                    {
                        UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteSheets[CEPersistence.NavalSpriteIndex[1]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sea"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sea_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                        UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteSheets[CEPersistence.NavalSpriteIndex[0]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sea"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sea_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);

                        UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteSheets[CEPersistence.NavalSpriteIndex[2]] = name == "default" ? (CESettings.Instance?.SexualContent ?? true) && (CESettings.Instance?.CustomBackgrounds ?? true) ? QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_raft"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_raft_sfw"]) : QuickLoadCampaignTexture(CEPersistence.CEEventImageList[name]);
                    }
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
                // Check if the image exists in the dictionary first
                if (name != "default" && !CEPersistence.CEEventImageList.ContainsKey(name))
                {
                    CECustomHandler.ForceLogToFile($"Notification image '{name}' not found in CEEventImageList. Using default.");

                    if (forcelog)
                    {
                        InformationManager.DisplayMessage(new InformationMessage($"Notification image '{name}' not found. Using default.", Colors.Red));
                    }

                    name = "default";
                }

                Texture texture = name == "default" ? SafeLoadTexture("CE_default_notification", "CE_default_notification") : SafeLoadTexture(name, "CE_default_notification");

                if (texture != null)
                {
                    UIResourceManager.SpriteData.SpriteCategories["ce_notification_icons"].SpriteSheets[sheet] = texture;
                }
                else
                {
                    CECustomHandler.ForceLogToFile($"Failed to load notification texture for '{name}' - texture is null");
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

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;

            ModuleInfo ceModule = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == "zCaptivityEvents");
            ModuleInfo nativeModule = ModuleHelper.GetModules().FirstOrDefault(searchInfo => searchInfo.IsNative);

            if (ceModule != null && nativeModule != null)
            {
                ApplicationVersion modversion = ceModule.Version;
                ApplicationVersion gameversion = nativeModule.Version;

                if (gameversion.Major != modversion.Major || gameversion.Minor != modversion.Minor)
                {
                    CECustomHandler.ForceLogToFile("Captivity Events " + modversion + " has the detected the wrong version " + gameversion);
                    MessageBox.Show("Warning:\n Captivity Events " + modversion + " has the detected the wrong game version. Please download the correct version for " + gameversion + ". Or continue at your own risk.", "Captivity Events has the detected the wrong version");
                }
                else
                {
                    CECustomHandler.ForceLogToFile("Captivity Events " + modversion + " has the detected the version " + gameversion);
                }
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
                                                          item.CEModuleName = modules.FirstOrDefault(moduleInfo => moduleInfo.Id == item.CEModuleName)?.Name ?? item.CEModuleName;
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
            string[] requiredImages = [.. Directory.EnumerateFiles(requiredPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))];

            // Get All in ModuleLoader
            string[] files = [.. Directory.EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))];

            // Module Image Load
            if (modulePaths.Count != 0)
            {
                foreach (string filePath in modulePaths)
                {
                    try
                    {
                        string[] moduleFiles = [.. Directory.EnumerateFiles(filePath, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"))];

                        foreach (string file in moduleFiles)
                        {
                            if (!CEPersistence.CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                            {
                                try
                                {
                                    CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), file);
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
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failure to load  - exception : " + e);
                    }
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
                            CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), file);
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
                        CEPersistence.CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), file);
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure to load textures, Critical failure. " + e);
            }

            // Validate required default images exist
            ValidateRequiredImages();

            CECustomHandler.ForceLogToFile("Loading Notification Sprites");

            try
            {
                // Load theMount & Blade II Bannerlord\Modules\SandBox\GUI\Brushes
                // MapNotification Sprite (REMEMBER TO DOUBLE CHECK FOR NEXT VERSION 1.1.1)
                SpriteData loadedData = new("CESpriteData");
                loadedData.Load(UIResourceManager.ResourceDepot);

                string categoryName = "ce_notification_icons";
                SpriteData spriteData = UIResourceManager.SpriteData;

                SpriteCategory spriteCategory = spriteData.SpriteCategories[categoryName];
                spriteCategory.SpriteSheets.Add(QuickLoadCampaignTexture(CEPersistence.CEEventImageList["CE_default_notification"]));
                spriteCategory.SpriteSheets.Add(QuickLoadCampaignTexture(CEPersistence.CEEventImageList["CE_default_notification"]));
                spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);

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
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Harmony Check Failed: " + e.Message);
                }

                CECustomHandler.ForceLogToFile(CESettings.Instance?.EventCaptorNotifications ?? true ? "Patching Map Notifications: No Conflicts Detected : Enabled." : "EventCaptorNotifications: Disabled.");

                // Patch Module.OnInitialModuleScreenActivated to play our intro after native splash screen
                try
                {
                    _harmony.Patch(
                        AccessTools.Method(typeof(TaleWorlds.MountAndBlade.Module), "OnInitialModuleScreenActivated", new Type[] { typeof(bool) }),
                        prefix: new HarmonyMethod(typeof(CESubModule), nameof(OnInitialModuleScreenActivatedPrefix))
                    );
                    CECustomHandler.ForceLogToFile("Module.OnInitialModuleScreenActivated patched successfully for intro movie");
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed to patch Module.OnInitialModuleScreenActivated: " + e.Message);
                }

                _harmony.PatchAll();
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile("Failed to load: " + ex);
                MessageBox.Show($"Error Initializing Captivity Events:\n\n{ex}");
            }

            // Check for Dramalord mod
            try
            {
                IsDramalordLoaded = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Dramalord");
                CECustomHandler.ForceLogToFile(IsDramalordLoaded ? "Dramalord mod detected, integration enabled." : "Dramalord mod not detected.");
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to check for Dramalord: " + e.Message);
            }

            if (_isLoaded) return;

            ReloadImagesAgain();

            try
            {
                if (CESettings.Instance?.IsHardCoded ?? false)
                {
                    TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(new InitialStateOption("CaptivityEventsSettings", new TextObject("Captivity Events Settings"), 9990, () => { ScreenManager.PushScreen(new CESettingsScreen()); }, () => new ValueTuple<bool, TextObject>(false, TextObject.GetEmpty())));
                }

                if (CESettingsIntegrations.Instance == null)
                {
                    CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettingsIntegrations missing MCMv5");
                }
                else
                {
                    CESettingsIntegrations.Instance.InitializeSettings();
                }

                if (CESettingsFlags.Instance == null)
                {
                    CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettingsFlags missing MCMv5");
                }
                else
                {
                    CESettingsFlags.Instance.InitializeSettings(CEPersistence.CECustomFlags);
                }

                CECustomHandler.ForceLogToFile("Loaded CESettings: " + (CESettings.Instance?.LogToggle ?? false ? "Logs are enabled." : "Extra Event Logs are disabled enable them through settings."));
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettings is being accessed improperly.");
            }

            foreach (CEEvent listedEvent in CEPersistence.CEEvents.Where(listedEvent => !string.IsNullOrWhiteSpace(listedEvent.Name)))
            {
                if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Overwritable) && (CEPersistence.CEEventList.FindAll(matchEvent => matchEvent.Name == listedEvent.Name).Count > 0 || CEPersistence.CEWaitingList.FindAll(matchEvent => matchEvent.Name == listedEvent.Name).Count > 0)) continue;

                if (!CEHelper.BrothelFlagFemale)
                {
                    if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale)) CEHelper.BrothelFlagFemale = true;
                }

                if (!CEHelper.BrothelFlagMale)
                {
                    if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale)) CEHelper.BrothelFlagMale = true;
                }

                if (listedEvent?.MenuOptions != null && listedEvent?.MenuOptions.Length != 0)
                {
                    CEPersistence.CEMenuOptionEvents.Add(listedEvent);
                }

                if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.PartyEnteredSettlement))
                {
                    CEPersistence.CEPartyEnteredSettlementEvents.Add(listedEvent);
                }

                if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.BirthAlternative))
                {
                    CEPersistence.CEAlternativePregnancyEvents.Add(listedEvent);
                }
                else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.DeathAlternative))
                {
                    CEPersistence.CEAlternativeDeathEvents.Add(listedEvent);
                }
                else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.MarriageAlternative))
                {
                    CEPersistence.CEAlternativeMarriageEvents.Add(listedEvent);
                }
                else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.DesertionAlternative))
                {
                    CEPersistence.CEAlternativeDesertionEvents.Add(listedEvent);
                }
                else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu))
                {
                    CEPersistence.CEWaitingList.Add(listedEvent);
                }
                else
                {
                    if (!listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CanOnlyBeTriggeredByOtherEvent) && listedEvent.EventType?.ToLower() != "incident")
                    {
                        int weightedChance = 1;

                        try
                        {
                            if (listedEvent.WeightedChanceOfOccurring != null) weightedChance = new CEVariablesLoader().GetIntFromXML(listedEvent.WeightedChanceOfOccurring);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.LogToFile("Missing WeightedChanceOfOccurring on " + listedEvent.Name);
                        }

                        if (weightedChance > 0)
                        {
                            CEPersistence.CECallableEvents.Add(listedEvent);
                        }
                    }


                    if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                    {
                        CEPersistence.CECaptiveEvents.Add(listedEvent);
                    }
                    else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                    {
                        CEPersistence.CERandomEvents.Add(listedEvent);
                    }
                    else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                    {
                        CEPersistence.CECaptorEvents.Add(listedEvent);
                    }

                    CEPersistence.CEEventList.Add(listedEvent);
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
                        CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettingsEvents missing MCMv5");
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
                    TextObject textObject = new("{=CEEVENTS1000}Captivity Events loaded with {EVENT_COUNT} Events and {IMAGE_COUNT} Images.");
                    TextObject textObject2 = new("{RANDOM_EVENT_COUNT} Random Events\n{CAPTOR_EVENT_COUNT} Captor Events\n{CAPTIVE_EVENT_COUNT} Captive Events\n\n{EVENT_CALLABLE_COUNT} Callable Events\n{ALTERNATIVE_EVENT_COUNT} Alternative Events");
                    TextObject textObject3 = new("\n^o^ Enjoy your events. Remember to endorse!");
                    textObject.SetTextVariable("EVENT_COUNT", CEPersistence.CEEvents.Count);
                    textObject.SetTextVariable("IMAGE_COUNT", CEPersistence.CEEventImageList.Count);
                    textObject2.SetTextVariable("EVENT_CALLABLE_COUNT", CEPersistence.CECallableEvents.Count);
                    textObject2.SetTextVariable("RANDOM_EVENT_COUNT", CEPersistence.CERandomEvents.Count);
                    textObject2.SetTextVariable("CAPTOR_EVENT_COUNT", CEPersistence.CECaptorEvents.Count);
                    textObject2.SetTextVariable("CAPTIVE_EVENT_COUNT", CEPersistence.CECaptiveEvents.Count);
                    textObject2.SetTextVariable("ALTERNATIVE_EVENT_COUNT", CEPersistence.CEAlternativeDeathEvents.Count + CEPersistence.CEAlternativeDesertionEvents.Count + CEPersistence.CEAlternativeMarriageEvents.Count + CEPersistence.CEAlternativePregnancyEvents.Count);

                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Cyan));
                    InformationManager.DisplayMessage(new InformationMessage(textObject2.ToString(), Colors.Cyan));
                    InformationManager.DisplayMessage(new InformationMessage(textObject3.ToString(), Colors.Magenta));
                    _isLoaded = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error Initializing Captivity Events:\n\n{e.GetType()}");
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

        public void ReloadImagesAgain()
        {
            CEPersistence.CELoadedTextures.Clear();

            try
            {
                CECustomHandler.ForceLogToFile("ReloadImagesAgain called - restoring CE textures to sprite categories");
                string[] modulesFound = Utilities.GetModulesNames();

                SpriteCategory spriteCategory = UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"];

                // After LoadUnloadAllCategories reloads the category, the SpriteSheets list is reset to original state
                // Check if the category has been reset by comparing current count to expected count
                int expectedCount = CEPersistence.CETexturesCount + 4; // Original count + our 4 textures
                bool categoryWasReset = spriteCategory.SpriteSheets.Count < expectedCount;

                CECustomHandler.ForceLogToFile($"ReloadImagesAgain: Current count={spriteCategory.SpriteSheets.Count}, Expected count={expectedCount}, Was reset={categoryWasReset}");

                bool needsFullReload = categoryWasReset;

                // If not reset, verify our textures are still valid
                if (!needsFullReload)
                {
                    foreach (int index in CEPersistence.SpriteIndex)
                    {
                        if (index < 0 || index >= spriteCategory.SpriteSheets.Count)
                        {
                            CECustomHandler.ForceLogToFile($"ReloadImagesAgain: sprite_index {index} out of bounds (count: {spriteCategory.SpriteSheets.Count}), needs full reload");
                            needsFullReload = true;

                            break;
                        }

                        Texture texture = spriteCategory.SpriteSheets[index];

                        if (texture == null || texture.PlatformTexture == null)
                        {
                            CECustomHandler.ForceLogToFile($"ReloadImagesAgain: texture at index {index} is invalid (null or null PlatformTexture), needs full reload");
                            needsFullReload = true;

                            break;
                        }

                        // Check if the underlying engine texture is "material_error"
                        if (texture.PlatformTexture is EngineTexture engineTexture && engineTexture.Texture != null)
                        {
                            string textureName = engineTexture.Texture.Name;

                            if (textureName == "material_error")
                            {
                                CECustomHandler.ForceLogToFile($"ReloadImagesAgain: texture at index {index} is 'material_error', needs full reload");
                                needsFullReload = true;

                                break;
                            }
                        }
                    }
                }

                if (needsFullReload)
                {
                    CECustomHandler.ForceLogToFile("ReloadImagesAgain: Performing full texture reload");

                    // Clear and reload textures
                    CEPersistence.CETextures.Clear();

                    // Force clear cached default textures so they reload from disk
                    string[] defaultTextureKeys =
                    [
                        "default_female_prison.png", "default_male_prison.png",
                        "default_female.png", "default_male.png",
                        "default_female_sea.png", "default_male_sea.png", "default_raft.png"
                    ];

                    foreach (string key in defaultTextureKeys)
                    {
                        if (CEPersistence.CELoadedTextures.ContainsKey(key))
                        {
                            CECustomHandler.ForceLogToFile($"ReloadImagesAgain: Clearing cached texture {key}");
                            CEPersistence.CELoadedTextures.Remove(key);
                        }
                    }

                    CEPersistence.CELoadedTextures.Clear();

                    // Load fresh textures from disk
                    Texture t1 = SafeLoadTexture("default_female_prison", "default_female_prison");
                    Texture t2 = SafeLoadTexture("default_male_prison", "default_male_prison");
                    Texture t3 = SafeLoadTexture("default_female", "default_female");
                    Texture t4 = SafeLoadTexture("default_male", "default_male");

                    if (t1 != null) CEPersistence.CETextures.Add(t1);
                    if (t2 != null) CEPersistence.CETextures.Add(t2);
                    if (t3 != null) CEPersistence.CETextures.Add(t3);
                    if (t4 != null) CEPersistence.CETextures.Add(t4);

                    if (CEPersistence.CETextures.Count < 4)
                    {
                        CECustomHandler.ForceLogToFile($"WARNING: Only loaded {CEPersistence.CETextures.Count} out of 4 default textures");

                        return;
                    }

                    // Add textures to sprite category
                    spriteCategory.SpriteSheets.AddRange(CEPersistence.CETextures);
                    spriteCategory.SheetSizes = spriteCategory.SheetSizes.AddRangeToArray([new Vec2i(445, 805), new Vec2i(445, 805), new Vec2i(445, 805), new Vec2i(445, 805)]);
                    CEPersistence.CETexturesCount = spriteCategory.SpriteSheetCount;
                    spriteCategory.SpriteSheetCount = spriteCategory.SpriteSheetCount + 4;
                }
                else
                {
                    CECustomHandler.ForceLogToFile("ReloadImagesAgain: Textures still valid, skipping reload");

                    return;
                }

                CECustomHandler.ForceLogToFile("Loading Textures 1.1.1");

                PropertyInfo propertyWidth = typeof(SpritePart).GetProperty("Width");
                PropertyInfo propertyHeight = typeof(SpritePart).GetProperty("Height");

                foreach (SpritePart spritePart in UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteParts)
                {
                    switch (spritePart.Name)
                    {
                        case "wait_prisoner_female":
                            spritePart.SheetID = CEPersistence.CETexturesCount + 4;
                            CEPersistence.SpriteIndex[3] = spritePart.SheetID - 1;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                            propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                            spritePart.UpdateInitValues();

                            break;

                        case "wait_prisoner_male":
                            spritePart.SheetID = CEPersistence.CETexturesCount + 3;
                            CEPersistence.SpriteIndex[2] = spritePart.SheetID - 1;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                            propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                            spritePart.UpdateInitValues();

                            break;

                        case "wait_captive_female":
                            spritePart.SheetID = CEPersistence.CETexturesCount + 2;
                            CEPersistence.SpriteIndex[1] = spritePart.SheetID - 1;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                            propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                            spritePart.UpdateInitValues();

                            break;

                        case "wait_captive_male":
                            spritePart.SheetID = CEPersistence.CETexturesCount + 1;
                            CEPersistence.SpriteIndex[0] = spritePart.SheetID - 1;
                            spritePart.SheetX = 0;
                            spritePart.SheetY = 0;
                            propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                            propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                            spritePart.UpdateInitValues();

                            break;
                    }
                }

                if (modulesFound.Contains("NavalDLC"))
                {
                    SpriteCategory spriteCategoryNaval = UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"];

                    CEPersistence.CENavalTextures = [QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_female_sea"]), QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_male_sea"]), QuickLoadCampaignTexture(CEPersistence.CEEventImageList["default_raft"])];


                    spriteCategoryNaval.SpriteSheets.AddRange(CEPersistence.CENavalTextures);
                    spriteCategoryNaval.SheetSizes = spriteCategoryNaval.SheetSizes.AddRangeToArray([new Vec2i(445, 805), new Vec2i(445, 805), new Vec2i(445, 805)]);
                    CEPersistence.CENavalTexturesCount = spriteCategoryNaval.SpriteSheetCount;
                    spriteCategoryNaval.SpriteSheetCount = spriteCategoryNaval.SpriteSheetCount + 3;

                    _isNavalLoaded = true;

                    CECustomHandler.ForceLogToFile("Loading Textures 1.3.5");

                    foreach (SpritePart spritePart in UIResourceManager.SpriteData.SpriteCategories["ui_naval_fullbackgrounds"].SpriteParts)
                    {
                        switch (spritePart.Name)
                        {
                            case "wait_captive_at_sea_female":
                                spritePart.SheetID = CEPersistence.CENavalTexturesCount + 1;
                                CEPersistence.NavalSpriteIndex[0] = spritePart.SheetID - 1;
                                spritePart.SheetX = 0;
                                spritePart.SheetY = 0;
                                propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                                propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                                spritePart.UpdateInitValues();

                                break;

                            case "wait_captive_at_sea_male":
                                spritePart.SheetID = CEPersistence.CENavalTexturesCount + 2;
                                CEPersistence.NavalSpriteIndex[1] = spritePart.SheetID - 1;
                                spritePart.SheetX = 0;
                                spritePart.SheetY = 0;
                                propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                                propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                                spritePart.UpdateInitValues();

                                break;

                            case "raft_state":
                                spritePart.SheetID = CEPersistence.CENavalTexturesCount + 3;
                                CEPersistence.NavalSpriteIndex[2] = spritePart.SheetID - 1;
                                spritePart.SheetX = 0;
                                spritePart.SheetY = 0;
                                propertyWidth?.GetSetMethod(true).Invoke(spritePart, [445]);
                                propertyHeight?.GetSetMethod(true).Invoke(spritePart, [805]);
                                spritePart.UpdateInitValues();

                                break;
                        }
                    }
                }

                LoadTexture("default", false, true);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure to load textures, OnBeforeInitialModuleScreenSetAsRoot Critical failure. " + e);
            }
        }

        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            CEConsole.CleanSave([]);
            base.OnNewGameCreated(game, initializerObject);
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is not Campaign) return;
            CleanBugs();
            ResetHelper();
            PatchDefaultClanFinanceModel();
            if (!_isLoaded) return;
            InitializeAttributes(game);
            AddBehaviors((CampaignGameStarter)gameStarter);
            CEPersistence.HotButterAvailable = CEHelper.CheckHotButter();
        }

        private void PatchDefaultClanFinanceModel()
        {
            if (!(CESettings.Instance?.ProstitutionControl ?? true)) return;
            var original = AccessTools.Method(typeof(TaleWorlds.CampaignSystem.GameComponents.DefaultClanFinanceModel), "CalculateClanIncomeInternal");
            _harmony.Patch(original, postfix: new HarmonyMethod(AccessTools.Method(typeof(Patches.CEPatchDefaultClanFinanceModel), "CalculateClanIncomeInternal")));
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
                CEPersistence.CurrentHuntState = CEPersistence.HuntState.AfterBattle;
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("CheckEncounterIssue: " + e.Message);
            }
        }

        private void ResetHelper()
        {
            CEHelper.SpouseOne = null;
            CEHelper.SpouseTwo = null;
            CEHelper.WaitMenuCheck = -1;

            CEHelper.NotificationCaptorExists = false;
            CEHelper.NotificationCaptorCheck = false;
            CEHelper.NotificationEventExists = false;
            CEHelper.NotificationEventCheck = false;
        }

        public override bool DoLoading(Game game)
        {
            if (Campaign.Current == null) return true;

            CEConsole.ReloadEvents([]);

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

        private void InitializeAttributes(Game game) => CESkills.RegisterAll(game);

        private void AddBehaviors(CampaignGameStarter campaignStarter)
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

            if (CESettings.Instance?.EventCaptiveOn ?? true)
            {
                campaignStarter.AddBehavior(new PlayerCaptivityCampaignBehavior());
            }

            // Add CE Incident Handler for converting CE events to Bannerlord incidents
            CEIncidentHandler ceIncidentHandler = new CEIncidentHandler();
            campaignStarter.AddBehavior(ceIncidentHandler);

            // Initialize incidents after behaviors are added
            ceIncidentHandler.InitializeIncidents();

            CEPrisonerDialogue prisonerDialogue = new();

            // Check and add prisoner lines (prevent duplicates on reload)
            if ((CESettings.Instance?.EventCaptorOn ?? true) && (CESettings.Instance?.EventCaptorDialogue ?? true))
            {
                prisonerDialogue.AddPrisonerLines(campaignStarter);
                CECustomHandler.LogToFile("Prisoner dialog lines added");
            }

            // Check and add custom lines
            if (CEPersistence.CECustomScenes.Count > 0)
            {
                prisonerDialogue.AddCustomLines(campaignStarter, CEPersistence.CECustomScenes);
                CECustomHandler.LogToFile("Custom dialog lines added");
            }

            if (_isLoadedInGame)
                CEConsole.ReloadEvents([]);
            else
                AddCustomEvents(campaignStarter);

            if (_isLoadedInGame) return;
            //TooltipRefresherCollection RefreshWorkshopTooltip
            InformationManager.RegisterTooltip<CEBrothel, PropertyBasedTooltipVM>(CEBrothelToolTip.BrothelTypeTooltipAction, "PropertyBasedTooltip");
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

        public void AddCustomMenuOptions(CampaignGameStarter gameStarter)
        {
            CEVariablesLoader variablesLoader = new();

            // Custom Options Load
            foreach (CEEvent customEvent in CEPersistence.CEMenuOptionEvents)
            {
                foreach (MenuOption menuOption in customEvent.MenuOptions)
                {
                    MenuCallBackDelegateRandom mcb = new(customEvent, menuOption, CEPersistence.CEEvents);
                    gameStarter.AddGameMenuOption(menuOption.MenuID, menuOption.OptionID, menuOption.OptionText, mcb.RandomEventConditionMenuOption, mcb.RandomEventConsequenceMenuOption, false, variablesLoader.GetIntFromXML(menuOption.Order), false, "CEEVENTS");
                }
            }
        }

        public void AddCustomEvents(CampaignGameStarter gameStarter)
        {
            // Custom Options Load
            if (CEPersistence.CEMenuOptionEvents.Count != 0) AddCustomMenuOptions(gameStarter);

            // Waiting Menu Load
            foreach (CEEvent waitingEvent in CEPersistence.CEWaitingList) AddEvent(gameStarter, waitingEvent, CEPersistence.CEEvents);

            // Alternative Event Load 
            foreach (CEEvent alternativeEvent in CEPersistence.CEAlternativePregnancyEvents) AddEvent(gameStarter, alternativeEvent, CEPersistence.CEEvents);
            foreach (CEEvent alternativeEvent in CEPersistence.CEAlternativeDeathEvents) AddEvent(gameStarter, alternativeEvent, CEPersistence.CEEvents);
            foreach (CEEvent alternativeEvent in CEPersistence.CEAlternativeMarriageEvents) AddEvent(gameStarter, alternativeEvent, CEPersistence.CEEvents);
            foreach (CEEvent alternativeEvent in CEPersistence.CEAlternativeDesertionEvents) AddEvent(gameStarter, alternativeEvent, CEPersistence.CEEvents);

            // Listed Event Load
            foreach (CEEvent listedEvent in CEPersistence.CEEventList) AddEvent(gameStarter, listedEvent, CEPersistence.CEEvents);
        }

        private void AddEvent(CampaignGameStarter gameStarter, CEEvent listedEvent, List<CEEvent> eventList)
        {
            CECustomHandler.LogToFile("Loading Event: " + listedEvent.Name);

            try
            {
                if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                {
                    CEEventLoader.CELoadCaptorEvent(gameStarter, listedEvent, eventList);
                }
                else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                {
                    CEEventLoader.CELoadCaptiveEvent(gameStarter, listedEvent, eventList);
                }
                else if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                {
                    CEEventLoader.CELoadRandomEvent(gameStarter, listedEvent, eventList);
                }
                else
                {
                    CECustomHandler.ForceLogToFile("Failed to load " + listedEvent.Name + " contains no category flag (Captor, Captive, Random)");
                    TextObject textObject = new("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
                    textObject.SetTextVariable("NAME", listedEvent.Name);
                    textObject.SetTextVariable("TEST", "TEST");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to load " + listedEvent.Name + " exception: " + e.Message + " stacktrace: " + e.StackTrace);

                if (!_isLoadedInGame)
                {
                    TextObject textObject = new("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
                    textObject.SetTextVariable("NAME", listedEvent.Name);
                    textObject.SetTextVariable("ERROR", e.Message);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                }
            }
        }

        private void LoadBrothelSounds()
        {
            BrothelSounds.Add("female_01_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/01/stun"));
            BrothelSounds.Add("female_02_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/02/stun"));
            BrothelSounds.Add("female_03_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/03/stun"));
            BrothelSounds.Add("female_04_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/04/stun"));
            BrothelSounds.Add("female_05_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/05/stun"));

            BrothelSounds.Add("male_01_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/01/stun"));
            BrothelSounds.Add("male_02_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/02/stun"));
            BrothelSounds.Add("male_03_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/03/stun"));
            BrothelSounds.Add("male_04_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/04/stun"));
            BrothelSounds.Add("male_05_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/05/stun"));
            BrothelSounds.Add("male_06_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/06/stun"));
            BrothelSounds.Add("male_07_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/07/stun"));
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
            if (CEPersistence.SoundLoop && CEPersistence.SoundEvent != null && Game.Current.GameStateManager.ActiveState is MapState)
            {
                try
                {
                    if (!CEPersistence.SoundEvent.IsPlaying())
                    {
                        CEPersistence.SoundEvent.Play();
                    }
                }
                catch (Exception)
                {
                    CEPersistence.SoundEvent = null;
                }
            }
        }

        // TODO MOVE TO PROPER LISTENERS AND AWAY FROM ONAPPLICATIONTICK
        private void AnimationStateCheck()
        {
            if (CEPersistence.AnimationPlayEvent && Game.Current.GameStateManager.ActiveState is MapState)
            {
                try
                {
                    if (Game.Current.ApplicationTime > _lastCheck)
                    {
                        if (CEPersistence.AnimationIndex > CEPersistence.AnimationImageList.Count() - 1) CEPersistence.AnimationIndex = 0;

                        LoadTexture(CEPersistence.AnimationImageList[CEPersistence.AnimationIndex]);
                        CEPersistence.AnimationIndex++;

                        _lastCheck = Game.Current.ApplicationTime + CEPersistence.AnimationSpeed;
                    }
                }
                catch (Exception)
                {
                    CEPersistence.AnimationPlayEvent = false;
                }
            }
        }

        private void RemoveCaptiveStateCheck()
        {
            switch (CEPersistence.CaptiveInventoryStage)
            {
                case 0:
                    break;

                case 1:
                    if (Game.Current.GameStateManager.ActiveState is InventoryState)
                    {
                        CEPersistence.CaptiveInventoryStage = 2;
                    }

                    break;

                case 2:
                    if (Game.Current.GameStateManager.ActiveState is MapState)
                    {
                        if (CEPersistence.RemoveHero != null)
                        {
                            while (MobileParty.MainParty.MemberRoster.Contains(CEPersistence.RemoveHero.CharacterObject))
                            {
                                MobileParty.MainParty.MemberRoster.RemoveTroop(CEPersistence.RemoveHero.CharacterObject);
                            }

                            CEPersistence.RemoveHero = null;
                            PartyBase.MainParty.SetVisualAsDirty();
                        }

                        CEPersistence.CaptiveInventoryStage = 0;
                    }

                    break;
            }
        }

        private void CaptiveStateCheck()
        {
            // CaptiveState
            if (!CEPersistence.CaptivePlayEvent) return;

            // Dungeon
            DungeonStateCheck();

            // Party Menu -> Map State
            if (Game.Current.GameStateManager.ActiveState is PartyState) Game.Current.GameStateManager.PopState();

            // Map State -> Play Menu
            if (Game.Current.GameStateManager.ActiveState is MapState mapState)
            {
                CEPersistence.CaptivePlayEvent = false;

                try
                {
                    if (Hero.MainHero.IsFemale)
                    {
                        CEEvent triggeredEvent = CEPersistence.CaptiveToPlay.IsFemale ? CEPersistence.CECaptorEvents.Find(item => item.Name == "CE_captor_female_sexual_menu") : CEPersistence.CECaptorEvents.Find(item => item.Name == "CE_captor_female_sexual_menu_m");
                        triggeredEvent.Captive = CEPersistence.CaptiveToPlay;

                        if (mapState.AtMenu)
                        {
                            CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }
                        else
                        {
                            CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = null;
                            CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = null;
                        }

                        CEHelper.SafeActivateGameMenu(triggeredEvent.Name);
                        mapState.MenuContext?.SetBackgroundMeshName("wait_prisoner_female");
                    }
                    else
                    {
                        CEEvent triggeredEvent = CEPersistence.CaptiveToPlay.IsFemale ? CEPersistence.CECaptorEvents.Find(item => item.Name == "CE_captor_male_sexual_menu") : CEPersistence.CECaptorEvents.Find(item => item.Name == "CE_captor_male_sexual_menu_m");
                        triggeredEvent.Captive = CEPersistence.CaptiveToPlay;

                        if (mapState.AtMenu)
                        {
                            CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }
                        else
                        {
                            CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = null;
                            CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = null;
                        }

                        CEHelper.SafeActivateGameMenu(triggeredEvent.Name);
                        mapState.MenuContext?.SetBackgroundMeshName("wait_prisoner_male");
                    }
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile(Hero.MainHero.IsFemale ? "Missing : CE_captor_female_sexual_menu/CE_captor_female_sexual_menu_m" : "Missing : CE_captor_male_sexual_menu/CE_captor_male_sexual_menu_m");
                }

                CEPersistence.CaptiveToPlay = null;
            }
        }

        private void DungeonStateCheck()
        {
            if (CEPersistence.CurrentDungeonState == CEPersistence.DungeonState.Normal) return;

            // Dungeon
            if (Game.Current.GameStateManager.ActiveState is MissionState missionStateDungeon && missionStateDungeon.CurrentMission.IsLoadingFinished)
            {
                switch (CEPersistence.CurrentDungeonState)
                {
                    case CEPersistence.DungeonState.StartWalking:
                        if (CharacterObject.OneToOneConversationCharacter == null)
                        {
                            try
                            {
                                MissionCameraFadeView behavior = Mission.Current.GetMissionBehavior<MissionCameraFadeView>();

                                Mission.Current.MainAgentServer.Controller = AgentControllerType.AI;

                                WorldPosition worldPosition = new(Mission.Current.Scene, UIntPtr.Zero, CEPersistence.GameEntity.GlobalPosition, false);

                                if (CEPersistence.AgentTalkingTo.CanBeAssignedForScriptedMovement())
                                {
                                    CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                    _dungeonFadeOut = 2f;
                                }
                                else
                                {
                                    CEPersistence.AgentTalkingTo.DisableScriptedMovement();
                                    CEPersistence.AgentTalkingTo.HandleStopUsingAction();
                                    CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                    _dungeonFadeOut = 2f;
                                }

                                behavior.BeginFadeOut(_dungeonFadeOut);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                            }

                            _brothelTimerOne = missionStateDungeon.CurrentMission.CurrentTime + _dungeonFadeOut;
                            CEPersistence.CurrentDungeonState = CEPersistence.DungeonState.FadeIn;
                        }

                        break;

                    case CEPersistence.DungeonState.FadeIn:
                        if (_brothelTimerOne < missionStateDungeon.CurrentMission.CurrentTime)
                        {
                            CEPersistence.AgentTalkingTo.ResetLookAgent();
                            CEPersistence.AgentTalkingTo.ResetAgentProperties();
                            CEPersistence.CurrentDungeonState = CEPersistence.DungeonState.Normal;
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
            if (CEPersistence.CurrentBrothelState == CEPersistence.BrothelState.Normal) return;

            if (Game.Current.GameStateManager.ActiveState is MissionState missionStateBrothel && missionStateBrothel.CurrentMission.IsLoadingFinished)
            {
                switch (CEPersistence.CurrentBrothelState)
                {
                    case CEPersistence.BrothelState.Start:
                        if (CharacterObject.OneToOneConversationCharacter == null)
                        {
                            try
                            {
                                MissionCameraFadeView behavior = Mission.Current.GetMissionBehavior<MissionCameraFadeView>();

                                Mission.Current.MainAgentServer.Controller = AgentControllerType.AI;

                                if (CEPersistence.GameEntity != null)
                                {
                                    WorldPosition worldPosition = new(Mission.Current.Scene, UIntPtr.Zero, CEPersistence.GameEntity.GlobalPosition, false);

                                    if (CEPersistence.AgentTalkingTo.CanBeAssignedForScriptedMovement())
                                    {
                                        CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                        CEPersistence.BrothelFadeIn = 3f;
                                    }
                                    else
                                    {
                                        CEPersistence.AgentTalkingTo.DisableScriptedMovement();
                                        CEPersistence.AgentTalkingTo.HandleStopUsingAction();
                                        CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                        CEPersistence.BrothelFadeIn = 3f;
                                    }
                                }

                                if (CESettingsIntegrations.Instance.ActivateHotButter && CEPersistence.HotButterAvailable)
                                {
                                    SfIn = .5f; // base .5/1/.5 goes dark, then does a come-back
                                    SfBlack = 1f; // 1/1/.5 Goes dark AFTER
                                    SfOut = .5f; // .5/1/1 not too bad but goes dark after
                                    // .5/1/1.5 stays dark
                                    // 1.5/1/.95 stays dark
                                    // 1/1/1 stays dark
                                    // 1/1/.95 stays dark
                                }
                                else
                                {
                                    SfIn = CEPersistence.BrothelFadeIn;
                                    SfBlack = CEPersistence.BrothelBlack;
                                    SfOut = CEPersistence.BrothelFadeOut;
                                }

                                behavior.BeginFadeOutAndIn(SfOut, SfBlack, SfIn);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                            }

                            _brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.BrothelFadeIn;
                            CEPersistence.CurrentBrothelState = CEPersistence.BrothelState.FadeIn;
                        }

                        break;

                    case CEPersistence.BrothelState.FadeIn:
                        if (_brothelTimerOne < missionStateBrothel.CurrentMission.CurrentTime)
                        {
                            _brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.BrothelBlack - 2f;
                            _brothelTimerTwo = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(BrothelSoundMin, BrothelSoundMax);
                            _brothelTimerThree = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(BrothelSoundMin, BrothelSoundMax);
                            Hero.MainHero.HitPoints += 10;

                            CEPersistence.AgentTalkingTo.ResetLookAgent();
                            CEPersistence.AgentTalkingTo.ResetAgentProperties();

                            CEPersistence.CurrentBrothelState = CEPersistence.BrothelState.Black;

                            if (CESettingsIntegrations.Instance != null && CESettingsIntegrations.Instance.ActivateHotButter && CEPersistence.HotButterAvailable)
                            {
                                _brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.BrothelFadeOut;
                                Mission.Current.MainAgentServer.Controller = AgentControllerType.Player;
                                CEPersistence.CurrentBrothelState = CEPersistence.BrothelState.FadeOut;

                                try
                                {
                                    string sceneToPlay = CEHelper.CustomSceneToPlay("scn_pompa_$location_culture_$location_$randomize", PartyBase.MainParty);
                                    CharacterObject hotbutterChar = (CharacterObject)CEPersistence.AgentTalkingTo.Character;

                                    CESceneNotification data = new(hotbutterChar.IsFemale ? Hero.MainHero.CharacterObject : hotbutterChar, !hotbutterChar.IsFemale ? Hero.MainHero.CharacterObject : hotbutterChar, sceneToPlay);
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
                        if (_brothelTimerOne < missionStateBrothel.CurrentMission.CurrentTime)
                        {
                            _brothelTimerOne = missionStateBrothel.CurrentMission.CurrentTime + CEPersistence.BrothelFadeOut;
                            Mission.Current.MainAgentServer.Controller = AgentControllerType.Player;
                            CEPersistence.CurrentBrothelState = CEPersistence.BrothelState.FadeOut;
                        }
                        else if (_brothelTimerTwo < missionStateBrothel.CurrentMission.CurrentTime && (!CEPersistence.HotButterAvailable || (CESettingsIntegrations.Instance == null || !CESettingsIntegrations.Instance.ActivateHotButter)))
                        {
                            _brothelTimerTwo = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(BrothelSoundMin, BrothelSoundMax);

                            try
                            {
                                int soundNumber = BrothelSounds.Where(sound => sound.Key.StartsWith(Agent.Main.GetAgentVoiceDefinition())).GetRandomElementInefficiently().Value;
                                Mission.Current.MakeSound(soundNumber, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        else if (_brothelTimerThree < missionStateBrothel.CurrentMission.CurrentTime && (!CEPersistence.HotButterAvailable || (CESettingsIntegrations.Instance == null || !CESettingsIntegrations.Instance.ActivateHotButter)))
                        {
                            _brothelTimerThree = missionStateBrothel.CurrentMission.CurrentTime + MBRandom.RandomFloatRanged(BrothelSoundMin, BrothelSoundMax);

                            try
                            {
                                int soundNumber = BrothelSounds.Where(sound => sound.Key.StartsWith(CEPersistence.AgentTalkingTo.GetAgentVoiceDefinition())).GetRandomElementInefficiently().Value;
                                Mission.Current.MakeSound(soundNumber, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        break;

                    case CEPersistence.BrothelState.FadeOut:
                        if (_brothelTimerOne < missionStateBrothel.CurrentMission.CurrentTime)
                        {
                            CEPersistence.AgentTalkingTo = null;
                            CEPersistence.CurrentBrothelState = CEPersistence.BrothelState.Normal;
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
            if (CEPersistence.CurrentHuntState == CEPersistence.HuntState.Normal) return;

            // Hunt Event States
            if ((CEPersistence.CurrentHuntState == CEPersistence.HuntState.StartHunt || CEPersistence.CurrentHuntState == CEPersistence.HuntState.HeadStart) && Game.Current.GameStateManager.ActiveState is MissionState missionState && missionState.CurrentMission.IsLoadingFinished)
            {
                try
                {
                    switch (CEPersistence.CurrentHuntState)
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

                                CEHelper.AddQuickInformation(new TextObject("{=CEEVENTS1069}Let's allow them a brief lead before the pursuit begins."), 100, CharacterObject.PlayerCharacter);

                                CEPersistence.CurrentHuntState = CEPersistence.HuntState.HeadStart;
                                _huntingTimerOne = Mission.Current.CurrentTime + (CESettings.Instance?.HuntBegins ?? 7f);
                            }

                            break;

                        case CEPersistence.HuntState.HeadStart:
                            if (Mission.Current != null && Mission.Current.Agents != null && Mission.Current.CurrentTime > _huntingTimerOne)
                            {
                                foreach (Agent agent2 in from agent in Mission.Current.Agents.ToList()
                                                         where agent.IsHuman && agent.IsEnemyOf(Agent.Main)
                                                         select agent)
                                {
                                    CommonAIComponent component = agent2.GetComponent<CommonAIComponent>();
                                    component?.Panic();
                                    agent2.SetMaximumSpeedLimit(0.5f, false);
                                }

                                CEHelper.AddQuickInformation(new TextObject("{=CEEVENTS1068}Hunt them down!"), 100, CharacterObject.PlayerCharacter, CharacterObject.PlayerCharacter.IsFemale ? "event:/voice/combat/female/01/victory" : "event:/voice/combat/male/01/victory");
                                CEPersistence.CurrentHuntState = CEPersistence.HuntState.Hunting;
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
                    CEPersistence.CurrentHuntState = CEPersistence.HuntState.Hunting;
                }
            }
            else if ((CEPersistence.CurrentHuntState == CEPersistence.HuntState.HeadStart || CEPersistence.CurrentHuntState == CEPersistence.HuntState.Hunting) && Game.Current.GameStateManager.ActiveState is MapState mapState && mapState.IsActive)
            {
                CEPersistence.CurrentHuntState = CEPersistence.HuntState.AfterBattle;
                PlayerEncounter.SetPlayerVictorious();
                if (CESettings.Instance?.HuntLetPrisonersEscape ?? false) PlayerEncounter.EnemySurrender = true;
                PlayerEncounter.Update();
            }
            else if (CEPersistence.CurrentHuntState == CEPersistence.HuntState.AfterBattle && Game.Current.GameStateManager.ActiveState is MapState mapState2 && !mapState2.IsMenuState)
                //TODO: move all of these to their proper listeners and out of the OnApplicationTick
            {
                if (PlayerEncounter.Current == null)
                {
                    CEPersistence.CurrentHuntState = CEPersistence.HuntState.Normal;
                }
                else
                {
                    //PlayerEncounter.Update();
                }
            }
        }

        private void HandleFinishBattle(MapState mapState)
        {
            CEPersistence.CurrentBattleState = CEPersistence.BattleState.Normal;

            // Validate and restore textures after battle if needed
            try
            {
                ValidateAndRestoreTextures();
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile($"HandleFinishBattle: Failed to validate textures after battle: {e.Message}");
            }

            PartyBase.MainParty.MemberRoster.RemoveIf(t => !t.Character.IsPlayerCharacter || CEPersistence.RemovePlayer);

            foreach (TroopRosterElement troopRosterElement in CEPersistence.PlayerTroops)
            {
                PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, 0, troopRosterElement.Xp);
            }

            foreach (TroopRosterElement troopRosterElement in CEPersistence.TemporaryTroops)
            {
                PartyBase.MainParty.MemberRoster.AddToCounts(troopRosterElement.Character, troopRosterElement.Number, false, 0, troopRosterElement.Xp);
            }

            foreach (TroopRosterElement troopRosterElement in CEPersistence.TemporaryTroops)
            {
                PartyBase.MainParty.MemberRoster.RemoveTroop(troopRosterElement.Character, troopRosterElement.Number);
            }


            if (CEPersistence.PlayerWon)
            {
                if (CEPersistence.VictoryEvent != null)
                {
                    // Check if victory event should be launched as an incident
                    if (CEIncidentHelper.ShouldLaunchAsIncident(CEPersistence.VictoryEvent))
                    {
                        CEIncidentHelper.LaunchBattleResultIncident(CEPersistence.VictoryEvent);
                    }
                    else
                    {
                        CEHelper.SafeActivateGameMenu(CEPersistence.VictoryEvent);
                        mapState.MenuContext?.SetBackgroundMeshName(Hero.MainHero.IsFemale ? "wait_prisoner_female" : "wait_prisoner_male");
                    }
                    CEPersistence.VictoryEvent = null;
                    CEPersistence.DefeatEvent = null;
                }
            }
            else
            {
                if (CEPersistence.DefeatEvent != null)
                {
                    // Check if defeat event should be launched as an incident
                    if (CEIncidentHelper.ShouldLaunchAsIncident(CEPersistence.DefeatEvent))
                    {
                        CEIncidentHelper.LaunchBattleResultIncident(CEPersistence.DefeatEvent);
                    }
                    else
                    {
                        CEHelper.SafeActivateGameMenu(CEPersistence.DefeatEvent);
                        mapState.MenuContext?.SetBackgroundMeshName(Hero.MainHero.IsFemale ? "wait_prisoner_female" : "wait_prisoner_male");
                    }
                    CEPersistence.VictoryEvent = null;
                    CEPersistence.DefeatEvent = null;
                }
            }
        }


        private void BattleStateCheck()
        {
            if (CEPersistence.CurrentBattleState == CEPersistence.BattleState.Normal) return;

            switch (CEPersistence.CurrentBattleState)
            {
                case CEPersistence.BattleState.StartBattle:
                    if (Game.Current.GameStateManager.ActiveState is MissionState missionState && missionState.CurrentMission.IsLoadingFinished)
                    {
                        CEPersistence.CurrentBattleState = CEPersistence.BattleState.AfterBattle;
                    }

                    break;
                case CEPersistence.BattleState.AfterBattle:
                    if (CEPersistence.CurrentBattleState == CEPersistence.BattleState.AfterBattle && Game.Current.GameStateManager.ActiveState is MapState mapState2 && !mapState2.IsMenuState)
                        //TODO: move all of these to their proper listeners and out of the OnApplicationTick
                    {
                        if (PlayerEncounter.Current == null)
                        {
                            try
                            {
                                HandleFinishBattle(mapState2);
                            }
                            catch (Exception e)
                            {
                                CECustomHandler.ForceLogToFile("BattleStateCheck: PlayerEncounter.Current == null : " + e);
                                LoadingWindow.DisableGlobalLoadingWindow();
                                CEPersistence.CurrentBattleState = CEPersistence.BattleState.Normal;
                            }
                        }
                        else
                        {
                            try
                            {
                                if (PlayerEncounter.Battle == null)
                                {
                                    TroopRoster troopRoster = PartyBase.MainParty.MemberRoster;
                                    troopRoster.AddToCounts(CharacterObject.PlayerCharacter, -1, true);
                                    CEPersistence.PlayerWon = !((troopRoster.TotalManCount == 0 && CEPersistence.PlayerDied) || CEPersistence.PlayerSurrendered);

                                    CEPersistence.PlayerSurrendered = false;
                                    CEPersistence.PlayerDied = false;

                                    troopRoster.AddToCounts(CharacterObject.PlayerCharacter, 1, true);

                                    HandleFinishBattle(mapState2);

                                    return;
                                }

                                CEPersistence.PlayerWon = PlayerEncounter.Battle.WinningSide == PlayerEncounter.Battle.PlayerSide;

                                if (PlayerEncounter.EncounteredMobileParty != null && CEPersistence.SurrenderParty)
                                {
                                    try
                                    {
                                        if (CEPersistence.PlayerWon)
                                        {
                                            PlayerEncounter.EnemySurrender = true;
                                        }
                                        else
                                        {
                                            PlayerEncounter.PlayerSurrender = true;
                                        }

                                        PlayerEncounter.Update();
                                        CEPersistence.CurrentBattleState = CEPersistence.BattleState.UpdateBattle;
                                    }
                                    catch (Exception)
                                    {
                                        PlayerEncounter.Finish();
                                    }
                                }
                                else if (PlayerEncounter.EncounteredMobileParty != null && CEPersistence.DestroyParty)
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
                                        HandleFinishBattle(mapState2);
                                    }
                                }
                                else
                                {
                                    PlayerEncounter.Current.FinalizeBattle();
                                    PlayerEncounter.Finish(false);

                                    if (Settlement.CurrentSettlement != null)
                                    {
                                        EncounterManager.StartSettlementEncounter(MobileParty.MainParty, Settlement.CurrentSettlement);
                                        HandleFinishBattle(mapState2);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                CECustomHandler.ForceLogToFile("BattleStateCheck: " + e);
                                LoadingWindow.DisableGlobalLoadingWindow();
                                CEPersistence.CurrentBattleState = CEPersistence.BattleState.Normal;
                            }
                        }
                    }

                    break;
                case CEPersistence.BattleState.UpdateBattle:
                    if (CEPersistence.CurrentBattleState == CEPersistence.BattleState.UpdateBattle && Game.Current.GameStateManager.ActiveState is MapState mapState3 && !mapState3.IsMenuState)
                        //TODO: move all of these to their proper listeners and out of the OnApplicationTick
                    {
                        try
                        {
                            if (PlayerEncounter.Current == null)
                            {
                                HandleFinishBattle(mapState3);
                            }
                            else
                            {
                                PlayerEncounter.Update();
                            }
                        }
                        catch (Exception)
                        {
                            CEPersistence.CurrentBattleState = CEPersistence.BattleState.Normal;
                        }
                    }

                    break;
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
                agent.RemoveEquippedWeapon(EquipmentIndex.ExtraWeaponSlot);

                if (agent.HasMount) agent.MountAgent.Die(new Blow(), Agent.KillInfo.Musket);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        // Harmony prefix for Module.OnInitialModuleScreenActivated
        private static bool OnInitialModuleScreenActivatedPrefix(TaleWorlds.MountAndBlade.Module __instance, bool isFromSplashScreenVideo)
        {
            try
            {
                CECustomHandler.ForceLogToFile("OnInitialModuleScreenActivatedPrefix called with isFromSplashScreenVideo: " + isFromSplashScreenVideo);

                // Only play our intro after the native splash screen has finished (isFromSplashScreenVideo should be true)
                if (isFromSplashScreenVideo && !_customIntroPlayed)
                {
                    CECustomHandler.ForceLogToFile("Native splash screen finished, checking for custom intro movie");

                    // Check if we have a custom intro movie to play
                    string customVideoPath = ModuleHelper.GetModuleFullPath("zCaptivityEvents") + "Videos/intro_custom.ivf";

                    if (File.Exists(customVideoPath))
                    {
                        CECustomHandler.ForceLogToFile("Found custom intro movie, playing: " + customVideoPath);

                        // Create VideoPlaybackState and play the custom intro movie
                        VideoPlaybackState videoPlaybackState = __instance.GlobalGameStateManager.CreateState<VideoPlaybackState>();
                        string customAudioPath = ModuleHelper.GetModuleFullPath("zCaptivityEvents") + "Videos/intro_custom.ogg";

                        videoPlaybackState.SetStartingParameters(customVideoPath, customAudioPath, string.Empty, 24f, true);

                        // Set delegate to continue to main menu after video
                        videoPlaybackState.SetOnVideoFinisedDelegate(new Action(() =>
                        {
                            CECustomHandler.ForceLogToFile("Custom intro movie finished, returning to main menu");
                            CESubModule._customIntroPlayed = true;

                            // Pop the video state to return to the main menu (InitialState)
                            if (__instance.GlobalGameStateManager.ActiveState is VideoPlaybackState)
                            {
                                __instance.GlobalGameStateManager.PopState(0);
                                CECustomHandler.ForceLogToFile("Popped video state, should now be back at main menu");
                            }
                        }));

                        __instance.GlobalGameStateManager.CleanAndPushState(videoPlaybackState, 0);

                        CECustomHandler.ForceLogToFile("Custom intro movie started after native splash screen");

                        // Return false to prevent the original method from running
                        return false;
                    }
                    else
                    {
                        CECustomHandler.ForceLogToFile("Custom intro movie not found: " + customVideoPath);
                        CESubModule._customIntroPlayed = true; // Don't try again
                        // Continue with original method
                        return true;
                    }
                }
                else
                {
                    // Either isFromSplashScreenVideo is false (not after splash) or we've already played our intro
                    CECustomHandler.ForceLogToFile("Skipping custom intro - isFromSplashScreenVideo: " + isFromSplashScreenVideo + ", already played: " + _customIntroPlayed);
                    return true;
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failed to check for custom intro movie: " + e.Message);
                CESubModule._customIntroPlayed = true; // Don't retry on error
                return true; // Continue with original method on error
            }
        }
    }
}
