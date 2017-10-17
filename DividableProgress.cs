namespace J
{
	using System;
	using UniRx;

	public class DividableProgress : IProgress<float>, ISubject<float>, IDisposable
	{
		readonly Subject<float> subject = new Subject<float>();

		CompositeDisposable cancel;
		float value;

		public float Value => value;

		public void Report(float v)
		{
			value = v;
			subject.OnNext(value);
		}

		public void ReportDelta(float d) => Report(value + d);

		void IObserver<float>.OnNext(float v) => Report(v);

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
			float last = sub.value;
			sub.Subscribe(current =>
			{
				float v = value + (current - last) * weight;
				last = current;
				Report(v);
			}).AddTo(cancel);
			return sub;
		}
	}
}
