#if !UNITY_2018_1_OR_NEWER
using UnityWebRequestAssetBundle = UnityEngine.Networking.UnityWebRequest;
#endif

namespace J
{
	using J.Internal;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	partial class AssetLoaderInstance
	{
		public IObservable<BundleDownloader> Download(IEnumerable<string> bundleNames, bool includeDependencies = true)
		{
			return WaitForManifestLoaded().Select(_ =>
			{
				var stream = bundleNames.Select(NormBundleName).Select(GetActualBundleName);
				if (includeDependencies)
					stream = stream.SelectMany(bundleName =>
						bundleName.ToSingleEnumerable().Concat(Manifest.GetAllDependencies(bundleName)));
				var downloader = new BundleDownloader { RootUri = RootUri };
				downloader.List = stream.Distinct()
					.Select(bundleName => new BundleInfo
					{
						Downloader = downloader,
						ActualName = bundleName,
						Hash = Manifest.GetAssetBundleHash(bundleName),
					}).Where(info => !Caching.IsVersionCached(info.ActualName, info.Hash))
					.ToArray();
				return downloader;
			});
		}
	}

	public sealed class BundleDownloader
	{
		public string RootUri;
		public IReadOnlyList<BundleInfo> List;
		public int FetchedCount;
		public ulong FetchedTotalSize;

		public TaskQueue FetchSizeTask()
		{
			var queue = new TaskQueue();
			for (int i = 0; i < List.Count; i++)
			{
				var info = List[i];
				if (info.Size == 0) queue.Add(info.FetchSize);
			}
			return queue;
		}

		public TaskQueue DownloadTask()
		{
			TaskQueue fetchQueue = null;
			TaskQueue nonFetchQueue = null;
			int fetched = 0;
			ulong fetchedSize = 0;
			for (int i = 0; i < List.Count; i++)
			{
				var info = List[i];
				if (info.Size > 0)
				{
					if (fetchQueue == null) fetchQueue = new TaskQueue();
					fetchQueue.Add(info.Download, info.Size);
					fetched++;
					fetchedSize += info.Size;
				}
				else
				{
					if (nonFetchQueue == null) nonFetchQueue = new TaskQueue();
					nonFetchQueue.Add(info.Download);
				}
			}
			var queue = new TaskQueue();
			if (fetchQueue != null) queue.AddTaskQueue(fetchQueue, (float)fetched / fetchedSize);
			if (nonFetchQueue != null) queue.AddTaskQueue(nonFetchQueue);
			return queue;
		}
	}

	public sealed class BundleInfo
	{
		public BundleDownloader Downloader;
		public string ActualName;
		public Hash128 Hash;
		public ulong Size;

		public IObservable<Unit> FetchSize(IProgress<float> progress = null)
		{
			return UnityWebRequest.Head(Downloader.RootUri + ActualName)
				.SendAsObservable(progress)
				.Do(req =>
				{
					if (ulong.TryParse(req.GetResponseHeader(HttpHeader.ContentLength), out Size))
					{
						Downloader.FetchedCount++;
						Downloader.FetchedTotalSize += Size;
					}
				})
				.AsUnitObservable();
		}

		public IObservable<Unit> Download(IProgress<float> progress = null)
		{
			return UnityWebRequestAssetBundle.GetAssetBundle(Downloader.RootUri + ActualName, Hash, 0)
				.SendAsObservable(progress)
				.AsUnitObservable();
		}
	}

	public static partial class AssetLoader
	{
		public static IObservable<BundleDownloader> Download(IEnumerable<string> bundleNames,
			bool includeDependencies = true) => Instance.Download(bundleNames, includeDependencies);
	}
}
