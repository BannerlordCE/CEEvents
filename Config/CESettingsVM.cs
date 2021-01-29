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
            CaptiveOptions = new CESettingsVMCategory(this, new TextObject("Captive", null), CaptiveList, false);
            CaptorOptions = new CESettingsVMCategory(this, new TextObject("Captor", null), CaptorList, false);
            RandomOptions = new CESettingsVMCategory(this, new TextObject("Random", null), RandomList, false);
            EventsListOptions = new CESettingsVMCategory(this, new TextObject("Event List", null), EventsList, false);
            CustomFlagsOptions = new CESettingsVMCategory(this, new TextObject("Custom Flags", null), CustomFlagList, false);
        }

        private IEnumerable<ICEOptionData> GeneralList
        {
            get
            {

                yield break;
            }
        }

        private IEnumerable<ICEOptionData> CaptiveList
        {
            get
            {
                //yield return new CEManagedBooleanOptionData("captiveToggle", "Captive Events", "", 1f);
                yield break;
            }
        }

        private IEnumerable<ICEOptionData> CaptorList
        {
            get
            {
                //yield return new CEManagedBooleanOptionData("captorToggle", "Captor Events", "", 1f);
                yield break;
            }
        }

        private IEnumerable<ICEOptionData> RandomList
        {
            get
            {
                //yield return new CEManagedBooleanOptionData("randomToggle", "Random Events", "", 1f);
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

        public void SetConfig(ICEOptionData data, float value)
        {

        }

        private void ExecuteDone() => ScreenManager.PopScreen();

        private void ExecuteCancel() => ScreenManager.PopScreen();


        public CESettingsVMCategory GeneralOptions { get; }

        public CESettingsVMCategory CaptiveOptions { get; }

        public CESettingsVMCategory CaptorOptions { get; }

        public CESettingsVMCategory RandomOptions { get; }

        public CESettingsVMCategory EventsListOptions { get; }

        public CESettingsVMCategory CustomFlagsOptions { get; }


    }
}
