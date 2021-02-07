using System;
using System.Collections.Generic;
using TaleWorlds.Engine.Options;

namespace CaptivityEvents.Config
{

    public interface ICEOptionData
    {
        float GetDefaultValue();

        void Commit();

        float GetValue(bool forceRefresh);

        bool SetValue(float value);

        string GetName();

        string GetDescription();

        string GetId();
    }

    public interface ICENumericOptionData : ICEOptionData
    {
        float GetMinValue();

        float GetMaxValue();

        bool GetIsDiscrete();

        bool GetShouldUpdateContinuously();
    }

    public interface ICEBooleanOptionData : ICEOptionData
    {
    }

    public interface ICESelectionOptionData : ICEOptionData
    {
        int GetSelectableOptionsLimit();

        IEnumerable<SelectionData> GetSelectableOptionNames();
    }

    public abstract class CEManagedOptionData : ICEOptionData
    {
        protected CEManagedOptionData(string id, string name, float defaultValue = 0.0f, string description = "", Func<float, float> onChange = null)
        {
            _id = id;
            _name = name;
            _description = description;
            _value = defaultValue;
            _defaultValue = defaultValue;
            _onChange = onChange;
        }

        public virtual float GetDefaultValue() => _defaultValue;

        public void Commit()
        {
        }

        public float GetValue(bool forceRefresh)
        {
            if (forceRefresh)
            {

            }
            return _value;
        }

        public bool SetValue(float value) {
            float oldValue = _value;
            _value = _onChange(value);
            return oldValue != _value;
        }

        public string GetName() => _name;

        public string GetDescription() => _description;

        public string GetId() => _name;

        private readonly string _id;
        internal string _name;
        private readonly string _description;
        private float _value;
        private readonly float _defaultValue;

        private readonly Func<float,float> _onChange;
    }

    public class CEActionOptionData : ICEOptionData
    {
        public Action OnAction { get; private set; }

        public CEActionOptionData(string id, string name, Action onAction)
        {
            _id = id;
            _name = name;
            OnAction = onAction;
        }

        public void Commit()
        {
        }

        public float GetDefaultValue() => 0f;

        public float GetValue(bool forceRefresh) => 0f;

        public string GetName() => _name;

        public string GetId() => _name;

        public string GetDescription() => "";

        public bool SetValue(float value)
        {
            return true;
        }

        private readonly string _id;
        internal string _name;
    }

    public class CEManagedNumericOptionData : CEManagedOptionData, ICENumericOptionData, ICEOptionData
    {
        public CEManagedNumericOptionData(string id, string name, string description, float defaultValue, Func<float, float> onChange, float min, float max, bool discrete = true, bool updateContinuously = false) : base(id, name, defaultValue, description, onChange)
        {
            _minValue = min;
            _maxValue = max;
            _discrete = discrete;
            _updateContinuously = updateContinuously;
        }

        public float GetMinValue() => _minValue;

        public float GetMaxValue() => _maxValue;

        public bool GetIsDiscrete() => _discrete;

        public bool GetShouldUpdateContinuously() => _updateContinuously;

        private readonly float _minValue;
        private readonly float _maxValue;
        private readonly bool _discrete;
        private readonly bool _updateContinuously;
    }

    public class CEManagedBooleanOptionData : CEManagedOptionData, ICEBooleanOptionData, ICEOptionData
    {
        public CEManagedBooleanOptionData(string id, string name, string description, float defaultValue, Func<float, float> onChange) : base(id, name, defaultValue, description, onChange)
        {
        }
    }

    public class CEManagedSelectionOptionData : CEManagedOptionData, ICESelectionOptionData, ICEOptionData
    {
        public CEManagedSelectionOptionData(string id, string name, string description, float defaultValue, Func<float, float> onChange, int limit, IEnumerable<SelectionData> names) : base(id, name, defaultValue, description, onChange)
        {
            _selectableOptionsLimit = limit;
            _selectableOptionNames = names;
        }

        public int GetSelectableOptionsLimit() => _selectableOptionsLimit;

        public IEnumerable<SelectionData> GetSelectableOptionNames() => _selectableOptionNames;

        private readonly int _selectableOptionsLimit;

        private readonly IEnumerable<SelectionData> _selectableOptionNames;
    }


}
