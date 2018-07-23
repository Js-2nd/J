namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;

	public class BatchDownloader
	{
		readonly List<Downloader> list = new List<Downloader>();

		public int Count => list.Count;

		public int FetchedCount { get; private set; }
		public ulong FetchedSize { get; private set; }

		public int PendingCount { get; private set; }
		public ulong PendingSize { get; private set; }

		public void Add(Downloader downloader)
		{
			if (downloader == null) return;
			downloader.HeadFetched += OnHeadFetched;
			downloader.Downloaded += OnDownloaded;
			list.Add(downloader);
			PendingCount++;
		}

		void OnHeadFetched(Downloader downloader)
		{
			FetchedCount++;
			if (downloader.Size > 0)
			{
				ulong size = (ulong)downloader.Size.Value;
				FetchedSize += size;
				PendingSize += size;
			}
		}

		void OnDownloaded(Downloader downloader)
		{
			PendingCount--;
			if (downloader.Size > 0) PendingSize -= (ulong)downloader.Size.Value;
		}

		public TaskQueue FetchHeadTask()
		{
			var queue = new TaskQueue();
			for (int i = 0; i < list.Count; i++)
			{
				var downloader = list[i];
				if (!downloader.IsHeadFetched)
					queue.Add(progress => downloader.FetchHead(progress).AsUnitObservable());
			}
			return queue;
		}

		public TaskQueue DownloadTask()
		{
			var queue = new TaskQueue();
			var averageSize = FetchedCount > 0 ? (float)FetchedSize / FetchedCount : (float?)null;
			for (int i = 0; i < list.Count; i++)
			{
				var downloader = list[i];
				if (!downloader.IsDownloaded)
					queue.Add(progress => downloader.Download(progress).AsUnitObservable(),
						downloader.Size >= 0 ? Math.Max(downloader.Size.Value, 1) : averageSize);
			}
			return queue;
		}
	}
}
