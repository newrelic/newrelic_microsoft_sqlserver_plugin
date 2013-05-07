namespace NewRelic.Microsoft.SqlServer.Plugin.Properties
{
	internal static class Constants
	{
		public const string ComponentGuid = "com.newrelic.platform.microsoft.sqlserver";
		public const string SqlMonitorLogger = "SqlMonitor";
		public const string VerboseSqlLogger = "VerboseSqlOutput";
		public const string VerboseMetricsLogger = "VerboseMetricOutput";

		public const string WhereClauseReplaceToken = @"/*{WHERE}*/";
		public const string WhereClauseAndReplaceToken = @"/*{AND_WHERE}*/";

		public static string[] SystemDatabases
		{
			get { return new[] {"tempdb", "master", "model", "msdb",}; }
		}
	}
}
