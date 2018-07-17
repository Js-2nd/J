namespace J
{
	using System;
	using UniRx;
	using UnityEngine;

	public class BundleCache : IObserver<AssetBundle>, IDisposable
	{
		public int RefCount { get; private set; }

		readonly AsyncSubject<AssetBundle> subject = new AsyncSubject<AssetBundle>();

		public void OnNext(AssetBundle value) => subject.OnNext(value);

		public void OnError(Exception error) => subject.OnError(error);

		public void OnCompleted() => subject.OnCompleted();

		public void Dispose() => subject.Dispose();

		public BundleReference CreateReference()
		{
			RefCount++;
			return new BundleReference(subject, Disposable.Create(OnDisposeReference));
		}

		void OnDisposeReference() => RefCount--;
	}

	public class BundleReference : IDisposable
	{
		readonly AssetBundle bundle;
		//readonly IObservable<AssetBundle> observable;
		readonly IDisposable disposable;

		public BundleReference(AssetBundle bundle, IDisposable disposable)
		{
			this.bundle = bundle;
			//this.observable = observable;
			this.disposable = disposable;
		}

		//public IDisposable Subscribe(IObserver<AssetBundle> observer) => observable.Subscribe(observer);

		public AssetBundle Bundle => bundle;

		public void Dispose() => disposable.Dispose();
	}
}
