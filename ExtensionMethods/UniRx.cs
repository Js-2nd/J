namespace UniRx
{
	using J;
	using System;
	using System.Collections.Generic;

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

		public static TaskList ToTaskList<T>(this IEnumerable<IObservable<T>> sources)
		{
			var list = new TaskList();
			foreach (var source in sources)
				list.AddObservable(source);
			return list;
		}

		public static TaskQueue ToTaskQueue<T>(this IEnumerable<IObservable<T>> sources)
		{
			var queue = new TaskQueue();
			foreach (var source in sources)
				queue.AddObservable(source);
			return queue;
		}
	}
}
