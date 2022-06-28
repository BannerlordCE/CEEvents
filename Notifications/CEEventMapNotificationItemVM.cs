#define V180

using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;

#if V172
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
#else
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
#endif

namespace CaptivityEvents.Notifications
{
    // ArmyDispersionItemVM
    internal class CEEventMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private readonly CEEvent _randomEvent;

        public CEEventMapNotificationItemVM(InformationData data) : base(data)
        {
            NotificationIdentifier = (CESettings.Instance?.EventCaptorCustomTextureNotifications ?? true)
                ? "ceevent"
                : "vote";
            _randomEvent = ((CEEventMapNotification)data).RandomEvent;
            _onInspect = OnRandomNotificationInspect;
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();

            if (PlayerCaptivity.IsCaptive || !CEHelper.notificationEventExists || !(CESettings.Instance?.EventCaptorNotifications ?? true))
            {
                CEHelper.notificationEventExists = false;
                ExecuteRemove();
            }
            else if (CECampaignBehavior.ExtraProps != null && CEHelper.notificationEventCheck)
            {
                if (new CEEventChecker(_randomEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter) != null)
                {
                    CEHelper.notificationEventCheck = false;
                    CEHelper.notificationEventExists = false;
                    ExecuteRemove();
                }
                else
                {
                    CEHelper.notificationEventCheck = false;
                }
            }
        }

        private void OnRandomNotificationInspect()
        {
            CEHelper.notificationEventExists = false;
            ExecuteRemove();
            string result = new CEEventChecker(_randomEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);

            if (result == null)
            {
                if (Game.Current.GameStateManager.ActiveState is not MapState mapState)
                {
                    TextObject textObject = new("{=CEEVENTS1058}Event conditions are no longer met.");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
                    return;
                }

                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                if (_randomEvent.SceneToPlay != null)
                {
                    CESceneNotification data = new(null, Hero.MainHero, _randomEvent.SceneToPlay);
                    MBInformationManager.ShowSceneNotification(data);
                }

                if (!mapState.AtMenu)
                {
                    if (CECampaignBehavior.ExtraProps != null)
                    {
                        CECampaignBehavior.ExtraProps.menuToSwitchBackTo = null;
                        CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = null;
                    }
                    GameMenu.ActivateGameMenu(_randomEvent.Name);
                }
                else
                {
                    if (CECampaignBehavior.ExtraProps != null)
                    {
                        CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                        CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                    }
                    GameMenu.SwitchToMenu(_randomEvent.Name);
                }
            }
            else
            {
                TextObject textObject = new("{=CEEVENTS1058}Event conditions are no longer met.");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
            }
        }
    }
}