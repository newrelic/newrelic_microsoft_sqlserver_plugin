using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("FileIOView.sql", QueryName = "File I/O", Enabled = true)]
	internal class FileIoView : IQueryResult
	{
		public string DatabaseName { get; set; }
		public long BytesRead { get; set; }
		public long BytesWritten { get; set; }
		public long SizeInBytes { get; set; }

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}", DatabaseName, BytesRead, BytesWritten, SizeInBytes);
		}

		public string DefineComponent(string agent)
		{
			return string.Format("{0}.{1}", agent, DatabaseName);
		}

		public void AddMetrics(ComponentData componentData)
		{
			componentData.AddMetric("File I/O - Bytes Read", (int)BytesRead);
			componentData.AddMetric("File I/O - Bytes Written", (int)BytesWritten);
			componentData.AddMetric("File I/O - Size In Bytes", (int)SizeInBytes);
		}
	}
}
