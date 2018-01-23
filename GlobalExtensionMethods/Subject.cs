public static partial class GlobalExtensionMethods
{
	public static void OnNext<T>(this UniRx.Subject<T> subject, System.Func<T> factory)
	{
		if (subject.HasObservers) subject.OnNext(factory());
	}
}
