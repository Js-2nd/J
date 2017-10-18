namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		[NonSerialized]
		public AssetBundleManifest m_Manifest;
		[NonSerialized]
		public string m_RootUri;

		Dictionary<string, string> m_BundleNames = new Dictionary<string, string>();
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
						m_Manifest = newManifest;
						m_RootUri = uri.Substring(0, uri.LastIndexOfAny(Delimiters) + 1);
						MapBundleNames();
						Debug.Log("AssetBundleManifest loaded");
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

		void MapBundleNames()
		{
			m_BundleNames.Clear();
			foreach (var item in m_Manifest.GetAllAssetBundles())
			{
				var name = item;
				var suffix = "_" + m_Manifest.GetAssetBundleHash(item);
				if (name.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
				{
					name = name.Substring(0, name.Length - suffix.Length);
				}
				m_BundleNames.Add(name, item);
			}
		}

		enum LoadManifestStatus
		{
			NotLoaded,
			Loading,
			Loaded,
		}
	}
}
