namespace NewRelic.Microsoft.SqlServer.Plugin.Properties
{
    internal static class Constants
    {
        public const string SqlServerComponentGuid = "com.newrelic.platform.microsoft.sqlserver";
        public const string SqlAzureComponentGuid = "com.newrelic.platform.microsoft.azuresql";

        public const string WhereClauseReplaceToken = @"/*{WHERE}*/";
        public const string WhereClauseAndReplaceToken = @"/*{AND_WHERE}*/";

        public const string ServiceString = "service";
        public const string ProxyString = "proxy";
        public const string SqlServerString = "sqlServers";
        public const string AzureString = "azure";

        public const string TypeProperty = "type";
        public const string NameProperty = "name";
        public const string ConnectionStringProperty = "connectionString";
        public const string IncludeSystemDbsProperty = "includeSystemDatabases";
        public const string IncludesProperty = "includes";
        public const string ExcludesProperty = "excludes";
        public const string DisplayNameProperty = "displayName";

        public static string[] SystemDatabases
        {
            get { return new[] {"tempdb", "master", "model", "msdb",}; }
        }
    }
}
