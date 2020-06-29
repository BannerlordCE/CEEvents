using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CaptivityEvents.Brothel;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Models;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.TwoDimension;
using Path = System.IO.Path;
using Texture = TaleWorlds.TwoDimension.Texture;

namespace CaptivityEvents
{
    public static class CEPersistence
    {
        public enum DungeonStates
        {
            Normal,
            StartWalking,
            FadeIn
        }

        
        public enum BrothelStates
        {
            Normal,
            Start,
            FadeIn,
            Black,
            FadeOut
        }
        
        
        public enum HuntStates
        {
            Normal,
            StartHunt,
            HeadStart,
            Hunting,
            AfterBattle
        }

        // Complete Loss of Data if not static
        public static List<CEEvent> CEEvents = new List<CEEvent>();
        public static List<CEEvent> CEEventList = new List<CEEvent>();
        public static List<CEEvent> CEWaitingList = new List<CEEvent>();
        public static List<CEEvent> CECallableEvents = new List<CEEvent>();
        public static bool CaptivePlayEvent;
        public static CharacterObject CaptiveToPlay;
        public static bool AnimationPlayEvent;
        public static List<string> AnimationImageList = new List<string>();
        public static int AnimationIndex;
        public static float AnimationSpeed = 0.03f;
        public static DungeonStates DungeonState = DungeonStates.Normal;
        public static Agent AgentTalkingTo;
        public static GameEntity GameEntity = null;
        public static BrothelStates BrothelState = BrothelStates.Normal;
        public static HuntStates HuntState = HuntStates.Normal;
        public static bool NotificationExists;
        public static List<CECustom> CEFlags = new List<CECustom>();
        public static float LastCheck;
        public static readonly Dictionary<string, int> BrothelSounds = new Dictionary<string, int>();
    }



    public class CESubModule : MBSubModuleBase
    {
        private static bool _isLoaded;
        private static bool _isLoadedInGame;
        private static float dungeonFadeOut = 2.0f;
        private static float brothelTimerOne;
        private static float brothelTimerTwo;
        private static float brothelTimerThree;
        private static readonly float brothelSoundMin = 1.0f;
        private static readonly float brothelSoundMax = 3.0f;
        private static readonly Dictionary<string, Texture> CEEventImageList = new Dictionary<string, Texture>();
        private static float brothelFadeOut = 2f;
        private static float brothelBlack = 10f;
        private static float brothelFadeIn = 2f;
        //private static float playerSpeed = 0f;


        public void LoadTexture(string name, bool swap = false, bool forcelog = false)
        {
            try
            {
                if (!swap)
                {
                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[34] = name == "default"
                        ? CEEventImageList["default_female_prison"]
                        : CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[13] = name == "default"
                        ? CEEventImageList["default_female"]
                        : CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[28] = name == "default"
                        ? CEEventImageList["default_male_prison"]
                        : CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[12] = name == "default"
                        ? CEEventImageList["default_male"]
                        : CEEventImageList[name];
                }
                else
                {
                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[34] = name == "default"
                        ? CEEventImageList["default_male_prison"]
                        : CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[13] = name == "default"
                        ? CEEventImageList["default_male"]
                        : CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[28] = name == "default"
                        ? CEEventImageList["default_female_prison"]
                        : CEEventImageList[name];

                    UIResourceManager.SpriteData.SpriteCategories["ui_fullbackgrounds"].SpriteSheets[12] = name == "default"
                        ? CEEventImageList["default_female"]
                        : CEEventImageList[name];
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

        public  void LoadCampaignNotificationTexture(string name, int sheet = 0, bool forcelog = false)
        {
            try
            {
                UIResourceManager.SpriteData.SpriteCategories["ce_notification_icons"].SpriteSheets[sheet] = name == "default"
                    ? CEEventImageList["CE_default_notification"]
                    : CEEventImageList[name];
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

            var ceModule = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == "zCaptivityEvents");
            var moduleVersion = ceModule.Version;
            var nativeModule = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.IsNative());
            var gameVersion = nativeModule.Version;

            if (gameVersion.Major != moduleVersion.Major || gameVersion.Minor != moduleVersion.Minor || gameVersion.Revision != moduleVersion.Revision)
            {
                CECustomHandler.ForceLogToFile("Captivity Events " + moduleVersion + " has the detected the wrong version " + gameVersion);
                MessageBox.Show("Warning:\n Captivity Events " + moduleVersion + " has the detected the wrong game version. Please download the correct version for " + gameVersion + ". Or continue at your own risk.", "Captivity Events has the detected the wrong version");
            }

            var modulesFound = Utilities.GetModulesNames();
            var modulePaths = new List<string>();

            CECustomHandler.ForceLogToFile("\n -- Loaded Modules -- \n" + string.Join("\n", modulesFound));

            foreach (var moduleID in modulesFound)
                try
                {
                    var moduleInfo = ModuleInfo.GetModules().FirstOrDefault(searchInfo => searchInfo.Id == moduleID);

                    if (moduleInfo != null && !moduleInfo.DependedModuleIds.Contains("zCaptivityEvents")) continue;

                    try
                    {
                        if (moduleInfo == null) continue;
                        CECustomHandler.ForceLogToFile("Added to ModuleLoader: " + moduleInfo.Name);
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

            // Load Events
            CEPersistence.CEEvents = CECustomHandler.GetAllVerifiedXSEFSEvents(modulePaths);
            CEPersistence.CEFlags = CECustomHandler.GetFlags();

            // Load Images
            var fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/";
            var requiredPath = fullPath + "CaptivityRequired";

            // Get Required
            var requiredImages = Directory.GetFiles(requiredPath, "*.png", SearchOption.AllDirectories);

            // Get All in ModuleLoader
            var files = Directory.GetFiles(fullPath, "*.png", SearchOption.AllDirectories);

            // Module Image Load
            if (modulePaths.Count != 0)
                foreach (var filepath in modulePaths)
                    try
                    {
                        var moduleFiles = Directory.GetFiles(filepath, "*.png", SearchOption.AllDirectories);

                        foreach (var file in moduleFiles)
                            if (!CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                                try
                                {
                                    var texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                                    texture.PreloadTexture();
                                    var texture2D = new Texture(new EngineTexture(texture));
                                    CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                                }
                                catch (Exception e)
                                {
                                    CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                                }
                            else CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                    }
                    catch (Exception) { }

            // Captivity Location Image Load
            try
            {
                foreach (var file in files)
                {
                    if (requiredImages.Contains(file)) continue;

                    if (!CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                        try
                        {
                            var texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                            texture.PreloadTexture();
                            var texture2D = new Texture(new EngineTexture(texture));
                            CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                        }
                        catch (Exception e)
                        {
                            CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                        }
                    else CECustomHandler.ForceLogToFile("Failure to load " + file + " - duplicate found.");
                }

                foreach (var file in requiredImages)
                {
                    if (CEEventImageList.ContainsKey(Path.GetFileNameWithoutExtension(file))) continue;

                    try
                    {
                        var texture = TaleWorlds.Engine.Texture.LoadTextureFromPath($"{Path.GetFileName(file)}", $"{Path.GetDirectoryName(file)}");
                        texture.PreloadTexture();
                        var texture2D = new Texture(new EngineTexture(texture));
                        CEEventImageList.Add(Path.GetFileNameWithoutExtension(file), texture2D);
                    }
                    catch (Exception e)
                    {
                        CECustomHandler.ForceLogToFile("Failure to load " + file + " - exception : " + e);
                    }
                }

                // Load the Notifications Sprite
                // 1.4.1 Checked
                var loadedData = new SpriteData("CESpriteData");
                loadedData.Load(UIResourceManager.UIResourceDepot);

                var categoryName = "ce_notification_icons";
                var partNameCaptor = "CEEventNotification\\notification_captor";
                var partNameEvent = "CEEventNotification\\notification_event";
                var spriteData = UIResourceManager.SpriteData;
                spriteData.SpriteCategories.Add(categoryName, loadedData.SpriteCategories[categoryName]);
                spriteData.SpritePartNames.Add(partNameCaptor, loadedData.SpritePartNames[partNameCaptor]);
                spriteData.SpritePartNames.Add(partNameEvent, loadedData.SpritePartNames[partNameEvent]);
                spriteData.SpriteNames.Add(partNameCaptor, new SpriteGeneric(partNameCaptor, loadedData.SpritePartNames[partNameCaptor]));
                spriteData.SpriteNames.Add(partNameEvent, new SpriteGeneric(partNameEvent, loadedData.SpritePartNames[partNameEvent]));

                var spriteCategory = spriteData.SpriteCategories[categoryName];
                spriteCategory.SpriteSheets.Add(CEEventImageList["CE_default_notification"]);
                spriteCategory.SpriteSheets.Add(CEEventImageList["CE_default_notification"]);
                spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

                UIResourceManager.BrushFactory.Initialize();

                LoadTexture("default", false, true);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure to load textures. " + e);
            }

            CECustomHandler.ForceLogToFile("Loaded " + CEEventImageList.Count + " images and " + CEPersistence.CEEvents.Count + " events.");
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            try
            {
                CECustomHandler.ForceLogToFile("Loaded CESettings: "
                                               + (CESettings.Instance != null && CESettings.Instance.LogToggle
                                                   ? "Logs are enabled."
                                                   : "Extra Event Logs are disabled enable them through settings."));
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("OnBeforeInitialModuleScreenSetAsRoot : CESettings is being accessed improperly.");
            }

            try
            {
                var harmony = new Harmony("com.CE.captivityEvents");
                var dict = Harmony.VersionInfo(out var myVersion);
                CECustomHandler.ForceLogToFile("My version: " + myVersion);

                foreach (var entry in dict)
                {
                    var id = entry.Key;
                    var version = entry.Value;
                    CECustomHandler.ForceLogToFile("Mod " + id + " uses Harmony version " + version);
                }

                CECustomHandler.ForceLogToFile(CESettings.Instance != null && CESettings.Instance.EventCaptorNotifications
                                                   ? "Patching Map Notifications: No Conflicts Detected : Enabled."
                                                   : "EventCaptorNotifications: Disabled.");

                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile("Failed to load: " + ex);
                MessageBox.Show($"Error Initializing Captivity Events:\n\n{ex}");
            }

            foreach (var _listedEvent in CEPersistence.CEEvents.Where(_listedEvent => !_listedEvent.Name.IsStringNoneOrEmpty()))
            {
                if (_listedEvent.MultipleListOfCustomFlags != null && _listedEvent.MultipleListOfCustomFlags.Count > 0)
                    if (!CEPersistence.CEFlags.Exists(match => match.CEFlags.Any(x => _listedEvent.MultipleListOfCustomFlags.Contains(x))))
                        continue;

                if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Overwriteable) && CEPersistence.CEEvents.FindAll(matchEvent => matchEvent.Name == _listedEvent.Name).Count > 1) continue;

                if (!CEHelper.brothelFlagFemale)
                    if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale))
                        CEHelper.brothelFlagFemale = true;

                if (!CEHelper.brothelFlagMale)
                    if (_listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && _listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale))
                        CEHelper.brothelFlagMale = true;

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

            CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CEWaitingList.Count + " waiting menus ");
            CECustomHandler.ForceLogToFile("Loaded " + CEPersistence.CECallableEvents.Count + " callable events ");

            if (_isLoaded) return;

            if (CEPersistence.CEEvents.Count > 0)
            {
                try
                {
                    var textObject = new TextObject("{=CEEVENTS1000}Captivity Events Loaded with {EVENT_COUNT} Events and {IMAGE_COUNT} Images.\n^o^ Enjoy your events. Remember to endorse!");
                    textObject.SetTextVariable("EVENT_COUNT", CEPersistence.CEEvents.Count);
                    textObject.SetTextVariable("IMAGE_COUNT", CEEventImageList.Count);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Magenta));
                    _isLoaded = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error Initialising Captivity Events:\n\n{e.GetType()}");
                    CECustomHandler.ForceLogToFile("Failed to load: " + e);
                    InformationManager.DisplayMessage(new InformationMessage("{=CEEVENTS1005}Error: Captivity Events failed to load events. Please refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs. Mod is disabled.", Colors.Red));
                    _isLoaded = false;
                }
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage("{=CEEVENTS1005}Error: Captivity Events failed to load events. Please refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs. Mod is disabled.", Colors.Red));
                _isLoaded = false;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (!(game.GameType is Campaign) || !_isLoaded) return;
            game.GameTextManager.LoadGameTexts(BasePath.Name + "Modules/zCaptivityEvents/ModuleData/module_strings_xml.xml");
            InitalizeAttributes(game);
            var campaignStarter = (CampaignGameStarter)gameStarter;
            AddBehaviours(campaignStarter);
        }

        public override bool DoLoading(Game game)
        {
            if (Campaign.Current == null) return true;

            if (CESettings.Instance != null && !CESettings.Instance.PrisonerEscapeBehavior) return base.DoLoading(game);
            var dailyTickHeroEvent = CampaignEvents.DailyTickHeroEvent;

            if (dailyTickHeroEvent != null)
            {
                dailyTickHeroEvent.ClearListeners(Campaign.Current.GetCampaignBehavior<PrisonerEscapeCampaignBehavior>());
                if (CESettings.Instance != null && !CESettings.Instance.PrisonerAutoRansom) dailyTickHeroEvent.ClearListeners(Campaign.Current.GetCampaignBehavior<DiplomaticBartersBehavior>());
            }

            var hourlyPartyTick = CampaignEvents.HourlyTickPartyEvent;
            hourlyPartyTick?.ClearListeners(Campaign.Current.GetCampaignBehavior<PrisonerEscapeCampaignBehavior>());

            var barterablesRequested = CampaignEvents.BarterablesRequested;
            barterablesRequested?.ClearListeners(Campaign.Current.GetCampaignBehavior<SetPrisonerFreeBarterBehavior>());

            return base.DoLoading(game);
        }

        private void InitalizeAttributes(Game game)
        {
            CESkills.RegisterAll(game);
        }

        private void AddBehaviours(CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddBehavior(new CECampaignBehavior());
            if (CESettings.Instance != null && CESettings.Instance.ProstitutionControl) campaignStarter.AddBehavior(new CEBrothelBehavior());

            if (CESettings.Instance != null && CESettings.Instance.PrisonerEscapeBehavior)
            {
                campaignStarter.AddBehavior(new CEPrisonerEscapeCampaignBehavior());
                campaignStarter.AddBehavior(new CESetPrisonerFreeBarterBehavior());
            }

            //if (CESettings.Instance.PregnancyToggle)
            //{
            //    ReplaceModel<PregnancyModel, CEDefaultPregnancyModel>(campaignStarter);
            //}
            if (CESettings.Instance != null && CESettings.Instance.EventCaptiveOn) ReplaceModel<PlayerCaptivityModel, CEPlayerCaptivityModel>(campaignStarter);

            if (CESettings.Instance != null && (CESettings.Instance.EventCaptorOn && CESettings.Instance.EventCaptorDialogue)) new CEPrisonerDialogue().AddPrisonerLines(campaignStarter);

            AddCustomEvents(campaignStarter);

            if (_isLoadedInGame) return;
            TooltipVM.AddTooltipType(typeof(CEBrothel), CEBrothelToolTip.BrothelTypeTooltipAction);
            LoadBrothelSounds();
            _isLoadedInGame = true;
        }

        protected void ReplaceModel<TBaseType, TChildType>(IGameStarter gameStarter) where TBaseType : GameModel where TChildType : GameModel
        {
            if (!(gameStarter.Models is IList<GameModel> list)) return;
            var flag = false;

            for (var i = 0; i < list.Count; i++)
                if (list[i] is TBaseType)
                {
                    flag = true;
                    if (!(list[i] is TChildType)) list[i] = Activator.CreateInstance<TChildType>();
                }

            if (!flag) gameStarter.AddModel(Activator.CreateInstance<TChildType>());
        }

        protected void ReplaceBehaviour<TBaseType, TChildType>(CampaignGameStarter gameStarter) where TBaseType : CampaignBehaviorBase where TChildType : CampaignBehaviorBase
        {
            if (!(gameStarter.CampaignBehaviors is IList<CampaignBehaviorBase> list)) return;
            var flag = false;

            for (var i = 0; i < list.Count; i++)
                if (list[i] is TBaseType)
                {
                    flag = true;
                    if (!(list[i] is TChildType)) list[i] = Activator.CreateInstance<TChildType>();
                }

            if (!flag) gameStarter.AddBehavior(Activator.CreateInstance<TChildType>());
        }

        private void AddCustomEvents(CampaignGameStarter gameStarter)
        {
            // Waiting Menu Load
            foreach (var waitingEvent in CEPersistence.CEWaitingList) AddEvent(gameStarter, waitingEvent, CEPersistence.CEEvents);

            // Listed Event Load
            foreach (var listedEvent in CEPersistence.CEEventList) AddEvent(gameStarter, listedEvent, CEPersistence.CEEvents);
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
                    var textObject = new TextObject("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
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
                    var textObject = new TextObject("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
                    textObject.SetTextVariable("NAME", _listedEvent.Name);
                    textObject.SetTextVariable("ERROR", e.Message);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                }
            }
        }

        private void LoadBrothelSounds()
        {
            CEPersistence.BrothelSounds.Add("female_01_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/01/stun"));
            CEPersistence.BrothelSounds.Add("female_02_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/02/stun"));
            CEPersistence.BrothelSounds.Add("female_03_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/03/stun"));
            CEPersistence.BrothelSounds.Add("female_04_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/04/stun"));
            CEPersistence.BrothelSounds.Add("female_05_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/female/05/stun"));

            CEPersistence.BrothelSounds.Add("male_01_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/01/stun"));
            CEPersistence.BrothelSounds.Add("male_02_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/02/stun"));
            CEPersistence.BrothelSounds.Add("male_03_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/03/stun"));
            CEPersistence.BrothelSounds.Add("male_04_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/04/stun"));
            CEPersistence.BrothelSounds.Add("male_05_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/05/stun"));
            CEPersistence.BrothelSounds.Add("male_06_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/06/stun"));
            CEPersistence.BrothelSounds.Add("male_07_stun", SoundEvent.GetEventIdFromString("event:/voice/combat/male/07/stun"));
        }


        protected override void OnApplicationTick(float dt)
        {
            if (Game.Current == null || Game.Current.GameStateManager == null) return;

            // CaptiveState
            if (CEPersistence.CaptivePlayEvent)
            {
                // Dungeon
                if (CEPersistence.DungeonState != CEPersistence.DungeonStates.Normal && Game.Current.GameStateManager.ActiveState is MissionState missionStateDungeon && missionStateDungeon.CurrentMission.IsLoadingFinished)
                    switch (CEPersistence.DungeonState)
                    {
                        case CEPersistence.DungeonStates.StartWalking:
                            if (CharacterObject.OneToOneConversationCharacter == null)
                            {
                                try
                                {
                                    var behaviour = Mission.Current.GetMissionBehaviour<MissionCameraFadeView>();

                                    Mission.Current.MainAgentServer.Controller = Agent.ControllerType.AI;

                                    var worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, CEPersistence.GameEntity.GlobalPosition, false);

                                    if (CEPersistence.AgentTalkingTo.CanBeAssignedForScriptedMovement())
                                    {
                                        CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                        dungeonFadeOut = 2f;
                                    }
                                    else
                                    {
                                        CEPersistence.AgentTalkingTo.DisableScriptedMovement();
                                        CEPersistence.AgentTalkingTo.HandleStopUsingAction();
                                        CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                        dungeonFadeOut = 2f;
                                    }

                                    behaviour.BeginFadeOut(dungeonFadeOut);
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                                }

                                brothelTimerOne = missionStateDungeon.CurrentMission.Time + dungeonFadeOut;
                                CEPersistence.DungeonState = CEPersistence.DungeonStates.FadeIn;
                            }

                            break;
                        case CEPersistence.DungeonStates.FadeIn:
                            if (brothelTimerOne < missionStateDungeon.CurrentMission.Time)
                            {
                                CEPersistence.AgentTalkingTo.ResetAI();
                                CEPersistence.DungeonState = CEPersistence.DungeonStates.Normal;
                                Mission.Current.EndMission();
                            }

                            break;
                        case CEPersistence.DungeonStates.Normal:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                // Party Menu -> Map State
                if (Game.Current.GameStateManager.ActiveState is PartyState) Game.Current.GameStateManager.PopState();

                // Map State -> Play Menu
                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                {
                    CEPersistence.CaptivePlayEvent = false;

                    if (Hero.MainHero.IsFemale)
                    {

                        try
                        {
                            var triggeredEvent = CEPersistence.CaptiveToPlay.IsFemale ? CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_female_sexual_menu") : CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_female_sexual_menu_m");
                            triggeredEvent.Captive = CEPersistence.CaptiveToPlay;

                            if (!mapState.AtMenu)
                            {
                                GameMenu.ActivateGameMenu("prisoner_wait");
                            }
                            else
                            {
                                CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                                CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                            }

                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                            mapState.MenuContext.SetBackgroundMeshName("wait_prisoner_female");
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Missing : CE_captor_female_sexual_menu/CE_captor_female_sexual_menu_m");
                        }


                    }
                    else
                    {

                        try
                        {
                            var triggeredEvent = CEPersistence.CaptiveToPlay.IsFemale ? CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_male_sexual_menu") : CEPersistence.CEEventList.Find(item => item.Name == "CE_captor_male_sexual_menu_m");
                            triggeredEvent.Captive = CEPersistence.CaptiveToPlay;

                            if (!mapState.AtMenu)
                            {
                                GameMenu.ActivateGameMenu("prisoner_wait");
                            }
                            else
                            {
                                CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                                CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                            }

                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                            mapState.MenuContext.SetBackgroundMeshName("wait_prisoner_male");
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Missing : CE_captor_male_sexual_menu/CE_captor_male_sexual_menu_m");
                        }

                    }

                    CEPersistence.CaptiveToPlay = null;
                }
            }

            // Animated Background Menus
            if (CEPersistence.AnimationPlayEvent && Game.Current.GameStateManager.ActiveState is MapState)
                try
                {
                    if (Game.Current.ApplicationTime > CEPersistence.LastCheck)
                    {
                        if (CEPersistence.AnimationIndex > CEPersistence.AnimationImageList.Count() - 1) CEPersistence.AnimationIndex = 0;

                        LoadTexture(CEPersistence.AnimationImageList[CEPersistence.AnimationIndex]);
                        CEPersistence.AnimationIndex++;

                        CEPersistence.LastCheck = Game.Current.ApplicationTime + CEPersistence.AnimationSpeed;
                    }
                }
                catch (Exception)
                {
                    CEPersistence.AnimationPlayEvent = false;
                }

            // Brothel Event To Play
            if (CEPersistence.BrothelState != CEPersistence.BrothelStates.Normal && Game.Current.GameStateManager.ActiveState is MissionState missionStateBrothel && missionStateBrothel.CurrentMission.IsLoadingFinished)
                switch (CEPersistence.BrothelState)
                {
                    case CEPersistence.BrothelStates.Start:
                        if (CharacterObject.OneToOneConversationCharacter == null)
                        {
                            try
                            {
                                var behaviour = Mission.Current.GetMissionBehaviour<MissionCameraFadeView>();

                                Mission.Current.MainAgentServer.Controller = Agent.ControllerType.AI;

                                var worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, CEPersistence.GameEntity.GlobalPosition, false);

                                if (CEPersistence.AgentTalkingTo.CanBeAssignedForScriptedMovement())
                                {
                                    CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                    Mission.Current.MainAgent.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                    brothelFadeIn = 3f;
                                }
                                else
                                {
                                    CEPersistence.AgentTalkingTo.DisableScriptedMovement();
                                    CEPersistence.AgentTalkingTo.HandleStopUsingAction();
                                    CEPersistence.AgentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                    brothelFadeIn = 3f;
                                }

                                behaviour.BeginFadeOutAndIn(brothelFadeIn, brothelBlack, brothelFadeOut);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                            }

                            brothelTimerOne = missionStateBrothel.CurrentMission.Time + brothelFadeIn;
                            CEPersistence.BrothelState = CEPersistence.BrothelStates.FadeIn;
                        }

                        break;

                    case CEPersistence.BrothelStates.FadeIn:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.Time)
                        {
                            brothelTimerOne = missionStateBrothel.CurrentMission.Time + brothelBlack;
                            brothelTimerTwo = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);
                            brothelTimerThree = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            Hero.MainHero.HitPoints += 10;

                            CEPersistence.AgentTalkingTo.ResetAI();
                            Mission.Current.MainAgent.TeleportToPosition(CEPersistence.GameEntity.GlobalPosition);
                            CEPersistence.BrothelState = CEPersistence.BrothelStates.Black;
                        }

                        break;

                    case CEPersistence.BrothelStates.Black:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.Time)
                        {
                            Mission.Current.MainAgentServer.Controller = Agent.ControllerType.Player;

                            brothelTimerOne = missionStateBrothel.CurrentMission.Time + brothelFadeOut;
                            CEPersistence.BrothelState = CEPersistence.BrothelStates.FadeOut;
                        }
                        else if (brothelTimerTwo < missionStateBrothel.CurrentMission.Time)
                        {
                            brothelTimerTwo = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            try
                            {
                                var soundnum = CEPersistence.BrothelSounds.Where(sound => { return sound.Key.StartsWith(Agent.Main.GetAgentVoiceDefinition()); }).GetRandomElement().Value;
                                Mission.Current.MakeSound(soundnum, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception) { }
                        }
                        else if (brothelTimerThree < missionStateBrothel.CurrentMission.Time)
                        {
                            brothelTimerThree = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            try
                            {
                                var soundnum = CEPersistence.BrothelSounds.Where(sound => { return sound.Key.StartsWith(CEPersistence.AgentTalkingTo.GetAgentVoiceDefinition()); }).GetRandomElement().Value;
                                Mission.Current.MakeSound(soundnum, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception) { }
                        }

                        break;

                    case CEPersistence.BrothelStates.FadeOut:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.Time)
                        {
                            CEPersistence.AgentTalkingTo = null;
                            CEPersistence.BrothelState = CEPersistence.BrothelStates.Normal;
                        }

                        break;
                    case CEPersistence.BrothelStates.Normal:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            // Hunt Event To Play
            if (CEPersistence.HuntState == CEPersistence.HuntStates.Normal) return;

            // Hunt Event States
            if ((CEPersistence.HuntState == CEPersistence.HuntStates.StartHunt || CEPersistence.HuntState == CEPersistence.HuntStates.HeadStart) && Game.Current.GameStateManager.ActiveState is MissionState missionState && missionState.CurrentMission.IsLoadingFinished)
            {
                try
                {
                    switch (CEPersistence.HuntState)
                    {
                        case CEPersistence.HuntStates.StartHunt:
                            if (Mission.Current != null && Mission.Current.IsLoadingFinished && Mission.Current.Time > 2f && Mission.Current.Agents != null)
                            {
                                foreach (var agent2 in from agent in Mission.Current.Agents
                                                       where agent.IsHuman && agent.IsEnemyOf(Agent.Main)
                                                       select agent) ForceAgentDropEquipment(agent2);
                                missionState.CurrentMission.ClearCorpses();

                                InformationManager.AddQuickInformation(new TextObject("{=CEEVENTS1069}Let's give them a headstart."), 100, CharacterObject.PlayerCharacter);
                                CEPersistence.HuntState = CEPersistence.HuntStates.HeadStart;
                            }

                            break;

                        case CEPersistence.HuntStates.HeadStart:
                            if (Mission.Current != null && Mission.Current.Time > CESettings.Instance.HuntBegins && Mission.Current.Agents != null)
                            {
                                foreach (var agent2 in from agent in Mission.Current.Agents
                                                       where agent.IsHuman && agent.IsEnemyOf(Agent.Main)
                                                       select agent)
                                {
                                    var component = agent2.GetComponent<MoraleAgentComponent>();
                                    component?.Panic();
                                    agent2.DestinationSpeed = 0.5f;
                                }

                                InformationManager.AddQuickInformation(new TextObject("{=CEEVENTS1068}Hunt them down!"), 100, CharacterObject.PlayerCharacter, CharacterObject.PlayerCharacter.IsFemale
                                                                           ? "event:/voice/combat/female/01/victory"
                                                                           : "event:/voice/combat/male/01/victory");
                                CEPersistence.HuntState = CEPersistence.HuntStates.Hunting;
                            }

                            break;
                        case CEPersistence.HuntStates.Normal:
                        case CEPersistence.HuntStates.Hunting:
                        case CEPersistence.HuntStates.AfterBattle:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed on hunting mission: " + e);
                    CEPersistence.HuntState = CEPersistence.HuntStates.Hunting;
                }
            }
            else if ((CEPersistence.HuntState == CEPersistence.HuntStates.HeadStart || CEPersistence.HuntState == CEPersistence.HuntStates.Hunting) && Game.Current.GameStateManager.ActiveState is MapState mapstate && mapstate.IsActive) 
                //warning why mapstate? // PS Add TODO for questions if you have hard to find them in file. 
                
                //mapstate is to prevent a crash that occurs if the player encounter update updates before the active state of map state is up, the only way around it is declare the map state variable.
            {
                CEPersistence.HuntState = CEPersistence.HuntStates.AfterBattle;
                PlayerEncounter.SetPlayerVictorious();
                if (CESettings.Instance.HuntLetPrisonersEscape) PlayerEncounter.EnemySurrender = true;
                PlayerEncounter.Update();
            }
            else if (CEPersistence.HuntState == CEPersistence.HuntStates.AfterBattle && Game.Current.GameStateManager.ActiveState is MapState mapstate2 && !mapstate2.IsMenuState) //warning why mapstate?
                //Checks to see if mapstate is not in menu state, should connect it to the on menu exit listener.
                //TODO: move all of these to their proper listeners and out of the OnApplicationTick
            {
                if (PlayerEncounter.Current == null)
                {
                    LoadingWindow.DisableGlobalLoadingWindow();
                    CEPersistence.HuntState = CEPersistence.HuntStates.Normal;
                }
                else
                {
                    PlayerEncounter.Update();
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