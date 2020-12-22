using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptivityEvents.Custom
{
    public class CECustomModule
    {
        public CECustomModule(string CEModuleName, List<CEEvent> CEEvents)
        {
            this.CEModuleName = CEModuleName;
            this.CEEvents = CEEvents;
        }

        public string CEModuleName { get; set; }

        public List<CEEvent> CEEvents { get; set; }
    }
}
