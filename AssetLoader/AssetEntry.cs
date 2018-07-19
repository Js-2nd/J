namespace J
{
	using System;
	using UniRx;
	using UnityEngine;
	using Object = UnityEngine.Object;

	partial class AssetLoaderInstance
	{
		public static string DefaultAssetName(string normBundleName) =>
			normBundleName.Substring(normBundleName.LastIndexOfAny(Delimiters) + 1);
	}

	public sealed class AssetEntry
	{
		public BundleEntry BundleEntry { get; }
		public string NormBundleName => BundleEntry.NormName;
		public string AssetName { get; }
		public Type AssetType { get; }
		public LoadMethod LoadMethod { get; }

		public AssetEntry(string bundleName, string assetName = null, Type assetType = null, LoadMethod? loadMethod = null)
		{
			BundleEntry = new BundleEntry(bundleName);
			AssetName = assetName ?? AssetLoaderInstance.DefaultAssetName(NormBundleName);
			AssetType = assetType ?? typeof(Object);
			LoadMethod = loadMethod ?? LoadMethod.Single;
		}

		public override string ToString() => $"{BundleEntry}<{AssetType}>{AssetName}";

		public IObservable<Object> LoadFrom(AssetBundle bundle) => Observable.Defer(() =>
		{
			if (bundle == null) throw new ArgumentNullException(nameof(bundle));
			switch (LoadMethod)
			{
				case LoadMethod.Single:
					return bundle.LoadAssetAsync(AssetName, AssetType)
						.AsAsyncOperationObservable().Select(req => req.asset);
				case LoadMethod.Multi:
					return bundle.LoadAssetWithSubAssetsAsync(AssetName, AssetType)
						.AsAsyncOperationObservable().SelectMany(req => req.allAssets);
				default: throw new ArgumentException("Unknown LoadMethod. " + LoadMethod);
			}
		});
	}

	public enum LoadMethod
	{
		Single = 0,
		Multi = 1,
	}
}
