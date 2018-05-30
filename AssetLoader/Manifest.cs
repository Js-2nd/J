namespace J
{
	using System;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	partial class AssetLoaderInstance
	{
		public AssetBundleManifest Manifest { get; private set; }
		public string RootUri { get; set; }

		public IObservable<AssetBundleManifest> LoadManifest(string uri, bool? setRoot = null)
		{
			return Observable.Defer(() =>
			{
				m_ManifestStatus.Value = ManifestStatus.Loading;
				AssetBundle manifestBundle = null;
				AssetBundleManifest manifest = null;
				bool cancel = false;
				return UnityWebRequest.GetAssetBundle(uri).ToAssetBundleObservable().ContinueWith(bundle =>
				{
					manifestBundle = bundle;
					return bundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest").AsAsyncOperationObservable();
				}).Select(req =>
				{
					manifest = req.asset as AssetBundleManifest;
					if (manifest == null) throw new Exception("AssetBundleManifest not found.");
					return manifest;
				}).DoOnCancel(() =>
				{
					cancel = true;
				}).Finally(() =>
				{
					if (manifestBundle != null) manifestBundle.Unload(false);
					if (cancel) return;
					if (manifest != null)
					{
						if (setRoot ?? true) RootUri = uri.Substring(0, uri.LastIndexOfAny(Delimiters) + 1);
						SetManifest(manifest);
					}
					else
					{
						m_ManifestStatus.Value = Manifest != null ? ManifestStatus.Loaded : ManifestStatus.NotLoaded;
					}
				});
			});
		}

		public void SetManifest(AssetBundleManifest manifest)
		{
			if (manifest == null) throw new ArgumentNullException(nameof(manifest));
			Manifest = manifest;
			MapBundleNames();
			m_ManifestStatus.Value = ManifestStatus.Loaded;
		}

		void MapBundleNames()
		{
			m_BundleNames.Clear();
			var all = Manifest.GetAllAssetBundles();
			for (int i = 0; i < all.Length; i++)
			{
				string bundleName = all[i], trim;
				if (TrimHash(bundleName, out trim))
					m_BundleNames.Add(trim, bundleName);
			}
		}

		bool TrimHash(string bundleName, out string trim)
		{
			string hash = Manifest.GetAssetBundleHash(bundleName).ToString();
			if (bundleName.EndsWith(hash, StringComparison.OrdinalIgnoreCase))
			{
				trim = bundleName.Substring(0, bundleName.Length - hash.Length - 1);
				return true;
			}
			trim = bundleName;
			return false;
		}

		public void UnloadManifest()
		{
			Manifest = null;
			m_BundleNames.Clear();
			if (m_ManifestStatus.Value == ManifestStatus.Loaded)
				m_ManifestStatus.Value = ManifestStatus.NotLoaded;
		}

		public IObservable<Unit> WaitForManifestLoaded()
		{
			return Observable.Defer(() =>
			{
				if (IsManifestLoaded()) return Observable.ReturnUnit();
				return m_ManifestStatus.FirstOrEmpty(_ => IsManifestLoaded()).AsUnitObservable();
			});
		}

		bool IsManifestLoaded()
		{
			if (m_ManifestStatus.Value == ManifestStatus.NotLoaded)
				throw new Exception("No AssetBundleManifest loading or loaded.");
			return m_ManifestStatus.Value == ManifestStatus.Loaded;
		}

		enum ManifestStatus
		{
			NotLoaded,
			Loading,
			Loaded,
		}
	}

	partial class AssetLoader
	{
		public static AssetBundleManifest Manifest => Instance.Manifest;

		public static string RootUri { get { return Instance.RootUri; } set { Instance.RootUri = value; } }

		public static IObservable<AssetBundleManifest> LoadManifest(string uri, bool? setRoot = null) => Instance.LoadManifest(uri, setRoot);

		public static void SetManifest(AssetBundleManifest manifest) => Instance.SetManifest(manifest);

		public static void UnloadManifest() => Instance.UnloadManifest();

		public static IObservable<Unit> WaitForManifestLoaded() => Instance.WaitForManifestLoaded();
	}
}
