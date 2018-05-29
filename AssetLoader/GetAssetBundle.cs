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

		public IObservable<AssetBundle> GetAssetBundleWithDependencies(BundleEntry entry)
		{
			AssetBundle entryBundle = null;
			return WaitForManifestLoaded()
				.ContinueWith(_ =>
				{
					Manifest.GetAllDependencies(entry.BundleName)
						.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)))
						.Merge(4);
					return GetAssetBundle(entry).Do(bundle => entryBundle = bundle);
				});
					.Concat(Manifest.GetAllDependencies(entry.BundleName))
					.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)).AsUnitObservable())
					.WhenAll())
				.SelectMany(bundles => bundles.Take(1));
		}

		public IObservable<Unit> GetAssetBundleWithProgress(IEnumerable<string> bundleNames, DividableProgress progress)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				var set = new HashSet<string>();
				foreach (var name in bundleNames)
				{
					var entry = new BundleEntry(name);
					set.Add(entry.BundleName);
					var dep = Manifest.GetAllDependencies(entry.BundleName);
					for (int i = 0; i < dep.Length; i++)
						set.Add(dep[i]);
				}
				var list = new TaskList();
				foreach (var name in set)
					list.AddObservable(GetAssetBundle(new BundleEntry(name)));
				return GetAssetBundle(entry).Do(bundle => entryBundle = bundle);
			});
					.Concat(Manifest.GetAllDependencies(entry.BundleName))
					.Select(bundleName => GetAssetBundle(new BundleEntry(bundleName)).AsUnitObservable())
					.WhenAll())
				.SelectMany(bundles => bundles.Take(1));
		}
	}
}
