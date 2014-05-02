using System.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    internal class ServiceElement : ConfigurationElement
    {
        /// <summary>
        /// New Relic license key. Required.
        /// </summary>
        [ConfigurationProperty("licenseKey", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string LicenseKey
        {
            get { return ((string) (base["licenseKey"])); }
            set { base["licenseKey"] = value; }
        }

        /// <summary>
        /// Number of seconds between polling of the servers. Default is 60.
        /// </summary>
        [ConfigurationProperty("pollIntervalSeconds", DefaultValue = 60, IsKey = false, IsRequired = false)]
        public int PollIntervalSeconds
        {
            get { return (int?)(base["pollIntervalSeconds"]) ?? 60; }
            set { base["pollIntervalSeconds"] = value; }
        }
    }
}