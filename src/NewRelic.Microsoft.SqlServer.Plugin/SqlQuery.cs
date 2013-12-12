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
	public class SqlQuery : MetricQuery, ISqlQuery
	{
		/// <summary>Stores a pointer to the generic method so this reflection isn't repeated later.</summary>
		private static readonly MethodInfo _QueryMethod = typeof (SqlQuery).GetMethod("Query", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly MethodInfo _DatabaseMetricQueryMethod = typeof (SqlQuery).GetMethod("DatabaseMetricQuery", BindingFlags.Instance | BindingFlags.NonPublic);

		private readonly IDapperWrapper _dapperWrapper;
		private readonly MethodInfo _genericMethod;
		internal QueryAttribute QueryAttribute;

		public SqlQuery(Type queryType, QueryAttribute attribute, IDapperWrapper dapperWrapper, string commandText = null)
			: base(queryType, attribute.QueryName, queryType.Name, attribute.MetricTransformEnum)
		{
			_dapperWrapper = dapperWrapper;
			QueryAttribute = attribute;

			if (attribute.MetricPattern != null)
			{
				MetricPattern = attribute.MetricPattern;
			}

			// Get the SQL resource from the same assembly as the type, when commandText is not supplied
			CommandText = commandText ?? queryType.Assembly.SearchForStringResource(attribute.ResourceName);

			// Get a pointer to the correctly typed Query method below with the QueryType as the generic parameter
			_genericMethod = typeof (IDatabaseMetric).IsAssignableFrom(queryType)
				                 ? _DatabaseMetricQueryMethod.MakeGenericMethod(queryType)
				                 : _QueryMethod.MakeGenericMethod(queryType);
		}

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
	}
}
