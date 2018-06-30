namespace J
{
	using System;
	using System.IO;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public static partial class ExtensionMethods
	{
		public static IObservable<UnityWebRequest> SendAsObservable(this UnityWebRequest request,
			IUnityWebRequestSendOptions options, IProgress<float> progress = null) => request.SendAsObservable(
			options.ETag, progress, options.ThrowNetworkError, options.ThrowHttpError, options.AutoDispose);
		public static IObservable<UnityWebRequest> SendAsObservable(this UnityWebRequest request,
			IProgress<float> progress = null, bool throwNetworkError = true, bool throwHttpError = true,
			bool autoDispose = true) =>
			request.SendAsObservable(null, progress, throwNetworkError, throwHttpError, autoDispose);
		public static IObservable<UnityWebRequest> SendAsObservable(this UnityWebRequest request, string eTag,
			IProgress<float> progress = null, bool throwNetworkError = true, bool throwHttpError = true,
			bool autoDispose = true)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			return Observable.Defer(() =>
			{
				if (eTag != null) request.SetIfNoneMatch(eTag);
				var stream = request.SendWebRequest()
					.AsAsyncOperationObservable(progress)
					.Select(op =>
					{
						var req = op.webRequest;
						if (eTag == null || req.responseCode != 304)
						{
							if (req.isNetworkError && throwNetworkError) throw new NetworkException(req);
							if (req.isHttpError && throwHttpError) throw new HttpException(req);
						}
						return req;
					});
				if (autoDispose) stream = stream.Finally(request.Dispose);
				return stream;
			});
		}

		public static IObservable<AssetBundle> LoadAssetBundle(this IObservable<UnityWebRequest> source,
			bool throwError = true)
		{
			return source.Select(request =>
			{
				try
				{
					var bundle = DownloadHandlerAssetBundle.GetContent(request);
					if (bundle == null)
						throw new InvalidDataException("Invalid AssetBundle. " + request.url);
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
