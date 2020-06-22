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
    internal class CECaptorMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private readonly CEEvent _captorEvent;

        public CECaptorMapNotificationItemVM(CEEvent captorEvent, InformationData data, Action onInspect, Action<MapNotificationItemBaseVM> onRemove) : base(data, onInspect, onRemove)
        {
            NotificationIdentifier = CESettings.Instance != null && CESettings.Instance.EventCaptorCustomTextureNotifications
                ? "cecaptor"
                : "death";
            _captorEvent = captorEvent;

            _onInspect = delegate
                         {
                             OnCaptorNotificationInspect();
                         };
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();

            if (MobileParty.MainParty.Party.PrisonRoster.Count == 0 || PlayerCaptivity.IsCaptive || !CEHelper.notificationCaptorExists)
            {
                CEHelper.notificationCaptorExists = false;
                ExecuteRemove();
            }
            else if (CEHelper.notificationCaptorExists)
            {
                if (!MobileParty.MainParty.Party.PrisonRoster.Contains(_captorEvent.Captive) || CEEventChecker.FlagsDoMatchEventConditions(_captorEvent, _captorEvent.Captive, PartyBase.MainParty) != null)
                {
                    CEHelper.notificationCaptorCheck = false;
                    CEHelper.notificationCaptorExists = false;
                    ExecuteRemove();
                }
                else
                {
                    CEHelper.notificationCaptorCheck = false;
                }
            }
        }

        private void OnCaptorNotificationInspect()
        {
            CEHelper.notificationCaptorExists = false;
            ExecuteRemove();

            if (MobileParty.MainParty.Party.PrisonRoster.Count > 0 && MobileParty.MainParty.Party.PrisonRoster.Contains(_captorEvent.Captive))
            {
                // Declare Variables
                var returnString = CEEventChecker.FlagsDoMatchEventConditions(_captorEvent, _captorEvent.Captive, PartyBase.MainParty);

                if (returnString == null)
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

                    GameMenu.SwitchToMenu(_captorEvent.Name);
                }
                else
                {
                    var textObject = new TextObject("{=CEEVENTS1058}Event conditions are no longer met.");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
                }
            }
            else
            {
                var textObject = new TextObject("{=CEEVENTS1058}Event conditions are no longer met.");
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
            }
        }
    }
}