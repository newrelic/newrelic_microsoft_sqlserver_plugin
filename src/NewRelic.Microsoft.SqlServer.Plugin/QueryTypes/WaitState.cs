using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("WaitStates.sql", "Component/WaitState/{WaitType}", QueryName = "Wait States", Enabled = true)]
	internal class WaitState
	{
		public string WaitType { get; set; }
		public decimal WaitSeconds { get; set; }
		public decimal ResourceSeconds { get; set; }
		public decimal SignalSeconds { get; set; }
		public long WaitCount { get; set; }
		public decimal Percentage { get; set; }
		public decimal AvgWaitSeconds { get; set; }
		public decimal AvgResourceSeconds { get; set; }
		public decimal AvgSignalSeconds { get; set; }
	}
}
