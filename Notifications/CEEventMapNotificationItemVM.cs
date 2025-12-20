#define V120

using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    // ArmyDispersionItemVM
    internal class CEEventMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private readonly CEEvent _randomEvent;

        public CEEventMapNotificationItemVM(InformationData data) : base(data)
        {
            NotificationIdentifier = (CESettings.Instance?.EventCaptorCustomTextureNotifications ?? true) ? "ceevent" : "vote";
            _randomEvent = ((CEEventMapNotification)data).RandomEvent;
            _onInspect = OnRandomNotificationInspect;
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();

            if (PlayerCaptivity.IsCaptive || !CEHelper.NotificationEventExists || !(CESettings.Instance?.EventCaptorNotifications ?? true))
            {
                CEHelper.NotificationEventExists = false;
                ExecuteRemove();
            }
            else if (CECampaignBehavior.ExtraProps != null && CEHelper.NotificationEventCheck)
            {
                if (new CEEventChecker(_randomEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter) != null)
                {
                    CEHelper.NotificationEventCheck = false;
                    CEHelper.NotificationEventExists = false;
                    ExecuteRemove();
                }
                else
                {
                    CEHelper.NotificationEventCheck = false;
                }
            }
        }

        private void OnRandomNotificationInspect()
        {
            CEHelper.NotificationEventExists = false;
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

                if (!mapState.AtMenu)
                {
                    if (CECampaignBehavior.ExtraProps != null)
                    {
                        CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = null;
                        CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = null;
                    }

                    CEHelper.SafeActivateGameMenu(_randomEvent.Name);
                }
                else
                {
                    if (CECampaignBehavior.ExtraProps != null)
                    {
                        CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = mapState.GameMenuId;
                        CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                    }

                    CEHelper.SafeSwitchToMenu(_randomEvent.Name);
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