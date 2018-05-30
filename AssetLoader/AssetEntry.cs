namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public static string DefaultAssetName(string normedBundleName)
		{
			int index = normedBundleName.LastIndexOfAny(Delimiters) + 1;
			return normedBundleName.Substring(index);
		}
	}

	public sealed class AssetEntry
	{
		public BundleEntry BundleEntry { get; }
		public string NormedBundleName => BundleEntry.NormedBundleName;
		public string AssetName { get; }
		public Type AssetType { get; }
		public LoadMethod LoadMethod { get; }

		public AssetEntry(string bundleName, string assetName = null, Type assetType = null, LoadMethod? loadMethod = null)
		{
			BundleEntry = new BundleEntry(bundleName);
			AssetName = assetName ?? AssetLoaderInstance.DefaultAssetName(NormedBundleName);
			AssetType = assetType ?? typeof(UnityEngine.Object);
			LoadMethod = loadMethod ?? LoadMethod.Single;
		}
	}

	public enum LoadMethod
	{
		Single = 0,
		Multi = 1,
	}
}
