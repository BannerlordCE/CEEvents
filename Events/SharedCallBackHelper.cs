using System;
using System.Linq;
using CaptivityEvents.Custom;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CaptivityEvents.Events
{
    public class SharedCallBackHelper
    {
        private readonly CEEvent _listedEvent;
        private readonly Option _option;

        private readonly Dynamics _dynamics = new Dynamics();
        private readonly ScoresCalculation _score = new ScoresCalculation();

        public SharedCallBackHelper(CEEvent listedEvent, Option option)
        {
            _listedEvent = listedEvent;
            _option = option;
        }


        #region private

        internal void ConsequenceXP()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveXP)) GiveXP();
        }

        internal void ConsequenceLeaveSpouse()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.CaptiveLeaveSpouse)) _dynamics.ChangeSpouse(Hero.MainHero, null);
        }

        internal void GiveXP()
        {
            try
            {
                string skillToLevel = "";

                if (!string.IsNullOrEmpty(_option.SkillToLevel)) skillToLevel = _option.SkillToLevel;
                else if (!string.IsNullOrEmpty(_listedEvent.SkillToLevel)) skillToLevel = _listedEvent.SkillToLevel;
                else CECustomHandler.LogToFile("Missing SkillToLevel");

                foreach (SkillObject skillObject in SkillObject.All.Where(skillObject => skillObject.Name.ToString().Equals(skillToLevel, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skillToLevel)) _dynamics.GainSkills(skillObject, 50, 100);
            }
            catch (Exception) { CECustomHandler.LogToFile("GiveXP Failed"); }
        }

        internal void ConsequenceGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.GiveGold)) return;

            int content = _score.AttractivenessScore(Hero.MainHero);
            int currentValue = Hero.MainHero.GetSkillValue(CESkills.Prostitution);
            content += currentValue / 2;
            content *= _option.MultipleRestrictedListOfConsequences.Count(consequence => consequence == RestrictedListOfConsequences.GiveGold);
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, content);
        }

        internal void ConsequenceChangeGold()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeGold)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.GoldTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.GoldTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.GoldTotal);
                else CECustomHandler.LogToFile("Missing GoldTotal");

                GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, level);
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid GoldTotal"); }
        }

        internal void ConsequenceChangeTrait()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeTrait)) return;

            try
            {
                int level = 0;

                if (!string.IsNullOrEmpty(_option.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_option.TraitTotal);
                else if (!string.IsNullOrEmpty(_listedEvent.TraitTotal)) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.TraitTotal);
                else CECustomHandler.LogToFile("Missing Trait TraitTotal");

                if (!string.IsNullOrEmpty(_option.TraitToLevel)) _dynamics.TraitModifier(Hero.MainHero, _option.TraitToLevel, level);
                else if (!string.IsNullOrEmpty(_listedEvent.TraitToLevel)) _dynamics.TraitModifier(Hero.MainHero, _listedEvent.TraitToLevel, level);
                else CECustomHandler.LogToFile("Missing TraitToLevel");
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Trait Flags"); }
        }


        internal void ConsequenceChangeSkill()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSkill)) return;

            try
            {
                int level = 0;

                if (!_option.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_option.SkillTotal);
                else if (!_listedEvent.SkillTotal.IsStringNoneOrEmpty()) level = new CEVariablesLoader().GetIntFromXML(_listedEvent.SkillTotal);
                else CECustomHandler.LogToFile("Missing Skill SkillTotal");

                if (!_option.SkillToLevel.IsStringNoneOrEmpty()) _dynamics.SkillModifier(Hero.MainHero, _option.SkillToLevel, level);
                else if (!_listedEvent.SkillToLevel.IsStringNoneOrEmpty()) _dynamics.SkillModifier(Hero.MainHero, _listedEvent.SkillToLevel, level);
                else CECustomHandler.LogToFile("Missing SkillToLevel");
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid Skill Flags"); }
        }

        internal void ConsequenceSlaveryLevel()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeSlaveryLevel)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.SlaveryTotal)) { _dynamics.VictimSlaveryModifier(new CEVariablesLoader().GetIntFromXML(_option.SlaveryTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(_listedEvent.SlaveryTotal)) { _dynamics.VictimSlaveryModifier(new CEVariablesLoader().GetIntFromXML(_listedEvent.SlaveryTotal), Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing SlaveryTotal");
                    _dynamics.VictimSlaveryModifier(1, Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid SlaveryTotal"); }
        }


        internal void ConsequenceSlaveryFlags()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddSlaveryFlag)) _dynamics.VictimSlaveryModifier(1, Hero.MainHero, true, false, true);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveSlaveryFlag)) _dynamics.VictimSlaveryModifier(0, Hero.MainHero, true, false, true);
        }

        internal void ConsequenceProstitutionLevel()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeProstitutionLevel)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.ProstitutionTotal)) { _dynamics.VictimProstitutionModifier(new CEVariablesLoader().GetIntFromXML(_option.ProstitutionTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(_listedEvent.ProstitutionTotal)) { _dynamics.VictimProstitutionModifier(new CEVariablesLoader().GetIntFromXML(_listedEvent.ProstitutionTotal), Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing ProstitutionTotal");
                    _dynamics.VictimProstitutionModifier(1, Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid ProstitutionTotal"); }
        }

        internal void ConsequenceProstitutionFlags()
        {
            if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.AddProstitutionFlag)) _dynamics.VictimProstitutionModifier(1, Hero.MainHero, true, false, true);
            else if (_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.RemoveProstitutionFlag)) _dynamics.VictimProstitutionModifier(0, Hero.MainHero, true, false, true);
        }

        internal void ConsequenceRenown()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeRenown)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.RenownTotal)) { _dynamics.RenownModifier(new CEVariablesLoader().GetIntFromXML(_option.RenownTotal), Hero.MainHero); }
                else if (!string.IsNullOrEmpty(_listedEvent.RenownTotal)) { _dynamics.RenownModifier(new CEVariablesLoader().GetIntFromXML(_listedEvent.RenownTotal), Hero.MainHero); }
                else
                {
                    CECustomHandler.LogToFile("Missing RenownTotal");
                    _dynamics.RenownModifier(MBRandom.RandomInt(-5, 5), Hero.MainHero);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid RenownTotal"); }
        }

        internal void ConsequenceChangeHealth()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeHealth)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.HealthTotal)) { Hero.MainHero.HitPoints += new CEVariablesLoader().GetIntFromXML(_option.HealthTotal); }
                else if (!string.IsNullOrEmpty(_listedEvent.HealthTotal)) { Hero.MainHero.HitPoints += new CEVariablesLoader().GetIntFromXML(_listedEvent.HealthTotal); }
                else
                {
                    CECustomHandler.LogToFile("Invalid HealthTotal");
                    Hero.MainHero.HitPoints += MBRandom.RandomInt(-20, 20);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Missing HealthTotal"); }
        }


        internal void ConsequenceChangeMorale()
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            PartyBase party = PlayerCaptivity.IsCaptive
                ? PlayerCaptivity.CaptorParty //captive         
                : PartyBase.MainParty; //random, captor

            try
            {
                if (!string.IsNullOrEmpty(_option.MoraleTotal)) { _dynamics.MoralChange(new CEVariablesLoader().GetIntFromXML(_option.MoraleTotal), party); }
                else if (!string.IsNullOrEmpty(_listedEvent.MoraleTotal)) { _dynamics.MoralChange(new CEVariablesLoader().GetIntFromXML(_listedEvent.MoraleTotal), party); }
                else
                {
                    CECustomHandler.LogToFile("Missing MoralTotal");
                    _dynamics.MoralChange(MBRandom.RandomInt(-5, 5), party);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }
        }

        // TODO: Not being used anywhere
        internal void ConsequenceChangeMorale(PartyBase party)
        {
            if (!_option.MultipleRestrictedListOfConsequences.Contains(RestrictedListOfConsequences.ChangeMorale)) return;

            try
            {
                if (!string.IsNullOrEmpty(_option.MoraleTotal)) { _dynamics.MoralChange(new CEVariablesLoader().GetIntFromXML(_option.MoraleTotal), party); }
                else if (!string.IsNullOrEmpty(_listedEvent.MoraleTotal)) { _dynamics.MoralChange(new CEVariablesLoader().GetIntFromXML(_listedEvent.MoraleTotal), party); }
                else
                {
                    CECustomHandler.LogToFile("Missing MoralTotal");
                    _dynamics.MoralChange(MBRandom.RandomInt(-5, 5), party);
                }
            }
            catch (Exception) { CECustomHandler.LogToFile("Invalid MoralTotal"); }
        }


        internal void LoadBackgroundImage(string textureFlag = "default_random")
        {
            try
            {
                string backgroundName = _listedEvent.BackgroundName;

                if (!backgroundName.IsStringNoneOrEmpty())
                {
                    CEPersistence.animationPlayEvent = false;
                    new CESubModule().LoadTexture(backgroundName);
                }
                else if (_listedEvent.BackgroundAnimation != null && _listedEvent.BackgroundAnimation.Count > 0)
                {
                    CEPersistence.animationImageList = _listedEvent.BackgroundAnimation;
                    CEPersistence.animationIndex = 0;
                    CEPersistence.animationPlayEvent = true;
                    float speed = 0.03f;

                    try
                    {
                        if (!_listedEvent.BackgroundAnimationSpeed.IsStringNoneOrEmpty()) speed = new CEVariablesLoader().GetFloatFromXML(_listedEvent.BackgroundAnimationSpeed);
                    }
                    catch (Exception e)
                    {
                        // Will force log if cannot load animation speed
                        CECustomHandler.ForceLogToFile("Failed to load BackgroundAnimationSpeed for " + _listedEvent.Name + " : Exception: " + e);
                    }

                    CEPersistence.animationSpeed = speed;
                }
                else
                {
                    CEPersistence.animationPlayEvent = false;
                    new CESubModule().LoadTexture(textureFlag);
                }
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to load background for " + _listedEvent.Name);
                new CESubModule().LoadTexture(textureFlag);
            }
        }

        #endregion
    }
}