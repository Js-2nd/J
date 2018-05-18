using J.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;
using UniRx;

namespace System.Collections.Generic
{
	public static partial class ExtensionMethods
	{
		public static IObservable<T> ToObservable<T>(this IAsyncEnumerator<T> source, CancellationToken cancel = default(CancellationToken))
		{
			return Observable.Defer(() =>
			{
				var subject = new Subject<T>();
				Task.Run(async () =>
				{
					try
					{
						while (await source.MoveNext(cancel))
							subject.OnNext(source.Current);
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
			});
		}
	}
}

namespace J
{
	static class InternalClass
	{
		static void IAsyncEnumerator<T>(IAsyncEnumerator<T> _) { }
	}

	namespace Internal
	{
		public interface IAsyncEnumerator<out T> : IDisposable
		{
			T Current { get; }
			Task<bool> MoveNext(CancellationToken cancellationToken);
		}
	}
}
