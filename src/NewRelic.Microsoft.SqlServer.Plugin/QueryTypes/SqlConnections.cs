using System;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlMonitorQuery("SQLConnections.sql")]
	internal class SqlConnections
	{
		public Guid ConnectionId { get; set; }
		public string ClientNetAddress { get; set; }
		public int NumberOfReads { get; set; }
		public int NumberOfWrites { get; set; }

		public override string ToString()
		{
			return string.Format("{0}\t{1}\t{2}\t{3}", ConnectionId, ClientNetAddress, NumberOfReads, NumberOfWrites);
		}
	}
}
