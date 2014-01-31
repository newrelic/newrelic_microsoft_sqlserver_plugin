using System.ServiceProcess;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    /// <summary>
    /// Windows Service wrapper class.
    /// </summary>
    public class SqlMonitorService : ServiceBase
    {
        private readonly Settings _settings;
        private SqlPoller _sqlPoller;

        public SqlMonitorService(Settings settings)
        {
            _settings = settings;
            ServiceName = settings.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            if (_sqlPoller == null)
            {
                _sqlPoller = new SqlPoller(_settings);
            }

            _sqlPoller.Start();
        }

        protected override void OnStop()
        {
            if (_sqlPoller != null)
            {
                _sqlPoller.Stop();
            }

            _sqlPoller = null;
        }
    }
}
