public static partial class GlobalExtensionMethods
{
	public static void Rethrow(this System.Exception exception) => System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
}
