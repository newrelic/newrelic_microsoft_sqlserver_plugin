using System;
using System.Linq;
using System.Reflection;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class MetricMapper
	{
		private static readonly Type[] _NumericMappingTypes = new[] {typeof (long), typeof (byte), typeof (short), typeof (decimal), typeof(int)};
		private static readonly Type[] _NumericMetricTypes = new[] {typeof (decimal), typeof (int)};
		private readonly MapAction _metricSetter;
		private readonly PropertyInfo _propertyInfo;

		internal MetricMapper(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;

			var attribute = propertyInfo.GetCustomAttribute<MetricAttribute>();
			if (attribute != null && attribute.Ignore)
			{
				throw new ArgumentException(string.Format("The property {0}.{1} is marked to ignore and cannot be mapped.", propertyInfo.ReflectedType.FullName, propertyInfo.Name));
			}

			_metricSetter = GetMetricSetter(propertyInfo);

			if (_metricSetter == null)
			{
				throw new ArgumentException(string.Format("No convention-based mapping for {0} property {1}.{2}", propertyInfo.PropertyType.Name, propertyInfo.ReflectedType.FullName, propertyInfo.Name));
			}

			MetricName = GetMetricName(_propertyInfo, attribute);
			MetricUnits = attribute != null ? attribute.Units : null;
		}

		private MetricMapper(PropertyInfo propertyInfo, MapAction metricSetter, string metricName, string metricUnits)
		{
			_propertyInfo = propertyInfo;
			_metricSetter = metricSetter;
			MetricName = metricName;
			MetricUnits = metricUnits;
		}

		/// <summary>
		/// The part of the metric name for this metric value.
		/// </summary>
		/// <example>For example, the metric <code>Custom/SqlCpuUsage/*/SystemIdle</code>, <see cref="MetricName" /> would be <code>SystemIdle</code>.</example>
		public string MetricName { get; set; }

		/// <summary>
		/// Metrics have a notion of units that describe what the count field and value field represent. Should be surrounded by braces.
		/// </summary>
		/// <example>[bytes], [sec|op], [bytes/sec]</example>
		/// <remarks>More info at https://newrelic.com/docs/platform/metric-api-and-naming-reference</remarks>
		public string MetricUnits { get; set; }

		private static string GetMetricName(PropertyInfo propertyInfo, MetricAttribute attribute)
		{
			return attribute == null || string.IsNullOrEmpty(attribute.MetricName) ? propertyInfo.Name : attribute.MetricName;
		}

		private static MapAction GetMetricSetter(PropertyInfo propertyInfo)
		{
			var attribute = propertyInfo.GetCustomAttribute<MetricAttribute>();
			var metricValueType = attribute != null ? attribute.MetricValueType : MetricValueType.Default;
			switch (metricValueType)
			{
				case MetricValueType.Count:
					if (propertyInfo.PropertyType == typeof (int))
					{
						return AddCountMetric;
					}
					if (_NumericMappingTypes.Contains(propertyInfo.PropertyType))
					{
						return ConvertToCountMetric;
					}
					return null;

				case MetricValueType.Value:
					if (propertyInfo.PropertyType == typeof (decimal))
					{
						return AddValueMetric;
					}
					if (_NumericMappingTypes.Contains(propertyInfo.PropertyType))
					{
						return ConvertToValueMetric;
					}
					
					return null;

				default:
					return GetMetricSetterByConvention(propertyInfo);
			}
		}

		private static MapAction GetMetricSetterByConvention(PropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType == typeof (int))
			{
				return AddCountMetric;
			}

			if (propertyInfo.PropertyType == typeof (decimal))
			{
				return AddValueMetric;
			}

			if (_NumericMappingTypes.Contains(propertyInfo.PropertyType))
			{
				return ConvertToCountMetric;
			}

			return null;
		}

		public void AddMetric(QueryContext queryContext, object result)
		{
			var metricName = queryContext.FormatMetricKey(result, MetricName, MetricUnits);
			_metricSetter(queryContext, metricName, _propertyInfo, result);
		}

		private static void AddCountMetric(QueryContext queryContext, string metricName, PropertyInfo propertyInfo, object result)
		{
			queryContext.AddMetric(metricName, (int) propertyInfo.GetValue(result, null));
		}

		private static void ConvertToCountMetric(QueryContext queryContext, string metricName, PropertyInfo propertyInfo, object result)
		{
			// If the query result is a bigint, an exception is thrown when attempting to make it an int.
			var potentiallyLargeValue = Convert.ToInt64(propertyInfo.GetValue(result, null));
			if (potentiallyLargeValue >= int.MaxValue)
			{
				// Best we can do when the value is > int.MaxValue
				queryContext.AddMetric(metricName, int.MaxValue);
			}
			else
			{
				var value = Convert.ToInt32(potentiallyLargeValue);
				queryContext.AddMetric(metricName, value);
			}
		}

		private static void AddValueMetric(QueryContext queryContext, string metricName, PropertyInfo propertyInfo, object result)
		{
			queryContext.AddMetric(metricName, (decimal) propertyInfo.GetValue(result, null));
		}

		private static void ConvertToValueMetric(QueryContext queryContext, string metricName, PropertyInfo propertyInfo, object result)
		{
			// If the query result is a bigint, an exception is thrown when attempting to make it an decimal.
			var potentiallyLargeValue = Convert.ToInt64(propertyInfo.GetValue(result, null));
			if (potentiallyLargeValue >= decimal.MaxValue)
			{
				// Best we can do when the value is > decimal.MaxValue
				queryContext.AddMetric(metricName, decimal.MaxValue);
			}
			else
			{
				var value = Convert.ToDecimal(potentiallyLargeValue);
				queryContext.AddMetric(metricName, value);
			}
		}

		public static bool IsMetricNumeric(object metricValue)
		{
			return _NumericMetricTypes.Contains(metricValue.GetType());
		}

		public static MetricMapper TryCreate(PropertyInfo propertyInfo)
		{
			var attribute = propertyInfo.GetCustomAttribute<MetricAttribute>();
			if (attribute != null && attribute.Ignore)
			{
				return null;
			}

			var setter = GetMetricSetter(propertyInfo);
			var metricName = GetMetricName(propertyInfo, attribute);
			var metricUnits = attribute != null ? attribute.Units : null;
			return setter != null ? new MetricMapper(propertyInfo, setter, metricName, metricUnits) : null;
		}

		private delegate void MapAction(QueryContext queryContext, string metricName, PropertyInfo propertyInfo, object result);
	}
}
