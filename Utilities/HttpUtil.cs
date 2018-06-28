namespace J
{
	using J.Internal;
	using System;
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

		public static IObservable<UnityWebRequest> DownloadFile(string url, string path, bool removeFileOnAbort = false)
		{
			var handler = new DownloadHandlerFile(path) { removeFileOnAbort = removeFileOnAbort };
			var req = new UnityWebRequest(url) { downloadHandler = handler };
			return req.SendAsObservable();
		}
	}
}
