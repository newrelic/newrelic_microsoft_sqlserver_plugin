using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
    [SqlServerQuery("FileIOView.sql", "FileIO/{MetricName}/{DatabaseName}", QueryName = "File I/O", Enabled = true)]
    public class FileIoView : DatabaseMetricBase
    {
        [Metric(MetricValueType = MetricValueType.Value, Units = "bytes", MetricTransform = MetricTransformEnum.Delta)]
        public long BytesRead { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "bytes", MetricTransform = MetricTransformEnum.Delta)]
        public long BytesWritten { get; set; }

        [Metric(MetricValueType = MetricValueType.Count, Units = "reads", MetricTransform = MetricTransformEnum.Delta)]
        public long NumberOfReads { get; set; }

        [Metric(MetricValueType = MetricValueType.Count, Units = "writes", MetricTransform = MetricTransformEnum.Delta)]
        public long NumberOfWrites { get; set; }

        [Metric(MetricValueType = MetricValueType.Value, Units = "bytes", MetricTransform = MetricTransformEnum.Delta)]
        public long SizeInBytes { get; set; }

        protected override string DbNameForWhereClause
        {
            get { return "d.name"; }
        }

        protected override WhereClauseTokenEnum WhereClauseToken
        {
            get { return WhereClauseTokenEnum.Where; }
        }

        public override string ToString()
        {
            return string.Format("DatabaseName: {0},\t" +
                                 "BytesRead: {1},\t" +
                                 "BytesWritten: {2},\t" +
                                 "NumberOfReads: {3},\t" +
                                 "NumberOfWrites: {4},\t" +
                                 "SizeInBytes: {5}",
                                 DatabaseName, BytesRead, BytesWritten, NumberOfReads, NumberOfWrites, SizeInBytes);
        }
    }
}
