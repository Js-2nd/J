using J.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace J
{
	using TaskFunc = Func<IProgress<float>, IObservable<Unit>>;
	using TaskWeightPair = KeyValuePair<Func<IProgress<float>, IObservable<Unit>>, Func<float>>;
	using WeightFunc = Func<float>;

	public sealed class TaskQueue : IEnumerable<TaskWeightPair>
	{
		readonly Queue<IEnumerable<TaskWeightPair>> queue = new Queue<IEnumerable<TaskWeightPair>>();

		public void Clear() => queue.Clear();

		public void Add(TaskFunc taskFunc, WeightFunc weightFunc = null)
		{
			var pair = new TaskWeightPair(taskFunc, weightFunc);
			queue.Enqueue(pair.ToSingleEnumerable());
		}

		public void AddObservable(IObservable<Unit> observable, WeightFunc weightFunc = null)
		{
			if (observable == null) return;
			TaskFunc task = observable.ReportOnCompleted;
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

		public void AddCoroutine(Func<IProgress<float>, IEnumerator> coroutine, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1>(Func<T1, IProgress<float>, IEnumerator> coroutine, T1 arg1, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1, T2>(Func<T1, T2, IProgress<float>, IEnumerator> coroutine, T1 arg1, T2 arg2, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1, T2, T3>(Func<T1, T2, T3, IProgress<float>, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, arg3, progress)
				.ToObservable().ReportOnCompleted(progress);
			Add(task, weightFunc);
		}
		public void AddCoroutine<T1, T2, T3, T4>(Func<T1, T2, T3, T4, IProgress<float>, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3, T4 arg4, WeightFunc weightFunc = null)
		{
			if (coroutine == null) return;
			TaskFunc task = progress => coroutine(arg1, arg2, arg3, arg4, progress)
				.ToObservable().ReportOnCompleted(progress);
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
					.Select(pair => new TaskWeightPair(pair.Key, () => pair.Value.Get() * weight));
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
					.Select(pair => pair.Key(dividableProgress.Divide(pair.Value.Get() / total)))
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
					GetPairs().Aggregate(0f, (sum, pair) => sum + pair.Value.Get())
						.DoOnError(ex => weightCache = null)
						.Subscribe(weightCache);
				}
				return weightCache;
			});
		}

		public IEnumerator<TaskWeightPair> GetEnumerator() => queue.SelectMany(x => x).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	namespace Internal
	{
		static partial class ExtensionMethods
		{
			public static float Get(this WeightFunc func) => func?.Invoke() ?? 1;

			public static WeightFunc Combine(this WeightFunc first, WeightFunc second)
			{
				if (first == null) return second;
				if (second == null) return first;
				return () => first() * second();
			}

			public static IObservable<T> ReportOnCompleted<T>(this IObservable<T> observable, IProgress<float> progress)
			{
				if (progress == null) return observable;
				return observable.DoOnCompleted(() => progress.Report(1f));
			}
		}
	}
}
