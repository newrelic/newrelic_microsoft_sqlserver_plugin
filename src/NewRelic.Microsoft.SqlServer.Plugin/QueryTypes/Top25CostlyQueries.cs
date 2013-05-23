using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("Top25CostlyQueries.sql", "Component/CostlyQueries", QueryName = "Top 25 Costly Queries", Enabled = false)]
	public class Top25CostlyQueries
	{
		public long AverageCPU { get; set; }
		public long TotalCPU { get; set; }
		public long AverageDuration { get; set; }
		public long TotalDuration { get; set; }
		public long AverageReads { get; set; }
		public long TotalReads { get; set; }
		public long ExecutionCount { get; set; }
		public string SQLStatement { get; set; }
	}
}
