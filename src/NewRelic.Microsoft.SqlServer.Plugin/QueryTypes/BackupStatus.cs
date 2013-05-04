using System;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [Query("BackupStatus.sql", "Custom/BackupStatus/{DatabaseName}", QueryName = "Backup Status", Enabled = false)]
    [Query("LogBackupStatus.sql", "Custom/LogBackupStatus/{DatabaseName}", QueryName = "Log Backup Status", Enabled = false)]
    internal class BackupStatus : IDatabaseMetric
    {
        public string BackupDate { get; set; }
        public string Comment { get; set; }
        public string Flag { get; set; }
        public string DatabaseName { get; set; }

        public string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs)
        {
            const string whereToken = Constants.WhereClauseReplaceToken;
            if (!commandText.Contains(whereToken))
            {
                throw new Exception(string.Format("SQL is not in the expected format. Missing replacement token '{0}'", whereToken));
            }

            if (includeDBs != null && includeDBs.Any())
            {
                var dbIds = includeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
                var replacement = string.Format("WHERE s.name IN ({0})", string.Join(", ", dbIds));
                return commandText.Replace(whereToken, replacement);
            }

            if (excludeDBs != null && excludeDBs.Any())
            {
                var dbIds = excludeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
                var replacement = string.Format("WHERE s.name NOT IN ({0})", string.Join(", ", dbIds));
                return commandText.Replace(whereToken, replacement);
            }

            return commandText;
        }
    }
}
