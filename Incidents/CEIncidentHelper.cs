using System;
using System.Linq;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Incidents
{
    public static class CEIncidentHelper
    {
        public static bool ShouldLaunchAsIncident(string eventName)
        {
            if (string.IsNullOrEmpty(eventName)) return false;

            // Find the event in the CE events list
            CEEvent ceEvent = CEPersistence.CEEvents.Find(e => e.Name == eventName);
            if (ceEvent == null) return false;

            // Check if EventType is "incident"
            return ceEvent.EventType?.ToLower() == "incident";
        }

        public static void LaunchBattleResultIncident(string eventName)
        {
            try
            {
                // Launch incident using the pattern "ce_incident_{eventName}"
                string incidentName = $"ce_incident_{eventName}";

                // Find the incident by StringId (similar to CEConsole.TriggerIncident)
                Incident incident = MBObjectManager.Instance.GetObjectTypeList<Incident>()
                                                   .FirstOrDefault(i => i.StringId.Equals(incidentName, StringComparison.OrdinalIgnoreCase));

                if (incident != null)
                {
                    // Launch the incident by setting it as the next incident (same as CEConsole.TriggerIncident)
                    MapState mapState = GameStateManager.Current.LastOrDefault<MapState>();
                    mapState?.NextIncident = incident;
                    CECustomHandler.ForceLogToFile($"Launched battle result incident: {incidentName}");
                }
                else
                {
                    CECustomHandler.ForceLogToFile($"Battle result incident not found: {incidentName}");
                }
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile($"Failed to launch battle result incident '{eventName}': {ex.Message}");
            }
        }
    }
}
