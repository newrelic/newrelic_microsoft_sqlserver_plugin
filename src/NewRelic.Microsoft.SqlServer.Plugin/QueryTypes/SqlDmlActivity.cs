using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "Sql DML Activity", Enabled = true)]
	[SqlServerQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "Sql DML Activity", Enabled = true)]
	public class SqlDmlActivity
	{
		[Metric(Ignore = true)]
		public Byte[] PlanHandle { get; set; }

		[Metric(Ignore = true)]
		public long ExecutionCount { get; set; }

		[Metric(Ignore = true)]
		public string QueryType { get; set; }

		[Metric(Ignore = true)]
		public string SQlStatement { get; set; }

		[Metric(Ignore = true)]
		public DateTime CreationTime { get; set; }

		public long Writes { get; set; }

		public long Reads { get; set; }

		public override string ToString()
		{
			return string.Format("PlanHandle: {0},\t" +
								  "ExecutionCount: {1}\t" +
								  "CreationTime: {2}\t" +
								  "QueryType: {3}",
			                     PlanHandle != null ? BitConverter.ToString(PlanHandle) : string.Empty,
			                     ExecutionCount,
								 CreationTime,
			                     QueryType
				);
		}
	}
}
