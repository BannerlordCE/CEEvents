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
    internal class CECaptorMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private CEEvent _captorEvent;

        public CECaptorMapNotificationItemVM(CEEvent captorEvent, InformationData data, Action onInspect, Action<MapNotificationItemBaseVM> onRemove) : base(data, onInspect, onRemove)
        {
            base.NotificationIdentifier = CESettings.Instance.EventCaptorCustomTextureNotifications ? "cecaptor" : "death";
            _captorEvent = captorEvent;
            _onInspect = delegate ()
            {
                OnCaptorNotificationInspect();
            };
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();
            if (MobileParty.MainParty.Party.PrisonRoster.Count == 0 || PlayerCaptivity.IsCaptive || !CECampaignBehavior.extraVariables.notificationCaptorExists)
            {
                CECampaignBehavior.extraVariables.notificationCaptorExists = false;
                base.ExecuteRemove();
            }
            else if (CECampaignBehavior.extraVariables.notificationCaptorExists)
            {
                if (!MobileParty.MainParty.Party.PrisonRoster.Contains(_captorEvent.Captive) || CEEventChecker.FlagsDoMatchEventConditions(_captorEvent, _captorEvent.Captive, PartyBase.MainParty) != null)
                {
                    CECampaignBehavior.extraVariables.notificationCaptorCheck = false;
                    CECampaignBehavior.extraVariables.notificationCaptorExists = false;
                    base.ExecuteRemove();
                }
                else
                {
                    CECampaignBehavior.extraVariables.notificationCaptorCheck = false;
                }
            }
        }

        private void OnCaptorNotificationInspect()
        {
            CECampaignBehavior.extraVariables.notificationCaptorExists = false;
            base.ExecuteRemove();
            if (MobileParty.MainParty.Party.PrisonRoster.Count > 0 && MobileParty.MainParty.Party.PrisonRoster.Contains(_captorEvent.Captive))
            {
                // Declare Variables
                string returnString = CEEventChecker.FlagsDoMatchEventConditions(_captorEvent, _captorEvent.Captive, PartyBase.MainParty);
                if (returnString == null)
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

                        GameMenu.SwitchToMenu(_captorEvent.Name);
                    }
                }
                else
                {
                    TextObject textObject = new TextObject("{=CEEVENTS1058}Event conditions are no longer met.", null);
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
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