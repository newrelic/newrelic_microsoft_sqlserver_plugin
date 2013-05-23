using System.Collections.Generic;
using System.Data;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	public interface IDapperWrapper
	{
		IEnumerable<T> Query<T>(IDbConnection connection, string sql, object param);
	}

    public class DapperWrapper : IDapperWrapper
	{
		public IEnumerable<T> Query<T>(IDbConnection connection, string sql, object param)
		{
			return connection.Query<T>(sql, param);
		}
	}
}
