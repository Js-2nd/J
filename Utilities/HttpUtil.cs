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
				var req = UnityWebRequest.Head(url);
				if (eTag != null) req.SetRequestHeader(HttpHeader.IfNoneMatch, eTag);
				return req.SendAsObservable(progress, false, false).Select(r =>
				{
					if (r.responseCode != 304) r.TryThrowError();
					ulong length;
					if (ulong.TryParse(r.GetResponseHeader(HttpHeader.ContentLength), out length))
						return length;
					return (ulong?)null;
				});
			});
		}

		public static IObservable<UnityWebRequest> DownloadToPersist(string url, string path)
		{
			string fullPath = Path.Combine(Application.persistentDataPath, path);
			var req = new UnityWebRequest(url) { downloadHandler = new DownloadHandlerFile(fullPath) };
			return req.SendAsObservable();
		}
	}
}
