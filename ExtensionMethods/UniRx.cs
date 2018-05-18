namespace UniRx
{
	using System;

	public static partial class ExtensionMethods
	{
		public static IObservable<T> FirstOrEmpty<T>(this IObservable<T> source)
		{
			return source.Take(1);
		}
		public static IObservable<T> FirstOrEmpty<T>(this IObservable<T> source, Func<T, bool> predicate)
		{
			return source.Where(predicate).Take(1);
		}
		public static IObservable<T> FirstOrEmpty<T>(this IObservable<T> source, Func<T, int, bool> predicate)
		{
			return source.Where(predicate).Take(1);
		}

		public static void OnNext<T>(this Subject<T> subject, Func<T> factory)
		{
			if (subject.HasObservers) subject.OnNext(factory());
		}
	}
}
