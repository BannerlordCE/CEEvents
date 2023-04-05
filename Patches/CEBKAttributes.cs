using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Patches
{
    // Compatibility to Banner Kings
    internal class CEBKAttributes
    {
        // TaleWorlds.CampaignSystem Attributes
        [HarmonyPatch(typeof(Campaign), "AllCharacterAttributes", MethodType.Getter)]
        internal class CEPatchAllCharacterAttributes
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
}
