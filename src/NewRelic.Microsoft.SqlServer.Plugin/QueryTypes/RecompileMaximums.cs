using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public class RecompileMaximums
	{
		[Metric(MetricValueType = MetricValueType.Count, Units = "[objects]")]
		public int SingleUseObjects { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[objects]")]
		public int MultipleUseObjects { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[%_Single_Use]")]
		public decimal SingleUsePercent { get; set; }

		public override string ToString()
		{
			return string.Format("SingleUseObjects: {0},\t" +
			                     "MultipleUseObjects: {1},\t" +
			                     "SingleUsePercent: {2}",
			                     SingleUseObjects, MultipleUseObjects, SingleUsePercent);
		}
	}
}