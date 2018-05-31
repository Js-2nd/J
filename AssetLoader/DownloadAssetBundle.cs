namespace J
{
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
				var stream = bundleNames.Select(GetActualBundleName);
				if (includeDependencies)
					stream = stream.SelectMany(bundleName => bundleName.ToSingleEnumerable().Concat(Manifest.GetAllDependencies(bundleName)));
				var list = stream.Distinct()
					.Select(bundleName => new BundleInfo
					{
						RootUri = RootUri,
						ActualName = bundleName,
						Hash = Manifest.GetAssetBundleHash(bundleName),
					}).Where(info => !Caching.IsVersionCached(info.ActualName, info.Hash))
					.ToArray();
				return new BundleDownloader { List = list };
			});
		}
	}

	public sealed class BundleDownloader
	{
		public BundleInfo[] List;
		public ulong TotalSize;

		public IObservable<BundleDownloader> FetchSize(IProgress<float> progress = null, int maxConcurrent = 4)
		{
			var queue = new TaskQueue();
			for (int i = 0; i < List.Length; i++)
				queue.Add(List[i].FetchSize);
			return queue.ToObservable(progress, maxConcurrent).Select(_ =>
			{
				TotalSize = 0;
				for (int i = 0; i < List.Length; i++)
					TotalSize += List[i].Size;
				return this;
			});
		}

		public IObservable<Unit> Download(IProgress<float> progress = null, int maxConcurrent = 4)
		{
			var queue = new TaskQueue();
			for (int i = 0; i < List.Length; i++)
			{
				var info = List[i];
				queue.Add(info.Download, info.Weight);
			}
			return queue.ToObservable(progress, maxConcurrent);
		}
	}

	public sealed class BundleInfo
	{
		public string RootUri;
		public string ActualName;
		public Hash128 Hash;
		public ulong Size;

		public IObservable<Unit> FetchSize(IProgress<float> progress = null)
		{
			return UnityWebRequest.Head(RootUri + ActualName)
				.SendAsObservable(progress)
				.Do(req => ulong.TryParse(req.GetResponseHeader("Content-Length"), out Size))
				.AsUnitObservable();
		}

		public IObservable<Unit> Download(IProgress<float> progress = null)
		{
			return UnityWebRequest.GetAssetBundle(RootUri + ActualName, Hash, 0)
				.SendAsObservable(progress)
				.AsUnitObservable();
		}

		public float Weight() => Mathf.Max(Size, 1);
	}

	public static partial class AssetLoader
	{
		public static IObservable<BundleDownloader> Download(IEnumerable<string> bundleNames,
			bool includeDependencies = true) => Instance.Download(bundleNames, includeDependencies);
	}
}
