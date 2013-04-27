using System.ServiceProcess;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	/// <summary>
	/// Windows Service wrapper class.
	/// </summary>
	public class SqlMonitorService : ServiceBase
	{
		private readonly Settings _settings;
		private SqlMonitor _monitor;

		public SqlMonitorService(Settings settings)
		{
			_settings = settings;
			ServiceName = ServiceConstants.ServiceName;
		}

		protected override void OnStart(string[] args)
		{
			if (_monitor == null)
			{
				_monitor = new SqlMonitor(_settings);
			}

			_monitor.Start();
		}

		protected override void OnStop()
		{
			if (_monitor != null)
			{
				_monitor.Stop();
			}

			_monitor = null;
		}
	}
}
