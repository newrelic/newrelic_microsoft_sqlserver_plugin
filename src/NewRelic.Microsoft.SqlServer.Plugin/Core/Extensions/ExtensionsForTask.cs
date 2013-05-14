using System;
using System.Threading.Tasks;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions
{
	public static class ExtensionsForTask
	{
		public static TTask Catch<TTask>(this TTask task, Action<Exception> catcher) where TTask : Task
		{
			if (task != null)
			{
				task.ContinueWith(innerTask =>
				                  {
					                  var exception = innerTask.Exception;
					                  if (exception != null)
					                  {
						                  var ex = exception.GetBaseException();
						                  if (catcher != null)
						                  {
							                  catcher(ex);
						                  }
					                  }
				                  }, TaskContinuationOptions.OnlyOnFaulted);
			}
			return task;
		}
	}
}