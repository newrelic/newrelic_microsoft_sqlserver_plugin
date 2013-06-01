using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("FileIOView.sql", "Component/FileIO/{MetricName}/{DatabaseName}", QueryName = "File I/O", MetricTransformEnum = MetricTransformEnum.Delta, Enabled = true)]
	public class FileIoView : DatabaseMetricBase
	{
		[Metric(MetricValueType = MetricValueType.Value, Units = "[bytes]")]
		public long BytesRead { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[bytes]")]
		public long BytesWritten { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[reads]")]
		public long NumberOfReads { get; set; }

		[Metric(MetricValueType = MetricValueType.Count, Units = "[writes]")]
		public long NumberOfWrites { get; set; }

		[Metric(MetricValueType = MetricValueType.Value, Units = "[bytes]")]
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
