namespace J
{
	using System;
	using UnityEngine.Networking;

	public interface IDownloader
	{
		string Url { get; }
		string ETag { get; }
		IObservable<UnityWebRequest> Download(IProgress<float> progress = null);
	}

	public static partial class ExtensionMethods
	{
		public static IObservable<UnityWebRequest> Head(this IDownloader downloader, IProgress<float> progress = null)
		{
			var request = UnityWebRequest.Head(downloader.Url);
			var options = downloader as IUnityWebRequestSendOptions;
			return options != null
				? request.SendAsObservable(options, progress)
				: request.SendAsObservable(downloader.ETag, progress);
		}
	}
}
