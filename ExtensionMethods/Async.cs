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
				while (await @this.MoveNext())
				{
					subject.OnNext(@this.Current);
				}
				subject.OnCompleted();
			});
			return subject;
		}
	}
}
