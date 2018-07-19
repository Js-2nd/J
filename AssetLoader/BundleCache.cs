namespace J
{
	using System;
	using UniRx;
	using UnityEngine;

	public class BundleCache : IObserver<AssetBundle>, IDisposable
	{
		public string Name { get; }

		public int RefCount { get; private set; }

		readonly AsyncSubject<AssetBundle> subject = new AsyncSubject<AssetBundle>();

		public BundleCache(string name)
		{
			Name = name;
		}

		void IObserver<AssetBundle>.OnNext(AssetBundle value) => subject.OnNext(value);

		void IObserver<AssetBundle>.OnError(Exception error) => subject.OnError(error);

		void IObserver<AssetBundle>.OnCompleted() => subject.OnCompleted();

		public void Dispose() => subject.Dispose();

		public IObservable<BundleReference> GetReference() => Observable.Defer(() =>
		{
			RefCount++;
			var cancel = Disposable.Create(() => RefCount--);
			return subject.Select(bundle => new BundleReference(bundle, cancel))
				.DoOnError(_ => cancel.Dispose())
				.DoOnCancel(() => cancel.Dispose());
		});
	}

	public class BundleReference : IDisposable
	{
		public AssetBundle Bundle { get; }

		readonly IDisposable cancel;

		public BundleReference(AssetBundle bundle, IDisposable cancel)
		{
			Bundle = bundle;
			this.cancel = cancel;
		}

		public void Dispose() => cancel?.Dispose();
	}
}
