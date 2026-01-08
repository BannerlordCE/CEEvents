using CaptivityEvents.Config;
using CaptivityEvents.Custom;
using CaptivityEvents.Events;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace CaptivityEvents
{
    public static class CEDramalordIntegration
    {
        private static dynamic? _relationsInstance;
        private static dynamic? _personalitiesInstance;
        private static Type? _relationshipTypeEnum;

        static CEDramalordIntegration()
        {
            if (CESubModule.IsDramalordLoaded)
            {
                try
                {
                    Assembly dramalordAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Dramalord");
                    Type relationsType = dramalordAssembly.GetType("Dramalord.Data.DramalordRelations");
                    Type personalitiesType = dramalordAssembly.GetType("Dramalord.Data.DramalordPersonalities");
                    _relationsInstance = relationsType.GetProperty("Instance").GetValue(null);
                    _personalitiesInstance = personalitiesType.GetProperty("Instance").GetValue(null);
                    _relationshipTypeEnum = dramalordAssembly.GetType("Dramalord.Data.RelationshipType");
                }
                catch (Exception e)
                {
                    CECustomHandler.LogToFile("Failed to initialize Dramalord integration: " + e.Message);
                }
            }
        }

        public static void HandleCaptorEventConsequences(CEEvent ceEvent, Option option)
        {
            if (!CESubModule.IsDramalordLoaded || ceEvent.Captive == null || ceEvent.Captive.HeroObject == null || _relationsInstance == null) return;
            if (!(CESettingsIntegrations.Instance?.ActivateDramalord ?? true)) return;

            try
            {
                Hero captive = ceEvent.Captive.HeroObject;
                
                // For sexual interactions (impregnation by player)
                if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationByPlayer))
                {
                    // Get captive's personality to determine reaction
                    dynamic personality = _personalitiesInstance?.GetPersonality(captive);
                    
                    if (personality != null)
                    {
                        // Submissive personalities (low Neuroticism, high Agreeableness) develop stronger feelings
                        int loveChange = 5;
                        int neuroticism = (int)personality.Neuroticism;
                        int agreeableness = (int)personality.Agreeableness;
                        
                        if (neuroticism < -20 && agreeableness > 20)
                        {
                            loveChange = 15; // Submissive, accepts situation
                        }
                        else if (neuroticism > 20 || agreeableness < -20)
                        {
                            loveChange = -10; // Resistant personality
                        }
                        
                        IncreaseLove(Hero.MainHero, captive, loveChange);
                    }
                    else
                    {
                        IncreaseLove(Hero.MainHero, captive, 5);
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Dramalord integration error: " + e.Message);
            }
        }

        public static void HandleCaptiveEventConsequences(CEEvent ceEvent, Option option)
        {
            if (!CESubModule.IsDramalordLoaded || ceEvent.Captive == null || ceEvent.Captive.HeroObject == null || _relationsInstance == null) return;
            if (!(CESettingsIntegrations.Instance?.ActivateDramalord ?? true)) return;

            try
            {
                // For sexual interactions in captive events
                if (option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationByPlayer) ||
                    option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
                {
                    // In captive events, the captor is the one interacting
                    Hero captor = ceEvent.Captive?.HeroObject?.PartyBelongedTo?.LeaderHero ?? ceEvent.Captive?.HeroObject?.PartyBelongedToAsPrisoner?.LeaderHero;
                    if (captor != null && captor != Hero.MainHero)
                    {
                        // Get player's personality to determine reaction
                        dynamic personality = _personalitiesInstance?.GetPersonality(Hero.MainHero);
                        
                        if (personality != null)
                        {
                            // Stockholm syndrome effect based on personality
                            int loveChange = 0;
                            int neuroticism = (int)personality.Neuroticism;
                            int agreeableness = (int)personality.Agreeableness;
                            int conscientiousness = (int)personality.Conscientiousness;
                            
                            if (neuroticism < -20 && agreeableness > 20 && conscientiousness < 0)
                            {
                                loveChange = 10; // Submissive, develops feelings
                            }
                            else if (neuroticism > 20 || agreeableness < -20)
                            {
                                loveChange = -15; // Resistant personality
                            }
                            else
                            {
                                loveChange = 2; // Neutral response
                            }
                            
                            IncreaseLove(captor, Hero.MainHero, loveChange);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Dramalord integration error: " + e.Message);
            }
        }

        private static void IncreaseLove(Hero hero1, Hero hero2, int amount)
        {
            try
            {
                dynamic relation = _relationsInstance.GetRelation(hero1, hero2);
                relation.Love = Math.Min(100, (int)relation.Love + amount);
                relation.LastInteraction = CampaignTime.Now;
            }
            catch (Exception e)
            {
                CECustomHandler.LogToFile("Failed to increase love: " + e.Message);
            }
        }

        private static void SetAsFriend(Hero hero1, Hero hero2)
        {
        }
    }
}