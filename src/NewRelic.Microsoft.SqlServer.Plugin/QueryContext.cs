using System;
using System.Collections.Generic;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Binding.DotNET;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class QueryContext
	{
		private static readonly ILog _VerboseMetricsLogger = LogManager.GetLogger(Constants.VerboseMetricsLogger);

		public SqlMonitorQuery Query { get; set; }
		public IEnumerable<object> Results { get; set; }
		public ComponentData ComponentData { get; set; }
		public int MetricsRecorded { get; private set; }

		public string FormatMetricKey(object queryResult, string metricName)
		{
			var databaseName = new Lazy<string>(() =>
			                                    {
				                                    var databaseMetric = queryResult as IDatabaseMetric;
				                                    return databaseMetric != null ? databaseMetric.DatabaseName : null;
			                                    });

			return FormatMetricKey(Query.MetricPattern, databaseName, metricName);
		}

		internal static string FormatMetricKey(string pattern, Lazy<string> lazyDatabaseName, string metricName)
		{
			var result = pattern;

			if (result.Contains("{DatabaseName}"))
			{
				string databaseName;
				if (lazyDatabaseName != null && lazyDatabaseName.Value != null)
				{
					databaseName = lazyDatabaseName.Value;
				}
				else
				{
					databaseName = "(none)";
				}

				result = result.Replace("{DatabaseName}", databaseName);
			}

			if (result.Contains("{MetricName}"))
			{
				return result.Replace("{MetricName}", metricName);
			}

			return result.EndsWith("/") ? result + metricName : result + "/" + metricName;
		}

		public void AddMetric(string name, int value)
		{
			_VerboseMetricsLogger.InfoFormat("Component: {0}; Metric: {1}; Value: {2}", ComponentData.Name, name, value);
			ComponentData.AddMetric(name, value);
			MetricsRecorded++;
		}

		public void AddMetric(string name, decimal value)
		{
			_VerboseMetricsLogger.InfoFormat("Component: {0}; Metric: {1}; Value: {2}", ComponentData.Name, name, value);
			ComponentData.AddMetric(name, value);
			MetricsRecorded++;
		}

		public void AddMetric(string name, MinMaxMetricValue value)
		{
			_VerboseMetricsLogger.InfoFormat("Component: {0}; Metric: {1}; Value: {2}", ComponentData.Name, name, value);
			ComponentData.AddMetric(name, value);
			MetricsRecorded++;
		}
	}
}
