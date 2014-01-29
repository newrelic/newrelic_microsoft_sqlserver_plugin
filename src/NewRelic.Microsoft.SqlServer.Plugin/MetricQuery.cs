using System;
using System.Linq;
using System.Reflection;

using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public class MetricQuery : IMetricQuery
    {
        private readonly MetricMapper[] _metricMappers;

        public MetricQuery(Type queryType, string queryName, string resultTypeName)
        {
            QueryType = queryType;
            QueryName = queryName;
            MetricPattern = string.Format("{0}", QueryType.Name);
            ResultTypeName = resultTypeName;
            _metricMappers = GetMappers(QueryType);
        }

        public Type QueryType { get; private set; }

        public string QueryName { get; private set; }

        public string MetricPattern { get; protected set; }

        public string ResultTypeName { get; private set; }

        public void AddMetrics(QueryContext context)
        {
            context.Results.ForEach(r => _metricMappers.ForEach(m => m.AddMetric(context, r)));
        }

        /// <summary>
        ///     Sets up the mappers that take the values on the query result and records each one as a metric.
        /// </summary>
        /// <param name="type">
        ///     <em>QueryType</em> to look for metric properties
        /// </param>
        /// <returns>
        ///     An array of mappers capable of creating metrics for a <em>QueryType</em>
        /// </returns>
        internal static MetricMapper[] GetMappers(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Select(MetricMapper.TryCreate)
                       .Where(m => m != null)
                       .ToArray();
        }
    }
}
