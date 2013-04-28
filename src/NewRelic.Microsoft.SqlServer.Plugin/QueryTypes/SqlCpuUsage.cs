using System;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("SQL-NonSQL-IdleCPUUsage.sql", QueryName = "SQL CPU Usage", Enabled = false)]
	public class SqlCpuUsage
	{
		public long RecordID { get; set; }
		public DateTime EventTime { get; set; }
		public byte SQLProcessUtilization { get; set; }
		public byte SystemIdle { get; set; }
		public byte OtherProcessUtilization { get; set; }
	}
}