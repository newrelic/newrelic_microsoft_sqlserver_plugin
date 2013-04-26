using System.Collections.Generic;
using System.Data;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	internal interface IDapperWrapper
	{
		IEnumerable<T> Query<T>(IDbConnection connection, string sql, object param);
	}

	internal class DapperWrapper : IDapperWrapper
	{
		public IEnumerable<T> Query<T>(IDbConnection connection, string sql, object param)
		{
			return connection.Query<T>(sql, param);
		}
	}
}
