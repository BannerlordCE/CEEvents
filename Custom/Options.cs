using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents.Custom
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class Options
    {
        [XmlElement("Option")]
        public Option[] Option { get; set; }
    }
}