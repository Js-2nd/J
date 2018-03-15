namespace J
{
	using System.Collections.Generic;

	public static partial class ExtensionMethods
	{
		public static IEnumerable<T> ToSingleEnumerable<T>(this T t)
		{
			yield return t;
		}
	}
}
