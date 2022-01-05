using System;
using System.Text;
using System.Windows;

namespace MainSpace
{
    public partial class SavePrompt : IDisposable
    {
        private static string _name;
        TimerConfiguration _configuration;
        private bool disposedValue;

        public SavePrompt(TimerConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            _name = GenerateTemplateName();
            NameBox.Text = _name;
        }

        public static bool ShowDialog(TimerConfiguration configuration, out string configurationName)
        {
            using (var customDialog = new SavePrompt(configuration))
            {
                configurationName = _name;
                return (bool)customDialog.ShowDialog();
            }
        }

        private string GenerateTemplateName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Interwał - ");
            sb.Append($"{_configuration.SeriesInCycle}x{_configuration.Cycles} - ");
            sb.Append($"{_configuration.UptimeInSeconds}s pracy, {_configuration.DowntimeInSeconds}s przerwy, {_configuration.RestBetweenCyclesInSeconds}s odpoczynku");

            return sb.ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            _name = NameBox.Text;
            this.DialogResult = true;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}