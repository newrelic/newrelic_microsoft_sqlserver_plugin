using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("Connections.SqlServer.sql", "Component/Connections/{MetricName}/{DatabaseName}", QueryName = "SQL Connections", Enabled = true)]
	public class SqlServerConnections : DatabaseMetricBase
	{
		[Metric(MetricValueType = MetricValueType.Count, Units = "[connections]")]
		public int NumberOfConnections { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[reads]")]
		public int NumberOfReads { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[writes]")]
		public int NumberOfWrites { get; set; }

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.WhereAnd; }
		}

		protected override string DbNameForWhereClause
		{
			get { return "d.Name"; }
		}
	}
}
