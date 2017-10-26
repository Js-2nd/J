using SCG = System.Collections.Generic;
using SCO = System.Collections.ObjectModel;

public static partial class GlobalExtensionMethods
{
	public static SCO.ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary) => new SCO.ReadOnlyDictionary<TKey, TValue>(dictionary);

	public static TValue GetOrDefault<TKey, TValue>(this SCG.IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		TValue value;
		if (dictionary.TryGetValue(key, out value) == false)
			value = defaultValue;
		return value;
	}
	public static TValue GetOrDefault<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue)) =>
		dictionary.AsReadOnly().StaticCast<SCG.IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, defaultValue);
	public static TValue GetOrDefault<TKey, TValue>(this SCG.Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue)) =>
		dictionary.StaticCast<SCG.IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, defaultValue);
	public static TValue GetOrDefault<TKey, TValue>(this SCO.ReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue)) =>
		dictionary.StaticCast<SCG.IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, defaultValue);

	public static TValue GetOrDefault<TKey, TValue, TNewValue>(this SCG.IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TNewValue> defaultFactory) where TNewValue : TValue
	{
		TValue value;
		if (dictionary.TryGetValue(key, out value) == false)
			value = defaultFactory(key);
		return value;
	}
	public static TValue GetOrDefault<TKey, TValue, TNewValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TNewValue> defaultFactory) where TNewValue : TValue =>
		dictionary.AsReadOnly().StaticCast<SCG.IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, defaultFactory);
	public static TValue GetOrDefault<TKey, TValue, TNewValue>(this SCG.Dictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TNewValue> defaultFactory) where TNewValue : TValue =>
		dictionary.StaticCast<SCG.IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, defaultFactory);
	public static TValue GetOrDefault<TKey, TValue, TNewValue>(this SCO.ReadOnlyDictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TNewValue> defaultFactory) where TNewValue : TValue =>
		dictionary.StaticCast<SCG.IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, defaultFactory);

	public static TValue GetOrAdd<TKey, TValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		TValue value;
		if (dictionary.TryGetValue(key, out value) == false)
			dictionary.Add(key, value = defaultValue);
		return value;
	}

	public static TValue GetOrAdd<TKey, TValue, TNewValue>(this SCG.IDictionary<TKey, TValue> dictionary, TKey key, System.Func<TKey, TNewValue> defaultFactory) where TNewValue : TValue
	{
		TValue value;
		if (dictionary.TryGetValue(key, out value) == false)
			dictionary.Add(key, value = defaultFactory(key));
		return value;
	}
}
