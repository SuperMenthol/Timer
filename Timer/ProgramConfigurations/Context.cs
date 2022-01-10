using MainSpace.Controllers;
using MainSpace.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace MainSpace.ProgramConfigurations
{
    public sealed class Context : INotifyPropertyChanged
    {
        public static Context GetContext()
        {
            if (_instance == null)
            {
                _instance = new Context();
            }

            return _instance;
        }

        public bool CanChangeTimer
        {
            get { return _canChangeTimer; }
            set 
            { 
                _canChangeTimer = value; 
                OnPropertyChanged(nameof(CanChangeTimer)); 
            }
        }

        public TimerConfiguration CurrentConfiguration
        {
            get { return ConfigurationController.UsedConfiguration; }
            set 
            { 
                _currentConfiguration = value; 
                OnPropertyChanged(nameof(CurrentConfiguration)); 
            }
        }

        public List<TimerConfiguration> AllConfigurations
        {
            get
            {
                return ConfigurationController.GetAllConfigurations();
            }
            set
            {
                _allConfigurations = value; 
                OnPropertyChanged(nameof(AllConfigurations));
            }
        }

        public static List<string> TimeComboBoxValues { get; set; } = TimeComboBoxValue.Values;

        public event PropertyChangedEventHandler? PropertyChanged;

        private static Context _instance;
        private static bool _canChangeTimer = true;
        private static TimerConfiguration _currentConfiguration;
        private static List<TimerConfiguration> _allConfigurations;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
