using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("Queries.FileIOView.sql")]
	internal class FileIoView
	{
		public string DatabaseName { get; set; }
		public long BytesRead { get; set; }
		public long BytesWritten { get; set; }
		public long SizeInBytes { get; set; }

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}", DatabaseName, BytesRead, BytesWritten, SizeInBytes);
		}
	}
}
