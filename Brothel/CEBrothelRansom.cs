using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelRansom
    {
        internal bool SellPrisonersCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count <= 0) return false;

            var ransomValueOfAllPrisoners = GetRansomValueOfAllPrisoners();
            MBTextManager.SetTextVariable("RANSOM_AMOUNT", ransomValueOfAllPrisoners);
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            return true;
        }

        internal void SellAllPrisoners()
        {
            SellPrisonersAction.ApplyForAllPrisoners(MobileParty.MainParty, MobileParty.MainParty.PrisonRoster, Settlement.CurrentSettlement);
            GameMenu.SwitchToMenu("town_brothel");
        }

        internal void ChooseRansomPrisoners()
        {
            GameMenu.SwitchToMenu("town_brothel");
            PartyScreenManager.OpenScreenAsRansom();
        }

        internal bool SellPrisonerOneStackOnCondition(MenuCallbackArgs args)
        {
            if (PartyBase.MainParty.PrisonRoster.Count <= 0) return false;

            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            return true;
        }

        private int GetRansomValueOfAllPrisoners()
        {
            return Enumerable.Sum(PartyBase.MainParty.PrisonRoster, troopRosterElement => troopRosterElement.Character.PrisonerRansomValue(Hero.MainHero) * troopRosterElement.Number);
        }
    }
}