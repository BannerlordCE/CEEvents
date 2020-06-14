using CaptivityEvents.CampaignBehaviours;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using System;
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
            base.NotificationIdentifier = CESettings.Instance.EventCaptorCustomTextureNotifications ? "ceevent" : "vote";
            _randomEvent = randomEvent;
            _onInspect = delegate ()
            {
                OnRandomNotificationInspect();
            };
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();
            if (PlayerCaptivity.IsCaptive || !CECampaignBehavior.extraVariables.notificationEventExists)
            {
                CECampaignBehavior.extraVariables.notificationEventExists = false;
                base.ExecuteRemove();
            }
            else if (CECampaignBehavior.extraVariables.notificationEventCheck)
            {
                if (CEEventChecker.FlagsDoMatchEventConditions(_randomEvent, CharacterObject.PlayerCharacter) != null)
                {
                    CECampaignBehavior.extraVariables.notificationEventCheck = false;
                    CECampaignBehavior.extraVariables.notificationEventExists = false;
                    base.ExecuteRemove();
                }
                else
                {
                    CECampaignBehavior.extraVariables.notificationEventCheck = false;
                }
            }
        }

        private void OnRandomNotificationInspect()
        {
            CECampaignBehavior.extraVariables.notificationEventExists = false;
            base.ExecuteRemove();
            string result = CEEventChecker.FlagsDoMatchEventConditions(_randomEvent, CharacterObject.PlayerCharacter);
            if (result == null)
            {
                if (Game.Current.GameStateManager.ActiveState is MapState mapState)
                {
                    Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;
                    if (!mapState.AtMenu)
                    {
                        GameMenu.ActivateGameMenu("prisoner_wait");
                    }
                    else
                    {
                        CECampaignBehavior.extraVariables.menuToSwitchBackTo = mapState.GameMenuId;
                        CECampaignBehavior.extraVariables.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                    }

                    GameMenu.SwitchToMenu(_randomEvent.Name);
                }
            }
            else
            {
                TextObject textObject = new TextObject("{=CEEVENTS1058}Event conditions are no longer met.", null);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
            }
        }
    }
}