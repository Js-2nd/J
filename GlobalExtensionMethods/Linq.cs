using SCG = System.Collections.Generic;

public static partial class GlobalExtensionMethods
{
	public static SCG.IEnumerable<T> ToSingleEnumerable<T>(this T t)
	{
		yield return t;
	}
}
