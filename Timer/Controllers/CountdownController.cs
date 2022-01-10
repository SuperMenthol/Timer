using MainSpace.Enums;
using MainSpace.ProgramConfigurations;
using System;
using System.Text.RegularExpressions;

namespace MainSpace.Controllers
{
    public static class CountdownController
    {
        public static bool ValidateTextBox(bool startingAt, string input)
        {
            return startingAt ? 
                ValidateTextBoxAt(input) : 
                ValidateTextBoxIn(input);
        }

        private static bool ValidateTextBoxAt(string input)
        {
            return Regex.IsMatch(input, @"[\d][\d]:[\d][\d]") &&
                TimeOnly.TryParse(input, out TimeOnly targetTime);
        }

        private static bool ValidateTextBoxIn(string input)
        {
            return int.TryParse(input, out int targetTime);
        }

        public static TimerConfiguration GetCountdownConfiguration(bool startingAt, string inputString, bool? seconds = null)
        {
            return new TimerConfiguration()
            {
                Name = "Countdown",
                SeriesInCycle = 1,
                TimerType = (int)TimerType.Countdown,
                Cycles = 1,
                UptimeInSeconds = 0,
                DowntimeInSeconds = 0,
                RestBetweenCyclesInSeconds = GetCountdownTime(startingAt, inputString, seconds),
            };
        }

        private static int GetCountdownTime(bool startingAt, string input, bool? seconds = null)
        {
            int output = 0;
            if (ValidateTextBox(startingAt, input))
            {
                if (startingAt)
                {
                    output = (int)(TimeOnly.Parse(input)-TimeOnly.FromDateTime(DateTime.Now)).TotalSeconds;
                }
                else
                {
                    output = int.Parse(input);
                }
            }

            if (seconds != null)
            {
                output *= (bool)seconds ? 1 : 60;
            }

            return output;
        }
    }
}
