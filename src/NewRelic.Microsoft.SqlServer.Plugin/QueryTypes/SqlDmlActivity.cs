using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "Sql DML Activity", Enabled = true)]
	[SqlServerQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "Sql DML Activity", Enabled = true)]
	public class SqlDmlActivity
	{
		[Metric(Ignore = true)]
		public Byte[] SqlHandle { get; set; }

		[Metric(Ignore = true)]
		public Byte[] QueryHash { get; set; }

		[Metric(Ignore = true)]
		public int ExecutionCount { get; set; }

		[Metric(Ignore = true)]
		public string QueryType { get; set; }

		public int Writes { get; set; }

		public int Reads { get; set; }

		public override string ToString()
		{
			return string.Format(("SqlHandle: {0},\t" +
			                      "QueryHash: {1},\t" +
			                      "ExecutionCount: {2}\t" +
			                      "QueryType: {3},\t" +
			                      "Writes: {4},\t" +
			                      "Reads: {5}"),
			                     SqlHandle != null ? BitConverter.ToString(SqlHandle) : string.Empty,
			                     QueryHash != null ? BitConverter.ToString(QueryHash) : string.Empty,
			                     ExecutionCount,
			                     QueryType,
			                     Writes,
			                     Reads
				);
		}
	}
}
