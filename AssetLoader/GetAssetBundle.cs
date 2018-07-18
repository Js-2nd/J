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
		public static Hash128 VersionToHash(int version) => new Hash128(0, 0, 0, (uint)version);

		public static IObservable<RequestInfo> SendAssetBundleRequest(string uri, string versionKey, string eTagKey) // TODO
		{
			return Observable.Interval(TimeSpan.Zero).FirstOrEmpty(_ => Caching.ready).ContinueWith(_ =>
			{
				Func<UnityWebRequest, int, RequestInfo> save = (req, ver) =>
				{
					PlayerPrefs.SetInt(versionKey, ver);
					PlayerPrefs.SetString(eTagKey, req.GetETag());
					return new RequestInfo(req, ver);
				};
				int version = PlayerPrefs.GetInt(versionKey, 1);
				if (!Caching.IsVersionCached(uri, VersionToHash(version)))
					return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version), 0)
						.SendAsObservable().Select(req => save(req, version));
				return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(unchecked(version + 1)), 0)
					.SendAsObservable(new UnityWebRequestSendOptions().SetETag(PlayerPrefs.GetString(eTagKey)))
					.ContinueWith(req =>
					{
						if (req.responseCode == 304)
							return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version), 0)
								.SendAsObservable().Select(r => new RequestInfo(r, version));
						return Observable.Return(save(req, unchecked(version + 1)));
					});
			});
		}

		string NormToActualName(string normBundleName) => m_NormToActual.GetOrDefault(normBundleName, normBundleName);

		IObservable<AssetBundle> GetAssetBundleCore(string actualName)
		{
			ThrowIfManifestNotLoaded();
			string url = RootUrl + actualName;
			var hash = Manifest.GetAssetBundleHash(actualName);
			return UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0).SendAsObservable().LoadAssetBundle();
		}

		IObservable<BundleReference> GetAssetBundle(string actualName) => Observable.Defer(() =>
		{
			BundleCache cache;
			if (!m_BundleCaches.TryGetValue(actualName, out cache))
			{
				cache = new BundleCache(actualName);
				m_BundleCaches.Add(actualName, cache);
				GetAssetBundleCore(actualName)
					.DoOnError(ex => m_BundleCaches.Remove(actualName))
					.Subscribe(cache);
			}
			return cache.GetReference();
		});

		public IObservable<BundleReference> GetAssetBundle(BundleEntry entry)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				string actualName = NormToActualName(entry.NormName);
				return GetAssetBundle(actualName);
			});
		}

		public IObservable<BundleReference> GetAssetBundleWithDependencies(BundleEntry entry, int maxConcurrent = 8)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				if (!ManifestContains(entry))
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
					.Concat(dependencies.Select(dep => GetAssetBundle(dep).Do(reference =>
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
	}

	public class RequestInfo // TODO
	{
		public UnityWebRequest Request { get; }
		public int Version { get; }

		public RequestInfo(UnityWebRequest request, int version)
		{
			Request = request;
			Version = version;
		}
	}

	partial class AssetLoader
	{
		//public static IObservable<RequestInfo>
		//	SendAssetBundleRequest(string uri, string versionKey, string eTagKey) =>
		//	AssetLoaderInstance.SendAssetBundleRequest(uri, versionKey, eTagKey);

		public static IObservable<BundleReference> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<BundleReference> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));
	}
}
