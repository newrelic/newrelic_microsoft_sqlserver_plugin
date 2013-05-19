using System.Collections.Generic;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class AzureSqlDatabase : SqlEndpoint
	{
		public AzureSqlDatabase(string name, string connectionString)
			: base(name, connectionString) {}

		protected internal override IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries)
		{
			return queries.Where(q => q.QueryAttribute is AzureSqlQueryAttribute);
		}
	}
}
