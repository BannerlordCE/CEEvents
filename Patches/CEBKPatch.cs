using HarmonyLib;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Patches
{

    // Compatibility to Banner Kings
    internal class CEBKPatch
    {
        // TaleWorlds.CampaignSystem AllCharacterAttributes
        [HarmonyPatch(typeof(Campaign), "AllCharacterAttributes", MethodType.Getter)]
        internal class CEPatchAllCharacterAttributes
        {
            [HarmonyPostfix]
            private static void AllCharacterAttributes(ref MBReadOnlyList<CharacterAttribute> __result)
            {
                if (__result.Any((CharacterAttribute item) => item.StringId == "CEAttribute"))
                {
                    __result = new MBReadOnlyList<CharacterAttribute>(__result.Where((CharacterAttribute item) => item.StringId != "CEAttribute").ToList());
                }
            }
        }

        // TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation CharacterCreationGainedPropertiesVM GetItemFromAttribute
        [HarmonyPatch(typeof(CharacterCreationGainedPropertiesVM), "GetItemFromAttribute")]
        internal class CEPatchGetItemFromAttribute
        {
            [HarmonyPostfix]
            private static void GetItemFromAttribute(CharacterCreationGainedPropertiesVM __instance, ref CharacterCreationGainedAttributeItemVM __result, CharacterAttribute attribute)
            {
                if (__result == null)
                {
                    FieldInfo _characterCreation = __instance.GetType().GetField("_characterCreation", BindingFlags.Instance | BindingFlags.NonPublic);
                    FieldInfo _currentIndex = __instance.GetType().GetField("_currentIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                    __instance.GainGroups.Add(new CharacterCreationGainGroupItemVM(attribute));
                    CharacterCreationGainGroupItemVM characterCreationGainGroupItemVM = __instance.GainGroups.SingleOrDefault((CharacterCreationGainGroupItemVM g) => g.AttributeObj == attribute);
                    __result = characterCreationGainGroupItemVM.Attribute;
                }
            }
        }
    }
}
