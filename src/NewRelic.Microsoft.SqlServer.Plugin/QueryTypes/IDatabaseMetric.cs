namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	internal interface IDatabaseMetric
	{
		string DatabaseName { get; }

		string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs);
	}
}
