using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [Query("BackupStatus.sql", "Custom/BackupStatus/{DatabaseName}", QueryName = "Backup Status", Enabled = false)]
    [Query("LogBackupStatus.sql", "Custom/LogBackupStatus/{DatabaseName}", QueryName = "Log Backup Status", Enabled = false)]
    internal class BackupStatus : DatabaseMetricBase
    {
        public string BackupDate { get; set; }
        public string Comment { get; set; }
        public string Flag { get; set; }

        protected override WhereClauseTokenEnum WhereClauseToken
        {
            get { return WhereClauseTokenEnum.Where; }
        }

        protected override string DbNameForWhereClause
        {
            get { return "s.name"; }
        }
    }
}
