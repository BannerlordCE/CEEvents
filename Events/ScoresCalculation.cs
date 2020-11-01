using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace CaptivityEvents.Events
{
    public class ScoresCalculation
    {
        internal int AttractivenessScore(Hero targetHero)
        {
            if (targetHero == null) return 10;

            int num = 0;
            if (targetHero.GetPerkValue(DefaultPerks.Medicine.PerfectHealth)) num += 10;

            // 1.5.3
            //if (targetHero.GetPerkValue(DefaultPerks.Steward.Prominence)) num += 15;

            // 1.5.4
            if (targetHero.GetPerkValue(DefaultPerks.Steward.Gourmet)) num += 15;

            if (targetHero.GetPerkValue(DefaultPerks.Charm.InBloom)) num += 10;

            return (targetHero.GetSkillValue(DefaultSkills.Charm) + targetHero.GetSkillValue(DefaultSkills.Athletics) / 2 + targetHero.GetSkillValue(DefaultSkills.Roguery) / 3 + targetHero.GetAttributeValue(CharacterAttributesEnum.Social) * 5 + num) / 2;
        }

        internal int EscapeProwessScore(Hero targetHero)
        {
            if (targetHero == null) return 10;

            return (targetHero.GetSkillValue(DefaultSkills.Tactics) / 4 + targetHero.GetSkillValue(DefaultSkills.Roguery) / 4) / 4;
        }
    }
}