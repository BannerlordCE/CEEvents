using System;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    internal class CEEventMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private readonly CEEvent _randomEvent;

        public CEEventMapNotificationItemVM(CEEvent randomEvent, InformationData data, Action onInspect, Action<MapNotificationItemBaseVM> onRemove) : base(data, onInspect, onRemove)
        {
            NotificationIdentifier = CESettings.Instance != null && CESettings.Instance.EventCaptorCustomTextureNotifications
                ? "ceevent"
                : "vote";
            _randomEvent = randomEvent;

            _onInspect = OnRandomNotificationInspect;
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();

            if (PlayerCaptivity.IsCaptive || !CEHelper.notificationEventExists)
            {
                CEHelper.notificationEventExists = false;
                ExecuteRemove();
            }
            else if (CECampaignBehavior.ExtraProps != null && CEHelper.notificationEventCheck)
            {
                if (CEEventChecker.FlagsDoMatchEventConditions(_randomEvent, CharacterObject.PlayerCharacter) != null)
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
            var result = CEEventChecker.FlagsDoMatchEventConditions(_randomEvent, CharacterObject.PlayerCharacter);

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
                var textObject = new TextObject("{=CEEVENTS1058}Event conditions are no longer met.");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
            }
        }
    }
}