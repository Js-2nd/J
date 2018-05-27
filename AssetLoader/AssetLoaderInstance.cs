namespace J
{
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
#if UNITY_STANDALONE
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

		protected override void SingletonAwake()
		{
			base.SingletonAwake();
			AwakeManifest();
			if (m_DontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
			if (!SimulationMode && AutoLoadManifest && !string.IsNullOrWhiteSpace(AutoLoadManifestUri))
				LoadManifest(AutoLoadManifestUri);
		}
	}
}
