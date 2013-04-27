using System.Linq;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public class Settings
	{
		public Settings(SqlServerToMonitor[] sqlServers)
		{
			SqlServers = sqlServers;
			PollIntervalSeconds = 60;
		}

		public string LicenseKey { get; set; }
		public bool UseSsl { get; set; }
		public int PollIntervalSeconds { get; set; }
		public SqlServerToMonitor[] SqlServers { get; private set; }

		internal static Settings FromConfigurationSection(NewRelicConfigurationSection section)
		{
			var sqlInstanceToMonitors = section.SqlServers.Select(s => new SqlServerToMonitor(s.Name, s.ConnectionString)).ToArray();
			var service = section.Service;
			return new Settings(sqlInstanceToMonitors)
			       {
				       LicenseKey = service.LicenseKey,
				       PollIntervalSeconds = service.PollIntervalSeconds,
				       UseSsl = service.UseSsl,
			       };
		}
	}
}
