using System;
using System.Linq;
using System.Reflection;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class MetricMapper
	{
		private static readonly Type[] _IntMappingTypes = new[] {typeof (long), typeof (byte), typeof (short),};
		private readonly MapAction _metricSetter;
		private readonly PropertyInfo _propertyInfo;

		public MetricMapper(PropertyInfo propertyInfo)
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
		}

		private MetricMapper(PropertyInfo propertyInfo, MapAction metricSetter, string metricName)
		{
			_propertyInfo = propertyInfo;
			_metricSetter = metricSetter;
			MetricName = metricName;
		}

		/// <summary>
		/// The part of the metric name for this metric value. For example, the metric SqlMonitor/SqlCpuUsage/*/SystemIdle, <see cref="MetricName"/> would be "SystemIdle".
		/// </summary>
		public string MetricName { get; set; }

		private static string GetMetricName(PropertyInfo propertyInfo, MetricAttribute attribute)
		{
			return attribute == null || string.IsNullOrEmpty(attribute.MetricName) ? propertyInfo.Name : attribute.MetricName;
		}

		private static MapAction GetMetricSetter(PropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType == typeof (int))
			{
				return AddIntMetric;
			}

			if (_IntMappingTypes.Contains(propertyInfo.PropertyType))
			{
				return ConvertToIntMetric;
			}

			if (propertyInfo.PropertyType == typeof (decimal))
			{
				return AddDecimalMetric;
			}

			return null;
		}

		public void AddMetric(QueryContext queryContext, object result)
		{
			_metricSetter(queryContext, this, result);
		}

		private static void AddIntMetric(QueryContext queryContext, MetricMapper mapper, object result)
		{
			queryContext.ComponentData.AddMetric(mapper.MetricName, (int) mapper._propertyInfo.GetValue(result, null));
		}

		private static void ConvertToIntMetric(QueryContext queryContext, MetricMapper mapper, object result)
		{
			var value = Convert.ToInt32(mapper._propertyInfo.GetValue(result, null));
			queryContext.ComponentData.AddMetric(mapper.MetricName, value);
		}

		private static void AddDecimalMetric(QueryContext queryContext, MetricMapper mapper, object result)
		{
			queryContext.ComponentData.AddMetric(mapper.MetricName, (decimal) mapper._propertyInfo.GetValue(result, null));
		}

		public static MetricMapper TryCreate(PropertyInfo propertyInfo)
		{
			var attribute = propertyInfo.GetCustomAttribute<MetricAttribute>();
			if (attribute != null && attribute.Ignore) return null;

			var setter = GetMetricSetter(propertyInfo);
			var metricName = GetMetricName(propertyInfo, attribute);
			return setter != null ? new MetricMapper(propertyInfo, setter, metricName) : null;
		}

		private delegate void MapAction(QueryContext queryContext, MetricMapper metricMapper, object result);
	}
}
