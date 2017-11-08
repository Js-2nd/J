namespace J
{
	using System;
	using UniRx;
	using UnityEngine;
	using static J.AssetLoaderInstance;
	using static J.SingletonMonoBehaviour<AssetLoaderInstance>;
	using Object = UnityEngine.Object;

	public static class AssetLoader
	{
		public static string InspectorManifestUri => Instance.InspectorManifestUri;

		public static AssetBundleManifest Manifest => Instance.Manifest;

		public static string RootUri => Instance.RootUri;

		public static void LoadManifest(string uri) =>
			Instance.LoadManifest(uri);

		public static IObservable<Unit> WaitForManifestLoaded() =>
			Instance.WaitForManifestLoaded();

		public static IObservable<AssetBundle> GetAssetBundle(string bundleName) =>
			Instance.GetAssetBundle(new BundleEntry(bundleName));

		public static IObservable<AssetBundle> GetAssetBundleWithDependencies(string bundleName) =>
			Instance.GetAssetBundleWithDependencies(new BundleEntry(bundleName));

		public static IObservable<Object> Load(string bundleName, string assetName = null, Type assetType = null) =>
			Instance.Load(new AssetEntry(bundleName, assetName, assetType, LoadMethod.Single));
		public static IObservable<Object> Load(string bundleName, Type assetType) =>
			Instance.Load(new AssetEntry(bundleName, null, assetType, LoadMethod.Single));
		public static IObservable<T> Load<T>(string bundleName, string assetName = null) where T : Object =>
			Instance.Load(new AssetEntry(bundleName, assetName, typeof(T), LoadMethod.Single)).Select(obj => obj as T);

		public static IObservable<Object> LoadMulti(string bundleName, string assetName = null, Type assetType = null) =>
			Instance.Load(new AssetEntry(bundleName, assetName, assetType, LoadMethod.Multi));
		public static IObservable<Object> LoadMulti(string bundleName, Type assetType) =>
			Instance.Load(new AssetEntry(bundleName, null, assetType, LoadMethod.Multi));
		public static IObservable<T> LoadMulti<T>(string bundleName, string assetName = null) where T : Object =>
			Instance.Load(new AssetEntry(bundleName, assetName, typeof(T), LoadMethod.Multi)).Select(obj => obj as T);
	}
}
