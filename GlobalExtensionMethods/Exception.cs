public static partial class GlobalExtensionMethods
{
	public static void Throw(this System.Exception exception)
	{
#if NET_4_6
		System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
#endif
		throw exception;
	}

	public static T Throw<T>(this System.Exception exception)
	{
#if NET_4_6
		System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
#endif
		throw exception;
	}
}
