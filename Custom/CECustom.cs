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
        public CESkillNode()
        {
            MinLevel = null;
            MaxLevel = null;
            Name = null;
            Id = null;
        }

        public CESkillNode(string Id, string Name, string MinLevel = "0", string MaxLevel = null)
        {
            this.MinLevel = MinLevel;
            this.MaxLevel = MaxLevel;
            this.Name = Name;
            this.Id = Id;
        }

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

        [XmlAttribute()]
        public bool DefaultValue { get; set; }
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