using System;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	public interface ISqlServerToMonitor
	{
		string Name { get; }
		string ConnectionString { get; }

		/// <summary>
		///     The number of seconds since the last recorded successful report of metrics.
		/// </summary>
		int Duration { get; }

		string[] IncludedDatabases { get; }
		string[] ExcludedDatabases { get; }

		/// <summary>
		/// Inform the server context that a report was sent on its behalf. Used to determine the <see cref="Duration"/>
		/// </summary>
		/// <param name="reportDate"></param>
		void MetricReportSuccessful(DateTime? reportDate = null);
	}
}
