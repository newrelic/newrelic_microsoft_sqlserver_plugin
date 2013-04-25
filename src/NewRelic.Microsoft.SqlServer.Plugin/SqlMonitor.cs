using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	/// <summary>
	/// Periodically polls SQL databases and reports the data back to a collector.
	/// </summary>
	public class SqlMonitor
	{
		private readonly string _connectionString;

		public SqlMonitor(string server, string database)
		{
			_connectionString = string.Format("Server={0};Database={1};Trusted_Connection=True;", server, database);
		}

		public void Start()
		{
			// TODO Polling
			GetQueryStats();
		}

		/// <summary>
		/// Example query to test Dapper
		/// </summary>
		private void GetQueryStats()
		{
			IEnumerable<QueryStat> queryStats;
			Console.Out.WriteLine("Connecting with {0}", _connectionString);
			using (var conn = new SqlConnection(_connectionString))
			{
				queryStats = conn.Query<QueryStat>(QueryStat.Query, new {Id = 1});
			}

			foreach (var queryStat in queryStats)
			{
				Console.Out.WriteLine("{0}\t{1}\t{2}", queryStat.statement_start_offset, queryStat.creation_time, queryStat.last_execution_time);
			}
		}

		public void Stop()
		{
			// TODO Stop Polling
		}
	}
}
