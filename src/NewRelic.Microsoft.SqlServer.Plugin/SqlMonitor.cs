using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	///     Periodically polls SQL databases and reports the data back to a collector.
	/// </summary>
	internal class SqlMonitor
	{
		private readonly string _connectionString;
		private readonly ILog _log;
		private readonly object _syncRoot;
		private PollingThread _pollingThread;

		public SqlMonitor(string server, string database, ILog log = null)
		{
			_syncRoot = new object();
			_connectionString = string.Format("Server={0};Database={1};Trusted_Connection=True;", server, database);
			_log = log ?? LogManager.GetLogger("SqlMonitor");
		}

		public void Start(int? pollIntervalSeconds = 60)
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
						                            PollIntervalSeconds = pollIntervalSeconds ?? 60,
						                            PollAction = () => QueryDatabases(queries),
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
		private void QueryDatabases(IEnumerable<Func<IDbConnection, IEnumerable<object>>> queries)
		{
			try
			{
				Console.Out.WriteLine("Connecting with {0}", _connectionString);
				using (var conn = new SqlConnection(_connectionString))
				{
					foreach (var query in queries)
					{
						var results = query(conn);
						foreach (var result in results)
						{
							Console.Out.WriteLine(result);
						}
						Console.Out.WriteLine();
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
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
