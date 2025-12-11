using CaptivityEvents.Custom;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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

        static readonly List<MBGUID> ms = [];

        public static void RestartCharacter(CharacterObject character)
        {
            try
            {
                CharacterObject characterObject = CharacterObject.All.GetRandomElement();
                BodyProperties bodyProperties = characterObject.GetBodyProperties(null, -1);
                FaceGenerationParams faceGenerationParams = FaceGenerationParams.Create();
                MBBodyProperties.GetParamsFromKey(ref faceGenerationParams, bodyProperties, false, false);
                MBBodyProperties.ProduceNumericKeyWithParams(faceGenerationParams, false, false, ref bodyProperties);
                character.UpdatePlayerCharacterBodyProperties(bodyProperties, characterObject.Race, characterObject.IsFemale);

                if (character?.HeroObject != null) DisableHeroAction.Apply(character.HeroObject);
            }
            catch (Exception e) { CECustomHandler.LogToFile("Failed RestartCharacter " + e); }
        }

        public static void RemoveParty()
        {
            try
            {
                List<MobileParty> mobileParties = MobileParty.All
                .Where((mobileParty) =>
                {
                    return mobileParty.StringId.StartsWith("CustomPartyCE_");
                }
                ).ToList();

                if (mobileParties.Count != 0)
                {
                    CECustomHandler.ForceLogToFile("Removing Parties");
                    InformationManager.DisplayMessage(new InformationMessage("Removing CustomPartyCE_ Parties", Colors.Red));
                }

                foreach (MobileParty mobile in mobileParties)
                {
                    DestroyPartyAction.Apply(PartyBase.MainParty, mobile);
                }
            }
            catch (Exception e) { CECustomHandler.LogToFile("Failed RemoveParty " + e); }
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
                        CECustomHandler.ForceLogToFile("CharacterObject UpgradeTargets is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject UpgradeTargets Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                        RestartCharacter(__instance);
                        RemoveParty();
                    }
                }
                catch (Exception e) { CECustomHandler.LogToFile("Failed UpgradeTargets " + e); }
                __result = [];
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
                        CECustomHandler.ForceLogToFile("CharacterObject Culture is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject Culture Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                        RestartCharacter(__instance);
                        RemoveParty();
                    }
                }
                catch (Exception e) { CECustomHandler.LogToFile("Failed Culture " + e); }
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
                        CECustomHandler.ForceLogToFile("CharacterObject FirstBattleEquipment is null on " + __instance.Id);
                        InformationManager.DisplayMessage(new InformationMessage("Invalid CharacterObject FirstBattleEquipment Detected of " + __instance.Id + ".", Colors.Red));
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;

                        RestartCharacter(__instance);
                        RemoveParty();
                    }
                }
                catch (Exception e) { CECustomHandler.LogToFile("Failed FirstBattleEquipment " + e); }
                __result = new Equipment();
            }
        }


    }
}
