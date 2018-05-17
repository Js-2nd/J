namespace UniRx
{
	using System;

	public static partial class ExtensionMethods
	{
		public static void OnNext<T>(this Subject<T> subject, Func<T> factory)
		{
			if (subject.HasObservers) subject.OnNext(factory());
		}
	}
}
