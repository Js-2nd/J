#if !UNITY_2018_1_OR_NEWER
using UnityWebRequestAssetBundle = UnityEngine.Networking.UnityWebRequest;
#endif

namespace J
{
	using J.Internal;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	partial class AssetLoaderInstance
	{
		static Hash128 VersionToHash(int version) => new Hash128(0, 0, 0, (uint)version);

		public static IObservable<RequestInfo> SendAssetBundleRequest(string uri, string versionKey, string eTagKey)
		{
			return Observable.Interval(TimeSpan.Zero).FirstOrEmpty(_ => Caching.ready).ContinueWith(_ =>
			{
				Func<UnityWebRequest, int, RequestInfo> saveNewVersion = (req, ver) =>
				{
					PlayerPrefs.SetInt(versionKey, ver);
					PlayerPrefs.SetString(eTagKey, req.GetResponseHeader(HttpHeader.ETag));
					PlayerPrefs.Save();
					return new RequestInfo(req, ver, true);
				};
				int version = PlayerPrefs.GetInt(versionKey, 1);
				if (!Caching.IsVersionCached(uri, VersionToHash(version)))
					return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version), 0)
						.SendAsObservable().Select(req => saveNewVersion(req, version));
				var request = UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version + 1), 0);
				request.SetRequestHeader(HttpHeader.IfNoneMatch, PlayerPrefs.GetString(eTagKey));
				return request.SendAsObservable(throwNetworkError: false, throwHttpError: false).ContinueWith(req =>
				{
					if (req.responseCode == 304)
						return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version), 0)
							.SendAsObservable().Select(oldReq => new RequestInfo(oldReq, version, false));
					req.TryThrowError();
					return Observable.Return(saveNewVersion(req, version + 1));
				});
			});
		}

		string GetActualBundleName(string normBundleName) => m_BundleNames.GetOrDefault(normBundleName, normBundleName);

		UnityWebRequest GetAssetBundleRequest(string normBundleName, uint crc = 0)
		{
			string actualBundleName = GetActualBundleName(normBundleName);
			return UnityWebRequestAssetBundle.GetAssetBundle(RootUri + actualBundleName, Manifest.GetAssetBundleHash(actualBundleName), crc);
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
				var dep = Manifest.GetAllDependencies(GetActualBundleName(entry.NormBundleName))
					.Select(bundleName => GetAssetBundle(new BundleEntry(TrimBundleNameHash(bundleName))));
				return GetAssetBundle(entry)
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
		public bool IsNew { get; }

		public RequestInfo(UnityWebRequest request, int version, bool isNew)
		{
			Request = request;
			Version = version;
			IsNew = isNew;
		}
	}

	partial class AssetLoader
	{
		public static IObservable<RequestInfo>
			SendAssetBundleRequest(string uri, string versionKey, string eTagKey) =>
			AssetLoaderInstance.SendAssetBundleRequest(uri, versionKey, eTagKey);

		public static IObservable<AssetBundle> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));
	}
}
