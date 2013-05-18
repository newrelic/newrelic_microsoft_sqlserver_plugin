using System;
using System.Reflection;

using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    /// <summary>
    ///     Marks a <em>QueryType</em> with the information required to gather and report a set of metrics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class SqlServerQueryAttribute : Attribute
    {
        [Obsolete("Supply metricPattern", true)]
        public SqlServerQueryAttribute(string resourceName)
            : this(resourceName, null) {}

        /// <summary>
        ///     Marks a <em>QueryType</em> with the information required to gather and report a set of metrics. The metrics are gathered using a SQL query that is embedded in the assembly.
        /// </summary>
        /// <param name="resourceName">
        ///     <see cref="ResourceName" />
        /// </param>
        /// <param name="metricPattern">
        ///     <see cref="MetricPattern" />
        /// </param>
        public SqlServerQueryAttribute(string resourceName, string metricPattern)
        {
            ResourceName = resourceName;
            MetricPattern = metricPattern;
            Enabled = true;
        }

        /// <summary>
        ///     Path to the embedded query file. Can be absolute with the entire namespace and file name, or partial.
        ///     A partial name is used to search all resources. An ambiguous name will produce an exception elsewhere during initialization.
        /// </summary>
        public string ResourceName { get; private set; }

        /// <summary>
        ///     Optional. A log-friendly name for the query.
        /// </summary>
        public string QueryName { get; set; }

        /// <summary>
        ///     Optional. Default is true. When false, the query is ignored by the monitor.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Optional. Indicates how, once collected the metrics should be sent to New Relic. Defaults to Simple which does no transform and sends the most recent values.
        /// </summary>
        public MetricTransformEnum MetricTransformEnum { get; set; }

        /// <summary>
        ///     <para>Required. The pattern is formatted and sent to New Relic.</para>
        ///     <para>A pattern must start with 'Custom/'. A good metric supports wildcards for supporting better charts.</para>
        ///     <para>For example, file I/O metrics patterns could look like:</para>
        ///     <para>
        ///         <em>Custom/FileIO/{DatabaseName}/{MetricName}</em>
        ///     </para>
        ///     <para>The pattern placeholders are replaced when recording the metric. Support includes:</para>
        ///     <para>
        ///         {DatabaseName} -
        ///         <em>
        ///             Replaced by <see cref="IDatabaseMetric.DatabaseName" /> when the <see cref="Type.ReflectedType" /> implements
        ///             <see
        ///                 cref="IDatabaseMetric" />
        ///         </em>
        ///     </para>
        ///     <para>
        ///         {MetricName} -
        ///         <em>
        ///             Replaced by <see cref="MetricAttribute.MetricName" /> when applied to a given property. If not applied, the convention is to use the value of
        ///             <see
        ///                 cref="PropertyInfo.Name" />
        ///         </em>
        ///     </para>
        /// </summary>
        public string MetricPattern { get; private set; }
    }
}
