using System;
using System.Linq;
using CaptivityEvents.Custom;
using CaptivityEvents.Enums;
using CaptivityEvents.Events;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace CaptivityEvents.Brothel
{
    internal class CEBrothelCustomerConditions
    {
        internal readonly string[] CustomerStrings = {"customer_confident", "customer_tired"};
        internal readonly string[] Responses = {"{=CEBROTHEL1019}That's too much, no thanks.", "{=CEBROTHEL1049}Alright, here you go."};
        internal readonly string[] RageResponses = {"{=CEBROTHEL1065}Well perhaps you should, you sure look like a {?PLAYER.GENDER}whore{?}prostitute{\\?}!", "{=CEBROTHEL1066}My apologies, {?PLAYER.GENDER}milady{?}my lord{\\?}!"};
        internal CEBrothelOwnerConditions Owner { get; set; }

        public CEBrothelCustomerConditions(CEBrothelOwnerConditions owner)
        {
            Owner = owner;
        }


        internal bool ConversationWithCustomerNotMetRequirements()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId.StartsWith("customer") && (!Hero.MainHero.IsFemale || Campaign.Current.IsMainHeroDisguised);
        }

        internal bool ConversationWithConfidentCustomer()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "customer_confident";
        }

        internal bool ConversationWithTiredCustomer()
        {
            return CharacterObject.OneToOneConversationCharacter.StringId == "customer_tired";
        }

        internal void ConversationCustomerConsequenceSex()
        {
            try
            {
                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, Owner.ProstitutionCost);
                var prostitutionSkill = CESkills.Prostitution;
                if (Hero.MainHero.GetSkillValue(prostitutionSkill) < 100) Hero.MainHero.SetSkillValue(prostitutionSkill, 100);
                CEEventLoader.VictimProstitutionModifier(MBRandom.RandomInt(1, 10), Hero.MainHero, false, true, true);

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
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");

                        break;
                    case CultureCode.Khuzait:
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_b");

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
                        CESubModule.GameEntity = Mission.Current.Scene.GetFirstEntityWithName("bed_convolute_f");

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


        internal bool ConversationWithCustomerRandomResponse()
        {
            if (MBRandom.RandomInt(0, 100) > 20)
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(Responses[0]));
            }
            else
            {
                MBTextManager.SetTextVariable("RESPONSE_STRING", new TextObject(Responses[1]));
                ConversationCustomerConsequenceSex();
            }

            return true;
        }

        internal bool ConversationWithCustomerRandomResponseRage()
        {
            MBTextManager.SetTextVariable("RESPONSE_STRING", MBRandom.RandomInt(0, 100) > 40
                                              ? new TextObject(RageResponses[0])
                                              : new TextObject(RageResponses[1]));

            return true;
        }
    }
}