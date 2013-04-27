using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class SqlMonitorQuery
	{
		private static readonly MethodInfo _GenericQueryMethod = typeof (SqlMonitorQuery).GetMethod("Query", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		private readonly IDapperWrapper _dapperWrapper;
		private readonly Func<IDbConnection, IEnumerable<object>> _dynamicInvoke;

		internal SqlMonitorQuery(Type queryType, SqlMonitorQueryAttribute attribute, IDapperWrapper dapperWrapper)
		{
			_dapperWrapper = dapperWrapper;
			ResultTypeName = queryType.Name;
			ResourceName = attribute.ResourceName;

			// Get the SQL resource the same assembly as the type
			CommandText = queryType.Assembly.SearchForStringResources(attribute.ResourceName);

			// Get a pointer to the Query method below with the QueryType as the generic parameter
			var genericMethod = _GenericQueryMethod.MakeGenericMethod(queryType);

			// Provide a Func that takes a IDbConnection and returns the result set
			_dynamicInvoke = conn => ((IEnumerable) genericMethod.Invoke(this, new object[] {conn})).Cast<object>();
		}

		public string ResultTypeName { get; private set; }
		public string ResourceName { get; private set; }
		public string CommandText { get; private set; }

		public IEnumerable<object> Invoke(IDbConnection dbConnection)
		{
			return _dynamicInvoke(dbConnection);
		}

		public IEnumerable<T> Query<T>(IDbConnection dbConnection)
		{
			return _dapperWrapper.Query<T>(dbConnection, CommandText, new {Id = 1});
		}
	}
}
