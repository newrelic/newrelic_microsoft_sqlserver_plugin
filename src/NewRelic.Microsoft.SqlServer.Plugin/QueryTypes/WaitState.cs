using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("WaitStates.sql", QueryName = "Wait States", Enabled = true)]
	public class WaitState : IQueryResult
	{
		public string WaitType { get; set; }
		public decimal WaitSeconds { get; set; }
		public decimal ResourceSeconds { get; set; }
		public decimal SignalSeconds { get; set; }
		public int WaitCount { get; set; }
		public decimal Percentage { get; set; }
		public decimal AvgWaitSeconds { get; set; }
		public decimal AvgResourceSeconds { get; set; }
		public decimal AvgSignalSeconds { get; set; }

		public void AddMetrics(ComponentData componentData)
		{
			var waitState = string.IsNullOrEmpty(WaitType) ? "(none)" : WaitType;
			var metricBase = string.Format("WaitState/{0}/", waitState);
			componentData.AddMetric(metricBase + "WaitSeconds", WaitSeconds);
			componentData.AddMetric(metricBase + "ResourceSeconds", ResourceSeconds);
			componentData.AddMetric(metricBase + "SignalSeconds", SignalSeconds);
			componentData.AddMetric(metricBase + "WaitCount", WaitCount);
			componentData.AddMetric(metricBase + "Percentage", Percentage);
			componentData.AddMetric(metricBase + "AvgWaitSeconds", AvgWaitSeconds);
			componentData.AddMetric(metricBase + "AvgResourceSeconds", AvgResourceSeconds);
			componentData.AddMetric(metricBase + "AvgSignalSeconds", AvgSignalSeconds);
		}

		public override string ToString()
		{
			var waitState = string.IsNullOrEmpty(WaitType) ? "(none)" : WaitType;
			return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", waitState, WaitSeconds, ResourceSeconds, SignalSeconds, WaitCount, Percentage, AvgWaitSeconds, AvgResourceSeconds, AvgSignalSeconds);
		}
	}
}