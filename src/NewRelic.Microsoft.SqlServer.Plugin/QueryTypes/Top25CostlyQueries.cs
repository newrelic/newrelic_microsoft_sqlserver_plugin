using System;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("Top25CostlyQueries.sql")]
	internal class Top25CostlyQueries
	{
		public long AverageCPU { get; set; }
		public long TotalCPU { get; set; }
		public long AverageDuration { get; set; }
		public long TotalDuration { get; set; }
		public long AverageReads { get; set; }
		public long TotalReads { get; set; }
		public long ExecutionCount { get; set; }
		public string SQLStatement { get; set; }

		public override string ToString()
		{
			var sqlStatement = SQLStatement.Substring(0, Math.Min(50, SQLStatement.Length)).Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
			return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", AverageCPU, TotalCPU, AverageDuration, TotalDuration, AverageReads, TotalReads, ExecutionCount, sqlStatement);
		}
	}
}
