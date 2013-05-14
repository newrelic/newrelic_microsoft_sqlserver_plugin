using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions
{
	public static class ExtensionsForAssembly
	{
		public static string GetStringResource(this Assembly assembly, string resourceName)
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

		public static string SearchForStringResource(this Assembly assembly, string fullOrPartialResourceName)
		{
			var stream = assembly.GetManifestResourceStream(fullOrPartialResourceName);
			if (stream == null)
			{
				var delimitedResourceName = fullOrPartialResourceName.StartsWith(".") ? fullOrPartialResourceName : "." + fullOrPartialResourceName;

				var matchingNames = assembly.GetManifestResourceNames().Where(n => n.EndsWith(delimitedResourceName)).ToArray();
				if (!matchingNames.Any())
				{
					throw new Exception(string.Format("Unable to locate resource '{0}'", fullOrPartialResourceName));
				}

				if (matchingNames.Length > 1)
				{
					throw new Exception(string.Format("Ambiguous partial resource name '{0}' also matched '{1}'", fullOrPartialResourceName, string.Join("', '", matchingNames)));
				}

				var fullResourceName = matchingNames.Single();
				stream = assembly.GetManifestResourceStream(fullResourceName);
				if (stream == null)
				{
					throw new Exception(string.Format("Unable to locate resource '{0}' based on partial resource '{1}'", fullResourceName, fullOrPartialResourceName));
				}
			}

			using (stream)
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}

		public static string GetLocalPath(this Assembly assembly)
		{
			var uri = new Uri(assembly.CodeBase);
			return Path.GetDirectoryName(uri.LocalPath);
		}
	}
}
