public static partial class GlobalExtensionMethods
{
	public static System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly<T>(this T[] array) => System.Array.AsReadOnly(array);
}
