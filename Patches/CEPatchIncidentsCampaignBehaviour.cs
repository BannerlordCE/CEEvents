using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Incidents;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(IncidentsCampaignBehaviour), "OnIncidentResolved")]
    internal static class CEIncidentsCampaignBehaviourPatch
    {
        private static readonly AccessTools.FieldRef<IncidentsCampaignBehaviour, Dictionary<Incident, CampaignTime>> _incidentsOnCooldown =
            AccessTools.FieldRefAccess<IncidentsCampaignBehaviour, Dictionary<Incident, CampaignTime>>("_incidentsOnCooldown");

        private static readonly AccessTools.FieldRef<IncidentsCampaignBehaviour, CampaignTime> _lastGlobalIncidentCooldown =
            AccessTools.FieldRefAccess<IncidentsCampaignBehaviour, CampaignTime>("_lastGlobalIncidentCooldown");

        private delegate CampaignTime GetCooldownTimeDelegate(IncidentsCampaignBehaviour instance);
        private static readonly GetCooldownTimeDelegate GetCooldownTime =
            AccessTools.MethodDelegate<GetCooldownTimeDelegate>(AccessTools.Method(typeof(IncidentsCampaignBehaviour), "GetCooldownTime"));

        public static bool Prefix(Incident incident, IncidentsCampaignBehaviour __instance)
        {
            var incidentsOnCooldown = _incidentsOnCooldown(__instance);

            if (incidentsOnCooldown.ContainsKey(incident))
            {
                incidentsOnCooldown[incident] = CampaignTime.Now;
            }
            else
            {
                incidentsOnCooldown.Add(incident, CampaignTime.Now);
            }

            _lastGlobalIncidentCooldown(__instance) = CampaignTime.Now + GetCooldownTime(__instance);

            // Return false to prevent the original method from running
            return false;
        }
    }
}