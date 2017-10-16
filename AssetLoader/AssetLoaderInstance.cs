namespace J
{
	using UnityEngine;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		public const string BundleNameSuffix = ".unity3d";

		public static readonly char[] Delimiters = { '/', '\\' };

		public bool m_DontDestroyOnLoad = true;
		public bool m_SimulationMode = true;
		public bool m_AutoLoadManifest = true;

		[SerializeField] string ANDROID_URI;
		[SerializeField] string IOS_URI;
		[SerializeField] string STANDALONE_URI;

		public string ManifestUri =>
#if UNITY_ANDROID
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

			if (m_AutoLoadManifest && !string.IsNullOrEmpty(ManifestUri))
				LoadManifest(ManifestUri);
		}

		protected override void SingletonOnDestroy()
		{
			m_BundlePending.Dispose();
		}
	}
}
