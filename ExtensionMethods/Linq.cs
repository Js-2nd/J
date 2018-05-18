﻿namespace System.Linq
{
	using System;
	using System.Collections.Generic;

	public static partial class ExtensionMethods
	{
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
