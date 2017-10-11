namespace J
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public abstract class VirtualDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	{
		protected Dictionary<TKey, TValue> innerDict = new Dictionary<TKey, TValue>();

		public virtual TValue this[TKey key] { get { return innerDict[key]; } set { innerDict[key] = value; } }

		public virtual int Count => innerDict.Count;

		public virtual ICollection<TKey> Keys => innerDict.Keys;

		public virtual ICollection<TValue> Values => innerDict.Values;

		public virtual void Add(TKey key, TValue value) => innerDict.Add(key, value);

		public virtual void Clear() => innerDict.Clear();

		public virtual bool ContainsKey(TKey key) => innerDict.ContainsKey(key);

		public virtual bool ContainsValue(TValue value) => innerDict.ContainsValue(value);

		public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => innerDict.GetEnumerator();

		public virtual bool Remove(TKey key) => innerDict.Remove(key);

		public virtual bool TryGetValue(TKey key, out TValue value) => innerDict.TryGetValue(key, out value);

		#region Interface

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => innerDict.StaticCast<ICollection<KeyValuePair<TKey, TValue>>>().IsReadOnly;

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			if (!TryGetValue(item.Key, out value)) return false;
			return EqualityComparer<TValue>.Default.Equals(value, item.Value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) => innerDict.StaticCast<ICollection<KeyValuePair<TKey, TValue>>>().CopyTo(array, index);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			TValue value;
			if (!TryGetValue(item.Key, out value)) return false;
			if (!EqualityComparer<TValue>.Default.Equals(value, item.Value)) return false;
			return Remove(item.Key);
		}

		#endregion
	}

	public static partial class ExtensionMethods
	{
		public static TValue GetOrDefault<TKey, TValue>(this VirtualDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue)) =>
			@this.StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, @default);

		public static TValue GetOrDefault<TKey, TValue, TNewValue>(this VirtualDictionary<TKey, TValue> @this, TKey key, Func<TKey, TNewValue> factory) where TNewValue : TValue =>
			@this.StaticCast<IReadOnlyDictionary<TKey, TValue>>().GetOrDefault(key, factory);
	}
}
