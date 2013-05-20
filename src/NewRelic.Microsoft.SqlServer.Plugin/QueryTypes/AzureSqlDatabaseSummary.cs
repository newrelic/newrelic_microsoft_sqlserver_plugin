using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("Summary.AzureSql.sql", "Component/Summary/{MetricName}", QueryName = "Azure SQL Database Summary", Enabled = true)]
	public class AzureSqlDatabaseSummary
	{
		[Metric(MetricValueType = MetricValueType.Value, Units = "[MB]")]
		public long DbSizeInMB { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[connections]")]
		public int NumberOfConnections { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[reads]")]
		public int NumberOfReads { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[writes]")]
		public int NumberOfWrites { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[sessions]")]
		public int NumberOfSessions { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[requests]")]
		public int TotalCurrentRequests { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int SumSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int MinSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int MaxSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int AvgSessionMemoryUsageInKB { get; set; }
	}
}