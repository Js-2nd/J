namespace J
{
	using System;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static readonly Type TypeObject = typeof(UnityEngine.Object);

		static string DefaultAssetName(string bundleName)
		{
			bundleName = NormBundleName(bundleName);
			int index = bundleName.LastIndexOfAny(Delimiters) + 1;
			return bundleName.Substring(index);
		}

		public class AssetEntry : Tuple<string, string, Type>
		{
			public string BundleName { get { return base.Item1; } }
			public string AssetName { get { return base.Item2; } }
			public Type AssetType { get { return base.Item3; } }

			public AssetEntry(string bundleName, string assetName = null, Type assetType = null) : base(NormBundleName(bundleName), assetName ?? DefaultAssetName(bundleName), assetType ?? TypeObject) { }
			public AssetEntry(string bundleName, Type assetType) : this(bundleName, null, assetType) { }

			public BundleEntry ToBundleEntry()
			{
				return new BundleEntry(BundleName);
			}

			[Obsolete("Use BundleName instead", true)]
			public new object Item1 { get; }
			[Obsolete("Use AssetName instead", true)]
			public new object Item2 { get; }
			[Obsolete("Use AssetType instead", true)]
			public new object Item3 { get; }
		}
	}
}
