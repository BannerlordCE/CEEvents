using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    // ArmyDispersionItemVM
    internal class CEEventMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private readonly CEEvent _randomEvent;

        // 1.5.5
        // public CEEventMapNotificationItemVM(InformationData data, Action onInspect, Action<MapNotificationItemBaseVM> onRemove) : base(data, onInspect, onRemove)

        // 1.5.6
        public CEEventMapNotificationItemVM(InformationData data) : base(data)
        {
            NotificationIdentifier = CESettings.Instance != null && CESettings.Instance.EventCaptorCustomTextureNotifications
                ? "ceevent"
                : "vote";
            _randomEvent = ((CEEventMapNotification)data).RandomEvent;
            _onInspect = OnRandomNotificationInspect;
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();

            if (PlayerCaptivity.IsCaptive || !CEHelper.notificationEventExists || !CESettings.Instance.EventCaptorNotifications)
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
                if (!(Game.Current.GameStateManager.ActiveState is MapState mapState)) return;
                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                if (!mapState.AtMenu)
                {
                    GameMenu.ActivateGameMenu("prisoner_wait");
                }
                else
                {
                    if (CECampaignBehavior.ExtraProps != null)
                    {
                        CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                        CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                    }
                }

                GameMenu.SwitchToMenu(_randomEvent.Name);
            }
            else
            {
                TextObject textObject = new TextObject("{=CEEVENTS1058}Event conditions are no longer met.");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
            }
        }
    }
}