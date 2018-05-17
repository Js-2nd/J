namespace System.Linq
{
	using System;
	using System.Collections.Generic;

	public static partial class ExtensionMethods
	{
		public static IEnumerable<T> FirstOrEmpty<T>(this IEnumerable<T> source)
		{
			foreach (var item in source)
			{
				yield return item;
				yield break;
			}
		}
		public static IEnumerable<T> FirstOrEmpty<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			foreach (var item in source)
			{
				if (predicate(item))
				{
					yield return item;
					yield break;
				}
			}
		}
		public static IEnumerable<T> FirstOrEmpty<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
		{
			int count = 0;
			foreach (var item in source)
			{
				if (predicate(item, count++))
				{
					yield return item;
					yield break;
				}
			}
		}

		public static IEnumerable<T> ToSingleEnumerable<T>(this T t)
		{
			yield return t;
		}
	}
}
