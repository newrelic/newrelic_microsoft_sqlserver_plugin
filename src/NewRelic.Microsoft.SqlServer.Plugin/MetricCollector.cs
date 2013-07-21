using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using NewRelic.Microsoft.SqlServer.Plugin.Communication;
using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	///     Polls SQL databases and reports the data back to a collector.
	/// </summary>
	internal class MetricCollector
	{
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
				                                             .ContinueWith(t => t.Result.ForEach(ctx => ctx.AddAllMetrics()))
				                                             .Catch(e => _log.Error(e))
				                                             .ContinueWith(t =>
				                                                           {
					                                                           var queryContexts = t.Result.ToArray();
					                                                           endpoint.UpdateHistory(queryContexts);
					                                                           SendComponentDataToCollector(endpoint);
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

		/// <summary>
		/// Sends data to New Relic, unless in "collect only" mode.
		/// </summary>
		/// <param name="endpoint">SQL endpoint from which the metrics were harvested.</param>
		internal void SendComponentDataToCollector(ISqlEndpoint endpoint)
		{
			var platformData = endpoint.GeneratePlatformData(_agentData);

			// Allows a testing mode that does not send data to New Relic
			if (_settings.CollectOnly)
			{
				return;
			}

			try
			{
				_log.DebugFormat("Reporting metrics for {0} with duration {1}s", endpoint.Name, endpoint.Duration);

				// Record the report time as now. If SendData takes very long, the duration comes up short and the chart shows a drop out
				var reportTime = DateTime.Now;
				// Send the data to New Relic
				new SqlRequest(_settings.LicenseKey) {Data = platformData}.SendData();
				// If send is error free, reset the last report date to calculate accurate duration
				endpoint.MetricReportSuccessful(reportTime);
			}
			catch (Exception e)
			{
				_log.Error("Error sending data to connector", e);
			}
		}
	}
}
