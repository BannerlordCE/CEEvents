using CaptivityEvents.Custom;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    public class CEEventMapNotification : InformationData
    {
        public override TextObject TitleText => new TextObject("{=CEEVENTS1060}Random Event", null);
        public override string SoundEventPath => "event:/ui/notification/alert";

        public CEEvent RandomEvent = null;

        public CEEventMapNotification(CEEvent randomEvent, TextObject descriptionText) : base(descriptionText)
        {
            RandomEvent = randomEvent;
        }
    }
}