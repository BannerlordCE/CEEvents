using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace CaptivityEvents
{
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = null, IsNullable = false)]
    [Serializable]
    public class CEFlags
    {
        [XmlElement("CEModuleName")]
        public string Name { get; set; }

        [XmlElement("CEFlag")]
        public List<string> Flags { get; set; }
    }
}