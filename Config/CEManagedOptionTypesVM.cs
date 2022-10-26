#define V100
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

using TaleWorlds.Core.ViewModelCollection.Selector;

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
                    base.OnPropertyChangedWithValue(value, "Description");
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
                    base.OnPropertyChangedWithValue(value, "Name");
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
                    base.OnPropertyChangedWithValue(value, "ImageIDs");
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
                    base.OnPropertyChangedWithValue(value, "OptionTypeID");
                }
            }
        }

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
                    base.OnPropertyChangedWithValue(value, "OptionValueAsBoolean");
                    UpdateValue();
                }
            }
        }

        public override void UpdateValue()
        {
            if (!Option.SetValue(OptionValueAsBoolean ? 1 : 0))
            {
                Option.Commit();
                _optionsVM.SetConfig(Option, OptionValueAsBoolean ? 1 : 0);
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
            _initialValue = _numericOptionData.GetValue(false);
            Min = _numericOptionData.GetMinValue();
            Max = _numericOptionData.GetMaxValue();
            IsDiscrete = _numericOptionData.GetIsDiscrete();
            UpdateContinuously = _numericOptionData.GetShouldUpdateContinuously();
            OptionValue = _initialValue;
        }

        [DataSourceProperty]
        public float Min
        {
            get => _min;
            set
            {
                if (value != _min)
                {
                    _min = value;
                    base.OnPropertyChangedWithValue(value, "Min");
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
                    base.OnPropertyChangedWithValue(value, "Max");
                }
            }
        }

        [DataSourceProperty]
        public float OptionValue
        {
            get => _optionValue;
            set
            {
                if (value != _optionValue)
                {
                    _optionValue = value;
                    base.OnPropertyChangedWithValue(value, "OptionValue");
                    base.OnPropertyChanged("OptionValueAsString");
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
                    base.OnPropertyChangedWithValue(value, "IsDiscrete");
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
                    base.OnPropertyChangedWithValue(value, "UpdateContinuously");
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

        public override void UpdateValue()
        {
            if (!Option.SetValue(OptionValue)) return;
            Option.Commit();
            _optionsVM.SetConfig(Option, OptionValue);
        }

        public override void Cancel()
        {
            OptionValue = _initialValue;
            UpdateValue();
        }

        public override void SetValue(float value) => OptionValue = value;

        public override void ResetData() => OptionValue = Option.GetDefaultValue();

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
            if (selectableOptionNames.Any<SelectionData>())
            {
                if (selectableOptionNames.All((SelectionData n) => n.IsLocalizationId))
                {
                    List<TextObject> list = new();
                    foreach (SelectionData selectionData in selectableOptionNames)
                    {
                        TextObject item = Module.CurrentModule.GlobalTextManager.FindText(selectionData.Data, null);
                        list.Add(item);
                    }
                    Selector.Refresh(list, (int)Option.GetValue(!initalUpdate), onChange);
                    goto IL_183;
                }
            }
            List<string> list2 = new();
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
                Option.Commit();
                _optionsVM.SetConfig(Option, Selector.SelectedIndex);
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
                    base.OnPropertyChangedWithValue(value, "Selector");
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
                    base.OnPropertyChangedWithValue(value, "ActionName");
                }
            }
        }

        private readonly Action _onAction;

        private readonly TextObject _optionActionName;

        private string _actionName;
    }
}