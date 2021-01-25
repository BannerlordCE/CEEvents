using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace CaptivityEvents.Config
{
    internal class CESettingsScreen : ScreenBase
    {

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _viewModel = new CESettingsVM();
            _gauntletLayer = new GauntletLayer(1, "GauntletLayer");
            _gauntletLayer.LoadMovie("CaptivityEventsConfigScreen", _viewModel);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            base.AddLayer(_gauntletLayer);
        }

        protected override void OnFinalize()
        {
            base.OnFinalize();
            base.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
            _viewModel = null;
        }

        public CESettingsScreen()
        {
        }

        private GauntletLayer _gauntletLayer;

        private CESettingsVM _viewModel;
    }
}
