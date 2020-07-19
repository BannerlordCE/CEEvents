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
    public class CECustom
    {
        [XmlElement("CEModuleName")]
        public string CEModuleName { get; set; }

        [XmlArrayItem("CEFlag")]
        public List<string> CEFlags { get; set; }
    }
}