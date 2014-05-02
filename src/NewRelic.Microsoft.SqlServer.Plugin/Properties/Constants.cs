namespace NewRelic.Microsoft.SqlServer.Plugin.Properties
{
    internal static class Constants
    {
        public const string SqlServerComponentGuid = "com.newrelic.platform.microsoft.sqlserver";
        public const string SqlAzureComponentGuid = "com.newrelic.platform.microsoft.azuresql";

        public const string WhereClauseReplaceToken = @"/*{WHERE}*/";
        public const string WhereClauseAndReplaceToken = @"/*{AND_WHERE}*/";

        public static string[] SystemDatabases
        {
            get { return new[] {"tempdb", "master", "model", "msdb",}; }
        }
    }
}
