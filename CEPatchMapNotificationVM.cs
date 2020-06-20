using System;
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
        public static MethodInfo RemoveNotificationItem = AccessTools.Method(typeof(MapNotificationVM), "RemoveNotificationItem");

        [HarmonyPrepare]
        private static bool ShouldPatch()
        {
            return CESettings.Instance.EventCaptorNotifications;
        }

        [HarmonyPostfix]
        private static void DetermineNotificationType(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result, InformationData data)
        {
            Type type = data.GetType();
            if (type.Equals(typeof(CECaptorMapNotification)))
            {
                CECaptorMapNotification captorMapNotification = data as CECaptorMapNotification;
                __result = new CECaptorMapNotificationItemVM(captorMapNotification.CaptorEvent, data, null, new Action<MapNotificationItemBaseVM>((MapNotificationItemBaseVM item) =>
                {
                    CEHelper.notificationCaptorExists = false;
                    CESubModule.LoadCampaignNotificationTexture("default");

                    object[] parameters = new object[1];
                    parameters[0] = item;
                    RemoveNotificationItem.Invoke(__instance, parameters);
                }));
            }
            else if (type.Equals(typeof(CEEventMapNotification)))
            {
                CEEventMapNotification eventMapNotification = data as CEEventMapNotification;
                __result = new CEEventMapNotificationItemVM(eventMapNotification.RandomEvent, data, null, new Action<MapNotificationItemBaseVM>((MapNotificationItemBaseVM item) =>
                {
                    CEHelper.notificationEventExists = false;
                    CESubModule.LoadCampaignNotificationTexture("default", 1);

                    object[] parameters = new object[1];
                    parameters[0] = item;
                    RemoveNotificationItem.Invoke(__instance, parameters);
                }));
            }
        }
    }
}