namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.ExceptionServices;

	public static class ExceptionExtensions
	{
		public static void ThrowIfNotEmpty(this IEnumerable<Exception> exceptions)
		{
			if (exceptions == null)
			{
				return;
			}

			var array = exceptions.ToArray();

			if (array.Length == 1)
			{
				ExceptionDispatchInfo.Capture(array[0]).Throw();
			}

			if (array.Length > 1)
			{
				throw new AggregateException(array);
			}
		}

		public static IEnumerable<string> GetAllExceptionMessages(this Exception exception)
		{
			if (exception is AggregateException aggregateException)
			{
				var innerExceptions = aggregateException.Flatten().InnerExceptions;

				foreach (var message in innerExceptions.SelectMany(x => GetAllExceptionMessages(x)))
				{
					yield return message;
				}
			}
			else
			{
				yield return exception.Message;
			}
		}

		public static IEnumerable<Exception> Unwrap(this Exception exception)
		{
			if (exception is AggregateException aggregateException)
			{
				var innerExceptions = aggregateException.Flatten().InnerExceptions;

				foreach (var inner in innerExceptions)
				{
					yield return inner;
				}
			}
			else
			{
				yield return exception;
			}
		}
	}
}
