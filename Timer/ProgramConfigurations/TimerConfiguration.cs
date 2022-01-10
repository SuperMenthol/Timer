namespace MainSpace.ProgramConfigurations
{
    public class TimerConfiguration
    {
        public string Name { get; set; }
        public int TimerType { get; set; }
        public int SeriesInCycle { get; set; }
        public int Cycles { get; set; }
        public int UptimeInSeconds { get; set; }
        public int DowntimeInSeconds { get; set; }
        public int RestBetweenCyclesInSeconds { get; set; }
    }
}
