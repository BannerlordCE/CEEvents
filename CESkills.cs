#define STABLE
using CaptivityEvents.Custom;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents
{
    internal static class CESkills
    {
        public static SkillObject Prostitution => CustomSkills[_StartDefaultSkillNode];

        public static SkillObject IsProstitute => CustomSkills[_StartDefaultSkillNode + 1];

        public static SkillObject Slavery => CustomSkills[_StartDefaultSkillNode + 2];

        public static SkillObject IsSlave => CustomSkills[_StartDefaultSkillNode + 3];

        public static bool IsInitialized { get; private set; } = false;

        public static bool isUninstalled { get; private set; } = false;

        internal static List<SkillObject> CustomSkills { get; private set; }

        internal static CharacterAttribute CEAttribute { get; private set; }

        private static int _StartDefaultSkillNode = 0;

        private static readonly List<CESkillNode> _Skills = new List<CESkillNode>();

        public static void AddCustomSkill(CESkillNode skillNode) => _Skills.Add(skillNode);


        public static CESkillNode FindSkillNode(string skill)
        {
            if (isUninstalled) return null;
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
            if (isUninstalled) return null;

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

                isUninstalled = true;
                return true;

            } 
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("Uninstall Error: " + e.ToString());
                InformationManager.DisplayMessage(new InformationMessage("Failure to Uninstall. Refer to LogFileFC.txt in Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs", Colors.Red));

                isUninstalled = true;
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