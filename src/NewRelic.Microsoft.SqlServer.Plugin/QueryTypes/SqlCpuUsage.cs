using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("SQL-NonSQL-IdleCPUUsage.sql", "Component/SqlCpuUsage", QueryName = "SQL CPU Usage", Enabled = true)]
	public class SqlCpuUsage
	{
		[Metric(Ignore = true)]
		public long RecordID { get; set; }

		public DateTime EventTime { get; set; }

		[Metric(MetricName = "SQLProcess", MetricValueType = MetricValueType.Value, Units = "[%_CPU]")]
		public byte SQLProcessUtilization { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_CPU]")]
		public byte SystemIdle
		{
			get { return (byte) (100 - SQLProcessUtilization - OtherProcessUtilization); }
		}

		[Metric(MetricName = "OtherProcess", MetricValueType = MetricValueType.Value, Units = "[%_CPU]")]
		public byte OtherProcessUtilization { get; set; }

		public override string ToString()
		{
			return string.Format("RecordID: {0},\t" +
			                     "EventTime: {1},\t" +
			                     "SQLProcessUtilization: {2},\t" +
			                     "SystemIdle: {3},\t" +
			                     "OtherProcessUtilization: {4}",
			                     RecordID, EventTime, SQLProcessUtilization, SystemIdle, OtherProcessUtilization);
		}
	}
}
