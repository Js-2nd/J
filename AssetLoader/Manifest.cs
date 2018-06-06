namespace J
{
	using System;
	using System.IO;
	using UniRx;
	using UnityEngine;

	partial class AssetLoaderInstance
	{
		const string ManifestVersionKey = "AssetLoader.ManifestVersion";
		const string ManifestETagKey = "AssetLoader.ManifestETag";

		public AssetBundleManifest Manifest { get; private set; }
		public int ManifestVersion { get; private set; }
		public bool ManifestIsNew { get; private set; }
		public string RootUri { get; set; }

		public IObservable<Unit> LoadManifest(string uri = null, bool? setRootUri = null)
		{
			if (string.IsNullOrEmpty(uri))
			{
				uri = PresetManifestUri;
				if (string.IsNullOrEmpty(uri)) uri = "/";
			}
			return Observable.Defer(() =>
			{
				m_ManifestStatus.Value = ManifestStatus.Loading;
				RequestInfo requestInfo = null;
				AssetBundle manifestBundle = null;
				return SendAssetBundleRequest(uri, ManifestVersionKey, ManifestETagKey).Select(info =>
				{
					requestInfo = info;
					return info.Request;
				}).ToAssetBundle().ContinueWith(bundle =>
				{
					manifestBundle = bundle;
					return bundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest")
						.AsAsyncOperationObservable();
				}).Select(bundleRequest =>
				{
					var manifest = bundleRequest.asset as AssetBundleManifest;
					if (manifest == null) throw new InvalidDataException("AssetBundleManifest not found.");
					if (setRootUri ?? true) RootUri = uri.Substring(0, uri.LastIndexOfAny(Delimiters) + 1);
					SetManifest(manifest, requestInfo.Version, requestInfo.IsNew);
					return Unit.Default;
				}).Finally(() =>
				{
					if (manifestBundle != null) manifestBundle.Unload(false);
					if (m_ManifestStatus.Value == ManifestStatus.Loading)
						m_ManifestStatus.Value = Manifest != null ? ManifestStatus.Loaded : ManifestStatus.NotLoaded;
				});
			});
		}

		void SetManifest(AssetBundleManifest manifest, int version, bool isNew) // TODO clear bundle cache?
		{
			if (manifest == null) throw new ArgumentNullException(nameof(manifest));
			Manifest = manifest;
			ManifestVersion = version;
			ManifestIsNew = isNew;
			MapBundleNames();
			m_ManifestStatus.Value = ManifestStatus.Loaded;
		}

		void MapBundleNames()
		{
			m_BundleNames.Clear();
			var all = Manifest.GetAllAssetBundles();
			for (int i = 0; i < all.Length; i++)
			{
				string actualBundleName = all[i], trimBundleName;
				if (TrimBundleNameHash(actualBundleName, out trimBundleName))
					m_BundleNames.Add(trimBundleName, actualBundleName);
			}
		}

		bool TrimBundleNameHash(string normBundleName, out string trimBundleName)
		{
			string hash = Manifest.GetAssetBundleHash(normBundleName).ToString();
			if (normBundleName.EndsWith(hash, StringComparison.OrdinalIgnoreCase))
			{
				trimBundleName = normBundleName.Substring(0, normBundleName.Length - hash.Length - 1);
				return true;
			}
			trimBundleName = normBundleName;
			return false;
		}
		string TrimBundleNameHash(string normBundleName)
		{
			string trimBundleName;
			TrimBundleNameHash(normBundleName, out trimBundleName);
			return trimBundleName;
		}

		public void UnloadManifest() // TODO unload bundle cache?
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
				if (m_ManifestStatus.Value == ManifestStatus.Loaded)
					return Observable.ReturnUnit();
				if (m_ManifestStatus.Value == ManifestStatus.NotLoaded && AutoLoadManifest)
					LoadManifest().Subscribe();
				return m_ManifestStatus.FirstOrEmpty(status =>
				{
					if (status == ManifestStatus.NotLoaded)
						throw new InvalidOperationException("No AssetBundleManifest loading or loaded.");
					return status == ManifestStatus.Loaded;
				}).AsUnitObservable();
			});
		}
	}

	public enum ManifestStatus
	{
		NotLoaded,
		Loading,
		Loaded,
	}

	partial class AssetLoader
	{
		public static AssetBundleManifest Manifest => Instance.Manifest;

		public static int ManifestVersion => Instance.ManifestVersion;

		public static bool ManifestIsNew => Instance.ManifestIsNew;

		public static string RootUri { get { return Instance.RootUri; } set { Instance.RootUri = value; } }

		public static IObservable<Unit> LoadManifest(string uri = null, bool? setRootUri = null) =>
			Instance.LoadManifest(uri, setRootUri);

		public static void UnloadManifest() => Instance.UnloadManifest();

		public static IObservable<Unit> WaitForManifestLoaded() => Instance.WaitForManifestLoaded();
	}
}
