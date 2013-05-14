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
	}
}
