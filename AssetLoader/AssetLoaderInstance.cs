namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static readonly char[] Delimiters = { '/', '\\' };

		public Simulation SimulationMode;
		[SerializeField] bool m_DontDestroyOnLoad;
		public bool UnloadAssetsOnDestroy;
		public bool AutoLoadManifest;
		public string EDITOR_URI;
		public string STANDALONE_URI;
		public string ANDROID_URI;
		public string IOS_URI;

		public string PresetManifestUri
		{
			get
			{
				return
#if UNITY_EDITOR
					EDITOR_URI
#elif UNITY_ANDROID
					ANDROID_URI
#elif UNITY_IOS
					IOS_URI
#else
					STANDALONE_URI
#endif
					;
			}
			set
			{
#if UNITY_EDITOR
				EDITOR_URI
#elif UNITY_ANDROID
				ANDROID_URI
#elif UNITY_IOS
				IOS_URI
#else
				STANDALONE_URI
#endif
					= value;
			}
		}

		void Reset()
		{
			m_DontDestroyOnLoad = true;
			AutoLoadManifest = true;
		}

		ReactiveProperty<ManifestStatus> m_ManifestStatus;
		Dictionary<string, string> m_BundleNames;
		Dictionary<BundleEntry, AsyncSubject<AssetBundle>> m_BundleCache;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
			m_BundleNames = new Dictionary<string, string>();
			m_BundleCache = new Dictionary<BundleEntry, AsyncSubject<AssetBundle>>();
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
			foreach (var cache in m_BundleCache.Values)
				cache.Subscribe(bundle => bundle.Unload(UnloadAssetsOnDestroy));
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

		public static string PresetManifestUri
		{
			get { return Instance.PresetManifestUri; }
			set { Instance.PresetManifestUri = value; }
		}
	}

	namespace Internal
	{
		public delegate string[] GetAssetPathsDelegate(string bundleName, string assetName);
		public delegate IObservable<UnityEngine.Object> LoadAssetDelegate(AssetEntry entry);
	}
}
