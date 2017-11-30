namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;

	public static partial class ExtensionMethods
	{
		public static IObservable<Unit> WhenAllWithProgress(this IEnumerable<IObservable<Unit>> sources, DividableProgress progress = null)
		{
			return new WhenAllWithProgress(sources, progress);
		}
	}

	class WhenAllWithProgress : IObservable<Unit>
	{
		readonly IEnumerable<IObservable<Unit>> sources;

		DividableProgress progress;

		public WhenAllWithProgress(IEnumerable<IObservable<Unit>> sources, DividableProgress progress)
		{
			this.sources = sources;
			this.progress = progress;
		}

		WhenAllWithProgress AddProgress(DividableProgress dp)
		{
			if (progress == null) progress = dp;
			else progress.Subscribe(dp.Report);
			return this;
		}

		public IDisposable Subscribe(IObserver<Unit> observer)
		{
			if (progress == null) return sources.WhenAll().Subscribe(observer);

			var list = new List<IObservable<Unit>>(sources);
			if (list.Count == 0)
			{
				progress.Report(1f);
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
				return Disposable.Empty;
			}

			return new Instance(progress, observer, list);
		}

		class Instance : IObserver<Unit>, IDisposable
		{
			readonly CompositeDisposable cancel = new CompositeDisposable();

			readonly DividableProgress progress;
			readonly IObserver<Unit> observer;

			readonly int total;
			readonly float weight;

			int count;

			public Instance(DividableProgress dp, IObserver<Unit> ob, List<IObservable<Unit>> list)
			{
				progress = dp;
				observer = ob;

				total = list.Count;
				weight = 1f / total;

				for (int i = 0; i < total; i++)
				{
					var source = list[i];
					var whenAll = source as WhenAllWithProgress;
					if (whenAll != null)
					{
						whenAll.AddProgress(total > 1 ? progress.Divide(weight) : progress)
							.Subscribe(this).AddTo(cancel);
					}
					else
					{
						source.DoOnCompleted(() => progress.ReportDelta(weight))
							.Subscribe(this).AddTo(cancel);
					}
				}
			}

			public void OnNext(Unit value) { }

			public void OnError(Exception error)
			{
				try { observer.OnError(error); }
				finally { Dispose(); }
			}

			public void OnCompleted()
			{
				if (++count == total)
				{
					try
					{
						progress.Report(1f);
						observer.OnNext(Unit.Default);
						observer.OnCompleted();
					}
					finally { Dispose(); }
				}
			}

			public void Dispose()
			{
				cancel.Dispose();
			}
		}
	}
}
