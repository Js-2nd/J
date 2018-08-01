namespace J
{
	using System;
	using UniRx;

	public class DividableProgress : IProgress<float>, IObservable<float>, IDisposable
	{
		readonly ReactiveProperty<float> inner = new ReactiveProperty<float>();

		CompositeDisposable cancel;

		public float Value => inner.Value;

		public void Report(float value) => inner.Value = value;

		public void ReportDelta(float delta) => inner.Value += delta;

		public IDisposable Subscribe(IObserver<float> observer) => inner.Subscribe(observer);

		public void Dispose()
		{
			cancel?.Dispose();
			inner.Dispose();
		}

		public DividableProgress Divide(float weight)
		{
			if (cancel == null) cancel = new CompositeDisposable();
			var divide = new DividableProgress();
			float last = 0;
			divide.Subscribe(current =>
			{
				float delta = current - last;
				last = current;
				inner.Value += delta * weight;
			}).AddTo(cancel);
			return divide;
		}

		public DividableProgress DivideRest(float weight = 1) => Divide((1 - inner.Value) * weight);
	}

	public static partial class ExtensionMethods
	{
		public static DividableProgress ToDividableProgress(this IProgress<float> progress)
		{
			var dividableProgress = progress as DividableProgress;
			if (dividableProgress == null && progress != null)
			{
				dividableProgress = new DividableProgress();
				dividableProgress.Subscribe(progress.Report);
			}
			return dividableProgress;
		}
	}
}
