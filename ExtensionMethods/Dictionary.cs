namespace System.Collections.Generic
{
	using System;

	public static partial class ExtensionMethods
	{
		public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
				value = defaultValue;
			return value;
		}

		public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> defaultFactory)
		{
			TValue value;
			if (!dictionary.TryGetValue(key, out value))
				value = defaultFactory(key);
			return value;
		}
	}
}
