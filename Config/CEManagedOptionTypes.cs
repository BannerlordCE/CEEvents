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

    public interface ICEBooleanOptionData : ICEOptionData { }

    public interface ICESelectionOptionData : ICEOptionData
    {
        int GetSelectableOptionsLimit();

        IEnumerable<SelectionData> GetSelectableOptionNames();
    }

    public abstract class CEManagedOptionData(string id, string name, float defaultValue = 0.0f, string description = "", Func<float, float> onChange = null) : ICEOptionData
    {
        public virtual float GetDefaultValue() => _defaultValue;

        public void Commit() { }

        public float GetValue(bool forceRefresh)
        {
            if (forceRefresh) { }

            return _value;
        }

        public bool SetValue(float value)
        {
            float oldValue = _value;
            _value = onChange(value);

            return Math.Abs(oldValue - _value) > 0.001f;
        }

        public string GetName() => Name;

        public string GetDescription() => description;

        public string GetId() => Name;

        private readonly string _id = id;
        internal string Name = name;
        private float _value = defaultValue;
        private readonly float _defaultValue = defaultValue;
    }

    public class CEActionOptionData(string id, string name, Action onAction) : ICEOptionData
    {
        public Action OnAction { get; private set; } = onAction;

        public void Commit() { }

        public float GetDefaultValue() => 0f;

        public float GetValue(bool forceRefresh) => 0f;

        public string GetName() => Name;

        public string GetId() => Name;

        public string GetDescription() => "";

        public bool SetValue(float value) => true;

        private readonly string _id = id;
        internal string Name = name;
    }

    public class CEManagedNumericOptionData : CEManagedOptionData, ICENumericOptionData
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

    public class CEManagedBooleanOptionData(string id, string name, string description, float defaultValue, Func<float, float> onChange) : CEManagedOptionData(id, name, defaultValue, description, onChange), ICEBooleanOptionData;

    public class CEManagedSelectionOptionData(string id, string name, string description, float defaultValue, Func<float, float> onChange, int limit, IEnumerable<SelectionData> names) : CEManagedOptionData(id, name, defaultValue, description, onChange), ICESelectionOptionData
    {
        public int GetSelectableOptionsLimit() => limit;

        public IEnumerable<SelectionData> GetSelectableOptionNames() => names;
    }
}