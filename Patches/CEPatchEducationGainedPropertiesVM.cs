#define BETA
using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Education;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
#if BETA
    // TaleWorlds.CampaignSystem.ViewModelCollection.Education EducationGainedPropertiesVM
    [HarmonyPatch(typeof(EducationGainedPropertiesVM), "PopulateInitialValues")]
    internal class CEPatchEducationGainedPropertiesVM
    {
		public static AccessTools.FieldRef<EducationGainedPropertiesVM, Hero> _child = AccessTools.FieldRefAccess<EducationGainedPropertiesVM, Hero>("_child");
		public static AccessTools.FieldRef<EducationGainedPropertiesVM, Dictionary<SkillObject, Tuple<int, int>>> _affectedSkillFocusMap = AccessTools.FieldRefAccess<EducationGainedPropertiesVM, Dictionary<SkillObject, Tuple<int, int>>>("_affectedSkillFocusMap");
		public static AccessTools.FieldRef<EducationGainedPropertiesVM, Dictionary<SkillObject, Tuple<int, int>>> _affectedSkillValueMap = AccessTools.FieldRefAccess<EducationGainedPropertiesVM, Dictionary<SkillObject, Tuple<int, int>>>("_affectedSkillValueMap");
		public static AccessTools.FieldRef<EducationGainedPropertiesVM, Dictionary<CharacterAttribute, Tuple<int, int>>> _affectedAttributesMap = AccessTools.FieldRefAccess<EducationGainedPropertiesVM, Dictionary<CharacterAttribute, Tuple<int, int>>>("_affectedAttributesMap");


	   [HarmonyPrefix]
        public static bool PopulateInitialValues(EducationGainedPropertiesVM __instance)
        {
			foreach (SkillObject skillObject in Skills.All)
			{
				if (CESkills.CustomSkills.Exists(item => item.StringId == skillObject.StringId)) continue;
				int focus = _child(__instance).HeroDeveloper.GetFocus(skillObject);
				if (_affectedSkillFocusMap(__instance).ContainsKey(skillObject))
				{
					Tuple<int, int> tuple = _affectedSkillFocusMap(__instance)[skillObject];
					_affectedSkillFocusMap(__instance)[skillObject] = new Tuple<int, int>(tuple.Item1 + focus, 0);
				}
				else
				{
					_affectedSkillFocusMap(__instance).Add(skillObject, new Tuple<int, int>(focus, 0));
				}
				int skillValue = _child(__instance).GetSkillValue(skillObject);
				if (_affectedSkillValueMap(__instance).ContainsKey(skillObject))
				{
					Tuple<int, int> tuple2 = _affectedSkillValueMap(__instance)[skillObject];
					_affectedSkillValueMap(__instance)[skillObject] = new Tuple<int, int>(tuple2.Item1 + skillValue, 0);
				}
				else
				{
					_affectedSkillValueMap(__instance).Add(skillObject, new Tuple<int, int>(skillValue, 0));
				}
			}

			foreach (CharacterAttribute characterAttribute in Attributes.All)
			{
				int attributeValue = _child(__instance).GetAttributeValue(characterAttribute);
				if (_affectedAttributesMap(__instance).ContainsKey(characterAttribute))
				{
					Tuple<int, int> tuple3 = _affectedAttributesMap(__instance)[characterAttribute];
					_affectedAttributesMap(__instance)[characterAttribute] = new Tuple<int, int>(tuple3.Item1 + attributeValue, 0);
				}
				else
				{
					_affectedAttributesMap(__instance).Add(characterAttribute, new Tuple<int, int>(attributeValue, 0));
				}
			}

			return false;
        }
    }
#endif
}