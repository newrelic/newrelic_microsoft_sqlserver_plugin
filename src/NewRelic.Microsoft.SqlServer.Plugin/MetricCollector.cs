using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using NewRelic.Microsoft.SqlServer.Plugin.Communication;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	/// Polls SQL databases and reports the data back to a collector.
	/// </summary>
	internal class MetricCollector
	{
		private static readonly ILog _VerboseSqlOutputLogger = LogManager.GetLogger(Constants.VerboseSqlLogger);
		private readonly AgentData _agentData;
		private readonly ILog _log;
		private readonly Settings _settings;

		public MetricCollector(Settings settings, ILog log)
		{
			_settings = settings;
			_log = log;
			_agentData = new AgentData {Host = Environment.MachineName, Pid = Process.GetCurrentProcess().Id, Version = _settings.Version,};
		}

		/// <summary>
		///     Performs the queries against the database
		/// </summary>
		/// <param name="queries"></param>
		internal void QueryServers(IEnumerable<SqlMonitorQuery> queries)
		{
			try
			{
				var tasks = _settings.SqlServers
				                     .Select(server => Task.Factory.StartNew(() => QueryServer(queries, server, _log))
				                                           .Catch(e => _log.Debug(e))
														   .ContinueWith(t => t.Result.ForEach(ctx => ctx.AddAllMetrics()))
				                                           .Catch(e => _log.Error(e))
				                                           .ContinueWith(t =>
				                                                         {
					                                                         var queryContexts = t.Result.ToArray();
					                                                         SendComponentDataToCollector(queryContexts);
					                                                         return queryContexts.Sum(q => q.MetricsRecorded);
				                                                         }))
				                     .ToArray();

				Task.WaitAll(tasks.ToArray<Task>());

				_log.InfoFormat("Recorded {0} metrics", tasks.Sum(t => t.Result));
			}
			catch (Exception e)
			{
				_log.Error(e);
			}
		}

		internal static IEnumerable<QueryContext> QueryServer(IEnumerable<SqlMonitorQuery> queries, SqlServerToMonitor server, ILog log)
		{
			// Remove password from logging
			var safeConnectionString = new SqlConnectionStringBuilder(server.ConnectionString);
			if (!string.IsNullOrEmpty(safeConnectionString.Password))
			{
				safeConnectionString.Password = "[redacted]";
			}

			_VerboseSqlOutputLogger.InfoFormat("Connecting with {0}", safeConnectionString);
			_VerboseSqlOutputLogger.Info("");

			using (var conn = new SqlConnection(server.ConnectionString))
			{
				foreach (var query in queries)
				{
					object[] results;
					try
					{
						_VerboseSqlOutputLogger.InfoFormat("Executing {0}", query.ResourceName);
						results = query.Query(conn, server).ToArray();
						foreach (var result in results)
						{
							// TODO Replace ToString() with something more useful that prints each property in the object
							_VerboseSqlOutputLogger.Info(result.ToString());
						}
						_VerboseSqlOutputLogger.Info("");
					}
					catch (Exception e)
					{
						log.Error(string.Format("Error with query '{0}'", query.QueryName), e);
						continue;
					}
					yield return new QueryContext(query) {Results = results, ComponentData = new ComponentData(server.Name, Constants.ComponentGuid, 1),};
				}
			}
		}

		/// <summary>
		/// Sends data to New Relic, unless in "collect only" mode.
		/// </summary>
		/// <param name="queryContexts">Query data containing <see cref="ComponentData"/> where metrics are recorded</param>
		internal void SendComponentDataToCollector(QueryContext[] queryContexts)
		{
			// Allows a testing mode that does not send data to New Relic
			if (_settings.CollectOnly)
			{
				return;
			}

			try
			{
				var platformData = new PlatformData(_agentData);
				queryContexts.ForEach(c => platformData.AddComponent(c.ComponentData));
				new SqlRequest(_settings.LicenseKey) {Data = platformData}.SendData();
			}
			catch (Exception e)
			{
				_log.Error("Error sending data to connector", e);
			}
		}
	}
}
