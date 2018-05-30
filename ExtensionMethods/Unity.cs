namespace UnityEngine
{
	using System;
	using UniRx;
	using UnityEngine.Networking;

	public static partial class ExtensionMethods
	{
		public static IObservable<UnityWebRequest> SendAsObservable(this UnityWebRequest request, IProgress<float> progress = null, bool autoDispose = true)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			return Observable.Defer(() =>
			{
				var stream = request.SendWebRequest()
					.AsAsyncOperationObservable(progress)
					.Select(op => op.webRequest);
				if (autoDispose) stream = stream.Finally(request.Dispose);
				return stream;
			});
		}

		public static IObservable<AssetBundle> ToAssetBundle(this IObservable<UnityWebRequest> source, bool throwError = true)
		{
			return source.Select(request =>
			{
				try
				{
					if (request.isNetworkError) throw new Exception(string.Format("{0} {1}", request.error, request.url));
					if (request.isHttpError) throw new Exception(string.Format("HTTP{0} {1}", request.responseCode, request.url));
					var bundle = DownloadHandlerAssetBundle.GetContent(request);
					if (bundle == null) throw new Exception("Invalid AssetBundle. " + request.url);
					return bundle;
				}
				catch
				{
					if (throwError) throw;
					return null;
				}
			});
		}
	}
}
