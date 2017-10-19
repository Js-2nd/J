namespace J
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		readonly Dictionary<BundleEntry, ReplaySubject<AssetBundle>> m_BundleCache = new Dictionary<BundleEntry, ReplaySubject<AssetBundle>>();
		readonly DictionaryDisposable<BundleEntry, CompositeDisposable> m_BundlePending = new DictionaryDisposable<BundleEntry, CompositeDisposable>();

		ReplaySubject<AssetBundle> GetAssetBundleCore(BundleEntry entry)
		{
			var cache = new ReplaySubject<AssetBundle>();
			CompositeDisposable pending = new CompositeDisposable();
			m_BundlePending.Add(entry, pending);
			WaitForManifestLoaded()
				.ContinueWith(_ =>
				{
					var name = m_BundleNames.GetOrDefault(entry.BundleName, entry.BundleName);
					var uri = RootUri + name;
					var hash = Manifest.GetAssetBundleHash(name);
					return UnityWebRequest.GetAssetBundle(uri, hash, 0).AsAssetBundleObservable();
				})
				.Finally(() => m_BundlePending.Remove(entry))
				.Subscribe(cache)
				.AddTo(pending);
			return cache;
		}

		public IObservable<AssetBundle> GetAssetBundle(BundleEntry entry)
		{
			return m_BundleCache.GetOrAdd(entry, GetAssetBundleCore);
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
