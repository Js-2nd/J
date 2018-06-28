namespace J
{
	using J.Internal;
	using System;
	using System.IO;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public static class HttpUtil
	{
		public static IObservable<ulong?> GetContentLength(string url, IProgress<float> progress = null) =>
			GetContentLength(url, null, progress);
		public static IObservable<ulong?> GetContentLength(string url, string eTag, IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				return UnityWebRequest.Head(url).SendAsObservable(eTag, progress).Select(r =>
				{
					ulong length;
					if (ulong.TryParse(r.GetResponseHeader(HttpHeader.ContentLength), out length))
						return length;
					return (ulong?)null;
				});
			});
		}

		public static IObservable<UnityWebRequest> DownloadFile(string url, string path, bool removeOnError = false)
		{
			var req = new UnityWebRequest(url);
			var handler = new DownloadHandlerFile(path);
			req.downloadHandler = handler;
			handler.removeFileOnAbort = removeOnError;
			return req.SendAsObservable();
		}
		public static IObservable<UnityWebRequest> DownloadFile(string url, string eTag, string path,
			bool removeOnError = false)
		{
			var req = new UnityWebRequest(url);
			var handler = new DownloadHandlerFile(path);
			req.downloadHandler = handler;
			var stream = req.SendAsObservable(eTag);
			if (removeOnError) stream = stream.DoOnError(_ => // TODO will this work?
			{
				handler.Dispose();
				File.Delete(path);
			});
			return stream;
		}
	}
}
