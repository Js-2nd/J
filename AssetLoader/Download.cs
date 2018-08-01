namespace J
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;

	partial class AssetLoaderInstance
	{
		public IObservable<BatchDownloader> Download(IEnumerable<string> bundleNames,
			bool includeDependencies = true, float? yieldInterval = null)
		{
			return WhenManifestLoaded().ContinueWith(_ => Observable.FromMicroCoroutine<BatchDownloader>(observer =>
				DownloadCoroutine(observer, bundleNames, includeDependencies, yieldInterval)));
		}

		IEnumerator DownloadCoroutine(IObserver<BatchDownloader> observer, IEnumerable<string> bundleNames,
			bool includeDependencies, float? yieldInterval)
		{
			var yieldTime = Time.realtimeSinceStartup + yieldInterval;
			var set = new HashSet<string>();
			var downloader = new BatchDownloader();
			Action<string> add = actualName =>
			{
				if (!set.Add(actualName)) return;
				var hash = Manifest.GetAssetBundleHash(actualName);
				if (Caching.IsVersionCached(actualName, hash)) return;
				downloader.Add(new AssetBundleDownloader { Url = RootUrl + actualName, Hash = hash });
			};
			foreach (string bundleName in bundleNames)
			{
				string normBundleName = NormBundleName(bundleName);
				if (ManifestContains(normBundleName))
				{
					string actualName = NormToActualName(normBundleName);
					add(actualName);
					if (includeDependencies)
					{
						var dependencies = Manifest.GetAllDependencies(actualName);
						for (int i = 0, iCount = dependencies.Length; i < iCount; i++)
							add(dependencies[i]);
					}
				}
				if (yieldTime <= Time.realtimeSinceStartup)
				{
					yield return null;
					yieldTime = Time.realtimeSinceStartup + yieldInterval;
				}
			}
			set.Clear();
			observer.OnNext(downloader);
			observer.OnCompleted();
		}
	}

	partial class AssetLoader
	{
		public static IObservable<BatchDownloader> Download(IEnumerable<string> bundleNames,
			bool includeDependencies = true, float? yieldInterval = null) =>
			Instance.Download(bundleNames, includeDependencies, yieldInterval);
	}
}
