namespace J
{
	using System;
	using System.Collections.Generic;

	public static partial class ExtensionMethods
	{
		public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
		{
			TValue value;
			if (dictionary.TryGetValue(key, out value) == false)
				value = defaultValue;
			return value;
		}

		public static TValue GetOrDefault<TKey, TValue, TNewValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TNewValue> defaultFactory) where TNewValue : TValue
		{
			TValue value;
			if (dictionary.TryGetValue(key, out value) == false)
				value = defaultFactory(key);
			return value;
		}
	}
}
