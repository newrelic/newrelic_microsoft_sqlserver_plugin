using System;
using System.Collections.Generic;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class AzureSqlDatabase : SqlEndpoint
	{
		private readonly ILog _log;

		public AzureSqlDatabase(string name, string connectionString, ILog log)
			: base(name, connectionString)
		{
			_log = log;
			SqlDmlActivityHistory = new Dictionary<string, SqlDmlActivity>();
		}

		protected override string ComponentGuid
		{
			get { return Constants.SqlAzureComponentGuid; }
		}

		protected internal override IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries)
		{
			return queries.Where(q => q.QueryAttribute is AzureSqlQueryAttribute);
		}
	}
}
