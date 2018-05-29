using System;
using System.Collections.Generic;
using UniRx;

namespace J
{
	using J.Internal;
	using System.Collections;
	using TaskFunc = Func<DividableProgress, IObservable<Unit>>;
	using TaskWeightPair = KeyValuePair<Func<DividableProgress, IObservable<Unit>>, Func<float>>;
	using WeightFunc = Func<float>;

	public sealed class TaskQueue
	{
		readonly Queue<IObservable<TaskWeightPair>> queue = new Queue<IObservable<TaskWeightPair>>();

		public int Count => queue.Count;

		public void Clear() => queue.Clear();

		public void Add(TaskFunc taskFunc, WeightFunc weightFunc = null) => queue.Enqueue(Observable.Return(new TaskWeightPair(taskFunc, weightFunc)));

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
			var pairs = taskQueue.GetPairs();
			if (weightFunc != null)
			{
				float weight = 1;
				pairs = pairs.DoOnSubscribe(() => weight = weightFunc())
					.Select(pair => new TaskWeightPair(pair.Key, () => pair.Weight() * weight));
			}
			queue.Enqueue(pairs);
		}

		public IObservable<Unit> ToObservable(IProgress<float> progress = null, int maxConcurrent = 4)
		{
			return Observable.Defer(() =>
			{
				var dividableProgress = progress.ToDividableProgress();
				if (dividableProgress == null)
					return GetPairs().Select(pair => pair.Key(null))
						.Merge(maxConcurrent).AsSingleUnitObservable();
				return GetWeight().ContinueWith(total => GetPairs()
					.Select(pair => pair.Key(dividableProgress.Divide(pair.Weight() / total)))
					.Merge(maxConcurrent).AsSingleUnitObservable()
					.ReportOnCompleted(dividableProgress)
					.Finally(() => weightCache = null));
			});
		}

		IObservable<TaskWeightPair> GetPairs() => queue.Merge();

		AsyncSubject<float> weightCache;
		IObservable<float> GetWeight()
		{
			return Observable.Defer(() =>
			{
				if (weightCache == null)
				{
					weightCache = new AsyncSubject<float>();
					GetPairs().Aggregate(0f, (sum, pair) => sum + pair.Weight())
						.DoOnError(ex => weightCache = null)
						.Subscribe(weightCache);
				}
				return weightCache;
			});
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
