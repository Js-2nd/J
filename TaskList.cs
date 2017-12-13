namespace J
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;

	public class TaskList
	{
		readonly List<Func<DividableProgress, IObservable<Unit>>> list = new List<Func<DividableProgress, IObservable<Unit>>>();

		public int Count => list.Count;

		public void Clear() => list.Clear();

		public void AddObservable(IObservable<Unit> observable) =>
			list.Add(progress => observable.DoOnCompleted(() => progress?.Report(1f)));
		public void AddObservable<T>(IObservable<T> observable) =>
			list.Add(progress => observable.DoOnCompleted(() => progress?.Report(1f)).AsUnitObservable());

		public void AddCoroutine(Func<DividableProgress, IEnumerator> coroutine) =>
			list.Add(progress => coroutine(progress).ToObservable());
		public void AddCoroutine<T1>(Func<T1, DividableProgress, IEnumerator> coroutine, T1 arg1) =>
			list.Add(progress => coroutine(arg1, progress).ToObservable());
		public void AddCoroutine<T1, T2>(Func<T1, T2, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2) =>
			list.Add(progress => coroutine(arg1, arg2, progress).ToObservable());
		public void AddCoroutine<T1, T2, T3>(Func<T1, T2, T3, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3) =>
			list.Add(progress => coroutine(arg1, arg2, arg3, progress).ToObservable());
		public void AddCoroutine<T1, T2, T3, T4>(Func<T1, T2, T3, T4, DividableProgress, IEnumerator> coroutine, T1 arg1, T2 arg2, T3 arg3, T4 arg4) =>
			list.Add(progress => coroutine(arg1, arg2, arg3, arg4, progress).ToObservable());

		public void AddTaskList(TaskList taskList) => list.Add(taskList.Start);

		public void MergeTaskList(TaskList taskList) => list.AddRange(taskList.list);

		public IObservable<Unit> Start(DividableProgress progress = null)
		{
			return list.Select(task => task(Count == 1 ? progress : progress?.Divide(1f / Count)))
				.WhenAll().DoOnCompleted(() => progress?.Report(1f));
		}
	}
}
