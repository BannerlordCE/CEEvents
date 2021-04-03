using TaleWorlds.CampaignSystem;

namespace CaptivityEvents.Helper
{
    public class CEDelayedEvent
    {
        public string eventName;
        public float eventTime;
        public bool hasBeenFired = false;
        public bool conditions;

        public CEDelayedEvent(string eventName, float eventTime = -1, bool conditions = false)
        {
            this.eventName = eventName;
            this.eventTime = eventTime;
            this.conditions = conditions;
        }
    }
}
