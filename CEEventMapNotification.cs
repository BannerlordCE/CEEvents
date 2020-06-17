using CaptivityEvents.Custom;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace CaptivityEvents.Notifications
{
    public class CEEventMapNotification : InformationData
    {
        public CEEvent RandomEvent = null;

        public override TextObject TitleText => new TextObject("{=CEEVENTS1060}Random Event", null);
        public override string SoundEventPath => "event:/ui/notification/alert";

        public CEEventMapNotification(CEEvent randomEvent, TextObject descriptionText) : base(descriptionText)
        {
            RandomEvent = randomEvent;
        }
    }
}