using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class MemoryViewTests
	{
		[Test]
		public void Assert_buffer_cache_miss_ratio_stays_between_0_and_100()
		{
			var memoryView = new MemoryView {BufferCacheHitRatio = 120m};
			Assert.That(memoryView.BufferCacheMissRatio, Is.EqualTo(0m), "Hit ratio of 1.2m");

			memoryView.BufferCacheHitRatio = 81m;
			Assert.That(memoryView.BufferCacheMissRatio, Is.EqualTo(19m), "Hit ratio of 81m");

			memoryView.BufferCacheHitRatio = -5m;
			Assert.That(memoryView.BufferCacheMissRatio, Is.EqualTo(100m), "Hit ratio of -5m");
		}

		[Test]
		public void Assert_buffer_cache_hit_ratio_stays_between_0_and_100()
		{
			var memoryView = new MemoryView {BufferCacheHitRatio = 120m};
			Assert.That(memoryView.BufferCacheHitRatio, Is.EqualTo(100m), "Hit ratio of 120m");

			memoryView.BufferCacheHitRatio = 81m;
			Assert.That(memoryView.BufferCacheHitRatio, Is.EqualTo(81m), "Hit ratio of 81m");

			memoryView.BufferCacheHitRatio = -5m;
			Assert.That(memoryView.BufferCacheHitRatio, Is.EqualTo(0m), "Hit ratio of -5m");
		}

		[Test]
		public void Assert_page_threat_stays_between_0_and_100()
		{
			var memoryView = new MemoryView {PageLife = 0};
			Assert.That(memoryView.PageLifeThreat, Is.EqualTo(100m), "0 page life maxes at 100%");

			// Try the default
			memoryView.PageLife = 300; 
			Assert.That(memoryView.PageLifeThreat, Is.EqualTo(100m), "300 page life maxes at 100%");

			// Just above the default
			memoryView.PageLife = 301; 
			Assert.That(memoryView.PageLifeThreat, Is.LessThan(100m), "301 page life is slightly less than 100%");

			// Just above the default
			memoryView.PageLife = long.MaxValue; 
			Assert.That(memoryView.PageLifeThreat, Is.EqualTo(0m), "Max page life should effectively report 0%");
		}
	}
}
