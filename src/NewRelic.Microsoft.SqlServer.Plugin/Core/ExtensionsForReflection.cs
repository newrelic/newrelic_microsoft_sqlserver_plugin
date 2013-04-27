using System;
using System.Collections.Generic;
using System.Linq;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
	public static class ExtensionsForReflection
	{
		public static bool HasCustomAttribute<TAttribute>(this Type type, bool inherit = false)
			where TAttribute : Attribute
		{
			return type.GetCustomAttributes<TAttribute>(inherit).Any();
		}

		public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type type, bool inherit = false)
			where TAttribute : Attribute
		{
			return type.GetCustomAttributes(typeof (TAttribute), inherit).Cast<TAttribute>();
		}
	}
}