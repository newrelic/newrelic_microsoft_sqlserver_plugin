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

		public void AddMetrics(ComponentData componentData)
		{
			var metricPerDatabase = string.Format("FileIO/{0}/", DatabaseName);
			componentData.AddMetric(metricPerDatabase + "BytesRead", (int) BytesRead);
			componentData.AddMetric(metricPerDatabase + "BytesWritten", (int) BytesWritten);
			componentData.AddMetric(metricPerDatabase + "SizeInBytes", (int) SizeInBytes);
		}

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}", DatabaseName, BytesRead, BytesWritten, SizeInBytes);
		}
	}
}
