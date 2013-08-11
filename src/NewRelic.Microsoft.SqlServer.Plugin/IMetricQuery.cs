using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public interface IMetricQuery
	{
		MetricTransformEnum MetricTransformEnum { get; }
		string QueryName { get; }
		Type QueryType { get; }
		string MetricPattern { get; }
		string ResultTypeName { get; }
		void AddMetrics(QueryContext context);
	}
}