using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("MemoryView.sql", "Component/Memory", QueryName = "Memory View", Enabled = true)]
	internal class MemoryView
	{
		private decimal _bufferCacheHitRatio;

		public decimal BufferCacheHitRatio
		{
			get { return _bufferCacheHitRatio; }
			set
			{
				// Denominator and numerator in SQL are not populated at precisely at the same time. This results in values > 100%.
				// Normalize this between 0 and 1
				_bufferCacheHitRatio = Math.Max(Math.Min(value, 1), 0);
			}
		}

		public long PageLifeExpectancyInSeconds { get; set; }
		public long PageLifeExpectancyInSecondsNuma { get; set; }

		public decimal BufferCacheMissRatio
		{
			get
			{
				// Normalize this between 0 and 1					
				var missRatio = 1m - BufferCacheHitRatio;
				return Math.Max(Math.Min(1, missRatio), 0);
			}
		}
	}
}
