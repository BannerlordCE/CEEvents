using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelOwnerConditions
    {
        internal List<CEBrothel> Brothels;
        internal int ProstitutionCost;
        internal int BrothelCost;

        public CEBrothelOwnerConditions()
        {
            Brothels = new List<CEBrothel>();
            ProstitutionCost = 60;
            BrothelCost = 5000;
        }


        internal bool DoesOwnBrothelInSettlement(Settlement settlement)
        {
            return Brothels.Exists(brothelData => brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero);
        }

        internal bool ConversationWithBrothelAssistantAfterSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && !DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        internal bool ConversationWithBrothelOwnerAfterSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        internal bool ConversationWithBrothelAssistantBeforeSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant" && DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        internal bool ConversationWithBrothelOwnerBeforeSelling()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        internal bool ConversationWithBrothelOwnerShowBuy()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "brothel_owner" && !Campaign.Current.IsMainHeroDisguised;
        }

        internal void ConversationBoughtBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, BrothelCost);
            BrothelInteraction(Settlement.CurrentSettlement, true);
        }

        internal void ConversationSoldBrothel()
        {
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, BrothelCost);
            BrothelInteraction(Settlement.CurrentSettlement, false);
        }

        internal bool ConversationHasEnoughMoneyForBrothel(out TextObject text)
        {
            text = TextObject.Empty;

            if (Hero.MainHero.Gold >= BrothelCost) return true;
            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        internal void BrothelInteraction(Settlement settlement, bool flagToPurchase)
        {
            try
            {
                Brothels.Where(brothel => { return brothel.Settlement.StringId == settlement.StringId; }).Select(brothel =>
                                                                                                                 {
                                                                                                                     brothel.Owner = flagToPurchase
                                                                                                                         ? Hero.MainHero
                                                                                                                         : null;
                                                                                                                     brothel.Capital = brothel.InitialCapital;

                                                                                                                     return brothel;
                                                                                                                 }).ToList();
            }
            catch (Exception) { }
        }

        internal List<CEBrothel> BrothelInteractionTest(Settlement settlement, bool flagToPurchase)
        {
            var result = new List<CEBrothel>();

            foreach (var brothel in Brothels.Where(n => n.Settlement.StringId == settlement.StringId))
            {
                brothel.Owner = flagToPurchase
                    ? Hero.MainHero
                    : null;
                brothel.Capital = brothel.InitialCapital;
                result.Add(brothel);
            }

            return result;
        }

        internal bool PriceWithBrothel()
        {
            try
            {
                MBTextManager.SetTextVariable("AMOUNT", CharacterObject.OneToOneConversationCharacter.StringId == "brothel_assistant"
                                                  ? new TextObject(GetPlayerBrothel(Settlement.CurrentSettlement).Capital.ToString())
                                                  : new TextObject(BrothelCost.ToString()));
            }
            catch (Exception) { }

            return true;
        }

        internal CEBrothel GetPlayerBrothel(Settlement settlement)
        {
            return Brothels.FirstOrDefault(brothelData => brothelData.Settlement.StringId == settlement.StringId && brothelData.Owner == Hero.MainHero);
        }
    }
}