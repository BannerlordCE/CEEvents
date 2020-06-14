using CaptivityEvents.Custom;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    public class CECaptorMapNotification : InformationData
    {
        public override TextObject TitleText => new TextObject("{=CEEVENTS1091}Captor Event", null);
        public override string SoundEventPath => "event:/ui/notification/alert";

        public CEEvent CaptorEvent = null;

        public CECaptorMapNotification(CEEvent captorEvent, TextObject descriptionText) : base(descriptionText)
        {
            CaptorEvent = captorEvent;
        }
    }
}