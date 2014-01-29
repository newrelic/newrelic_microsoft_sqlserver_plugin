using System;
using System.Reflection;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    /// <summary>
    /// Allows override of default conventions for discovering and naming metrics on <em>QueryTypes</em>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MetricAttribute : Attribute
    {
        /// <summary>
        /// Override the name of the metric recorded by the property the attribute is applied to. By default, the <see cref="PropertyInfo.Name"/> is used as the metric name.
        /// </summary>
        public string MetricName { get; set; }

        /// <summary>
        /// Metrics have a notion of units that describe what the count or value represent. The format is [value_unit|count_unit].
        /// Common units include [MB/sec], [bytes], and [failure|attempts].
        /// </summary>
        /// <remarks>See more at https://newrelic.com/docs/platform/metric-api-and-naming-reference.</remarks>
        public string Units { get; set; }

        /// <summary>
        /// Default is false. When true, prevents the property from being reported as a metric.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Metric value type is based on the property type (e.g. int is a count, decimal is a value). The default is <see cref="NewRelic.Microsoft.SqlServer.Plugin.Core.MetricValueType.Default"/>.
        /// Any other value overrides the type-based determination of the value type.
        /// </summary>
        public MetricValueType MetricValueType { get; set; }

        /// <summary>
        /// Dictates whether or not additional processing should be done over an individual metric before sending it to the New Relic servic.e
        /// </summary>
        public MetricTransformEnum MetricTransform { get; set; }
    }
}
