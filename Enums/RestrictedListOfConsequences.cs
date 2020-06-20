using System;
using System.Xml.Serialization;

namespace CaptivityEvents.Enums
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public enum RestrictedListOfConsequences
    {
        GiveXP,
        GiveGold,
        GiveCaptorGold,
        ChangeClan,
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
        AttemptEscape,
        Escape,
        Leave,
        Continue,
        SoldToCaravan,
        SoldToSettlement,
        SoldToLordParty,
        SoldToNotable,
        PlayerIsNotBusy,
        HuntPrisoners,
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
        Submenu,
        RansomAndBribe,
        Trade,
        RebelPrisoners,
        GainRandomPrisoners
    }
}