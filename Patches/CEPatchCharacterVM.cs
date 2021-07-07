using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace CaptivityEvents.Patches
{
    // TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper CharacterVM
    [HarmonyPatch(typeof(CharacterVM), "InitializeCharacter")]
    internal class CEPatchCharacterVM
    {

        [HarmonyPrefix]
        public static bool InitializeCharacter(CharacterVM __instance)
        {
            __instance.HeroCharacter = new HeroViewModel(CharacterViewModel.StanceTypes.None);
            __instance.Skills = new MBBindingList<SkillVM>();
            __instance.Traits = new MBBindingList<EncyclopediaTraitItemVM>();
            __instance.Attributes.Clear();
            __instance.HeroCharacter.FillFrom(__instance.Hero, -1, false);
            __instance.HeroCharacter.SetEquipment(EquipmentIndex.ArmorItemEndSlot, default(EquipmentElement));
            __instance.HeroCharacter.SetEquipment(EquipmentIndex.HorseHarness, default(EquipmentElement));
            __instance.HeroCharacter.SetEquipment(EquipmentIndex.NumAllWeaponSlots, default(EquipmentElement));
            foreach (CharacterAttribute characterAttribute in Attributes.All)
            {
                if (characterAttribute.StringId == "CEAttribute") continue;
                CharacterAttributeItemVM item = new CharacterAttributeItemVM(
                    __instance.Hero,
                    characterAttribute,
                    __instance,
                    new Action<CharacterAttributeItemVM>((CharacterAttributeItemVM att) =>
                {
                    __instance.CurrentInspectedAttribute = att;
                    __instance.IsInspectingAnAttribute = true;
                }),
                    new Action<CharacterAttributeItemVM>((CharacterAttributeItemVM att) =>
                {
                    int unspentAttributePoints = __instance.UnspentAttributePoints;
                    __instance.UnspentAttributePoints = unspentAttributePoints - 1;
                    __instance.RefreshCharacterValues();
                }));
                __instance.Attributes.Add(item);
                foreach (SkillObject skill2 in characterAttribute.Skills)
                {
                    __instance.Skills.Add(new SkillVM(skill2, __instance, new Action<PerkVM>((PerkVM perk) =>
                {
                    __instance.PerkSelection.SetCurrentSelectionPerk(perk);
                })));
                }
            }
            using (List<SkillObject>.Enumerator enumerator3 = Skills.All.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    SkillObject skill = enumerator3.Current;
                    if (CESkills.CustomSkills.Exists(item => item.StringId == skill.StringId)) continue;
                    if (__instance.Skills.All((SkillVM s) => s.Skill != skill))
                    {
                        __instance.Skills.Add(new SkillVM(skill, __instance, new Action<PerkVM>((PerkVM perk) =>
                        {
                            __instance.PerkSelection.SetCurrentSelectionPerk(perk);
                        })));
                    }
                }
            }
            foreach (SkillVM skillVM in __instance.Skills)
            {
                skillVM.RefreshWithCurrentValues();
            }
            foreach (CharacterAttributeItemVM characterAttributeItemVM in __instance.Attributes)
            {
                characterAttributeItemVM.RefreshWithCurrentValues();
            }
            __instance.SetCurrentSkill(__instance.Skills[0]);
            __instance.RefreshCharacterValues();
            __instance.CharacterStats = new MBBindingList<StringPairItemVM>();
            if (__instance.Hero.GovernorOf != null)
            {
                GameTexts.SetVariable("SETTLEMENT_NAME", __instance.Hero.GovernorOf.Name.ToString());
                __instance.CharacterStats.Add(new StringPairItemVM(GameTexts.FindText("str_governor_of_label", null).ToString(), "", null));
            }
            if (MobileParty.MainParty.GetHeroPerkRole(__instance.Hero) != SkillEffect.PerkRole.None)
            {
                __instance.CharacterStats.Add(new StringPairItemVM(CampaignUIHelper.GetHeroClanRoleText(__instance.Hero, Clan.PlayerClan), "", null));
            }
            foreach (TraitObject traitObject in CampaignUIHelper.GetHeroTraits())
            {
                if (__instance.Hero.GetTraitLevel(traitObject) != 0)
                {
                    __instance.Traits.Add(new EncyclopediaTraitItemVM(traitObject, __instance.Hero));
                }
            }

            return false;
        }
    }

}
