using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace CaptivityEvents.Config
{
    public class CESettingsVMCategory : ViewModel
    {
        public CESettingsVMCategory(CESettingsVM options, TextObject name, IEnumerable<ICEOptionData> targetList, bool isNative)
        {
            _options = [];
            IsNative = isNative;
            _nameObj = name;
            foreach (ICEOptionData optionData in targetList)
            {
                string text = optionData.GetName().ToString();
                TextObject name2 = new(text);
                TextObject textObject = new(text);
                textObject.SetTextVariable("newline", "\n");
                CEActionOptionData actionOptionData;
                if (optionData is ICEBooleanOptionData)
                {
                    CEBooleanOptionDataVM booleanOptionDataVM = new(options, optionData as ICEBooleanOptionData, name2, textObject)
                    {
                        ImageIDs =
                    [
                        text + "_0",
                        text + "_1"
                    ]
                    };
                    _options.Add(booleanOptionDataVM);
                }
                else if (optionData is ICENumericOptionData)
                {
                    CENumericOptionDataVM item = new(options, optionData as ICENumericOptionData, name2, textObject);
                    _options.Add(item);
                }
                else if (optionData is ICESelectionOptionData)
                {
                    ICESelectionOptionData selectionOptionData = optionData as ICESelectionOptionData;
                    CEStringOptionDataVM stringOptionDataVM = new(options, selectionOptionData, name2, textObject);
                    string[] array = new string[selectionOptionData.GetSelectableOptionsLimit()];
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = text + "_" + i;
                    }
                    stringOptionDataVM.ImageIDs = array;
                    _options.Add(stringOptionDataVM);
                }
                else if ((actionOptionData = (optionData as CEActionOptionData)) != null)
                {
                    TextObject optionActionName = Module.CurrentModule.GlobalTextManager.FindText("str_options_type_action", text);
                    CEActionOptionDataVM item2 = new(actionOptionData.OnAction, options, actionOptionData, name2, optionActionName, textObject);
                    _options.Add(item2);
                }
            }
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name = _nameObj.ToString();
            Options.ApplyActionOnAllItems(delegate (CEGenericOptionDataVM x)
            {
                x.RefreshValues();
            });
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
        public MBBindingList<CEGenericOptionDataVM> Options
        {
            get => _options;
            set
            {
                if (value != _options)
                {
                    _options = value;
                    OnPropertyChangedWithValue(value, "Options");
                }
            }
        }

        // Needed by OptionsGroupedPage: exposes the base (ungrouped) option list.
        [DataSourceProperty]
        public MBBindingList<CEGenericOptionDataVM> BaseOptions
        {
            get => _options;
            set
            {
                if (value != _options)
                {
                    _options = value;
                    OnPropertyChangedWithValue(value, "BaseOptions");
                    // Keep Options in sync
                    OnPropertyChangedWithValue(value, "Options");
                }
            }
        }

        // Group support (currently unused). Required by OptionsGroupedPage; kept empty so page renders BaseOptions panel only.
        [DataSourceProperty]
        public MBBindingList<CEOptionGroupVM> Groups
        {
            get => _groups;
            set
            {
                if (value != _groups)
                {
                    _groups = value;
                    OnPropertyChangedWithValue(value, "Groups");
                }
            }
        }

        private readonly TextObject _nameObj;

        public readonly bool IsNative;

        private string _name;

        private MBBindingList<CEGenericOptionDataVM> _options;
        private MBBindingList<CEOptionGroupVM> _groups = new();
    }
}