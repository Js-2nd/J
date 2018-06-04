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

		public static IObservable<RequestVersionInfo> SendAssetBundleRequest(string uri, string versionKey, string eTagKey)
		{
			return Observable.Interval(TimeSpan.Zero).FirstOrEmpty(_ => Caching.ready).ContinueWith(_ =>
			{
				int version = PlayerPrefs.GetInt(versionKey);
				if (Caching.IsVersionCached(uri, VersionToHash(version)))
				{
					var request = UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version + 1), 0);
					request.SetRequestHeader(HttpHeader.IfNoneMatch, PlayerPrefs.GetString(eTagKey));
					return request.SendAsObservable(throwNetworkError: false, throwHttpError: false).ContinueWith(req =>
					{
						if (req.responseCode == 304)
							return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version), 0).SendAsObservable()
								.Select(r => new RequestVersionInfo { Request = r, Version = version, IsNew = false });
						req.TryThrowError();
						PlayerPrefs.SetInt(versionKey, version + 1);
						PlayerPrefs.SetString(eTagKey, req.GetResponseHeader(HttpHeader.ETag));
						var info = new RequestVersionInfo { Request = req, Version = version + 1, IsNew = true };
						return Observable.Return(info);
					});
				}
				return UnityWebRequestAssetBundle.GetAssetBundle(uri, VersionToHash(version), 0).SendAsObservable().Select(req =>
				{
					PlayerPrefs.SetString(eTagKey, req.GetResponseHeader(HttpHeader.ETag));
					return new RequestVersionInfo { Request = req, Version = version, IsNew = true };
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

	public class RequestVersionInfo
	{
		public UnityWebRequest Request;
		public int Version;
		public bool IsNew;
	}

	partial class AssetLoader
	{
		public static IObservable<RequestVersionInfo>
			SendAssetBundleRequest(string uri, string versionKey, string eTagKey) =>
			AssetLoaderInstance.SendAssetBundleRequest(uri, versionKey, eTagKey);

		public static IObservable<AssetBundle> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));
	}
}
