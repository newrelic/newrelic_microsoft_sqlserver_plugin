using System;
using System.Collections.Generic;
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

		public IEnumerable<SqlQuery> PrepareQueries()
		{
			// Look for all query types
			var types = _assembly.GetTypes();

			return PrepareQueries(types);
		}

		public IEnumerable<SqlQuery> PrepareQueries(Type[] types, bool onlyEnabledQueries = true)
		{
			// Search for types with at least one attribute that have a QueryAttribute
			return types.SelectMany(t => t.GetCustomAttributes<QueryAttribute>()
			                              .Where(a => !onlyEnabledQueries || a.Enabled)
			                              .Select(a => new SqlQuery(t, a, _dapper)))
			            .ToArray();
		}
	}
}
