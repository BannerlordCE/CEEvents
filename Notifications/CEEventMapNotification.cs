using CaptivityEvents.Custom;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CaptivityEvents.Notifications
{
    public class CEEventMapNotification : InformationData
    {
        public CEEvent RandomEvent;

        public override TextObject TitleText => new TextObject("{=CEEVENTS1060}Random Event");
        public override string SoundEventPath => "event:/ui/notification/alert";

        public CEEventMapNotification(CEEvent randomEvent, TextObject descriptionText) : base(descriptionText)
        {
            RandomEvent = randomEvent;
        }
    }
}