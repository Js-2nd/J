namespace J
{
	using System;
	using System.IO;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public static class HttpUtil
	{
		public static IObservable<UnityWebRequest> DownloadFile(string url, string savePath, string tempPath = null,
			Action beforeSave = null, string eTag = null, IProgress<float> progress = null,
			bool throwNetworkError = true, bool throwHttpError = true, bool autoDispose = true)
		{
			if (tempPath == null) tempPath = savePath + ".tmp";
			var request = new UnityWebRequest(url);
			var handler = new DownloadHandlerFile(tempPath);
			request.downloadHandler = handler;
			return request.SendAsObservable(eTag, progress, throwNetworkError, throwHttpError, autoDispose)
				.Do(req =>
				{
					handler.Dispose();
					if (req.responseCode == 200)
					{
						beforeSave?.Invoke();
						File.Delete(savePath);
						File.Move(tempPath, savePath);
					}
					else
					{
						File.Delete(tempPath);
					}
				});
		}
	}
}
