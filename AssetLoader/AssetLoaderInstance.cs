namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		public delegate string[] GetAssetPathsDelegate(string bundleName, string assetName);
		public delegate IObservable<UnityEngine.Object> LoadDelegate(AssetEntry entry);

		static readonly char[] Delimiters = { '/', '\\' };

		[SerializeField] AssetSimulation m_Simulation = AssetSimulation.AssetDatabase;
		public bool LoadManifestOnDemand = true;
		public string EditorManifestUrl;
		public string StandaloneManifestUrl;
		public string AndroidManifestUrl;
		public string IosManifestUrl;

		ReactiveProperty<ManifestStatus> m_ManifestStatus;
		HashSet<string> m_ActualNames;
		Dictionary<string, string> m_NormToActual;
		Dictionary<string, BundleCache> m_BundleCaches;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
			m_ActualNames = new HashSet<string>();
			m_NormToActual = new Dictionary<string, string>();
			m_BundleCaches = new Dictionary<string, BundleCache>();
			UpdateLoadMethod();
			DontDestroyOnLoad(transform.root);
			SceneManager.activeSceneChanged += OnChangeScene;
		}

		protected override void SingletonOnDestroy()
		{
			SceneManager.activeSceneChanged -= OnChangeScene;
			UnloadUnusedBundles();
			m_ManifestStatus.Dispose();
			base.SingletonOnDestroy();
		}

		void OnValidate()
		{
			if (Application.isPlaying) UpdateLoadMethod();
		}

		void OnChangeScene(Scene from, Scene to)
		{
			UnloadUnusedBundles();
		}
	}

	public static partial class AssetLoader
	{
		public static AssetLoaderInstance Instance => AssetLoaderInstance.Instance;

		public static bool LoadManifestOnDemand
		{
			get { return Instance.LoadManifestOnDemand; }
			set { Instance.LoadManifestOnDemand = value; }
		}

		public static IObservable<T> WhenCacheReady<T>(Func<IObservable<T>> factory)
		{
			return WhenCacheReady().ContinueWith(_ =>
			{
				try { return factory(); }
				catch (Exception ex) { return Observable.Throw<T>(ex); }
			});
		}
		public static IObservable<Unit> WhenCacheReady()
		{
			if (Caching.ready) return Observable.ReturnUnit();
			return Observable.Defer(() =>
			{
				if (Caching.ready) return Observable.ReturnUnit();
				return Observable.EveryUpdate().AsUnitObservable().FirstOrEmpty(_ => Caching.ready);
			});
		}
	}
}
