namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	public partial class AssetLoaderInstance
	{
		readonly Dictionary<string, string> m_BundleNames = new Dictionary<string, string>();
		readonly ReactiveProperty<LoadManifestStatus> m_LoadManifestStatus = new ReactiveProperty<LoadManifestStatus>(LoadManifestStatus.NotLoaded);

		public AssetBundleManifest Manifest { get; private set; }
		public string RootUri { get; private set; }

		IDisposable m_ManifestPending;
		public IDisposable LoadManifest(string uri)
		{
			m_ManifestPending?.Dispose();
			m_LoadManifestStatus.Value = LoadManifestStatus.Loading;
			AssetBundle manifestBundle = null;
			AssetBundleManifest newManifest = null;
			bool disposed = false;
			return m_ManifestPending = UnityWebRequest.GetAssetBundle(uri).AsAssetBundleObservable()
				.ContinueWith(ab => (manifestBundle = ab).LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest").AsAsyncOperationObservable())
				.Select(req =>
				{
					newManifest = req.asset as AssetBundleManifest;
					if (newManifest == null)
						throw new Exception("AssetBundleManifest not found.");
					return newManifest;
				})
				.DoOnCancel(() => disposed = true)
				.Finally(() =>
				{
					manifestBundle?.Unload(false);
					if (disposed) return;
					if (newManifest == null)
					{
						Debug.LogError("Failed to load AssetBundleManifest.");
						m_LoadManifestStatus.Value = LoadManifestStatus.NotLoaded;
					}
					else
					{
						Manifest = newManifest;
						RootUri = uri.Substring(0, uri.LastIndexOfAny(Delimiters) + 1);
						MapBundleNames();
						Debug.Log("AssetBundleManifest loaded.");
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
					throw new Exception("No AssetBundleManifest loading or loaded.");
				return status == LoadManifestStatus.Loaded;
			}).Take(1).AsUnitObservable();
		}

		void MapBundleNames()
		{
			m_BundleNames.Clear();
			foreach (var item in Manifest.GetAllAssetBundles())
			{
				var name = item;
				var suffix = "_" + Manifest.GetAssetBundleHash(item);
				if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
					name = name.Substring(0, name.Length - suffix.Length);
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
