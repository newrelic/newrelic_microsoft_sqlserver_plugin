using System.Configuration;
using System.IO;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	internal static class ConfigurationParser
	{
		/// <summary>
		/// Extracts the settings from a config file
		/// </summary>
		/// <param name="configFilePath">Optional. If null, then uses the loaded app.config. Otherwise attempts to find and load the <paramref name="configFilePath"/>.</param>
		/// <returns>Settings parsed from the config file.</returns>
		/// <exception cref="FileNotFoundException">Thrown when <paramref name="configFilePath"/> is not null and the file cannot be located.</exception>
		public static Settings ParseSettings(string configFilePath = null)
		{
			if (string.IsNullOrEmpty(configFilePath))
			{
				return GetSettingsFromAppConfig();
			}

			if (!File.Exists(configFilePath))
			{
				throw new FileNotFoundException("Unable to locate config file", configFilePath);
			}

			return LoadConfigurationFromFile(configFilePath);
		}

		/// <summary>
		/// Opens a config file on disk.
		/// </summary>
		/// <param name="configFilePath">Path to external config file.</param>
		/// <returns>Settings parsed from <paramref name="configFilePath"/>.</returns>
		private static Settings LoadConfigurationFromFile(string configFilePath)
		{
			var fileMap = new ConfigurationFileMap(configFilePath);
			var configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
			var section = (NewRelicConfigurationSection) configuration.GetSection("newRelic");
			return Settings.FromConfigurationSection(section);
		}

		private static Settings GetSettingsFromAppConfig()
		{
			var section = (NewRelicConfigurationSection) ConfigurationManager.GetSection("newRelic");
			return Settings.FromConfigurationSection(section);
		}
	}
}
