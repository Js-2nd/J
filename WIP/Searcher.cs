namespace J
{
	using System;
	using System.Collections.Generic;

	public static class Searcher
	{
		public static IEnumerable<SearchNode<T>> BreadthFirst<T>(IEnumerable<T> start,
			Func<T, IEnumerable<T>> expander, int maxDepth = -1)
		{
			var visit = new HashSet<T>();
			var queue = new Queue<SearchNode<T>>();
			int count = 0;
			foreach (var value in start)
				if (visit.Add(value))
					queue.Enqueue(new SearchNode<T>(count++, value));
			while (queue.Count > 0)
			{
				var node = queue.Dequeue();
				yield return node;
				if (maxDepth < 0 || node.Depth < maxDepth)
					foreach (var value in expander(node.Value))
						if (visit.Add(value))
							queue.Enqueue(new SearchNode<T>(count++, value, node));
			}
		}
	}

	public class SearchNode<T>
	{
		public readonly int Index;
		public readonly T Value;
		public readonly SearchNode<T> Parent;
		public readonly int Depth;

		public SearchNode(int index, T value, SearchNode<T> parent = null)
		{
			Index = index;
			Value = value;
			if (parent != null)
			{
				Parent = parent;
				Depth = parent.Depth + 1;
			}
		}
	}
}
