namespace System.Linq
{
	using System;
	using System.Collections.Generic;

	public static partial class ExtensionMethods
	{
		public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
				yield return item;
			}
		}
		public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T, int> action)
		{
			int index = -1;
			foreach (var item in source)
			{
				++index;
				action(item, index);
				yield return item;
			}
		}

		public static IEnumerable<T> DoOnStart<T>(this IEnumerable<T> source, Action action)
		{
			action();
			foreach (var item in source)
				yield return item;
		}

		public static IEnumerable<T> FirstOrEmpty<T>(this IEnumerable<T> source)
		{
			return source.Take(1);
		}
		public static IEnumerable<T> FirstOrEmpty<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			return source.Where(predicate).Take(1);
		}
		public static IEnumerable<T> FirstOrEmpty<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
		{
			return source.Where(predicate).Take(1);
		}

		public static IEnumerable<T> ToSingleEnumerable<T>(this T t)
		{
			yield return t;
		}
	}
}
