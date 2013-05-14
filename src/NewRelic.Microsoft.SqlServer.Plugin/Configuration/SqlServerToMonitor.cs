using System.Collections.Generic;
using System.Linq;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    public interface ISqlServerToMonitor {
        string Name { get; }
        string ConnectionString { get; }
        string[] IncludedDatabases { get; }
        string[] ExcludedDatabases { get; }
        string ToString();
    }

    public class SqlServerToMonitor : ISqlServerToMonitor
    {
		public SqlServerToMonitor(string name, string connectionString, bool includeSystemDatabases)
			: this(name, connectionString, includeSystemDatabases, null, null) {}

		public SqlServerToMonitor(string name, string connectionString, bool includeSystemDatabases, IEnumerable<string> includedDatabaseNames, IEnumerable<string> excludedDatabaseNames)
		{
			Name = name;
			ConnectionString = connectionString;
			var includedDbs = new List<string>();
			var excludedDbs = new List<string>();

			if (!includeSystemDatabases)
			{
				excludedDbs.AddRange(Constants.SystemDatabases);
			}

			if (includedDatabaseNames != null)
			{
				includedDbs.AddRange(includedDatabaseNames.Select(TransformDatabaseName));
			}

			if (excludedDatabaseNames != null)
			{
				excludedDbs.AddRange(excludedDatabaseNames.Select(TransformDatabaseName));
			}

			IncludedDatabases = includedDbs.ToArray();
			ExcludedDatabases = excludedDbs.ToArray();
		}

		public string Name { get; private set; }
		public string ConnectionString { get; private set; }

		public string[] IncludedDatabases { get; private set; }
		public string[] ExcludedDatabases { get; private set; }

		/// <summary>
		///     Used to transform a the database name string from the configuration file into a sql ready database name
		/// </summary>
		/// <param name="name">Database name from the configuration file</param>
		/// <returns>Formatted and qualified sql ready string representing a database name</returns>
		private static string TransformDatabaseName(string name)
		{
			return name.Replace('*', '%');
		}

		public override string ToString()
		{
			return FormatProperties(Name, ConnectionString, IncludedDatabases, ExcludedDatabases);
		}

		public static string FormatProperties(string name, string connectionString, string[] includedDatabases, string[] excludedDatabases)
		{
			return string.Format("Name: {0}, ConnectionString: {1}, IncludedDatabases: {2}, ExcludedDatabases: {3}",
			                     name,
			                     connectionString,
			                     string.Join(", ", includedDatabases),
			                     string.Join(", ", excludedDatabases));
		}
	}
}
