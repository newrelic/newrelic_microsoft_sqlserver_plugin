using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "Sql DML Activity", Enabled = true)]
	[SqlServerQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "Sql DML Activity", Enabled = false)]
	public class SqlDmlActivity
	{
		[Metric(Ignore = true)]
		public Byte[] PlanHandle { get; set; }

		[Metric(Ignore = true)]
		public long ExecutionCount { get; set; }

		[Metric(Ignore = true)]
		public string QueryType { get; set; }

		[Metric(Ignore = true)]
		public Byte[] SqlStatementHash { get; set; }

		[Metric(Ignore = true)]
		public DateTime CreationTime { get; set; }

		public long Writes { get; set; }

		public long Reads { get; set; }

		public override string ToString()
		{
			return string.Format("PlanHandle: {0},\t" +
								 "SqlStatementHash: {1},\t" +
			                     "ExecutionCount: {2},\t" +
			                     "CreationTime: {3},\t" +
			                     "QueryType: {4},",
			                     PlanHandle != null ? BitConverter.ToString(PlanHandle) : string.Empty,
			                     SqlStatementHash != null ? BitConverter.ToString(SqlStatementHash) : string.Empty,
			                     ExecutionCount,
			                     CreationTime,
			                     QueryType
				);
		}
	}
}
