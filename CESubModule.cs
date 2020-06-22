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
    public class CESubModule : MBSubModuleBase
    {
        private bool _isLoaded;
        private bool _isLoadedInGame;

        public static List<CEEvent> CEEvents = new List<CEEvent>();
        public static List<CECustom> CEFlags = new List<CECustom>();

        public static List<CEEvent> CEEventList = new List<CEEvent>();
        public static List<CEEvent> CEWaitingList = new List<CEEvent>();
        public static List<CEEvent> CECallableEvents = new List<CEEvent>();

        private static readonly Dictionary<string, Texture> CEEventImageList = new Dictionary<string, Texture>();

        // Captive Menu
        public static bool captivePlayEvent;

        public static CharacterObject captiveToPlay;

        // Animation Flags
        public static bool animationPlayEvent;

        public static List<string> animationImageList = null;
        public static int animationIndex;
        public static float animationSpeed = 0.03f;

        // Last Check
        private static float lastCheck;

        // Dungeon State
        public enum DungeonState
        {
            Normal,
            StartWalking,
            FadeIn
        }

        private static float dungeonFadeOut = 2f;
        public static DungeonState dungeonState = DungeonState.Normal;

        // Brothel State
        public enum BrothelState
        {
            Normal,
            Start,
            FadeIn,
            Black,
            FadeOut
        }

        public static Agent agentTalkingTo;
        public static GameEntity gameEntity = null;
        public static float playerSpeed = 0f;
        public static BrothelState brothelState = BrothelState.Normal;

        public static float brothelFadeIn = 2f;
        public static float brothelBlack = 10f;
        public static float brothelFadeOut = 2f;

        private static float brothelTimerOne;
        private static float brothelTimerTwo;
        private static float brothelTimerThree;

        private static readonly float brothelSoundMin = 1f;
        private static readonly float brothelSoundMax = 3f;

        private readonly Dictionary<string, int> brothelSounds = new Dictionary<string, int>();

        // Hunt State
        public enum HuntState
        {
            Normal,
            StartHunt,
            HeadStart,
            Hunting,
            AfterBattle
        }

        public static HuntState huntState = HuntState.Normal;
        public static bool notificationExists;

        public static void LoadTexture(string name, bool swap = false, bool forcelog = false)
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

        public static void LoadCampaignNotificationTexture(string name, int sheet = 0, bool forcelog = false)
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

            var ceModule = ModuleInfo.GetModules().FirstOrDefault(searchInfo => { return searchInfo.Id == "zCaptivityEvents"; });
            var modversion = ceModule.Version;
            var nativeModule = ModuleInfo.GetModules().FirstOrDefault(searchInfo => { return searchInfo.IsNative(); });
            var gameversion = nativeModule.Version;

            if (gameversion.Major != modversion.Major || gameversion.Minor != modversion.Minor || gameversion.Revision != modversion.Revision)
            {
                CECustomHandler.ForceLogToFile("Captivity Events " + modversion + " has the detected the wrong version " + gameversion);
                MessageBox.Show("Warning:\n Captivity Events " + modversion + " has the detected the wrong game version. Please download the correct version for " + gameversion + ". Or continue at your own risk.", "Captivity Events has the detected the wrong version");
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
            CEEvents = CECustomHandler.GetAllVerifiedXSEFSEvents(modulePaths);
            CEFlags = CECustomHandler.GetFlags();

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

            CECustomHandler.ForceLogToFile("Loaded " + CEEventImageList.Count + " images and " + CEEvents.Count + " events.");
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

            foreach (var listedEvent in CEEvents.Where(listedEvent => !listedEvent.Name.IsStringNoneOrEmpty()))
            {
                if (listedEvent.MultipleListOfCustomFlags != null && listedEvent.MultipleListOfCustomFlags.Count > 0)
                    if (!CEFlags.Exists(match => match.CEFlags.Any(x => listedEvent.MultipleListOfCustomFlags.Contains(x))))
                        continue;

                if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Overwriteable) && CEEvents.FindAll(matchEvent => matchEvent.Name == listedEvent.Name).Count > 1) continue;

                if (!CEHelper.brothelFlagFemale)
                    if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsFemale))
                        CEHelper.brothelFlagFemale = true;

                if (!CEHelper.brothelFlagMale)
                    if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.LocationCity) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroIsProstitute) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Prostitution) && listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.HeroGenderIsMale))
                        CEHelper.brothelFlagMale = true;

                if (listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.WaitingMenu))
                {
                    CEWaitingList.Add(listedEvent);
                }
                else
                {
                    if (!listedEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.CanOnlyBeTriggeredByOtherEvent)) CECallableEvents.Add(listedEvent);

                    CEEventList.Add(listedEvent);
                }
            }

            CECustomHandler.ForceLogToFile("Loaded " + CEWaitingList.Count + " waiting menus ");
            CECustomHandler.ForceLogToFile("Loaded " + CECallableEvents.Count + " callable events ");

            if (_isLoaded) return;

            if (CEEvents.Count > 0)
            {
                try
                {
                    var textObject = new TextObject("{=CEEVENTS1000}Captivity Events Loaded with {EVENT_COUNT} Events and {IMAGE_COUNT} Images.\n^o^ Enjoy your events. Remember to endorse!");
                    textObject.SetTextVariable("EVENT_COUNT", CEEvents.Count);
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
            var campaignStarter = (CampaignGameStarter) gameStarter;
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

            if (CESettings.Instance != null && (CESettings.Instance.EventCaptorOn && CESettings.Instance.EventCaptorDialogue)) CEPrisonerDialogue.AddPrisonerLines(campaignStarter);

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
            foreach (var waitingEvent in CEWaitingList) AddEvent(gameStarter, waitingEvent, CEEvents);

            // Listed Event Load
            foreach (var listedEvent in CEEventList) AddEvent(gameStarter, listedEvent, CEEvents);
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
                    var textObject = new TextObject("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
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
                    var textObject = new TextObject("{=CEEVENTS1004}Failed to load event {NAME} : {ERROR} refer to logs in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs for more information");
                    textObject.SetTextVariable("NAME", listedEvent.Name);
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
            if (captivePlayEvent)
            {
                // Dungeon
                if (dungeonState != DungeonState.Normal && Game.Current.GameStateManager.ActiveState is MissionState missionStateDungeon && missionStateDungeon.CurrentMission.IsLoadingFinished)
                    switch (dungeonState)
                    {
                        case DungeonState.StartWalking:
                            if (CharacterObject.OneToOneConversationCharacter == null)
                            {
                                try
                                {
                                    var behaviour = Mission.Current.GetMissionBehaviour<MissionCameraFadeView>();

                                    Mission.Current.MainAgentServer.Controller = Agent.ControllerType.AI;

                                    var worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, gameEntity.GlobalPosition, false);

                                    if (agentTalkingTo.CanBeAssignedForScriptedMovement())
                                    {
                                        agentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                        dungeonFadeOut = 2f;
                                    }
                                    else
                                    {
                                        agentTalkingTo.DisableScriptedMovement();
                                        agentTalkingTo.HandleStopUsingAction();
                                        agentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                        dungeonFadeOut = 2f;
                                    }

                                    behaviour.BeginFadeOut(dungeonFadeOut);
                                }
                                catch (Exception)
                                {
                                    CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                                }

                                brothelTimerOne = missionStateDungeon.CurrentMission.Time + dungeonFadeOut;
                                dungeonState = DungeonState.FadeIn;
                            }

                            break;
                        case DungeonState.FadeIn:
                            if (brothelTimerOne < missionStateDungeon.CurrentMission.Time)
                            {
                                agentTalkingTo.ResetAI();
                                dungeonState = DungeonState.Normal;
                                Mission.Current.EndMission();
                            }

                            break;
                        case DungeonState.Normal:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                // Party Menu -> Map State
                if (Game.Current.GameStateManager.ActiveState is PartyState) Game.Current.GameStateManager.PopState();

                // Map State -> Play Menu
                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                {
                    captivePlayEvent = false;

                    if (Hero.MainHero.IsFemale)
                    {
                        if (!mapState.AtMenu)
                        {
                            GameMenu.ActivateGameMenu("prisoner_wait");
                        }
                        else
                        {
                            CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }

                        var triggeredEvent = captiveToPlay.IsFemale
                            ? CEEventList.Find(item => item.Name == "CE_captor_female_sexual_menu")
                            : CEEventList.Find(item => item.Name == "CE_captor_female_sexual_menu_m");
                        triggeredEvent.Captive = captiveToPlay;

                        try
                        {
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Missing : " + triggeredEvent.Name);
                        }

                        mapState.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                                       ? "wait_prisoner_female"
                                                                       : "wait_prisoner_male");
                    }
                    else
                    {
                        if (!mapState.AtMenu)
                        {
                            GameMenu.ActivateGameMenu("prisoner_wait");
                        }
                        else
                        {
                            CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }

                        var triggeredEvent = captiveToPlay.IsFemale
                            ? CEEventList.Find(item => item.Name == "CE_captor_male_sexual_menu")
                            : CEEventList.Find(item => item.Name == "CE_captor_male_sexual_menu_m");
                        triggeredEvent.Captive = captiveToPlay;

                        try
                        {
                            GameMenu.SwitchToMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Missing : " + triggeredEvent.Name);
                        }

                        mapState.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                                       ? "wait_prisoner_female"
                                                                       : "wait_prisoner_male");
                    }

                    captiveToPlay = null;
                }
            }

            // Animated Background Menus
            if (animationPlayEvent && Game.Current.GameStateManager.ActiveState is MapState)
                try
                {
                    if (Game.Current.ApplicationTime > lastCheck)
                    {
                        if (animationIndex > animationImageList.Count() - 1) animationIndex = 0;

                        LoadTexture(animationImageList[animationIndex]);
                        animationIndex++;

                        lastCheck = Game.Current.ApplicationTime + animationSpeed;
                    }
                }
                catch (Exception)
                {
                    animationPlayEvent = false;
                }

            // Brothel Event To Play
            if (brothelState != BrothelState.Normal && Game.Current.GameStateManager.ActiveState is MissionState missionStateBrothel && missionStateBrothel.CurrentMission.IsLoadingFinished)
                switch (brothelState)
                {
                    case BrothelState.Start:
                        if (CharacterObject.OneToOneConversationCharacter == null)
                        {
                            try
                            {
                                var behaviour = Mission.Current.GetMissionBehaviour<MissionCameraFadeView>();

                                Mission.Current.MainAgentServer.Controller = Agent.ControllerType.AI;

                                var worldPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, gameEntity.GlobalPosition, false);

                                if (agentTalkingTo.CanBeAssignedForScriptedMovement())
                                {
                                    agentTalkingTo.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                    Mission.Current.MainAgent.SetScriptedPosition(ref worldPosition, true, Agent.AIScriptedFrameFlags.DoNotRun);
                                    brothelFadeIn = 3f;
                                }
                                else
                                {
                                    agentTalkingTo.DisableScriptedMovement();
                                    agentTalkingTo.HandleStopUsingAction();
                                    agentTalkingTo.SetScriptedPosition(ref worldPosition, false, Agent.AIScriptedFrameFlags.DoNotRun);
                                    brothelFadeIn = 3f;
                                }

                                behaviour.BeginFadeOutAndIn(brothelFadeIn, brothelBlack, brothelFadeOut);
                            }
                            catch (Exception)
                            {
                                CECustomHandler.ForceLogToFile("Failed MissionCameraFadeView.");
                            }

                            brothelTimerOne = missionStateBrothel.CurrentMission.Time + brothelFadeIn;
                            brothelState = BrothelState.FadeIn;
                        }

                        break;

                    case BrothelState.FadeIn:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.Time)
                        {
                            brothelTimerOne = missionStateBrothel.CurrentMission.Time + brothelBlack;
                            brothelTimerTwo = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);
                            brothelTimerThree = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            Hero.MainHero.HitPoints += 10;

                            agentTalkingTo.ResetAI();
                            Mission.Current.MainAgent.TeleportToPosition(gameEntity.GlobalPosition);
                            brothelState = BrothelState.Black;
                        }

                        break;

                    case BrothelState.Black:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.Time)
                        {
                            Mission.Current.MainAgentServer.Controller = Agent.ControllerType.Player;

                            brothelTimerOne = missionStateBrothel.CurrentMission.Time + brothelFadeOut;
                            brothelState = BrothelState.FadeOut;
                        }
                        else if (brothelTimerTwo < missionStateBrothel.CurrentMission.Time)
                        {
                            brothelTimerTwo = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            try
                            {
                                var soundnum = brothelSounds.Where(sound => { return sound.Key.StartsWith(Agent.Main.GetAgentVoiceDefinition()); }).GetRandomElement().Value;
                                Mission.Current.MakeSound(soundnum, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception) { }
                        }
                        else if (brothelTimerThree < missionStateBrothel.CurrentMission.Time)
                        {
                            brothelTimerThree = missionStateBrothel.CurrentMission.Time + MBRandom.RandomFloatRanged(brothelSoundMin, brothelSoundMax);

                            try
                            {
                                var soundnum = brothelSounds.Where(sound => { return sound.Key.StartsWith(agentTalkingTo.GetAgentVoiceDefinition()); }).GetRandomElement().Value;
                                Mission.Current.MakeSound(soundnum, Agent.Main.Frame.origin, true, false, -1, -1);
                            }
                            catch (Exception) { }
                        }

                        break;

                    case BrothelState.FadeOut:
                        if (brothelTimerOne < missionStateBrothel.CurrentMission.Time)
                        {
                            agentTalkingTo = null;
                            brothelState = BrothelState.Normal;
                        }

                        break;
                    case BrothelState.Normal:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            // Hunt Event To Play
            if (huntState == HuntState.Normal) return;

            // Hunt Event States
            if ((huntState == HuntState.StartHunt || huntState == HuntState.HeadStart) && Game.Current.GameStateManager.ActiveState is MissionState missionState && missionState.CurrentMission.IsLoadingFinished)
            {
                try
                {
                    switch (huntState)
                    {
                        case HuntState.StartHunt:
                            if (Mission.Current != null && Mission.Current.IsLoadingFinished && Mission.Current.Time > 2f && Mission.Current.Agents != null)
                            {
                                foreach (var agent2 in from agent in Mission.Current.Agents
                                                       where agent.IsHuman && agent.IsEnemyOf(Agent.Main)
                                                       select agent) ForceAgentDropEquipment(agent2);
                                missionState.CurrentMission.ClearCorpses();

                                InformationManager.AddQuickInformation(new TextObject("{=CEEVENTS1069}Let's give them a headstart."), 100, CharacterObject.PlayerCharacter);
                                huntState = HuntState.HeadStart;
                            }

                            break;

                        case HuntState.HeadStart:
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
                                huntState = HuntState.Hunting;
                            }

                            break;
                        case HuntState.Normal:
                        case HuntState.Hunting:
                        case HuntState.AfterBattle:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    CECustomHandler.ForceLogToFile("Failed on hunting mission: " + e);
                    huntState = HuntState.Hunting;
                }
            }
            else if ((huntState == HuntState.HeadStart || huntState == HuntState.Hunting) && Game.Current.GameStateManager.ActiveState is MapState mapstate) //warning why mapstate?
            {
                huntState = HuntState.AfterBattle;
                PlayerEncounter.SetPlayerVictorious();
                if (CESettings.Instance.HuntLetPrisonersEscape) PlayerEncounter.EnemySurrender = true;
                PlayerEncounter.Update();
            }
            else if (huntState == HuntState.AfterBattle && Game.Current.GameStateManager.ActiveState is MapState mapstate2 && !mapstate2.IsMenuState) //warning why mapstate?
            {
                if (PlayerEncounter.Current == null)
                {
                    LoadingWindow.DisableGlobalLoadingWindow();
                    huntState = HuntState.Normal;
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