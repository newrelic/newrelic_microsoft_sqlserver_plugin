using System;
using System.Threading;
using log4net;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    public class SqlPoller
    {
        private readonly ILog _log;
        private readonly MetricCollector _metricCollector;
        private readonly Settings _settings;
        private readonly object _syncRoot;
        private PollingThread _pollingThread;

        public SqlPoller(Settings settings, ILog log = null)
        {
            _settings = settings;
            _syncRoot = new object();
            _log = log ?? LogManager.GetLogger(Constants.SqlMonitorLogger);
            _metricCollector = new MetricCollector(settings, _log);
        }

        public void Start()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_pollingThread != null)
                    {
                        return;
                    }

                    _log.Info("----------------");
                    _log.Info("Service Starting");

                    var queries = new QueryLocator(new DapperWrapper()).PrepareQueries();
                    _settings.Endpoints.ForEach(e => e.SetQueries(queries));

                    var pollingThreadSettings = new PollingThreadSettings
                                                {
                                                    Name = "SqlPoller",
                                                    PollIntervalSeconds = _settings.PollIntervalSeconds,
                                                    PollAction = () => _metricCollector.QueryEndpoints(queries),
                                                    AutoResetEvent = new AutoResetEvent(false),
                                                };

                    _pollingThread = new PollingThread(pollingThreadSettings, _log);
                    _pollingThread.ExceptionThrown += e => _log.Error("Polling thread exception", e);

                    _log.Debug("Service Threads Starting...");

                    _pollingThread.Start();

                    _log.Debug("Service Threads Started");
                }
            }
            catch (Exception e)
            {
                _log.Fatal("Failed while attempting to start service");
                _log.Warn(e);
                throw;
            }
        }

        public void Stop()
        {
            lock (_syncRoot)
            {
                if (_pollingThread == null)
                {
                    return;
                }

                try
                {
                    if (!_pollingThread.Running)
                    {
                        return;
                    }

                    _log.Debug("Service Threads Stopping...");
                    _pollingThread.Stop(true);
                    _log.Debug("Service Threads Stopped");
                }
                finally
                {
                    _pollingThread = null;
                    _log.Info("Service Stopped");
                }
            }
        }
    }
}
