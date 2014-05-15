using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    internal class AzureSqlDatabaseElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string)(base["name"])); }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("connectionString", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string ConnectionString
        {
            get { return ((string)(base["connectionString"])); }
            set { base["connectionString"] = value; }
        }
    }
}