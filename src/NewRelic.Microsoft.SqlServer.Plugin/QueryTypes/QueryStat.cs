using System;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	internal class QueryStat
	{
		public const string Query = @"SELECT 
      [statement_start_offset]
      ,[statement_end_offset]
      ,[plan_generation_num]
      ,[creation_time]
      ,[last_execution_time]
      ,[execution_count]
      ,[total_worker_time]
      ,[last_worker_time]
      ,[min_worker_time]
      ,[max_worker_time]
      ,[total_physical_reads]
      ,[last_physical_reads]
      ,[min_physical_reads]
      ,[max_physical_reads]
      ,[total_logical_writes]
      ,[last_logical_writes]
      ,[min_logical_writes]
      ,[max_logical_writes]
      ,[total_logical_reads]
      ,[last_logical_reads]
      ,[min_logical_reads]
      ,[max_logical_reads]
      ,[total_clr_time]
      ,[last_clr_time]
      ,[min_clr_time]
      ,[max_clr_time]
      ,[total_elapsed_time]
      ,[last_elapsed_time]
      ,[min_elapsed_time]
      ,[max_elapsed_time]
      ,[query_hash]
      ,[query_plan_hash]
      ,[total_rows]
      ,[last_rows]
      ,[min_rows]
      ,[max_rows]
  FROM [sys].[dm_exec_query_stats]";

		public int statement_start_offset { get; set; }
		public DateTime creation_time { get; set; }
		public DateTime last_execution_time { get; set; }
	}
}