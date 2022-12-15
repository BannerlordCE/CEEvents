using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Patches
{
	[HarmonyPatch(typeof(HeroCreator), "DeliverOffSpring")]
	internal static class CEHeroCreatorPatch
	{
		[HarmonyPostfix]
		public static void DeliverOffSpring(ref Hero __result, Hero mother, Hero father, bool isOffspringFemale, CultureObject culture = null)
		{
			__result.Clan = __result.Mother.Clan;
			if (__result.Clan != Clan.PlayerClan)
			{
				__result.HasMet = false;
			}
		}
	}
}
