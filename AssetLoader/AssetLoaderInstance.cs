namespace J
{
	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		public const string BundleNameSuffix = ".unity3d";

		public static readonly char[] Delimiters = { '/', '\\' };

		public bool m_DontDestroyOnLoad = true;
		public bool m_SimulationMode = false;
		public bool m_AutoLoadManifest = true;
		public string m_ManifestUrl;

		protected override void SingletonAwake()
		{
			if (m_DontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);

			if (m_AutoLoadManifest && !string.IsNullOrEmpty(m_ManifestUrl))
				LoadManifest(m_ManifestUrl);
		}

		protected override void SingletonOnDestroy()
		{
			m_BundlePending.Dispose();
		}
	}
}
