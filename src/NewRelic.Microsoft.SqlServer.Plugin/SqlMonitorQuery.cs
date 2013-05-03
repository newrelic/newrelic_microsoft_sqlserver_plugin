using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class SqlMonitorQuery
	{
		/// <summary>Stores a pointer to the generic method so this reflection isn't repeated later.</summary>
		private static readonly MethodInfo _GenericQueryMethod = typeof (SqlMonitorQuery).GetMethod("Query", BindingFlags.Instance | BindingFlags.NonPublic);

		private readonly IDapperWrapper _dapperWrapper;
		private readonly MethodInfo _genericMethod;
		private readonly MetricMapper[] _metricMappers;
		private string _metricPattern;

		internal SqlMonitorQuery(Type queryType, QueryAttribute attribute, IDapperWrapper dapperWrapper, string commandText = null)
		{
			_dapperWrapper = dapperWrapper;
			ResultTypeName = queryType.Name;
			QueryName = attribute.QueryName;
			ResourceName = attribute.ResourceName;
			MetricPattern = attribute.MetricPattern;

			// Get the SQL resource from the same assembly as the type, when commandText is not supplied
			CommandText = commandText ?? queryType.Assembly.SearchForStringResource(attribute.ResourceName);

			// Get a pointer to the correctly typed Query method below with the QueryType as the generic parameter
			_genericMethod = _GenericQueryMethod.MakeGenericMethod(queryType);

			_metricMappers = GetMappers(queryType);
		}

		public string QueryName { get; private set; }

		public string MetricPattern
		{
			get { return _metricPattern ?? string.Format("Custom/{0}", ResultTypeName); }
			set { _metricPattern = value; }
		}

		public string ResultTypeName { get; private set; }
		public string ResourceName { get; private set; }
		public string CommandText { get; private set; }

		/// <summary>
		/// Queries data from the database and returns the results
		/// </summary>
		/// <param name="dbConnection"></param>
		/// <returns>An enumeration of a the type where the <see cref="QueryAttribute"/> for this query object was found during initialization</returns>
		public IEnumerable<object> Query(IDbConnection dbConnection)
		{
			return ((IEnumerable) _genericMethod.Invoke(this, new object[] {dbConnection})).Cast<object>();
		}

		public void AddMetrics(QueryContext context)
		{
			context.Results.ForEach(r => _metricMappers.ForEach(m => m.AddMetric(context, r)));
		}

		/// <summary>
		/// Never called directly, rather called via reflection.
		/// </summary>
		protected IEnumerable<T> Query<T>(IDbConnection dbConnection)
		{
			// Pass the simple Id=1 anonymous object to support Dapper's hashing and caching of queries
			return _dapperWrapper.Query<T>(dbConnection, CommandText, new {Id = 1});
		}

		/// <summary>
		/// Sets up the mappers that take the values on the query result and records each one as a metric.
		/// </summary>
		/// <param name="type"><em>QueryType</em> to look for metric properties</param>
		/// <returns>An array of mappers capable of creating metrics for a <em>QueryType</em></returns>
		internal static MetricMapper[] GetMappers(Type type)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			           .Select(MetricMapper.TryCreate)
			           .Where(m => m != null)
			           .ToArray();
		}
	}
}
