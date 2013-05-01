using System;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("SQL-NonSQL-IdleCPUUsage.sql", QueryName = "SQL CPU Usage", Enabled = true)]
	public class SqlCpuUsage : IQueryResult
	{
		public long RecordID { get; set; }
		public DateTime EventTime { get; set; }
		public byte SQLProcessUtilization { get; set; }
		public byte SystemIdle { get; set; }
		public byte OtherProcessUtilization { get; set; }

		public void AddMetrics(ComponentData componentData)
		{
			const string metricBase = "SqlCpuUsage/";
			componentData.AddMetric(metricBase + "SQLProcessUtilization", SQLProcessUtilization);
			componentData.AddMetric(metricBase + "OtherProcessUtilization", OtherProcessUtilization);
			componentData.AddMetric(metricBase + "SystemIdle", SystemIdle);
		}

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}", SQLProcessUtilization, OtherProcessUtilization, SystemIdle);
		}
	}
}
