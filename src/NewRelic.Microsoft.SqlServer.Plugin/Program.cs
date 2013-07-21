using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

using CommandLine;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

using log4net;
using log4net.Config;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			try
			{
				var options = new Options();

				var log = SetUpLogConfig();

				// If bad args were passed, will exit and print usage
				Parser.Default.ParseArgumentsStrict(args, options);

				var settings = ConfigurationParser.ParseSettings(log, options.ConfigFile);
				Settings.Default = settings;

				var installController = new InstallController(settings.ServiceName, settings.IsProcessElevated);
				if (options.Uninstall)
				{
					installController.Uninstall();
				}
				else if (options.Install)
				{
					installController.Install();
					if (options.Start)
						installController.StartService();
				}
				else if (options.Start)
				{
					installController.StartService();
				}
				else if (options.Stop)
				{
					installController.StopService();
				}
				else if (options.InstallOrStart)
				{
					installController.InstallOrStart();
				}
				else
				{
					Thread.CurrentThread.Name = "Main";
					settings.CollectOnly = options.CollectOnly;
					log.InfoFormat("New Relic® Sql Server Plugin");
					log.Info("Loaded Settings:");
					settings.ToLog(log);

					if (!settings.Endpoints.Any())
					{
						log.Error("No sql endpoints found please, update the configuration file to monitor one or more sql server instances.");
					}

					if (Environment.UserInteractive)
					{
						Console.Out.WriteLine("Starting Interactive mode");
						RunInteractive(settings);
					}
					else
					{
						ServiceBase[] services = {new SqlMonitorService(settings)};
						ServiceBase.Run(services);
					}
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);

				if (Environment.UserInteractive)
				{
					Console.Out.WriteLine();
					Console.Out.WriteLine("Press any key to exit...");
					Console.ReadKey();
				}

				return -1;
			}
		}

		public static ILog SetUpLogConfig()
		{
			const string log4NetConfig = "log4net.config";
			var assemblyPath = Assembly.GetExecutingAssembly().GetLocalPath();
			var configPath = Path.Combine(assemblyPath, log4NetConfig);
			XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath));
			return LogManager.GetLogger(Constants.SqlMonitorLogger);
		}

		/// <summary>
		///     Runs from the command shell, printing to the Console.
		/// </summary>
		/// <param name="settings"></param>
		private static void RunInteractive(Settings settings)
		{
			Console.Out.WriteLine("Starting Server");

			// Start our services
			var poller = new SqlPoller(settings);
			poller.Start();

			// Capture Ctrl+C
			Console.TreatControlCAsInput = true;

			char key;
			do
			{
				Console.Out.WriteLine("Press Q to quit...");
				var consoleKeyInfo = Console.ReadKey(true);
				Console.WriteLine();
				key = consoleKeyInfo.KeyChar;
			} while (key != 'q' && key != 'Q');

			Console.Out.WriteLine("Stopping...");

			// Stop our services
			poller.Stop();

#if DEBUG
			if (Debugger.IsAttached)
			{
				Console.Out.WriteLine("Press any key to stop debugging...");
				Console.ReadKey();
			}
#endif
		}
	}
}
