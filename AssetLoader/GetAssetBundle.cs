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

		string NormToActualName(string normBundleName) => m_NormToActual.GetOrDefault(normBundleName, normBundleName); // TODO return null

		IObservable<AssetBundle> GetAssetBundleCore(string actualName, IProgress<float> progress = null)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				string url = RootUrl + actualName;
				var hash = Manifest.GetAssetBundleHash(actualName);
				return UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0)
					.SendAsObservable(progress: progress).LoadAssetBundle();
			});
		}

		IObservable<BundleCache> GetAssetBundle(string actualName)
		{
			return Observable.Defer(() =>
			{
				BundleCache cache;
				if (!m_BundleCaches.TryGetValue(actualName, out cache))
				{
					cache = new BundleCache
					{
						Subject = new AsyncSubject<AssetBundle>(),
					};
					m_BundleCaches.Add(actualName, cache);
					GetAssetBundleCore(actualName)
						.DoOnError(ex => m_BundleCaches.Remove(actualName))
						.Subscribe(cache.Subject);
				}
				cache.RefCount++;
				return Observable.Return(cache);
			});
		}
		public IObservable<BundleCache> GetAssetBundle(BundleEntry entry)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				string actualName = NormToActualName(entry.NormName);
				return GetAssetBundle(actualName);
			});
		}

		public IObservable<AssetBundle> GetAssetBundleWithDependencies(BundleEntry entry, int maxConcurrent = 8)
		{
			return WaitForManifestLoaded().ContinueWith(_ =>
			{
				AssetBundle entryBundle = null;
				string actualName = NormToActualName(entry.NormName);
				var dep = Manifest.GetAllDependencies(actualName).Select(GetAssetBundle);
				return GetAssetBundle(actualName)
					.Do(bundle => entryBundle = bundle)
					.ToSingleEnumerable().Concat(dep)
					.Merge(maxConcurrent).AsSingleUnitObservable()
					.Select(__ => entryBundle);
			});
		}
	}

	public class RequestInfo
	{
		public UnityWebRequest Request { get; }
		public int Version { get; }

		public RequestInfo(UnityWebRequest request, int version)
		{
			Request = request;
			Version = version;
		}
	}

	//partial class AssetLoader
	//{
	//	public static IObservable<RequestInfo>
	//		SendAssetBundleRequest(string uri, string versionKey, string eTagKey) =>
	//		AssetLoaderInstance.SendAssetBundleRequest(uri, versionKey, eTagKey);

	//	public static IObservable<AssetBundle> GetAssetBundle(string bundleName) =>
	//		Instance.GetAssetBundle(new BundleEntry(bundleName));

	//	public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName) =>
	//		Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));
	//}
}
