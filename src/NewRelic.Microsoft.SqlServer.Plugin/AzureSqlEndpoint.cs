using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class AzureSqlEndpoint : SqlEndpointBase
	{
		private readonly string _masterConnectionString;

		public AzureSqlEndpoint(string name, string connectionString)
			: base(name, connectionString)
		{
			SqlDmlActivityHistory = new Dictionary<string, SqlDmlActivity>();

			var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

			// For the queries against master, only return metrics for the target DB
			IncludedDatabases = new[] {new Database {Name = connectionStringBuilder.InitialCatalog},};

			// Prepare the master connection string
			connectionStringBuilder.InitialCatalog = "master";
			_masterConnectionString = connectionStringBuilder.ToString();
		}

		protected override string ComponentGuid
		{
			get { return Constants.SqlAzureComponentGuid; }
		}

		protected internal override IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries)
		{
			return queries.Where(q => q.QueryAttribute is AzureSqlQueryAttribute);
		}

		public override IEnumerable<IQueryContext> ExecuteQueries(ILog log)
		{
			return base.ExecuteQueries(log).Concat(PerformMasterDatabaseQueries(log));
		}

		/// <summary>
		///     Runs a query against a connection to the master DB on this Azure SQL Server
		/// </summary>
		/// <param name="log"></param>
		/// <returns></returns>
		internal IEnumerable<IQueryContext> PerformMasterDatabaseQueries(ILog log)
		{
			var queries = new QueryLocator(new DapperWrapper())
               .PrepareQueries(new[]
                               {
                                    typeof (AzureServiceInterruptionEvents),
                                    typeof (AzureSqlResourceStats)
                               }, false).ToArray();
			return ExecuteQueries(queries, _masterConnectionString, log);
		}
	}
}
