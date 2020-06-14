using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents
{
    internal static class CESkills
    {
        public static SkillObject Prostitution => CESkills.SkillProstitution;

        public static SkillObject Slavery => CESkills.SkillSlavery;

        public static bool IsInitialized => CESkills._initialized;

        internal static bool _initialized = false;

        internal static CharacterAttribute CEAttribute { get; private set; }

        internal static SkillObject SkillProstitution { get; private set; }

        internal static SkillObject SkillSlavery { get; private set; }

        internal static CharacterAttribute CEFlags { get; private set; }

        internal static SkillObject IsProstitute { get; private set; }

        internal static SkillObject IsSlave { get; private set; }

        public static void RegisterAll(Game game)
        {
            CEAttribute = game.ObjectManager.RegisterPresumedObject<CharacterAttribute>(new CharacterAttribute("CEAttribute"));
            SkillProstitution = game.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("Prostitution"));
            SkillSlavery = game.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("Slavery"));

            CEFlags = game.ObjectManager.RegisterPresumedObject<CharacterAttribute>(new CharacterAttribute("CEFlags"));
            IsProstitute = game.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("IsProstitute"));
            IsSlave = game.ObjectManager.RegisterPresumedObject<SkillObject>(new SkillObject("IsSlave"));

            InitializeAll();
        }

        public static void InitializeAll()
        {
            CEAttribute.Initialize(new TextObject("CE"), new TextObject("CE represents the ability to move with speed and force."), new TextObject("CE"), CharacterAttributesEnum.Social);
            SkillProstitution.Initialize(new TextObject("{=CEEVENTS1106}Prostitution"), new TextObject("Prostitution Skill"), SkillObject.SkillTypeEnum.Personal).SetAttribute(CEAttribute);
            SkillSlavery.Initialize(new TextObject("{=CEEVENTS1105}Slavery"), new TextObject("Slave training"), SkillObject.SkillTypeEnum.Personal).SetAttribute(CEAttribute);

            CEFlags.Initialize(new TextObject("CEFlags"), new TextObject("CEFlags represents the ability to move with speed and force."), new TextObject("CEF"), CharacterAttributesEnum.Social);
            IsProstitute.Initialize(new TextObject("{=CEEVENTS1104}prostitute"), new TextObject("IsProstitute Flag"), SkillObject.SkillTypeEnum.Personal).SetAttribute(CEFlags);
            IsSlave.Initialize(new TextObject("{=CEEVENTS1103}slave"), new TextObject("IsSlave Flag"), SkillObject.SkillTypeEnum.Personal).SetAttribute(CEFlags);

            _initialized = true;
        }
    }
}