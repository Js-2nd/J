namespace J
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;

	public class TaskList
	{
		List<Func<DividableProgress, IObservable<Unit>>> list = new List<Func<DividableProgress, IObservable<Unit>>>();

		public void AddTask(Func<DividableProgress, IObservable<Unit>> task) => list.Add(task);

		public void AddCoroutine(Func<DividableProgress, IEnumerator> coroutine) => list.Add(p => coroutine(p).ToObservable());

		public void AddTaskList(TaskList taskList) => list.Add(taskList.Start);

		public IObservable<Unit> Start(DividableProgress progress = null)
		{
			if (list.Count == 0)
			{
				progress?.Report(1f);
				return Observable.ReturnUnit();
			}

			if (list.Count == 1)
			{
				return list[0](progress);
			}

			float weight = 1f / list.Count;
			return list.Select(task => task(progress?.Divide(weight))).WhenAll();
		}
	}
}
