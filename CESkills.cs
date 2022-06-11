#define V180

using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Extensions;


namespace CaptivityEvents
{
    internal static class CESkills
    {
        public static SkillObject Prostitution => CustomSkills[_StartDefaultSkillNode];

        public static SkillObject IsProstitute => CustomSkills[_StartDefaultSkillNode + 1];

        public static SkillObject Slavery => CustomSkills[_StartDefaultSkillNode + 2];

        public static SkillObject IsSlave => CustomSkills[_StartDefaultSkillNode + 3];

        public static bool IsInitialized { get; private set; } = false;

        internal static List<SkillObject> CustomSkills { get; private set; }

        internal static CharacterAttribute CEAttribute { get; private set; }

        private static int _StartDefaultSkillNode = 0;

        private static readonly List<CESkillNode> _Skills = new();

        public static void AddCustomSkill(CESkillNode skillNode)
        {
            int index = _Skills.FindIndex((item) => item.Id == skillNode.Id);
            if (index == -1)
            {
                _Skills.Add(skillNode);
            }
            else
            {
                _Skills[index].MaxLevel = skillNode.MaxLevel;
                _Skills[index].MinLevel = skillNode.MinLevel;
                _Skills[index].Name = skillNode.Name;
            }
        }

        public static CESkillNode FindSkillNode(string skill)
        {
            try
            {
                return _Skills.Find(skillNode => skillNode.Id == skill);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile(skill + " : " + e);
                return null;
            }
        }

        public static SkillObject FindSkill(string skill)
        {
            foreach (SkillObject skillObjectCustom in CustomSkills)
            {
                if (skillObjectCustom.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObjectCustom.StringId == skill)
                {
                    return skillObjectCustom;
                }
            }

            foreach (SkillObject skillObject in Skills.All)
            {
                if (skillObject.Name.ToString().Equals(skill, StringComparison.InvariantCultureIgnoreCase) || skillObject.StringId == skill)
                {
                    return skillObject;
                }
            }

            return null;
        }

        public static void RegisterAll(Game game)
        {
            CustomSkills = new List<SkillObject>();

            CEAttribute = game.ObjectManager.RegisterPresumedObject(new CharacterAttribute("CEAttribute"));

            _StartDefaultSkillNode = _Skills.Count;

            _Skills.Add(new CESkillNode("Prostitution", "{=CEEVENTS1106}Prostitution", "0"));
            _Skills.Add(new CESkillNode("IsProstitute", "{=CEEVENTS1104}prostitute", "0", "1"));
            _Skills.Add(new CESkillNode("Slavery", "{=CEEVENTS1105}Slavery", "0"));
            _Skills.Add(new CESkillNode("IsSlave", "{=CEEVENTS1103}slave", "0", "1"));

            foreach (CESkillNode skill in _Skills)
            {
                CustomSkills.Add(game.ObjectManager.RegisterPresumedObject(new SkillObject(skill.Id)));
            }

            InitializeAll();
        }

        public static bool Uninstall(Game game)
        {
            try
            {
                foreach (SkillObject skill in CustomSkills)
                {
                    game.ObjectManager.UnregisterObject(skill);
                }

                game.ObjectManager.UnregisterObject(CEAttribute);
                return true;
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Uninstall Error: " + e.ToString());
                InformationManager.DisplayMessage(new InformationMessage("Failure to Uninstall. Refer to LogFileFC.txt in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs", Colors.Red));

                return false;
            }
        }

        public static void InitializeAll()
        {
            CEAttribute.Initialize(new TextObject("CE"), new TextObject("Skills added by Captivity Events."), new TextObject("CE"));

            for (int i = 0; i < CustomSkills.Count; i++)
            {
                CustomSkills[i].Initialize(new TextObject(_Skills[i].Name), new TextObject(_Skills[i].Name), SkillObject.SkillTypeEnum.Personal).SetAttribute(CEAttribute);
            }

            IsInitialized = true;
        }
    }
}