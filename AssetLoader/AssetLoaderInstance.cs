namespace J
{
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static readonly char[] Delimiters = { '/', '\\' };

		[SerializeField] bool m_DontDestroyOnLoad = true;
		[SerializeField] bool m_SimulationMode = true;
		[SerializeField] bool m_AutoLoadManifest = true;

		[SerializeField] string STANDALONE_URI;
		[SerializeField] string ANDROID_URI;
		[SerializeField] string IOS_URI;

		bool SimulationMode => Application.isEditor && m_SimulationMode;

		public string InspectorManifestUri =>
#if UNITY_EDITOR
			STANDALONE_URI
#elif UNITY_ANDROID
			ANDROID_URI
#elif UNITY_IOS
			IOS_URI
#else
			STANDALONE_URI
#endif
			;

		protected override void SingletonAwake()
		{
			if (m_DontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);

			if (!SimulationMode && m_AutoLoadManifest && !string.IsNullOrEmpty(InspectorManifestUri))
				LoadManifest(InspectorManifestUri);
		}

		protected override void SingletonOnDestroy()
		{
			m_BundlePending.Dispose();
		}
	}
}
