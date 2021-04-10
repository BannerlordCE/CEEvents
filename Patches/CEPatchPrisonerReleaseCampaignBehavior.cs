using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(PrisonerReleaseCampaignBehavior))]
	internal class CEPatchPrisonerReleaseCampaignBehavior
	{
		[HarmonyPatch("OnGameLoaded")]
		[HarmonyPrefix]
		private static bool OnGameLoaded(CampaignGameStarter campaignGameStarter)
		{
			// 1.5.8 Fix
			foreach (Settlement settlement in Settlement.All)
			{
				foreach (TroopRosterElement troopRosterElement in settlement.Party.PrisonRoster.GetTroopRoster())
				{
					if (troopRosterElement.Character.IsHero && troopRosterElement.Character.HeroObject != Hero.MainHero && !troopRosterElement.Character.HeroObject.MapFaction.IsAtWarWith(settlement.MapFaction))
					{
						if (troopRosterElement.Character.HeroObject.PartyBelongedToAsPrisoner == settlement.Party && troopRosterElement.Character.HeroObject.IsPrisoner)
						{
							EndCaptivityAction.ApplyByReleasing(troopRosterElement.Character.HeroObject, null);
						}
						else
						{
							settlement.Party.PrisonRoster.RemoveTroop(troopRosterElement.Character, 1, default(UniqueTroopDescriptor), 0);
						}
					}
				}
			}
			return false;
		}

      
    }
}
