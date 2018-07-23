namespace J
{
	using System;
	using UniRx;
	using UnityEngine.Networking;

	public abstract class Downloader
	{
		protected static readonly IObservable<UnityWebRequest> ReturnNull = Observable.Return<UnityWebRequest>(null);

		public bool IsHeadFetched { get; private set; }
		public bool IsDownloaded { get; private set; }
		public long? Size { get; private set; }

		public event Action<Downloader> HeadFetched;
		public event Action<Downloader> Downloaded;

		public abstract IObservable<UnityWebRequest> FetchHead(IProgress<float> progress = null);
		public abstract IObservable<UnityWebRequest> Download(IProgress<float> progress = null);

		protected void OnHeadFetched(UnityWebRequest request)
		{
			if (IsHeadFetched) return;
			IsHeadFetched = true;
			if (request != null) Size = request.GetContentLengthNum();
			HeadFetched?.Invoke(this);
			if (request != null && request.responseCode == 304) OnDownloaded(request);
		}

		protected void OnDownloaded(UnityWebRequest request)
		{
			if (IsDownloaded) return;
			IsDownloaded = true;
			OnHeadFetched(request);
			Downloaded?.Invoke(this);
		}
	}
}
