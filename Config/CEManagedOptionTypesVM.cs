using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace CaptivityEvents.Config
{
    public abstract class CEGenericOptionDataVM : ViewModel
    {
        protected CEGenericOptionDataVM(CESettingsVM optionsVM, ICEOptionData option, TextObject name, TextObject description, OptionsVM.OptionsDataType typeID)
        {
            _nameObj = name;
            _descriptionObj = description;
            _optionsVM = optionsVM;
            Option = option;
            OptionTypeID = (int)typeID;
            RefreshValues();
        }

        public virtual void UpdateData(bool initUpdate)
        {
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name = _nameObj.ToString();
            Description = _descriptionObj.ToString();
        }

        public object GetOptionType() => Option.GetId();

        public ICEOptionData GetOptionData() => Option;

        [DataSourceProperty]
        public string Description
        {
            get => _description;
            set
            {
                if (value != _description)
                {
                    _description = value;
                    OnPropertyChangedWithValue(value, "Description");
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public string[] ImageIDs
        {
            get => _imageIDs;
            set
            {
                if (value != _imageIDs)
                {
                    _imageIDs = value;
                    OnPropertyChangedWithValue(value, "ImageIDs");
                }
            }
        }

        [DataSourceProperty]
        public int OptionTypeID
        {
            get => _optionTypeId;
            set
            {
                if (value != _optionTypeId)
                {
                    _optionTypeId = value;
                    OnPropertyChangedWithValue(value, "OptionTypeID");
                }
            }
        }

        // Enable interaction for OptionItemWidget (IsOptionEnabled="@IsEnabled")
        [DataSourceProperty]
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; }
        }

        protected bool _isEnabled = true;

        // Some native templates may bind IsOptionEnabled instead of IsEnabled.
        [DataSourceProperty]
        public bool IsOptionEnabled => IsEnabled;

        public abstract void UpdateValue();

        public abstract void Cancel();

        public abstract bool IsChanged();

        public abstract void SetValue(float value);

        public abstract void ResetData();

        private readonly TextObject _nameObj;

        private readonly TextObject _descriptionObj;

        protected CESettingsVM _optionsVM;

        protected ICEOptionData Option;

        private string _description;

        private string _name;

        private int _optionTypeId = -1;

        private string[] _imageIDs;
    }

    public class CEBooleanOptionDataVM : CEGenericOptionDataVM
    {
        public CEBooleanOptionDataVM(CESettingsVM optionsVM, ICEBooleanOptionData option, TextObject name, TextObject description) : base(optionsVM, option, name, description, OptionsVM.OptionsDataType.BooleanOption)
        {
            _booleanOptionData = option;
            _initialValue = option.GetValue(false).Equals(1f);
            OptionValueAsBoolean = _initialValue;
        }

        [DataSourceProperty]
        public bool OptionValueAsBoolean
        {
            get => _optionValue;
            set
            {
                if (value != _optionValue)
                {
                    _optionValue = value;
                    OnPropertyChangedWithValue(value, "OptionValueAsBoolean");
                    UpdateValue();
                }
            }
        }

        public override void UpdateValue()
        {
            int newVal = OptionValueAsBoolean ? 1 : 0;
            if ((int)Option.GetValue(false) != newVal)
            {
                Option.SetValue(newVal);
                Option.Commit();
                _optionsVM.SetConfig(Option, newVal);
            }
        }

        public override void Cancel()
        {
            OptionValueAsBoolean = _initialValue;
            UpdateValue();
        }

        public override void SetValue(float value) => OptionValueAsBoolean = ((int)value == 1);

        public override void ResetData() => OptionValueAsBoolean = ((int)Option.GetDefaultValue() == 1);

        public override bool IsChanged() => _initialValue != OptionValueAsBoolean;

        private readonly bool _initialValue;

        private readonly ICEBooleanOptionData _booleanOptionData;

        private bool _optionValue;
    }

    public class CENumericOptionDataVM : CEGenericOptionDataVM
    {
        public CENumericOptionDataVM(CESettingsVM optionsVM, ICENumericOptionData option, TextObject name, TextObject description) : base(optionsVM, option, name, description, OptionsVM.OptionsDataType.NumericOption)
        {
            _numericOptionData = option;
            _min = _numericOptionData.GetMinValue();
            _max = _numericOptionData.GetMaxValue();
            // Ensure sane defaults if data source returns invalid values
            if (_min >= _max || _min < -1e10f || _max > 1e10f)
            {
                _min = 0f;
                _max = 100f;
            }
            _initialValue = _numericOptionData.GetValue(false);
            // Clamp initial value to range
            if (_initialValue < _min) _initialValue = _min;
            if (_initialValue > _max) _initialValue = _max;
            _optionValue = _initialValue;
            IsDiscrete = _numericOptionData.GetIsDiscrete();
            UpdateContinuously = _numericOptionData.GetShouldUpdateContinuously();
            _isEnabled = false; // Disabled: use manual XML editing
        }

        // Some bundled widget templates may expect OptionMin/OptionMax rather than Min/Max.
        [DataSourceProperty]
        public float OptionMin => Min;

        [DataSourceProperty]
        public float OptionMax => Max;

        [DataSourceProperty]
        public float Min
        {
            get => _min;
            set
            {
                if (value != _min)
                {
                    _min = value;
                    OnPropertyChangedWithValue(value, "Min");
                }
            }
        }

        [DataSourceProperty]
        public float Max
        {
            get => _max;
            set
            {
                if (value != _max)
                {
                    _max = value;
                    OnPropertyChangedWithValue(value, "Max");
                }
            }
        }

        [DataSourceProperty]
        public float OptionValue
        {
            get => _optionValue;
            set
            {
                float clamped = value;
                if (clamped < Min) clamped = Min;
                if (clamped > Max) clamped = Max;
                if (clamped != _optionValue)
                {
                    _optionValue = clamped;
                    OnPropertyChangedWithValue(clamped, "OptionValue");
                    OnPropertyChanged("OptionValueAsString");
                    OnPropertyChanged("NormalizedValue");
                    OnPropertyChanged("Value");
                    UpdateValue();
                }
            }
        }

        [DataSourceProperty]
        public bool IsDiscrete
        {
            get => _isDiscrete;
            set
            {
                if (value != _isDiscrete)
                {
                    _isDiscrete = value;
                    OnPropertyChangedWithValue(value, "IsDiscrete");
                }
            }
        }

        [DataSourceProperty]
        public bool UpdateContinuously
        {
            get => _updateContinuously;
            set
            {
                if (value != _updateContinuously)
                {
                    _updateContinuously = value;
                    OnPropertyChangedWithValue(value, "UpdateContinuously");
                }
            }
        }

        [DataSourceProperty]
        public string OptionValueAsString
        {
            get
            {
                if (!IsDiscrete)
                {
                    return _optionValue.ToString("F");
                }
                return ((int)_optionValue).ToString();
            }
        }

        // Alias often used in UI text bindings.
        [DataSourceProperty]
        public string ValueText => OptionValueAsString;

        // Step suggestion for discrete sliders.
        [DataSourceProperty]
        public float StepSize
        {
            get
            {
                if (IsDiscrete)
                {
                    return 1f;
                }
                // Provide a reasonable granularity.
                float span = Max - Min;
                return span <= 0 ? 0.1f : span / 100f;
            }
        }

        public override void UpdateValue()
        {
            float newVal = IsDiscrete ? (float)Math.Round(OptionValue) : OptionValue;
            if (newVal < Min) newVal = Min;
            if (newVal > Max) newVal = Max;
            if (Option.SetValue(newVal))
            {
                Option.Commit();
                _optionsVM.SetConfig(Option, newVal);
            }
        }

        public override void Cancel()
        {
            OptionValue = _initialValue;
            UpdateValue();
        }

        public override void SetValue(float value) => OptionValue = value;
        
        public override void ResetData() => OptionValue = Option.GetDefaultValue();

        // Additional alias properties for various slider templates.
        [DataSourceProperty]
        public float MinValue => Min;

        [DataSourceProperty]
        public float MaxValue => Max;

        [DataSourceProperty]
        public float RangeMin => Min;

        [DataSourceProperty]
        public float RangeMax => Max;

        [DataSourceProperty]
        public float Value
        {
            get => OptionValue;
            set => OptionValue = value;
        }

        // Additional common alias names some templates may reference
        [DataSourceProperty]
        public float OptionCurrentValue
        {
            get => OptionValue;
            set => OptionValue = value;
        }

        [DataSourceProperty]
        public float CurrentValue
        {
            get => OptionValue;
            set => OptionValue = value;
        }

        [DataSourceProperty]
        public float Current => OptionValue;

        [DataSourceProperty]
        public float OptionMinValue => Min;

        [DataSourceProperty]
        public float OptionMaxValue => Max;

        [DataSourceProperty]
        public float LowerBound => Min;

        [DataSourceProperty]
        public float UpperBound => Max;

        [DataSourceProperty]
        public float NormalizedValue
        {
            get
            {
                float span = Max - Min;
                if (span <= 0.00001f) return 0f;
                return (OptionValue - Min) / span;
            }
            set
            {
                float span = Max - Min;
                if (span <= 0.00001f) return;
                float v = value;
                if (v < 0f) v = 0f; else if (v > 1f) v = 1f;
                OptionValue = Min + (v * span);
            }
        }

        public override bool IsChanged() => _initialValue != OptionValue;

        private readonly float _initialValue;

        private readonly ICENumericOptionData _numericOptionData;

        private float _min;

        private float _max;

        private float _optionValue;

        private bool _isDiscrete;

        private bool _updateContinuously;
    }

    public class CEStringOptionDataVM : CEGenericOptionDataVM
    {
        public CEStringOptionDataVM(CESettingsVM optionsVM, ICESelectionOptionData option, TextObject name, TextObject description) : base(optionsVM, option, name, description, OptionsVM.OptionsDataType.MultipleSelectionOption)
        {
            Selector = new SelectorVM<SelectorItemVM>(0, null);
            _selectionOptionData = option;
            UpdateData(true);
            _initialValue = (int)Option.GetValue(false);
            Selector.SelectedIndex = _initialValue;
        }

        public override void UpdateData(bool initalUpdate)
        {
            base.UpdateData(initalUpdate);
            IEnumerable<SelectionData> selectableOptionNames = _selectionOptionData.GetSelectableOptionNames();
            Selector.SetOnChangeAction(null);
            bool flag = (int)Option.GetValue(true) != Selector.SelectedIndex;
            Action<SelectorVM<SelectorItemVM>> onChange = null;
            if (flag)
            {
                onChange = new Action<SelectorVM<SelectorItemVM>>(UpdateValue);
            }
            if (selectableOptionNames.Any())
            {
                if (selectableOptionNames.All((SelectionData n) => n.IsLocalizationId))
                {
                    List<TextObject> list = [];
                    foreach (SelectionData selectionData in selectableOptionNames)
                    {
                        TextObject item = Module.CurrentModule.GlobalTextManager.FindText(selectionData.Data, null);
                        list.Add(item);
                    }
                    Selector.Refresh(list, (int)Option.GetValue(!initalUpdate), onChange);
                    goto IL_183;
                }
            }
            List<string> list2 = [];
            foreach (SelectionData selectionData2 in selectableOptionNames)
            {
                if (selectionData2.IsLocalizationId)
                {
                    TextObject textObject = Module.CurrentModule.GlobalTextManager.FindText(selectionData2.Data, null);
                    list2.Add(textObject.ToString());
                }
                else
                {
                    list2.Add(selectionData2.Data);
                }
            }
            Selector.Refresh(list2, (int)Option.GetValue(!initalUpdate), onChange);
        IL_183:
            if (!flag)
            {
                Selector.SetOnChangeAction(new Action<SelectorVM<SelectorItemVM>>(UpdateValue));
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            SelectorVM<SelectorItemVM> selector = Selector;
            if (selector == null)
            {
                return;
            }
            selector.RefreshValues();
        }

        public void UpdateValue(SelectorVM<SelectorItemVM> selector)
        {
            if (selector.SelectedIndex >= 0)
            {
                if (!Option.SetValue(selector.SelectedIndex)) return;
                Option.Commit();
                _optionsVM.SetConfig(Option, selector.SelectedIndex);
            }
        }

        public override void UpdateValue()
        {
            if (Selector.SelectedIndex >= 0 && Selector.SelectedIndex != Option.GetValue(false))
            {
                if (Option.SetValue(Selector.SelectedIndex))
                {
                    Option.Commit();
                    _optionsVM.SetConfig(Option, Selector.SelectedIndex);
                }
            }
        }

        public override void Cancel()
        {
            Selector.SelectedIndex = _initialValue;
            UpdateValue();
        }

        public override void SetValue(float value) => Selector.SelectedIndex = (int)value;

        public override void ResetData() => Selector.SelectedIndex = (int)Option.GetDefaultValue();

        public override bool IsChanged() => _initialValue != Selector.SelectedIndex;

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> Selector
        {
            get
            {
                SelectorVM<SelectorItemVM> selector = _selector;
                return selector;
            }
            set
            {
                if (value != _selector)
                {
                    _selector = value;
                    OnPropertyChangedWithValue(value, "Selector");
                }
            }
        }

        private readonly int _initialValue;

        private readonly ICESelectionOptionData _selectionOptionData;

        public SelectorVM<SelectorItemVM> _selector;
    }

    public class CEActionOptionDataVM : CEGenericOptionDataVM
    {
        public CEActionOptionDataVM(Action onAction, CESettingsVM optionsVM, ICEOptionData option, TextObject name, TextObject optionActionName, TextObject description) : base(optionsVM, option, name, description, OptionsVM.OptionsDataType.ActionOption)
        {
            _onAction = onAction;
            _optionActionName = optionActionName;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (_optionActionName != null)
            {
                ActionName = _optionActionName.ToString();
            }
        }

        private void ExecuteAction()
        {
            Action onAction = _onAction;
            if (onAction == null)
            {
                return;
            }
            onAction.DynamicInvokeWithLog(Array.Empty<object>());
        }

        public override void Cancel()
        {
        }

        public override bool IsChanged() => false;

        public override void ResetData()
        {
        }

        public override void SetValue(float value)
        {
        }

        public override void UpdateValue()
        {
        }

        [DataSourceProperty]
        public string ActionName
        {
            get => _actionName;
            set
            {
                if (value != _actionName)
                {
                    _actionName = value;
                    OnPropertyChangedWithValue(value, "ActionName");
                }
            }
        }

        private readonly Action _onAction;

        private readonly TextObject _optionActionName;

        private string _actionName;
    }
}