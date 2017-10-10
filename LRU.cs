using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace J
{
	public class LRU<T> : ICollection<T>, IDisposable
	{
		public LRU(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			Capacity = capacity;
		}

		public int Capacity { get; }
		public int Count => list.Count;
		public IObservable<T> Expired => expired;

		ICollection<T> iCollection => list;
		bool ICollection<T>.IsReadOnly => iCollection.IsReadOnly;

		LinkedList<T> list = new LinkedList<T>();
		Dictionary<T, LinkedListNode<T>> dict = new Dictionary<T, LinkedListNode<T>>();
		Subject<T> expired = new Subject<T>();

		void ICollection<T>.Add(T item) => Touch(item);

		public void Clear()
		{
			list.Clear();
			dict.Clear();
		}

		public bool Contains(T value) => dict.ContainsKey(value);

		void ICollection<T>.CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

		public void Dispose() => expired.Dispose();

		public LinkedList<T>.Enumerator GetEnumerator() => list.GetEnumerator();

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

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
	}
}
