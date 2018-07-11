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
			IUnityWebRequestSendOptions options = null, IProgress<float> progress = null)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			if (options == null) options = new UnityWebRequestSendOptions();
			return Observable.Defer(() =>
			{
				if (options.ETag != null) request.SetIfNoneMatch(options.ETag);
				if (options.LastModified != null) request.SetIfModifiedSince(options.LastModified);
				var stream = request.SendWebRequest()
					.AsAsyncOperationObservable(progress)
					.Select(op =>
					{
						var req = op.webRequest;
						if (req.responseCode != 304)
						{
							if (req.isNetworkError && options.ThrowNetworkError) throw new NetworkException(req);
							if (req.isHttpError && options.ThrowHttpError) throw new HttpException(req);
						}
						return req;
					});
				if (options.AutoDispose) stream = stream.Finally(request.Dispose);
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
