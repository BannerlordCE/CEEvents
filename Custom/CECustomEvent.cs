using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Custom
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public enum RestrictedListOfConsequences
    {
        GiveXP,
        GiveGold,
        GiveCaptorGold,
        CaptiveMarryCaptor,
        ChangeGold,
        ChangeCaptorGold,
        ChangeProstitutionLevel,
        ChangeSlaveryLevel,
        AddProstitutionFlag,
        RemoveProstitutionFlag,
        AddSlaveryFlag,
        RemoveSlaveryFlag,
        ChangeMorale,
        ChangeRenown,
        ChangeCaptorRenown,
        ChangeHealth,
        ChangeRelation,
        ChangeTrait,
        ChangeCaptorTrait,
        ChangeSkill,
        ChangeCaptorSkill,
        ImpregnationRisk,
        ImpregnationHero,
        ImpregnationByPlayer,
        AttemptEscape,
        Escape,
        Leave,
        Continue,
        SoldToCaravan,
        SoldToSettlement,
        SoldToLordParty,
        SoldToNotable,
        CapturePlayer,
        PlayerIsNotBusy,
        PlayerAllowedCompanion,
        StartBattle,
        HuntPrisoners,
        ReleaseRandomPrisoners,
        ReleaseAllPrisoners,
        KillPrisoner,
        KillCaptor,
        KillRandomPrisoners,
        KillAllPrisoners,
        CaptiveLeaveSpouse,
        Strip,
        StripHero,
        EmptyIcon,
        Wait,
        BribeAndEscape,
        EscapeIcon,
        Submenu,
        RansomAndBribe,
        Trade,
        RebelPrisoners,
        GainRandomPrisoners,
        StripPlayer,
        NoInformationMessage,
        NoMessages,
        JoinCaptor,
        WoundPrisoner,
        WoundCaptor,
        WoundAllPrisoners,
        WoundRandomPrisoners,
        MakeHeroCompanion,
        CaptorLeaveSpouse,
        TeleportPlayer,
        KillRandomTroops,
        WoundRandomTroops
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public enum RestrictedListOfFlags
    {
        WaitingMenu,
        ProgressMenu,
        CanOnlyBeTriggeredByOtherEvent,
        Common,
        Femdom,
        Bestiality,
        Prostitution,
        Romance,
        Slavery,
        Straight,
        Lesbian,
        Overwriteable,
        Gay,
        CaravanParty,
        BanditParty,
        LordParty,
        DefaultParty,
        NotableFemalesNearby,
        NotableMalesNearby,
        VisitedByCaravan,
        VisitedByLord,
        DuringSiege,
        DuringRaid,
        LocationTravellingParty,
        LocationCaravan,
        LocationPartyInTown,
        LocationPartyInCastle,
        LocationPartyInVillage,
        LocationDungeon,
        LocationHideout,
        LocationTavern,
        LocationVillage,
        LocationCity,
        LocationCastle,
        TimeNight,
        TimeDay,
        HeroGenderIsFemale,
        HeroGenderIsMale,
        Random,
        Captor,
        Captive,
        CaptiveIsHero,
        CaptiveIsNonHero,
        OwnerGenderIsFemale,
        OwnerGenderIsMale,
        CaptorIsHero,
        CaptorIsNonHero,
        CaptorGenderIsFemale,
        CaptorGenderIsMale,
        CaptorHaveOffspring,
        CaptorNotHaveOffspring,
        CaptorHaveSpouse,
        CaptorNotHaveSpouse,
        CaptorOwnsCurrentSettlement,
        CaptorOwnsNotCurrentSettlement,
        CaptorFactionOwnsSettlement,
        CaptorNeutralFactionOwnsSettlement,
        CaptorEnemyFactionOwnsSettlement,
        HeroOwnedByNotable,
        HeroNotOwnedByNotable,
        HeroHaveOffspring,
        HeroNotHaveOffspring,
        HeroHaveSpouse,
        HeroNotHaveSpouse,
        HeroIsPregnant,
        HeroIsNotPregnant,
        HeroIsProstitute,
        HeroIsNotProstitute,
        HeroIsSlave,
        HeroIsNotSlave,
        HeroIsOwned,
        HeroIsNotOwned,
        HeroOwnsFief,
        HeroOwnsNoFief,
        HeroIsClanLeader,
        HeroIsNotClanLeader,
        HeroIsFactionLeader,
        HeroIsNotFactionLeader,
        HeroOwnsCurrentParty,
        HeroOwnsNotCurrentParty,
        HeroFactionOwnsParty,
        HeroNeutralFactionOwnsParty,
        HeroEnemyFactionOwnsParty,
        HeroOwnsCurrentSettlement,
        HeroOwnsNotCurrentSettlement,
        HeroFactionOwnsSettlement,
        HeroNeutralFactionOwnsSettlement,
        HeroEnemyFactionOwnsSettlement,
        DeathAlternative,
        CaptureAlternative,
        DesertionAlternative,
        MarriageAlternative,
        DuringPlayerRaid,
        SeasonSpring,
        SeasonWinter,
        SeasonSummer,
        SeasonFall,
        CaptivesOutNumber,
        PlayerIsNotBusy,
        PlayerAllowedCompanion,
        PlayerOwnsBrothelInSettlement,
        PlayerOwnsNotBrothelInSettlement,
        StripEnabled,
        StripDisabled,
        IgnoreAllOther,
        CaptorIsNotPregnant,
        CaptorIsPregnant,
        CaptorOwnsNoFief,
        CaptorOwnsFief,
        CaptorIsNotClanLeader,
        CaptorIsClanLeader,
        CaptorIsFactionLeader,
        CaptorIsNotFactionLeader,
    }

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public enum TerrainType
    {
        Water,
        Mountain,
        Snow,
        Steppe,
        Plain,
        Desert,
        Swamp,
        Dune,
        Bridge,
        River,
        Forest,
        ShallowRiver,
        Lake,
        Canyon,
        RuralArea
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class ProgressEvent
    {
        public bool ShouldStopMoving { get; set; }

        public string DisplayProgressMode { get; set; }

        public string TimeToTake { get; set; }

        public string TriggerEventName { get; set; }

        [XmlArrayItem("TriggerEvent", IsNullable = true)]
        public TriggerEvent[] TriggerEvents { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class DelayEvent
    {
        public string UseConditions { get; set; }

        public string TimeToTake { get; set; }

        public string TriggerEventName { get; set; }

        [XmlArrayItem("TriggerEvent", IsNullable = true)]
        public TriggerEvent[] TriggerEvents { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class StripSettings
    {
        [XmlAttribute()]
        public bool Forced { get; set; }

        [XmlAttribute()]
        public bool QuestEnabled { get; set; }

        public string Clothing { get; set; }

        public string Mount { get; set; }

        public string Melee { get; set; }

        public string Ranged { get; set; }

        public string CustomBody { get; set; }

        public string CustomCape { get; set; }

        public string CustomGloves { get; set; }

        public string CustomLegs { get; set; }

        public string CustomHead { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class TriggerEvent
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string EventName { get; set; }

        public string EventWeight { get; set; }

        public string EventUseConditions { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class TeleportSettings
    {
        [XmlAttribute()]
        public string Location { get; set; }

        [XmlAttribute()]
        public string LocationName { get; set; }

        [XmlAttribute()]
        public string Distance { get; set; }

        [XmlAttribute()]
        public string Faction { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class SceneSettings
    {
        [XmlAttribute()]
        public string TalkTo { get; set; }

        [XmlAttribute()]
        public string SceneName { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class BattleSettings
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Victory { get; set; }

        [XmlAttribute()]
        public string Defeat { get; set; }

        [XmlAttribute()]
        public string PlayerTroops { get; set; }

        [XmlAttribute()]
        public string EnemyName { get; set; }

        [XmlArrayItem("SpawnTroop", IsNullable = true)]
        public SpawnTroop[] SpawnTroops { get; set; }
    }

    public class Companion
    {
        [XmlAttribute()]
        public string Id { get; set; }

        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Type { get; set; }

        [XmlAttribute()]
        public string Location { get; set; }

        [XmlAttribute()]
        public string UseOtherConditions { get; set; }

        [XmlArrayItem("RestrictedListOfConsequences", IsNullable = false)]
        public RestrictedListOfConsequences[] MultipleRestrictedListOfConsequences { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string PregnancyRiskModifier { get; set; }

        public string GoldTotal { get; set; }

        public string CaptorGoldTotal { get; set; }

        public string MoraleTotal { get; set; }

        public string RelationTotal { get; set; }

        public string HealthTotal { get; set; }

        public string RenownTotal { get; set; }

        [XmlArrayItem("Trait", IsNullable = true)]
        public TraitToLevel[] TraitsToLevel { get; set; }

        [XmlArrayItem("Skill", IsNullable = true)]
        public SkillToLevel[] SkillsToLevel { get; set; }

        [XmlArrayItem("KingdomOption", IsNullable = true)]
        public KingdomOption[] KingdomOptions { get; set; }

        [XmlArrayItem("ClanOption", IsNullable = true)]
        public ClanOption[] ClanOptions { get; set; }
    }

    public class TraitToLevel
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string ByXP { get; set; }

        [XmlAttribute()]
        public string ByLevel { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }

        [XmlAttribute()]
        public string Color { get; set; }

        [XmlAttribute()]
        public bool HideNotification { get; set; }
    }

    public class TraitRequired
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Max { get; set; }

        [XmlAttribute()]
        public string Min { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }
    }

    public class SkillToLevel
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string ByXP { get; set; }

        [XmlAttribute()]
        public string ByLevel { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }

        [XmlAttribute()]
        public string Color { get; set; }

        [XmlAttribute()]
        public bool HideNotification { get; set; }
    }

    public class SkillRequired
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Max { get; set; }

        [XmlAttribute()]
        public string Min { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }
    }

    public class ClanOption
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Action { get; set; }

        [XmlAttribute()]
        public string Clan { get; set; }

        [XmlAttribute()]
        public bool HideNotification { get; set; }
    }

    public class KingdomOption
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Action { get; set; }

        [XmlAttribute()]
        public string Kingdom { get; set; }

        [XmlAttribute()]
        public bool HideNotification { get; set; }
    }

    public class SpawnTroop
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }

        [XmlAttribute()]
        public string Number { get; set; }

        [XmlAttribute()]
        public string WoundedNumber { get; set; }
    }

    public class SpawnHero
    {
        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string Culture { get; set; }

        [XmlAttribute()]
        public string Gender { get; set; }

        [XmlAttribute()]
        public string Clan { get; set; }

        [XmlArrayItem("Skill", IsNullable = true)]
        public SkillToLevel[] SkillsToLevel { get; set; }
    }

    public class Background
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Weight { get; set; }

        [XmlAttribute()]
        public string UseConditions { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public class Option
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Order { get; set; }

        [XmlArrayItem("RestrictedListOfConsequences", IsNullable = false)]
        public RestrictedListOfConsequences[] MultipleRestrictedListOfConsequences { get; set; }

        public string SoundName { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string OptionText { get; set; }

        public string PregnancyRiskModifier { get; set; }

        public string EscapeChance { get; set; }

        public string ReqHeroPartyHaveItem { get; set; }

        public string ReqCaptorPartyHaveItem { get; set; }

        public string ReqHeroHealthBelowPercentage { get; set; }

        public string ReqHeroHealthAbovePercentage { get; set; }

        public string ReqHeroCaptorRelationAbove { get; set; }

        public string ReqHeroCaptorRelationBelow { get; set; }

        public string ReqHeroSlaveLevelAbove { get; set; }

        public string ReqHeroSlaveLevelBelow { get; set; }

        public string ReqHeroProstituteLevelAbove { get; set; }

        public string ReqHeroProstituteLevelBelow { get; set; }

        public string ReqHeroTraitLevelAbove { get; set; }

        public string ReqHeroTraitLevelBelow { get; set; }

        public string ReqCaptorTraitLevelAbove { get; set; }

        public string ReqCaptorTraitLevelBelow { get; set; }

        public string ReqHeroSkillLevelAbove { get; set; }

        public string ReqHeroSkillLevelBelow { get; set; }

        public string ReqCaptorSkillLevelAbove { get; set; }

        public string ReqCaptorSkillLevelBelow { get; set; }

        public string ReqMoraleAbove { get; set; }

        public string ReqMoraleBelow { get; set; }

        public string ReqTroopsAbove { get; set; }

        public string ReqTroopsBelow { get; set; }

        public string ReqMaleTroopsAbove { get; set; }

        public string ReqMaleTroopsBelow { get; set; }

        public string ReqFemaleTroopsAbove { get; set; }

        public string ReqFemaleTroopsBelow { get; set; }

        public string ReqCaptivesAbove { get; set; }

        public string ReqCaptivesBelow { get; set; }

        public string ReqFemaleCaptivesAbove { get; set; }

        public string ReqFemaleCaptivesBelow { get; set; }

        public string ReqMaleCaptivesAbove { get; set; }

        public string ReqMaleCaptivesBelow { get; set; }

        public string ReqHeroTroopsAbove { get; set; }

        public string ReqHeroTroopsBelow { get; set; }

        public string ReqHeroMaleTroopsAbove { get; set; }

        public string ReqHeroMaleTroopsBelow { get; set; }

        public string ReqHeroFemaleTroopsAbove { get; set; }

        public string ReqHeroFemaleTroopsBelow { get; set; }

        public string ReqHeroCaptivesAbove { get; set; }

        public string ReqHeroCaptivesBelow { get; set; }

        public string ReqHeroFemaleCaptivesAbove { get; set; }

        public string ReqHeroFemaleCaptivesBelow { get; set; }

        public string ReqHeroMaleCaptivesAbove { get; set; }

        public string ReqHeroMaleCaptivesBelow { get; set; }

        public string ReqGoldAbove { get; set; }

        public string ReqGoldBelow { get; set; }

        public string SkillToLevel { get; set; }

        public string ReqCaptorSkill { get; set; }

        public string ReqHeroSkill { get; set; }

        public string TraitToLevel { get; set; }

        public string ReqCaptorTrait { get; set; }

        public string ReqHeroTrait { get; set; }

        public string GoldTotal { get; set; }

        public string CaptorGoldTotal { get; set; }

        public string MoraleTotal { get; set; }

        public string RelationTotal { get; set; }

        public string HealthTotal { get; set; }

        public string RenownTotal { get; set; }

        public string ProstitutionTotal { get; set; }

        public string SlaveryTotal { get; set; }

        public string TraitTotal { get; set; }

        public string TraitXPTotal { get; set; }

        public string SkillTotal { get; set; }

        public string SkillXPTotal { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string TriggerEventName { get; set; }

        [XmlArrayItem("TriggerEvent", IsNullable = true)]
        public TriggerEvent[] TriggerEvents { get; set; }

        [XmlArrayItem("TraitRequired", IsNullable = true)]
        public TraitRequired[] TraitsRequired { get; set; }

        [XmlArrayItem("Companion", IsNullable = true)]
        public Companion[] Companions { get; set; }

        [XmlArrayItem("Trait", IsNullable = true)]
        public TraitToLevel[] TraitsToLevel { get; set; }

        [XmlArrayItem("Skill", IsNullable = true)]
        public SkillToLevel[] SkillsToLevel { get; set; }

        [XmlArrayItem("SkillRequired", IsNullable = true)]
        public SkillRequired[] SkillsRequired { get; set; }

        [XmlElement("StripSettings", IsNullable = true)]
        public StripSettings StripSettings { get; set; }

        [XmlElement("BattleSettings", IsNullable = true)]
        public BattleSettings BattleSettings { get; set; }

        [XmlElement("TeleportSettings", IsNullable = true)]
        public TeleportSettings TeleportSettings { get; set; }

        [XmlElement("SceneSettings", IsNullable = true)]
        public SceneSettings SceneSettings { get; set; }

        [XmlElement("DelayEvent", IsNullable = true)]
        public DelayEvent DelayEvent { get; set; }

        [XmlArrayItem("SpawnTroop", IsNullable = true)]
        public SpawnTroop[] SpawnTroops { get; set; }

        [XmlArrayItem("SpawnHero", IsNullable = true)]
        public SpawnHero[] SpawnHeroes { get; set; }

        [XmlArrayItem("KingdomOption", IsNullable = true)]
        public KingdomOption[] KingdomOptions { get; set; }

        [XmlArrayItem("ClanOption", IsNullable = true)]
        public ClanOption[] ClanOptions { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public class CEEvent
    {
        public string Name { get; set; }

        public string Text { get; set; }

        public string BackgroundName { get; set; }

        [XmlArrayItem("Background", IsNullable = true)]
        public Background[] Backgrounds { get; set; }

        public string NotificationName { get; set; }

        public string SoundName { get; set; }

        [XmlArrayItem("BackgroundName")]
        public List<string> BackgroundAnimation { get; set; }

        public string BackgroundAnimationSpeed { get; set; }

        [XmlArrayItem("CustomFlag")]
        public List<string> MultipleListOfCustomFlags { get; set; }

        [XmlArrayItem("RestrictedListOfFlags", IsNullable = false)]
        public RestrictedListOfFlags[] MultipleRestrictedListOfFlags { get; set; }

        [XmlArrayItem("Option", IsNullable = true)]
        public Option[] Options { get; set; }

        [XmlElement("ProgressEvent", IsNullable = true)]
        public ProgressEvent ProgressEvent { get; set; }

        public bool ReqCustomCode { get; set; }

        public string SceneToPlay { get; set; }

        public string OrderToCall { get; set; }

        public bool SexualContent { get; set; }

        public string WeightedChanceOfOccuring { get; set; }

        public string CanOnlyHappenNrOfTimes { get; set; }

        public string ReqHeroMinAge { get; set; }

        public string ReqHeroMaxAge { get; set; }

        public string ReqHeroPartyHaveItem { get; set; }

        public string ReqCaptorPartyHaveItem { get; set; }

        public string PregnancyRiskModifier { get; set; }

        public string EscapeChance { get; set; }

        public string ReqHeroHealthBelowPercentage { get; set; }

        public string ReqHeroHealthAbovePercentage { get; set; }

        public string ReqHeroCaptorRelationAbove { get; set; }

        public string ReqHeroCaptorRelationBelow { get; set; }

        public string ReqHeroProstituteLevelAbove { get; set; }

        public string ReqHeroProstituteLevelBelow { get; set; }

        public string ReqHeroSlaveLevelAbove { get; set; }

        public string ReqHeroSlaveLevelBelow { get; set; }

        public string ReqHeroTraitLevelAbove { get; set; }

        public string ReqHeroTraitLevelBelow { get; set; }

        public string ReqCaptorTraitLevelAbove { get; set; }

        public string ReqCaptorTraitLevelBelow { get; set; }

        public string ReqHeroSkillLevelAbove { get; set; }

        public string ReqHeroSkillLevelBelow { get; set; }

        public string ReqCaptorSkillLevelAbove { get; set; }

        public string ReqCaptorSkillLevelBelow { get; set; }

        public string ReqMoraleAbove { get; set; }

        public string ReqMoraleBelow { get; set; }

        public string ReqTroopsAbove { get; set; }

        public string ReqTroopsBelow { get; set; }

        public string ReqMaleTroopsAbove { get; set; }

        public string ReqMaleTroopsBelow { get; set; }

        public string ReqFemaleTroopsAbove { get; set; }

        public string ReqFemaleTroopsBelow { get; set; }

        public string ReqCaptivesAbove { get; set; }

        public string ReqCaptivesBelow { get; set; }

        public string ReqFemaleCaptivesAbove { get; set; }

        public string ReqFemaleCaptivesBelow { get; set; }

        public string ReqMaleCaptivesAbove { get; set; }

        public string ReqMaleCaptivesBelow { get; set; }

        public string ReqHeroTroopsAbove { get; set; }

        public string ReqHeroTroopsBelow { get; set; }

        public string ReqHeroMaleTroopsAbove { get; set; }

        public string ReqHeroMaleTroopsBelow { get; set; }

        public string ReqHeroFemaleTroopsAbove { get; set; }

        public string ReqHeroFemaleTroopsBelow { get; set; }

        public string ReqHeroCaptivesAbove { get; set; }

        public string ReqHeroCaptivesBelow { get; set; }

        public string ReqHeroFemaleCaptivesAbove { get; set; }

        public string ReqHeroFemaleCaptivesBelow { get; set; }

        public string ReqHeroMaleCaptivesAbove { get; set; }

        public string ReqHeroMaleCaptivesBelow { get; set; }

        public string ReqGoldAbove { get; set; }

        public string ReqGoldBelow { get; set; }

        public string GoldTotal { get; set; }

        public string CaptorGoldTotal { get; set; }

        public string MoraleTotal { get; set; }

        public string RelationTotal { get; set; }

        public string HealthTotal { get; set; }

        public string RenownTotal { get; set; }

        public string SlaveryTotal { get; set; }

        public string ProstitutionTotal { get; set; }

        public string TraitTotal { get; set; }

        public string TraitXPTotal { get; set; }

        public string SkillTotal { get; set; }

        public string SkillXPTotal { get; set; }

        public string SkillToLevel { get; set; }

        public string ReqCaptorSkill { get; set; }

        public string ReqHeroSkill { get; set; }

        public string TraitToLevel { get; set; }

        public string ReqCaptorTrait { get; set; }

        public string ReqHeroTrait { get; set; }

        [XmlArrayItem("SkillRequired", IsNullable = true)]
        public SkillRequired[] SkillsRequired { get; set; }

        [XmlArrayItem("Skill", IsNullable = true)]
        public SkillToLevel[] SkillsToLevel { get; set; }

        [XmlArrayItem("TraitRequired", IsNullable = true)]
        public TraitRequired[] TraitsRequired { get; set; }

        [XmlArrayItem("Trait", IsNullable = true)]
        public TraitToLevel[] TraitsToLevel { get; set; }

        [XmlArrayItem("Companion", IsNullable = true)]
        public Companion[] Companions { get; set; }

        [XmlArrayItem("TerrainTypes", IsNullable = true)]
        public TerrainType[][] TerrainTypesRequirements { get; set; }

        [XmlIgnore]
        public CharacterObject Captive { get; set; }

        [XmlIgnore]
        public Dictionary<string, Hero> SavedCompanions { get; set; }

        [XmlIgnore]
        public string OldBackgroundName { get; set; }

        [XmlIgnore]
        public string OldWeightedChanceOfOccuring { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CEEvents
    {
        [XmlElement("CEEvent")]
        public CEEvent[] CEEvent { get; set; }
    }
}