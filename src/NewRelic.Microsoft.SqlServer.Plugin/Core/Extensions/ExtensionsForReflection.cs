using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions
{
	public static class ExtensionsForReflection
	{
		public static bool HasCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = false)
			where TAttribute : Attribute
		{
			return member.GetCustomAttributes<TAttribute>(inherit).Any();
		}

		public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this MemberInfo member, bool inherit = false)
			where TAttribute : Attribute
		{
			return member.GetCustomAttributes(typeof (TAttribute), inherit).Cast<TAttribute>();
		}

		public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo member, bool inherit = false)
			where TAttribute : Attribute
		{
			return member.GetCustomAttributes<TAttribute>(inherit).SingleOrDefault();
		}
	}
}
