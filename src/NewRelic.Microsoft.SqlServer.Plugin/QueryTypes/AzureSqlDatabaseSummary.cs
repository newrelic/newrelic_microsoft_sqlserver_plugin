using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("Summary.AzureSql.sql", "Component/Summary/{MetricName}", QueryName = "Azure SQL Database Summary", Enabled = true)]
	public class AzureSqlDatabaseSummary
	{
		[Metric(MetricValueType = MetricValueType.Value, Units = "[MB]")]
		public long DbSizeInMB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[connections]")]
		public int NumberOfConnections { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[reads]")]
		public int NumberOfReads { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[writes]")]
		public int NumberOfWrites { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sessions]")]
		public int NumberOfSessions { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[requests]")]
		public int TotalCurrentRequests { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int SumSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int MinSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int MaxSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
		public int AvgSessionMemoryUsageInKB { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[objects]")]
		public int SingleUseObjects { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[objects]")]
		public int MultipleUseObjects { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_Single_Use]")]
		public decimal SingleUsePercent { get; set; }

		public override string ToString()
		{
			return string.Format("DbSizeInMB: {0},\t" +
			                     "NumberOfConnections: {1},\t" +
			                     "NumberOfReads: {2},\t" +
			                     "NumberOfWrites: {3},\t" +
			                     "NumberOfSessions: {4},\t" +
			                     "TotalCurrentRequests: {5},\t" +
			                     "SumSessionMemoryUsageInKB: {6},\t" +
			                     "MinSessionMemoryUsageInKB: {7},\t" +
			                     "MaxSessionMemoryUsageInKB: {8},\t" +
			                     "AvgSessionMemoryUsageInKB: {9},\t" +
			                     "SingleUseObjects: {10},\t" +
			                     "MultipleUseObjects: {11},\t" +
			                     "SingleUsePercent: {12}",
			                     DbSizeInMB, NumberOfConnections, NumberOfReads, NumberOfWrites, NumberOfSessions, TotalCurrentRequests,
			                     SumSessionMemoryUsageInKB, MinSessionMemoryUsageInKB, MaxSessionMemoryUsageInKB,
			                     AvgSessionMemoryUsageInKB, SingleUseObjects, MultipleUseObjects, SingleUsePercent);
		}
	}
}
