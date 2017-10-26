using SCO = System.Collections.ObjectModel;

public static partial class GlobalExtensionMethods
{
	public static SCO.ReadOnlyCollection<T> AsReadOnly<T>(this T[] array) => System.Array.AsReadOnly(array);
}
