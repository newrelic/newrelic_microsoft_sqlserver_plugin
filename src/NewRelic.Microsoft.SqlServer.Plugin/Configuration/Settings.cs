using System;
using System.Linq;
using System.Net;
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
		private static string _ProxyDetails;

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
		public bool TestMode { get; set; }
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

			var webProxy = GetWebProxy(section, log);
			if (webProxy != null)
			{
				WebRequest.DefaultWebProxy = webProxy;
			}

			return settings;
		}

		private static IWebProxy GetWebProxy(NewRelicConfigurationSection section, ILog log)
		{
			var proxyElement = section.Proxy;
			if (proxyElement == null || !proxyElement.ElementInformation.IsPresent) return null;

			Uri uri;
			if (!Uri.TryCreate(proxyElement.Host, UriKind.RelativeOrAbsolute, out uri))
			{
				log.ErrorFormat("Proxy host '{0}' is not a valid URI, skipping proxy.", proxyElement.Host);
				return null;	
			}

			int port;
			if (!int.TryParse(proxyElement.Port, out port))
			{
				log.ErrorFormat("Unable to parse proxy port from '{0}', skipping proxy. Expecting a number from 1-65535.", proxyElement.Port);
				return null;
			}

			WebProxy webProxy;
			try
			{
				webProxy = new WebProxy(proxyElement.Host, port);
			}
			catch (Exception e)
			{
				log.ErrorFormat("Proxy settings are invalid. {0}", e.Message);
				return null;
			}

			if ("true".Equals(proxyElement.UseDefaultCredentials, StringComparison.InvariantCultureIgnoreCase))
			{
				webProxy.UseDefaultCredentials = true;
				webProxy.Credentials = CredentialCache.DefaultCredentials;
				_ProxyDetails = string.Format("Proxy Server: {0}:{1} with default credentials", proxyElement.Host, port);
			}
			else if (!string.IsNullOrEmpty(proxyElement.User))
			{
				if (string.IsNullOrEmpty(proxyElement.Domain))
				{
					webProxy.Credentials = new NetworkCredential(proxyElement.User, proxyElement.Password);
					_ProxyDetails = string.Format("Proxy Server: {0}@{1}:{2}", proxyElement.User, proxyElement.Host, port);
				}
				else
				{
					webProxy.Credentials = new NetworkCredential(proxyElement.User, proxyElement.Password, proxyElement.Domain);
					_ProxyDetails = string.Format("Proxy Server: {0}\\{1}@{2}:{3}", proxyElement.Domain, proxyElement.User, proxyElement.Host, port);
				}
			}
			else
			{
				_ProxyDetails = string.Format("Proxy Server: {0}:{1}", proxyElement.Host, port);
			}

			return webProxy;
		}

		public void ToLog(ILog log)
		{
			// Pending review by New Relic before adding this information
			//			log.Info("  New Relic Key: " + LicenseKey);

			log.Info("  Version: " + Version);
			log.Info("  Test Mode: " + (TestMode ? "Yes" : "No"));
			log.Info("  Windows Service: " + (Environment.UserInteractive ? "No" : "Yes"));
			log.InfoFormat(@"  User: {0}\{1}", Environment.UserDomainName, Environment.UserName);
			log.Info("  Run as Administrator: " + (IsProcessElevated ? "Yes" : "No"));

			if (_ProxyDetails != null)
			{
				log.Info("  " + _ProxyDetails);
			}

			log.Info("  Total Endpoints: " + Endpoints.Length);
			log.Info("  Poll Interval Seconds: " + PollIntervalSeconds);

			var sqlServerEndpoints = Endpoints.OfType<SqlServerEndpoint>().ToArray();
			if (sqlServerEndpoints.Any())
			{
				log.InfoFormat("    SqlServerEndpoints: {0}", sqlServerEndpoints.Count());
				log.InfoFormat("    PluginGUID: {0}", Constants.SqlServerComponentGuid);
				foreach (ISqlEndpoint endpoint in sqlServerEndpoints)
				{
					endpoint.ToLog(log);
				}
				log.Info(string.Empty);
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
				log.Info(string.Empty);
			}
			else
			{
				log.Debug("No Azure SQL endpoints configured.");
			}
		}

		public override string ToString()
		{
			return string.Format("Version: {0}, PollIntervalSeconds: {1}, TestMode: {2}", Version, PollIntervalSeconds, TestMode);
		}
	}
}
