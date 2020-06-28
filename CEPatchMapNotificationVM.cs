using System.Reflection;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;

namespace CaptivityEvents
{
    [HarmonyPatch(typeof(MapNotificationVM), "DetermineNotificationType")]
    internal class CEMapNotificationVM
    {
        public static readonly MethodInfo RemoveNotificationItem = AccessTools.Method(typeof(MapNotificationVM), "RemoveNotificationItem");

        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            return CESettings.Instance != null && CESettings.Instance.EventCaptorNotifications;
        }

        [HarmonyPostfix]
        private static void DetermineNotificationType(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result, InformationData data)
        {
            var type = data.GetType();
            var t = new CESubModule();

            if (type == typeof(CECaptorMapNotification))
            {
                if (data is CECaptorMapNotification captorMapNotification)
                    __result = new CECaptorMapNotificationItemVM(captorMapNotification.CaptorEvent, data, null, item =>
                                                                                                                {
                                                                                                                    CEHelper.notificationCaptorExists = false;
                                                                                                                    t.LoadCampaignNotificationTexture("default");

                                                                                                                    var parameters = new object[1];
                                                                                                                    parameters[0] = item;
                                                                                                                    RemoveNotificationItem.Invoke(__instance, parameters);
                                                                                                                });
            }
            else if (type == typeof(CEEventMapNotification))
            {
                if (data is CEEventMapNotification eventMapNotification)
                    __result = new CEEventMapNotificationItemVM(eventMapNotification.RandomEvent, data, null, item =>
                                                                                                              {
                                                                                                                  CEHelper.notificationEventExists = false;
                                                                                                                  t.LoadCampaignNotificationTexture("default", 1);

                                                                                                                  var parameters = new object[1];
                                                                                                                  parameters[0] = item;
                                                                                                                  RemoveNotificationItem.Invoke(__instance, parameters);
                                                                                                              });
            }
        }
    }
}