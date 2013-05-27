using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[AzureSqlQuery("SqlDMLActivity.SqlServerAndAzureSQL.sql", "Component/DMLActivity/{MetricName}", QueryName = "SQL DML Activity", Enabled = true)]
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
	}
}
