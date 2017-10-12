namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;

	public class LRUDictionary<TKey, TValue> : VirtualDictionary<TKey, TValue>, IDisposable
	{
		LRU<TKey> lru;
		IObservable<KeyValuePair<TKey, TValue>> expired;

		public LRUDictionary(int capacity)
		{
			lru = new LRU<TKey>(capacity);
			lru.Expired.Subscribe(key => Remove(key));
			expired = lru.Expired.Select(key => new KeyValuePair<TKey, TValue>(key, base[key])).Share();
		}

		public override TValue this[TKey key]
		{
			get
			{
				TValue value = base[key];
				lru.Touch(key);
				return value;
			}
			set
			{
				base[key] = value;
				lru.Touch(key);
			}
		}

		public IObservable<KeyValuePair<TKey, TValue>> Expired => expired;

		public override void Add(TKey key, TValue value)
		{
			base.Add(key, value);
			lru.Touch(key);
		}

		public override void Clear()
		{
			base.Clear();
			lru.Clear();
		}

		public override bool ContainsKey(TKey key)
		{
			bool ok = base.ContainsKey(key);
			if (ok) lru.Touch(key);
			return ok;
		}

		public void Dispose() => lru.Dispose();

		public override bool Remove(TKey key)
		{
			bool ok = base.Remove(key);
			if (ok) lru.Remove(key);
			return ok;
		}

		public override bool TryGetValue(TKey key, out TValue value)
		{
			bool ok = base.TryGetValue(key, out value);
			if (ok) lru.Touch(key);
			return ok;
		}
	}
}
