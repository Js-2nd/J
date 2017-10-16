namespace J
{
	using System;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		[NonSerialized]
		public AssetBundleManifest m_Manifest;
		[NonSerialized]
		public string m_RootUri;
		ReactiveProperty<LoadManifestStatus> m_LoadManifestStatus = new ReactiveProperty<LoadManifestStatus>(LoadManifestStatus.NotLoaded);

		public void LoadManifest(string uri)
		{
			m_LoadManifestStatus.Value = LoadManifestStatus.Loading;
			AssetBundleManifest newManifest = null;
			UnityWebRequest.GetAssetBundle(uri).AsAssetBundleObservable()
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
						m_RootUri = uri.Substring(0, uri.LastIndexOfAny(Delimiters) + 1);
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
