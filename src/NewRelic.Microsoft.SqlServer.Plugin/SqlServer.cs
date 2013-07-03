using System;
using System.Collections.Generic;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class SqlServer : SqlEndpoint
	{
		public SqlServer(string name, string connectionString, bool includeSystemDatabases)
			: this(name, connectionString, includeSystemDatabases, null, null) {}

		public SqlServer(string name, string connectionString, bool includeSystemDatabases, IEnumerable<Database> includedDatabases, IEnumerable<string> excludedDatabaseNames)
			: base(name, connectionString)
		{
			var excludedDbs = new List<string>();
			var includedDbs = new List<Database>();

			if (excludedDatabaseNames != null)
			{
				excludedDbs.AddRange(excludedDatabaseNames);
			}

			if (includedDatabases != null)
			{
				includedDbs.AddRange(includedDatabases);
			}

			if (includeSystemDatabases && includedDbs.Any())
			{
				IEnumerable<Database> systemDbsToAdd = Constants.SystemDatabases
				                                                .Where(dbName => includedDbs.All(db => db.Name != dbName))
				                                                .Select(dbName => new Database {Name = dbName});
				includedDbs.AddRange(systemDbsToAdd);
			}
			else if (!includeSystemDatabases)
			{
				IEnumerable<string> systemDbsToAdd = Constants.SystemDatabases
				                                              .Where(dbName => excludedDbs.All(db => db != dbName));
				excludedDbs.AddRange(systemDbsToAdd);
			}

			IncludedDatabases = includedDbs.ToArray();
			ExcludedDatabaseNames = excludedDbs.ToArray();
		}

		protected override string ComponentGuid
		{
			get { return Constants.SqlServerComponentGuid; }
		}

		public override string ToString()
		{
			return FormatProperties(Name, ConnectionString, IncludedDatabaseNames, ExcludedDatabaseNames);
		}

		internal static string FormatProperties(string name, string connectionString, string[] includedDatabases, string[] excludedDatabases)
		{
			return String.Format("Name: {0}, ConnectionString: {1}, IncludedDatabaseNames: {2}, ExcludedDatabaseNames: {3}",
			                     name,
			                     connectionString,
			                     string.Join(", ", includedDatabases),
			                     string.Join(", ", excludedDatabases));
		}

		/// <summary>
		///     Replaces the database name on <see cref="IDatabaseMetric" /> results with the <see cref="Database.DisplayName" /> when possible.
		/// </summary>
		/// <param name="includedDatabases"></param>
		/// <param name="results"></param>
		internal static void ApplyDatabaseDisplayNames(IEnumerable<Database> includedDatabases, object[] results)
		{
			if (includedDatabases == null)
			{
				return;
			}

			Dictionary<string, string> renameMap = includedDatabases.Where(d => !string.IsNullOrEmpty(d.DisplayName)).ToDictionary(d => d.Name.ToLower(), d => d.DisplayName);
			if (!renameMap.Any())
			{
				return;
			}

			IDatabaseMetric[] databaseMetrics = results.OfType<IDatabaseMetric>().Where(d => !string.IsNullOrEmpty(d.DatabaseName)).ToArray();
			if (!databaseMetrics.Any())
			{
				return;
			}

			foreach (IDatabaseMetric databaseMetric in databaseMetrics)
			{
				string displayName;
				if (renameMap.TryGetValue(databaseMetric.DatabaseName.ToLower(), out displayName))
				{
					databaseMetric.DatabaseName = displayName;
				}
			}
		}

		protected internal override IEnumerable<SqlQuery> FilterQueries(IEnumerable<SqlQuery> queries)
		{
			return queries.Where(q => q.QueryAttribute is SqlServerQueryAttribute);
		}

		public override void Trace(ILog log)
		{
			base.Trace(log);

			foreach (Database database in IncludedDatabases)
			{
				log.Debug("\t\t\tIncluding: " + database.Name);
			}

			foreach (string database in ExcludedDatabaseNames)
			{
				log.Debug("\t\t\tExcluding: " + database);
			}
		}

		protected override object[] OnQueryExecuted(ISqlQuery query, object[] results, ILog log)
		{
			results = base.OnQueryExecuted(query, results, log);

			ApplyDatabaseDisplayNames(IncludedDatabases, results);
			return results;
		}
	}
}
