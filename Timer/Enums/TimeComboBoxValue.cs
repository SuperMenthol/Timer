using System.Collections.Generic;

namespace MainSpace.Enums
{
    public static class TimeComboBoxValue
    {
        public static string Second = "sekund";
        public static string Minute = "minut";

        public static List<string> Values { get; set; } = new List<string>() { Second, Minute };
    }
}
