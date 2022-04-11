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
    // TaleWorlds.CampaignSystem Skills
    [HarmonyPatch(typeof(Skills), "All", MethodType.Getter)]
    internal class CEPatchSkills
    {
        [HarmonyPostfix]
        private static void All(ref MBReadOnlyList<SkillObject> __result)
        {
            if (__result.Any((SkillObject item) => item?.CharacterAttribute == null || item?.CharacterAttribute?.StringId == "CEAttribute"))
            {
                __result = new MBReadOnlyList<SkillObject>(__result.Where((SkillObject item) => item.CharacterAttribute != null && item.CharacterAttribute.StringId != "CEAttribute").ToList());
            }

        }

    }
}
