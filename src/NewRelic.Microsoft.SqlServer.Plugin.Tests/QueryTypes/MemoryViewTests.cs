using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class MemoryViewTests
	{
		[Test]
		public void Assert_buffer_miss_ratio_stays_between_0_and_1()
		{
			var memoryView = new MemoryView {BufferCacheHitRatio = 1.2m};
			Assert.That(memoryView.BufferCacheMissRatio, Is.EqualTo(0m), "Hit ratio of 1.2");

			memoryView.BufferCacheHitRatio = .81m;
			Assert.That(memoryView.BufferCacheMissRatio, Is.EqualTo(.19m), "Hit ratio of .81");

			memoryView.BufferCacheHitRatio = -.5m;
			Assert.That(memoryView.BufferCacheMissRatio, Is.EqualTo(1m), "Hit ratio of -.5m");
		}
	}
}
