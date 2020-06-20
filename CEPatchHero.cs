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
            if (CEHelper.spouseOne != null && __instance == CEHelper.spouseOne)
            {
                __result = CEHelper.spouseTwo;
            }
            else
            {
                bool flag2 = CEHelper.spouseTwo != null && __instance == CEHelper.spouseTwo;
                if (flag2)
                {
                    __result = CEHelper.spouseOne;
                }
            }
        }
    }
}