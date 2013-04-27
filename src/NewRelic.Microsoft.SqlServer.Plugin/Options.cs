using CommandLine;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class Options
	{
		[Option('i', "install", HelpText = "Installs the " + ServiceConstants.DisplayName + " Windows service on this machine using the current directory as the install directory for the process.",
			MutuallyExclusiveSet = "mode")]
		public bool Install { get; set; }

		[Option('u', "uninstall", HelpText = "Attempts to Stop and Uninstall the " + ServiceConstants.DisplayName + " Windows service from this machine", MutuallyExclusiveSet = "mode")]
		public bool Uninstall { get; set; }

		[Option('s', "start", HelpText = "Attempts to Start the " + ServiceConstants.DisplayName + " Windows service on this machine", MutuallyExclusiveSet = "mode")]
		public bool Start { get; set; }

		[Option("stop", HelpText = "Attempts to Stop the " + ServiceConstants.DisplayName + " Windows service on this machine", MutuallyExclusiveSet = "mode")]
		public bool Stop { get; set; }

		[Option("installorstart",
			HelpText = "If the service was not previously installed it installs it, but does not start it. Alternatively if the service is already installed it simply starts the service",
			MutuallyExclusiveSet = "mode")]
		public bool InstallOrStart { get; set; }

		[Option('t', "sendtest", HelpText = "Launches process that sends simple sample metric data (random ints) to Dashboard", MutuallyExclusiveSet = "mode")]
		public bool SendTestMetrics { get; set; }

		[Option("config-file", HelpText = "Specify that settings are in a different file than the .exe.config")]
		public string ConfigFile { get; set; }
	}
}
