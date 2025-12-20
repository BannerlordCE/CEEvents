
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

        public override SceneNotificationCharacter[] GetSceneNotificationCharacters()
        {
            List<SceneNotificationCharacter> list = [];

            try
            {
                Equipment overridenEquipmentMale = MaleHero.CivilianEquipment.Clone();
                Equipment overridenEquipmentFemale = FemaleHero.CivilianEquipment.Clone();


                for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
                {
                    overridenEquipmentMale[i] = EquipmentElement.Invalid;
                    overridenEquipmentFemale[i] = EquipmentElement.Invalid;
                }

                list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(MaleHero, overridenEquipmentMale));
                list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(FemaleHero, overridenEquipmentFemale));
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Invalid Scene GetSceneNotificationCharacters: " + e);
            }

            return list.ToArray();
        }

        public CESceneNotification(Hero maleHero, Hero femaleHero, string sceneID)
        {
            SceneID = sceneID;

            try
            {
                if (maleHero == null)
                {
                    CharacterObject m = Campaign.Current.Characters.GetRandomElementWithPredicate(characterObject => characterObject.Culture == CharacterObject.PlayerCharacter.Culture && characterObject.IsFemale == false && characterObject.Occupation == Occupation.Wanderer);
                    MaleHero = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), null, null, CEHelper.HelperMBRandom(20) + 20);
                    MaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }
                else
                {
                    maleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                    MaleHero = maleHero;
                }

                if (femaleHero == null)
                {
                    CharacterObject m = Campaign.Current.Characters.GetRandomElementWithPredicate(characterObject => characterObject.Culture == CharacterObject.PlayerCharacter.Culture && characterObject.IsFemale && characterObject.Occupation == Occupation.Wanderer);
                    femaleHero = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), null, null, CEHelper.HelperMBRandom(20) + 20);
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

        public CESceneNotification(CharacterObject maleHero, CharacterObject femaleHero, string sceneID)
        {
            SceneID = sceneID;

            try
            {
                if (maleHero == null)
                {
                    CharacterObject m = Campaign.Current.Characters.GetRandomElementWithPredicate(characterObject => characterObject.Culture == CharacterObject.PlayerCharacter.Culture && characterObject.IsFemale == false && characterObject.Occupation == Occupation.Wanderer);
                    MaleHero = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), null, null, CEHelper.HelperMBRandom(20) + 20);
                    MaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }
                else
                {
                    Hero mh;
                    HeroCreator.CreateBasicHero(maleHero.StringId, maleHero, out mh);
                    MaleHero = mh;
                    MaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }

                if (femaleHero == null)
                {
                    CharacterObject m = Campaign.Current.Characters.GetRandomElementWithPredicate(characterObject => characterObject.Culture == CharacterObject.PlayerCharacter.Culture && characterObject.IsFemale && characterObject.Occupation == Occupation.Wanderer);
                    FemaleHero = HeroCreator.CreateSpecialHero(m, SettlementHelper.FindRandomSettlement(x => x.IsTown && x.Culture == m.Culture), null, null, CEHelper.HelperMBRandom(20) + 20);
                    FemaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }
                else
                {
                    Hero fh;
                    HeroCreator.CreateBasicHero(femaleHero.StringId, femaleHero, out fh);
                    FemaleHero = fh;
                    FemaleHero.CheckInvalidEquipmentsAndReplaceIfNeeded();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Invalid Scene CESceneNotification: " + e);
            }
        }
    }
}
