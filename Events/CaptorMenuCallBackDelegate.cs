using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityEvents.CampaignBehaviors;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Events
{
    public class CaptorMenuCallBackDelegate
    {
        private readonly CEEvent _listedEvent;
        private readonly List<CEEvent> _eventList;
        private readonly Option _option;

        internal CaptorMenuCallBackDelegate(CEEvent listedEvent)
        {
            _listedEvent = listedEvent;
        }

        internal CaptorMenuCallBackDelegate(CEEvent listedEvent, Option option, List<CEEvent> eventList)
        {
            _listedEvent = listedEvent;
            _option = option;
            _eventList = eventList;
        }

        internal void CaptorEventWaitGameMenu(MenuCallbackArgs args)
        {
            SetNames(ref args);

            new SharedCallBackHelper(_listedEvent, _option).LoadBackgroundImage("captor_default");
        }

        internal bool CaptorEventOptionGameMenu(MenuCallbackArgs args)
        {
            PlayerIsNotBusy(ref args);
            GiveCaptorGold();
            CaptorGoldTotal();
            LeaveTypes(ref args);
            ReqHeroCaptorRelation(ref args);
            ReqMorale(ref args);
            ReqTroops(ref args);
            ReqMaleTroops(ref args);
            ReqFemaleTroops(ref args);
            ReqCaptives(ref args);
            ReqMaleCaptives(ref args);
            ReqFemaleCaptives(ref args);
            ReqTrait(ref args);
            ReqCaptorTrait(ref args);
            ReqSkill(ref args);
            ReqCaptorSkill(ref args);
            ReqGold(ref args);

            return true;
        }

        internal void CaptorConsequenceWaitGameMenu(MenuCallbackArgs args)
        {
            Hero captiveHero = null;

            try
            {
                if (_listedEvent.Captive != null)
                {
                    if (_listedEvent.Captive.IsHero) captiveHero = _listedEvent.Captive.HeroObject;
                    MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive.Name);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Hero doesn't exist"); }

            CaptorGold(captiveHero);
            CaptorChangeGold();
            CaptorSkill();
            CaptorTrait();
            CaptorRenown();
            ChangeMorale();

            if (captiveHero != null)
            {
                LeaveSpouse(captiveHero);
                ForceMarry(captiveHero);
                ChangeClan(captiveHero);
                SlaveryFlags(captiveHero);
                SlaveryLevel(captiveHero);
                ProstitutionFlags(captiveHero);
                ProstitutionLevel(captiveHero);
                Relations(captiveHero);
                Gold(captiveHero);
                ChangeGold(captiveHero);
                Trait(captiveHero);
                Skill(captiveHero);
                Renown(captiveHero);
                ChangeHealth(captiveHero);
                Impregnation(captiveHero);
                Strip(captiveHero);
            }

            Escape();
            GainRandomPrisoners();
            KillPrisoner(ref args);

            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.StripHero) && captiveHero != null)
            {
                if (CESettings.Instance.EventCaptorGearCaptives) CECampaignBehavior.AddReturnEquipment(captiveHero, captiveHero.BattleEquipment, captiveHero.CivilianEquipment);
                InventoryManager.OpenScreenAsInventoryOf(Hero.MainHero.PartyBelongedTo.Party.MobileParty, captiveHero.CharacterObject);
            }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RebelPrisoners)) { new CaptorSpecifics().CEPrisonerRebel(args); }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.HuntPrisoners)) { new CaptorSpecifics().CEHuntPrisoners(args); }
            else if (_option.TriggerEvents != null && _option.TriggerEvents.Length > 0)
            {
                List<CEEvent> eventNames = new List<CEEvent>();

                try
                {
                    foreach (TriggerEvent triggerEvent in _option.TriggerEvents)
                    {
                        CEEvent triggeredEvent = _eventList.Find(item => item.Name == triggerEvent.EventName);

                        if (triggeredEvent == null)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + triggerEvent.EventName + " in events.");
                            InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + triggerEvent.EventName + " in events.", Colors.Red));           
                            continue;
                        }

                        if (!triggerEvent.EventUseConditions.IsStringNoneOrEmpty() && triggerEvent.EventUseConditions == "True")
                        {
                            string conditionMatched = null;
                            if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Captor))
                            {
                                conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(_listedEvent.Captive, PartyBase.MainParty);
                            }
                            else if (triggeredEvent.MultipleRestrictedListOfFlags.Contains(RestrictedListOfFlags.Random))
                            {
                                conditionMatched = new CEEventChecker(triggeredEvent).FlagsDoMatchEventConditions(CharacterObject.PlayerCharacter);
                            }

                            if (conditionMatched != null)
                            {
                                CECustomHandler.LogToFile(conditionMatched);
                                continue;
                            }
                        }

                        int weightedChance = 1;

                        try
                        {
                            weightedChance = new CEVariablesLoader().GetIntFromXML(!triggerEvent.EventWeight.IsStringNoneOrEmpty()
                                                                         ? triggerEvent.EventWeight
                                                                         : triggeredEvent.WeightedChanceOfOccuring);
                        }
                        catch (Exception) { CECustomHandler.LogToFile("Missing EventWeight"); }

                        for (int a = weightedChance; a > 0; a--) eventNames.Add(triggeredEvent);
                    }

                    if (eventNames.Count > 0)
                    {
                        int number = MBRandom.Random.Next(0, eventNames.Count - 1);

                        try
                        {
                            CEEvent triggeredEvent = eventNames[number];
                            triggeredEvent.Captive = _listedEvent.Captive;
                            GameMenu.ActivateGameMenu(triggeredEvent.Name);
                        }
                        catch (Exception)
                        {
                            CECustomHandler.ForceLogToFile("Couldn't find " + eventNames[number] + " in events.");
                            InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + eventNames[number] + " in events.", Colors.Red));
                            new CaptorSpecifics().CECaptorContinue(args);
                        }
                    }
                    else { new CaptorSpecifics().CECaptorContinue(args); }
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("MBRandom.Random in events Failed.");
                    new CaptorSpecifics().CECaptorContinue(args);
                }
            }
            else if (!string.IsNullOrEmpty(_option.TriggerEventName))
            {
                try
                {
                    CEEvent triggeredEvent = _eventList.Find(item => item.Name == _option.TriggerEventName);
                    triggeredEvent.Captive = _listedEvent.Captive;
                    GameMenu.SwitchToMenu(triggeredEvent.Name);
                }
                catch (Exception)
                {
                    CECustomHandler.ForceLogToFile("Couldn't find " + _option.TriggerEventName + " in events.");
                    InformationManager.DisplayMessage(new InformationMessage("Couldn't find " + _option.TriggerEventName + " in events.", Colors.Red));
                    new CaptorSpecifics().CECaptorContinue(args);
                }
            }
            else { new CaptorSpecifics().CECaptorContinue(args); }
        }


#region private

        
        private void KillPrisoner(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillPrisoner))
            {
                if (_listedEvent.Captive.IsHero) KillCharacterAction.ApplyByExecution(_listedEvent.Captive.HeroObject, Hero.MainHero);
                else PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
            }
            
            // Kill Player
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillCaptor)) new Dynamics().CEKillPlayer(_listedEvent.Captive.HeroObject);
            // Kill All
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillAllPrisoners)) new CaptorSpecifics().CEKillPrisoners(args, PartyBase.MainParty.PrisonRoster.Count(), true);
            // Kill Random
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.KillRandomPrisoners)) new CaptorSpecifics().CEKillPrisoners(args);
        }

        private void GainRandomPrisoners()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GainRandomPrisoners)) new Dynamics().CEGainRandomPrisoners(PartyBase.MainParty);
        }

        private void Escape()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Escape)) return;

            try
            {
                if (_listedEvent.Captive.IsHero) EndCaptivityAction.ApplyByEscape(_listedEvent.Captive.HeroObject);
                else PartyBase.MainParty.PrisonRoster.AddToCounts(_listedEvent.Captive, -1);
            } 
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Failure of Captor Escape: " + e.ToString());
            }
        }

        private void Strip(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Strip)) new CaptorSpecifics().CEStripVictim(captiveHero);
        }

        private void Impregnation(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationHero))
                try
                {
                    new ImpregnationSystem().CaptivityImpregnationChance(captiveHero, !string.IsNullOrEmpty(_option.PregnancyRiskModifier)
                                                      ? new CEVariablesLoader().GetIntFromXML(_option.PregnancyRiskModifier)
                                                      : new CEVariablesLoader().GetIntFromXML(_listedEvent.PregnancyRiskModifier));
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    new ImpregnationSystem().CaptivityImpregnationChance(captiveHero, 30);
                }
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ImpregnationRisk))
                try
                {
                    new ImpregnationSystem().CaptivityImpregnationChance(captiveHero, !string.IsNullOrEmpty(_option.PregnancyRiskModifier)
                                                      ? new CEVariablesLoader().GetIntFromXML(_option.PregnancyRiskModifier)
                                                      : new CEVariablesLoader().GetIntFromXML(_listedEvent.PregnancyRiskModifier), false, false);
                }
                catch (Exception)
                {
                    CECustomHandler.LogToFile("Missing PregnancyRiskModifier");
                    new ImpregnationSystem().CaptivityImpregnationChance(captiveHero, 30, false, false);
                }
        }

        private void ChangeHealth(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth)) return;

            try
            {
                captiveHero.HitPoints += !string.IsNullOrEmpty(_option.HealthTotal)
                    ? new CEVariablesLoader().GetIntFromXML(_option.HealthTotal)
                    : new CEVariablesLoader().GetIntFromXML(_listedEvent.HealthTotal);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing HealthTotal");
                captiveHero.HitPoints += MBRandom.RandomInt(-20, 20);
            }
        }

        private void Renown(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown)) return;

            try
            {
                new Dynamics().RenownModifier(!string.IsNullOrEmpty(_option.RenownTotal)
                                                  ? new CEVariablesLoader().GetIntFromXML(_option.RenownTotal)
                                                  : new CEVariablesLoader().GetIntFromXML(_listedEvent.RenownTotal), captiveHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RenownTotal");
                new Dynamics().RenownModifier(MBRandom.RandomInt(-5, 5), captiveHero);
            }
        }

        private void Skill(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill)) return;

            try
            {
                int level = 0;

                if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_option.SkillTotal);
                else if (!_listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillTotal);
                else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                if (!_option.SkillToLevel.IsStringNoneOrEmpty()) new Dynamics().SkillModifier(captiveHero, _option.SkillToLevel, level);
                else if (!_listedEvent.SkillToLevel.IsStringNoneOrEmpty()) new Dynamics().SkillModifier(captiveHero, _listedEvent.SkillToLevel, level);
                else CECustomHandler.LogToFile("Missing SkillToLevel");
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        private void Trait(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait)) return;

            try
            {
                int level = new CEVariablesLoader().GetIntFromXML(!string.IsNullOrEmpty(_option.TraitTotal)
                                                        ? _option.TraitTotal
                                                        : _listedEvent.TraitTotal);

                new Dynamics().TraitModifier(captiveHero, !string.IsNullOrEmpty(_option.TraitToLevel)
                                           ? _option.TraitToLevel
                                           : _listedEvent.TraitToLevel, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing Trait Flags"); }
        }

        private void ChangeGold(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        private void Gold(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;

            int content = new ScoresCalculation().AttractivenessScore(captiveHero);
            int currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            GiveGoldAction.ApplyBetweenCharacters(null, captiveHero, content);
        }

        private void Relations(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRelation)) return;

            try
            {
                new Dynamics().RelationsModifier(captiveHero, !string.IsNullOrEmpty(_option.RelationTotal)
                                               ? new CEVariablesLoader().GetIntFromXML(_option.RelationTotal)
                                               : new CEVariablesLoader().GetIntFromXML(_listedEvent.RelationTotal));
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RelationTotal");
                new Dynamics().RelationsModifier(captiveHero, MBRandom.RandomInt(-5, 5));
            }
        }

        private void ProstitutionLevel(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel)) return;

            try
            {
                new Dynamics().VictimProstitutionModifier(!string.IsNullOrEmpty(_option.ProstitutionTotal)
                                                        ? new CEVariablesLoader().GetIntFromXML(_option.ProstitutionTotal)
                                                        : new CEVariablesLoader().GetIntFromXML(_listedEvent.ProstitutionTotal), captiveHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing ProstitutionTotal");
                new Dynamics().VictimProstitutionModifier(1, captiveHero);
            }
        }

        private void ProstitutionFlags(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) new Dynamics().VictimProstitutionModifier(1, captiveHero, true, false, true);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) new Dynamics().VictimProstitutionModifier(0, captiveHero, true, false, true);
        }

        private void SlaveryLevel(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel)) return;

            try
            {
                new Dynamics().VictimSlaveryModifier(!string.IsNullOrEmpty(_option.SlaveryTotal)
                                                   ? new CEVariablesLoader().GetIntFromXML(_option.SlaveryTotal)
                                                   : new CEVariablesLoader().GetIntFromXML(_listedEvent.SlaveryTotal), captiveHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing SlaveryTotal");
                new Dynamics().VictimSlaveryModifier(1, captiveHero);
            }
        }

        private void SlaveryFlags(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) new Dynamics().VictimSlaveryModifier(1, captiveHero, true, false, true);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) new Dynamics().VictimSlaveryModifier(0, captiveHero, true, false, true);
        }

        private void ChangeClan(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeClan)) new Dynamics().ChangeClan(captiveHero, Hero.MainHero);
        }

        private void ForceMarry(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveMarryCaptor)) new Dynamics().ChangeSpouse(captiveHero, Hero.MainHero);
        }

        private void LeaveSpouse(Hero captiveHero)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) new Dynamics().ChangeSpouse(captiveHero, null);
        }

        private void ChangeMorale()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            try
            {
                new Dynamics().MoralChange(!string.IsNullOrEmpty(_option.MoraleTotal)
                                         ? new CEVariablesLoader().GetIntFromXML(_option.MoraleTotal)
                                         : new CEVariablesLoader().GetIntFromXML(_listedEvent.MoraleTotal), PartyBase.MainParty);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing MoralTotal");
                new Dynamics().MoralChange(MBRandom.RandomInt(-5, 5), PlayerCaptivity.CaptorParty);
            }
        }

        private void CaptorRenown()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorRenown)) return;

            try
            {
                new Dynamics().RenownModifier(!string.IsNullOrEmpty(_option.RenownTotal)
                                            ? new CEVariablesLoader().GetIntFromXML(_option.RenownTotal)
                                            : new CEVariablesLoader().GetIntFromXML(_listedEvent.RenownTotal), Hero.MainHero);
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Missing RenownTotal");
                new Dynamics().RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
            }
        }

        private void CaptorTrait()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorTrait)) return;

            try
            {
                int level = new CEVariablesLoader().GetIntFromXML(!string.IsNullOrEmpty(_option.TraitTotal)
                                                                    ? _option.TraitTotal
                                                                    : _listedEvent.TraitTotal);

                new Dynamics().TraitModifier(Hero.MainHero, !string.IsNullOrEmpty(_option.TraitToLevel)
                                           ? _option.TraitToLevel
                                           : _listedEvent.TraitToLevel, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing Trait Flags"); }
        }

        private void CaptorSkill()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorSkill)) return;

            try
            {
                int level = 0;

                if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_option.SkillTotal);
                else if (!_listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillTotal);
                else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                if (!_option.SkillToLevel.IsStringNoneOrEmpty()) new Dynamics().SkillModifier(Hero.MainHero, _option.SkillToLevel, level);
                else if (!_listedEvent.SkillToLevel.IsStringNoneOrEmpty()) new Dynamics().SkillModifier(Hero.MainHero, _listedEvent.SkillToLevel, level);
                else CECustomHandler.LogToFile("Missing SkillToLevel");
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        private void CaptorChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.CaptorGoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.CaptorGoldTotal);
                else CECustomHandler.LogToFile("Missing CaptorGoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }
        }

        private void CaptorGold(Hero captiveHero)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold)) return;
            int content = new ScoresCalculation().AttractivenessScore(captiveHero);
            int currentValue = captiveHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
        }

        private void ReqGold(ref MenuCallbackArgs args)
        {
            try
            {
                ReqGoldAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldAbove / Failed "); }

            try
            {
                ReqGoldBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqGoldBelow / Failed "); }
        }

        private void ReqGoldBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrEmpty(_option.ReqGoldBelow)) return;
            if (Hero.MainHero.Gold <= new CEVariablesLoader().GetIntFromXML(_option.ReqGoldBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "high");
            args.IsEnabled = false;
        }

        private void ReqGoldAbove(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrEmpty(_option.ReqGoldAbove)) return;
            if (Hero.MainHero.Gold >= new CEVariablesLoader().GetIntFromXML(_option.ReqGoldAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_gold_level", "low");
            args.IsEnabled = false;
        }

        private void ReqCaptorSkill(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptorSkill.IsStringNoneOrEmpty()) return;

            int skillLevel = ReqCaptorSkill();

            try
            {
                ReqCaptorSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelAbove"); }

            try
            {
                ReqCaptorSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorSkillLevelBelow"); }
        }

        private void ReqCaptorSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (!_option.ReqCaptorSkillLevelBelow.IsStringNoneOrEmpty()) return;
            if (skillLevel <= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptorSkillLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "high");
            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (_option.ReqCaptorSkillLevelAbove.IsStringNoneOrEmpty()) return;
            if (skillLevel >= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptorSkillLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_level", "low");
            text.SetTextVariable("SKILL", _option.ReqCaptorSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int ReqCaptorSkill()
        {
            int skillLevel = 0;

            try { skillLevel = Hero.MainHero.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqCaptorSkill)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill Captor");
                skillLevel = 0;
            }

            return skillLevel;
        }

        private void ReqSkill(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroSkill.IsStringNoneOrEmpty()) return;

            int skillLevel = ReqHeroSkill();

            try
            {
                ReqHeroSkillLevelAbove(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelAbove"); }

            try
            {
                ReqHeroSkillLevelBelow(ref args, skillLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroSkillLevelBelow"); }
        }

        private void ReqHeroSkillLevelBelow(ref MenuCallbackArgs args, int skillLevel)
        {
            if (_option.ReqHeroSkillLevelBelow.IsStringNoneOrEmpty()) return;
            if (skillLevel <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroSkillLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_captive_level", "high");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroSkillLevelAbove(ref MenuCallbackArgs args, int skillLevel)
        {
            if (_option.ReqHeroSkillLevelAbove.IsStringNoneOrEmpty()) return;
            if (skillLevel >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroSkillLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_skill_captive_level", "low");
            text.SetTextVariable("SKILL", _option.ReqHeroSkill);
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int ReqHeroSkill()
        {
            int skillLevel = 0;

            try { skillLevel = _listedEvent.Captive.GetSkillValue(SkillObject.FindFirst(skill => skill.StringId == _option.ReqHeroSkill)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Skill Captive");
                skillLevel = 0;
            }

            return skillLevel;
        }

        private void ReqCaptorTrait(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptorTrait.IsStringNoneOrEmpty()) return;

            int traitLevel = SetCaptorTraitLevel();

            try
            {
                ReqCaptorTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelAbove"); }

            try
            {
                ReqCaptorTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing ReqCaptorTraitLevelBelow"); }
        }

        private void ReqCaptorTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrEmpty(_option.ReqCaptorTraitLevelBelow)) return;
            if (traitLevel <= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptorTraitLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqCaptorTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrEmpty(_option.ReqCaptorTraitLevelAbove)) return;
            if (traitLevel >= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptorTraitLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqCaptorTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int SetCaptorTraitLevel()
        {
            int traitLevel;

            try { traitLevel = Hero.MainHero.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captor");
                traitLevel = 0;
            }

            return traitLevel;
        }

        private void ReqTrait(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroTrait.IsStringNoneOrEmpty()) return;

            int traitLevel = SetCaptiveTraitLevel();

            try
            {
                ReqHeroTraitLevelAbove(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelAbove"); }

            try
            {
                ReqHeroTraitLevelBelow(ref args, traitLevel);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ReqHeroTraitLevelBelow"); }
        }

        private void ReqHeroTraitLevelBelow(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrEmpty(_option.ReqHeroTraitLevelBelow)) return;
            if (traitLevel <= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTraitLevelBelow)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_captive_level", "high");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private void ReqHeroTraitLevelAbove(ref MenuCallbackArgs args, int traitLevel)
        {
            if (string.IsNullOrEmpty(_option.ReqHeroTraitLevelAbove)) return;
            if (traitLevel >= new CEVariablesLoader().GetIntFromXML(_option.ReqHeroTraitLevelAbove)) return;

            TextObject text = GameTexts.FindText("str_CE_trait_captive_level", "low");
            text.SetTextVariable("TRAIT", CEStrings.FetchTraitString(_option.ReqHeroTrait));
            args.Tooltip = text;
            args.IsEnabled = false;
        }

        private int SetCaptiveTraitLevel()
        {
            int traitLevel;

            try { traitLevel = _listedEvent.Captive.GetTraitLevel(TraitObject.Find(_option.ReqCaptorTrait)); }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Invalid Trait Captive");
                traitLevel = 0;
            }

            return traitLevel;
        }

        private void ReqFemaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                ReqFemaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesAbove / Failed "); }

            try
            {
                ReqFemaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleCaptivesBelow / Failed "); }
        }

        private void ReqFemaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqFemaleCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) <= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqFemaleCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) >= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                ReqMaleCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesAbove / Failed "); }

            try
            {
                ReqMaleCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleCaptivesBelow / Failed "); }
        }

        private void ReqMaleCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqMaleCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqMaleCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.PrisonRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqCaptives(ref MenuCallbackArgs args)
        {
            try
            {
                ReqCaptivesAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesAbove / Failed "); }

            try
            {
                ReqCaptivesBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqCaptivesBelow / Failed "); }
        }

        private void ReqCaptivesBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptivesBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.NumberOfPrisoners <= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptivesBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "high");
            args.IsEnabled = false;
        }

        private void ReqCaptivesAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqCaptivesAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.NumberOfPrisoners >= new CEVariablesLoader().GetIntFromXML(_option.ReqCaptivesAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_captives_level", "low");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                ReqFemaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsAbove / Failed "); }

            try
            {
                ReqFemaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqFemaleTroopsBelow / Failed "); }
        }

        private void ReqFemaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqFemaleTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) <= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqFemaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqFemaleTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) >= new CEVariablesLoader().GetIntFromXML(_option.ReqFemaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqMaleTroops(ref MenuCallbackArgs args)
        {
            try
            {
                ReqMaleTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsAbove / Failed "); }

            try
            {
                ReqMaleTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMaleTroopsBelow / Failed "); }
        }

        private void ReqMaleTroopsBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqMaleTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMaleTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqMaleTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.MemberRoster.Sum(troopRosterElement => { return (!troopRosterElement.Character.IsFemale) ? troopRosterElement.Number : 0; } ) >= new CEVariablesLoader().GetIntFromXML(_option.ReqMaleTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqTroops(ref MenuCallbackArgs args)
        {
            try
            {
                ReqTroopsAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsAbove / Failed "); }

            try
            {
                ReqTroopsBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqTroopsBelow / Failed "); }
        }

        private void ReqTroopsBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqTroopsBelow.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.NumberOfHealthyMembers <= new CEVariablesLoader().GetIntFromXML(_option.ReqTroopsBelow)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "high");
            args.IsEnabled = false;
        }

        private void ReqTroopsAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqTroopsAbove.IsStringNoneOrEmpty()) return;
            if (PartyBase.MainParty.NumberOfHealthyMembers >= new CEVariablesLoader().GetIntFromXML(_option.ReqTroopsAbove)) return;

            args.Tooltip = GameTexts.FindText("str_CE_member_level", "low");
            args.IsEnabled = false;
        }

        private void ReqMorale(ref MenuCallbackArgs args)
        {
            try
            {
                ReqMoraleAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleAbove / Failed "); }

            try
            {
                ReqMoraleBelow(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqMoraleBelow / Failed "); }
        }

        private void ReqMoraleBelow(ref MenuCallbackArgs args)
        {
            if (_option.ReqMoraleBelow.IsStringNoneOrEmpty()) return;
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale > new CEVariablesLoader().GetIntFromXML(_option.ReqMoraleBelow))) return;

            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "high");
            args.IsEnabled = false;
        }

        private void ReqMoraleAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqMoraleAbove.IsStringNoneOrEmpty()) return;
            if (!PartyBase.MainParty.IsMobile || !(PartyBase.MainParty.MobileParty.Morale < new CEVariablesLoader().GetIntFromXML(_option.ReqMoraleAbove))) return;

            args.Tooltip = GameTexts.FindText("str_CE_morale_level", "low");
            args.IsEnabled = false;
        }

        private void LeaveTypes(ref MenuCallbackArgs args)
        {
            Wait(ref args);
            Trade(ref args);
            RansomAndBribe(ref args);
            Submenu(ref args);
            Continue(ref args);
            Default(ref args);
        }

        private void Default(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.EmptyIcon)) args.optionLeaveType = GameMenuOption.LeaveType.Default;
        }

        private void Continue(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Continue)) args.optionLeaveType = GameMenuOption.LeaveType.Continue;
        }

        private void Submenu(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Submenu)) args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
        }

        private void RansomAndBribe(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RansomAndBribe)) args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
        }

        private void Trade(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Trade)) args.optionLeaveType = GameMenuOption.LeaveType.Trade;
        }

        private void Wait(ref MenuCallbackArgs args)
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.Wait)) args.optionLeaveType = GameMenuOption.LeaveType.Wait;
        }

        private void CaptorGoldTotal()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeCaptorGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.CaptorGoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.CaptorGoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.CaptorGoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.CaptorGoldTotal);
                else CECustomHandler.LogToFile("Missing CaptorGoldTotal");
                MBTextManager.SetTextVariable("CAPTOR_MONEY_AMOUNT", level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid CaptorGoldTotal"); }
        }

        private void GiveCaptorGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveCaptorGold)) return;

            int content = new ScoresCalculation().AttractivenessScore(_listedEvent.Captive.HeroObject);
            if (_listedEvent.Captive.HeroObject != null) content += _listedEvent.Captive.HeroObject.GetSkillValue(CESkills.Prostitution) / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveCaptorGold);
            MBTextManager.SetTextVariable("MONEY_AMOUNT", content);
        }

        private void PlayerIsNotBusy(ref MenuCallbackArgs args)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.PlayerIsNotBusy)) return;
            if (PlayerEncounter.Current == null) return;

            args.Tooltip = GameTexts.FindText("str_CE_busy_right_now");
            args.IsEnabled = false;
        }

        private void SetNames(ref MenuCallbackArgs args)
        {
            try
            {
                if (_listedEvent.Captive != null)
                    //Hero captiveHero = null;
                    //if (_listedEvent.Captive.IsHero) captiveHero = _listedEvent.Captive.HeroObject; //WARNING: captiveHero never used
                    MBTextManager.SetTextVariable("CAPTIVE_NAME", _listedEvent.Captive.Name);
            }
            catch (Exception) { CECustomHandler.LogToFile("Hero doesn't exist"); }

            TextObject text = args.MenuContext.GameMenu.GetText();
            if (MobileParty.MainParty.CurrentSettlement != null) text.SetTextVariable("SETTLEMENT_NAME", MobileParty.MainParty.CurrentSettlement.Name);
            text.SetTextVariable("PARTY_NAME", MobileParty.MainParty.Name);
            text.SetTextVariable("CAPTOR_NAME", Hero.MainHero.Name);

            args.MenuContext.SetBackgroundMeshName(Hero.MainHero.IsFemale
                                                       ? "wait_prisoner_female"
                                                       : "wait_prisoner_male");
        }

        private void ReqHeroCaptorRelation(ref MenuCallbackArgs args)
        {
            if (_listedEvent.Captive.HeroObject == null) return;

            try
            {
                ReqHeroCaptorRelationAbove(ref args);
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationAbove / Failed "); }

            try
            {
                if (ReqHeroCaptorRelationBelow(ref args)) return;
            }
            catch (Exception) { CECustomHandler.LogToFile("Incorrect ReqHeroCaptorRelationBelow / Failed "); }
        }

        private bool ReqHeroCaptorRelationBelow(ref MenuCallbackArgs args)
        {
            if (string.IsNullOrEmpty(_option.ReqHeroCaptorRelationBelow)) return true;

            if (!(_listedEvent.Captive.HeroObject.GetRelationWithPlayer() > new CEVariablesLoader().GetFloatFromXML(_option.ReqHeroCaptorRelationBelow))) return false;
            TextObject textResponse3 = GameTexts.FindText("str_CE_relationship", "high");
            textResponse3.SetTextVariable("HERO", _listedEvent.Captive.HeroObject.Name.ToString());
            args.Tooltip = textResponse3;
            args.IsEnabled = false;

            return false;
        }

        private void ReqHeroCaptorRelationAbove(ref MenuCallbackArgs args)
        {
            if (_option.ReqHeroCaptorRelationAbove.IsStringNoneOrEmpty()) return;
            if (!(_listedEvent.Captive.HeroObject.GetRelationWithPlayer() < new CEVariablesLoader().GetFloatFromXML(_option.ReqHeroCaptorRelationAbove))) return;

            TextObject textResponse4 = GameTexts.FindText("str_CE_relationship", "low");
            textResponse4.SetTextVariable("HERO", _listedEvent.Captive.HeroObject.Name.ToString());
            args.Tooltip = textResponse4;
            args.IsEnabled = false;
        }

#endregion
    }
}