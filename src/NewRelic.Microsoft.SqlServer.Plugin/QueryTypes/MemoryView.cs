using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("MemoryView.sql", "Component/Memory/{DatabaseName}", QueryName = "Memory View", Enabled = true)]
	internal class MemoryView
	{
		public decimal BufferCacheHitRatio { get; set; }
		public long PageLifeExpectancyInSeconds { get; set; }
		public long PageLifeExpectancyInSecondsNuma { get; set; }
	}
}
