namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public static class Constants
    {
        public const string ComponentGuid = "64E117A2-43DC-4E71-8BCB-07691F9E421E";
        public const string SqlMonitorLogger = "SqlMonitor";
        public const string VerboseSqlLogger = "VerboseSqlOutput";
        public const string VerboseMetricsLogger = "VerboseMetricOutput";

        public static string[] SystemDatabases
        {
            get { return new[] {"tempdb", "master", "model", "msdb"}; }
        }
    }
}