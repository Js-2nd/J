namespace J
{
	using System;
	using UnityEngine.Networking;

	public interface IDownloader
	{
		string Url { get; }
		string ETag { get; }
		string LastModified { get; }
		IObservable<UnityWebRequest> Download(IProgress<float> progress = null);
	}

	public static partial class ExtensionMethods
	{
		public static IObservable<UnityWebRequest> FetchHead(this IDownloader downloader,
			IProgress<float> progress = null)
		{
			var request = UnityWebRequest.Head(downloader.Url);
			var options = downloader as IUnityWebRequestSendOptions;
			if (options == null)
				options = new UnityWebRequestSendOptions()
					.SetETag(downloader.ETag)
					.SetLastModified(downloader.LastModified);
			return request.SendAsObservable(options, progress);
		}
	}
}
