using System.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    internal class SqlServerElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return ((string) (base["name"])); }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("connectionString", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string ConnectionString
        {
            get { return ((string) (base["connectionString"])); }
            set { base["connectionString"] = value; }
        }

        [ConfigurationProperty("includeSystemDatabases", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool IncludeSystemDatabases
        {
            get { return ((bool) (base["includeSystemDatabases"])); }
            set { base["includeSystemDatabases"] = value; }
        }

        [ConfigurationProperty("includes")]
        public DatabaseCollection IncludedDatabases
        {
            get { return ((DatabaseCollection) (base["includes"])); }
        }

        [ConfigurationProperty("excludes")]
        public DatabaseCollection ExcludedDatabases
        {
            get { return ((DatabaseCollection) (base["excludes"])); }
        }
    }
}
