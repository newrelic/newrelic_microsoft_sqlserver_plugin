using System.Collections.Generic;
using System.Data;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public interface ISqlQuery : IMetricQuery
	{
		string ResourceName { get; }
		string CommandText { get; }

		/// <summary>
		///     Queries data from the database and returns the results.
		/// </summary>
		/// <param name="dbConnection">Open connection to the database.</param>
		/// <param name="endpoint">Settings for the endpoint that is queried.</param>
		/// <returns>
		///     An enumeration of a the type where the <see cref="SqlServerQueryAttribute" /> for this query object was found during initialization.
		/// </returns>
		IEnumerable<object> Query(IDbConnection dbConnection, ISqlEndpoint endpoint);
	}
}