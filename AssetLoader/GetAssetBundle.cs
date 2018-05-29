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

		public IObservable<Unit> CreateBundleTaskList(IEnumerable<string> bundleNames, DividableProgress progress = null, int maxConcurrent = 4)
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
}
