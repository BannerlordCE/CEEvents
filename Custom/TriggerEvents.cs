using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents.Custom
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class TriggerEvents
    {
        [XmlElement("TriggerEvent")]
        public TriggerEvent[] Option { get; set; }
    }
}