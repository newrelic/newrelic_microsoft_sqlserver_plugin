using System;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class SqlServerQueryAttribute : QueryAttribute
	{
		/// <summary>
		///     Marks a <em>QueryType</em> with the information required to gather and report a set of metrics. The metrics are gathered using a SQL query that is embedded in the assembly.
		/// </summary>
		/// <param name="resourceName">
		///    Path to the embedded query file. <see cref="QueryAttribute.ResourceName" />
		/// </param>
		/// <param name="metricPattern">
		///     Required. The pattern is formatted and sent to New Relic.<see cref="QueryAttribute.ResourceName" />
		/// </param>
		public SqlServerQueryAttribute(string resourceName, string metricPattern)
			: base(resourceName, metricPattern)
		{
			Enabled = true;
		}
	}
}
