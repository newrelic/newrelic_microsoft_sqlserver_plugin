using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
   [AzureSqlQuery("ResourceStats.AzureSQL.sql", "Component/ResourceStats/{MetricName}", QueryName = "Azure SQL Resource Statistics", Enabled = false)]
   public class AzureSqlResourceStats : DatabaseMetricBase
   {
      [Metric(MetricValueType = MetricValueType.Value, Units = "[cores]")]
      public decimal AvgCpuCoresUsed { get; set; }

      [Metric(MetricValueType = MetricValueType.Value, Units = "[iops]")]
      public decimal AvgPhysicalReadIops { get; set; }

      [Metric(MetricValueType = MetricValueType.Value, Units = "[iops]")]
      public decimal AvgPhysicalWriteIops { get; set; }

      [Metric(MetricValueType = MetricValueType.Value, Units = "[KB]")]
      public int ActiveMemoryUsed { get; set; }

      [Metric(MetricValueType = MetricValueType.Value, Units = "[sessions]")]
      public int ActiveSessionCount { get; set; }

      [Metric(MetricValueType = MetricValueType.Value, Units = "[workers]")]
      public int ActiveWorkerCount { get; set; }

      protected override WhereClauseTokenEnum WhereClauseToken
      {
         get { return WhereClauseTokenEnum.Where; }
      }

      protected override string DbNameForWhereClause
      {
         get { return "database_name"; }
      }

      public override string ToString()
      {
         return string.Format("AvgCpuCoresUsed: {0},\t" +
                              "AvgPhysicalReadIops: {1},\t" +
                              "AvgPhysicalWriteIops: {2},\t" +
                              "ActiveSessionCount: {3},\t" +
                              "ActiveWorkerCount: {4}",
                              AvgCpuCoresUsed, AvgPhysicalReadIops, AvgPhysicalWriteIops, ActiveSessionCount, ActiveWorkerCount);
      }
   }

}

