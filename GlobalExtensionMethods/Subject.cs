using System;
using UniRx;

public static partial class GlobalExtensionMethods
{
	public static void OnNext<T>(this Subject<T> subject, Func<T> factory)
	{
		if (subject.HasObservers) subject.OnNext(factory());
	}
}
