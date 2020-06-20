using System;
using System.Linq;
using CaptivityEvents.Custom;
using CaptivityEvents.Enums;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelProstituteConditions
    {
        internal CEBrothelOwnerConditions Owner { get; set; }

        public readonly string[] ProstituteStrings = {"prostitute_confident", "prostitute_confident", "prostitute_tired"};


        public CEBrothelProstituteConditions(CEBrothelOwnerConditions owner)
        {
            Owner = owner;
        }


        private bool ConversationWithProstitute()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_regular";
        }

        internal bool ConversationWithProstituteIsOwner()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("prostitute") && Owner.DoesOwnBrothelInSettlement(Settlement.CurrentSettlement);
        }

        internal bool ConversationWithProstituteNotMetRequirements()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("prostitute") && Campaign.Current.IsMainHeroDisguised;
        }

        internal bool ConversationWithConfidentProstitute()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_confident";
        }

        internal bool ConversationWithTiredProstitute()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "prostitute_tired";
        }

        internal bool ConversationHasEnoughForService(out TextObject text)
        {
            text = TextObject.Empty;

            if (Hero.MainHero.Gold >= Owner.ProstitutionCost) return true;

            text = new TextObject("{=CEEVENTS1138}You don't have enough gold");

            return false;
        }

        internal bool PriceWithProstitute()
        {
            MBTextManager.SetTextVariable("AMOUNT", new TextObject(Owner.ProstitutionCost.ToString()));

            return true;
        }

        internal void ConversationProstituteConsequenceSex()
        {
            try
            {
                if (!Owner.DoesOwnBrothelInSettlement(Settlement.CurrentSettlement)) GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, null, Owner.ProstitutionCost);

                switch (Settlement.CurrentSettlement.Culture.GetCultureCode())
                {
                    case CultureCode.Sturgia:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_straw_a");

                        break;

                    case CultureCode.Vlandia:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_i");

                        break;

                    case CultureCode.Aserai:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_a");

                        break;

                    case CultureCode.Empire:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");

                        break;

                    case CultureCode.Battania:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_wodden_straw_a");

                        break;
                    case CultureCode.Khuzait:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_ground_f");

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
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_tavern_a");

                        break;
                }

                CESubModule.AgentTalkingTo = Mission.Current.Agents.FirstOrDefault(agent => agent.Character == CharacterObject.OneToOneConversationCharacter);
                CESubModule.brothelState = BrothelState.Start;
            }
            catch (Exception e)
            {
                CECustomHandler.LogMessage("Failed to launch ConversationProstituteConsequence : " + Hero.MainHero.CurrentSettlement.Culture + " : " + e);
            }
        }
    }
}