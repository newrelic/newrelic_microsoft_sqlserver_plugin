using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	///  Polls SQL databases and reports the data back to a collector.
	/// </summary>
	internal class SqlMonitor
	{
		private readonly ILog _log;
		private readonly Settings _settings;
		private readonly object _syncRoot;
		private PollingThread _pollingThread;

		public SqlMonitor(Settings settings, ILog log = null)
		{
			_settings = settings;
			_syncRoot = new object();
			_log = log ?? LogManager.GetLogger("SqlMonitor");
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

					var pollingThreadSettings = new PollingThreadSettings
					                            {
						                            Name = "SQL Monitor Query Polling Thread",
						                            InitialPollDelaySeconds = 0,
						                            PollIntervalSeconds = _settings.PollIntervalSeconds,
						                            PollAction = () => QueryServers(queries),
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


		/// <summary>
		/// Performs the queries against the database
		/// </summary>
		/// <param name="queries"></param>
		private void QueryServers(IEnumerable<SqlMonitorQuery> queries)
		{
			try
			{
				var tasks = _settings.SqlServers
				                     .Select(server => Task.Factory.StartNew(() => QueryServer(queries, server)).Catch(e => Console.Out.WriteLine(e)))
				                     .ToArray();
				Task.WaitAll(tasks);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static void QueryServer(IEnumerable<SqlMonitorQuery> queries, SqlServerToMonitor server)
		{
			Console.Out.WriteLine("Connecting with {0}", server.ConnectionString);
			Console.Out.WriteLine();
			using (var conn = new SqlConnection(server.ConnectionString))
			{
				foreach (var query in queries)
				{
					Console.Out.WriteLine("Executing {0}", query.ResourceName);
					var results = query.Invoke(conn);
					foreach (var result in results)
					{
						Console.Out.WriteLine(result);
					}
					Console.Out.WriteLine();
				}
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
