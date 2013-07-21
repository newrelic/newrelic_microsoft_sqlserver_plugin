using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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

			var identity = WindowsIdentity.GetCurrent();
			if (identity != null)
			{
				var principal = new WindowsPrincipal(identity);
				IsProcessElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		public static Settings Default { get; internal set; }

		public string LicenseKey { get; set; }
		public int PollIntervalSeconds { get; set; }
		public string ServiceName { get; set; }
		public ISqlEndpoint[] Endpoints { get; private set; }
		public bool CollectOnly { get; set; }
		public bool IsProcessElevated { get; private set; }

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
			var sqlEndpoints = section.SqlServers
			                          .Select(s =>
			                                  {
				                                  var includedDatabaseNames = s.IncludedDatabases.Select(d => d.ToDatabase()).ToArray();
				                                  var excludedDatabaseNames = s.ExcludedDatabases.Select(d => d.Name).ToArray();
				                                  return
					                                  (ISqlEndpoint) new SqlServerEndpoint(s.Name, s.ConnectionString, s.IncludeSystemDatabases, includedDatabaseNames, excludedDatabaseNames);
			                                  })
			                          .Union(section.AzureSqlDatabases.Select(s => (ISqlEndpoint) new AzureSqlEndpoint(s.Name, s.ConnectionString)));

			var service = section.Service;
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
			log.Info("  Version: " + Version);
			log.Info("  PollIntervalSeconds: " + PollIntervalSeconds);
			log.Info("  CollectOnly: " + CollectOnly);
			log.Info("  RunAsAdministrator: " + IsProcessElevated);
			log.Info("  TotalEndpoints: " + Endpoints.Length);

			var sqlServerEndpoints = Endpoints.OfType<SqlServerEndpoint>().ToArray();
			if (sqlServerEndpoints.Any())
			{
				log.InfoFormat("    SqlServerEndpoints: {0}", sqlServerEndpoints.Count());
				log.InfoFormat("    PluginGUID: {0}", Constants.SqlServerComponentGuid);
				foreach (ISqlEndpoint endpoint in sqlServerEndpoints)
				{
					endpoint.ToLog(log);
				}
			}
			else
			{
				log.Debug("No SQL Server endpoints configured.");
			}

			var azureEndpoints = Endpoints.OfType<AzureSqlEndpoint>().ToArray();
			if (azureEndpoints.Any())
			{
				log.InfoFormat("    AzureEndpoints: {0}", azureEndpoints.Count());
				log.InfoFormat("    PluginGUID: {0}", Constants.SqlAzureComponentGuid);
				foreach (ISqlEndpoint endpoint in azureEndpoints)
				{
					endpoint.ToLog(log);
				}
			}
			else
			{
				log.Debug("No Azure SQL endpoints configured.");
			}
		}

		public override string ToString()
		{
			return string.Format("Version: {0}, PollIntervalSeconds: {1}, CollectOnly: {2}", Version, PollIntervalSeconds, CollectOnly);
		}
	}
}
