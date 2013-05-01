using System;
using System.Collections.Generic;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class QueryContext
	{
		public SqlMonitorQuery Query { get; set; }
		public IEnumerable<object> Results { get; set; }
		public ComponentData ComponentData { get; set; }

		public string FormatMetricName(object queryResult, string metricName)
		{
			var databaseName = new Lazy<string>(() =>
			                                    {
				                                    var databaseMetric = queryResult as IDatabaseMetric;
				                                    return databaseMetric != null ? databaseMetric.DatabaseName : null;
			                                    });

			return FormatMetricName(Query.MetricPattern, databaseName, metricName);
		}

		internal static string FormatMetricName(string pattern, Lazy<string> lazyDatabaseName, string metricName)
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
	}
}
