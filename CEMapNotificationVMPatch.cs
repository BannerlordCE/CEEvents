using System;
using System.Reflection;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents
{
    [HarmonyPatch(typeof(MapNotificationVM), "DetermineNotificationType")]
    internal class CEMapNotificationVMPatch
    {
        public static MethodInfo RemoveNotificationItem = AccessTools.Method(typeof(MapNotificationVM), "RemoveNotificationItem");

        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            CECustomHandler.ForceLogToFile("EventCaptorNotifications: " + (CESettings.Instance != null && CESettings.Instance.EventCaptorNotifications) + ".");

            return CESettings.Instance != null && CESettings.Instance.EventCaptorNotifications;
        }

        [HarmonyPostfix]
        private static void DetermineNotificationType(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result)
        {
            if (__result.Data.TitleText.Equals(new TextObject("Captor Event")))
                __result = new NewTestNotificationItemVM(__result.Data, null, item =>
                {
                    RemoveNotificationItem.Invoke(__instance, new object[] { item });
                });
        }

        public class NewTestNotificationItemVM : MapNotificationItemBaseVM
        {
            public NewTestNotificationItemVM(InformationData data, Action onInspect, Action<MapNotificationItemBaseVM> onRemove) : base(data, onInspect, onRemove)
            {
                NotificationIdentifier = "death";

                _onInspect = OnNewTestNotificationInspect;
            }

            public override void ManualRefreshRelevantStatus()
            {
                base.ManualRefreshRelevantStatus();

                if (MobileParty.MainParty.Party.PrisonRoster.Count != 0) return;
                CEPersistence.NotificationExists = false;
                new CESubModule().LoadCampaignNotificationTexture("default");
                ExecuteRemove();
            }

            private void OnNewTestNotificationInspect()
            {
                CEPersistence.NotificationExists = false;
                new CESubModule().LoadCampaignNotificationTexture("default");
                ExecuteRemove();

                if (MobileParty.MainParty.Party.PrisonRoster.Count <= 0) return;
                // Declare Variables
                CharacterObject captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
                CEEvent returnedEvent = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(captive);

                if (returnedEvent == null) return;

                if (!(Game.Current.GameStateManager.ActiveState is MapState mapState)) return;
                Campaign.Current.LastTimeControlMode = Campaign.Current.TimeControlMode;

                if (!mapState.AtMenu)
                {
                    GameMenu.ActivateGameMenu("prisoner_wait");
                }
                else
                {
                    CECampaignBehavior.ExtraProps.menuToSwitchBackTo = mapState.GameMenuId;
                    CECampaignBehavior.ExtraProps.currentBackgroundMeshNameToSwitchBackTo = mapState.MenuContext.CurrentBackgroundMeshName;
                }

                GameMenu.SwitchToMenu(returnedEvent.Name);
            }
        }
    }
}