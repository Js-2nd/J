namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;

	public class BatchDownloader
	{
		readonly List<Item> list = new List<Item>();

		public int ItemCount => list.Count;
		public int DownloadCount => ItemCount - CachedCount;
		public int CachedCount { get; private set; }
		public int KnownSizeCount { get; private set; }
		public ulong TotalKnownSize { get; private set; }

		public double PredictDownloadSize
		{
			get
			{
				int downloadCount = DownloadCount;
				if (KnownSizeCount == downloadCount) return TotalKnownSize;
				if (KnownSizeCount == 0) return downloadCount;
				return (double)TotalKnownSize * downloadCount / KnownSizeCount;
			}
		}

		public TaskQueue FetchHeadTask()
		{
			var queue = new TaskQueue();
			for (int i = 0, iCount = ItemCount; i < iCount; i++)
			{
				var item = list[i];
				if (item.IsHeadFetched) continue;
				queue.Add(progress => item.Downloader.Head(progress).Do(req =>
				{
					// check IsHeadFetched again in case of parallel running
					if (item.IsHeadFetched) return;
					item.IsHeadFetched = true;
					if (req.responseCode == 304)
					{
						item.SkipDownload = true;
						CachedCount++;
						return;
					}
					var length = req.GetContentLengthNum();
					if (length >= 0)
					{
						item.Weight = Math.Max(length.Value, 1);
						KnownSizeCount++;
						TotalKnownSize += (ulong)length.Value;
					}
				}).AsUnitObservable());
			}
			return queue;
		}

		public TaskQueue DownloadTask()
		{
			var queue = new TaskQueue();
			int downloadCount = DownloadCount;
			if (downloadCount <= 0) return queue;
			float unknownSizeWeight = (float)Math.Max(PredictDownloadSize / downloadCount, 1);
			for (int i = 0, iCount = ItemCount; i < iCount; i++)
			{
				var item = list[i];
				if (item.SkipDownload) continue;
				queue.Add(progress => item.Downloader.Download(progress).AsUnitObservable(),
					item.Weight ?? unknownSizeWeight);
			}
			return queue;
		}

		public void Add(IDownloader downloader) => list.Add(Item.FromDownloader(downloader));

		public void AddRange(IEnumerable<IDownloader> downloaders) =>
			list.AddRange(downloaders.Select(Item.FromDownloader));

		public class Item
		{
			public IDownloader Downloader;
			public bool IsHeadFetched;
			public bool SkipDownload;
			public float? Weight;

			public static Item FromDownloader(IDownloader downloader) => new Item { Downloader = downloader };
		}
	}
}
