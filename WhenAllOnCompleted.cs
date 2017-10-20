namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;

	public static partial class ExtensionMethods
	{
		public static IObservable<Unit> WhenAllOnCompleted(this IEnumerable<IObservable<Unit>> sources)
		{
			return new WhenAllOnCompleted(sources);
		}
	}

	partial class WhenAllOnCompleted : IObservable<Unit>
	{
		readonly IEnumerable<IObservable<Unit>> sources;

		public WhenAllOnCompleted(IEnumerable<IObservable<Unit>> sources)
		{
			this.sources = sources;
		}

		public IDisposable Subscribe(IObserver<Unit> observer)
		{
			return sources.WhenAll().Subscribe(observer);
		}
	}
}
