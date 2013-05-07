using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [Query("FileIOView.sql", "Component/FileIO/{DatabaseName}", QueryName = "File I/O", Enabled = true)]
    internal class FileIoView : DatabaseMetricBase
    {
        public long BytesRead { get; set; }
        public long BytesWritten { get; set; }
        public long SizeInBytes { get; set; }

        protected override string DbNameForWhereClause
        {
            get { return "DB_NAME(a.database_id)"; }
        }

        protected override WhereClauseTokenEnum WhereClauseToken
        {
            get { return WhereClauseTokenEnum.Where; }
        }
    }
}
