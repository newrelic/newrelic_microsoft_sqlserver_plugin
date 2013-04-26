using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	///     Periodically polls SQL databases and reports the data back to a collector.
	/// </summary>
	internal class SqlMonitor
	{
		private readonly string _connectionString;

		public SqlMonitor(string server, string database)
		{
			_connectionString = string.Format("Server={0};Database={1};Trusted_Connection=True;", server, database);
		}

		public void Start()
		{
			var queries = new QueryLocator(new DapperWrapper()).PrepareQueries();

			// TODO Polling
			QueryDatabases(queries);
		}


		/// <summary>
		///     Example query to test Dapper
		/// </summary>
		/// <param name="queries"></param>
		private void QueryDatabases(IEnumerable<Func<IDbConnection, IEnumerable<object>>> queries)
		{
			Console.Out.WriteLine("Connecting with {0}", _connectionString);
			using (var conn = new SqlConnection(_connectionString))
			{
				foreach (var query in queries)
				{
					var results = query(conn);
					foreach (var result in results)
					{
						Console.Out.WriteLine(result);
					}
					Console.Out.WriteLine();
				}
			}
		}

		public void Stop()
		{
			// TODO Stop Polling
		}
	}
}
