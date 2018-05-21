namespace J
{
	using System;

	partial class AssetLoaderInstance
	{
		public sealed class AssetEntry
		{
			public BundleEntry BundleEntry { get; }
			public string BundleName => BundleEntry.BundleName;
			public string AssetName { get; }
			public Type AssetType { get; }
			public LoadMethod LoadMethod { get; }

			public AssetEntry(string bundleName, string assetName = null, Type assetType = null, LoadMethod? loadMethod = null)
			{
				BundleEntry = new BundleEntry(bundleName);
				AssetName = assetName != null ? assetName.ToLower() : BundleName.Substring(BundleName.LastIndexOfAny(Delimiters) + 1);
				AssetType = assetType ?? typeof(UnityEngine.Object);
				LoadMethod = loadMethod ?? LoadMethod.Single;
			}
		}

		public enum LoadMethod
		{
			Single,
			Multi,
		}
	}
}
