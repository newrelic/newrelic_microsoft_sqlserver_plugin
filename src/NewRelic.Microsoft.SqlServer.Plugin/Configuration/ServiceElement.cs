using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	internal class ServiceElement : ConfigurationElement
	{
		[ConfigurationProperty("licenseKey", DefaultValue = "", IsKey = false, IsRequired = true)]
		public string LicenseKey
		{
			get { return ((string) (base["licenseKey"])); }
			set { base["licenseKey"] = value; }
		}

		[ConfigurationProperty("pollIntervalSeconds", DefaultValue = 60, IsKey = false, IsRequired = false)]
		public int PollIntervalSeconds
		{
			get { return (int?)(base["pollIntervalSeconds"]) ?? 60; }
			set { base["pollIntervalSeconds"] = value; }
		}

		[ConfigurationProperty("useSsl", DefaultValue = false, IsKey = false, IsRequired = false)]
		public bool UseSsl
		{
			get { return (bool?)(base["useSsl"]) ?? false; }
			set { base["useSsl"] = value; }
		}
	}
}