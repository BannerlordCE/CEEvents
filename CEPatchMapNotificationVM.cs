using System.Reflection;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(MapNotificationVM), "DetermineNotificationType")]
    internal class CEMapNotificationVMPatch
    {
        private static readonly MethodInfo RemoveNotificationItem = AccessTools.Method(typeof(MapNotificationVM), "RemoveNotificationItem");

        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            return CESettings.Instance != null && CESettings.Instance.EventCaptorNotifications;
        }

        [HarmonyPostfix]
        private static void DetermineNotificationType(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result, InformationData data)
        {
            var type = data.GetType();

            if (type != typeof(CECaptorMapNotification)) return;

            switch (data)
            {
                case CECaptorMapNotification captorMapNotification:
                    __result = new CECaptorMapNotificationItemVM(captorMapNotification.CaptorEvent, data, null, item =>
                                                                                                                {
                                                                                                                    CEContext.notificationCaptorExists = false;
                                                                                                                    CESubModule.LoadCampaignNotificationTexture("default");

                                                                                                                    var parameters = new object[1];
                                                                                                                    parameters[0] = item;
                                                                                                                    RemoveNotificationItem.Invoke(__instance, parameters);
                                                                                                                });

                    break;

                case CEEventMapNotification eventMapNotification:
                    __result = new CEEventMapNotificationItemVM(eventMapNotification.RandomEvent, data, null, item =>
                                                                                                              {
                                                                                                                  CEContext.notificationEventExists = false;
                                                                                                                  CESubModule.LoadCampaignNotificationTexture("default", 1);

                                                                                                                  var parameters = new object[1];
                                                                                                                  parameters[0] = item;
                                                                                                                  RemoveNotificationItem.Invoke(__instance, parameters);
                                                                                                              });

                    break;
            }
        }
    }
}