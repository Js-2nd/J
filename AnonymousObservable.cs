namespace J
{
	using System;

	public class AnonymousObservable<T> : IObservable<T>
	{
		readonly Func<IObserver<T>, IDisposable> onSubscribe;

		public AnonymousObservable(Func<IObserver<T>, IDisposable> onSubscribe)
		{
			this.onSubscribe = onSubscribe;
		}

		public IDisposable Subscribe(IObserver<T> observer) => onSubscribe(observer);
	}
}
