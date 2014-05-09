using System.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    internal class NewRelicConfiguration : ConfigurationSection
    {
        [ConfigurationProperty(Constants.ServiceString, IsRequired = true)]
        public ServiceElement Service
        {
            get { return ((ServiceElement)(base[Constants.ServiceString])); }
            set { base[Constants.ServiceString] = (ServiceElement)value; }
        }

        [ConfigurationProperty(Constants.ProxyString)]
        public ProxyElement Proxy
        {
            get { return ((ProxyElement)(base[Constants.ProxyString])); }
            set { base[Constants.ProxyString] = (ProxyElement)base[Constants.ProxyString]; }
        }

        [ConfigurationProperty(Constants.SqlServerString)]
        public SqlServerCollection SqlServers
        {
            get { return ((SqlServerCollection)(base[Constants.SqlServerString])); }
            set { base[Constants.SqlServerString] = (SqlServerCollection)value; }
        }

        [ConfigurationProperty(Constants.AzureString)]
        public AzureCollection AzureSqlDatabases
        {
            get { return ((AzureCollection)(base[Constants.AzureString])); }
            set { base[Constants.AzureString] = (AzureCollection)value; }
        }
    }
}
