using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J
{
	public static partial class ExtensionMethods
	{
		public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue))
		{
			TValue value;
			if (@this.TryGetValue(key, out value) == false)
				value = @default;
			return value;
		}
		public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue)) =>
			new ReadOnlyDictionary<TKey, TValue>(@this).StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, @default);
		public static TValue GetOrDefault<TKey, TValue>(this ReadOnlyDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue)) =>
			@this.StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, @default);
		public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue)) =>
			@this.StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, @default);

		public static TValue GetOrDefault<TKey, TValue, TNewValue>(this IReadOnlyDictionary<TKey, TValue> @this, TKey key, Func<TKey, TNewValue> factory) where TNewValue : TValue
		{
			TValue value;
			if (@this.TryGetValue(key, out value) == false)
				value = factory(key);
			return value;
		}
		public static TValue GetOrDefault<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TNewValue> factory) where TNewValue : TValue =>
			new ReadOnlyDictionary<TKey, TValue>(@this).StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, factory);
		public static TValue GetOrDefault<TKey, TValue, TNewValue>(this ReadOnlyDictionary<TKey, TValue> @this, TKey key, Func<TKey, TNewValue> factory) where TNewValue : TValue =>
			@this.StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, factory);
		public static TValue GetOrDefault<TKey, TValue, TNewValue>(this Dictionary<TKey, TValue> @this, TKey key, Func<TKey, TNewValue> factory) where TNewValue : TValue =>
			@this.StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, factory);

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue))
		{
			TValue value;
			if (@this.TryGetValue(key, out value) == false)
				@this.Add(key, value = @default);
			return value;
		}

		public static TValue GetOrAdd<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TKey, TNewValue> factory) where TNewValue : TValue
		{
			TValue value;
			if (@this.TryGetValue(key, out value) == false)
				@this.Add(key, value = factory(key));
			return value;
		}
	}
}
