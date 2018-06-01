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
		string GetActualBundleName(string normBundleName) => m_BundleNames.GetOrDefault(normBundleName, normBundleName);

		UnityWebRequest GetAssetBundleRequest(string normBundleName, uint crc = 0)
		{
			string actualBundleName = GetActualBundleName(normBundleName);
			return UnityWebRequest.GetAssetBundle(RootUri + actualBundleName, Manifest.GetAssetBundleHash(actualBundleName), crc);
		}

		IObservable<AssetBundle> GetAssetBundleCore(string normBundleName, IProgress<float> progress = null)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
				GetAssetBundleRequest(normBundleName).SendAsObservable(progress).ToAssetBundle());
		}

		public IObservable<AssetBundle> GetAssetBundle(BundleEntry entry)
		{
			return Observable.Defer(() =>
			{
				AsyncSubject<AssetBundle> cache;
				if (!m_BundleCache.TryGetValue(entry, out cache))
				{
					cache = new AsyncSubject<AssetBundle>();
					m_BundleCache.Add(entry, cache);
					GetAssetBundleCore(entry.NormBundleName)
						.DoOnError(ex => m_BundleCache.Remove(entry))
						.Subscribe(cache);
				}
				return cache;
			});
		}

		public IObservable<AssetBundle> GetAssetBundleWithDependencies(BundleEntry entry, int maxConcurrent = 8)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				AssetBundle entryBundle = null;
				var dep = Manifest.GetAllDependencies(entry.NormBundleName)
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
