using System.Collections.Generic;

namespace CaptivityEvents.Custom
{
    public class CECustomModule(string ceModuleName, List<CEEvent> ceEvents)
    {
        public string CEModuleName { get; set; } = ceModuleName;

        public List<CEEvent> CEEvents { get; set; } = ceEvents;
    }
}