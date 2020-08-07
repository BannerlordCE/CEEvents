using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents
{

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = true)]
    [Serializable]
    public class CESkillNode
    {
        [XmlAttribute()]
        public string MinLevel { get; set; }

        [XmlAttribute()]
        public string MaxLevel { get; set; }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = true)]
    [Serializable]
    public class CEFlagNode
    {
        [XmlAttribute()]
        public string HintText { get; set; }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Id { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CECustom
    {
        [XmlElement("CEModuleName")]
        public string CEModuleName { get; set; }

        [XmlArrayItem("CEFlag")]
        public List<CEFlagNode> CEFlags { get; set; }

        [XmlArrayItem("CESkill")]
        public List<CESkillNode> CESkills { get; set; }

    }
}