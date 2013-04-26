using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	internal class QueryLocator
	{
		private readonly IDapperWrapper _dapper;

		public QueryLocator(IDapperWrapper dapper)
		{
			_dapper = dapper;
		}

		public IEnumerable<Func<IDbConnection, IEnumerable<object>>> PrepareQueries()
		{
			// The SQL resources are in this assembly
			var assembly = Assembly.GetExecutingAssembly();
			// The "root" namespace is prepended to the ResourceName of the configured SqlMonitorQueryAttribute attribute
			var rootNamespace = typeof (Program).Namespace;
			// Get the static, generic Query method in this class
			var genericQueryMethod = GetType().GetMethod("Query", BindingFlags.Instance | BindingFlags.NonPublic);

			// Look for all query types
			return assembly.GetTypes().Select(t => new {QueryType = t, Attribute = (SqlMonitorQueryAttribute) t.GetCustomAttributes(typeof (SqlMonitorQueryAttribute), false).FirstOrDefault(),})
				// that have a SqlMonitorQueryAttribute
			               .Where(x => x.Attribute != null)
			               .Select(x => new
			                            {
				                            x.QueryType.Name,
				                            // Get the SQL resource from this assembly
				                            Query = assembly.GetStringFromResources(rootNamespace + "." + x.Attribute.ResourceName),
				                            // Get a pointer to the Query method below with the QueryType as the generic parameter
				                            Method = genericQueryMethod.MakeGenericMethod(x.QueryType),
			                            })
				// Provide a Func that takes a IDbConnection and returns the result set
			               .Select(x => new Func<IDbConnection, IEnumerable<object>>(conn => ((IEnumerable) x.Method.Invoke(this, new object[] {conn, x.Query})).Cast<object>()))
			               .ToArray();
		}

		/// <summary>
		/// Used by reflection. Look for 'MakeGenericMethod'.
		/// </summary>
		// ReSharper disable UnusedMember.Local
		private IEnumerable<T> Query<T>(IDbConnection connection, string query)
			// ReSharper restore UnusedMember.Local
		{
			// Use Dapper to execute the query and map the result set
			return _dapper.Query<T>(connection, query, new {Id = 1});
		}
	}
}
