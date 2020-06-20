using System;
using System.Diagnostics;
using System.Xml.Serialization;
using CaptivityEvents.Enums;

namespace CaptivityEvents.Custom
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    [Serializable]
    public class MultipleRestrictedListOfFlags
    {
        [XmlElement("RestrictedListOfFlags")]
        public RestrictedListOfFlags[] RestrictedListOfFlags { get; set; }
    }
}