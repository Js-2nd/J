namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static readonly char[] Delimiters = { '/', '\\' };

		[SerializeField] AssetSimulation m_Simulation;
		[SerializeField] bool m_DontDestroyOnLoad = true;
		public bool UnloadAssetsOnDestroy;
		public bool AutoLoadManifest = true;
		public string EditorManifestUrl;
		public string StandaloneManifestUrl;
		public string AndroidManifestUrl;
		public string IosManifestUrl;

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
			get { return m_ManifestStatus?.Value ?? ManifestStatus.NotLoaded; }
			private set { if (m_ManifestStatus != null) m_ManifestStatus.Value = value; }
		}

		ReactiveProperty<ManifestStatus> m_ManifestStatus;
		Dictionary<string, string> m_NormToActual;
		Dictionary<string, BundleCache> m_BundleCaches;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
			m_NormToActual = new Dictionary<string, string>();
			m_BundleCaches = new Dictionary<string, BundleCache>();
			UpdateLoadMethod();
			if (m_DontDestroyOnLoad) DontDestroyOnLoad(gameObject);
		}

		void OnValidate()
		{
			if (Application.isPlaying) UpdateLoadMethod();
		}

		protected override void SingletonOnDestroy()
		{
			m_ManifestStatus.Dispose();
			foreach (var cache in m_BundleCaches.Values)
				cache.subject.Subscribe(bundle => bundle.Unload(UnloadAssetsOnDestroy));
			base.SingletonOnDestroy();
		}
	}

	public static partial class AssetLoader
	{
		public static AssetLoaderInstance Instance => AssetLoaderInstance.Instance;

		public static bool UnloadAssetsOnDestroy
		{
			get { return Instance.UnloadAssetsOnDestroy; }
			set { Instance.UnloadAssetsOnDestroy = value; }
		}

		public static bool AutoLoadManifest
		{
			get { return Instance.AutoLoadManifest; }
			set { Instance.AutoLoadManifest = value; }
		}

		public static string PresetManifestUrl
		{
			get { return Instance.PresetManifestUrl; }
			set { Instance.PresetManifestUrl = value; }
		}

		public static ManifestStatus ManifestStatus => Instance.ManifestStatus;
	}

	namespace Internal
	{
		public delegate string[] GetAssetPathsDelegate(string bundleName, string assetName);
		public delegate IObservable<UnityEngine.Object> LoadAssetDelegate(AssetEntry entry);
	}
}
