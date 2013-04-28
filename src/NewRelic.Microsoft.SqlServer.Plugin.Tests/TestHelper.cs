using System.IO;
using System.Reflection;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	public static class TestHelper
	{
		/// <summary>
		/// Finds the "local" dir that serves as the artifacti directory for tests.
		/// </summary>
		/// <returns></returns>
		public static string GetArtifactDir()
		{
			var assemblyPath = Assembly.GetExecutingAssembly().GetLocalPath();
			var currentPath = Path.GetDirectoryName(assemblyPath);
			while (currentPath != null && !File.Exists(Path.Combine(currentPath, "NewRelic.Microsoft.SqlServer.Plugin.sln")))
			{
				currentPath = Path.GetDirectoryName(currentPath);
			}

			if (currentPath == null) return null;

			var artifactPath = Path.Combine(Path.Combine(currentPath, "local"), "test");
			if (!Directory.Exists(artifactPath))
			{
				Directory.CreateDirectory(artifactPath);
			}
			return artifactPath;
		}
	}
}
