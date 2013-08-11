using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Binding.DotNET;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class QueryContext : IQueryContext
	{
		private static readonly ILog _VerboseMetricsLogger = LogManager.GetLogger(Constants.VerboseMetricsLogger);

		private readonly DateTime _creationTime;
		private readonly IMetricQuery _query;

		public QueryContext(IMetricQuery query)
		{
			_query = query;
			_creationTime = DateTime.Now;
		}

		public DateTime CreationTime
		{
			get { return _creationTime; }
		}

		public bool DataSent { get; set; }

		public string QueryName
		{
			get { return _query.QueryName; }
		}

		public Type QueryType
		{
			get { return _query.QueryType; }
		}

		public MetricTransformEnum MetricTransformEnum
		{
			get { return _query.MetricTransformEnum; }
		}

		public IEnumerable<object> Results { get; set; }
		public ComponentData ComponentData { get; set; }
		public int MetricsRecorded { get; private set; }

		public string FormatMetricKey(object queryResult, string metricName, string metricUnits)
		{
			return FormatMetricKey(_query.MetricPattern, queryResult, metricName, metricUnits);
		}

		public void AddAllMetrics()
		{
			_query.AddMetrics(this);
		}

		public void AddMetric(string name, int value)
		{
			_VerboseMetricsLogger.InfoFormat("Gathering Component: {0}; Metric: {1}; Value: {2}", ComponentData.Name, name, value);
			ComponentData.AddMetric(name, value);
			MetricsRecorded++;
		}

		public void AddMetric(string name, decimal value)
		{
			_VerboseMetricsLogger.InfoFormat("Gathering Component: {0}; Metric: {1}; Value: {2}", ComponentData.Name, name, value);
			ComponentData.AddMetric(name, value);
			MetricsRecorded++;
		}

		public void AddMetric(string name, MinMaxMetricValue value)
		{
			_VerboseMetricsLogger.InfoFormat("Gathering Component: {0}; Metric: {1}; Value: {2}", ComponentData.Name, name, value);
			ComponentData.AddMetric(name, value);
			MetricsRecorded++;
		}

		internal static string FormatMetricKey(string pattern, object queryResult, string metricName, string units = null)
		{
			var result = pattern;

			if (result.Contains("{DatabaseName}"))
			{
				var databaseMetric = queryResult as IDatabaseMetric;
				var databaseName = databaseMetric != null ? databaseMetric.DatabaseName : "(none)";
				result = result.Replace("{DatabaseName}", databaseName);
			}

			if (result.Contains("{MetricName}"))
			{
				result = result.Replace("{MetricName}", metricName);
			}
			else
			{
				result = result.EndsWith("/") ? result + metricName : result + "/" + metricName;
			}

			if (!string.IsNullOrEmpty(units))
			{
				result += units;
			}

			var matches = Regex.Matches(result, @"\{(?<property>[^}]+?)\}", RegexOptions.ExplicitCapture);
			if (matches.Count <= 0)
			{
				return result;
			}

			// Find placeholders
			var queryType = queryResult.GetType();
			foreach (Match match in matches)
			{
				// Get the property match
				var propertyName = match.Groups["property"].Value;
				// Get the property
				var propertyInfo = queryType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
				// Expect it to be a public instance property
				if (propertyInfo == null)
				{
					// Look for a similarly named property where maybe the case is mismatched. Performance is unimportant as this is a fatal error.
					if (queryType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) != null)
					{
						throw new Exception(string.Format("MetricPattern '{0}' contains a placeholder '{1}' for '{2}', however, it seems the placeholder has a case-mismatch." +
						                                  "The placeholder is case-sensitive.",
						                                  pattern,
						                                  propertyName,
						                                  queryType.Name));
					}
					throw new Exception(string.Format("MetricPattern '{0}' contains a placeholder '{1}' which was not found as a property on '{2}'. " +
					                                  "It must be a public, instance property with a getter.",
					                                  pattern,
					                                  propertyName,
					                                  queryType.Name));
				}
				// It must have a public getter
				if (!propertyInfo.CanRead || propertyInfo.GetGetMethod(false) == null)
				{
					throw new Exception(string.Format("MetricPattern '{0}' contains a placeholder for the property '{1}' on '{2}', however, it does not have a getter. " +
					                                  "It must be a public, instance property with a getter.",
					                                  pattern,
					                                  propertyName,
					                                  queryType.Name));
				}
				// Get the value
				var propertyValue = propertyInfo.GetValue(queryResult, null);
				// Try first as a string (most common), then ToString() when not null, else just the word "null"
				var replacement = propertyValue as string ?? (propertyValue != null ? propertyValue.ToString() : "null");
				// No leading or trailing whitespace
				replacement = replacement.Trim();
				// Replace all non-alphanumerics with underbar
				var safeReplacement = Regex.Replace(replacement, @"[^\w\d]", "_", RegexOptions.Singleline);
				// Finally, replace it in the metric pattern
				result = result.Replace("{" + propertyName + "}", safeReplacement);
			}

			return result;
		}
	}
}
