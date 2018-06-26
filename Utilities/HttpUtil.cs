namespace J
{
	using J.Internal;
	using System;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public static class HttpUtil
	{
		public static IObservable<ulong?> GetContentLength(string uri, IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				return UnityWebRequest.Head(uri).SendAsObservable(progress).Select(req =>
				{
					ulong length;
					if (ulong.TryParse(req.GetResponseHeader(HttpHeader.ContentLength), out length))
						return length;
					return (ulong?)null;
				});
			});
		}
	}
}
