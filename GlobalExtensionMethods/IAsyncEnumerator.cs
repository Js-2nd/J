using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniRx;

public static partial class GlobalExtensionMethods
{
	public static IObservable<T> ToObservable<T>(this IAsyncEnumerator<T> asyncEnumerator, CancellationToken cancellationToken = default(CancellationToken))
	{
		Subject<T> subject = new Subject<T>();
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
		});
		return subject;
	}
}
