using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("MemoryView.sql", "Component/Memory", QueryName = "Memory View", Enabled = true)]
	internal class MemoryView
	{
		private decimal _bufferCacheHitRatio;

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_hit]")]
		public decimal BufferCacheHitRatio
		{
			get { return _bufferCacheHitRatio; }
			set
			{
				// Denominator and numerator in SQL are not populated at precisely at the same time. This results in values > 100%.
				// Normalize this between 0 and 100%
				_bufferCacheHitRatio = Math.Max(Math.Min(value, 100), 0);
			}
		}

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec]")]
		public long PageLifeExpectancyInSeconds { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[sec]")]
		public long PageLifeExpectancyInSecondsNuma { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_miss]")]
		public decimal BufferCacheMissRatio
		{
			get
			{
				// Normalize this between 0 and 100%					
				var missRatio = 100m - BufferCacheHitRatio;
				return Math.Max(Math.Min(100, missRatio), 0);
			}
		}
	}
}
