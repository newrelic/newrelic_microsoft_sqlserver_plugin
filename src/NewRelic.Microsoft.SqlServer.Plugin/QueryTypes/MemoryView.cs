using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("MemoryView.sql", "Component/Memory", QueryName = "Memory View", Enabled = true)]
	public class MemoryView
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
		public long PageLife { get; set; }

		/// <summary>
		/// <see cref="PageLife"/> is an important metric, however, it is an even increasing metric.
		/// Such metrics, where higher is better, are not supported as Summary metrics in the New Relic dashboard.
		/// The Page Life Threat is a metric that takes the inverse of the page life and scores it against a default minimum page life.
		/// As the page life reaches this mininum, the value approaches 100% and this an indicator that something is bad.
		/// </summary>
		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_threat]")]
		public decimal PageLifeThreat
		{
			get
			{
				// Defend against bad page life
				if (PageLife <= 0) return 100m;
				// Minimum is 5 mins
				const decimal minimumPageLifeInSeconds = 300;
				// Get a "threat" value that maxes out at 1
				var threat = Math.Min(1m, minimumPageLifeInSeconds/PageLife);
				// Minimize decimal length
				// Square it to minimize the threat at values far away from 300
				var result = threat*threat
				       // Multiply for percentage
				       *100;
				// Reduce number of digits
				return decimal.Round(result, 2);
			}
		}

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

		public override string ToString()
		{
			return string.Format("BufferCacheHitRatio: {0},\t" +
			                     "PageLife: {1},\t" +
			                     "BufferCacheMissRatio: {2}",
			                     BufferCacheHitRatio, PageLife, BufferCacheMissRatio);
		}
	}
}
