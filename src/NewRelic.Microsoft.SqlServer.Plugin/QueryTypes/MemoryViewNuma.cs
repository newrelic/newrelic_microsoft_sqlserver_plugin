using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("MemoryView.Numa.sql", "Component/Memory", QueryName = "Memory View (NUMA)", Enabled = true)]
	public class MemoryViewNuma
	{
		[Metric(Ignore = true)]
		public string Node { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec]", MetricName = "PageLifeNuma/Node_{Node}")]
		public long PageLife { get; set; }
	}
}
