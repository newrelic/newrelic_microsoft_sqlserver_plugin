using System;
using System.Linq;
using System.Threading;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Sdk.Utils;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    internal class Program
    {
        private static readonly Logger _log = Logger.GetLogger(typeof(Program).Name);

        private static int Main(string[] args)
        {
            try
            {
                var options = Options.ParseArguments(args);

                var settings = ConfigurationParser.ParseSettings();
                settings.TestMode = options.TestMode;
                Settings.Default = settings;

                Thread.CurrentThread.Name = "Main";
                _log.Info("New Relic Sql Server Plugin");
                _log.Info("Loaded Settings:");
                settings.ToLog();

                if (!settings.Endpoints.Any())
                {
                    _log.Error("No sql endpoints found please, update the configuration file to monitor one or more sql server instances.");
                }
                else
                {
                    var poller = new SqlPoller(settings);
                    poller.Start();

                    // Suspend the main thread indefinitely as the SqlPoller will spawn its own thread
                    Thread.Sleep(Timeout.Infinite);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine();

                return -1;
            }
        }
    }
}
