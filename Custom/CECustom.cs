using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents.Custom
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = true)]
    [Serializable]
    public class CESkillNode(string id, string name, string minLevel = "0", string maxLevel = null, bool setZeroOnEscape = false)
    {
        public CESkillNode() : this(null, null, null) { }

        [XmlAttribute()] public string MinLevel { get; set; } = minLevel;

        [XmlAttribute()] public string MaxLevel { get; set; } = maxLevel;

        [XmlAttribute()] public string Name { get; set; } = name;

        [XmlAttribute()] public string Id { get; set; } = id;

        [XmlAttribute()] public bool SetZeroOnEscape { get; set; } = setZeroOnEscape;
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = true)]
    [Serializable]
    public class CEFlagNode
    {
        [XmlAttribute()] public string HintText { get; set; }

        [XmlAttribute()] public string Name { get; set; }

        [XmlAttribute()] public string Id { get; set; }

        [XmlAttribute()] public bool DefaultValue { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CECustom
    {
        [XmlElement("CEModuleName")] public string CEModuleName { get; set; }

        [XmlArrayItem("CEFlag")] public List<CEFlagNode> CEFlags { get; set; }

        [XmlArrayItem("CESkill")] public List<CESkillNode> CESkills { get; set; }
    }
}