using System;
using System.Collections.Generic;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Config
{

    [AttributeUsage(AttributeTargets.Property)]
    internal class MappedAttribute : Attribute
    {
        public MappedAttribute()
        {
        }
    }

    public class CESettingsVM : ViewModel
    {

        public CESettingsVM()
        {
            GeneralOptions = new CESettingsVMCategory(this, new TextObject("General", null), GeneralList, false);
            EventsOptions = new CESettingsVMCategory(this, new TextObject("Events", null), EventsList, false);
            CustomFlagsOptions = new CESettingsVMCategory(this, new TextObject("Custom Flags", null), CustomFlagList, false);
        }

        private IEnumerable<ICEOptionData> GeneralList
        {
            get
            {
                yield break;
            }
        }

        private IEnumerable<ICEOptionData> EventsList
        {
            get
            {
                yield break;
            }
        }

        private IEnumerable<ICEOptionData> CustomFlagList
        {
            get
            {
                yield break;
            }
        }

        public float GetConfig(ICEOptionData data) => 0;

        public void SetConfig(ICEOptionData data, float val)
        {

        }

        private void ExecuteDone() => ScreenManager.PopScreen();

        private void ExecuteCancel() => ScreenManager.PopScreen();


        public CESettingsVMCategory GeneralOptions { get; }

        public CESettingsVMCategory EventsOptions { get; }

        public CESettingsVMCategory CustomFlagsOptions { get; }


    }
}
