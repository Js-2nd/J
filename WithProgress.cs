namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;

	public static partial class ExtensionMethods
	{
		public static IObservable<Unit> WithProgress(this IObservable<Unit> source, IProgress<float> progress)
		{
			return new WithProgress(source, progress);
		}
	}

	class WithProgress : IObservable<Unit>
	{
		readonly IObservable<Unit> source;
		readonly IProgress<float> progress;

		public WithProgress(IObservable<Unit> source, IProgress<float> progress)
		{
			this.source = source;
			this.progress = progress;
		}

		public IDisposable Subscribe(IObserver<Unit> observer)
		{
			var whenAll = source as WhenAllOnCompleted;
			if (whenAll != null)
			{
				progress.Report(0f);
				return whenAll.SubscribeWithProgress(observer, progress);
			}

			return source
				.DoOnSubscribe(() => progress.Report(0f))
				.DoOnCompleted(() => progress.Report(1f))
				.Subscribe(observer);
		}
	}

	partial class WhenAllOnCompleted
	{
		public IDisposable SubscribeWithProgress(IObserver<Unit> observer, IProgress<float> progress)
		{
			var list = new List<IObservable<Unit>>(sources);
			if (list.Count == 0)
			{
				progress.Report(1f);
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
				return Disposable.Empty;
			}

			var divide = progress as DividableProgress;
			if (divide == null)
			{
				divide = new DividableProgress();
				divide.Subscribe(v => progress.Report(v));
			}

			float weight = 1f / list.Count;
			int count = 0;

			Action<Unit> onNext = Stubs<Unit>.Ignore;
			Action<Exception> onError = ex =>
			{
				try { observer.OnError(ex); }
				finally { divide.Dispose(); }
			};
			Action onCompleted = () =>
			{
				if (++count == list.Count)
				{
					try
					{
						observer.OnNext(Unit.Default);
						observer.OnCompleted();
					}
					finally { divide.Dispose(); }
				}
			};

			var cancel = new CompositeDisposable();
			for (int i = 0; i < list.Count; i++)
			{
				var source = list[i];
				var whenAll = source as WhenAllOnCompleted;
				if (whenAll != null)
				{
					whenAll.SubscribeWithProgress(Observer.Create(onNext, onError, onCompleted), divide.Divide(weight)).AddTo(cancel);
				}
				else
				{
					source.Subscribe(onNext, onError, () =>
					{
						divide.ReportDelta(weight);
						onCompleted();
					}).AddTo(cancel);
				}
			}
			return cancel;
		}
	}
}
