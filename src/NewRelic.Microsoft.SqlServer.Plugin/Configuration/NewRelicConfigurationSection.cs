using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	internal class NewRelicConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("service", IsRequired = true)]
		public ServiceElement Service
		{
			get { return ((ServiceElement) (base["service"])); }
		}

		[ConfigurationProperty("proxy")]
		public ProxyElement Proxy
		{
			get { return ((ProxyElement)(base["proxy"])); }
		}

		[ConfigurationProperty("sqlServers")]
		public SqlServerCollection SqlServers
		{
			get { return ((SqlServerCollection) (base["sqlServers"])); }
		}

		[ConfigurationProperty("azure")]
		public AzureCollection AzureSqlDatabases
		{
			get { return ((AzureCollection)(base["azure"])); }
		}
	}
}
