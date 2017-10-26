namespace J
{
	using System;
	using UniRx;

	public class DividableProgress : IProgress<float>, ISubject<float>, IDisposable
	{
		readonly Subject<float> subject = new Subject<float>();

		CompositeDisposable cancel;

		public float Value { get; protected set; }

		public void Report(float value)
		{
			Value = value;
			subject.OnNext(Value);
		}

		public void ReportDelta(float delta) => Report(Value + delta);

		void IObserver<float>.OnNext(float value) => Report(value);

		void IObserver<float>.OnError(Exception error) => subject.OnError(error);

		void IObserver<float>.OnCompleted() => subject.OnCompleted();

		public IDisposable Subscribe(IObserver<float> observer) => subject.Subscribe(observer);

		public void Dispose()
		{
			cancel?.Dispose();
			subject.Dispose();
		}

		public DividableProgress Divide(float weight)
		{
			if (cancel == null) cancel = new CompositeDisposable();
			var sub = new DividableProgress();
			float last = sub.Value;
			sub.Subscribe(current =>
			{
				float newValue = Value + (current - last) * weight;
				last = current;
				Report(newValue);
			}).AddTo(cancel);
			return sub;
		}
	}
}
