namespace J
{
	using J.Internal;
	using System;
	using System.IO;
	using UniRx;
	using UnityEngine.Networking;

	public sealed class FileDownloader : Downloader
	{
		public string Url { get; set; }
		public string ETag { get; set; }
		public string LastModified { get; set; }

		public string SavePath { get; set; }
		public string TempPath { get; set; }

		public Action<CallbackParam> BeforeSave { get; set; }
		public Action<CallbackParam> AfterSave { get; set; }

		public override IObservable<UnityWebRequest> FetchHead(IProgress<float> progress = null)
		{
			return Observable.Defer(() =>
			{
				if (IsHeadFetched) return ReturnNull.ReportOnCompleted(progress);
				return UnityWebRequest.Head(Url).SendAsObservable(progress, ETag, LastModified).Do(req =>
				{
					IsHeadFetched = true;
					if (req.responseCode == 304) IsDownloaded = true;
					Size = req.GetContentLengthNum();
				});
			});
		}

		public override IObservable<UnityWebRequest> Download(IProgress<float> progress = null) =>
			Observable.Defer(() => IsHeadFetched ? ReturnNull : FetchHead()).ContinueWith(_ =>
				IsDownloaded ? ReturnNull.ReportOnCompleted(progress) : DoDownload(progress));

		IObservable<UnityWebRequest> DoDownload(IProgress<float> progress) => Observable.Defer(() =>
		{
			string tempPath = TempPath ?? SavePath + ".tmp";
			var request = new UnityWebRequest(Url);
			var handler = new DownloadHandlerFile(tempPath);
			request.downloadHandler = handler;
			return request.SendAsObservable(progress).Do(req =>
			{
				req.downloadHandler.Dispose();
				BeforeSave?.Invoke(new CallbackParam(this, req));
				File.Delete(SavePath);
				File.Move(tempPath, SavePath);
				AfterSave?.Invoke(new CallbackParam(this, req));
				IsDownloaded = true;
			});
		});

		public struct CallbackParam
		{
			public readonly FileDownloader Downloader;
			public readonly UnityWebRequest Request;

			public CallbackParam(FileDownloader downloader, UnityWebRequest request)
			{
				Downloader = downloader;
				Request = request;
			}
		}
	}
}
