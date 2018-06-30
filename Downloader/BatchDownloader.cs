namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine.Networking;

	public class BatchDownloader
	{
		readonly List<Item> list = new List<Item>();

		public IReadOnlyList<Item> List => list;

		public int DownloadedCount { get; private set; }
		public int KnownSizeCount { get; private set; }
		public ulong TotalKnownSize { get; private set; }

		public TaskQueue FetchHeadTask()
		{
			var queue = new TaskQueue();
			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				if (item.IsHeadFetched) continue;
				queue.Add(progress => item.Downloader.FetchHead(progress)
					.Do(req => OnHeadFetched(item, req)).AsUnitObservable());
			}
			return queue;
		}

		void OnHeadFetched(Item item, UnityWebRequest request)
		{
			if (item.IsHeadFetched) return;
			item.IsHeadFetched = true;
			if (request.responseCode == 304)
			{
				OnDownloaded(item);
				return;
			}
			item.Size = request.GetContentLengthNum();
			if (item.Size >= 0)
			{
				KnownSizeCount++;
				TotalKnownSize += (ulong)item.Size.Value;
			}
		}

		void OnDownloaded(Item item)
		{
			if (item.IsDownloaded) return;
			item.IsDownloaded = true;
			DownloadedCount++;
		}

		public TaskQueue DownloadTask()
		{
			var queue = new TaskQueue();
			if (list.Count <= DownloadedCount) return queue;
			var averageSize = KnownSizeCount > 0 ? (float)TotalKnownSize / KnownSizeCount : (float?)null;
			for (int i = 0; i < list.Count; i++)
			{
				var item = list[i];
				if (item.IsDownloaded) continue;
				TaskFunc task = progress => item.Downloader.Download(progress).Do(req =>
				{
					OnHeadFetched(item, req);
					OnDownloaded(item);
				}).AsUnitObservable();
				queue.Add(task, item.Size >= 0 ? Math.Max(item.Size.Value, 1) : averageSize);
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
			public bool IsDownloaded;
			public long? Size;

			public static Item FromDownloader(IDownloader downloader) => new Item { Downloader = downloader };
		}
	}
}
