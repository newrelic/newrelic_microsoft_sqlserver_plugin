using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	internal class ProxyElement : ConfigurationElement
	{
		/// <summary>
		/// The proxy server host name. Required.
		/// </summary>
		[ConfigurationProperty("host", DefaultValue = "", IsKey = false, IsRequired = true)]
		public string Host
		{
			get { return ((string)(base["host"])); }
			set { base["host"] = value; }
		}
		/// <summary>
		/// The proxy server port (optional - defaults to 8080).
		/// </summary>
		[ConfigurationProperty("port", DefaultValue = "8080", IsKey = false, IsRequired = false)]
		public string Port
		{
			get { return ((string)(base["port"])); }
			set { base["port"] = value; }
		}
		/// <summary>
		/// The username used to authenticate with the proxy server (optional).
		/// </summary>
		[ConfigurationProperty("user", DefaultValue = "", IsKey = false, IsRequired = false)]
		public string User
		{
			get { return ((string)(base["user"])); }
			set { base["user"] = value; }
		}
		/// <summary>
		/// The password used to authenticate with the proxy server (optional).
		/// </summary>
		[ConfigurationProperty("password", DefaultValue = "", IsKey = false, IsRequired = false)]
		public string Password
		{
			get { return ((string)(base["password"])); }
			set { base["password"] = value; }
		}
		/// <summary>
		/// The domain used to authenticate with the proxy server (optional).
		/// </summary>
		[ConfigurationProperty("domain", DefaultValue = "", IsKey = false, IsRequired = false)]
		public string Domain
		{
			get { return ((string)(base["domain"])); }
			set { base["domain"] = value; }
		}
		/// <summary>
		/// 'true' or 'false. Uses the credentials of the account running the plugin (optional - defaults to false).
		/// </summary>
		[ConfigurationProperty("useDefaultCredentials", DefaultValue = "false", IsKey = false, IsRequired = false)]
		public string UseDefaultCredentials
		{
			get { return ((string)(base["useDefaultCredentials"])); }
			set { base["useDefaultCredentials"] = value; }
		}
	}
}