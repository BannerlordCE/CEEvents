using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.ObjectSystem;

namespace CaptivityEvents.Patches
{

    // TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement ClanLordItemVM
    [HarmonyPatch(typeof(ClanLordItemVM), "UpdateProperties")]
    internal class CEPatchClanLordItemVM
    {
        public static AccessTools.FieldRef<ClanLordItemVM, Hero> _hero = AccessTools.FieldRefAccess<ClanLordItemVM, Hero>("_hero");

        [HarmonyPrefix]
        public static bool UpdateProperties(ClanLordItemVM __instance)
        {
            __instance.RelationToMainHeroText = "";
            __instance.GovernorOfText = "";
            __instance.Skills.Clear();
            __instance.Traits.Clear();
            __instance.IsMainHero = (_hero(__instance) == Hero.MainHero);

            List<SkillObject> skillsToShow = new List<SkillObject>();
            foreach (SkillObject skill in Skills.All)
            {
                if (skill.CharacterAttribute == null || skill.CharacterAttribute.StringId == "CEAttribute") continue;
                skillsToShow.Add(skill);
            }

            foreach (SkillObject skill in (from s in skillsToShow group s by s.CharacterAttribute.Id).SelectMany((IGrouping<MBGUID, SkillObject> s) => s).ToList())
            {
                __instance.Skills.Add(new EncyclopediaSkillVM(skill, _hero(__instance).GetSkillValue(skill)));
            }
            foreach (TraitObject traitObject in CampaignUIHelper.GetHeroTraits())
            {
                if (_hero(__instance).GetTraitLevel(traitObject) != 0)
                {
                    __instance.Traits.Add(new EncyclopediaTraitItemVM(traitObject, _hero(__instance)));
                }
            }
            __instance.IsChild = _hero(__instance).IsChild;
            if (_hero(__instance) != Hero.MainHero)
            {
                __instance.RelationToMainHeroText = CampaignUIHelper.GetHeroRelationToHeroText(_hero(__instance), Hero.MainHero).ToString();
            }
            if (_hero(__instance).GovernorOf != null)
            {
                GameTexts.SetVariable("SETTLEMENT_NAME", _hero(__instance).GovernorOf.Owner.Settlement.EncyclopediaLinkWithName);
                __instance.GovernorOfText = GameTexts.FindText("str_governor_of_label", null).ToString();
            }
            __instance.HeroModel = new HeroViewModel(CharacterViewModel.StanceTypes.None);
            __instance.HeroModel.FillFrom(_hero(__instance), -1, false);
            __instance.Banner_9 = new ImageIdentifierVM(BannerCode.CreateFrom(_hero(__instance).ClanBanner), true);

            return false;
        }
    }
}
