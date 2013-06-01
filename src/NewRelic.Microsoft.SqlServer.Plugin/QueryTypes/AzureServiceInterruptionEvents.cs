using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	/// <summary>
	/// Used in AzureSqlDatabase as a special query. Disabled to prevent auto-detection and execution.
	/// </summary>
	[AzureSqlQuery("ServiceInterruptionEvents.AzureSQL.sql", "Component/{MetricName}/{Description}", QueryName = "Azure SQL Service Interuptions", Enabled = false)]
	public class AzureServiceInterruptionEvents : DatabaseMetricBase
	{
		[Metric(Ignore = true)]
		public string EventType { get; set; }

		[Metric(Ignore = true)]
		public string Description { get; set; }

		[Metric(MetricName = "ServiceInterruptionEvent", MetricValueType = MetricValueType.Value, Units = "[count]")]
		public int EventCount { get; set; }

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.Where; }
		}

		protected override string DbNameForWhereClause
		{
			get { return "t.DatabaseName"; }
		}

		public override string ToString()
		{
			return string.Format("DatabaseName: {0},\tEventType: {1}\tDescription: {2}\tEventCount: {3}",
			                     DatabaseName, EventType, Description, EventCount);
		}
	}
}
