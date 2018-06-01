using J.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace J
{
	using TaskFunc = Func<IProgress<float>, IObservable<Unit>>;
	using TaskWeightPair = KeyValuePair<Func<IProgress<float>, IObservable<Unit>>, float?>;

	public sealed class TaskQueue
	{
		readonly Queue<IEnumerable<TaskWeightPair>> queue = new Queue<IEnumerable<TaskWeightPair>>();

		public IEnumerable<TaskWeightPair> All => queue.SelectMany(item => item);

		public int Count => queue.Count;

		public void Clear() => queue.Clear();

		public void Add(TaskFunc taskFunc, float? weight = null)
		{
			var pair = new TaskWeightPair(taskFunc, weight);
			queue.Enqueue(pair.ToSingleEnumerable());
		}

		public void AddObservable(IObservable<Unit> observable, float? weight = null)
		{
			if (observable == null) return;
			TaskFunc task = observable.ReportOnCompleted;
			Add(task, weight);
		}
		public void AddObservable<T>(IObservable<T> observable, float? weight = null)
		{
			if (observable == null) return;
			var unitObservable = observable as IObservable<Unit>;
			if (unitObservable != null)
			{
				AddObservable(unitObservable, weight);
				return;
			}
			TaskFunc task = progress => observable.AsUnitObservable().ReportOnCompleted(progress);
			Add(task, weight);
		}

		public void AddCoroutine(Func<IProgress<float>, IEnumerator> coroutine, float? weight = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weight);
		}
		public void AddCoroutine<T1>(Func<T1, IProgress<float>, IEnumerator> coroutine,
			T1 arg1, float? weight = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weight);
		}
		public void AddCoroutine<T1, T2>(Func<T1, T2, IProgress<float>, IEnumerator> coroutine,
			T1 arg1, T2 arg2, float? weight = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weight);
		}
		public void AddCoroutine<T1, T2, T3>(Func<T1, T2, T3, IProgress<float>, IEnumerator> coroutine,
			T1 arg1, T2 arg2, T3 arg3, float? weight = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, arg3, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weight);
		}
		public void AddCoroutine<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IProgress<float>, IEnumerator> coroutine,
			T1 arg1, T2 arg2, T3 arg3, T4 arg4, float? weight = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, arg3, arg4, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weight);
		}

		public void AddTaskQueue(TaskQueue taskQueue, float? weight = null)
		{
			if (taskQueue == null) return;
			var pairs = taskQueue.All;
			if (weight != null) pairs = pairs.Select(pair =>
				new TaskWeightPair(pair.Key, pair.Weight() * weight.Value));
			queue.Enqueue(pairs);
		}

		public IObservable<Unit> ToObservable(IProgress<float> progress = null, int maxConcurrent = 8)
		{
			return Observable.Defer(() =>
			{
				var dividableProgress = progress.ToDividableProgress();
				if (dividableProgress == null)
					return All.Select(pair => pair.Key(null))
						.Merge(maxConcurrent)
						.AsSingleUnitObservable();
				float total = All.Aggregate(0f, (sum, pair) => sum + pair.Weight());
				return All.Select(pair => pair.Key(dividableProgress.Divide(pair.Weight() / total)))
					.Merge(maxConcurrent)
					.ReportOnCompleted(dividableProgress)
					.AsSingleUnitObservable();
			});
		}
	}

	namespace Internal
	{
		static partial class ExtensionMethods
		{
			public static IObservable<T> ReportOnCompleted<T>(this IObservable<T> observable, IProgress<float> progress)
			{
				if (progress == null) return observable;
				return observable.DoOnCompleted(() => progress.Report(1f));
			}

			public static float Weight(this TaskWeightPair pair) => pair.Value ?? 1;
		}
	}
}
