using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("WaitStates.sql", "Custom/WaitState/{WaitType}", QueryName = "Wait States", Enabled = true)]
	public class WaitState
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

		public override string ToString()
		{
			var waitState = string.IsNullOrEmpty(WaitType) ? "(none)" : WaitType;
			return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", waitState, WaitSeconds, ResourceSeconds, SignalSeconds, WaitCount, Percentage, AvgWaitSeconds, AvgResourceSeconds,
			                     AvgSignalSeconds);
		}
	}
}
