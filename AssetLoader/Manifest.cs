namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	partial class AssetLoaderInstance
	{
		Dictionary<string, string> m_BundleNames;
		ReactiveProperty<ManifestStatus> m_ManifestStatus;

		void AwakeManifest()
		{
			m_BundleNames = new Dictionary<string, string>();
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
		}

		public AssetBundleManifest Manifest { get; private set; }
		public string RootUri { get; private set; }


		public IObservable<AssetBundleManifest> LoadManifest(string uri)
		{
			return Observable.Defer(() =>
			{
				m_ManifestStatus.Value = ManifestStatus.Loading;
				AssetBundle bundle = null;
				AssetBundleManifest manifest = null;
				return UnityWebRequest.GetAssetBundle(uri).ToAssetBundleObservable().ContinueWith(ab =>
				{
					bundle = ab;
					return ab.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest").AsAsyncOperationObservable();
				}).Select(req =>
				{
					manifest = req.asset as AssetBundleManifest;
					if (manifest == null)
						throw new Exception("AssetBundleManifest not found.");
					return manifest;
				}).Finally(() =>
				{
					if (bundle != null) bundle.Unload(false);
					if (manifest != null)
					{
						RootUri = uri.Substring(0, uri.LastIndexOfAny(Delimiters) + 1);
						Debug.Log("AssetBundleManifest loaded.");
						SetManifest(manifest);
					}
					else
					{
						Debug.LogError("Failed to load AssetBundleManifest.");
						m_ManifestStatus.Value = ManifestStatus.NotLoaded;
					}
				});
			});
		}

		public IObservable<Unit> WaitForManifestLoaded()
		{
			return Observable.Defer(() =>
			{
				if (m_ManifestStatus.Value == ManifestStatus.Loaded) return Observable.ReturnUnit();
				if (m_ManifestStatus.Value == ManifestStatus.NotLoaded)
				{
				}
				return m_ManifestStatus.FirstOrEmpty(s => s == ManifestStatus.Loaded).AsUnitObservable();
			});
		}

		//bool Foo(ManifestStatus status)
		//{
		//	if (status == ManifestStatus.NotLoaded)
		//		throw new Exception("No AssetBundleManifest loading or loaded.");
		//	return status == ManifestStatus.Loaded;
		//}

		public void SetManifest(AssetBundleManifest manifest)
		{
			if (manifest == null)
				throw new ArgumentNullException(nameof(manifest));
			Manifest = manifest;
			MapBundleNames(manifest);
			m_ManifestStatus.Value = ManifestStatus.Loaded;
		}

		void MapBundleNames(AssetBundleManifest manifest)
		{
			m_BundleNames.Clear();
			var all = manifest.GetAllAssetBundles();
			for (int i = 0; i < all.Length; i++)
			{
				var item = all[i];
				var name = item;
				var hash = manifest.GetAssetBundleHash(name).ToString();
				if (name.EndsWith(hash, StringComparison.OrdinalIgnoreCase))
					name = name.Substring(0, name.Length - hash.Length - 1);
				m_BundleNames.Add(name, item);
			}
		}

		enum ManifestStatus
		{
			NotLoaded,
			Loading,
			Loaded,
		}
	}
}
