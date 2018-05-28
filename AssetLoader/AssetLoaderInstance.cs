namespace J
{
	using System;
	using System.Collections.Generic;
	using UniRx;
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static readonly char[] Delimiters = { '/', '\\' };

		[SerializeField] bool m_DontDestroyOnLoad = true;
		[SerializeField] Simulation m_SimulationMode = Simulation.AssetDatabase;
		public bool AutoLoadManifest = true;

		public string EDITOR_URI;
		public string STANDALONE_URI;
		public string ANDROID_URI;
		public string IOS_URI;

		public bool SimulationMode => Application.isEditor && m_SimulationMode != Simulation.Disable && AssetGraphLoader.IsValid;
		public string AutoLoadManifestUri =>
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

			if (m_DontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
			if (!SimulationMode && AutoLoadManifest && !string.IsNullOrWhiteSpace(AutoLoadManifestUri))
				LoadManifest(AutoLoadManifestUri);
		}

		protected override void SingletonOnDestroy()
		{
			m_ManifestStatus.Dispose();
			base.SingletonOnDestroy();
		}
	}
}
