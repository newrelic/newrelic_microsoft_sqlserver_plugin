using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[Query("FileIOView.sql", "Component/FileIO/{MetricName}/{DatabaseName}", QueryName = "File I/O", MetricTransformEnum = MetricTransformEnum.Delta, Enabled = true)]
	internal class FileIoView : DatabaseMetricBase
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
			get { return "DB_NAME(a.database_id)"; }
		}

		protected override WhereClauseTokenEnum WhereClauseToken
		{
			get { return WhereClauseTokenEnum.Where; }
		}
	}
}
