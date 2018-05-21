namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public partial class AssetLoaderInstance
	{
		readonly Dictionary<BundleEntry, IObservable<AssetBundle>> m_BundleCache = new Dictionary<BundleEntry, IObservable<AssetBundle>>();

		IObservable<AssetBundle> GetAssetBundleCore(BundleEntry entry)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				var name = m_BundleNames.GetOrDefault(entry.BundleName, entry.BundleName);
				var uri = RootUri + name;
				var hash = Manifest.GetAssetBundleHash(name);
				return UnityWebRequest.GetAssetBundle(uri, hash, 0).ToAssetBundleObservable();
			});
		}

		public IObservable<AssetBundle> GetAssetBundle(BundleEntry entry)
		{
			return m_BundleCache.GetOrAdd(entry, e =>
			{
				return GetAssetBundleCore(e).Replay(Scheduler.MainThreadIgnoreTimeScale);
			});
		}

		public IObservable<AssetBundle> GetAssetBundleWithDependencies(BundleEntry entry)
		{
			return WaitForManifestLoaded()
				.ContinueWith(_ => entry.BundleName.ToSingleEnumerable()
					.Concat(Manifest.GetAllDependencies(entry.BundleName))
					.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)))
					.WhenAll())
				.SelectMany(bundles => bundles.Take(1))
				.Share();
		}
	}
}
