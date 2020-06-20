using System;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CaptivityEvents.Custom
{
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
}