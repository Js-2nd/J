namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		public delegate string[] GetAssetPathsDelegate(string bundleName, string assetName);
		public delegate IObservable<UnityEngine.Object> LoadDelegate(AssetEntry entry);

		static readonly char[] Delimiters = { '/', '\\' };

		[SerializeField] bool m_DontDestroyOnLoad = true;
		public Simulation SimulationMode = Simulation.AssetDatabase;
		public bool UnloadAssetsOnDestroy;
		public bool AutoLoadManifest = true;

		public string EDITOR_URI;
		public string STANDALONE_URI;
		public string ANDROID_URI;
		public string IOS_URI;

		public string CurrentManifestUri
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
		Dictionary<BundleEntry, IObservable<AssetBundle>> m_BundleCache;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
			m_BundleNames = new Dictionary<string, string>();
			m_BundleCache = new Dictionary<BundleEntry, IObservable<AssetBundle>>();
			UpdateLoadMethod();

			if (m_DontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
			if (!IsSimulationEnabled && AutoLoadManifest && !string.IsNullOrWhiteSpace(CurrentManifestUri))
				LoadManifest(CurrentManifestUri).Subscribe();
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


		public static bool AutoLoadManifest
		{
			get { return Instance.AutoLoadManifest; }
			set { Instance.AutoLoadManifest = value; }
		}
	}
}
