﻿#define V172
using CaptivityEvents.Config;
using TaleWorlds.CampaignSystem;
#if V171
using TaleWorlds.CampaignSystem.Barterables;
#else
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Party;
#endif

namespace CaptivityEvents.CampaignBehaviors
{
    public class CESetPrisonerFreeBarterBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents() => CampaignEvents.BarterablesRequested.AddNonSerializedListener(this, CheckForBarters);

        public override void SyncData(IDataStore dataStore) { }

        public void CheckForBarters(BarterData args)
        {
            PartyBase offererParty = args.OffererParty;
            PartyBase otherParty = args.OtherParty;

            if (offererParty == null || otherParty == null) return;

            foreach (CharacterObject characterObject in offererParty.PrisonerHeroes)
            {
                if (characterObject.IsHero && !FactionManager.IsAtWarAgainstFaction(characterObject.HeroObject.MapFaction, otherParty.MapFaction))
                {
                    if (CESettings.Instance != null && ((CESettings.Instance.EscapeAutoRansom.SelectedIndex != 0) || !(CESettings.Instance.EscapeAutoRansom.SelectedIndex == 1) && (!characterObject.IsPlayerCharacter || offererParty != PartyBase.MainParty)))
                    {
                        Barterable barterable = new SetPrisonerFreeBarterable(characterObject.HeroObject, args.OffererHero, args.OffererParty, args.OtherHero);
                        args.AddBarterable<PrisonerBarterGroup>(barterable);
                    }
                }
            }

            foreach (CharacterObject characterObject2 in otherParty.PrisonerHeroes)
            {
                if (characterObject2.IsHero && !FactionManager.IsAtWarAgainstFaction(characterObject2.HeroObject.MapFaction, offererParty.MapFaction))
                {
                    if (CESettings.Instance != null && ((CESettings.Instance.EscapeAutoRansom.SelectedIndex != 0) || !(CESettings.Instance.EscapeAutoRansom.SelectedIndex == 1) && (!characterObject2.IsPlayerCharacter || otherParty != PartyBase.MainParty)))
                    {
                        Barterable barterable2 = new SetPrisonerFreeBarterable(characterObject2.HeroObject, args.OtherHero, args.OtherParty, args.OffererHero);
                        args.AddBarterable<PrisonerBarterGroup>(barterable2);
                    }
                }
            }
        }
    }
}