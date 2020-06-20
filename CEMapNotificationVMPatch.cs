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
using TaleWorlds.Library;
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
			CECustomHandler.ForceLogToFile("EventCaptorNotifications: " + CESettings.Instance.EventCaptorNotifications + ".");
			return CESettings.Instance.EventCaptorNotifications;
		}

		[HarmonyPostfix]
		private static void DetermineNotificationType(MapNotificationVM __instance, ref MapNotificationItemBaseVM __result)
		{
			if (__result.Data.TitleText.Equals(new TextObject("Captor Event", null)))
			{
				__result = new NewTestNotificationItemVM(__result.Data, null, new Action<MapNotificationItemBaseVM>((MapNotificationItemBaseVM item) =>
				{
					object[] parameters = new object[1];
					parameters[0] = item;
					RemoveNotificationItem.Invoke(__instance, parameters);
				}));
			}
		}

		public class NewTestNotificationItemVM : MapNotificationItemBaseVM
		{

			public NewTestNotificationItemVM(InformationData data, Action onInspect, Action<MapNotificationItemBaseVM> onRemove) : base(data, onInspect, onRemove)
			{
				base.NotificationIdentifier = "death";
				_onInspect = delegate ()
				{
					OnNewTestNotificationInspect();
				};
			}

			public override void ManualRefreshRelevantStatus()
			{
				base.ManualRefreshRelevantStatus();
				if (MobileParty.MainParty.Party.PrisonRoster.Count == 0)
				{
					CESubModule.notificationExists = false;
					CESubModule.LoadCampaignNotificationTexture("default");
					base.ExecuteRemove();
				}
			}

			private void OnNewTestNotificationInspect()
			{
				CESubModule.notificationExists = false;
				CESubModule.LoadCampaignNotificationTexture("default");
				base.ExecuteRemove();
				if (MobileParty.MainParty.Party.PrisonRoster.Count > 0)
				{
					// Declare Variables
					CharacterObject Captive = MobileParty.MainParty.Party.PrisonRoster.GetRandomElement().Character;
					var returnString = CEEventManager.ReturnWeightedChoiceOfEventsPartyLeader(Captive);
					if (returnString != null)
					{
						if (Game.Current.GameStateManager.ActiveState is MapState mapState)
						{
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

							GameMenu.SwitchToMenu(returnString.Name);
						}
					}
				}
			}

		}
	}
}
