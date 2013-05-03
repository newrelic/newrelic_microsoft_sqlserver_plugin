namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public interface IDatabaseMetric
	{
		string DatabaseName { get; }

		string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs);
	}
}
