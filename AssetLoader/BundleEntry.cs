namespace J
{
	using System;

	public partial class AssetLoaderInstance
	{
		static string NormBundleName(string bundleName) => bundleName.ToLower();

		public class BundleEntry : Tuple<string>
		{
			public string BundleName => base.Item1;

			public BundleEntry(string bundleName) : base(NormBundleName(bundleName)) { }

			[Obsolete("Use BundleName instead", true)]
			public new object Item1 { get; }
		}
	}
}
