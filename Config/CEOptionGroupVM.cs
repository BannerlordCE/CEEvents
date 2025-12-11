using TaleWorlds.Library;

namespace CaptivityEvents.Config
{
    public class CEOptionGroupVM : ViewModel
    {
        private MBBindingList<CEGenericOptionDataVM> _options = new();
        private string _name;

        public CEOptionGroupVM(string name)
        {
            Name = name;
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
    }
}
