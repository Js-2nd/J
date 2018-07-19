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
		Dictionary<string, string> m_NormToActual;
		Dictionary<string, BundleCache> m_BundleCaches;

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			m_ManifestStatus = new ReactiveProperty<ManifestStatus>(ManifestStatus.NotLoaded);
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

		public void UnloadUnusedBundles(bool unloadAllLoadedAssets = false) // TODO async?
		{
			if (m_BundleCaches.Count <= 0) return;
			var oldCaches = m_BundleCaches;
			m_BundleCaches = new Dictionary<string, BundleCache>();
			foreach (var item in oldCaches)
			{
				var cache = item.Value;
				if (cache.RefCount > 0)
				{
					m_BundleCaches.Add(item.Key, cache);
					continue;
				}
				cache.GetReference().CatchIgnore().Subscribe(reference =>
				{
					try { reference.Bundle.Unload(unloadAllLoadedAssets); }
					finally { reference.Dispose(); }
				});
			}
			oldCaches.Clear();
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

		public static string PresetManifestUrl
		{
			get { return Instance.PresetManifestUrl; }
			set { Instance.PresetManifestUrl = value; }
		}

		public static void UnloadUnusedBundles(bool unloadAllLoadedAssets = false) =>
			Instance.UnloadUnusedBundles(unloadAllLoadedAssets);
	}
}
