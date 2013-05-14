using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public class Settings
	{
		private string _version;

		public Settings(SqlServerToMonitor[] sqlServers)
		{
			SqlServers = sqlServers;
			PollIntervalSeconds = 60;
		}

		public string LicenseKey { get; set; }
		public bool UseSsl { get; set; }
		public int PollIntervalSeconds { get; set; }
		public SqlServerToMonitor[] SqlServers { get; private set; }
		public bool CollectOnly { get; set; }

		public string Version
		{
			get
			{
				if (_version == null)
				{
					var versionAttr = (AssemblyInformationalVersionAttribute) Assembly.GetExecutingAssembly().GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), true).First();
					_version = versionAttr.InformationalVersion;
				}
				return _version;
			}
		}

		internal static Settings FromConfigurationSection(NewRelicConfigurationSection section)
		{
			var sqlInstanceToMonitors = section.SqlServers
			                                   .Select(s =>
			                                           {
				                                           var includedDatabaseNames = s.IncludedDatabases.Select(d => d.Name).ToArray();
				                                           var excludedDatabaseNames = s.ExcludedDatabases.Select(d => d.Name).ToArray();
				                                           return new SqlServerToMonitor(s.Name, s.ConnectionString, s.IncludeSystemDatabases, includedDatabaseNames, excludedDatabaseNames);
			                                           })
			                                   .ToArray();
			var service = section.Service;
			return new Settings(sqlInstanceToMonitors)
			       {
				       LicenseKey = service.LicenseKey,
				       PollIntervalSeconds = service.PollIntervalSeconds,
				       UseSsl = service.UseSsl,
			       };
		}

		public void ToLog(ILog log)
		{
			log.Debug("\tVersion: " + Version);
			log.Debug("\tPollIntervalSeconds: " + PollIntervalSeconds);
			log.Debug("\tCollectOnly: " + CollectOnly);
			log.Debug("\tSqlServers: " + SqlServers.Length);
			foreach (var sqlServer in SqlServers)
			{
				// Remove password from logging
				var safeConnectionString = new SqlConnectionStringBuilder(sqlServer.ConnectionString);
				if (!string.IsNullOrEmpty(safeConnectionString.Password))
				{
					safeConnectionString.Password = "[redacted]";
				}
				log.DebugFormat("\t\t{0}: {1}", sqlServer.Name, safeConnectionString);

				foreach (var database in sqlServer.IncludedDatabases)
				{
					log.Debug("\t\t\tIncluding: " + database);
				}

				foreach (var database in sqlServer.ExcludedDatabases)
				{
					log.Debug("\t\t\tExcluding: " + database);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("Version: {0}, PollIntervalSeconds: {1}, CollectOnly: {2}", Version, PollIntervalSeconds, CollectOnly);
		}
	}
}
