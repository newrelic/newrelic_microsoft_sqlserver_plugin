using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("Connections.SqlServer.sql", "Component/Connections/{MetricName}/{DatabaseName}", QueryName = "SQL Connections", Enabled = true)]
	internal class Connections : DatabaseMetricBase
	{
		public Guid ConnectionId { get; set; }
		public string ClientNetAddress { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[reads]")]
		public int NumberOfReads { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[writes]")]
		public int NumberOfWrites { get; set; }

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
