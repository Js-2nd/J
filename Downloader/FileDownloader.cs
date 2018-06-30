﻿namespace J
{
	using System;
	using System.IO;
	using UniRx;
	using UnityEngine.Networking;

	public interface IFileDownloader : IDownloader
	{
		string SavePath { get; }
		string TempPath { get; }
		Action BeforeSave { get; }
	}

	public class FileDownloader : FileDownloader<FileDownloader>
	{
		public static FileDownloader Create(string url, string savePath) =>
			new FileDownloader().SetUrl(url).SetSavePath(savePath);
	}

	public class FileDownloader<T> : UnityWebRequestSendOptions<T>,
		IFileDownloader where T : FileDownloader<T>
	{
		public string Url { get; set; }
		public string SavePath { get; set; }
		public string TempPath { get; set; }
		public Action BeforeSave { get; set; }

		public T SetUrl(string url)
		{
			Url = url;
			return (T)this;
		}
		public T SetSavePath(string savePath)
		{
			SavePath = savePath;
			return (T)this;
		}
		public T SetTempPath(string tempPath)
		{
			TempPath = tempPath;
			return (T)this;
		}
		public T SetBeforeSave(Action beforeSave)
		{
			BeforeSave = beforeSave;
			return (T)this;
		}

		public IObservable<UnityWebRequest> Download(IProgress<float> progress = null)
		{
			string tempPath = TempPath ?? SavePath + ".tmp";
			var request = new UnityWebRequest(Url);
			var handler = new DownloadHandlerFile(tempPath);
			request.downloadHandler = handler;
			return request.SendAsObservable(this, progress)
				.Do(req =>
				{
					handler.Dispose();
					if (req.responseCode == 200)
					{
						BeforeSave?.Invoke();
						File.Delete(SavePath);
						File.Move(tempPath, SavePath);
					}
					else
					{
						File.Delete(tempPath);
					}
				});
		}
	}
}