using System;

using CommandLine;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	/// Specifies set and description of Commandline options for exe
	/// </summary>
	public class Options
	{
		[Option('i', "install", HelpText = "Installs the " + ServiceConstants.DisplayName + " Windows service on this machine using the current directory as the install directory for the process.",
			MutuallyExclusiveSet = "mode")]
		public bool Install { get; set; }

		[Option('u', "uninstall", HelpText = "Attempts to Stop and Uninstall the " + ServiceConstants.DisplayName + " Windows service from this machine.", MutuallyExclusiveSet = "mode")]
		public bool Uninstall { get; set; }

		[Option('s', "start", HelpText = "Attempts to Start the " + ServiceConstants.DisplayName + " Windows service on this machine.", MutuallyExclusiveSet = "mode")]
		public bool Start { get; set; }

		[Option("stop", HelpText = "Attempts to Stop the " + ServiceConstants.DisplayName + " Windows service on this machine.", MutuallyExclusiveSet = "mode")]
		public bool Stop { get; set; }

		[Option("installorstart",
			HelpText = "If the service was not previously installed it installs it, but does not start it. Alternatively if the service is already installed it simply starts the service.",
			MutuallyExclusiveSet = "mode")]
		public bool InstallOrStart { get; set; }

		[Option("service-name", HelpText = "Optional. Overrides the default service name when installing or uninstalling.")]
		public string ServiceName { get; set; }

		[Option('t', "test", DefaultValue = false, HelpText = "Verifies the configuration. Collect metrics locally but does not send them to New Relic.")]
		public bool TestMode { get; set; }

		[Option("config-file", HelpText = "Specify that settings are in a different file than the .exe.config.")]
		public string ConfigFile { get; set; }

		public static Options ParseArguments(string[] args)
		{
			// '--test' arg was once named '--collect-only'. This provides backwards compatibility.
			if (args != null)
			{
				var index = Array.IndexOf(args, "--collect-only");
				if (index >= 0)
				{
					args[index] = "--test";
				}
			}

			var options = new Options();

			// If bad args were passed, will exit and print usage
			Parser.Default.ParseArgumentsStrict(args, options);

			return options;
		}
	}
}
