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
        [Option('t', "test", DefaultValue = false, HelpText = "Verifies the configuration. Collect metrics locally but does not send them to New Relic.")]
        public bool TestMode { get; set; }

        public static Options ParseArguments(string[] args)
        {
            var options = new Options();

            // If bad args were passed, will exit and print usage
            Parser.Default.ParseArgumentsStrict(args, options);

            return options;
        }
    }
}
