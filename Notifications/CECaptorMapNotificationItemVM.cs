#define V120

using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    // ArmyDispersionItemVM
    internal class CECaptorMapNotificationItemVM : MapNotificationItemBaseVM
    {
        private readonly CEEvent _captorEvent;

        public CECaptorMapNotificationItemVM(InformationData data) : base(data)
        {
            NotificationIdentifier = (CESettings.Instance?.EventCaptorCustomTextureNotifications ?? true) ? "cecaptor" : "death";
            _captorEvent = ((CECaptorMapNotification)data).CaptorEvent;
            _onInspect = OnCaptorNotificationInspect;
        }

        public override void ManualRefreshRelevantStatus()
        {
            base.ManualRefreshRelevantStatus();

            if (MobileParty.MainParty.Party.PrisonRoster.Count == 0 || PlayerCaptivity.IsCaptive || !CEHelper.NotificationCaptorExists || !(CESettings.Instance?.EventCaptorNotifications ?? true))
            {
                CEHelper.NotificationCaptorExists = false;
                ExecuteRemove();
            }
            else if (CEHelper.NotificationCaptorExists)
            {
                if (!MobileParty.MainParty.Party.PrisonRoster.Contains(_captorEvent.Captive) || new CEEventChecker(_captorEvent).FlagsDoMatchEventConditions(_captorEvent.Captive, PartyBase.MainParty) != null)
                {
                    CEHelper.NotificationCaptorCheck = false;
                    CEHelper.NotificationCaptorExists = false;
                    ExecuteRemove();
                }
                else
                {
                    CEHelper.NotificationCaptorCheck = false;
                }
            }
        }

        private void OnCaptorNotificationInspect()
        {
            CEHelper.NotificationCaptorExists = false;
            ExecuteRemove();

            if (MobileParty.MainParty.Party.PrisonRoster.Count > 0 && MobileParty.MainParty.Party.PrisonRoster.Contains(_captorEvent.Captive))
            {
                // Declare Variables
                string returnString = new CEEventChecker(_captorEvent).FlagsDoMatchEventConditions(_captorEvent.Captive, PartyBase.MainParty);

                if (returnString == null)
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

                        CEHelper.SafeActivateGameMenu(_captorEvent.Name);
                    }
                    else
                    {
                        if (CECampaignBehavior.ExtraProps != null)
                        {
                            CECampaignBehavior.ExtraProps.MenuToSwitchBackTo = mapState.GameMenuId;
                            CECampaignBehavior.ExtraProps.CurrentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                        }

                        CEHelper.SafeSwitchToMenu(_captorEvent.Name);
                    }
                }
                else
                {
                    TextObject textObject = new("{=CEEVENTS1058}Event conditions are no longer met.");
                    InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Gray));
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