using SCG = System.Collections.Generic;
using SCO = System.Collections.ObjectModel;

public static partial class GlobalExtensionMethods
{
	public static SCO.ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary) => new SCO.ReadOnlyDictionary<TKey, TValue>(dictionary);

	public static TValue GetOrDefault<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
			value = defaultValue;
		return value;
	}

	public static TValue GetOrDefault<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TValue> defaultFactory)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
			value = defaultFactory(key);
		return value;
	}

	public static TValue GetOrAdd<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
			dictionary.Add(key, value = defaultValue);
		return value;
	}

	public static TValue GetOrAdd<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TValue> defaultFactory)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
			dictionary.Add(key, value = defaultFactory(key));
		return value;
	}

	public static int Increment<TKey>(this SCG.IDictionary<TKey, int> dictionary, TKey key, int count = 1, int defaultValue = 0)
	{
		int value = dictionary.GetOrDefault(key, defaultValue) + count;
		dictionary[key] = value;
		return value;
	}
}
