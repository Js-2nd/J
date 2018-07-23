#if !UNITY_2018_1_OR_NEWER
using UnityWebRequestAssetBundle = UnityEngine.Networking.UnityWebRequest;
#endif

namespace J
{
	using System;
	using UniRx;
	using UnityEngine;
	using UnityEngine.Networking;
	using ReqVerPair = System.Collections.Generic.KeyValuePair<UnityEngine.Networking.UnityWebRequest, int>;

	partial class AssetLoaderInstance
	{
		public const string ManifestVersionKey = "AssetLoader.ManifestVersion";
		public const string ManifestETagKey = "AssetLoader.ManifestETag";

		static Hash128 VersionToHash(int version) => new Hash128(0, 0, 0, (uint)version);

		static ReqVerPair CreateReqVerPair(UnityWebRequest request, int version, bool save = false)
		{
			if (save)
			{
				PlayerPrefs.SetInt(ManifestVersionKey, version);
				PlayerPrefs.SetString(ManifestETagKey, request.GetETag());
			}
			return new ReqVerPair(request, version);
		}

		static IObservable<ReqVerPair> SendManifestRequest(string url) => AssetLoader.WhenCacheReady(() =>
		{
			int version = PlayerPrefs.GetInt(ManifestVersionKey, 1);
			var hash = VersionToHash(version);
			if (!Caching.IsVersionCached(url, hash))
				return UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0)
					.SendAsObservable().Select(req => CreateReqVerPair(req, version, true));
			return UnityWebRequest.Head(url).SendAsObservable()
				.Catch((Exception __) => Observable.Return<UnityWebRequest>(null))
				.ContinueWith(head =>
				{
					if (head != null && head.GetETag() == PlayerPrefs.GetString(ManifestETagKey))
						return UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0)
							.SendAsObservable().Select(req => CreateReqVerPair(req, version));
					version = unchecked(version + 1);
					hash = VersionToHash(version);
					return UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0)
						.SendAsObservable().Select(req => CreateReqVerPair(req, version, true));
				});
		});

		public string PresetManifestUrl
		{
			get
			{
				return
#if UNITY_EDITOR
					EditorManifestUrl
#elif UNITY_ANDROID
					AndroidManifestUrl
#elif UNITY_IOS
					IosManifestUrl
#else
					StandaloneManifestUrl
#endif
					;
			}
			set
			{
#if UNITY_EDITOR
				EditorManifestUrl
#elif UNITY_ANDROID
				AndroidManifestUrl
#elif UNITY_IOS
				IosManifestUrl
#else
				StandaloneManifestUrl
#endif
					= value;
			}
		}

		public ManifestStatus ManifestStatus
		{
			get { return m_ManifestStatus.Value; }
			private set { m_ManifestStatus.Value = value; }
		}

		public AssetBundleManifest Manifest { get; private set; }
		public int ManifestVersion { get; private set; }
		public string RootUrl { get; set; }

		int m_ManifestLoadCount;

		public IObservable<Unit> LoadManifest(string url = null, bool? setRootUrl = null) => Observable.Defer(() =>
		{
			if (string.IsNullOrEmpty(url))
			{
				url = PresetManifestUrl;
				if (string.IsNullOrEmpty(url)) url = "/";
			}
			int version = 0;
			AssetBundle manifestBundle = null;
			int count = ++m_ManifestLoadCount;
			ManifestStatus = ManifestStatus.Loading;
			return SendManifestRequest(url).Select(pair =>
			{
				version = pair.Value;
				return pair.Key;
			}).LoadAssetBundle().ContinueWith(bundle =>
			{
				manifestBundle = bundle;
				return bundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest").AsAsyncOperationObservable();
			}).Select(request =>
			{
				if (count == m_ManifestLoadCount)
				{
					SetManifest(request.asset as AssetBundleManifest, version);
					if (setRootUrl ?? true) RootUrl = url.Substring(0, url.LastIndexOfAny(Delimiters) + 1);
					ManifestStatus = ManifestStatus.Loaded;
				}
				return Unit.Default;
			}).Finally(() =>
			{
				if (manifestBundle != null) manifestBundle.Unload(false);
				if (count == m_ManifestLoadCount && ManifestStatus == ManifestStatus.Loading)
					ManifestStatus = Manifest != null ? ManifestStatus.Loaded : ManifestStatus.NotLoaded;
			});
		});

		void SetManifest(AssetBundleManifest manifest, int version)
		{
			if (manifest == null) throw new ArgumentNullException(nameof(manifest));
			UnloadUnusedBundles();
			Manifest = manifest;
			ManifestVersion = version;
			m_ActualNames.Clear();
			m_NormToActual.Clear();
			foreach (string actualName in Manifest.GetAllAssetBundles())
			{
				m_ActualNames.Add(actualName);
				string hash = Manifest.GetAssetBundleHash(actualName).ToString();
				if (actualName.EndsWith(hash, StringComparison.OrdinalIgnoreCase))
					m_NormToActual.Add(actualName.Substring(0, actualName.Length - hash.Length - 1), actualName);
			}
		}

		public IObservable<Unit> WhenManifestLoaded(bool? load = null) => Observable.Defer(() =>
		{
			if (ManifestStatus == ManifestStatus.Loaded) return Observable.ReturnUnit();
			if (ManifestStatus == ManifestStatus.NotLoaded && (load ?? LoadManifestOnDemand))
				LoadManifest().Subscribe();
			return m_ManifestStatus.FirstOrEmpty(status =>
			{
				if (status == ManifestStatus.NotLoaded)
					throw new InvalidOperationException("No AssetBundleManifest loading or loaded.");
				return status == ManifestStatus.Loaded;
			}).AsUnitObservable();
		});

		public bool ManifestContains(BundleEntry entry) => ManifestContains(entry.NormName);
		bool ManifestContains(string normBundleName)
		{
			ThrowIfManifestNotLoaded();
			return m_NormToActual.ContainsKey(normBundleName) || m_ActualNames.Contains(normBundleName);
		}

		void ThrowIfManifestNotLoaded()
		{
			if (ManifestStatus != ManifestStatus.Loaded)
				throw new InvalidOperationException("AssetBundleManifest not loaded.");
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
		public static string PresetManifestUrl
		{
			get { return Instance.PresetManifestUrl; }
			set { Instance.PresetManifestUrl = value; }
		}

		public static ManifestStatus ManifestStatus => Instance ? Instance.ManifestStatus : ManifestStatus.NotLoaded;

		public static AssetBundleManifest Manifest => Instance.Manifest;

		public static int ManifestVersion => Instance.ManifestVersion;

		public static string RootUrl
		{
			get { return Instance.RootUrl; }
			set { Instance.RootUrl = value; }
		}

		public static IObservable<Unit> LoadManifest(string url = null, bool? setRootUrl = null) =>
			Instance.LoadManifest(url, setRootUrl);

		public static IObservable<Unit> WhenManifestLoaded(bool? load = null) =>
			Instance.WhenManifestLoaded(load);

		public static bool ManifestContains(string bundleName) =>
			ManifestStatus == ManifestStatus.Loaded && Instance.ManifestContains(new BundleEntry(bundleName));
	}
}
