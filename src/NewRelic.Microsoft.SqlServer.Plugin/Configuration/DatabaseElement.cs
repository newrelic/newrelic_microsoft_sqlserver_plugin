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
    }
}