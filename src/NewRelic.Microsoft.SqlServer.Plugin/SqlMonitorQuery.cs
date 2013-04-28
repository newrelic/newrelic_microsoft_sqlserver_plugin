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
	public class SqlMonitorQuery
	{
		private static readonly MethodInfo _GenericQueryMethod = typeof (SqlMonitorQuery).GetMethod("Query", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		private readonly IDapperWrapper _dapperWrapper;
		private readonly Func<IDbConnection, IEnumerable<IQueryResult>> _dynamicInvoke;

		internal SqlMonitorQuery(Type queryType, SqlMonitorQueryAttribute attribute, IDapperWrapper dapperWrapper)
		{
			_dapperWrapper = dapperWrapper;
			ResultTypeName = queryType.Name;
			QueryName = attribute.QueryName;
			ResourceName = attribute.ResourceName;

			// Get the SQL resource the same assembly as the type
			CommandText = queryType.Assembly.SearchForStringResource(attribute.ResourceName);

			// Get a pointer to the Query method below with the QueryType as the generic parameter
			var genericMethod = _GenericQueryMethod.MakeGenericMethod(queryType);

			// Provide a Func that takes a IDbConnection and returns the result set
			_dynamicInvoke = conn => ((IEnumerable) genericMethod.Invoke(this, new object[] {conn})).Cast<IQueryResult>();
		}

		public string QueryName { get; private set; }
		public string ComponentGuid { get; private set; }
		public string ResultTypeName { get; private set; }
		public string ResourceName { get; private set; }
		public string CommandText { get; private set; }

		public IEnumerable<IQueryResult> Invoke(IDbConnection dbConnection)
		{
			return _dynamicInvoke(dbConnection);
		}

		public IEnumerable<T> Query<T>(IDbConnection dbConnection)
			where T : IQueryResult
		{
			return _dapperWrapper.Query<T>(dbConnection, CommandText, new {Id = 1});
		}
	}
}
