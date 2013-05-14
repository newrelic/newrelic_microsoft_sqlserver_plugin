using System;
using System.Reflection;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	/// <summary>
	/// Allows override of default conventions for discovering and naming metrics on <em>QueryTypes</em>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class MetricAttribute : Attribute
	{
		/// <summary>
		/// Override the name of the metric recorded by the property the attribute is applied to. By default, the <see cref="PropertyInfo.Name"/> is used as the metric name.
		/// </summary>
		public string MetricName { get; set; }

		/// <summary>
		/// Default is false. When true, prevents the property from being reported as a metric.
		/// </summary>
		public bool Ignore { get; set; }
	}
}
