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

            if (targetHero.GetPerkValue(DefaultPerks.Charm.InBloom)) num += 15;

            if (targetHero.GetPerkValue(DefaultPerks.Steward.Gourmet)) num += 5;

            return (targetHero.GetSkillValue(DefaultSkills.Charm) + targetHero.GetSkillValue(DefaultSkills.Athletics) / 2 + targetHero.GetSkillValue(DefaultSkills.Roguery) / 3 + targetHero.GetAttributeValue(DefaultCharacterAttributes.Social) * 5 + num) / 2;
        }

        internal int EscapeProwessScore(Hero targetHero)
        {
            if (targetHero == null) return 10;

            return (targetHero.GetSkillValue(DefaultSkills.Tactics) / 4 + targetHero.GetSkillValue(DefaultSkills.Roguery) / 4) / 4;
        }
    }
}