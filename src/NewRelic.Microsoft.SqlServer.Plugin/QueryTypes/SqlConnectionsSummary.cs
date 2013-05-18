using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("SQLConnectionsSummary.sql", "Component/SqlConnectionCount/{MetricName}/{DatabaseName}", QueryName = "SQL Connections Summary", Enabled = true)]
	internal class SqlConnectionsSummary : DatabaseMetricBase
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