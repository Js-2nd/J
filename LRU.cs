﻿using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace J
{
	public class LRU<T> : ICollection<T>, IDisposable
	{
		public int Capacity { get; }
		public int Count { get { return list.Count; } }
		public IObservable<T> Expired { get { return expired; } }

		bool ICollection<T>.IsReadOnly { get; }

		LinkedList<T> list = new LinkedList<T>();
		Dictionary<T, LinkedListNode<T>> dict = new Dictionary<T, LinkedListNode<T>>();
		Subject<T> expired = new Subject<T>();

		public LRU(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			Capacity = capacity;
		}

		void ICollection<T>.Add(T value) => Touch(value);

		public void Clear()
		{
			list.Clear();
			dict.Clear();
		}

		public bool Contains(T value) => dict.ContainsKey(value);

		public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

		public void Dispose() => expired.Dispose();

		public LinkedList<T>.Enumerator GetEnumerator() => list.GetEnumerator();

		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
			if (node?.List != list)
				return false;

			list.Remove(node);
			dict.Remove(node.Value);
			return true;
		}

		void Renew(LinkedListNode<T> node)
		{
			list.Remove(node);
			list.AddLast(node);
		}
	}
}