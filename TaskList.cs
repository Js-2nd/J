namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;

	public class TaskList : List<Func<DividableProgress, IObservable<Unit>>>
	{
		public void Add(TaskList taskList) => Add(taskList.Start);

		public IObservable<Unit> Start(DividableProgress progress = null)
		{
			if (Count == 0)
			{
				progress?.Report(1f);
				return Observable.ReturnUnit();
			}

			if (Count == 1)
			{
				return this[0](progress);
			}

			float weight = 1f / Count;
			return this.Select(task => task(progress?.Divide(weight))).WhenAll();
		}
	}
}
