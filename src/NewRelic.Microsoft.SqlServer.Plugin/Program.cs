using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			IEnumerable<QueryStat> queryStats;
			using (var conn = new SqlConnection(@"Server=.;Database=YourDatabase;Trusted_Connection=True;"))
			{
				queryStats = conn.Query<QueryStat>(QueryStat.Query, new {Id = 1});
			}

			foreach (var queryStat in queryStats)
			{
				Console.Out.WriteLine("{0}\t{1}\t{2}", queryStat.statement_start_offset, queryStat.creation_time, queryStat.last_execution_time);
			}

			Console.ReadKey();
		}
	}
}
