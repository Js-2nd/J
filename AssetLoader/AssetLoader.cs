namespace J
{
	using System;
	using UniRx;
	using UnityEngine;
	using static J.AssetLoaderInstance;
	using Object = UnityEngine.Object;

	public static partial class AssetLoader
	{

		public static IObservable<AssetBundle> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));

	}
}
