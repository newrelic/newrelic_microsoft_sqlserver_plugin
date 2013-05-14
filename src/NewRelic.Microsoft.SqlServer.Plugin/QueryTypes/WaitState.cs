using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("WaitStates.sql", "Component/WaitState/{MetricName}/{WaitType}", QueryName = "Wait States", Enabled = true)]
	internal class WaitState
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
	}
}
