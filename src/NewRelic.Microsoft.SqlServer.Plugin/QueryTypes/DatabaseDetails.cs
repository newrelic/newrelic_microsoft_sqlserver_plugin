using System;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("DatabaseDetails.SqlServer.sql", "Component/DatabaseDetails/{MetricName}/{DatabaseName}", QueryName = "Database Details", Enabled = false)]
	public class DatabaseDetails : DatabaseMetricBase
	{
		// ReSharper disable InconsistentNaming
		public int database_id { get; set; }
		public int? source_database_id { get; set; }
		public DateTime create_date { get; set; }
		public int compatibility_level { get; set; }
		public string collation_name { get; set; }
		public byte? user_access { get; set; }
		public string user_access_desc { get; set; }
		public bool? is_read_only { get; set; }
		public bool is_auto_close_on { get; set; }
		public bool? is_auto_shrink_on { get; set; }
		public byte? state { get; set; }
		public string state_desc { get; set; }
		public bool? is_in_standby { get; set; }
		public bool? is_cleanly_shutdown { get; set; }
		public bool? is_supplemental_logging_enabled { get; set; }
		public byte? snapshot_isolation_state { get; set; }
		public string snapshot_isolation_state_desc { get; set; }
		public bool? is_read_committed_snapshot_on { get; set; }
		public byte? recovery_model { get; set; }
		public string recovery_model_desc { get; set; }
		public byte? page_verify_option { get; set; }
		public string page_verify_option_desc { get; set; }
		public bool? is_auto_create_stats_on { get; set; }
		public bool? is_auto_update_stats_on { get; set; }
		public bool? is_auto_update_stats_async_on { get; set; }
		public bool? is_ansi_null_default_on { get; set; }
		public bool? is_ansi_nulls_on { get; set; }
		public bool? is_ansi_padding_on { get; set; }
		public bool? is_ansi_warnings_on { get; set; }
		public bool? is_arithabort_on { get; set; }
		public bool? is_concat_null_yields_null_on { get; set; }
		public bool? is_numeric_roundabort_on { get; set; }
		public bool? is_quoted_identifier_on { get; set; }
		public bool? is_recursive_triggers_on { get; set; }
		public bool? is_cursor_close_on_commit_on { get; set; }
		public bool? is_local_cursor_default { get; set; }
		public bool? is_fulltext_enabled { get; set; }
		public bool? is_trustworthy_on { get; set; }
		public bool? is_db_chaining_on { get; set; }
		public bool? is_parameterization_forced { get; set; }
		public bool is_master_key_encrypted_by_server { get; set; }
		public bool is_published { get; set; }
		public bool is_subscribed { get; set; }
		public bool is_merge_published { get; set; }
		public bool is_distributor { get; set; }
		public bool is_sync_with_backup { get; set; }
		public bool is_broker_enabled { get; set; }
		public byte? log_reuse_wait { get; set; }
		public string log_reuse_wait_desc { get; set; }
		public bool is_date_correlation_on { get; set; }
		public bool is_cdc_enabled { get; set; }
		public bool? is_encrypted { get; set; }
		public bool? is_honor_broker_priority_on { get; set; }
		public string replica_id { get; set; }
		public string group_database_id { get; set; }
		public short? default_language_lcid { get; set; }
		public string default_language_name { get; set; }
		public int? default_fulltext_language_lcid { get; set; }
		public string default_fulltext_language_name { get; set; }
		public bool? is_nested_triggers_on { get; set; }
		public bool? is_transform_noise_words_on { get; set; }
		public short? two_digit_year_cutoff { get; set; }
		public byte? containment { get; set; }
		public string containment_desc { get; set; }
		public int? target_recovery_time_in_seconds { get; set; }
		// ReSharper restore InconsistentNaming

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.Where; }
		}

		protected override string DbNameForWhereClause
		{
			get { return "d.[name]"; }
		}

		public override string ToString()
		{
			return string.Format(
			                     "database_id: {0}, source_database_id: {1}, create_date: {2}, compatibility_level: {3}, collation_name: {4}, user_access: {5}, user_access_desc: {6}, is_read_only: {7}, " +
			                     "is_auto_close_on: {8}, is_auto_shrink_on: {9}, state: {10}, state_desc: {11}, is_in_standby: {12}, is_cleanly_shutdown: {13}, is_supplemental_logging_enabled: {14}, " +
			                     "snapshot_isolation_state: {15}, snapshot_isolation_state_desc: {16}, is_read_committed_snapshot_on: {17}, recovery_model: {18}, recovery_model_desc: {19}, " +
			                     "page_verify_option: {20}, page_verify_option_desc: {21}, is_auto_create_stats_on: {22}, is_auto_update_stats_on: {23}, is_auto_update_stats_async_on: {24}, " +
			                     "is_ansi_null_default_on: {25}, is_ansi_nulls_on: {26}, is_ansi_padding_on: {27}, is_ansi_warnings_on: {28}, is_arithabort_on: {29}, is_concat_null_yields_null_on: {30}, " +
			                     "is_numeric_roundabort_on: {31}, is_quoted_identifier_on: {32}, is_recursive_triggers_on: {33}, is_cursor_close_on_commit_on: {34}, is_local_cursor_default: {35}, " +
			                     "is_fulltext_enabled: {36}, is_trustworthy_on: {37}, is_db_chaining_on: {38}, is_parameterization_forced: {39}, is_master_key_encrypted_by_server: {40}, is_published: {41}, " +
			                     "is_subscribed: {42}, is_merge_published: {43}, is_distributor: {44}, is_sync_with_backup: {45}, is_broker_enabled: {46}, log_reuse_wait: {47}, log_reuse_wait_desc: {48}, " +
			                     "is_date_correlation_on: {49}, is_cdc_enabled: {50}, is_encrypted: {51}, is_honor_broker_priority_on: {52}, replica_id: {53}, group_database_id: {54}, " +
			                     "default_language_lcid: {55}, default_language_name: {56}, default_fulltext_language_lcid: {57}, default_fulltext_language_name: {58}, is_nested_triggers_on: {59}, " +
			                     "is_transform_noise_words_on: {60}, two_digit_year_cutoff: {61}, containment: {62}, containment_desc: {63}, target_recovery_time_in_seconds: {64}",
			                     database_id, source_database_id, create_date, compatibility_level, collation_name, user_access, user_access_desc, is_read_only, is_auto_close_on, is_auto_shrink_on, state,
			                     state_desc, is_in_standby, is_cleanly_shutdown, is_supplemental_logging_enabled, snapshot_isolation_state, snapshot_isolation_state_desc, is_read_committed_snapshot_on,
			                     recovery_model, recovery_model_desc, page_verify_option, page_verify_option_desc, is_auto_create_stats_on, is_auto_update_stats_on, is_auto_update_stats_async_on,
			                     is_ansi_null_default_on, is_ansi_nulls_on, is_ansi_padding_on, is_ansi_warnings_on, is_arithabort_on, is_concat_null_yields_null_on, is_numeric_roundabort_on,
			                     is_quoted_identifier_on, is_recursive_triggers_on, is_cursor_close_on_commit_on, is_local_cursor_default, is_fulltext_enabled, is_trustworthy_on, is_db_chaining_on,
			                     is_parameterization_forced, is_master_key_encrypted_by_server, is_published, is_subscribed, is_merge_published, is_distributor, is_sync_with_backup, is_broker_enabled,
			                     log_reuse_wait, log_reuse_wait_desc, is_date_correlation_on, is_cdc_enabled, is_encrypted, is_honor_broker_priority_on, replica_id, group_database_id, default_language_lcid,
			                     default_language_name, default_fulltext_language_lcid, default_fulltext_language_name, is_nested_triggers_on, is_transform_noise_words_on, two_digit_year_cutoff, containment,
			                     containment_desc, target_recovery_time_in_seconds);
		}
	}
}
