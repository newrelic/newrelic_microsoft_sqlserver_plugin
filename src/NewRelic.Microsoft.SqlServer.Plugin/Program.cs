using System;
using System.Diagnostics;
using System.ServiceProcess;
using CommandLine;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			try
			{
				var options = new Options();

				// If bad args were passed, will exit and print usage
				Parser.Default.ParseArgumentsStrict(args, options);

				if (options.Uninstall)
				{
					InstallController.Uninstall();
				}
				else if (options.Install)
				{
					InstallController.Install();
				}
				else if (options.Start)
				{
					InstallController.StartService();
				}
				else if (options.Stop)
				{
					InstallController.StopService();
				}
				else if (options.InstallOrStart)
				{
					InstallController.InstallOrStart();
				}
				else
				{
					var settings = ConfigurationParser.ParseSettings(options.ConfigFile);
					settings.CollectOnly = options.CollectOnly;

					if (Environment.UserInteractive)
					{
						RunInteractive(settings);
					}
					else
					{
						ServiceBase[] services = { new SqlMonitorService(settings) };
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

		/// <summary>
		///     Runs from the command shell, printing to the Console.
		/// </summary>
		/// <param name="settings"></param>
		private static void RunInteractive(Settings settings)
		{
			Console.Out.WriteLine("Starting Server");

			// Start our services
			var monitor = new SqlMonitor(settings);
			monitor.Start();

			// Capture Ctrl+C
			Console.TreatControlCAsInput = true;

			char key;
			do
			{
				Console.Out.WriteLine("Press Q to quit...");
				var consoleKeyInfo = Console.ReadKey();
				Console.WriteLine();
				key = consoleKeyInfo.KeyChar;
			} while (key != 'q' && key != 'Q');

			Console.Out.WriteLine("Stopping...");

			// Stop our services
			monitor.Stop();

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
