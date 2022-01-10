using MainSpace.ProgramConfigurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MainSpace.Controllers
{
    public static class ConfigurationController
    {
        public static TimerConfiguration UsedConfiguration
        {
            get { return _usedConfiguration; }
            set { _usedConfiguration = value; }
        }

        private static TimerConfiguration _usedConfiguration = new TimerConfiguration();
        private static string _configurationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.json");

        public static List<TimerConfiguration> GetAllConfigurations()
        {
            var configurationPath = _configurationFilePath;
            using (StreamReader file = File.OpenText(configurationPath))
            {
                var readFile = file.ReadToEnd();
                return JsonConvert.DeserializeObject<List<TimerConfiguration>>(readFile) ?? new List<TimerConfiguration>();
            }
        }

        public static void SaveConfiguration(TimerConfiguration configToAdd)
        {
            
            var configurationsToSave = GetAllConfigurations();
            configurationsToSave.Add(configToAdd);

            File.WriteAllText(_configurationFilePath, JsonConvert.SerializeObject(configurationsToSave), Encoding.UTF8);
        }
    }
}
