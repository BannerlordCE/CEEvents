using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents.Custom
{
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