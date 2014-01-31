using System;
using System.Collections.Generic;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Sdk.Binding;
using NewRelic.Platform.Sdk.Processors;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public interface IQueryContext
    {
        string QueryName { get; }
        Type QueryType { get; }
        IEnumerable<object> Results { get; set; }
        string ComponentName { get; set; }
        string ComponentGuid { get; set; }
        IContext Context { get; set; }
        int MetricsRecorded { get; }
        IDictionary<string, IProcessor> MetricProcessors { get; set; }
        
        string FormatMetricKey(object queryResult, string metricName);
        void AddAllMetrics();
        void AddMetric(string name, string units, int value, MetricTransformEnum metricTransform);
        void AddMetric(string name, string units, decimal value, MetricTransformEnum metricTransform);
    }
}