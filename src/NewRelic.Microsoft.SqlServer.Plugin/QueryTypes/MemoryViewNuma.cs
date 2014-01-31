using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [SqlServerQuery("MemoryView.Numa.sql", "Memory", QueryName = "Memory View (NUMA)", Enabled = true)]
    public class MemoryViewNuma
    {
        [Metric(Ignore = true)]
        public string Node { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "sec", MetricName = "PageLifeNuma/Node_{Node}")]
        public long PageLife { get; set; }

        public override string ToString()
        {
            return string.Format("Node: {0},\t" +
                                 "PageLife: {1}",
                                 Node != null ? Node.Trim() : "N/A", PageLife);
        }
    }
}
