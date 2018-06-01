namespace UnityEngine
{
	using J;
	using System;
	using UniRx;
	using UnityEngine.Networking;

	public static partial class ExtensionMethods
	{
		public static IObservable<UnityWebRequest> SendAsObservable(this UnityWebRequest request,
			IProgress<float> progress = null, bool throwNetworkError = true, bool throwHttpError = true,
			bool autoDispose = true)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			return Observable.Defer(() =>
			{
				var stream = request.SendWebRequest()
					.AsAsyncOperationObservable(progress)
					.Select(op =>
					{
						var req = op.webRequest;
						if (req.isNetworkError && throwNetworkError) throw new NetworkException(req);
						if (req.isHttpError && throwHttpError) throw new HttpException(req);
						return req;
					});
				if (autoDispose) stream = stream.Finally(request.Dispose);
				return stream;
			});
		}

		public static IObservable<AssetBundle> ToAssetBundle(this IObservable<UnityWebRequest> source,
			bool throwError = true)
		{
			return source.Select(request =>
			{
				try
				{
					var bundle = DownloadHandlerAssetBundle.GetContent(request);
					if (bundle == null)
						throw new InvalidOperationException("Invalid AssetBundle. " + request.url);
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
