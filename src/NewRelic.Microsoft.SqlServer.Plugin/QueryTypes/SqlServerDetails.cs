using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[SqlServerQuery("ServerDetails.SqlServer.sql", "Component/SqlServerDetails", QueryName = "SQL Server Details", Enabled = false)]
	public class SqlServerDetails
	{
		public string ProductVersion { get; set; }
		public string SQLTitle { get; set; }
		public string ProductLevel { get; set; }
		public string Edition { get; set; }

		public override string ToString()
		{
			return string.Format("ProductVersion: {0},\tSQLTitle: {1},\tProductLevel: {2},\tEdition: {3}",
			                     ProductVersion, SQLTitle, ProductLevel, Edition);
		}
	}
}
