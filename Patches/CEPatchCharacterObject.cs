#define V181

using CaptivityEvents.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Patches
{

    [HarmonyPatch(typeof(CharacterObject))]
    internal static class CEPatchCharacterObject
    {

        static readonly List<MBGUID> ms = new();

        public static void RestartCharacter(CharacterObject character)
        {
            try
            {
                CharacterObject characterObject = CharacterObject.All.GetRandomElement();
                BodyProperties bodyProperties = characterObject.GetBodyProperties(null, -1);
                FaceGenerationParams faceGenerationParams = FaceGenerationParams.Create();
#if V181
                MBBodyProperties.GetParamsFromKey(ref faceGenerationParams, bodyProperties, false);
                faceGenerationParams._heightMultiplier = 0.5f;
                MBBodyProperties.ProduceNumericKeyWithParams(faceGenerationParams, false, ref bodyProperties);
                character.UpdatePlayerCharacterBodyProperties(bodyProperties, characterObject.Race, characterObject.IsFemale);
                character.Culture = new CultureObject();
#else
                MBBodyProperties.GetParamsFromKey(ref faceGenerationParams, bodyProperties, false, false);
                faceGenerationParams._heightMultiplier = 0.5f;
                MBBodyProperties.ProduceNumericKeyWithParams(faceGenerationParams, false, false, ref bodyProperties);
                character.UpdatePlayerCharacterBodyProperties(bodyProperties, characterObject.Race, characterObject.IsFemale);
                character.Culture = new CultureObject();
#endif
            }
            catch (Exception) { }
        }

        public static void RemoveParty()
        {
            List<MobileParty> mobileParties = MobileParty.All
                .Where((mobileParty) =>
                {
                    return mobileParty.StringId.StartsWith("CustomPartyCE_");
                }
                ).ToList();

            foreach (MobileParty mobile in mobileParties)
            {
                mobile.RemoveParty();
            }
        }


        [HarmonyPatch("UpgradeTargets", MethodType.Getter)]
        [HarmonyPostfix]
        public static void UpgradeTargets(CharacterObject __instance, ref CharacterObject[] __result)
        {
            if (__result == null)
            {
                try
                {
                    if (!ms.Contains(__instance.Id))
                    {
                        ms.Add(__instance.Id);
                        CECustomHandler.ForceLogToFile("CharacterObject is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                        RestartCharacter(__instance);
                        RemoveParty();
                    }
                }
                catch (Exception) { }
                __result = new CharacterObject[0];
            }
        }

        [HarmonyPatch("Culture", MethodType.Getter)]
        [HarmonyPostfix]
        public static void Culture(CharacterObject __instance, ref CultureObject __result)
        {
            if (__result == null)
            {
                try
                {
                    if (!ms.Contains(__instance.Id))
                    {
                        ms.Add(__instance.Id);
                        CECustomHandler.ForceLogToFile("CharacterObject is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                        RestartCharacter(__instance);
                        RemoveParty();
                    }
                }
                catch (Exception) { }
                __result = new CultureObject();
            }
        }

        [HarmonyPatch("FirstBattleEquipment", MethodType.Getter)]
        [HarmonyPostfix]
        public static void FirstBattleEquipment(CharacterObject __instance, ref Equipment __result)
        {
            if (__result == null)
            {
                try
                {
                    if (!ms.Contains(__instance.Id))
                    {
                        ms.Add(__instance.Id);
                        CECustomHandler.ForceLogToFile("CharacterObject is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                        RestartCharacter(__instance);
                        RemoveParty();
                    }
                }
                catch (Exception) { }
                __result = new Equipment();
            }
        }


    }
}
