#define V172
using HarmonyLib;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
#if V171
using TaleWorlds.CampaignSystem;
#else
using TaleWorlds.CampaignSystem.Extensions;
#endif

namespace CaptivityEvents.Patches
{
    // TaleWorlds.CampaignSystem Attributes
    [HarmonyPatch(typeof(Attributes), "All", MethodType.Getter)]
    internal class CEPatchAttributes
    {
        [HarmonyPostfix]
        private static void All(ref MBReadOnlyList<CharacterAttribute> __result)
        {
            if (__result.Any((CharacterAttribute item) => item.StringId == "CEAttribute"))
            {
                __result = new MBReadOnlyList<CharacterAttribute>(__result.Where((CharacterAttribute item) => item.StringId != "CEAttribute").ToList());
            }
  
        }

    }
}
