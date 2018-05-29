namespace J
{
	using J.Internal;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;

	public class TaskList
	{
		public static int DefaultMaxConcurrent = 4;

		readonly List<Func<DividableProgress, IObservable<Unit>>> list = new List<Func<DividableProgress, IObservable<Unit>>>();

		public int MaxConcurrent { get; set; } = DefaultMaxConcurrent;

		public int Count => list.Count;

		public void Clear() => list.Clear();

		public TaskList AddObservable(IObservable<Unit> observable)
		{
			if (observable != null) list.Add(progress => observable.ReportOnCompleted(progress));
			return this;
		}
		public TaskList AddObservable<T>(IObservable<T> observable)
		{
			if (observable != null)
			{
				var unitObservable = observable as IObservable<Unit>;
				if (unitObservable != null) return AddObservable(unitObservable);
				list.Add(progress => observable.AsUnitObservable().ReportOnCompleted(progress));
			}
			return this;
		}

		public TaskList AddCoroutine(Func<DividableProgress, IEnumerator> coroutine)
		{
			if (coroutine != null) list.Add(progress => coroutine(progress).ToObservable().ReportOnCompleted(progress));
			return this;
		}
		public TaskList AddCoroutine<T1>(Func<T1, DividableProgress, IEnumerator> coroutine, T1 arg1)
		{
			if (coroutine != null) list.Add(progress => coroutine(arg1, progress).ToObservable().ReportOnCompleted(progress));
			return this;
		}
		public TaskList AddCoroutine<T1, T2>(Func<T1, T2, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2)
		{
			if (coroutine != null) list.Add(progress => coroutine(arg1, arg2, progress).ToObservable().ReportOnCompleted(progress));
			return this;
		}
		public TaskList AddCoroutine<T1, T2, T3>(Func<T1, T2, T3, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3)
		{
			if (coroutine != null) list.Add(progress => coroutine(arg1, arg2, arg3, progress).ToObservable().ReportOnCompleted(progress));
			return this;
		}
		public TaskList AddCoroutine<T1, T2, T3, T4>(Func<T1, T2, T3, T4, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (coroutine != null) list.Add(progress => coroutine(arg1, arg2, arg3, arg4, progress).ToObservable().ReportOnCompleted(progress));
			return this;
		}

		public TaskList AddTaskList(TaskList taskList)
		{
			if (taskList != null) list.Add(taskList.ToObservable);
			return this;
		}

		public TaskList MergeTaskList(TaskList taskList)
		{
			if (taskList != null) list.AddRange(taskList.list);
			return this;
		}

		public IObservable<Unit> ToObservable(DividableProgress progress = null)
		{
			return list.Select(task => task(Count == 1 ? progress : progress?.Divide(1f / Count)))
				.Merge(MaxConcurrent)
				.AsSingleUnitObservable()
				.ReportOnCompleted(progress);
		}
	}

	namespace Internal
	{
		static partial class ExtensionMethods
		{
			public static IObservable<T> ReportOnCompleted<T>(this IObservable<T> observable, DividableProgress progress)
			{
				if (progress == null) return observable;
				return observable.DoOnCompleted(() => progress.Report(1f));
			}
		}
	}
}
