using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("WaitStates.AzureSql.sql", "Component/WaitState/{MetricName}/{WaitType}", QueryName = "Wait States", Enabled = true)]
	[SqlServerQuery("WaitStates.SqlServer.sql", "Component/WaitState/{MetricName}/{WaitType}", QueryName = "Wait States", Enabled = true)]
	public class WaitState
	{
		public string WaitType { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec]")]
		public decimal WaitSeconds { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec]")]
		public decimal ResourceSeconds { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec]")]
		public decimal SignalSeconds { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[wait]")]
		public long WaitCount { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_wait]")]
		public decimal Percentage { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec/wait]")]
		public decimal AvgWaitSeconds { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec/resource]")]
		public decimal AvgResourceSeconds { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec/signal]")]
		public decimal AvgSignalSeconds { get; set; }

		public override string ToString()
		{
			return string.Format("WaitType: {0},\t" +
			                     "WaitSeconds: {1},\t" +
			                     "ResourceSeconds: {2},\t" +
			                     "SignalSeconds: {3},\t" +
			                     "WaitCount: {4},\t" +
			                     "Percentage: {5},\t" +
			                     "AvgWaitSeconds: {6},\t" +
			                     "AvgResourceSeconds: {7},\t" +
			                     "AvgSignalSeconds: {8}",
			                     WaitType, WaitSeconds, ResourceSeconds, SignalSeconds, WaitCount, Percentage, AvgWaitSeconds, AvgResourceSeconds, AvgSignalSeconds);
		}
	}
}
