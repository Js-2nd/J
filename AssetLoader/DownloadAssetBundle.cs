using UnityEngine.Networking;

namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;

	partial class AssetLoaderInstance
	{
		public IObservable<BatchDownloader> Download(IEnumerable<string> bundleNames, bool includeDependencies = true)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				var stream = bundleNames.Select(GetActualBundleName);
				if (includeDependencies)
					stream = stream.SelectMany(bundleName => bundleName.ToSingleEnumerable().Concat(Manifest.GetAllDependencies(bundleName)));
				var list = stream.Distinct()
					.Select(bundleName => new KeyValuePair<string, Hash128>(bundleName, Manifest.GetAssetBundleHash(bundleName)))
					.Where(item => !Caching.IsVersionCached(item.Key, item.Value)).ToArray();
				var downloader = new BatchDownloader
				{
					RootUri = RootUri,
					List = list,
				};
				return Observable.Return(downloader);
			});
		}

		//public IObservable<Unit> Download(IEnumerable<string> bundleNames, IProgress<float> progress = null, bool includeDependencies = true)
		//{
		//	return WaitForManifestLoaded().ContinueWith(_ =>
		//	{
		//		var set = new HashSet<string>();
		//		foreach (var item in bundleNames)
		//		{
		//			var bundleName = new BundleEntry(item).NormBundleName;
		//			set.Add(bundleName);
		//			if (!includeDependencies) continue;
		//			var dep = Manifest.GetAllDependencies(bundleName);
		//			for (int i = 0; i < dep.Length; i++)
		//				set.Add(dep[i]);
		//		}
		//		return set.Where(bundleName => !Caching.IsVersionCached(bundleName, Manifest.GetAssetBundleHash(bundleName)))
		//			.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)))
		//			.ToTaskQueue().ToObservable(progress);
		//	});
		//}

		//public IObservable<Unit> DownloadAssetBundleProgress(IEnumerable<string> bundleNames, IProgress<float> progress = null, int maxConcurrent = 4)
		//{
		//	return WaitForManifestLoaded().ContinueWith(_ =>
		//	{
		//		var set = new HashSet<string>();
		//		foreach (var item in bundleNames)
		//		{
		//			var bundleName = new BundleEntry(item).NormBundleName;
		//			set.Add(bundleName);
		//			var dep = Manifest.GetAllDependencies(bundleName);
		//			for (int i = 0; i < dep.Length; i++)
		//				set.Add(dep[i]);
		//		}
		//		return set.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)))
		//			.ToTaskList().SetMaxConcurrent(maxConcurrent).ToObservable(progress);
		//	});
		//}

	}

	public sealed class BatchDownloader
	{
		public string RootUri;
		public KeyValuePair<string, Hash128>[] List;

		public TaskQueue ToTaskQueue()
		{
			var queue = new TaskQueue();
			for (int i = 0; i < List.Length; i++)
			{
				var item = List[i];
				var request = UnityWebRequest.GetAssetBundle(RootUri + item.Key, item.Value, 0);
				queue.Add(progress => request.SendAsObservable(progress).AsUnitObservable());
			}
			return queue;
		}
	}
}
