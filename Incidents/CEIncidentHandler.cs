using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.IncidentsCampaignBehaviour;

namespace CaptivityEvents.Incidents
{
    public class CEIncidentHandler : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
            CampaignEvents.GameMenuOptionSelectedEvent.AddNonSerializedListener(this, OnGameMenuOptionSelected);
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, OnGameMenuOpened);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, ConversationEnded);
            CampaignEvents.OnHeirSelectionOverEvent.AddNonSerializedListener(this, OnHeirSelectionOver);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.OnIncidentResolvedEvent.AddNonSerializedListener(this, OnIncidentResolved);
        }

        private void OnIncidentResolved(Incident incident)
        {
            // Update or add the incident to cooldown list
            if (_incidentsOnCooldown.ContainsKey(incident))
            {
                _incidentsOnCooldown[incident] = CampaignTime.Now;
            }
            else
            {
                _incidentsOnCooldown.Add(incident, CampaignTime.Now);
            }
            _lastGlobalIncidentCooldown = CampaignTime.Now + GetCooldownTime();
        }

        private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (mobileParty == MobileParty.MainParty && Campaign.Current.CurrentMenuContext == null)
            {
                _canInvokeSettlementEvent = true;
            }
        }

        private void OnSettlementLeft(MobileParty mobileParty, Settlement settlement)
        {
            if (mobileParty == MobileParty.MainParty)
            {
                _canInvokeSettlementEvent = false;
            }
        }

        private void OnNewGameCreated(CampaignGameStarter obj)
        {
            _lastGlobalIncidentCooldown = CampaignTime.Now + GetCooldownTime();
        }

        void OnBeforeNonReadyObjectsDeleted()
        {
            InitializeIncidents();
        }

        private void ConversationEnded(IEnumerable<CharacterObject> conversationCharacters)
        {
            if (PlayerEncounter.Current != null && PlayerEncounter.LeaveEncounter && MobileParty.MainParty.CurrentSettlement == null && MobileParty.MainParty.RandomFloatWithSeed((uint)CampaignTime.Now.ToSeconds) < Campaign.Current.Models.IncidentModel.GetIncidentTriggerGlobalProbability())
            {
                TryInvokeIncident(IncidentTrigger.LeavingEncounter);
            }
        }

        private void OnMapEventEnded(MapEvent evt)
        {
            if (!evt.IsPlayerMapEvent || evt.IsNavalMapEvent)
            {
                return;
            }

            if (!evt.HasWinner || evt.DefeatedSide == evt.PlayerSide)
            {
                return;
            }

            if ((evt.IsFieldBattle || evt.IsHideoutBattle) && MobileParty.MainParty.RandomFloatWithSeed((uint)CampaignTime.Now.ToSeconds) < Campaign.Current.Models.IncidentModel.GetIncidentTriggerGlobalProbability())
            {
                TryInvokeIncident(IncidentTrigger.LeavingBattle);
            }
        }

        private void OnGameMenuOpened(MenuCallbackArgs args)
        {
            if (!_canInvokeSettlementEvent)
            {
                return;
            }

            if (!(MobileParty.MainParty.RandomFloatWithSeed((uint)CampaignTime.Now.ToSeconds) < Campaign.Current.Models.IncidentModel.GetIncidentTriggerGlobalProbability())) return;

            switch (args.MenuContext.GameMenu.StringId)
            {
                case "town":
                    TryInvokeIncident(IncidentTrigger.EnteringTown);
                    _canInvokeSettlementEvent = false;

                    break;
                case "village":
                    TryInvokeIncident(IncidentTrigger.EnteringVillage);
                    _canInvokeSettlementEvent = false;

                    break;
                case "castle":
                    TryInvokeIncident(IncidentTrigger.EnteringCastle);
                    _canInvokeSettlementEvent = false;

                    break;
            }
        }

        private void OnGameMenuOptionSelected(GameMenu gameMenu, GameMenuOption option)
        {
            if (!(MobileParty.MainParty.RandomFloatWithSeed((uint)CampaignTime.Now.ToSeconds) < Campaign.Current.Models.IncidentModel.GetIncidentTriggerGlobalProbability())) return;

            switch (gameMenu.StringId)
            {
                case "town" when option.IdString == "town_leave":
                    TryInvokeIncident(IncidentTrigger.LeavingTown | IncidentTrigger.LeavingSettlement);

                    break;
                case "castle" when option.IdString == "leave":
                    TryInvokeIncident(IncidentTrigger.LeavingCastle | IncidentTrigger.LeavingSettlement);

                    break;
                case "village" when option.IdString == "leave":
                    TryInvokeIncident(IncidentTrigger.LeavingVillage | IncidentTrigger.LeavingSettlement);

                    break;
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_incidentsOnCooldown", ref _incidentsOnCooldown);
            dataStore.SyncData("_lastGlobalIncidentCooldown", ref _lastGlobalIncidentCooldown);
            dataStore.SyncData("_activeIncidentSeed", ref _activeIncidentSeed);
        }

        private void OnHeirSelectionOver(Hero obj)
        {
            _lastGlobalIncidentCooldown = CampaignTime.Now + CampaignTime.Hours(1f);
            _incidentsOnCooldown.Clear();
        }

        private void OnHourlyTick()
        {
            float num = MobileParty.MainParty.RandomFloatWithSeed((uint)CampaignTime.Now.ToSeconds);

            if (MobileParty.MainParty.SiegeEvent != null && num < Campaign.Current.Models.IncidentModel.GetIncidentTriggerProbabilityDuringSiege())
            {
                TryInvokeIncident(IncidentTrigger.DuringSiege);

                return;
            }

            if (Campaign.Current.CurrentMenuContext != null && (Campaign.Current.CurrentMenuContext.GameMenu.StringId == "town_wait_menus" || Campaign.Current.CurrentMenuContext.GameMenu.StringId == "village_wait_menus") && num < Campaign.Current.Models.IncidentModel.GetIncidentTriggerProbabilityDuringWait())
            {
                TryInvokeIncident(IncidentTrigger.WaitingInSettlement);
            }
        }

        private void TryInvokeIncident(IncidentTrigger trigger)
        {
            if (!CESettings.Instance.IncidentsEnabled)
            {
                return;
            }

            if (((trigger & IncidentTrigger.EnteringCastle) != 0 || (trigger & IncidentTrigger.EnteringTown) != 0 || (trigger & IncidentTrigger.EnteringVillage) != 0) && (MobileParty.MainParty.CurrentSettlement == null || MobileParty.MainParty.CurrentSettlement.IsSettlementBusy(this)))
            {
                return;
            }

            if (((trigger & IncidentTrigger.LeavingTown) != 0 || (trigger & IncidentTrigger.LeavingVillage) != 0 || (trigger & IncidentTrigger.LeavingSettlement) != 0) && MobileParty.MainParty.LastVisitedSettlement.IsSettlementBusy(this))
            {
                return;
            }

            if (Hero.MainHero.IsPrisoner || Campaign.Current.ConversationManager.IsConversationFlowActive)
            {
                return;
            }

            if (!_lastGlobalIncidentCooldown.IsPast) return;

            CheckIncidentsOnCooldown();
            _activeIncidentSeed = (long)CampaignTime.Now.ToSeconds;
            IReadOnlyList<Incident> occurableEventsForTrigger = GetOccurableEventsForTrigger(trigger);

            if (occurableEventsForTrigger.Count <= 0) return;

            Incident randomElement = occurableEventsForTrigger.GetRandomElement();
            InvokeIncident(randomElement);
        }

        private CampaignTime GetCooldownTime()
        {
            CampaignTime minGlobalCooldownTime = Campaign.Current.Models.IncidentModel.GetMinGlobalCooldownTime();
            CampaignTime maxGlobalCooldownTime = Campaign.Current.Models.IncidentModel.GetMaxGlobalCooldownTime();

            return CampaignTime.Hours(MBRandom.RandomFloatRanged((float)minGlobalCooldownTime.ToHours, (float)maxGlobalCooldownTime.ToHours));
        }

        private void CheckIncidentsOnCooldown()
        {
            List<Incident> list = (from keyValuePair in _incidentsOnCooldown
                                   where keyValuePair.Value + keyValuePair.Key.Cooldown <= CampaignTime.Now
                                   select keyValuePair.Key).ToList();

            foreach (Incident key in list)
            {
                _incidentsOnCooldown.Remove(key);
            }
        }

        private IReadOnlyList<Incident> GetOccurableEventsForTrigger(IncidentTrigger trigger)
        {
            return MBObjectManager.Instance.GetObjectTypeList<Incident>().Where(incident => !_incidentsOnCooldown.ContainsKey(incident) && (incident.Trigger & trigger) != 0 && incident.CanIncidentBeInvoked()).ToList();
        }

        private void InvokeIncident(Incident incident)
        {
            MapState mapState = GameStateManager.Current.LastOrDefault<MapState>();
            mapState?.NextIncident = incident;
        }

        private Incident RegisterIncident(string id, string title, string description, IncidentTrigger trigger, IncidentType type, CampaignTime cooldown, Func<TextObject, bool> condition)
        {
            Incident incident = Game.Current.ObjectManager.RegisterPresumedObject(new Incident(id));
            incident.Initialize(title, description, trigger, type, cooldown, condition);

            return incident;
        }

        public void InitializeIncidents()
        {
            CECustomHandler.ForceLogToFile($"InitializeIncidents called. Total CE events: {CEPersistence.CEEvents.Count}");

            // Convert CE events with EventType="incident" into Bannerlord incidents
            int incidentCount = 0;
            foreach (CEEvent ceEvent in CEPersistence.CEEvents.Where(ceEvent => ceEvent.EventType?.ToLower() == "incident"))
            {
                try
                {
                    ConvertCEEventToIncident(ceEvent);
                    incidentCount++;
                    CECustomHandler.ForceLogToFile($"Converted CE event '{ceEvent.Name}' to incident");
                }
                catch (Exception ex)
                {
                    CECustomHandler.ForceLogToFile($"Failed to convert CE event '{ceEvent.Name}' to incident: {ex.Message}");
                }
            }

            CECustomHandler.ForceLogToFile($"InitializeIncidents completed. Converted {incidentCount} incidents. Total registered incidents: {MBObjectManager.Instance.GetObjectTypeList<Incident>().Count}");
        }

        private void ConvertCEEventToIncident(CEEvent ceEvent)
        {
            // Convert CE event trigger conditions to Bannerlord incident triggers
            IncidentTrigger incidentTrigger = ConvertCETriggerToIncidentTrigger(ceEvent);

            // Convert CE event type to Bannerlord incident type
            IncidentType incidentType = ConvertCETypeToIncidentType(ceEvent);

            // Create cooldown based on CE event settings
            CampaignTime cooldown = GetCooldownFromCEEvent(ceEvent);

            // Create condition function
            Func<TextObject, bool> condition = CreateConditionFromCEEvent(ceEvent);

            // Register the incident
            Incident incident = RegisterIncident("ce_incident_" + ceEvent.Name.ToLower().Replace(" ", "_"), ceEvent.Name, ceEvent.Text, incidentTrigger, incidentType, cooldown, condition);

            // Convert CE options to incident options
            if (ceEvent.Options != null)
            {
                foreach (Option option in ceEvent.Options)
                {
                    try
                    {
                        AddCEOptionToIncident(incident, option, ceEvent);
                    }
                    catch (Exception ex)
                    {
                        CECustomHandler.ForceLogToFile($"Failed to add option '{option.OptionText}' to incident '{ceEvent.Name}': {ex.Message}");
                    }
                }
            }

            CECustomHandler.ForceLogToFile($"Successfully converted CE event '{ceEvent.Name}' to Bannerlord incident");
        }

        private IncidentTrigger ConvertCETriggerToIncidentTrigger(CEEvent ceEvent)
        {
            IncidentTrigger trigger = 0;

            // Convert based on CE event restrictions

            if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyEnteredSettlementIsVillage))
                trigger |= IncidentTrigger.EnteringVillage;
            else if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyLeavingSettlementIsVillage)) trigger |= IncidentTrigger.LeavingVillage;


            if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyEnteredSettlementIsTown))
                trigger |= IncidentTrigger.EnteringTown;
            else if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyLeavingSettlementIsTown)) trigger |= IncidentTrigger.LeavingTown;


            if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyEnteredSettlementIsCastle))
                trigger |= IncidentTrigger.EnteringCastle;
            else if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyLeavingSettlementIsCastle)) trigger |= IncidentTrigger.LeavingCastle;

            if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.LeavingBattle)) trigger |= IncidentTrigger.LeavingBattle;

            if (ceEvent.MultipleRestrictedListOfIncidentTriggers.Contains(RestrictedListOfIncidentTriggers.PartyIsWaitingInSettlement)) trigger |= IncidentTrigger.WaitingInSettlement;

            // Default fallback
            if (trigger == 0) trigger = IncidentTrigger.LeavingEncounter;

            return trigger;
        }

        private IncidentType ConvertCETypeToIncidentType(CEEvent ceEvent)
        {
            /*
            IncidentType determines the visual style and UI presentation of the incident in Bannerlord.
            Valid IncidentType values are:

            TroopSettlementRelation - Events involving troop-settlement relationships
            FoodConsumption - Events related to food and consumption
            PlightOfCivilians - Events about civilian suffering/difficulties
            PartyCampLife - Default for camp/party related events (shows camp background)
            AnimalIllness - Events involving sick animals
            Illness - Events involving illness/disease
            HuntingForaging - Events about hunting or foraging
            PostBattle - Events that occur after battles
            HardTravel - Events involving difficult travel conditions
            Profit - Events related to financial gain/loss
            DreamsSongsAndSigns - Events involving dreams, songs, or omens
            FiefManagement - Events about managing fiefs
            Siege - Events related to sieges
            Workshop - Events involving workshops/crafting

            To specify an IncidentType in your CE event XML, add:
            <IncidentType>PartyCampLife</IncidentType>
            */

            // Check if IncidentType is specified in the CEEvent
            if (!string.IsNullOrEmpty(ceEvent.IncidentType))
            {
                // Try to parse the incident type from the string
                if (Enum.TryParse<IncidentType>(ceEvent.IncidentType, out var parsedType))
                {
                    // Convert to the CampaignBehaviors enum type
                    return parsedType;
                }

                CECustomHandler.ForceLogToFile($"Warning: Unknown IncidentType '{ceEvent.IncidentType}' for event '{ceEvent.Name}', using default PartyCampLife");
            }

            // Default fallback - PartyCampLife for camp/party related events
            return IncidentType.PartyCampLife;
        }

        private CampaignTime GetCooldownFromCEEvent(CEEvent ceEvent)
        {
            /*
            Cooldown determines how long before this incident can trigger again.
            CampaignTime provides various time units:

            CampaignTime.Days(float days) - Cooldown in days (e.g., Days(30f) = 30 days)
            CampaignTime.Hours(float hours) - Cooldown in hours (e.g., Hours(24f) = 1 day)
            CampaignTime.Weeks(float weeks) - Cooldown in weeks (e.g., Weeks(2f) = 2 weeks)
            CampaignTime.Years(float years) - Cooldown in years (e.g., Years(1f) = 1 year)

            Common cooldown values:
            - Days(30f) = 30 days (about 1 month)
            - Days(60f) = 60 days (about 2 months) [current default]
            - Days(90f) = 90 days (about 3 months)
            - Weeks(4f) = 4 weeks (about 1 month)
            - Weeks(8f) = 8 weeks (about 2 months)

            To specify a cooldown in your CE event XML, add:
            <IncidentCooldown>Days(30f)</IncidentCooldown>
            */

            // Check if IncidentCooldown is specified in the CEEvent
            if (!string.IsNullOrEmpty(ceEvent.IncidentCooldown))
            {
                try
                {
                    CEVariablesLoader variableLoader = new CEVariablesLoader();
                    string expression = ceEvent.IncidentCooldown.Trim();

                    // Check if it's a function call with parentheses (e.g., "Days(30)")
                    if (expression.Contains("(") && expression.EndsWith(")"))
                    {
                        // Parse function calls like "Days(30f)", "Weeks(2f)", etc.
                        if (expression.StartsWith("Days(") && expression.EndsWith(")"))
                        {
                            string paramStr = expression.Substring(5, expression.Length - 6);
                            if (float.TryParse(paramStr, out float days))
                            {
                                return CampaignTime.Days(days);
                            }
                        }
                        else if (expression.StartsWith("Hours(") && expression.EndsWith(")"))
                        {
                            string paramStr = expression.Substring(6, expression.Length - 7);
                            if (float.TryParse(paramStr, out float hours))
                            {
                                return CampaignTime.Hours(hours);
                            }
                        }
                        else if (expression.StartsWith("Weeks(") && expression.EndsWith(")"))
                        {
                            string paramStr = expression.Substring(6, expression.Length - 7);
                            if (float.TryParse(paramStr, out float weeks))
                            {
                                return CampaignTime.Weeks(weeks);
                            }
                        }
                        else if (expression.StartsWith("Years(") && expression.EndsWith(")"))
                        {
                            string paramStr = expression.Substring(6, expression.Length - 7);
                            if (float.TryParse(paramStr, out float years))
                            {
                                return CampaignTime.Years(years);
                            }
                        }
                    }
                    else
                    {
                        // If no parentheses, treat as hours using CEVariablesLoader
                        float hours = variableLoader.GetFloatFromXML(ceEvent.IncidentCooldown);
                        return CampaignTime.Hours(hours);
                    }

                    CECustomHandler.ForceLogToFile($"Warning: Invalid IncidentCooldown format '{ceEvent.IncidentCooldown}' for event '{ceEvent.Name}', using default 60 days");
                }
                catch (Exception ex)
                {
                    CECustomHandler.ForceLogToFile($"Warning: Failed to parse IncidentCooldown '{ceEvent.IncidentCooldown}' for event '{ceEvent.Name}': {ex.Message}, using default 60 days");
                }
            }

            // Default fallback - 60 days
            return CampaignTime.Days(60f);
        }

        private Func<TextObject, bool> CreateConditionFromCEEvent(CEEvent ceEvent)
        {
            // Use the appropriate delegate based on event type
            try
            {
                if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                {
                    IncidentHandlerDelegateCaptive captiveDelegate = new IncidentHandlerDelegateCaptive(ceEvent, CEPersistence.CEEvents);
                    return captiveDelegate.CreateCondition();
                }

                if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                {
                    IncidentHandlerDelegateRandom randomDelegate = new IncidentHandlerDelegateRandom(ceEvent, CEPersistence.CEEvents);
                    return randomDelegate.CreateCondition();
                }

                if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                {
                    IncidentHandlerDelegateCaptor captorDelegate = new IncidentHandlerDelegateCaptor(ceEvent, CEPersistence.CEEvents);
                    return captorDelegate.CreateCondition();
                }

                // Default behavior - use random delegate
                IncidentHandlerDelegateRandom defaultDelegate = new IncidentHandlerDelegateRandom(ceEvent, CEPersistence.CEEvents);
                return defaultDelegate.CreateCondition();
            }
            catch
            {
                return text => true;
            }
        }

        private void AddCEOptionToIncident(Incident incident, Option ceOption, CEEvent ceEvent)
        {
            // Convert CE consequences to incident effects
            List<IncidentEffect> effects = ConvertCEOptionToIncidentEffects(ceOption, ceEvent);

            Incident.IncidentOptionConditionDelegate condition = ConvertCEOptionToCondition(ceOption);

            Incident.IncidentOptionConsequenceDelegate consequence = ConvertCEOptionToConsequence(ceOption, ceEvent);

            // Add the option to the incident
            incident.AddOption(ceOption.OptionText, effects, condition, consequence);
        }

        private List<IncidentEffect> ConvertCEOptionToIncidentEffects(Option option, CEEvent ceEvent)
        {
            List<IncidentEffect> effects = new List<IncidentEffect>();

            try
            {
                // Convert simple CE consequences to incident effects using Bannerlord's static factory methods
                // These will be displayed in the incident UI and applied immediately

                CEVariablesLoader variableLoader = new CEVariablesLoader();

                // Get the IncidentEffect type to access its static methods
                Type incidentEffectType = typeof(IncidentEffect);

                // Handle effects based on event type (Random or Captor)
                if (true) // Allow for all event types for now
                {
                    // Gold changes - create actual incident effect
                    if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold))
                    {
                        try
                        {
                            int goldAmount = 0;
                            if (!string.IsNullOrEmpty(option.GoldTotal))
                                goldAmount = variableLoader.GetIntFromXML(option.GoldTotal);
                            else if (!string.IsNullOrEmpty(ceEvent.GoldTotal))
                                goldAmount = variableLoader.GetIntFromXML(ceEvent.GoldTotal);

                            if (goldAmount != 0)
                            {
                                // Use Harmony to access the GoldChange method
                                MethodInfo goldChangeMethod = AccessTools.Method(incidentEffectType, "GoldChange", new[] { typeof(Func<int>) });
                                if (goldChangeMethod != null)
                                {
                                    IncidentEffect goldEffect = (IncidentEffect)goldChangeMethod.Invoke(null, new object[] { (Func<int>)(() => goldAmount) });
                                    effects.Add(goldEffect);
                                    CECustomHandler.ForceLogToFile($"Created gold effect for option '{option.OptionText}': {goldAmount} gold");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CECustomHandler.LogToFile($"Failed to create gold effect: {ex.Message}");
                        }
                    }

                    // Renown changes - create actual incident effect
                    if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown))
                    {
                        try
                        {
                            float renownAmount = 0f;
                            if (!string.IsNullOrEmpty(option.RenownTotal))
                                renownAmount = variableLoader.GetFloatFromXML(option.RenownTotal);
                            else if (!string.IsNullOrEmpty(ceEvent.RenownTotal))
                                renownAmount = variableLoader.GetFloatFromXML(ceEvent.RenownTotal);

                            if (renownAmount != 0f)
                            {
                                // Use Harmony to access the RenownChange method
                                MethodInfo renownChangeMethod = AccessTools.Method(incidentEffectType, "RenownChange", new Type[] { typeof(float) });
                                if (renownChangeMethod != null)
                                {
                                    IncidentEffect renownEffect = (IncidentEffect)renownChangeMethod.Invoke(null, new object[] { renownAmount });
                                    effects.Add(renownEffect);
                                    CECustomHandler.ForceLogToFile($"Created renown effect for option '{option.OptionText}': {renownAmount} renown");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CECustomHandler.LogToFile($"Failed to create renown effect: {ex.Message}");
                        }
                    }

                    // Health changes - create actual incident effect
                    if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth))
                    {
                        try
                        {
                            int healthAmount = 0;
                            if (!string.IsNullOrEmpty(option.HealthTotal))
                                healthAmount = variableLoader.GetIntFromXML(option.HealthTotal);
                            else if (!string.IsNullOrEmpty(ceEvent.HealthTotal))
                                healthAmount = variableLoader.GetIntFromXML(ceEvent.HealthTotal);

                            if (healthAmount != 0)
                            {
                                // Use Harmony to access the HealthChance method
                                MethodInfo healthChanceMethod = AccessTools.Method(incidentEffectType, "HealthChance", new Type[] { typeof(int) });
                                if (healthChanceMethod != null)
                                {
                                    IncidentEffect healthEffect = (IncidentEffect)healthChanceMethod.Invoke(null, new object[] { healthAmount });
                                    effects.Add(healthEffect);
                                    CECustomHandler.ForceLogToFile($"Created health effect for option '{option.OptionText}': {healthAmount} health");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CECustomHandler.LogToFile($"Failed to create health effect: {ex.Message}");
                        }
                    }

                    // Morale changes - create actual incident effect
                    if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale))
                    {
                        try
                        {
                            float moraleAmount = 0f;
                            if (!string.IsNullOrEmpty(option.MoraleTotal))
                                moraleAmount = variableLoader.GetFloatFromXML(option.MoraleTotal);
                            else if (!string.IsNullOrEmpty(ceEvent.MoraleTotal))
                                moraleAmount = variableLoader.GetFloatFromXML(ceEvent.MoraleTotal);

                            if (moraleAmount != 0f)
                            {
                                // Use Harmony to access the MoraleChange method
                                MethodInfo moraleChangeMethod = AccessTools.Method(incidentEffectType, "MoraleChange", new Type[] { typeof(float) });
                                if (moraleChangeMethod != null)
                                {
                                    IncidentEffect moraleEffect = (IncidentEffect)moraleChangeMethod.Invoke(null, new object[] { moraleAmount });
                                    effects.Add(moraleEffect);
                                    CECustomHandler.ForceLogToFile($"Created morale effect for option '{option.OptionText}': {moraleAmount} morale");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CECustomHandler.LogToFile($"Failed to create morale effect: {ex.Message}");
                        }
                    }
                }

                CECustomHandler.ForceLogToFile($"Converted {effects.Count} incident effects for option '{option.OptionText}' in {GetEventTypeString(ceEvent)} event");
            }
            catch (Exception ex)
            {
                CECustomHandler.ForceLogToFile($"Failed to convert incident effects: {ex.Message}");
            }

            return effects;
        }

        private string GetEventTypeString(CEEvent ceEvent)
        {
            if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                return "captive";

            if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                return "random";

            if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                return "captor";

            return "unknown";
        }

        private Incident.IncidentOptionConditionDelegate ConvertCEOptionToCondition(Option option)
        {
            return null;
        }

        private Incident.IncidentOptionConsequenceDelegate ConvertCEOptionToConsequence(Option option, CEEvent ceEvent)
        {
            return () =>
            {
                try
                {
                    // Use the appropriate delegate based on event type
                    if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captive))
                    {
                        IncidentHandlerDelegateCaptive captiveDelegate = new IncidentHandlerDelegateCaptive(ceEvent, option, CEPersistence.CEEvents);
                        captiveDelegate.ExecuteConsequences();
                    }
                    else if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                    {
                        IncidentHandlerDelegateRandom randomDelegate = new IncidentHandlerDelegateRandom(ceEvent, option, CEPersistence.CEEvents);
                        randomDelegate.ExecuteConsequences();
                    }
                    else if (ceEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                    {
                        IncidentHandlerDelegateCaptor captorDelegate = new IncidentHandlerDelegateCaptor(ceEvent, option, CEPersistence.CEEvents);
                        captorDelegate.ExecuteConsequences();
                    }
                    else
                    {
                        // Default behavior - use random delegate
                        IncidentHandlerDelegateRandom defaultDelegate = new IncidentHandlerDelegateRandom(ceEvent, option, CEPersistence.CEEvents);
                        defaultDelegate.ExecuteConsequences();
                    }

                    CECustomHandler.ForceLogToFile($"Executed consequences for incident option in event '{ceEvent.Name}'");
                }
                catch (Exception ex)
                {
                    CECustomHandler.ForceLogToFile($"Failed to execute consequences for incident option: {ex.Message}");
                }
            };
        }

        private CampaignTime _lastGlobalIncidentCooldown = CampaignTime.Zero;
        public Dictionary<Incident, CampaignTime> _incidentsOnCooldown = new Dictionary<Incident, CampaignTime>();
        private long _activeIncidentSeed;
        private bool _canInvokeSettlementEvent;
    }
}
