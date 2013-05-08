using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("MemoryView.sql", "Component/Memory", QueryName = "Memory View", Enabled = true)]
	internal class MemoryView
	{
		public decimal BufferCacheHitRatio { get; set; }
		public long PageLifeExpectancyInSeconds { get; set; }
		public long PageLifeExpectancyInSecondsNuma { get; set; }

		public decimal BufferCacheMissRatio
		{
			get
			{
				var missRatio = 1m - BufferCacheHitRatio;
				return Math.Max(Math.Min(1, missRatio), 0);
			}
		}
	}
}
