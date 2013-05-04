using System;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [Query("FileIOView.sql", "Custom/FileIO/{DatabaseName}", QueryName = "File I/O", Enabled = true)]
    internal class FileIoView : IDatabaseMetric
    {
        public const string WhereToken = "/*{WHERE}*/";

        public long BytesRead { get; set; }
        public long BytesWritten { get; set; }
        public long SizeInBytes { get; set; }
        public string DatabaseName { get; set; }

        public string ParameterizeQuery(string commandText, string[] includeDBs, string[] excludeDBs)
        {
            if (!commandText.Contains(WhereToken))
            {
                throw new Exception(string.Format("SQL is not in the expected format. Missing replacement token '{0}'", WhereToken));
            }

            if (includeDBs != null && includeDBs.Any())
            {
                var dbIds = includeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
                var replacement = string.Format("WHERE DB_NAME(a.database_id) IN ({0})", string.Join(", ", dbIds));
                return commandText.Replace(WhereToken, replacement);
            }

            if (excludeDBs != null && excludeDBs.Any())
            {
                var dbIds = excludeDBs.Select(d => string.Format("'{0}'", d)).ToArray();
                var replacement = string.Format("WHERE DB_NAME(a.database_id) NOT IN ({0})", string.Join(", ", dbIds));
                return commandText.Replace(WhereToken, replacement);
            }

            return commandText;
        }
    }
}
