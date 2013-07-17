using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;

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
			ServiceName = ServiceConstants.ServiceName;
		}

		public static Settings Default { get; internal set; }

		public string LicenseKey { get; set; }
		public int PollIntervalSeconds { get; set; }
		public string ServiceName { get; set; }
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

		internal static Settings FromConfigurationSection(NewRelicConfigurationSection section, ILog log)
		{
			IEnumerable<ISqlEndpoint> sqlEndpoints = section.SqlServers
			                                                .Select(s =>
			                                                        {
				                                                        Database[] includedDatabaseNames = s.IncludedDatabases.Select(d => d.ToDatabase()).ToArray();
				                                                        string[] excludedDatabaseNames = s.ExcludedDatabases.Select(d => d.Name).ToArray();
				                                                        return
					                                                        (ISqlEndpoint) new SqlServerEndpoint(s.Name, s.ConnectionString, s.IncludeSystemDatabases, includedDatabaseNames, excludedDatabaseNames);
			                                                        })
			                                                .Union(section.AzureSqlDatabases.Select(s => (ISqlEndpoint) new AzureSqlEndpoint(s.Name, s.ConnectionString)));

			ServiceElement service = section.Service;
			var settings = new Settings(sqlEndpoints.ToArray())
			               {
				               LicenseKey = service.LicenseKey,
				               PollIntervalSeconds = service.PollIntervalSeconds,
			               };

			if (!string.IsNullOrEmpty(service.ServiceName) && Regex.IsMatch(service.ServiceName, "^[a-zA-Z_0-9-]{6,32}$"))
			{
				settings.ServiceName = service.ServiceName;
			}

			return settings;
		}

		public void ToLog(ILog log)
		{
			log.Debug("\tVersion: " + Version);
			log.Debug("\tPollIntervalSeconds: " + PollIntervalSeconds);
			log.Debug("\tCollectOnly: " + CollectOnly);
			log.Debug("\tTotalEndpoints: " + Endpoints.Length);

			var sqlServerEndpoints = Endpoints.OfType<SqlServerEndpoint>().ToArray();
			log.DebugFormat("\t\tSqlServerEndpoints: {0}", sqlServerEndpoints.Count());
			log.DebugFormat("\t\tPluginGUID: {0}", Constants.SqlServerComponentGuid);
			foreach (ISqlEndpoint endpoint in sqlServerEndpoints)
			{
				endpoint.Trace(log);
			}

			var azureEndpoints = Endpoints.OfType<AzureSqlEndpoint>().ToArray();
			log.DebugFormat("\t\tAzureEndpoints: {0}", azureEndpoints.Count());
			log.DebugFormat("\t\tPluginGUID: {0}", Constants.SqlAzureComponentGuid);
			foreach (ISqlEndpoint endpoint in azureEndpoints)
			{
				endpoint.Trace(log);
			}
		}

		public override string ToString()
		{
			return string.Format("Version: {0}, PollIntervalSeconds: {1}, CollectOnly: {2}", Version, PollIntervalSeconds, CollectOnly);
		}
	}
}
