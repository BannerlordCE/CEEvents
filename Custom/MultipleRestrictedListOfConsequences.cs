using System;
using System.Diagnostics;
using System.Xml.Serialization;
using CaptivityEvents.Enums;

namespace CaptivityEvents.Custom
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    [Serializable]
    public class MultipleRestrictedListOfConsequences
    {
        [XmlElement("RestrictedListOfConsequences")]
        public RestrictedListOfConsequences[] RestrictedListOfConsequences { get; set; }
    }
}