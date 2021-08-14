using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents.Custom
{

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class Line
    {
        [XmlAttribute()]
        public string Id { get; set; }

        [XmlAttribute()]
        public string Ref { get; set; }

        [XmlAttribute()]
        public string InputToken { get; set; }

        [XmlAttribute()]
        public string OutputToken { get; set; }

        [XmlAttribute()]
        public string Text { get; set; }

    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class Dialogue
    {
        [XmlElement("Line")]
        public Line[] Lines { get; set; }

    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CEScene
    {

        public string Name { get; set; }

        [XmlElement("Dialogue", IsNullable = true)]
        public Dialogue Dialogue { get; set; }
    }

    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CECustomScenes
    {
        [XmlElement("CEScene")]
        public CEScene[] CEScene { get; set; }
    }
}
