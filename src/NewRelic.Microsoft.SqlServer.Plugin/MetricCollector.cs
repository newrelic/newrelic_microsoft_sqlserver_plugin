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
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	///     Polls SQL databases and reports the data back to a collector.
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
		internal void QueryServers(IEnumerable<ISqlMonitorQuery> queries)
		{
			try
			{
				// Calculate "duration" as the span between "now" and the last recorded report time. This avoids "drop outs" in the charts.
				var tasks = _settings.SqlServers
				                     .Select(server => Task.Factory
				                                           .StartNew(() => QueryServer(queries, server, _log))
				                                           .Catch(e => _log.Error(e))
				                                           .ContinueWith(t => t.Result.ForEach(ctx => ctx.AddAllMetrics()))
				                                           .Catch(e => _log.Error(e))
				                                           .ContinueWith(t =>
				                                                         {
					                                                         var queryContexts = t.Result.ToArray();
					                                                         server.UpdateHistory(queryContexts);
					                                                         SendComponentDataToCollector(server);
					                                                         return queryContexts.Sum(q => q.MetricsRecorded);
				                                                         }))
				                     .ToArray();

				// Wait for all of them to complete
				Task.WaitAll(tasks.ToArray<Task>());

				_log.InfoFormat("Recorded {0} metrics", tasks.Sum(t => t.Result));
			}
			catch (Exception e)
			{
				_log.Error(e);
			}
		}

		internal static IEnumerable<IQueryContext> QueryServer(IEnumerable<ISqlMonitorQuery> queries, ISqlServerToMonitor server, ILog log)
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
						ApplyDatabaseDisplayNames(server.IncludedDatabases, results);

						if (_VerboseSqlOutputLogger.IsInfoEnabled)
						{
							foreach (var result in results)
							{
								// TODO Replace ToString() with something more useful that prints each property in the object
								_VerboseSqlOutputLogger.Info(result.ToString());
							}
							_VerboseSqlOutputLogger.Info("");
						}
					}
					catch (Exception e)
					{
						log.Error(string.Format("Error with query '{0}'", query.QueryName), e);
						continue;
					}
					yield return new QueryContext(query) {Results = results, ComponentData = new ComponentData(server.Name, Constants.SqlServerComponentGuid, server.Duration),};
				}
			}
		}

		/// <summary>
		/// Replaces the database name on <see cref="IDatabaseMetric"/> results with the <see cref="Database.DisplayName"/> when possible.
		/// </summary>
		/// <param name="includedDatabases"></param>
		/// <param name="results"></param>
		internal static void ApplyDatabaseDisplayNames(IEnumerable<Database> includedDatabases, object[] results)
		{
			if (includedDatabases == null) return;

			var renameMap = includedDatabases.Where(d => !string.IsNullOrEmpty(d.DisplayName)).ToDictionary(d => d.Name.ToLower(), d => d.DisplayName);
			if (!renameMap.Any()) return;

			var databaseMetrics = results.OfType<IDatabaseMetric>().Where(d => !string.IsNullOrEmpty(d.DatabaseName)).ToArray();
			if (!databaseMetrics.Any()) return;

			foreach (var databaseMetric in databaseMetrics)
			{
				string displayName;
				if (renameMap.TryGetValue(databaseMetric.DatabaseName.ToLower(), out displayName))
				{
					databaseMetric.DatabaseName = displayName;
				}
			}
		}

		/// <summary>
		/// Sends data to New Relic, unless in "collect only" mode.
		/// </summary>
		/// <param name="server">SQL Server from which the metrics were harvested.</param>
		internal void SendComponentDataToCollector(ISqlServerToMonitor server)
		{
			var platformData = server.GeneratePlatformData(_agentData);

			// Allows a testing mode that does not send data to New Relic
			if (_settings.CollectOnly)
			{
				return;
			}

			try
			{
				_log.DebugFormat("Reporting metrics for {0} with duration {1}s", server.Name, server.Duration);

				// Record the report time as now. If SendData takes very long, the duration comes up short and the chart shows a drop out
				var reportTime = DateTime.Now;
				// Send the data to New Relic
				new SqlRequest(_settings.LicenseKey) {Data = platformData}.SendData();
				// If send is error free, reset the last report date to calculate accurate duration
				server.MetricReportSuccessful(reportTime);
			}
			catch (Exception e)
			{
				_log.Error("Error sending data to connector", e);
			}
		}
	}
}
