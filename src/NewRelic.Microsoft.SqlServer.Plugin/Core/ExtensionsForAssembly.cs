using System;
using System.IO;
using System.Reflection;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	public static class ExtensionsForAssembly
	{
		public static string GetStringFromResources(this Assembly assembly, string resourceName)
		{
			var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
			{
				throw new Exception(string.Format("Unable to locate resource '{0}'", resourceName));
			}

			using (stream)
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}
	}
}