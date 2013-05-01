using System;
using System.Linq;
using System.Reflection;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public class MetricMapper
	{
		private static readonly Type[] _IntMappingTypes = new[] {typeof (long), typeof (byte), typeof (short),};
		private readonly Lazy<Action<QueryContext, object>> _metricSetter;
		private readonly PropertyInfo _propertyInfo;

		public MetricMapper(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
			_metricSetter = new Lazy<Action<QueryContext, object>>(() => GetMetricSetter(propertyInfo));
		}

		public string MetricName { get; set; }

		private Action<QueryContext, object> GetMetricSetter(PropertyInfo propertyInfo)
		{
			if (propertyInfo.PropertyType == typeof(int))
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

			throw new ArgumentException(string.Format("No convention-based mapping for {0}.{1}", propertyInfo.ReflectedType.FullName, propertyInfo.Name));
		}

		public void AddMetric(QueryContext queryContext, object result)
		{
			_metricSetter.Value(queryContext, result);
		}

		private void AddIntMetric(QueryContext queryContext, object result)
		{
			queryContext.ComponentData.AddMetric(MetricName, (int) _propertyInfo.GetValue(result, null));
		}

		private void ConvertToIntMetric(QueryContext queryContext, object result)
		{
			var value = Convert.ToInt32(_propertyInfo.GetValue(result, null));
			queryContext.ComponentData.AddMetric(MetricName, value);
		}

		private void AddDecimalMetric(QueryContext queryContext, object result)
		{
			queryContext.ComponentData.AddMetric(MetricName, (decimal) _propertyInfo.GetValue(result, null));
		}
	}
}
