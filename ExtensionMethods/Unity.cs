namespace UnityEngine
{
	using System;
	using UniRx;
	using UnityEngine.Networking;

	public static partial class ExtensionMethods
	{
		public static IObservable<AssetBundle> ToAssetBundleObservable(this UnityWebRequest request, IProgress<float> progress = null)
		{
			return Observable.Defer(() => request.SendWebRequest().AsObservable(progress).Select(_ =>
			{
				if (request.isNetworkError)
					throw new Exception(string.Format("{0} {1}", request.error, request.url));
				if (request.isHttpError)
					throw new Exception(string.Format("HTTP{0} {1}", request.responseCode, request.url));
				var ab = DownloadHandlerAssetBundle.GetContent(request);
				if (ab == null)
					throw new Exception("Invalid AssetBundle. " + request.url);
				return ab;
			}).Finally(() => request.Dispose()));
		}
	}
}
