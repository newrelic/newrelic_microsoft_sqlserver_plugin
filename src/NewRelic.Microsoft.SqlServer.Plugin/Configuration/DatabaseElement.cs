using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	internal class DatabaseElement : ConfigurationElement
	{
		[ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return ((string) (base["name"])); }
			set { base["name"] = value; }
		}

		[ConfigurationProperty("displayName", IsRequired = false)]
		public string DisplayName
		{
			get { return ((string) (base["displayName"])); }
			set { base["displayName"] = value; }
		}

		public Database ToDatabase()
		{
			var displayName = DisplayName;
			return new Database
			       {
				       Name = Name,
				       DisplayName = !string.IsNullOrEmpty(displayName) ? displayName : null,
			       };
		}
	}
}
