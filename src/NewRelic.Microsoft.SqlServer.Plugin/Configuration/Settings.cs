using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public class Settings
	{
		private string _version;

		public Settings(ISqlEndpoint[] endpoints)
		{
			Endpoints = endpoints;
			PollIntervalSeconds = 60;
		}

		public string LicenseKey { get; set; }
		public int PollIntervalSeconds { get; set; }
		public ISqlEndpoint[] Endpoints { get; private set; }
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
			var sqlServerEndpoints = section.SqlServers
			                                   .Select(s =>
			                                           {
				                                           var includedDatabaseNames = s.IncludedDatabases.Select(d => d.ToDatabase()).ToArray();
				                                           var excludedDatabaseNames = s.ExcludedDatabases.Select(d => d.Name).ToArray();
				                                           return (ISqlEndpoint)new SqlEndpoint(s.Name, s.ConnectionString, s.IncludeSystemDatabases, includedDatabaseNames, excludedDatabaseNames);
			                                           })
			                                   .ToArray();
			var service = section.Service;
			return new Settings(sqlServerEndpoints)
			       {
				       LicenseKey = service.LicenseKey,
				       PollIntervalSeconds = service.PollIntervalSeconds,
			       };
		}

		public void ToLog(ILog log)
		{
			log.Debug("\tVersion: " + Version);
			log.Debug("\tPollIntervalSeconds: " + PollIntervalSeconds);
			log.Debug("\tCollectOnly: " + CollectOnly);
			log.Debug("\tSqlServers: " + Endpoints.Length);
			foreach (var endpoint in Endpoints)
			{
				// Remove password from logging
				var safeConnectionString = new SqlConnectionStringBuilder(endpoint.ConnectionString);
				if (!string.IsNullOrEmpty(safeConnectionString.Password))
				{
					safeConnectionString.Password = "[redacted]";
				}
				log.DebugFormat("\t\t{0}: {1}", endpoint.Name, safeConnectionString);

				foreach (var database in endpoint.IncludedDatabases)
				{
					log.Debug("\t\t\tIncluding: " + database.Name);
				}

				foreach (var database in endpoint.ExcludedDatabaseNames)
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
