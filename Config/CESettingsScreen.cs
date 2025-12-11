using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace CaptivityEvents.Config
{
    internal class CESettingsScreen : ScreenBase
    {
        private const string SettingsFilePath = "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CESettings.xml";

        private XDocument _settingsDocument;
        private string _fullSettingsPath;
        private CESettingsVM _viewModel;
        private GauntletLayer _gauntletLayer;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _viewModel = new CESettingsVM();
            LoadSettings();
            _gauntletLayer = new GauntletLayer("GauntletLayer", 1);
            _gauntletLayer.LoadMovie("CaptivityEventsConfigScreen", _viewModel);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            AddLayer(_gauntletLayer);
        }

        private void LoadSettings()
        {
            try
            {
                _fullSettingsPath = Path.Combine(BasePath.Name, SettingsFilePath);

                if (!File.Exists(_fullSettingsPath))
                {
                    InformationManager.DisplayMessage(new InformationMessage($"Settings file not found: {_fullSettingsPath}", Colors.Red));
                    return;
                }

                _settingsDocument = XDocument.Load(_fullSettingsPath);
                var root = _settingsDocument.Root;

                if (root == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Invalid CESettings.xml structure.", Colors.Red));
                    return;
                }

                ParseSettingsToViewModel(root);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Error loading settings: {ex.Message}", Colors.Red));
            }
        }

        private void ParseSettingsToViewModel(XElement root)
        {
            foreach (var element in root.Elements())
            {
                string name = element.Name.LocalName;
                string value = element.Value;

                // Try to set the value on the ViewModel using reflection
                var property = _viewModel.GetType().GetProperty(name);
                if (property != null && property.CanWrite)
                {
                    try
                    {
                        if (property.PropertyType == typeof(bool))
                        {
                            if (bool.TryParse(value, out var boolValue))
                                property.SetValue(_viewModel, boolValue);
                        }
                        else if (property.PropertyType == typeof(int))
                        {
                            if (int.TryParse(value, out var intValue))
                                property.SetValue(_viewModel, intValue);
                        }
                        else if (property.PropertyType == typeof(float))
                        {
                            if (float.TryParse(value, out var floatValue))
                                property.SetValue(_viewModel, floatValue);
                        }
                    }
                    catch { }
                }
            }
        }

        public void SaveSettings()
        {
            try
            {
                if (_settingsDocument == null)
                    return;

                // Update XML from ViewModel properties
                foreach (var prop in _viewModel.GetType().GetProperties())
                {
                    var element = _settingsDocument.Root?.Element(prop.Name);
                    if (element != null)
                    {
                        var value = prop.GetValue(_viewModel);
                        if (value != null)
                            element.Value = value.ToString();
                    }
                }

                _settingsDocument.Save(_fullSettingsPath);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Error saving settings: {ex.Message}", Colors.Red));
            }
        }

        protected override void OnFinalize()
        {
            SaveSettings();
            base.OnFinalize();
            if (_gauntletLayer != null)
            {
                RemoveLayer(_gauntletLayer);
                _gauntletLayer = null;
            }
            _viewModel = null;
        }
    }
}