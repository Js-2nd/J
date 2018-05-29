﻿namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	partial class AssetLoaderInstance
	{
		public TaskList Download(IEnumerable<string> bundleNames, bool includeDependencies = true)
		{
			var set = new HashSet<string>();
			foreach (var item in bundleNames)
			{
				var bundleName = new BundleEntry(item).BundleName;
				set.Add(bundleName);
				if (!includeDependencies) continue;
				var dep = Manifest.GetAllDependencies(bundleName);
				for (int i = 0; i < dep.Length; i++)
					set.Add(dep[i]);
			}
			return set.Where(bundleName => !Caching.IsVersionCached(bundleName, Manifest.GetAssetBundleHash(bundleName)))
				.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)))
				.ToTaskList();
		}

		public IObservable<Unit> DownloadAssetBundleProgress(IEnumerable<string> bundleNames, IProgress<float> progress = null, int maxConcurrent = 4)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				var set = new HashSet<string>();
				foreach (var item in bundleNames)
				{
					var bundleName = new BundleEntry(item).BundleName;
					set.Add(bundleName);
					var dep = Manifest.GetAllDependencies(bundleName);
					for (int i = 0; i < dep.Length; i++)
						set.Add(dep[i]);
				}
				return set.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)))
					.ToTaskList().SetMaxConcurrent(maxConcurrent).ToObservable(progress);
			});
		}
	}

	public sealed class AssetBundleDownloader
	{
		TaskList TaskList;
	}
}