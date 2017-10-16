namespace J
{
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public static partial class ExtensionMethods
	{
		public static IEnumerable<T> ToSingleEnumerable<T>(this T @this)
		{
			yield return @this;
		}

		public static T RandomValue<T>(this IEnumerable<T> @this)
		{
			return @this.ToArray().RandomValue();
		}

		public static T RandomValue<T>(this ICollection<T> @this)
		{
			return @this.ElementAtOrDefault(Random.Range(0, @this.Count));
		}

		public static T RandomValue<T>(this IList<T> @this)
		{
			int count = @this.Count;
			return count > 0 ? @this[Random.Range(0, count)] : default(T);
		}
	}
}
