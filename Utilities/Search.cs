namespace J
{
	using System;
	using System.Collections.Generic;

	public static class Search
	{
		public static IEnumerable<T> BreadthFirst<T>(IEnumerable<T> source,
			Func<T, IEnumerable<T>> expander, int maxDepth = -1,
			SearchYieldType yieldType = SearchYieldType.ExcludeSource,
			IEqualityComparer<T> equalityComparer = null)
		{
			if (equalityComparer == null) equalityComparer = EqualityComparer<T>.Default;
			var yield = new HashSet<T>(equalityComparer);
			var expand = new HashSet<T>(equalityComparer);
			var queue = new Queue<QueueItem<T>>();

			foreach (var item in source)
				if (expand.Add(item))
				{
					if (yieldType != SearchYieldType.ExcludeSourceAtFirst) yield.Add(item);
					queue.Enqueue(new QueueItem<T>(item, yieldType == SearchYieldType.IncludeSource, true, 0));
				}

			while (queue.Count > 0)
			{
				var item = queue.Dequeue();
				if (item.ShouldYield) yield return item.Value;
				if (item.ShouldExpand && (maxDepth < 0 || item.Depth < maxDepth))
				{
					int depth = item.Depth + 1;
					foreach (var next in expander(item.Value))
					{
						bool shouldYield = yield.Add(next);
						bool shouldExpand = expand.Add(next);
						if (shouldYield || shouldExpand)
							queue.Enqueue(new QueueItem<T>(next, shouldYield, shouldExpand, depth));
					}
				}
			}
		}

		class QueueItem<T>
		{
			public readonly T Value;
			public readonly bool ShouldYield;
			public readonly bool ShouldExpand;
			public readonly int Depth;

			public QueueItem(T value, bool shouldYield, bool shouldExpand, int depth)
			{
				Value = value;
				ShouldYield = shouldYield;
				ShouldExpand = shouldExpand;
				Depth = depth;
			}
		}
	}

	public enum SearchYieldType
	{
		ExcludeSource = 0,
		ExcludeSourceAtFirst = 1,
		IncludeSource = 2,
	}
}
