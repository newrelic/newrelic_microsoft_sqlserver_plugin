using System.Configuration;
using System.IO;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    internal static class ConfigurationParser
    {
        /// <summary>
        ///     Extracts the settings from a config file
        /// </summary>
        /// <param name="log">Log4Net ILog implementation</param>
        /// <param name="configFilePath">
        ///     Optional. If null, then uses the loaded app.config. Otherwise attempts to find and load the
        ///     <paramref
        ///         name="configFilePath" />
        ///     .
        /// </param>
        /// <returns>Settings parsed from the config file.</returns>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when <paramref name="configFilePath" /> is not null and the file cannot be located.
        /// </exception>
        public static Settings ParseSettings(ILog log, string configFilePath = null)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                return GetSettingsFromAppConfig(log);
            }

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Unable to locate config file", configFilePath);
            }

            return LoadConfigurationFromFile(configFilePath, log);
        }

        /// <summary>
        ///     Opens a config file on disk.
        /// </summary>
        /// <param name="configFilePath">Path to external config file.</param>
        /// <param name="log">Log4Net ILog implementation</param>
        /// <returns>
        ///     Settings parsed from <paramref name="configFilePath" />.
        /// </returns>
        private static Settings LoadConfigurationFromFile(string configFilePath, ILog log)
        {
            log.DebugFormat("Attempting to load settings from external configuration file '{0}'", configFilePath);
            var fileMap = new ConfigurationFileMap(configFilePath);
            System.Configuration.Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
            var section = (NewRelicConfigurationSection) configuration.GetSection("newRelic");
            Settings settingsFromConfig = Settings.FromConfigurationSection(section);
            log.DebugFormat("Settings loaded successfully");

            return settingsFromConfig;
        }

        private static Settings GetSettingsFromAppConfig(ILog log)
        {
            log.Debug("No external configuration path given, attempting to load settings from from default configuration file");
            var section = (NewRelicConfigurationSection) ConfigurationManager.GetSection("newRelic");
            Settings settingsFromAppConfig = Settings.FromConfigurationSection(section);
            log.DebugFormat("Settings loaded successfully");
            return settingsFromAppConfig;
        }
    }
}