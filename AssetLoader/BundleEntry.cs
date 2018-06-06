namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public static string NormBundleName(string bundleName) => bundleName.ToLower();
	}

	public sealed class BundleEntry
	{
		public string NormName { get; }

		public BundleEntry(string bundleName)
		{
			if (bundleName == null) throw new ArgumentNullException(nameof(bundleName));
			NormName = AssetLoaderInstance.NormBundleName(bundleName);
		}

		public override string ToString() => NormName;
	}
}
