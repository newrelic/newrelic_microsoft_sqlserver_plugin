using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Platform.Sdk.Binding;
using NewRelic.Platform.Sdk.Processors;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
    /// <summary>
    ///     Polls SQL databases and reports the data back to a collector.
    /// </summary>
    internal class MetricCollector
    {
        private readonly IContext _context;
        private readonly IDictionary<string, IProcessor> _processors;
        private readonly ILog _log;
        private readonly Settings _settings;

        public MetricCollector(Settings settings, ILog log)
        {
            _settings = settings;
            _log = log;
            _context = new Context(_settings.LicenseKey) { Version = _settings.Version };
            _processors = new Dictionary<string, IProcessor>();
        }

        /// <summary>
        ///     Performs the queries against the databases.
        /// </summary>
        /// <param name="queries"></param>
        internal void QueryEndpoints(IEnumerable<SqlQuery> queries)
        {
            try
            {
                var tasks = _settings.Endpoints
                                     .Select(endpoint => Task.Factory
                                                             .StartNew(() => endpoint.ExecuteQueries(_log))
                                                             .Catch(e => _log.Error(e))
                                                             .ContinueWith(t => t.Result.ForEach(ctx => 
                                                                                                    {
                                                                                                        ctx.Context = _context;
                                                                                                        ctx.MetricProcessors = _processors;
                                                                                                        ctx.AddAllMetrics(); 
                                                                                                    }))
                                                             .Catch(e => _log.Error(e))
                                                             .ContinueWith(t =>
                                                                           {
                                                                               var queryContexts = t.Result.ToArray();
                                                                               return queryContexts.Sum(q => q.MetricsRecorded);
                                                                           }))
                                     .ToArray();

                // Wait for all of them to complete
                Task.WaitAll(tasks.ToArray<Task>());

                // This sends all components to the server in a single request, we may run into performance issues with one component delaying the others.
                SendComponentDataToCollector();

                _log.InfoFormat("Recorded {0} metrics", tasks.Sum(t => t.Result));
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        /// <summary>
        /// Sends data to New Relic, unless in "collect only" mode.
        /// </summary>
        internal void SendComponentDataToCollector()
        {
            // Allows a testing mode that does not send data to New Relic
            if (_settings.TestMode)
            {
                return;
            }

            try
            {
                _context.SendMetricsToService();
            }
            catch (Exception e)
            {
                _log.Error("Error sending data to connector", e);
            }
        }
    }
}
