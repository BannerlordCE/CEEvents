using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(HeroCreator), "DeliverOffSpring")]
    internal static class CEPatchHeroCreator
    {
        [HarmonyPostfix]
        public static void DeliverOffSpring(ref Hero __result, Hero mother, Hero father, bool isOffspringFemale)
        {
            __result.Culture = __result.Father.Culture;
            __result.Clan = __result.Mother.Clan;
            if (__result.Clan == Clan.PlayerClan)
            {
                __result.SetHasMet();
            }
        }
    }
}
