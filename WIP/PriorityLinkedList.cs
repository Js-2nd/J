using C5;

namespace J
{
	using System.Collections;
	using System.Collections.Generic;

	public class PriorityLinkedList<T> : IReadOnlyCollection<T>
	{
		readonly TreeDictionary<int, HashedLinkedList<T>> main;
		readonly HashDictionary<T, int> sub;
		readonly IEqualityComparer<T> cmp;

		public PriorityLinkedList() : this(EqualityComparer<T>.Default) { }
		public PriorityLinkedList(IEqualityComparer<T> comparer)
		{
			main = new TreeDictionary<int, HashedLinkedList<T>>();
			sub = new HashDictionary<T, int>(comparer);
			cmp = comparer;
		}

		public int PriorityCount => main.Count;
		public int Count => sub.Count;

		public bool Contains(T item) => sub.Contains(item);

		HashedLinkedList<T> reserveList;
		public void Add(T item, int priority = 0)
		{
			int oldPriority;
			if (sub.UpdateOrAdd(item, priority, out oldPriority)) RemoveMain(oldPriority, item);
			if (reserveList == null) reserveList = new HashedLinkedList<T>(cmp);
			var list = reserveList;
			if (!main.FindOrAdd(priority, ref list)) reserveList = null;
			list.Add(item);
		}

		public bool Remove(T item)
		{
			T removedItem;
			return Remove(item, out removedItem);
		}
		public bool Remove(T item, out T removedItem)
		{
			int priority;
			if (sub.Remove(item, out priority))
			{
				removedItem = RemoveMain(priority, item);
				return true;
			}
			removedItem = default(T);
			return false;
		}

		T RemoveMain(int priority, T item)
		{
			int p = priority;
			HashedLinkedList<T> list;
			if (!main.Find(ref p, out list)) return default(T);
			T removedItem;
			list.Remove(item, out removedItem);
			if (list.Count == 0) main.Remove(priority);
			return removedItem;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var list in main.Values)
				foreach (var item in list)
					yield return item;
		}
	}
}
