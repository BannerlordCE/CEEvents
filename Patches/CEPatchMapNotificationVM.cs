using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(MapNotificationVM), "DetermineNotificationType")]
    internal class CEMapNotificationVM
    {
        public static readonly MethodInfo RemoveNotificationItem = AccessTools.Method(typeof(MapNotificationVM), "RemoveNotificationItem");

        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.InstanceToCheck != null && CESettings.InstanceToCheck.EventCaptorNotifications;

        [HarmonyPostfix]
        private static void DetermineNotificationType(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result, InformationData data)
        {
            Type type = data.GetType();

            if (type == typeof(CECaptorMapNotification))
            {
                if (data is CECaptorMapNotification captorMapNotification)
                {
                    __result = new CECaptorMapNotificationItemVM(captorMapNotification.CaptorEvent, data, null, item =>
                    {
                        CEHelper.notificationCaptorExists = false;
                        new CESubModule().LoadCampaignNotificationTexture("default");
                        RemoveNotificationItem.Invoke(__instance, new object[] { item });
                    });
                }
            }
            else if (type == typeof(CEEventMapNotification))
            {
                if (data is CEEventMapNotification eventMapNotification)
                {
                    __result = new CEEventMapNotificationItemVM(eventMapNotification.RandomEvent, data, null, item =>
                    {
                        CEHelper.notificationEventExists = false;
                        new CESubModule().LoadCampaignNotificationTexture("default", 1);
                        RemoveNotificationItem.Invoke(__instance, new object[] { item });
                    });
                }
            }
        }
    }
}