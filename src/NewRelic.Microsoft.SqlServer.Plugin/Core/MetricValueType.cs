namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	public enum MetricValueType
	{
		/// <summary>
		/// Instructs the metric mapper to use the property type to determine which value type the metric is.
		/// </summary>
		Default,

		/// <summary>
		/// Metric is a count. Coerces the metric value to <see cref="int"/>.
		/// </summary>
		Count,

		/// <summary>
		/// Metric is a value. Coerces the metric value to <see cref="decimal"/>.
		/// </summary>
		Value,
	}
}
