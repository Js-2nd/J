#if !UNITY_2018_1_OR_NEWER
using UnityWebRequestAssetBundle = UnityEngine.Networking.UnityWebRequest;
#endif

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
		string NormToActualName(string normBundleName) => m_NormToActual.GetOrDefault(normBundleName, normBundleName);

		IObservable<AssetBundle> GetAssetBundleCore(string actualName)
		{
			ThrowIfManifestNotLoaded();
			string url = RootUrl + actualName;
			var hash = Manifest.GetAssetBundleHash(actualName);
			var request = hash.isValid
				? UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0)
				: UnityWebRequestAssetBundle.GetAssetBundle(url);
			return request.SendAsObservable().LoadAssetBundle();
		}

		IObservable<BundleReference> GetAssetBundle(string actualName) => Observable.Defer(() =>
		{
			BundleCache cache;
			if (!m_BundleCaches.TryGetValue(actualName, out cache))
			{
				cache = new BundleCache(actualName);
				m_BundleCaches.Add(actualName, cache);
				GetAssetBundleCore(actualName)
					.DoOnError(_ => m_BundleCaches.Remove(actualName))
					.Subscribe(cache);
			}
			return cache.GetReference();
		});

		public IObservable<BundleReference> GetAssetBundle(BundleEntry entry)
		{
			return WhenManifestLoaded().ContinueWith(_ => GetAssetBundle(NormToActualName(entry.NormName)));
		}

		public IObservable<BundleReference> GetAssetBundleWithDependencies(BundleEntry entry, int maxConcurrent = 8)
		{
			return WhenManifestLoaded().ContinueWith(_ =>
			{
				if (!ManifestContains(entry)) // TODO throw directly https://github.com/neuecc/UniRx/issues/311
					return Observable.Throw<BundleReference>(new AssetNotFoundException(entry));
				string actualName = NormToActualName(entry.NormName);
				var dependencies = Manifest.GetAllDependencies(actualName);
				var cancel = new CompositeDisposable(dependencies.Length + 1);
				BundleReference entryReference = null;
				return GetAssetBundle(actualName)
					.Do(reference =>
					{
						if (reference == null) return;
						entryReference = reference;
						cancel.Add(reference);
					})
					.ToSingleEnumerable()
					.Concat(dependencies.Select(depend => GetAssetBundle(depend).Do(reference =>
					{
						if (reference == null) return;
						cancel.Add(reference);
					})))
					.Merge(maxConcurrent)
					.AsSingleUnitObservable()
					.Where(__ =>
					{
						if (entryReference != null) return true;
						cancel.Dispose();
						return false;
					})
					.Select(__ => new BundleReference(entryReference.Bundle, cancel))
					.DoOnError(__ => cancel.Dispose())
					.DoOnCancel(() => cancel.Dispose());
			});
		}

		public void UnloadUnusedBundles(bool unloadAllLoadedAssets = false) // TODO async?
		{
			if (m_BundleCaches.Count <= 0) return;
			var oldCaches = m_BundleCaches;
			m_BundleCaches = new Dictionary<string, BundleCache>();
			foreach (var item in oldCaches)
			{
				var cache = item.Value;
				if (cache.RefCount > 0)
				{
					m_BundleCaches.Add(item.Key, cache);
					continue;
				}
				cache.GetReference().CatchIgnore().Subscribe(reference =>
				{
					try { reference.Bundle.Unload(unloadAllLoadedAssets); }
					finally { reference.Dispose(); }
				});
			}
			oldCaches.Clear();
		}
	}

	partial class AssetLoader
	{
		public static IObservable<BundleReference> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<BundleReference> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));

		public static void UnloadUnusedBundles(bool unloadAllLoadedAssets = false) =>
			Instance.UnloadUnusedBundles(unloadAllLoadedAssets);
	}
}
