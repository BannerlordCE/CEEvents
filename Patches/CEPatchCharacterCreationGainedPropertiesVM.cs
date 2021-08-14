#define STABLE
using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.CampaignSystem.ViewModelCollection.Education;
using TaleWorlds.Core;

namespace CaptivityEvents.Patches
{
	//  TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation CharacterCreationGainedPropertiesVM
	[HarmonyPatch(typeof(CharacterCreationGainedPropertiesVM), "PopulateInitialValues")]
	internal class CEPatchCharacterCreationGainedPropertiesVM
	{
		public static AccessTools.FieldRef<CharacterCreationGainedPropertiesVM, Dictionary<SkillObject, Tuple<int, int>>> _affectedSkillMap = AccessTools.FieldRefAccess<CharacterCreationGainedPropertiesVM, Dictionary<SkillObject, Tuple<int, int>>>("_affectedSkillMap");
		public static AccessTools.FieldRef<CharacterCreationGainedPropertiesVM, Dictionary<CharacterAttribute, Tuple<int, int>>> _affectedAttributesMap = AccessTools.FieldRefAccess<CharacterCreationGainedPropertiesVM, Dictionary<CharacterAttribute, Tuple<int, int>>>("_affectedAttributesMap");


		[HarmonyPrefix]
		public static bool PopulateInitialValues(CharacterCreationGainedPropertiesVM __instance)
		{
			foreach (SkillObject skillObject in Skills.All)
			{
				if (skillObject.CharacterAttribute == null || skillObject.CharacterAttribute.StringId == "CEAttribute") continue;
				int focus = Hero.MainHero.HeroDeveloper.GetFocus(skillObject);
				if (_affectedSkillMap(__instance).ContainsKey(skillObject))
				{
					Tuple<int, int> tuple = _affectedSkillMap(__instance)[skillObject];
					_affectedSkillMap(__instance)[skillObject] = new Tuple<int, int>(tuple.Item1 + focus, 0);
				}
				else
				{
					_affectedSkillMap(__instance).Add(skillObject, new Tuple<int, int>(focus, 0));
				}
			}
			foreach (CharacterAttribute characterAttribute in Attributes.All)
			{
				int attributeValue = Hero.MainHero.GetAttributeValue(characterAttribute);
				if (_affectedAttributesMap(__instance).ContainsKey(characterAttribute))
				{
					Tuple<int, int> tuple2 = _affectedAttributesMap(__instance)[characterAttribute];
					_affectedAttributesMap(__instance)[characterAttribute] = new Tuple<int, int>(tuple2.Item1 + attributeValue, 0);
				}
				else
				{
					_affectedAttributesMap(__instance).Add(characterAttribute, new Tuple<int, int>(attributeValue, 0));
				}
			}

			return false;
		}
	}
}