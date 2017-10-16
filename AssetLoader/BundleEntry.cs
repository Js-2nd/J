namespace J
{
	using System;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static string NormBundleName(string bundleName)
		{
			if (!bundleName.EndsWith(BundleNameSuffix, StringComparison.CurrentCultureIgnoreCase))
			{
				bundleName = bundleName + BundleNameSuffix;
			}
			return bundleName.ToLower();
		}

		public class BundleEntry : Tuple<string>
		{
			public string BundleName { get { return base.Item1; } }

			public BundleEntry(string bundleName) : base(NormBundleName(bundleName)) { }

			[Obsolete("Use BundleName instead", true)]
			public new object Item1 { get; }
		}
	}
}
