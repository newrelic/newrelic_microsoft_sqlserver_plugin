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

		public string DefineComponent(string sqlInstance)
		{
			return string.Format(@"{0} - {1}", sqlInstance, DatabaseName);
		}

		public void AddMetrics(ComponentData componentData)
		{
			componentData.AddMetric("FileIO/BytesRead", (int)BytesRead);
			componentData.AddMetric("FileIO/BytesWritten", (int)BytesWritten);
			componentData.AddMetric("FileIO/SizeInBytes", (int)SizeInBytes);
		}
	}
}
