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

		public SqlServer(string name, string connectionString, bool includeSystemDatabases, IEnumerable<Database> includedDbs, IEnumerable<string> excludedDatabaseNames)
			: base(name, connectionString)
		{
			var excludedDbs = new List<string>();

			if (!includeSystemDatabases)
			{
				excludedDbs.AddRange(Constants.SystemDatabases);
			}

			if (excludedDatabaseNames != null)
			{
				excludedDbs.AddRange(excludedDatabaseNames);
			}

			IncludedDatabases = includedDbs != null ? includedDbs.ToArray() : new Database[0];
			ExcludedDatabaseNames = excludedDbs.ToArray();
		}

		public Database[] IncludedDatabases { get; private set; }

		public string[] IncludedDatabaseNames
		{
			get { return IncludedDatabases.Select(d => d.Name).ToArray(); }
		}

		public string[] ExcludedDatabaseNames { get; private set; }

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
			                     String.Join(", ", includedDatabases),
			                     String.Join(", ", excludedDatabases));
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

			Dictionary<string, string> renameMap = includedDatabases.Where(d => !String.IsNullOrEmpty(d.DisplayName)).ToDictionary(d => d.Name.ToLower(), d => d.DisplayName);
			if (!renameMap.Any())
			{
				return;
			}

			IDatabaseMetric[] databaseMetrics = results.OfType<IDatabaseMetric>().Where(d => !String.IsNullOrEmpty(d.DatabaseName)).ToArray();
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
