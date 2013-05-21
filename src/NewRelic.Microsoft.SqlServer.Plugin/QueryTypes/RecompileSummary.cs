using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("RecompileSummary.sql", "Component/Recompiles/{MetricName}/{DatabaseName}", QueryName = "Recompile Summary", Enabled = true)]
	public class RecompileSummary : DatabaseMetricBase
	{
		[Metric(MetricValueType = MetricValueType.Count, Units = "[objects]")]
		public int SingleUseObjects { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[objects]")]
		public int MultipleUseObjects { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_Single_Use]")]
		public decimal SingleUsePercent { get; set; }

		protected override string DbNameForWhereClause
		{
			get { return "d.name"; }
		}

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.Where; }
		}
	}
}
