using CaptivityEvents.Helper;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Patches
{
    //public static AccessTools.FieldRef<Hero, Hero> _spouse = AccessTools.FieldRefAccess<Hero, Hero>("_spouse");
    //public static AccessTools.FieldRef<Hero, List<Hero>> _exSpouses = AccessTools.FieldRefAccess<Hero, List<Hero>>("_exSpouses");

    [HarmonyPatch(typeof(Hero), "Spouse", MethodType.Getter)]
    internal static class CEHeroPatch
    {
        public static void Postfix(Hero __instance, ref Hero __result)
        {
            if (CEContext.spouseOne != null && __instance == CEContext.spouseOne) __result = CEContext.spouseTwo;
            else if (CEContext.spouseTwo != null && __instance == CEContext.spouseTwo) __result = CEContext.spouseOne;
        }
    }
}