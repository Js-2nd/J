namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using UniRx;

	public static partial class ExtensionMethods
	{
		public static IObservable<T> ToObservable<T>(this IAsyncEnumerator<T> asyncEnumerator)
		{
			return ToObservable(asyncEnumerator, CancellationToken.None);
		}

		public static IObservable<T> ToObservable<T>(this IAsyncEnumerator<T> asyncEnumerator, CancellationToken cancellationToken)
		{
			var subject = new Subject<T>();
			Task.Run(async () =>
			{
				try
				{
					while (await asyncEnumerator.MoveNext(cancellationToken))
					{
						subject.OnNext(asyncEnumerator.Current);
					}
					subject.OnCompleted();
				}
				catch (Exception ex)
				{
					subject.OnError(ex);
				}
				finally
				{
					subject.Dispose();
				}
			});
			return subject;
		}
	}
}
