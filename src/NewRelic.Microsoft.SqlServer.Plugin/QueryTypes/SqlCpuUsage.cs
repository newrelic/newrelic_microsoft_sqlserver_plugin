using System;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("SQL-NonSQL-IdleCPUUsage.sql", "Custom/SqlCpuUsage", QueryName = "SQL CPU Usage", Enabled = true)]
	public class SqlCpuUsage
	{
		[Metric(Ignore = true)]
		public long RecordID { get; set; }

		public DateTime EventTime { get; set; }
		public byte SQLProcessUtilization { get; set; }
		public byte SystemIdle { get; set; }
		public byte OtherProcessUtilization { get; set; }

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}", SQLProcessUtilization, OtherProcessUtilization, SystemIdle);
		}
	}
}
