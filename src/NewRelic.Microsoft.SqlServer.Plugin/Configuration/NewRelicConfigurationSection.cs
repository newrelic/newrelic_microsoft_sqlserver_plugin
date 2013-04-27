using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	internal class NewRelicConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("service")]
		public ServiceElement Service
		{
			get { return ((ServiceElement) (base["service"])); }
		}

		[ConfigurationProperty("sqlServers")]
		public SqlServerCollection SqlServers
		{
			get { return ((SqlServerCollection) (base["sqlServers"])); }
		}
	}
}
