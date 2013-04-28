using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	internal class QueryLocator
	{
		private readonly Assembly _assembly;
		private readonly IDapperWrapper _dapper;

		public QueryLocator(IDapperWrapper dapper, Assembly assembly = null)
		{
			_dapper = dapper;
			// The default is to look for SQL resources in this assembly
			_assembly = assembly ?? Assembly.GetExecutingAssembly();
		}

		public IEnumerable<SqlMonitorQuery> PrepareQueries()
		{
			// Look for all query types
			var types = _assembly.GetTypes();

			return PrepareQueries(types);
		}

		internal IEnumerable<SqlMonitorQuery> PrepareQueries(Type[] types)
		{
			// Search for types with at least one attribute that have a SqlMonitorQueryAttribute
			return types.SelectMany(t => t.GetCustomAttributes<SqlMonitorQueryAttribute>().Where(a => a.Enabled).Select(a => new SqlMonitorQuery(t, a, _dapper))).ToArray();
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
