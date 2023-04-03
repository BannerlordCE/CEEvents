#define V112

using CaptivityEvents.Config;
using CaptivityEvents.Helper;
using CaptivityEvents.Notifications;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(MapNotificationVM), "GetNotificationFromData")]
    internal class CEMapNotificationVM
    {
        public static readonly MethodInfo RemoveNotificationItem = AccessTools.Method(typeof(MapNotificationVM), "RemoveNotificationItem");

        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.Instance?.EventCaptorNotifications ?? true;

        [HarmonyPostfix]
        private static void GetNotificationFromData(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result, InformationData data)
        {
            Type type = data.GetType();
            MapNotificationItemBaseVM mapNotification = null;
            if (type == typeof(CECaptorMapNotification))
            {
                Action<MapNotificationItemBaseVM> onRemove = (MapNotificationItemBaseVM item) =>
                {
                    CEHelper.notificationCaptorExists = false;
                    new CESubModule().LoadCampaignNotificationTexture("default");
                    RemoveNotificationItem.Invoke(__instance, new object[] { item });
                };

                mapNotification = new CECaptorMapNotificationItemVM(data);

                FieldInfo fi = mapNotification.GetType().BaseType.GetField("OnRemove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
                if (fi != null) fi.SetValue(mapNotification, onRemove);

                __result = mapNotification;
            }
            else if (type == typeof(CEEventMapNotification))
            {
                Action<MapNotificationItemBaseVM> onRemove = (MapNotificationItemBaseVM item) =>
                {
                    CEHelper.notificationEventExists = false;
                    new CESubModule().LoadCampaignNotificationTexture("default", 1);
                    RemoveNotificationItem.Invoke(__instance, new object[] { item });
                };

                mapNotification = new CEEventMapNotificationItemVM(data);

                FieldInfo fi = mapNotification.GetType().BaseType.GetField("OnRemove", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
                if (fi != null) fi.SetValue(mapNotification, onRemove);

                __result = mapNotification;
            }
        }
    }
}