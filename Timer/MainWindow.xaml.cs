using MainSpace.Controllers;
using MainSpace.ProgramConfigurations;
using MainSpace.Enums;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace MainSpace
{
    public partial class MainWindow : Window
    {
        private bool _started = false;

        private int _uptimeMultiplier = 1;
        private int _downtimeMultiplier = 1;
        private int _restMultiplier = 1;

        public MainWindow()
        {
            this.DataContext = Context.GetContext();
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ResetBtn_Click(this, new RoutedEventArgs());
            if (e.AddedItems.Count > 0) LoadTimer((TimerConfiguration)ModeBox.SelectedItem);
        }

        public void LoadTimer(TimerConfiguration configuration)
        {
            TimerManager.LoadTimer(configuration);
            ConfigurationController.UsedConfiguration = configuration;

            SeriesBox.Value = configuration.SeriesInCycle;
            CycleBox.Value = configuration.Cycles;

            ActiveTimeBox.Value = configuration.UptimeInSeconds;
            DowntimeBox.Value = configuration.DowntimeInSeconds;
            CycleRestBox.Value = configuration.RestBetweenCyclesInSeconds;
        }

        private void DisplayFormatPrompt()
        {
            string prompt = StartMode.SelectedIndex == 0 ?
                "Wprowadź czas w formacie hh:mm" :
                "Wprowadź liczbę";

            ErrorPrompt.Text = prompt;
            ErrorPrompt.Visibility = Visibility.Visible;
        }

        private void HideFormatPrompt()
        {
            ErrorPrompt.Visibility = Visibility.Hidden;
        }

        private TimerConfiguration GetConfigurationFromUI()
        {
            var newConfiguration = new TimerConfiguration()
            {
                TimerType = Context.GetContext().CurrentConfiguration.TimerType,
                SeriesInCycle = (int)SeriesBox.Value,
                Cycles = (int)CycleBox.Value,
                UptimeInSeconds = (int)ActiveTimeBox.Value * _uptimeMultiplier,
                DowntimeInSeconds = (int)DowntimeBox.Value * _downtimeMultiplier,
                RestBetweenCyclesInSeconds = (int)CycleRestBox.Value * _restMultiplier
            };

            ConfigurationController.UsedConfiguration = newConfiguration;
            return newConfiguration;
        }

        #region control-handlers

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!TimerManager.IsTimerSet && (bool)StartAt.IsChecked)
            {
                TimerManager.LoadTimer(CountdownController.GetCountdownConfiguration(StartMode.SelectedIndex == 0, 
                    StartAtTbx.Text, 
                    StartInTimeBox.IsEnabled ? StartInTimeBox.SelectedIndex == 0 : null));
            }
            else if (!TimerManager.IsTimerSet)
            {
                TimerManager.LoadTimer(GetConfigurationFromUI());
            }

            _started = !_started;

            ActionBtn.Content = !_started ? "Start" : "Pauza";
            TimerManager.PauseUnpause(_started);
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            TimerManager.Reset();
            StartAt.IsEnabled = true;
            _started = false;

            ActionBtn.Content = "Start";
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var newConfiguration = GetConfigurationFromUI();

            var saveBox = new SavePrompt(newConfiguration);
            saveBox.ShowDialog();
            if ((bool)saveBox.DialogResult)
            {
                newConfiguration.Name = saveBox.NameBox.Text;
                ConfigurationController.SaveConfiguration(newConfiguration);
            }
        }

        private void DowntimeUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _downtimeMultiplier = DowntimeUnit.SelectedIndex == 0 ? 1 : 60;
        }

        private void RestUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _restMultiplier = RestUnit.SelectedIndex == 0 ? 1 : 60;
        }

        private void ActiveTimeUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _uptimeMultiplier = ActiveTimeUnit.SelectedIndex == 0 ? 1 : 60;
        }

        private void StartAtTbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!CountdownController.ValidateTextBox(StartMode.SelectedIndex == 0, StartAtTbx.Text))
            {
                DisplayFormatPrompt();
            }
        }

        private void StartAtTbx_GotFocus(object sender, RoutedEventArgs e)
        {
            HideFormatPrompt();
        }

        private void StartAt_Checked(object sender, RoutedEventArgs e)
        {
            if (!(bool)StartAt.IsChecked) HideFormatPrompt();
        }
        #endregion control-handlers
    }

    internal static class TimerManager
    {
        public static TimerConfiguration Configuration;
        private static Timer _timer;
        private static Context _context;

        public static bool IsTimerSet
        {
            get { return _isTimerSet; }
            private set 
            { 
                _isTimerSet = value;
                _context.CanChangeTimer = !value;
            }
        }

        private static bool _isTimerSet = false;

        private static TimeOnly _startTime;
        private static TimeOnly _targetTime;

        private static TimeSpan _timeLeft;

        private static TimeSpan _activeTime = TimeSpan.Zero;

        private static ActionType _currentAction;
        private static int _actionCount;
        private static int _cyclesCount;

        static TimerManager()
        {
            Configuration = new TimerConfiguration();
            _timer = new Timer() { AutoReset = true, Enabled = false };
            _context = Context.GetContext();

            MainTextBoxController._mainBox = (TextBox)Application.Current.MainWindow.FindName("MainTimeBox");
            MainTextBoxController.mainWindowDispatcher = Application.Current.MainWindow.Dispatcher;
        }

        public static void LoadTimer(TimerConfiguration configuration)
        {
            Configuration = configuration;
            ReapplyElapsedEvent();
        }

        public static void PauseUnpause(bool running = true)
        {
            if (!_isTimerSet) IsTimerSet = true;

            _timer.Enabled = running;

            if (_activeTime == TimeSpan.Zero)
            {
                MainTextBoxController.SetBrush(ActionType.Action);
                GetSecondsToTarget(ActionType.Action);
            }

            CalculateTargetTime(true);
        }

        public static void Reset()
        {
            _timer.Stop();

            ZeroVariables();
            MainTextBoxController.Reset();

            if (Configuration.TimerType == (int)TimerType.Countdown)
            {
                LoadTimer(ConfigurationController.UsedConfiguration);

                _currentAction = ActionType.Action;
                MainTextBoxController.SetBrush(_currentAction);

                GetSecondsToTarget(_currentAction);
                CalculateTargetTime(false);

                _timer.Start();
            }
            else IsTimerSet = false;
        }

        private static void ZeroVariables()
        {
            _actionCount = 0;
            _cyclesCount = 0;

            _startTime = new TimeOnly();
            _activeTime = TimeSpan.Zero;
            _targetTime = new TimeOnly();
            _timeLeft = TimeSpan.Zero;

            _currentAction = ActionType.Action;
        }

        private static void ReapplyElapsedEvent()
        {
            _timer.Elapsed -= OnElapsed_Action_Interval;
            _timer.Elapsed -= OnElapsed_Action_Stopwatch;
            _timer.Elapsed += Configuration.TimerType == (int)TimerType.Stopwatch ? 
                OnElapsed_Action_Stopwatch
                : OnElapsed_Action_Interval;
        }

        private static void OnElapsed_Action_Stopwatch(object source, ElapsedEventArgs e)
        {
            _timeLeft = TimeOnly.FromDateTime(DateTime.Now) - _startTime;
            MainTextBoxController.InvokeTimeBoxText(_timeLeft);
        }

        private static void OnElapsed_Action_Interval(object source, ElapsedEventArgs e)
        {
            _timeLeft = _targetTime - TimeOnly.FromDateTime(e.SignalTime);

            if (TimeOnly.FromDateTime(e.SignalTime) > _targetTime)
            {
                _timeLeft = TimeSpan.Zero;

                Application.Current.Dispatcher.Invoke(() => AudioController.Play());
                MainTextBoxController.InvokeTimeBoxText(_timeLeft);
                ChangeActionType();
                return;
            }

            MainTextBoxController.InvokeMain(_activeTime, _timeLeft);
        }

        private static void ChangeActionType()
        {
            _timeLeft = TimeSpan.Zero;

            switch (_currentAction)
            {
                case ActionType.Action:
                    _currentAction = ActionType.Downtime;
                    if (_actionCount >= Configuration.SeriesInCycle - 1)
                    {
                        _actionCount = 0;
                        _currentAction = ActionType.Rest;
                    }
                    break;
                case ActionType.Downtime:
                    _currentAction = ActionType.Action;
                    _actionCount++;
                    break;
                case ActionType.Rest:
                    _cyclesCount++;
                    if (_cyclesCount >= Configuration.Cycles - 1)
                    {
                        Reset();
                        return;
                    }
                    _currentAction = ActionType.Action;
                    break;
            }

            MainTextBoxController.SetBrush(_currentAction);

            GetSecondsToTarget(_currentAction);
            CalculateTargetTime(false);
        }

        private static void GetSecondsToTarget(ActionType mode)
        {
            int usedSeconds = 0;
            switch ((int)mode)
            {
                case 0: usedSeconds = Configuration.UptimeInSeconds; break;
                case 1: usedSeconds = Configuration.DowntimeInSeconds; break;
                case 2: usedSeconds = Configuration.RestBetweenCyclesInSeconds; break;
                default: break;
            }

            _activeTime = new TimeSpan(0, 0, usedSeconds);
        }

        private static void CalculateTargetTime(bool onPause)
        {
            if (_startTime.ToTimeSpan().TotalMilliseconds == 0)
            {
                _startTime = TimeOnly.FromDateTime(DateTime.Now);
                _targetTime = TimeOnly.FromDateTime(DateTime.Now.Add(_activeTime));
            }
            else
            {
                _startTime = Configuration.TimerType == (int)TimerType.Stopwatch ? TimeOnly.FromDateTime(DateTime.Now.Add(-_timeLeft))
                    : TimeOnly.FromDateTime(DateTime.Now);

                _targetTime = onPause ? _startTime.Add(_timeLeft) : _startTime.Add(_activeTime);
            }
        }
    }
}
