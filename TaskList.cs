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

		public void AddObservable(Func<DividableProgress, IObservable<Unit>> observable) => list.Add(observable);

		public void AddObservable(IObservable<Unit> observable) => list.Add(p => observable.Finally(() => p?.Report(1f)));

		public void AddObservable<T>(IObservable<T> observable) => list.Add(p => observable.Finally(() => p?.Report(1f)).AsUnitObservable());

		public void AddCoroutine(Func<DividableProgress, IEnumerator> coroutine) => list.Add(p => coroutine(p).ToObservable());

		public void AddTaskList(TaskList taskList) => list.Add(taskList.Start);

		public void MergeTaskList(TaskList taskList) => list.AddRange(taskList.list);

		public IObservable<Unit> Start(DividableProgress progress = null)
		{
			if (Count == 0)
			{
				progress?.Report(1f);
				return Observable.ReturnUnit();
			}

			if (Count == 1)
			{
				return list[0](progress);
			}

			float weight = 1f / Count;
			return list.Select(task => task(progress?.Divide(weight))).WhenAll();
		}
	}
}
