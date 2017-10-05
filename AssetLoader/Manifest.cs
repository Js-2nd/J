using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace J
{
	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		[NonSerialized]
		public AssetBundleManifest m_Manifest;
		[NonSerialized]
		public string m_RootUrl;
		ReactiveProperty<LoadManifestStatus> m_LoadManifestStatus = new ReactiveProperty<LoadManifestStatus>(LoadManifestStatus.NotLoaded);

		public void LoadManifest(string manifestUrl)
		{
			m_ManifestUrl = manifestUrl;
			m_LoadManifestStatus.Value = LoadManifestStatus.Loading;
			AssetBundleManifest newManifest = null;
			UnityWebRequest.GetAssetBundle(manifestUrl).AsAssetBundleObservable()
				.ContinueWith(ab => ab.LoadAllAssetsAsync<AssetBundleManifest>().AsAsyncOperationObservable())
				.Select(req => newManifest = req.allAssets[0] as AssetBundleManifest)
				.Finally(() =>
				{
					if (newManifest == null)
					{
						Debug.LogError("Failed to load AssetBundleManifest.");
						m_LoadManifestStatus.Value = LoadManifestStatus.NotLoaded;
					}
					else
					{
						Debug.Log("AssetBundleManifest loaded");
						m_Manifest = newManifest;
						m_RootUrl = manifestUrl.Substring(0, manifestUrl.LastIndexOfAny(Delimiters) + 1);
						m_LoadManifestStatus.Value = LoadManifestStatus.Loaded;
					}
				})
				.Subscribe();
		}

		public IObservable<Unit> WaitForManifestLoaded()
		{
			return m_LoadManifestStatus.Where(status =>
			{
				if (status == LoadManifestStatus.NotLoaded)
				{
					throw new Exception("No AssetBundleManifest loading or loaded.");
				}
				return status == LoadManifestStatus.Loaded;
			}).Take(1).AsUnitObservable();
		}

		enum LoadManifestStatus
		{
			NotLoaded,
			Loading,
			Loaded,
		}
	}
}
