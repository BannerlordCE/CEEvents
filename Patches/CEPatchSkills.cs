#define V127

using HarmonyLib;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Extensions;

namespace CaptivityEvents.Patches
{
    // TaleWorlds.CampaignSystem Skills
    [HarmonyPatch(typeof(Skills), "All", MethodType.Getter)]
    internal class CEPatchSkills
    {
        [HarmonyPostfix]
        private static void All(ref MBReadOnlyList<SkillObject> __result)
        {
            if (__result.Any((SkillObject item) => item.Attributes.Any((CharacterAttribute attribute) => attribute?.StringId == "CEAttribute")))
            {
                __result = new MBReadOnlyList<SkillObject>(__result.Where((SkillObject item) => item.Attributes.Any((CharacterAttribute attribute) => attribute?.StringId != "CEAttribute")).ToList());
            }
        }
    }
}