using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    public interface ISqlQuery
    {
        MetricTransformEnum MetricTransformEnum { get; }
        string QueryName { get; }
        string MetricPattern { get; }
        string ResultTypeName { get; }
        string ResourceName { get; }
        string CommandText { get; }

        /// <summary>
        ///     Queries data from the database and returns the results.
        /// </summary>
        /// <param name="dbConnection">Open connection to the database.</param>
        /// <param name="endpoint">Settings for the endpoint that is queried.</param>
        /// <returns>
        ///     An enumeration of a the type where the <see cref="SqlServerQueryAttribute" /> for this query object was found during initialization.
        /// </returns>
        IEnumerable<object> Query(IDbConnection dbConnection, ISqlEndpoint endpoint);

        void AddMetrics(QueryContext context);
    }

    public class SqlQuery : ISqlQuery
    {
        /// <summary>Stores a pointer to the generic method so this reflection isn't repeated later.</summary>
        private static readonly MethodInfo _QueryMethod = typeof (SqlQuery).GetMethod("Query", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo _DatabaseMetricQueryMethod = typeof (SqlQuery).GetMethod("DatabaseMetricQuery", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IDapperWrapper _dapperWrapper;
        private readonly MethodInfo _genericMethod;
        private readonly MetricMapper[] _metricMappers;
        internal QueryAttribute QueryAttribute;

        public SqlQuery(Type queryType, QueryAttribute attribute, IDapperWrapper dapperWrapper, string commandText = null)
        {
            _dapperWrapper = dapperWrapper;
            ResultTypeName = queryType.Name;
            QueryAttribute = attribute;

            MetricPattern = attribute.MetricPattern ?? string.Format("Component/{0}", ResultTypeName);

            // Get the SQL resource from the same assembly as the type, when commandText is not supplied
            CommandText = commandText ?? queryType.Assembly.SearchForStringResource(attribute.ResourceName);

            // Get a pointer to the correctly typed Query method below with the QueryType as the generic parameter
            _genericMethod = attribute.ShouldParameterizeDatabaseInQuery && typeof (IDatabaseMetric).IsAssignableFrom(queryType)
                                 ? _DatabaseMetricQueryMethod.MakeGenericMethod(queryType)
                                 : _QueryMethod.MakeGenericMethod(queryType);

            _metricMappers = GetMappers(queryType);
        }

        public MetricTransformEnum MetricTransformEnum
        {
            get { return QueryAttribute.MetricTransformEnum; }
        }

        public string QueryName
        {
            get { return QueryAttribute.QueryName; }
        }

        public string MetricPattern { get; private set; }
        public string ResultTypeName { get; private set; }

        public string ResourceName
        {
            get { return QueryAttribute.ResourceName; }
        }

        public string CommandText { get; private set; }

        /// <summary>
        ///     Queries data from the database and returns the results.
        /// </summary>
        /// <param name="dbConnection">Open connection to the database.</param>
        /// <param name="endpoint">Settings for the endpoint that is queried.</param>
        /// <returns>
        ///     An enumeration of a the type where the <see cref="SqlServerQueryAttribute" /> for this query object was found during initialization.
        /// </returns>
        public IEnumerable<object> Query(IDbConnection dbConnection, ISqlEndpoint endpoint)
        {
            return ((IEnumerable) _genericMethod.Invoke(this, new object[] {dbConnection, endpoint,})).Cast<object>();
        }

        public void AddMetrics(QueryContext context)
        {
            context.Results.ForEach(r => _metricMappers.ForEach(m => m.AddMetric(context, r)));
        }

        /// <summary>
        ///     Never called directly, rather called via reflection.
        /// </summary>
        internal IEnumerable<T> Query<T>(IDbConnection dbConnection, ISqlEndpoint endpoint)
            where T : class, new()
        {
            // Pass the simple Id=1 anonymous object to support Dapper's hashing and caching of queries
            return _dapperWrapper.Query<T>(dbConnection, CommandText, new {Id = 1});
        }

        /// <summary>
        ///     Never called directly, rather called via reflection.
        /// </summary>
        internal IEnumerable<T> DatabaseMetricQuery<T>(IDbConnection dbConnection, ISqlEndpoint endpoint)
            where T : IDatabaseMetric, new()
        {
            var commandText = PrepareCommandText<T>(CommandText, endpoint);
            // Pass the simple Id=1 anonymous object to support Dapper's hashing and caching of queries
            return _dapperWrapper.Query<T>(dbConnection, commandText, new {Id = 1});
        }

        internal static string PrepareCommandText<T>(string commandText, ISqlEndpoint endpoint)
            where T : IDatabaseMetric, new()
        {
            var metricInstance = (IDatabaseMetric) new T();
            return metricInstance.ParameterizeQuery(commandText, endpoint);
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
