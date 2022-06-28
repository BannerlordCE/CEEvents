﻿
using CaptivityEvents.Custom;
using CaptivityEvents.Helper;
using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    public class CESceneNotification : SceneNotificationData
    {
        public Hero MaleHero { get; }

        public Hero FemaleHero { get; }

        public override string SceneID { get; }

        public override TextObject TitleText
        {
            get
            {
                return new TextObject("");
            }
        }

        public override IEnumerable<SceneNotificationCharacter> GetSceneNotificationCharacters()
        {
            List<SceneNotificationCharacter> list = new();

            try
            {
                Equipment overridenEquipmentMale = MaleHero.CivilianEquipment.Clone(false);
                Equipment overridenEquipmentFemale = FemaleHero.CivilianEquipment.Clone(false);


                for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
                {
                    overridenEquipmentMale[i] = EquipmentElement.Invalid;
                    overridenEquipmentFemale[i] = EquipmentElement.Invalid;
                }

                list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(MaleHero, overridenEquipmentMale, false, default, uint.MaxValue, uint.MaxValue, false));
                list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(FemaleHero, overridenEquipmentFemale, false, default, uint.MaxValue, uint.MaxValue, false));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Invalid Scene GetSceneNotificationCharacters: " + e);
            }

            return list;
        }

        public CESceneNotification(Hero maleHero, Hero femaleHero, string sceneID)
        {
            SceneID = sceneID;
            try
            {
                if (maleHero == null)
                {
                    CharacterObject m = CharacterObject.PlayerCharacter.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate(characterObject => characterObject.IsFemale == false && characterObject.Occupation == Occupation.Wanderer);
                    MaleHero = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, CEHelper.HelperMBRandom(20) + 20);
                    MaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }
                else
                {
                    maleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                    MaleHero = maleHero;
                }

                if (femaleHero == null)
                {
                    CharacterObject m = CharacterObject.PlayerCharacter.Culture.NotableAndWandererTemplates.GetRandomElementWithPredicate(characterObject => characterObject.IsFemale != false && characterObject.Occupation == Occupation.Wanderer);
                    femaleHero = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), CampaignData.NeutralFaction, CampaignData.NeutralFaction, CEHelper.HelperMBRandom(20) + 20);
                    femaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }
                else
                {
                    femaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                    FemaleHero = femaleHero;
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Invalid Scene CESceneNotification: " + e);
            }
        }

    }
}
