using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Platform.Sdk.Configuration;
using NewRelic.Platform.Sdk.Extensions;
using NewRelic.Platform.Sdk.Utils;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    internal static class ConfigurationParser
    {
        private static readonly Logger _log = Logger.GetLogger(typeof(ConfigurationParser).Name);

        /// <summary>
        ///     Extracts the settings from a config file
        /// </summary>
        /// <returns>Settings parsed from the config file.</returns>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when 'newrelic.json' or 'plugin.json' cannot be located.
        /// </exception>
        public static Settings ParseSettings()
        {
            const string defaultConfigPath = @".\config\plugin.json";

            INewRelicConfig newrelicConfig = NewRelicConfig.Instance;
            string pluginConfigPath = Path.Combine(Assembly.GetExecutingAssembly().GetLocalPath(), defaultConfigPath);

            if (!File.Exists(pluginConfigPath))
            {
                throw new FileNotFoundException(@"Unable to locate the 'plugin.json' file in the .\config directory.", "plugin.json");
            }

            return GetSettingsFromConfigurationDirectory(pluginConfigPath);
        }

        private static Settings GetSettingsFromConfigurationDirectory(string pluginConfigPath)
        {
            INewRelicConfig newrelicConfig = NewRelicConfig.Instance;
            IDictionary<string, object> pluginConfigContents = JsonHelper.Deserialize(File.ReadAllText(pluginConfigPath)) as IDictionary<string, object>;

            List<object> configuredAgents = pluginConfigContents["agents"] as List<object>;

            NewRelicConfiguration newrelicConfigurationSection = new NewRelicConfiguration();

            // Set service level changes (e.g. license key, service name, proxy)
            AddServiceElement(newrelicConfigurationSection, newrelicConfig);
            AddProxyElement(newrelicConfigurationSection, newrelicConfig);

            // Set details for the SQL instances that are to be monitored
            AddSqlInstanceElements(newrelicConfigurationSection, configuredAgents);

            var settings = Settings.FromConfigurationSection(newrelicConfigurationSection);
            _log.Info("Settings loaded successfully");

            return settings;
        }

        private static void AddServiceElement(NewRelicConfiguration config, INewRelicConfig newrelicConfig) 
        {
            ServiceElement serviceElement = new ServiceElement();
            serviceElement.LicenseKey = newrelicConfig.LicenseKey;

            if (newrelicConfig.PollInterval.HasValue)
            {
                serviceElement.PollIntervalSeconds = newrelicConfig.PollInterval.Value;
            }

            config.Service = serviceElement;
        }

        private static void AddProxyElement(NewRelicConfiguration config, INewRelicConfig newrelicConfig)
        {
            string proxyHost = newrelicConfig.ProxyHost;
            if (proxyHost.IsValidString())
            {
               config.Proxy = new ProxyElement();

               config.Proxy.Host = proxyHost;

                string proxyPort = newrelicConfig.ProxyPort.ToString();
                if (proxyPort.IsValidString())
                {
                    config.Proxy.Port = proxyPort;
                }

                string proxyUsername = newrelicConfig.ProxyUserName;
                string proxyPassword = newrelicConfig.ProxyPassword;
                if (proxyUsername.IsValidString() && proxyPassword.IsValidString())
                {
                    config.Proxy.User = proxyUsername;
                    config.Proxy.Password = proxyPassword;
                }
            }
        }

        private static void AddSqlInstanceElements(NewRelicConfiguration config, List<object> agentConfigs)
        {
            SqlServerCollection sqlCollection = new SqlServerCollection();
            AzureCollection azureCollection = new AzureCollection();

            foreach (var agentDetails in agentConfigs)
            {
                var agentDictionary = (IDictionary<string, object>)agentDetails;

                if ("sqlserver".Equals(agentDictionary[Constants.TypeProperty]))
                {
                    string name = (string)agentDictionary[Constants.NameProperty];
                    string connectionString = (string)agentDictionary[Constants.ConnectionStringProperty];

                    SqlServerElement element = new SqlServerElement();
                    element.Name = name;
                    element.ConnectionString = connectionString;

                    if (agentDictionary.ContainsKey(Constants.IncludeSystemDbsProperty))
                    {
                        bool includeSystemDatabases = bool.Parse((string)agentDictionary[Constants.IncludeSystemDbsProperty]);
                        element.IncludeSystemDatabases = includeSystemDatabases;
                    }

                    if (agentDictionary.ContainsKey(Constants.IncludesProperty))
                    {
                        List<object> includedDbs = agentDictionary[Constants.IncludesProperty] as List<object>;

                        DatabaseCollection dbCollection = new DatabaseCollection();

                        foreach (var includedDb in includedDbs)
                        {
                            IDictionary<string, object> database = (IDictionary<string, object>)includedDb;
                            DatabaseElement dbElement = new DatabaseElement();

                            dbElement.Name = (string)database[Constants.NameProperty];
                            dbElement.DisplayName = (string)database[Constants.DisplayNameProperty];

                            dbCollection.Add(dbElement);
                        }

                        element.IncludedDatabases = dbCollection;
                    }

                    if (agentDictionary.ContainsKey(Constants.ExcludesProperty))
                    {
                        List<object> excludedDbs = agentDictionary[Constants.ExcludesProperty] as List<object>;

                        DatabaseCollection dbCollection = new DatabaseCollection();

                        foreach (var excludedDb in excludedDbs)
                        {
                            IDictionary<string, object> database = (IDictionary<string, object>)excludedDb;
                            DatabaseElement dbElement = new DatabaseElement();

                            dbElement.Name = (string)database[Constants.NameProperty];

                            dbCollection.Add(dbElement);
                        }

                        element.ExcludedDatabases = dbCollection;
                    }

                    sqlCollection.Add(element);
                }
                else if ("azure".Equals(agentDictionary[Constants.TypeProperty]))
                {
                    string name = (string)agentDictionary[Constants.NameProperty];
                    string connectionString = (string)agentDictionary[Constants.ConnectionStringProperty];

                    AzureSqlDatabaseElement element = new AzureSqlDatabaseElement();
                    element.Name = name;
                    element.ConnectionString = connectionString;

                    azureCollection.Add(element);
                }
                else
                {
                    throw new ConfigurationErrorsException("Error parsing type information from your 'plugin.json' file, ensure each JSON object in the 'agents' array has a 'type' property set to either 'sqlserver' or 'azure'");
                }
            }

            config.SqlServers = sqlCollection;
            config.AzureSqlDatabases = azureCollection;
        }
    }
}
