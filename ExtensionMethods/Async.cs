#if NET_4_6
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;

namespace J
{
	public static partial class ExtensionMethods
	{
		public static IObservable<T> ToObservable<T>(this IAsyncEnumerator<T> @this)
		{
			Subject<T> subject = new Subject<T>();
			Task.Run(async () =>
			{
				try
				{
					while (await @this.MoveNext())
					{
						subject.OnNext(@this.Current);
					}
					subject.OnCompleted();
				}
				catch (Exception e)
				{
					subject.OnError(e);
				}
			});
			return subject;
		}
	}
}
#endif
