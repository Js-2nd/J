namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;
	using static J.AssetLoaderInstance;

	partial class AssetLoaderInstance
	{
		IObservable<AssetBundle> GetAssetBundleCore(BundleEntry entry, IProgress<float> progress = null)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				var bundleName = m_BundleNames.GetOrDefault(entry.BundleName, entry.BundleName);
				var uri = RootUri + bundleName;
				var hash = Manifest.GetAssetBundleHash(bundleName);
				return UnityWebRequest.GetAssetBundle(uri, hash, 0).ToAssetBundleObservable(progress);
			});
		}

		public IObservable<AssetBundle> GetAssetBundle(BundleEntry entry)
		{
			return Observable.Defer(() =>
			{
				IObservable<AssetBundle> cache;
				if (m_BundleCache.TryGetValue(entry, out cache)) return cache;
				var subject = new AsyncSubject<AssetBundle>();
				m_BundleCache.Add(entry, subject);
				GetAssetBundleCore(entry)
					.DoOnError(ex => m_BundleCache.Remove(entry))
					.Subscribe(subject);
				return subject;
			});
		}

		public IObservable<AssetBundle> GetAssetBundleWithDependencies(BundleEntry entry, int maxConcurrent = 4)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				AssetBundle entryBundle = null;
				var dep = Manifest.GetAllDependencies(entry.BundleName)
					.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)));
				return GetAssetBundle(entry)
					.Do(bundle => entryBundle = bundle)
					.ToSingleEnumerable().Concat(dep)
					.Merge(maxConcurrent).AsSingleUnitObservable()
					.Select(__ => entryBundle);
			});
		}
	}

	partial class AssetLoader
	{
		public static IObservable<AssetBundle> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));
	}
}
