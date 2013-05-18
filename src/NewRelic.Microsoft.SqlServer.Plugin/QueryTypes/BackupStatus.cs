using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [SqlServerQuery("BackupStatus.sql", "Component/BackupStatus/{DatabaseName}", QueryName = "Backup Status", Enabled = false)]
    [SqlServerQuery("LogBackupStatus.sql", "Component/LogBackupStatus/{DatabaseName}", QueryName = "Log Backup Status", Enabled = false)]
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
