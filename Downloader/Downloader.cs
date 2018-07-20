namespace J
{
	using System;
	using UniRx;
	using UnityEngine.Networking;

	public abstract class Downloader
	{
		protected static readonly IObservable<UnityWebRequest> ReturnNull = Observable.Return<UnityWebRequest>(null);

		public bool IsHeadFetched { get; protected set; }
		public bool IsDownloaded { get; protected set; }
		public long? Size { get; protected set; }

		public abstract IObservable<UnityWebRequest> FetchHead(IProgress<float> progress = null);
		public abstract IObservable<UnityWebRequest> Download(IProgress<float> progress = null);
	}
}
