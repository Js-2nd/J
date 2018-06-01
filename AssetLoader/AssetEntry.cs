namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public static string DefaultAssetName(string normBundleName)
		{
			int index = normBundleName.LastIndexOfAny(Delimiters) + 1;
			return normBundleName.Substring(index);
		}
	}

	public sealed class AssetEntry
	{
		public BundleEntry BundleEntry { get; }
		public string NormBundleName => BundleEntry.NormBundleName;
		public string AssetName { get; }
		public Type AssetType { get; }
		public LoadMethod LoadMethod { get; }

		public AssetEntry(string bundleName, string assetName = null, Type assetType = null, LoadMethod? loadMethod = null)
		{
			BundleEntry = new BundleEntry(bundleName);
			AssetName = assetName ?? AssetLoaderInstance.DefaultAssetName(NormBundleName);
			AssetType = assetType ?? typeof(UnityEngine.Object);
			LoadMethod = loadMethod ?? LoadMethod.Single;
		}

		public override string ToString() => $"[{LoadMethod}]{BundleEntry}<{AssetType}>{AssetName}";
	}

	public enum LoadMethod
	{
		Single = 0,
		Multi = 1,
	}
}
