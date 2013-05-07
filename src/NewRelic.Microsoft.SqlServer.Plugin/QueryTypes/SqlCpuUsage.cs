using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("SQL-NonSQL-IdleCPUUsage.sql", "Component/SqlCpuUsage", QueryName = "SQL CPU Usage", Enabled = true)]
	internal class SqlCpuUsage
	{
		[Metric(Ignore = true)]
		public long RecordID { get; set; }

		public DateTime EventTime { get; set; }
		public byte SQLProcessUtilization { get; set; }
		public byte SystemIdle { get; set; }
		public byte OtherProcessUtilization { get; set; }
	}
}
