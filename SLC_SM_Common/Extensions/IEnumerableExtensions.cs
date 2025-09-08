namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class IEnumerableExtensions
	{
		public static bool ScrambledEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			var cnt = new Dictionary<T, int>();
			foreach (T s in first)
			{
				if (cnt.ContainsKey(s))
				{
					cnt[s]++;
				}
				else
				{
					cnt.Add(s, 1);
				}
			}

			foreach (T s in second)
			{
				if (cnt.ContainsKey(s))
				{
					cnt[s]--;
				}
				else
				{
					return false;
				}
			}

			return cnt.Values.All(c => c == 0);
		}

		public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
		{
			return source ?? Enumerable.Empty<T>();
		}

		public static Dictionary<TKey, TElement> SafeToDictionary<TSource, TKey, TElement>(
			 this IEnumerable<TSource> source,
			 Func<TSource, TKey> keySelector,
			 Func<TSource, TElement> elementSelector,
			 IEqualityComparer<TKey> comparer = null)
		{
			var dictionary = new Dictionary<TKey, TElement>(comparer);

			if (source == null)
			{
				return dictionary;
			}

			foreach (TSource element in source)
			{
				dictionary[keySelector(element)] = elementSelector(element);
			}

			return dictionary;
		}

		public static Dictionary<TKey, TSource> SafeToDictionary<TSource, TKey>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer = null)
		{
			return SafeToDictionary(source, keySelector, x => x, comparer);
		}

		public static bool AllOfType<T>(this IEnumerable<object> source)
		{
			return source.All(x => x is T);
		}
	}
}
