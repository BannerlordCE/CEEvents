using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;

namespace CaptivityEvents.CampaignBehaviours
{
    public class CESetPrisonerFreeBarterBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.BarterablesRequested.AddNonSerializedListener(this, new Action<BarterData>(CheckForBarters));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public void CheckForBarters(BarterData args)
        {
            PartyBase offererParty = args.OffererParty;
            PartyBase otherParty = args.OtherParty;
            if (offererParty != null && otherParty != null)
            {
                foreach (CharacterObject characterObject in offererParty.PrisonerHeroes())
                {
                    if (characterObject.IsHero && !FactionManager.IsAtWarAgainstFaction(characterObject.HeroObject.MapFaction, otherParty.MapFaction))
                    {
                        if (!CESettings.Instance.PrisonerAutoRansom && (!characterObject.IsPlayerCharacter || offererParty == PartyBase.MainParty))
                        {
                            Barterable barterable = new SetPrisonerFreeBarterable(characterObject.HeroObject, args.OffererHero, args.OffererParty, args.OtherHero);
                            args.AddBarterable<PrisonerBarterGroup>(barterable, false);
                        }
                    }
                }
                foreach (CharacterObject characterObject2 in otherParty.PrisonerHeroes())
                {
                    if (characterObject2.IsHero && !FactionManager.IsAtWarAgainstFaction(characterObject2.HeroObject.MapFaction, offererParty.MapFaction))
                    {
                        if (!CESettings.Instance.PrisonerAutoRansom && (!characterObject2.IsPlayerCharacter || otherParty == PartyBase.MainParty))
                        {
                            Barterable barterable2 = new SetPrisonerFreeBarterable(characterObject2.HeroObject, args.OtherHero, args.OtherParty, args.OffererHero);
                            args.AddBarterable<PrisonerBarterGroup>(barterable2, false);
                        }
                    }
                }
            }
        }
    }
}