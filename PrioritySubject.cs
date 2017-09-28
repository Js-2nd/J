using System;
using System.Collections.Generic;
using UniRx;

namespace J
{
	public sealed class PrioritySubject<T> : ISubject<T>, IDisposable
	{
		SortedDictionary<int, Subject<T>> dict = new SortedDictionary<int, Subject<T>> { { default(int), new Subject<T>() } };

		public bool HasObservers
		{
			get
			{
				foreach (var item in dict.Values)
					if (item.HasObservers) return true;
				return false;
			}
		}

		public void OnCompleted()
		{
			foreach (var item in dict.Values)
				item.OnCompleted();
		}

		public void OnError(Exception error)
		{
			foreach (var item in dict.Values)
				item.OnError(error);
		}

		public void OnNext(T value)
		{
			foreach (var item in dict.Values)
				item.OnNext(value);
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return Observe().Subscribe(observer);
		}

		public void Dispose()
		{
			foreach (var item in dict.Values)
				item.Dispose();
		}

		public IObservable<T> Observe(int priority = default(int))
		{
			return dict.GetOrAdd(priority, _ => new Subject<T>());
		}
	}
}
