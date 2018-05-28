namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;

	partial class AssetLoaderInstance
	{
		ReactiveProperty<ManifestStatus> m_ManifestStatus;
		Dictionary<string, string> m_BundleNames;

		void AwakeManifest()
		{
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
			m_BundleNames = new Dictionary<string, string>();
		}

		void OnDestroyManifest()
		{
			m_ManifestStatus.Dispose();
		}

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
}
