using System;
using System.Collections.Generic;
using UniRx;

namespace J
{
	using J.Internal;
	using System.Collections;
	using System.Linq;
	using TaskFunc = Func<DividableProgress, IObservable<Unit>>;
	using TaskWeightPair = KeyValuePair<Func<DividableProgress, IObservable<Unit>>, Func<float>>;
	using WeightFunc = Func<float>;

	public sealed class TaskQueue
	{
		public static int DefaultMaxConcurrent = 4;

		readonly Queue<TaskWeightPair> queue = new Queue<TaskWeightPair>();

		public int MaxConcurrent { get; set; } = DefaultMaxConcurrent;

		public int Count => queue.Count;

		public void Clear() => queue.Clear();

		public void Add(TaskFunc taskFunc, WeightFunc weightFunc = null) => queue.Enqueue(new TaskWeightPair(taskFunc, weightFunc));

		public void AddObservable(IObservable<Unit> observable, WeightFunc weightFunc = null)
		{
			if (observable == null) return;
			TaskFunc task = progress => observable.ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddObservable<T>(IObservable<T> observable, WeightFunc weightFunc = null)
		{
			if (observable == null) return;
			var unitObservable = observable as IObservable<Unit>;
			if (unitObservable != null)
			{
				AddObservable(unitObservable, weightFunc);
				return;
			}
			TaskFunc task = progress => observable.AsUnitObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}

		public void AddCoroutine(Func<DividableProgress, IEnumerator> coroutine, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(progress).ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1>(Func<T1, DividableProgress, IEnumerator> coroutine, T1 arg1, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, progress).ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1, T2>(Func<T1, T2, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, progress).ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1, T2, T3>(Func<T1, T2, T3, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, arg3, progress).ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1, T2, T3, T4>(Func<T1, T2, T3, T4, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3, T4 arg4, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, arg3, arg4, progress).ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}

		public void AddTaskQueue(TaskQueue taskQueue, WeightFunc weightFunc = null)
		{
			if (taskQueue == null) return;
			var weight = weightFunc == null ? (WeightFunc)taskQueue.GetWeight : () => taskQueue.GetWeight() * weightFunc();
			Add(taskQueue.ToObservable, weight);
		}

		public IObservable<Unit> ToObservable(IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				var dividableProgress = progress.ToDividableProgress();
				if (dividableProgress == null)
					return queue.Select(pair => pair.Key(null))
						.Merge(MaxConcurrent).AsSingleUnitObservable();
				float total = GetWeight();
				return queue.Select(pair => pair.Key(dividableProgress.Divide(pair.Weight() / total)))
					.Merge(MaxConcurrent).AsSingleUnitObservable()
					.ReportOnCompleted(dividableProgress)
					.Finally(() => lazyWeight = -1);
			});
		}

		float lazyWeight = -1;
		float GetWeight()
		{
			if (lazyWeight < 0)
			{
				lazyWeight = 0;
				foreach (var pair in queue)
					lazyWeight += pair.Weight();
			}
			return lazyWeight;
		}
	}

	namespace Internal
	{
		static partial class ExtensionMethods
		{
			public static float Weight(this TaskWeightPair pair) => pair.Value?.Invoke() ?? 1;

			public static IObservable<T> ReportOnCompleted<T>(this IObservable<T> observable, DividableProgress progress)
			{
				if (progress == null) return observable;
				return observable.DoOnCompleted(() => progress.Report(1f));
			}
		}
	}
}
