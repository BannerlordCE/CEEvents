#define V120

using CaptivityEvents.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Patches
{

    [HarmonyPatch(typeof(BasicCharacterObject))]
    internal class CEPatchBasicCharacterObject
    {
#if V120
        public static AccessTools.FieldRef<BasicCharacterObject, MBCharacterSkills> MBCharacterSkills = AccessTools.FieldRefAccess<BasicCharacterObject, MBCharacterSkills>("DefaultCharacterSkills");
#else
        public static AccessTools.FieldRef<BasicCharacterObject, MBCharacterSkills> MBCharacterSkills = AccessTools.FieldRefAccess<BasicCharacterObject, MBCharacterSkills>("CharacterSkills");
#endif

        static readonly List<MBGUID> ms = new();

        [HarmonyPatch("GetSkillValue")]
        [HarmonyPrefix]
        public static bool CharacterSkills(BasicCharacterObject __instance, ref int __result, SkillObject skill)
        {

            if (MBCharacterSkills.Invoke(__instance) == null)
            {
                try
                {
                    if (!ms.Contains(__instance.Id))
                    {
                        ms.Add(__instance.Id);
                        CECustomHandler.ForceLogToFile("CharacterObject is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                    }
                }
                catch (Exception)
                {
                }
                __result = 0;
                return false;
            }
            return true;
        }

    }
}
