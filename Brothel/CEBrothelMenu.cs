using CaptivityEvents.Events;
using CaptivityEvents.Helper;
using CaptivityEvents.Models;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelMenu
    {
        private static Location _Brothel;

        public CEBrothelMenu(Location brothel)
        {
            _Brothel = brothel;
        }

        public bool CanGoToBrothelDistrictOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;

            return true;
        }

        public bool VisitBrothelOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Mission;

            return true;
        }


        public bool ProstitutionMenuJoinOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;

            if (!CEContext.brothelFlagFemale && Hero.MainHero.IsFemale || !CEContext.brothelFlagMale && !Hero.MainHero.IsFemale) return false;

            return !Campaign.Current.IsMainHeroDisguised;
        }

        public void ProstitutionMenuJoinOnConsequence(MenuCallbackArgs args)
        {
            var ProstitueFlag = CESkills.IsProstitute;
            Hero.MainHero.SetSkillValue(ProstitueFlag, 1);
            var ProstitutionSkill = CESkills.Prostitution;

            if (Hero.MainHero.GetSkillValue(ProstitutionSkill) < 100) Hero.MainHero.SetSkillValue(ProstitutionSkill, 100);
            var textObject = GameTexts.FindText("str_CE_join_prostitution");
            textObject.SetTextVariable("PLAYER_HERO", Hero.MainHero.Name);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Green));
            InformationManager.AddQuickInformation(textObject, 0, CharacterObject.PlayerCharacter, "event:/ui/notification/relation");

            CEPlayerCaptivityModel.CaptureOverride = true;
            var capturerParty = SettlementHelper.FindNearestSettlement(settlement => settlement.IsTown).Party;
            var prisonerCharacter = Hero.MainHero;
            prisonerCharacter.CaptivityStartTime = CampaignTime.Now;
            prisonerCharacter.ChangeState(Hero.CharacterStates.Prisoner);
            while (PartyBase.MainParty.MemberRoster.Contains(CharacterObject.PlayerCharacter)) PartyBase.MainParty.AddElementToMemberRoster(CharacterObject.PlayerCharacter, -1, true);
            capturerParty.AddPrisoner(prisonerCharacter.CharacterObject, 1);
            if (prisonerCharacter == Hero.MainHero) PlayerCaptivity.StartCaptivity(capturerParty);
            var test = CEEventLoader.CEWaitingList();
            GameMenu.ExitToLast();
            if (test != null) GameMenu.ActivateGameMenu(test);
        }


        // Back Condition
        internal bool BackOnCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;

            return true;
        }

        // Brothel District Menu
        private static bool CheckAndOpenNextLocation(MenuCallbackArgs args)
        {
            if (Campaign.Current.GameMenuManager.NextLocation == null || !(GameStateManager.Current.ActiveState is MapState)) return false;

            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, Campaign.Current.GameMenuManager.PreviousLocation);
            Campaign.Current.GameMenuManager.SetNextMenu("town_brothel");
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;

            return true;
        }

        // Brothel Menu
        internal void BrothelDistrictOnInit(MenuCallbackArgs args)
        {
            Campaign.Current.GameMenuManager.MenuLocations.Clear();
            var settlement = Settlement.CurrentSettlement ?? MobileParty.MainParty.CurrentSettlement;
            _Brothel.SetOwnerComplex(settlement.LocationComplex);

            switch (settlement.Culture.GetCultureCode())
            {
                case CultureCode.Sturgia:
                    _Brothel.SetSceneName(0, "sturgia_house_a_interior_tavern");

                    break;

                case CultureCode.Vlandia:
                    _Brothel.SetSceneName(0, "vlandia_tavern_interior_a");

                    break;

                case CultureCode.Aserai:
                    _Brothel.SetSceneName(0, "arabian_house_new_c_interior_b_tavern");

                    break;

                case CultureCode.Empire:
                    _Brothel.SetSceneName(0, "empire_house_c_tavern_a");

                    break;

                case CultureCode.Battania:
                    _Brothel.SetSceneName(0, "battania_tavern_interior_b");

                    break;

                case CultureCode.Khuzait:
                    _Brothel.SetSceneName(0, "khuzait_tavern_a");

                    break;

                case CultureCode.Invalid:
                    break;
                case CultureCode.Nord:
                    break;
                case CultureCode.Darshi:
                    break;
                case CultureCode.Vakken:
                    break;
                case CultureCode.AnyOtherCulture:
                    break;
                default:
                    _Brothel.SetSceneName(0, "empire_house_c_tavern_a");

                    break;
            }

            Campaign.Current.GameMenuManager.MenuLocations.Add(_Brothel);

            if (CheckAndOpenNextLocation(args)) return;
            args.MenuTitle = new TextObject("{=CEEVENTS1099}Brothel");
        }


        internal void VisitBrothelOnConsequence(MenuCallbackArgs args)
        {
            if (((TownEncounter) PlayerEncounter.LocationEncounter).IsAmbush)
            {
                GameMenu.ActivateGameMenu("menu_town_thugs_start");

                return;
            }

            Campaign.Current.GameMenuManager.NextLocation = _Brothel;
            Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
            PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation);
            Campaign.Current.GameMenuManager.NextLocation = null;
            Campaign.Current.GameMenuManager.PreviousLocation = null;
        }
    }
}