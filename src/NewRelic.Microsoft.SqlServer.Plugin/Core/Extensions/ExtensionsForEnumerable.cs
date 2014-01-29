using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions
{
	public static class ExtensionsForEnumerable
	{
		[DebuggerStepThrough]
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
		{
			var array = values.ToArray();
			foreach (var value in array)
			{
				action(value);
			}
			return array;
		}
	}
}
