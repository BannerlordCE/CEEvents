#define V102

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
        public static SkillObject Prostitution => CustomSkills[0];

        public static SkillObject IsProstitute => CustomSkills[1];

        public static SkillObject Slavery => CustomSkills[2];

        public static SkillObject IsSlave => CustomSkills[3];

        public static bool IsInitialized { get; private set; } = false;

        internal static List<SkillObject> CustomSkills { get; private set; }

        internal static CharacterAttribute CEAttribute { get; private set; }

        internal static readonly List<CESkillNode> NodeSkills = new() {
           new CESkillNode("Prostitution", "{=CEEVENTS1106}Prostitution", "0"),
           new CESkillNode("IsProstitute", "{=CEEVENTS1104}prostitute", "0", "1"),
           new CESkillNode("Slavery", "{=CEEVENTS1105}Slavery", "0"),
           new CESkillNode("IsSlave", "{=CEEVENTS1103}slave", "0", "1")
        };


        public static void AddCustomSkill(CESkillNode skillNode)
        {
            int index = NodeSkills.FindIndex((item) => item.Id == skillNode.Id);
            if (index == -1)
            {
                NodeSkills.Add(skillNode);
            }
            else
            {
                NodeSkills[index].MaxLevel = skillNode.MaxLevel;
                NodeSkills[index].MinLevel = skillNode.MinLevel;
                NodeSkills[index].Name = skillNode.Name;
                NodeSkills[index].SetZeroOnEscape = skillNode.SetZeroOnEscape;
            }
        }

        public static CESkillNode FindSkillNode(string skill)
        {
            try
            {
                return NodeSkills.Find(skillNode => skillNode.Id == skill);
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
            if (IsInitialized) return;

            CustomSkills = new List<SkillObject>();

            CEAttribute = game.ObjectManager.RegisterPresumedObject(new CharacterAttribute("CEAttribute"));

            foreach (CESkillNode skill in NodeSkills)
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
                CustomSkills[i].Initialize(new TextObject(NodeSkills[i].Name), new TextObject(NodeSkills[i].Name), SkillObject.SkillTypeEnum.Personal).SetAttribute(CEAttribute);
            }

            IsInitialized = true;
        }
    }
}