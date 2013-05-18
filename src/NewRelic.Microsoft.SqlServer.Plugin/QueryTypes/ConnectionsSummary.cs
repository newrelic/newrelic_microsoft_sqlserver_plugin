using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQueryAttribute("ConnectionsSummary.AzureSql.sql", "Component/SqlConnectionCount/{MetricName}/{DatabaseName}", QueryName = "Azure SQL Connections Summary", Enabled = true)]
	[SqlServerQuery("ConnectionsSummary.SqlServer.sql", "Component/SqlConnectionCount/{MetricName}/{DatabaseName}", QueryName = "SQL Server Connections Summary", Enabled = true)]
	internal class ConnectionsSummary : DatabaseMetricBase
	{
		[Metric(MetricValueType = MetricValueType.Count, Units = "[connections]")]
		public int NumberOfConnections { get; set; }

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.WhereAnd; }
		}

		protected override string DbNameForWhereClause
		{
			get { return "DB_NAME(s.dbid)"; }
		}
	}
}