using NewRelic.Microsoft.SqlServer.Plugin.Core;

using System;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [AzureSqlQuery("ResourceStats.AzureSql.sql", "ResourceStats/{MetricName}", QueryName = "Resource Stats", Enabled = true)]
    public class ResourceStats
    {

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Avg_Cpu")]
        public decimal AvgCpuPercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Max_Cpu")]
        public decimal MaxCpuPercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Avg_Data_Io")]
        public decimal AvgDataIoPercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Max_Data_Io")]
        public decimal MaxDataIoPercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Avg_Log_Write")]
        public decimal AvgLogWritePercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Max_Log_Write")]
        public decimal MaxLogWritePercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Avg_Memory_Usage")]
        public decimal AvgMemoryUsagePercent { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "%_Max_Memory_Usage")]
        public decimal MaxMemoryUsagePercent { get; set; }

        public override string ToString()
        {
            return string.Format(   "AvgCpuPercent: {0},\t" +
                                    "MaxCpuPercent: {1},\t" +
                                    "AvgDataIoPercent: {2},\t" +
                                    "MaxDataIoPercent: {3},\t" +
                                    "AvgLogWritePercent: {4},\t" +
                                    "MaxLogWritePercent: {5},\t" +
                                    "AvgMemoryUsagePercent: {6},\t" +
                                    "MaxMemoryUsagePercent: {7},\t",
                                    AvgCpuPercent, MaxCpuPercent,
                                    AvgDataIoPercent, MaxDataIoPercent,
                                    AvgLogWritePercent, MaxLogWritePercent,
                                    AvgMemoryUsagePercent, MaxMemoryUsagePercent);
        }

    }
}
