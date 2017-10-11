namespace J
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UniRx;

	public class LRU<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection, IDisposable
	{
		LinkedList<T> list;
		Dictionary<T, LinkedListNode<T>> dict;
		Subject<T> expired;

		public LRU(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			list = new LinkedList<T>();
			dict = new Dictionary<T, LinkedListNode<T>>();
			expired = new Subject<T>();

			Capacity = capacity;
		}

		public int Capacity { get; }
		public int Count => list.Count;
		public IObservable<T> Expired => expired;

		public void Clear()
		{
			list.Clear();
			dict.Clear();
		}

		public bool Contains(T value) => dict.ContainsKey(value);

		public void Dispose() => expired.Dispose();

		public LinkedList<T>.Enumerator GetEnumerator() => list.GetEnumerator();

		public bool Remove(T item) => Remove(dict.GetOrDefault(item));

		public void Touch(T value)
		{
			LinkedListNode<T> node;
			if (dict.TryGetValue(value, out node))
			{
				Renew(node);
			}
			else
			{
				Add(value);
			}
		}

		void Add(T value)
		{
			var node = new LinkedListNode<T>(value);
			list.AddLast(node);
			dict.Add(value, node);

			if (list.Count > Capacity)
			{
				var first = list.First;
				Remove(first);
				expired.OnNext(first.Value);
			}
		}

		bool Remove(LinkedListNode<T> node)
		{
			if (dict.Remove(node.Value))
			{
				list.Remove(node);
				return true;
			}
			return false;
		}

		void Renew(LinkedListNode<T> node)
		{
			list.Remove(node);
			list.AddLast(node);
		}

		#region Interface

		bool ICollection<T>.IsReadOnly => ((ICollection<T>)list).IsReadOnly;
		void ICollection<T>.Add(T item) => Touch(item);
		void ICollection<T>.CopyTo(T[] array, int index) => list.CopyTo(array, index);

		bool ICollection.IsSynchronized => ((ICollection)list).IsSynchronized;
		object ICollection.SyncRoot => ((ICollection)list).SyncRoot;
		void ICollection.CopyTo(Array array, int index) => ((ICollection)list).CopyTo(array, index);

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => list.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

		#endregion
	}
}
