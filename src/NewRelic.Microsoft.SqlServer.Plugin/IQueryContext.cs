using System;
using System.Collections.Generic;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public interface IQueryContext
	{
		string QueryName { get; }
		Type QueryType { get; }
		IEnumerable<object> Results { get; set; }
		ComponentData ComponentData { get; set; }
		int MetricsRecorded { get; }
		bool DataSent { get; set; }
		MetricTransformEnum MetricTransformEnum { get; }
	    
		string FormatMetricKey(object queryResult, string metricName, string metricUnits);
		void AddAllMetrics();
		void AddMetric(string name, int value);
		void AddMetric(string name, decimal value);
		void AddMetric(string name, MinMaxMetricValue value);
	}
}