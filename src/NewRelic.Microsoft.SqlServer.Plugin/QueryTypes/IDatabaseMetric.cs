namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public interface IDatabaseMetric
	{
		string DatabaseName { get; set; }

		string ParameterizeQuery(string commandText, ISqlEndpoint endpoint);
	}
}
