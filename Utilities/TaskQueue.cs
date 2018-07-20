namespace J
{
	using J.Internal;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;

	public delegate IObservable<Unit> TaskFunc(IProgress<float> progress = null);

	public struct TaskWeightPair
	{
		public TaskFunc Task;
		public float? Weight;

		public TaskWeightPair(TaskFunc task, float? weight = null)
		{
			Task = task;
			Weight = weight;
		}

		public float GetWeightOrDefault() => Weight ?? 1;
	}

	public sealed class TaskQueue
	{
		readonly Queue<IEnumerable<TaskWeightPair>> queue = new Queue<IEnumerable<TaskWeightPair>>();

		public IEnumerable<TaskWeightPair> All
		{
			get
			{
				foreach (var pairs in queue)
					foreach (var pair in pairs)
						yield return pair;
			}
		}

		public int Count => queue.Count;

		public void Clear() => queue.Clear();

		public void Add(TaskFunc taskFunc, float? weight = null) => Add(new TaskWeightPair(taskFunc, weight));
		public void Add(TaskWeightPair pair) => queue.Enqueue(pair.ToSingleEnumerable());

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
				new TaskWeightPair(pair.Task, pair.GetWeightOrDefault() * weight.Value));
			queue.Enqueue(pairs);
		}

		public IObservable<Unit> ToObservable(IProgress<float> progress = null, Action<Exception> catchIgnore = null,
			int maxConcurrent = 8) => Observable.Defer(() =>
		{
			var dividableProgress = progress.ToDividableProgress();
			bool hasProgress = dividableProgress != null;
			IEnumerable<IObservable<Unit>> all;
			if (hasProgress)
			{
				float total = All.Aggregate(0f, (sum, pair) => sum + pair.GetWeightOrDefault());
				all = All.Select(pair => pair.Task(dividableProgress.Divide(pair.GetWeightOrDefault() / total)));
			}
			else
			{
				all = All.Select(pair => pair.Task());
			}
			if (catchIgnore != null)
				all = all.Select(source => source.CatchIgnore(catchIgnore));
			var merge = all.Merge(maxConcurrent);
			if (hasProgress)
				merge = merge.ReportOnCompleted(dividableProgress);
			return merge.AsSingleUnitObservable();
		});
	}

	namespace Internal
	{
		public static partial class ExtensionMethods
		{
			public static IObservable<T> ReportOnCompleted<T>(this IObservable<T> observable, IProgress<float> progress)
			{
				if (progress == null) return observable;
				return observable.DoOnCompleted(() => progress.Report(1));
			}
		}
	}
}
