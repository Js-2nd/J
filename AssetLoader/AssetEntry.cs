namespace J
{
	using System;
	using Object = UnityEngine.Object;

	public partial class AssetLoaderInstance : SingletonMonoBehaviour<AssetLoaderInstance>
	{
		static readonly Type TypeObject = typeof(Object);

		static string DefaultAssetName(string bundleName)
		{
			bundleName = NormBundleName(bundleName);
			int start = bundleName.LastIndexOfAny(Delimiters) + 1;
			int end = bundleName.LastIndexOf(BundleNameSuffix);
			return bundleName.Substring(start, end - start);
		}

		public class AssetEntry : Tuple<string, string, Type>
		{
			public string BundleName { get { return base.Item1; } }
			public string AssetName { get { return base.Item2; } }
			public Type AssetType { get { return base.Item3; } }

			public AssetEntry(string bundleName) : this(bundleName, DefaultAssetName(bundleName), TypeObject) { }
			public AssetEntry(string bundleName, string assetName) : this(bundleName, assetName, TypeObject) { }
			public AssetEntry(string bundleName, Type assetType) : this(bundleName, DefaultAssetName(bundleName), assetType) { }
			public AssetEntry(string bundleName, string assetName, Type assetType) : base(NormBundleName(bundleName), assetName, assetType) { }

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
